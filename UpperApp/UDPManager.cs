using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UpperApp
{
    class UDPManager : BaseCommunicationManager
    {
        private UdpClient _udpClient;
        private BindingList<string> UDPlist = new();
        private string _lastRemoteEndPoint = "";  // 用于判断是否新端点
        private string _lastSendRemoteEndPoint = "";  // 用于判断远端是否断联，通过发送后接收端报错来判断

        public UDPManager() : base(ChannelType.UDP)
        {
        }

        public void StartMonitor(IPEndPoint localEndPoint)
        {
            StartCore();
            _udpClient = new UdpClient(localEndPoint);
            _ = StartReceiveLoopAsync(_cts.Token);
        }

        protected override void OnStopping()
        {
            _udpClient?.Close();
            _udpClient = null;
            UDPlist.Clear();
        }

        public override void Send(string data, string target = null)
        {
            if (_udpClient == null)
            {
                OnStatusChanged(new Result(Result.NETStatus.SendMessage, "UDP未启动", 0, "") { status = Result.ResStatus.Error });
                return;
            }

            // 解析 peer 字符串 "IP:Port"
            int colonIndex = target.IndexOf(':');
            if (colonIndex <= 0 || !IPAddress.TryParse(target.AsSpan(0, colonIndex), out IPAddress remoteIP))
            {
                OnStatusChanged(new Result(Result.NETStatus.SendMessage, "远端IP错误!", 0, "") { status = Result.ResStatus.Error });
                return;
            }
            if (!int.TryParse(target.Substring(colonIndex + 1), out int remotePort))
            {
                OnStatusChanged(new Result(Result.NETStatus.SendMessage, "远端端口错误!", 0, "") { status = Result.ResStatus.Error });
                return;
            }

            byte[] sendBytes = encoding.GetBytes(data);
            if (sendBytes.Length == 0)
            {
                OnStatusChanged(new Result(Result.NETStatus.SendMessage, "未发出信息!", 0, "") { status = Result.ResStatus.Alert });
                return;
            }

            try
            {
                _udpClient.Send(sendBytes, sendBytes.Length, new IPEndPoint(remoteIP, remotePort));
                var result = new Result(Result.NETStatus.SendMessage, data, sendBytes.Length, target);
                _lastSendRemoteEndPoint = target;
                result.status = Result.ResStatus.SetNum;
                OnStatusChanged(result);
            }
            catch (SocketException)
            {
                UDPlist.Remove(target);
                OnStatusChanged(new Result(Result.NETStatus.RemoteStop, "远端关闭", 0, target));
            }
        }

        public BindingList<string> GetPeer() => UDPlist;

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
                    catch (SocketException ex)
                    {
                        if (ex.SocketErrorCode == SocketError.ConnectionReset)
                        {
                            OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, ex.Message, 0, _lastSendRemoteEndPoint));
                            UDPlist.Remove(_lastSendRemoteEndPoint);
                            _lastSendRemoteEndPoint = "";
                        }
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
            var result = new Result(Result.NETStatus.ReciveMessage, message, data.Length, remoteEndPoint);
            result.IPPort = remoteEndPoint;

            if (!UDPlist.Contains(remoteEndPoint))
                UDPlist.Add(remoteEndPoint);

            if (!remoteEndPoint.Equals(_lastRemoteEndPoint))
            {
                result.NewPeer = $"\r\nfrom {remoteEndPoint}:\r\n";
                _lastRemoteEndPoint = remoteEndPoint;
            }

            OnStatusChanged(result);
        }
    }
}
