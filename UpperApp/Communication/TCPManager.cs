using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UpperApp.Core;

namespace UpperApp.Communication
{
    internal class TCPManager : BaseCommunicationManager
    {
        private TcpListener _listener;
        private readonly BindingDic<Socket> _clients = new();

        public TCPManager() : base(ChannelType.TCP) { }

        public override void Start(CommunicationParams parameters)
        {
            if (parameters is not TcpServerParams tcpParams)
                throw new ArgumentException("参数类型必须为 TcpServerParams");

            if (!IPAddress.TryParse(tcpParams.LocalIP, out var ip) ||
                tcpParams.Port <= 0 || tcpParams.Port > 65535)
            {
                OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, "IP 或端口无效"));
                return;
            }

            StartCore();
            _listener = new TcpListener(ip, tcpParams.Port);
            _ = StartAcceptLoopAsync(_cts.Token);
        }

        protected override void OnStopping()
        {
            foreach (var key in _clients.connectionKeys.ToList())
            {
                if (_clients.Remove(key) is Socket socket)
                {
                    try { socket.Shutdown(SocketShutdown.Both); } catch { }
                    try { socket.Close(); } catch { }
                }
            }
            try { _listener?.Stop(); } catch { }
            _listener = null;
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
                    _clients.Add(endPointKey, clientSocket);
                    OnStatusChanged(new Result(Result.NETStatus.NewRemote, endPointKey, 0, endPointKey));
                    _ = ReceiveLoopAsync(tcpClient, endPointKey, token);
                }
            }
            catch (OperationCanceledException) { }
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
                    int bytesRead = await stream.ReadAsync(buffer, token);
                    if (bytesRead == 0) break;

                    string receivedData = encoding.GetString(buffer, 0, bytesRead);
                    OnStatusChanged(new Result(Result.NETStatus.ReciveMessage, receivedData, bytesRead, endPointKey));
                }
            }
            catch (OperationCanceledException) { }
            catch (SocketException ex)
            {
                OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, $"[TCPManager] 客户端 {endPointKey} 通信异常: {ex.Message}", 0, endPointKey));
            }
            finally
            {
                _clients.Remove(endPointKey);
                tcpClient.Close();
                if (!_isStopping)
                    OnStatusChanged(new Result(Result.NETStatus.RemoteStop, $"远端 {endPointKey} 已断开"));
            }
        }

        public override void Send(string data, string target = null)
        {
            if (string.IsNullOrEmpty(target))
            {
                OnStatusChanged(new Result(Result.NETStatus.SendMessage, "TCP 发送需要指定目标", 0, "") with { Status = Result.ResStatus.Error });
                return;
            }

            if (_clients.TryGet(target, out Socket socket))
            {
                try
                {
                    byte[] buffer = encoding.GetBytes(data);
                    socket.Send(buffer);
                    OnStatusChanged(new Result(Result.NETStatus.SendMessage, data, buffer.Length, target) with { Status = Result.ResStatus.SetNum });
                }
                catch (SocketException ex)
                {
                    OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, $"[TCPManager] 发送到 {target} 失败: {ex.Message}", 0, target));
                    _clients.Remove(target)?.Close();
                    OnStatusChanged(new Result(Result.NETStatus.RemoteStop, target));
                }
            }
        }

        public override IReadOnlyList<string> GetPeerList() => _clients.connectionKeys;
    }
}
