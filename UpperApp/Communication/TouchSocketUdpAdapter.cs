using System;
using System.Collections.Generic;
using System.Net;
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

        public override ChannelType Channel => ChannelType.UDP;

        protected override string StoppedMessage => "UDP 已停止";

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
                var message = e.Memory.Span.ToString(Utils.TextEncoding) + "\r\n";
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

        protected override void StopCore()
        {
            lock (_lock)
            {
                _peerList.Clear();
            }

            _udpSession?.StopAsync().GetAwaiter().GetResult();
            _udpSession?.SafeDispose();
            _udpSession = null;
            _lastRemoteEndPoint = "";
        }

        public override void Send(string data, string target = null)
        {
            if (!EnsureStarted() || !EnsureTarget(target)) return;
            if (!TryParseEndPoint(target, out IPEndPoint endPoint, out string error))
            {
                NotifyMessageSendError(error);
                return;
            }

            byte[] payload = Utils.TextEncoding.GetBytes(data);
            if (!HasPayload(payload)) return;

            Transmit(endPoint, payload, data, target);
        }

        public override void Send(byte[] data, string target = null)
        {
            if (!EnsureStarted()) return;
            if (!HasPayload(data)) return;
            if (!EnsureTarget(target)) return;
            if (!TryParseEndPoint(target, out IPEndPoint endPoint, out string error))
            {
                NotifyMessageSendError(error);
                return;
            }

            Transmit(endPoint, data, Utils.BytesToHexString(data), target);
        }

        private bool EnsureStarted()
        {
            if (_udpSession != null) return true;
            NotifyMessageSendError("UDP未启动");
            return false;
        }

        private bool EnsureTarget(string target)
        {
            if (!string.IsNullOrEmpty(target)) return true;
            NotifyMessageSendError("UDP 发送需要指定目标 IP:Port");
            return false;
        }

        private void Transmit(IPEndPoint endPoint, byte[] payload, string display, string target)
            => WriteAndReport(payload, display, target,
                p => _udpSession.SendAsync(endPoint, p.AsMemory()).GetAwaiter().GetResult(),
                _ =>
                {
                    lock (_lock)
                    {
                        _peerList.Remove(target);
                    }
                    NotifyPeerDisconnected("远端关闭", target);
                });

        /// <summary>
        /// 解析 "IP:Port" 形式的发送目标，失败时返回对应的错误提示。
        /// </summary>
        private static bool TryParseEndPoint(string target, out IPEndPoint endPoint, out string error)
        {
            endPoint = null;
            error = null;

            int colonIndex = target.LastIndexOf(':');
            if (colonIndex <= 0 || !IPAddress.TryParse(target.AsSpan(0, colonIndex), out IPAddress remoteIP))
            {
                error = "远端IP错误!";
                return false;
            }
            if (!int.TryParse(target.AsSpan(colonIndex + 1), out int remotePort))
            {
                error = "远端端口错误!";
                return false;
            }

            endPoint = new IPEndPoint(remoteIP, remotePort);
            return true;
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
