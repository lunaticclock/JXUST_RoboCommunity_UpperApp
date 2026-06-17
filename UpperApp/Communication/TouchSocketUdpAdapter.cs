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
    internal class TouchSocketUdpAdapter : CommunicatorBase
    {
        private UdpSession _udpSession;
        private readonly HashSet<string> _peerList = [];
        private readonly Lock _lock = new();
        private string _lastRemoteEndPoint = "";
        private readonly Encoding _encoding = Encoding.GetEncoding("GB2312");

        public override ChannelType Channel => ChannelType.UDP;

        static TouchSocketUdpAdapter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public override void Start(CommunicationParams parameters)
        {
            if (parameters is not UdpParams udpParams)
                throw new ArgumentException("参数类型必须为 UdpParams");

            if (!IPAddress.TryParse(udpParams.LocalIP, out _) ||
                udpParams.Port <= 0 || udpParams.Port > 65535)
            {
                NotifyException("IP 或端口无效");
                return;
            }

            Stop();
            State = DeviceState.Connecting;

            _udpSession = new UdpSession();

            _udpSession.Received = (session, e) =>
            {
                var remoteEndPoint = e.EndPoint.ToString();
                var message = e.Memory.Span.ToString(_encoding) + "\r\n";
                int byteCount = e.Memory.Length;

                lock (_lock)
                {
                    if (_peerList.Add(remoteEndPoint))
                        NotifyPeerConnected(remoteEndPoint);
                }

                // 切换到新对端时附加提示前缀
                string peerHint = "";
                if (!remoteEndPoint.Equals(_lastRemoteEndPoint))
                {
                    peerHint = $"\r\nfrom {remoteEndPoint}:\r\n";
                    _lastRemoteEndPoint = remoteEndPoint;
                }

                NotifyMessageReceived(message, byteCount, remoteEndPoint, peerHint);
                return EasyTask.CompletedTask;
            };

            var config = new TouchSocketConfig()
                .SetBindIPHost(new IPHost($"{udpParams.LocalIP}:{udpParams.Port}"));

            try
            {
                _udpSession.SetupAsync(config).GetAwaiter().GetResult();
                _udpSession.StartAsync().GetAwaiter().GetResult();
                NotifyMonitorStarted("UDP 监听开始");
            }
            catch (Exception ex)
            {
                NotifyException($"UDP 启动失败: {ex.Message}");
                _udpSession?.SafeDispose();
                _udpSession = null;
            }
        }

        public override void Stop()
        {
            if (IsStopping) return;
            if (_udpSession == null && State == DeviceState.Disconnected) return;

            BeginStop();

            lock (_lock)
            {
                _peerList.Clear();
            }

            _udpSession?.StopAsync().GetAwaiter().GetResult();
            _udpSession?.SafeDispose();
            _udpSession = null;
            _lastRemoteEndPoint = "";

            NotifyMonitorStopped("UDP 已停止");
            EndStop();
        }

        public override void Send(string data, string target = null)
        {
            if (_udpSession == null)
            {
                NotifyMessageSendError("UDP未启动");
                return;
            }

            if (string.IsNullOrEmpty(target))
            {
                NotifyMessageSendError("UDP 发送需要指定目标 IP:Port");
                return;
            }

            int colonIndex = target.LastIndexOf(':');
            if (colonIndex <= 0 || !IPAddress.TryParse(target.AsSpan(0, colonIndex), out IPAddress remoteIP))
            {
                NotifyMessageSendError("远端IP错误!");
                return;
            }
            if (!int.TryParse(target.AsSpan(colonIndex + 1), out int remotePort))
            {
                NotifyMessageSendError("远端端口错误!");
                return;
            }

            byte[] sendBytes = _encoding.GetBytes(data);
            if (sendBytes.Length == 0)
            {
                NotifyMessageSendAlert("未发出信息!");
                return;
            }

            try
            {
                var endPoint = new IPEndPoint(remoteIP, remotePort);
                _udpSession.SendAsync(endPoint, sendBytes.AsMemory()).GetAwaiter().GetResult();
                NotifyMessageSent(data, sendBytes.Length, target);
            }
            catch (Exception)
            {
                lock (_lock)
                {
                    _peerList.Remove(target);
                }
                NotifyPeerDisconnected("远端关闭", target);
            }
        }

        public override void Send(byte[] data, string target = null)
        {
            if (_udpSession == null)
            {
                NotifyMessageSendError("UDP未启动");
                return;
            }
            if (data == null || data.Length == 0)
            {
                NotifyMessageSendAlert("未发出信息!");
                return;
            }
            if (string.IsNullOrEmpty(target))
            {
                NotifyMessageSendError("UDP 发送需要指定目标 IP:Port");
                return;
            }

            int colonIndex = target.LastIndexOf(':');
            if (colonIndex <= 0 || !IPAddress.TryParse(target.AsSpan(0, colonIndex), out IPAddress remoteIP))
            {
                NotifyMessageSendError("远端IP错误!");
                return;
            }
            if (!int.TryParse(target.AsSpan(colonIndex + 1), out int remotePort))
            {
                NotifyMessageSendError("远端端口错误!");
                return;
            }

            try
            {
                var endPoint = new IPEndPoint(remoteIP, remotePort);
                _udpSession.SendAsync(endPoint, data.AsMemory()).GetAwaiter().GetResult();
                NotifyMessageSent(Utils.BytesToHexString(data), data.Length, target);
            }
            catch (Exception)
            {
                lock (_lock)
                {
                    _peerList.Remove(target);
                }
                NotifyPeerDisconnected("远端关闭", target);
            }
        }

        public override IReadOnlyList<string> GetPeerList()
        {
            lock (_lock)
            {
                return [.. _peerList];
            }
        }
    }
}
