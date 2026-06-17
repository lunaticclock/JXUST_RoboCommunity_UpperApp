using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using TouchSocket.Core;
using TouchSocket.Sockets;
using UpperApp.Core;
using UpperApp.Services;

namespace UpperApp.Communication
{
    internal class TouchSocketTcpAdapter : CommunicatorBase
    {
        private TcpService _service;
        private readonly Dictionary<string, ITcpSessionClient> _clients = [];
        private readonly Lock _lock = new();
        private readonly Encoding _encoding = Encoding.GetEncoding("GB2312");

        public override ChannelType Channel => ChannelType.TCP;

        static TouchSocketTcpAdapter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public override void Start(CommunicationParams parameters)
        {
            if (parameters is not TcpServerParams tcpParams)
                throw new ArgumentException("参数类型必须为 TcpServerParams");

            if (!IPAddress.TryParse(tcpParams.LocalIP, out _) ||
                tcpParams.Port <= 0 || tcpParams.Port > 65535)
            {
                NotifyException("IP 或端口无效");
                return;
            }

            Stop();
            State = DeviceState.Connecting;

            _service = new TcpService
            {
                Connected = (client, e) =>
                {
                    var endPoint = client.GetIPPort();
                    lock (_lock)
                    {
                        _clients[endPoint] = client;
                    }
                    NotifyPeerConnected(endPoint);
                    return EasyTask.CompletedTask;
                },

                Closed = (client, e) =>
                    {
                        var endPoint = client.GetIPPort();
                        lock (_lock)
                        {
                            _clients.Remove(endPoint);
                        }
                        if (!IsStopping)
                            NotifyPeerDisconnected($"远端 {endPoint} 已断开");
                        return EasyTask.CompletedTask;
                    },

                Received = (client, e) =>
                    {
                        var endPoint = client.GetIPPort();
                        var receivedData = e.Memory.Span.ToString(_encoding);
                        NotifyMessageReceived(receivedData, e.Memory.Length, endPoint);
                        return EasyTask.CompletedTask;
                    }
            };

            var config = new TouchSocketConfig()
                .SetListenIPHosts(new IPHost($"{tcpParams.LocalIP}:{tcpParams.Port}"));

            try
            {
                _service.SetupAsync(config).GetAwaiter().GetResult();
                _service.StartAsync().GetAwaiter().GetResult();
                NotifyMonitorStarted("TCP 监听开始");
            }
            catch (Exception ex)
            {
                NotifyException($"TCP 启动失败: {ex.Message}");
                _service?.SafeDispose();
                _service = null;
            }
        }

        public override void Stop()
        {
            if (IsStopping) return;
            if (_service == null && State == DeviceState.Disconnected) return;

            BeginStop();

            lock (_lock)
            {
                _clients.Clear();
            }

            _service?.StopAsync().GetAwaiter().GetResult();
            _service?.SafeDispose();
            _service = null;

            NotifyMonitorStopped("TCP 已停止");
            EndStop();
        }

        public override void Send(string data, string target = null)
        {
            if (string.IsNullOrEmpty(target))
            {
                NotifyMessageSendError("TCP 发送需要指定目标");
                return;
            }

            ITcpSessionClient client = null;
            bool found;
            lock (_lock)
            {
                found = _clients.TryGetValue(target, out client);
            }

            if (!found || client == null || !client.Online)
            {
                NotifyMessageSendError($"目标 {target} 不在线");
                return;
            }

            try
            {
                var bytes = _encoding.GetBytes(data);
                client.SendAsync(bytes.AsMemory()).GetAwaiter().GetResult();
                NotifyMessageSent(data, bytes.Length, target);
            }
            catch (Exception ex)
            {
                lock (_lock)
                {
                    _clients.Remove(target);
                }
                NotifyException($"[TCP] 发送到 {target} 失败: {ex.Message}", target);
                NotifyPeerDisconnected("发送失败导致断开", target);
            }
        }

        public override IReadOnlyList<string> GetPeerList()
        {
            lock (_lock)
            {
                return [.. _clients.Keys];
            }
        }
    }
}
