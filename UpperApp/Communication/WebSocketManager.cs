using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UpperApp.Core;
using UpperApp.Services;

namespace UpperApp.Communication
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    internal class WebSocketManager : CommunicatorBase
    {
        private HttpListener _listener;
        private readonly BindingDic<WebSocket> _serverClients = new();
        private ClientWebSocket _clientSocket;
        private bool _isClientMode;
        private string _clientTarget;
        private CancellationTokenSource _cts;
        private bool _isMonitoring;
        private readonly Encoding _encoding = Encoding.GetEncoding("GB2312");

        public override ChannelType Channel => ChannelType.WebSocket;

        static WebSocketManager()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public override void Start(CommunicationParams parameters)
        {
            if (parameters is not WebSocketParams wsParams)
                throw new ArgumentException("参数类型必须为 WebSocketParams");

            Stop();
            State = DeviceState.Connecting;
            _cts = new CancellationTokenSource();

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
            NotifyMonitorStarted($"WebSocket 监听 {url}");
            _isMonitoring = true;
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
                NotifyMonitorStarted($"WebSocket 客户端已连接 {serverUrl}");
                _isMonitoring = true;
            }
            catch (Exception ex)
            {
                NotifyException($"WebSocket 连接失败: {ex.Message}");
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
                        NotifyPeerConnected(clientId, $"WebSocket 客户端连接: {clientId}");
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
                NotifyException($"监听异常: {ex.Message}");
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
                    NotifyMessageReceived(receivedData, result.Count, clientId);
                }
            }
            catch (OperationCanceledException) { }
            catch (WebSocketException ex)
            {
                NotifyException($"{clientId} 通信异常: {ex.Message}", clientId);
            }
            finally
            {
                if (!_isClientMode)
                    _serverClients.Remove(clientId);
                socket.Dispose();
                if (!IsStopping)
                    NotifyPeerDisconnected("WebSocket 断开", clientId);
            }
        }

        public override void Stop()
        {
            if (IsStopping) return;
            if (!_isMonitoring && State == DeviceState.Disconnected) return;

            BeginStop();
            _isMonitoring = false;
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

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

            NotifyMonitorStopped("WebSocket 已停止");
            EndStop();
        }

        public override void Send(string data, string target = null)
        {
            if (_isClientMode)
            {
                if (_clientSocket == null || _clientSocket.State != WebSocketState.Open)
                {
                    NotifyMessageSendError("WebSocket 客户端未连接");
                    return;
                }
                try
                {
                    var buffer = Encoding.UTF8.GetBytes(data);
                    _clientSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, _cts.Token).Wait();
                    NotifyMessageSent(data, buffer.Length, _clientTarget ?? "");
                }
                catch (Exception ex)
                {
                    NotifyException($"发送失败: {ex.Message}", _clientTarget ?? "");
                }
            }
            else
            {
                if (string.IsNullOrEmpty(target))
                {
                    NotifyMessageSendError("WebSocket 服务器模式需要指定客户端标识");
                    return;
                }
                if (_serverClients.TryGet(target, out WebSocket socket))
                {
                    try
                    {
                        byte[] buffer = Encoding.UTF8.GetBytes(data);
                        socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, _cts.Token).Wait();
                        NotifyMessageSent(data, buffer.Length, target);
                    }
                    catch (Exception ex)
                    {
                        NotifyException($"发送到 {target} 失败: {ex.Message}", target);
                        _serverClients.Remove(target)?.Dispose();
                        NotifyPeerDisconnected("发送失败导致断开", target);
                    }
                }
                else
                {
                    NotifyMessageSendError($"未找到客户端标识: {target}");
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
