using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UpperApp.Core;

namespace UpperApp.Communication
{
    internal class WebSocketManager : BaseCommunicationManager
    {
        private HttpListener _listener;
        private readonly BindingDic<WebSocket> _serverClients = new();
        private ClientWebSocket _clientSocket;
        private bool _isClientMode;
        private string _clientTarget; // 客户端模式下，保存远程标识

        public WebSocketManager() : base(ChannelType.WebSocket) { }

        public override void Start(CommunicationParams parameters)
        {
            if (parameters is not WebSocketParams wsParams)
                throw new ArgumentException("参数类型必须为 WebSocketParams");

            StartCore();

            if (wsParams.IsServerMode)
            {
                StartServer(wsParams.Url);
            }
            else
            {
                _ = ConnectAsync(wsParams.Url);
            }
        }

        private void StartServer(string url)
        {
            if (_listener == null)
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add(url);
            }
            _listener.Start();
            _ = AcceptLoopAsync(_cts.Token);
            OnStatusChanged(new Result(Result.NETStatus.MonitorStart, $"WebSocket 监听 {url}"));
        }

        private async Task ConnectAsync(string serverUrl)
        {
            _isClientMode = true;
            _clientSocket = new ClientWebSocket();
            try
            {
                await _clientSocket.ConnectAsync(new Uri(serverUrl), _cts.Token);
                _clientTarget = serverUrl;
                _ = ReceiveLoopAsync(_clientSocket, "Server", _cts.Token);
                OnStatusChanged(new Result(Result.NETStatus.MonitorStart, $"WebSocket 客户端已连接 {serverUrl}"));
            }
            catch (Exception ex)
            {
                OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, $"WebSocket 连接失败: {ex.Message}"));
                Stop();
            }
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
                        _serverClients.Add(clientId, socket);
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
            catch (OperationCanceledException) { }
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
                if (!_isClientMode)
                    _serverClients.Remove(clientId);
                socket.Dispose();
                OnStatusChanged(new Result(Result.NETStatus.RemoteStop, clientId));
            }
        }

        protected override void OnStopping()
        {
            try { _listener?.Stop(); } catch { }
            _listener = null;

            foreach (string key in _serverClients.connectionKeys.ToArray())
            {
                try { _serverClients.Remove(key)?.Dispose(); } catch { }
            }

            if (_clientSocket != null && (_clientSocket.State == WebSocketState.Open || _clientSocket.State == WebSocketState.Connecting))
            {
                try { _clientSocket.Abort(); } catch { }
            }
            try { _clientSocket?.Dispose(); } catch { }
            _clientSocket = null;
        }

        public override void Send(string data, string target = null)
        {
            if (_isClientMode)
            {
                if (_clientSocket == null || _clientSocket.State != WebSocketState.Open)
                {
                    OnStatusChanged(new Result(Result.NETStatus.SendMessage, "WebSocket 客户端未连接", 0) with { Status = Result.ResStatus.Error });
                    return;
                }
                try
                {
                    var buffer = Encoding.UTF8.GetBytes(data);
                    _clientSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, _cts.Token).Wait();
                    var result = new Result(Result.NETStatus.SendMessage, data, buffer.Length, _clientTarget) with { Status = Result.ResStatus.SetNum };
                    OnStatusChanged(result);
                }
                catch (Exception ex)
                {
                    OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, $"发送失败: {ex.Message}", 0, _clientTarget));
                }
            }
            else // 服务器模式
            {
                if (string.IsNullOrEmpty(target))
                {
                    OnStatusChanged(new Result(Result.NETStatus.SendMessage, "WebSocket 服务器模式需要指定客户端标识", 0) with { Status = Result.ResStatus.Error });
                    return;
                }
                if (_serverClients.TryGet(target, out WebSocket socket))
                {
                    try
                    {
                        byte[] buffer = Encoding.UTF8.GetBytes(data);
                        socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, _cts.Token).Wait();
                        var result = new Result(Result.NETStatus.SendMessage, data, buffer.Length, target) with { Status = Result.ResStatus.SetNum };
                        OnStatusChanged(result);
                    }
                    catch (Exception ex)
                    {
                        OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, $"发送到 {target} 失败: {ex.Message}", 0, target));
                        _serverClients.Remove(target)?.Dispose();
                        OnStatusChanged(new Result(Result.NETStatus.RemoteStop, target));
                    }
                }
                else
                {
                    OnStatusChanged(new Result(Result.NETStatus.SendMessage, $"未找到客户端标识: {target}", 0) with { Status = Result.ResStatus.Error });
                }
            }
        }

        public override IReadOnlyList<string> GetPeerList()
        {
            if (_isClientMode) return [];
            return _serverClients.connectionKeys;
        }
    }
}