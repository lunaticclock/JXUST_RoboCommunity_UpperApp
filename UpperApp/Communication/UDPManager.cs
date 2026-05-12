using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UpperApp.Core;

namespace UpperApp.Communication
{
    internal class UDPManager : BaseCommunicationManager
    {
        private UdpClient _udpClient;
        private readonly BindingList<string> _peerList = new();
        private string _lastRemoteEndPoint = "";
        private string _lastSendRemoteEndPoint = "";

        public UDPManager() : base(ChannelType.UDP) { }

        public override void Start(CommunicationParams parameters)
        {
            if (parameters is not UdpParams udpParams)
                throw new ArgumentException("参数类型必须为 UdpParams");

            if (!IPAddress.TryParse(udpParams.LocalIP, out var ip) ||
                udpParams.Port <= 0 || udpParams.Port > 65535)
            {
                OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, "IP 或端口无效"));
                return;
            }

            StartCore();
            _udpClient = new UdpClient(new IPEndPoint(ip, udpParams.Port));
            _ = StartReceiveLoopAsync(_cts.Token);
        }

        protected override void OnStopping()
        {
            try { _udpClient?.Close(); } catch { }
            _udpClient = null;
            _peerList.Clear();
        }

        public override void Send(string data, string target = null)
        {
            if (_udpClient == null)
            {
                OnStatusChanged(new Result(Result.NETStatus.SendMessage, "UDP未启动", 0, "") with { Status = Result.ResStatus.Error });
                return;
            }

            if (string.IsNullOrEmpty(target))
            {
                OnStatusChanged(new Result(Result.NETStatus.SendMessage, "UDP 发送需要指定目标 IP:Port", 0, "") with { Status = Result.ResStatus.Error });
                return;
            }

            int colonIndex = target.LastIndexOf(':');
            if (colonIndex <= 0 || !IPAddress.TryParse(target.AsSpan(0, colonIndex), out IPAddress remoteIP))
            {
                OnStatusChanged(new Result(Result.NETStatus.SendMessage, "远端IP错误!", 0, "") with { Status = Result.ResStatus.Error });
                return;
            }
            if (!int.TryParse(target.AsSpan(colonIndex + 1), out int remotePort))
            {
                OnStatusChanged(new Result(Result.NETStatus.SendMessage, "远端端口错误!", 0, "") with { Status = Result.ResStatus.Error });
                return;
            }

            byte[] sendBytes = encoding.GetBytes(data);
            if (sendBytes.Length == 0)
            {
                OnStatusChanged(new Result(Result.NETStatus.SendMessage, "未发出信息!", 0, "") with { Status = Result.ResStatus.Alert });
                return;
            }

            try
            {
                _udpClient.Send(sendBytes, sendBytes.Length, new IPEndPoint(remoteIP, remotePort));
                var result = new Result(Result.NETStatus.SendMessage, data, sendBytes.Length, target) with { Status = Result.ResStatus.SetNum };
                _lastSendRemoteEndPoint = target;
                OnStatusChanged(result);
            }
            catch (SocketException)
            {
                _peerList.Remove(target);
                OnStatusChanged(new Result(Result.NETStatus.RemoteStop, "远端关闭", 0, target));
            }
        }

        private async Task StartReceiveLoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        UdpReceiveResult result = await _udpClient!.ReceiveAsync(token);
                        ProcessReceivedData(result.Buffer, result.RemoteEndPoint.ToString());
                    }
                    catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionReset)
                    {
                        OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, ex.Message, 0, _lastSendRemoteEndPoint));
                        _peerList.Remove(_lastSendRemoteEndPoint);
                        _lastSendRemoteEndPoint = "";
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, ex.Message));
            }
            finally
            {
                _isMonitoring = false;
            }
        }

        private void ProcessReceivedData(byte[] data, string remoteEndPoint)
        {
            string message = encoding.GetString(data, 0, data.Length) + "\r\n";
            var result = new Result(Result.NETStatus.ReciveMessage, message, data.Length, remoteEndPoint)
                with { IPPort = remoteEndPoint };

            if (!_peerList.Contains(remoteEndPoint))
            {
                _peerList.Add(remoteEndPoint);
                OnStatusChanged(new Result(Result.NETStatus.NewRemote, remoteEndPoint, 0, remoteEndPoint));
            }

            if (!remoteEndPoint.Equals(_lastRemoteEndPoint))
            {
                result = result with { NewPeer = $"\r\nfrom {remoteEndPoint}:\r\n" };
                _lastRemoteEndPoint = remoteEndPoint;
            }

            OnStatusChanged(result);
        }

        public override IReadOnlyList<string> GetPeerList() => _peerList;
    }
}
