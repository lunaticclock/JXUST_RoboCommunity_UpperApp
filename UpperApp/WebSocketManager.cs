using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UpperApp
{
    class WebSocketManager : BaseCommunicationManager
    {
        private HttpListener _listener;
        private readonly BindingDic<WebSocket> _clients = new();

        private ClientWebSocket _clientSocket;
        private readonly BindingDic<WebSocket> _serverClients = new(); // 服务器模式的客户端
        private bool _isClientMode; // 区分当前模式

        public WebSocketManager() : base(ChannelType.WebSocket) { }

        // 启动 WebSocket 服务器，监听本地地址和端口，例如 "http://localhost:8080/"
        public void StartMonitor(string url)
        {
            StartCore();
            if (_listener == null)
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add(url);
            }
            _listener.Start();
            _ = AcceptLoopAsync(_cts.Token);
            OnStatusChanged(new Result(Result.NETStatus.MonitorStart, $"WebSocket 监听 {url}"));
        }

        // 客户端模式连接
        public async Task ConnectAsync(string serverUrl)
        {
            StartCore();
            _isClientMode = true;
            _clientSocket = new ClientWebSocket();
            await _clientSocket.ConnectAsync(new Uri(serverUrl), _cts.Token);
            _ = ReceiveLoopAsync(_clientSocket, "Server", _cts.Token);
            OnStatusChanged(new Result(Result.NETStatus.MonitorStart, $"WebSocket 客户端已连接 {serverUrl}"));
        }

        private async Task AcceptLoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var context = await _listener!.GetContextAsync();
                    if (context.Request.IsWebSocketRequest)
                    {
                        var wsContext = await context.AcceptWebSocketAsync(null);
                        var socket = wsContext.WebSocket;
                        string clientId = context.Request.RemoteEndPoint.ToString();
                        _clients.Add(clientId, socket);
                        OnStatusChanged(new Result(Result.NETStatus.NewRemote, $"WebSocket 客户端连接: {clientId}"));
                        _ = ReceiveLoopAsync(socket, clientId, token);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                        context.Response.Close();
                    }
                }
            }
            catch (OperationCanceledException) { /* 正常取消 */ }
            catch (Exception ex)
            {
                OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, $"监听异常: {ex.Message}"));
            }
            finally
            {
                _isMonitoring = false;
            }
        }

        private async Task ReceiveLoopAsync(WebSocket socket, string clientId, CancellationToken token)
        {
            var buffer = new byte[4096];
            try
            {
                while (socket.State == WebSocketState.Open && !token.IsCancellationRequested)
                {
                    var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), token);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", token);
                        break;
                    }
                    string receivedData = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    OnStatusChanged(new Result(Result.NETStatus.ReciveMessage, receivedData, result.Count, clientId));
                }
            }
            catch (OperationCanceledException) { }
            catch (WebSocketException ex)
            {
                OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, $"{clientId} 通信异常: {ex.Message}", 0, clientId));
            }
            finally
            {
                _clients.Remove(clientId);
                socket.Dispose();
                OnStatusChanged(new Result(Result.NETStatus.RemoteStop, clientId));
            }
        }

        protected override void OnStopping()
        {
            _listener?.Stop();
            _listener = null;
            foreach (string key in _clients.connectionKeys.ToArray())
            {
                _clients.Remove(key)?.Dispose();
            }
        }

        public override void Send(string data, string target = null)
        {
            if (target == null)
                throw new ArgumentException("WebSocket 发送需要指定客户端标识");
            if (_isClientMode)
            {
                // 发送给远程服务器
                var buffer = Encoding.UTF8.GetBytes(data);
                _clientSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, _cts.Token).Wait();
            }
            else
            {
                if (_clients.TryGet(target, out WebSocket socket))
                {
                    try
                    {
                        byte[] buffer = Encoding.UTF8.GetBytes(data);
                        socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, _cts.Token).Wait();
                        Result rs = new Result(Result.NETStatus.SendMessage, data, buffer.Length, target);
                        rs.status = Result.ResStatus.SetNum;
                        OnStatusChanged(rs);
                    }
                    catch (Exception ex)
                    {
                        OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, $"发送到 {target} 失败: {ex.Message}", 0, target));
                        _clients.Remove(target)?.Dispose();
                        OnStatusChanged(new Result(Result.NETStatus.RemoteStop, target));
                    }
                }
            }
        }

        public BindingList<string> GetPeer() => _clients.connectionKeys;
    }
}