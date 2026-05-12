using System.IO.Ports;

namespace UpperApp.Communication
{
    /// <summary>
    /// 通信参数基类（便于序列化）
    /// </summary>
    internal abstract class CommunicationParams { }

    /// <summary>
    /// 串口通信参数
    /// </summary>
    internal class SerialParams : CommunicationParams
    {
        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public Parity Parity { get; set; } = Parity.None;
        public int DataBits { get; set; } = 8;
        public StopBits StopBits { get; set; } = StopBits.One;
    }

    /// <summary>
    /// TCP 服务器参数（监听模式）
    /// </summary>
    internal class TcpServerParams : CommunicationParams
    {
        public string LocalIP { get; set; }
        public int Port { get; set; }
    }

    /// <summary>
    /// UDP 通信参数
    /// </summary>
    internal class UdpParams : CommunicationParams
    {
        public string LocalIP { get; set; }
        public int Port { get; set; }
    }

    /// <summary>
    /// 蓝牙通信参数
    /// </summary>
    internal class BluetoothParams : CommunicationParams
    {
        /// <summary>
        /// true: 作为服务端监听；false: 作为客户端主动连接指定设备
        /// </summary>
        public bool IsServerMode { get; set; } = true;

        /// <summary>
        /// 客户端模式下要连接的设备名称
        /// </summary>
        public string TargetDeviceName { get; set; }
    }

    /// <summary>
    /// CAN 总线参数
    /// </summary>
    internal class CanParams : CommunicationParams
    {
        /// <summary>
        /// CAN 通道标识，如 "PCAN_USBBUS1"
        /// </summary>
        public string ChannelName { get; set; } = "PCAN_USBBUS1";
    }

    /// <summary>
    /// WebSocket 参数
    /// </summary>
    internal class WebSocketParams : CommunicationParams
    {
        /// <summary>
        /// true: 服务端模式，监听 URL（如 http://localhost:8080/）；false: 客户端模式
        /// </summary>
        public bool IsServerMode { get; set; } = true;

        /// <summary>
        /// 服务端监听前缀或客户端连接 URL
        /// </summary>
        public string Url { get; set; }
    }
}
