using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace UpperApp
{



    class TCPManager : BaseCommunicationManager, IAsyncDisposable
    {
        private bool _isStopping;  // 新增：标记正在主动停止，避免触发 RemoteStop
        private TcpListener _listener;
        public readonly BindingDic<Socket> TCPdic = new();
        public TCPManager() : base(ChannelType.TCP)
        {
        }

        // 新方法：直接传入本地终结点，内部创建 TcpListener
        public override void StartMonitor(IPEndPoint localEndPoint)
        {
            if (_isMonitoring)
                StopMonitor();
            _listener = new TcpListener(localEndPoint);
            _ = StartAcceptLoopAsync(_cts.Token);
            OnStatusChanged(new Result(Result.NETStatus.MonitorStart, "监听开始"));
            _isMonitoring = true;
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

        public override void StopMonitor()
        {
            _isStopping = true;   // 设置标志，避免触发 RemoteStop
            _cts.Cancel();
            _cts.Dispose();
            
            _listener?.Stop();
            _listener = null;
            _isMonitoring = false;

            OnStatusChanged(new Result(Result.NETStatus.MonitorStop, "停止监听"));

            foreach (string key in TCPdic.connectionKeys.ToArray())
            {
                TCPdic.Remove(key)?.Close();
            }
            _cts = new CancellationTokenSource();
            _isStopping = false;
        }

        public void Send(string ip, string Buf)
        {
            if (TCPdic.TryGet(ip, out Socket socket))
            {
                try
                {
                    byte[] buffer = encoding.GetBytes(Buf);
                    socket.Send(buffer);
                    Result rs = new Result(Result.NETStatus.SendMessage, Buf, buffer.Length, ip);
                    rs.status = Result.ResStatus.SetNum;
                    OnStatusChanged(rs);
                }
                catch (SocketException ex)
                {
                    OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, $"[TCPManager] 发送到 {ip} 失败: {ex.Message}", 0, ip));
                    TCPdic.Remove(ip)?.Close();
                    OnStatusChanged(new Result(Result.NETStatus.RemoteStop, ip));
                }
            }
        }

        public void Remove(string peer)
        {
            TCPdic.Remove(peer)?.Close();
        }

        public async ValueTask DisposeAsync()
        {
            StopMonitor();
            _cts?.Dispose();
            await Task.CompletedTask;
        }
    }
}
