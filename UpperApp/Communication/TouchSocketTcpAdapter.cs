using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;
using UpperApp.Core;
using UpperApp.Services;

namespace UpperApp.Communication
{
    internal class TouchSocketTcpAdapter : ICommunicator
    {
        private TcpService _service;
        private readonly Dictionary<string, ITcpSessionClient> _clients = [];
        private readonly Lock _lock = new();
        private DeviceState _state = DeviceState.Disconnected;
        private bool _isStopping;
        private readonly Encoding _encoding = Encoding.GetEncoding("GB2312");

        public event Action<UpperApp.Core.Result> StatusChanged;

        public ChannelType Channel => ChannelType.TCP;

        public DeviceState State
        {
            get => _state;
            private set
            {
                if (_state != value)
                {
                    _state = value;
                }
            }
        }

        static TouchSocketTcpAdapter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void Start(CommunicationParams parameters)
        {
            if (parameters is not TcpServerParams tcpParams)
                throw new ArgumentException("参数类型必须为 TcpServerParams");

            if (!IPAddress.TryParse(tcpParams.LocalIP, out var ip) ||
                tcpParams.Port <= 0 || tcpParams.Port > 65535)
            {
                OnStatusChanged(new UpperApp.Core.Result(UpperApp.Core.Result.NETStatus.ExceptionStop, "IP 或端口无效"));
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
                    OnStatusChanged(new UpperApp.Core.Result(UpperApp.Core.Result.NETStatus.NewRemote, endPoint, 0, endPoint));
                    return EasyTask.CompletedTask;
                },

                Closed = (client, e) =>
                    {
                        var endPoint = client.GetIPPort();
                        lock (_lock)
                        {
                            _clients.Remove(endPoint);
                        }
                        if (!_isStopping)
                            OnStatusChanged(new UpperApp.Core.Result(UpperApp.Core.Result.NETStatus.RemoteStop, $"远端 {endPoint} 已断开"));
                        return EasyTask.CompletedTask;
                    },

                Received = (client, e) =>
                    {
                        var endPoint = client.GetIPPort();
                        var receivedData = e.Memory.Span.ToString(_encoding);
                        OnStatusChanged(new UpperApp.Core.Result(UpperApp.Core.Result.NETStatus.ReciveMessage, receivedData, e.Memory.Length, endPoint));
                        return EasyTask.CompletedTask;
                    }
            };

            var config = new TouchSocketConfig()
                .SetListenIPHosts(new IPHost($"{tcpParams.LocalIP}:{tcpParams.Port}"));

            try
            {
                _service.SetupAsync(config).GetAwaiter().GetResult();
                _service.StartAsync().GetAwaiter().GetResult();
                State = DeviceState.Connected;
                OnStatusChanged(new UpperApp.Core.Result(UpperApp.Core.Result.NETStatus.MonitorStart, "TCP 监听开始"));
            }
            catch (Exception ex)
            {
                OnStatusChanged(new UpperApp.Core.Result(UpperApp.Core.Result.NETStatus.ExceptionStop, $"TCP 启动失败: {ex.Message}"));
                _isStopping = true;
                _service?.SafeDispose();
                _service = null;
                State = DeviceState.Error;
            }

            _isStopping = false;
        }

        public void Stop()
        {
            if (_service == null && _state == DeviceState.Disconnected) return;

            _isStopping = true;
            State = DeviceState.Disconnecting;

            lock (_lock)
            {
                _clients.Clear();
            }

            _service?.StopAsync().GetAwaiter().GetResult();
            _service?.SafeDispose();
            _service = null;

            OnStatusChanged(new UpperApp.Core.Result(UpperApp.Core.Result.NETStatus.MonitorStop, "TCP 已停止"));
            State = DeviceState.Disconnected;
        }

        public void Send(string data, string target = null)
        {
            if (string.IsNullOrEmpty(target))
            {
                OnStatusChanged(new UpperApp.Core.Result(UpperApp.Core.Result.NETStatus.SendMessage, "TCP 发送需要指定目标", 0, "") { Status = UpperApp.Core.Result.ResStatus.Error });
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
                OnStatusChanged(new UpperApp.Core.Result(UpperApp.Core.Result.NETStatus.SendMessage, $"目标 {target} 不在线", 0, "") { Status = UpperApp.Core.Result.ResStatus.Error });
                return;
            }

            try
            {
                var bytes = _encoding.GetBytes(data);
                client.SendAsync(bytes.AsMemory()).GetAwaiter().GetResult();
                OnStatusChanged(new UpperApp.Core.Result(UpperApp.Core.Result.NETStatus.SendMessage, data, bytes.Length, target) { Status = UpperApp.Core.Result.ResStatus.SetNum });
            }
            catch (Exception ex)
            {
                lock (_lock)
                {
                    _clients.Remove(target);
                }
                OnStatusChanged(new UpperApp.Core.Result(UpperApp.Core.Result.NETStatus.ExceptionStop, $"[TCP] 发送到 {target} 失败: {ex.Message}", 0, target));
                OnStatusChanged(new UpperApp.Core.Result(UpperApp.Core.Result.NETStatus.RemoteStop, target));
            }
        }

        public IReadOnlyList<string> GetPeerList()
        {
            lock (_lock)
            {
                return [.. _clients.Keys];
            }
        }

        public ValueTask DisposeAsync()
        {
            Stop();
            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }

        private void OnStatusChanged(UpperApp.Core.Result result)
        {
            if (result.Channel == ChannelType.Unknown)
                result = result with { Channel = ChannelType.TCP };
            if (result.NetStatus == UpperApp.Core.Result.NETStatus.ExceptionStop)
                State = DeviceState.Error;
            StatusChanged?.Invoke(result);
        }
    }
}
