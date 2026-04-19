using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace UpperApp
{
    internal interface INetworkManager
    {
        // 事件：接收数据、状态变化等
        event Action<Result> StatusChanged;

        // 启动/停止监听（对于串口/蓝牙/网络服务器）
        void StartMonitor(IPEndPoint localEndpoint = null);
        void StopMonitor();
        bool IsMonitoring { get; }

        // 发送数据（字符串）
        void Send(string data, string target = null);  // target 可以是 IP:Port、设备名等

        // 绑定到 UI 的数据源（如连接列表）
        object GetPeerDataSource();  // 返回 BindingList 或其他可绑定集合

        // 获取当前连接的对端标识（如 IP:Port）
        string CurrentPeer { get; }

        void Remove(string peer);

        // 清理资源
        ValueTask DisposeAsync();
    }

    internal class TcpManagerAdapter : INetworkManager
    {
        private readonly TCPManager _tcpManager = new TCPManager();
        public event Action<Result> StatusChanged
        {
            add => _tcpManager.StatusChanged += value;
            remove => _tcpManager.StatusChanged -= value;
        }

        public bool IsMonitoring => _tcpManager.IsMonitoring;
        public string CurrentPeer => ""; // TCP 由外部维护，可返回 Peer.Text

        public void StartMonitor(IPEndPoint localEndpoint = null)
        {
            if (localEndpoint == null) throw new ArgumentNullException(nameof(localEndpoint));
            _tcpManager.StartMonitor(localEndpoint);
        }

        public void StopMonitor() => _tcpManager.StopMonitor();

        public void Send(string data, string target = null)
        {
            if (string.IsNullOrEmpty(target)) throw new ArgumentException("Target required for TCP");
            _tcpManager.Send(target, data);
        }

        public object GetPeerDataSource() => _tcpManager.TCPdic.connectionKeys;

        public async ValueTask DisposeAsync()
        {
            await _tcpManager.DisposeAsync();
        }

        public void Remove(string peer) => _tcpManager.Remove(peer);
    }

    internal class UdpManagerAdapter : INetworkManager
    {
        private readonly UDPManager _udpManager = new UDPManager();
        public event Action<Result> StatusChanged
        {
            add => _udpManager.StatusChanged += value;
            remove => _udpManager.StatusChanged -= value;
        }

        public bool IsMonitoring => _udpManager.IsMonitoring;
        public string CurrentPeer => "";

        public void StartMonitor(IPEndPoint localEndpoint = null)
        {
            if (localEndpoint == null) throw new ArgumentNullException(nameof(localEndpoint));
            _udpManager.StartMonitor(localEndpoint);
        }

        public void StopMonitor() => _udpManager.StopMonitor();

        public void Send(string data, string target = null)
        {
            if (string.IsNullOrEmpty(target)) throw new ArgumentException("Target required for UDP");
            _udpManager.UDP_Send(data, target);
        }

        public object GetPeerDataSource() => _udpManager.GetUDPPeer();

        public async ValueTask DisposeAsync()
        {
            await _udpManager.DisposeAsync();
        }

        public void Remove(string peer) => _udpManager.GetUDPPeer().Remove(peer);
    }
}
