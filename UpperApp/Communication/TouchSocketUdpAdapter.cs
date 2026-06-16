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
    internal class TouchSocketUdpAdapter : ICommunicator
    {
        private UdpSession _udpSession;
        private readonly HashSet<string> _peerList = [];
        private readonly Lock _lock = new();
        private string _lastRemoteEndPoint = "";
        private DeviceState _state = DeviceState.Disconnected;
        private readonly Encoding _encoding = Encoding.GetEncoding("GB2312");

        public event Action<UpperApp.Core.Result> StatusChanged;

        public ChannelType Channel => ChannelType.UDP;

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

        static TouchSocketUdpAdapter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void Start(CommunicationParams parameters)
        {
            if (parameters is not UdpParams udpParams)
                throw new ArgumentException("参数类型必须为 UdpParams");

            if (!IPAddress.TryParse(udpParams.LocalIP, out var ip) ||
                udpParams.Port <= 0 || udpParams.Port > 65535)
            {
                OnStatusChanged(new UpperApp.Core.Result(UpperApp.Core.Result.NETStatus.ExceptionStop, "IP 或端口无效"));
                return;
            }

            Stop();
            State = DeviceState.Connecting;

            _udpSession = new UdpSession();

            _udpSession.Received = (session, e) =>
            {
                var remoteEndPoint = e.EndPoint.ToString();
                var message = e.Memory.Span.ToString(_encoding) + "\r\n";
                var result = new UpperApp.Core.Result(UpperApp.Core.Result.NETStatus.ReciveMessage, message, e.Memory.Length, remoteEndPoint)
                {
                    IPPort = remoteEndPoint
                };

                lock (_lock)
                {
                    if (_peerList.Add(remoteEndPoint))
                    {
                        OnStatusChanged(new UpperApp.Core.Result(UpperApp.Core.Result.NETStatus.NewRemote, remoteEndPoint, 0, remoteEndPoint));
                    }
                }

                if (!remoteEndPoint.Equals(_lastRemoteEndPoint))
                {
                    result = result with { NewPeer = $"\r\nfrom {remoteEndPoint}:\r\n" };
                    _lastRemoteEndPoint = remoteEndPoint;
                }

                OnStatusChanged(result);
                return EasyTask.CompletedTask;
            };

            var config = new TouchSocketConfig()
                .SetBindIPHost(new IPHost($"{udpParams.LocalIP}:{udpParams.Port}"));

            try
            {
                _udpSession.SetupAsync(config).GetAwaiter().GetResult();
                _udpSession.StartAsync().GetAwaiter().GetResult();
                State = DeviceState.Connected;
                OnStatusChanged(new UpperApp.Core.Result(UpperApp.Core.Result.NETStatus.MonitorStart, "UDP 监听开始"));
            }
            catch (Exception ex)
            {
                OnStatusChanged(new UpperApp.Core.Result(UpperApp.Core.Result.NETStatus.ExceptionStop, $"UDP 启动失败: {ex.Message}"));
                _udpSession?.SafeDispose();
                _udpSession = null;
                State = DeviceState.Error;
            }
        }

        public void Stop()
        {
            if (_udpSession == null && _state == DeviceState.Disconnected) return;

            State = DeviceState.Disconnecting;

            lock (_lock)
            {
                _peerList.Clear();
            }

            _udpSession?.StopAsync().GetAwaiter().GetResult();
            _udpSession?.SafeDispose();
            _udpSession = null;
            _lastRemoteEndPoint = "";

            OnStatusChanged(new UpperApp.Core.Result(UpperApp.Core.Result.NETStatus.MonitorStop, "UDP 已停止"));
            State = DeviceState.Disconnected;
        }

        public void Send(string data, string target = null)
        {
            if (_udpSession == null)
            {
                OnStatusChanged(new UpperApp.Core.Result(UpperApp.Core.Result.NETStatus.SendMessage, "UDP未启动", 0, "") { Status = UpperApp.Core.Result.ResStatus.Error });
                return;
            }

            if (string.IsNullOrEmpty(target))
            {
                OnStatusChanged(new UpperApp.Core.Result(UpperApp.Core.Result.NETStatus.SendMessage, "UDP 发送需要指定目标 IP:Port", 0, "") { Status = UpperApp.Core.Result.ResStatus.Error });
                return;
            }

            int colonIndex = target.LastIndexOf(':');
            if (colonIndex <= 0 || !IPAddress.TryParse(target.AsSpan(0, colonIndex), out IPAddress remoteIP))
            {
                OnStatusChanged(new UpperApp.Core.Result(UpperApp.Core.Result.NETStatus.SendMessage, "远端IP错误!", 0, "") { Status = UpperApp.Core.Result.ResStatus.Error });
                return;
            }
            if (!int.TryParse(target.AsSpan(colonIndex + 1), out int remotePort))
            {
                OnStatusChanged(new UpperApp.Core.Result(UpperApp.Core.Result.NETStatus.SendMessage, "远端端口错误!", 0, "") { Status = UpperApp.Core.Result.ResStatus.Error });
                return;
            }

            byte[] sendBytes = _encoding.GetBytes(data);
            if (sendBytes.Length == 0)
            {
                OnStatusChanged(new UpperApp.Core.Result(UpperApp.Core.Result.NETStatus.SendMessage, "未发出信息!", 0, "") { Status = UpperApp.Core.Result.ResStatus.Alert });
                return;
            }

            try
            {
                var endPoint = new IPEndPoint(remoteIP, remotePort);
                _udpSession.SendAsync(endPoint, sendBytes.AsMemory()).GetAwaiter().GetResult();
                var result = new UpperApp.Core.Result(UpperApp.Core.Result.NETStatus.SendMessage, data, sendBytes.Length, target) { Status = UpperApp.Core.Result.ResStatus.SetNum };
                OnStatusChanged(result);
            }
            catch (Exception)
            {
                lock (_lock)
                {
                    _peerList.Remove(target);
                }
                OnStatusChanged(new UpperApp.Core.Result(UpperApp.Core.Result.NETStatus.RemoteStop, "远端关闭", 0, target));
            }
        }

        public IReadOnlyList<string> GetPeerList()
        {
            lock (_lock)
            {
                return [.. _peerList];
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
                result = result with { Channel = ChannelType.UDP };
            if (result.NetStatus == UpperApp.Core.Result.NETStatus.ExceptionStop)
                State = DeviceState.Error;
            StatusChanged?.Invoke(result);
        }
    }
}
