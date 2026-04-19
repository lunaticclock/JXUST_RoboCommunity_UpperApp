using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UpperApp
{
    class UDPManager : BaseCommunicationManager, IAsyncDisposable
    {
        private UdpClient _udpClient;
        private readonly SynchronizationContext _syncContext;
        private BindingList<string> UDPlist = new();
        private string _lastRemoteEndPoint = "";  // 用于判断是否新端点

        public UDPManager() : base(ChannelType.UDP)
        {
            _syncContext = SynchronizationContext.Current;
        }

        public void UDP_Send(string buf, string peer)
        {
            if (_udpClient == null)
            {
                OnStatusChanged(new Result(Result.NETStatus.SendMessage, "UDP未启动", 0, "") { status = Result.ResStatus.Error });
                return;
            }

            // 解析 peer 字符串 "IP:Port"
            int colonIndex = peer.IndexOf(':');
            if (colonIndex <= 0 || !IPAddress.TryParse(peer.AsSpan(0, colonIndex), out IPAddress remoteIP))
            {
                OnStatusChanged(new Result(Result.NETStatus.SendMessage, "远端IP错误!", 0, "") { status = Result.ResStatus.Error });
                return;
            }
            if (!int.TryParse(peer.Substring(colonIndex + 1), out int remotePort))
            {
                OnStatusChanged(new Result(Result.NETStatus.SendMessage, "远端端口错误!", 0, "") { status = Result.ResStatus.Error });
                return;
            }

            byte[] sendBytes = encoding.GetBytes(buf);
            if (sendBytes.Length == 0)
            {
                OnStatusChanged(new Result(Result.NETStatus.SendMessage, "未发出信息!", 0, "") { status = Result.ResStatus.Alert });
                return;
            }

            try
            {
                _udpClient.Send(sendBytes, sendBytes.Length, new IPEndPoint(remoteIP, remotePort));
                var result = new Result(Result.NETStatus.SendMessage, buf, sendBytes.Length, peer);
                result.status = Result.ResStatus.SetNum;
                OnStatusChanged(result);
            }
            catch (SocketException)
            {
                // 发送失败，从 UI 列表中移除该端点
                PostToUI(() => UDPlist.Remove(peer));
                OnStatusChanged(new Result(Result.NETStatus.RemoteStop, "远端关闭"));
            }
        }

        public BindingList<string> GetUDPPeer()
        {
            return UDPlist;
        }

        public override void StopMonitor()
        {
            _cts.Cancel();
            _udpClient?.Close();
            _udpClient = null;
            _isMonitoring = false;
            OnStatusChanged(new Result(Result.NETStatus.MonitorStop, "停止监听"));
            // 重置 CancellationTokenSource 以便重启
            _cts.Dispose();
            _cts = new CancellationTokenSource();
        }

        public override void StartMonitor(IPEndPoint localEndPoint)
        {
            if (_isMonitoring) 
                StopMonitor();
            UDPlist.Clear();
            _udpClient = new UdpClient(localEndPoint);
            _isMonitoring = true;
            _ = StartReceiveLoopAsync(_cts.Token);
            OnStatusChanged(new Result(Result.NETStatus.MonitorStart, "监听开始"));
        }

        private async Task StartReceiveLoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    UdpReceiveResult result = await _udpClient!.ReceiveAsync(token);
                    ProcessReceivedData(result.Buffer, result.RemoteEndPoint.ToString());
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

            // UI 列表操作需要同步到 UI 线程
            PostToUI(() =>
            {
                if (!UDPlist.Contains(remoteEndPoint))
                    UDPlist.Add(remoteEndPoint);
            });

            if (!remoteEndPoint.Equals(_lastRemoteEndPoint))
            {
                result.NewPeer = $"\r\nfrom {remoteEndPoint}:\r\n";
                _lastRemoteEndPoint = remoteEndPoint;
            }

            OnStatusChanged(result);
        }

        private void PostToUI(Action action)
        {
            if (_syncContext == null)
                action();
            else
                _syncContext.Post(_ => action(), null);
        }

        public async ValueTask DisposeAsync()
        {
            StopMonitor();
            await Task.CompletedTask;
        }
    }
}
