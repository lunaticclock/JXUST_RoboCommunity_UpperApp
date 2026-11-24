using System;
using System.Collections.Generic;
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

        public override ChannelType Channel => ChannelType.WebSocket;

        protected override string StoppedMessage => "WebSocket 已停止";

        protected override void StopCore()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

            try { _listener?.Stop(); } catch { }
            _listener = null;

            foreach (string key in _serverClients.Keys)
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
                IsMonitoring = false;
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

        public override void Send(string data, string target = null)
            => Transmit(Encoding.UTF8.GetBytes(data), data, WebSocketMessageType.Text, target);

        public override void Send(byte[] data, string target = null)
        {
            if (!HasPayload(data)) return;
            Transmit(data, Utils.BytesToHexString(data), WebSocketMessageType.Binary, target);
        }

        private void Transmit(byte[] payload, string display, WebSocketMessageType type, string target)
        {
            if (_isClientMode)
            {
                if (_clientSocket == null || _clientSocket.State != WebSocketState.Open)
                {
                    NotifyMessageSendError("WebSocket 客户端未连接");
                    return;
                }
                WriteAndReport(payload, display, _clientTarget ?? "",
                    p => _clientSocket.SendAsync(new ArraySegment<byte>(p), type, true, _cts.Token).Wait(),
                    ex => NotifyException($"发送失败: {ex.Message}", _clientTarget ?? ""));
                return;
            }

            if (string.IsNullOrEmpty(target))
            {
                NotifyMessageSendError("WebSocket 服务器模式需要指定客户端标识");
                return;
            }
            if (!_serverClients.TryGet(target, out WebSocket socket))
            {
                NotifyMessageSendError($"未找到客户端标识: {target}");
                return;
            }

            WriteAndReport(payload, display, target,
                p => socket.SendAsync(new ArraySegment<byte>(p), type, true, _cts.Token).Wait(),
                ex =>
                {
                    NotifyException($"发送到 {target} 失败: {ex.Message}", target);
                    _serverClients.Remove(target)?.Dispose();
                    NotifyPeerDisconnected("发送失败导致断开", target);
                });
        }

        public override IReadOnlyList<string> GetPeerList()
        {
            if (_isClientMode) return [];
            return _serverClients.Keys;
        }
    }
}
