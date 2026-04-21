using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace UpperApp
{



    class TCPManager : BaseCommunicationManager
    {
        private TcpListener _listener;
        private readonly BindingDic<Socket> TCPdic = new();
        public TCPManager() : base(ChannelType.TCP)
        {
        }

        // 新方法：直接传入本地终结点，内部创建 TcpListener
        public void StartMonitor(IPEndPoint localEndPoint)
        {
            StartCore();
            _listener = new TcpListener(localEndPoint);
            _ = StartAcceptLoopAsync(_cts.Token);
        }

        protected override void OnStopping()
        {
            _listener?.Stop();
            _listener = null;
            foreach (string key in TCPdic.connectionKeys.ToArray())
            {
                TCPdic.Remove(key)?.Close();
            }
        }

        private async Task StartAcceptLoopAsync(CancellationToken token)
        {
            try
            {
                _listener!.Start();
                while (!token.IsCancellationRequested)
                {
                    TcpClient tcpClient = await _listener.AcceptTcpClientAsync(token);
                    Socket clientSocket = tcpClient.Client;
                    string endPointKey = clientSocket.RemoteEndPoint?.ToString() ?? "unknown";
                    TCPdic.Add(endPointKey, clientSocket);

                    // 开始接收数据（fire-and-forget）
                    _ = ReceiveLoopAsync(tcpClient, endPointKey, token);
                }
            }
            catch (OperationCanceledException)
            {
                // 正常取消
            }
            catch (SocketException ex)
            {
                OnStatusChanged(new Result(Result.NETStatus.MonitorStop, $"监听异常停止: {ex.Message}"));
            }
            finally
            {
                _isMonitoring = false;
            }
        }

        private async Task ReceiveLoopAsync(TcpClient tcpClient, string endPointKey, CancellationToken token)
        {
            byte[] buffer = new byte[4096];
            NetworkStream stream = tcpClient.GetStream();

            try
            {
                while (!token.IsCancellationRequested)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token);
                    if (bytesRead == 0)
                        break;

                    string receivedData = encoding.GetString(buffer, 0, bytesRead);
                    OnStatusChanged(new Result(Result.NETStatus.ReciveMessage, receivedData, bytesRead, endPointKey));
                }
            }
            catch (OperationCanceledException)
            {
                // 正常取消
            }
            catch (SocketException ex)
            {
                OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, $"[TCPManager] 客户端 {endPointKey} 通信异常: {ex.Message}", 0, endPointKey));
            }
            finally
            {
                TCPdic.Remove(endPointKey);
                tcpClient.Close();
                if (!_isStopping)
                    OnStatusChanged(new Result(Result.NETStatus.RemoteStop, endPointKey));
            }
        }

        public override void Send(string data, string target = null)
        {
            if (TCPdic.TryGet(target, out Socket socket))
            {
                try
                {
                    byte[] buffer = encoding.GetBytes(data);
                    socket.Send(buffer);
                    Result rs = new Result(Result.NETStatus.SendMessage, data, buffer.Length, target);
                    rs.status = Result.ResStatus.SetNum;
                    OnStatusChanged(rs);
                }
                catch (SocketException ex)
                {
                    OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, $"[TCPManager] 发送到 {target} 失败: {ex.Message}", 0, target));
                    TCPdic.Remove(target)?.Close();
                    OnStatusChanged(new Result(Result.NETStatus.RemoteStop, target));
                }
            }
        }

        public BindingList<string> GetPeer() => TCPdic.connectionKeys;
    }
}
