using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UpperApp.Communication;
using UpperApp.Core;

namespace UpperApp.Core
{
    /// <summary>
    /// 应用程序持久化配置
    /// </summary>
    internal class AppSettings
    {
        // 最近使用的通道
        public ChannelType LastActiveSendChannel { get; set; } = ChannelType.Serial;

        // 各通信协议参数
        public SerialParams SerialConfig { get; set; } = new SerialParams();
        public TcpServerParams TcpConfig { get; set; } = new TcpServerParams();
        public UdpParams UdpConfig { get; set; } = new UdpParams();
        public BluetoothParams BthConfig { get; set; } = new BluetoothParams();
        public CanParams CanConfig { get; set; } = new CanParams();
        public WebSocketParams WebSocketConfig { get; set; } = new WebSocketParams();

        // UI 偏好
        public int SliderSmallChange { get; set; } = 25;
        public int AutoSendIntervalMs { get; set; } = 1000;
        public bool AutoSendEnabled { get; set; } = false;
        public bool LocalEcho { get; set; } = true;
        public bool AngleDisplayEnabled { get; set; } = false;
        public bool SaveDataEnabled { get; set; } = false;
        public string SelectedNetType { get; set; } = "TCP";
        public bool IsHexMode { get; set; } = false;
        public bool IsCharMode { get; set; } = true;

        // 批量消息列表（最多8条）
        public List<PresetMessage> PresetMessages { get; set; } = [];

        // 地图相关
        public string LastMapImagePath { get; set; } = "";
        public float CalibratedDistance { get; set; } = 1.0f;

        // 窗口位置与大小（可选）
        public int WindowLeft { get; set; } = 0;
        public int WindowTop { get; set; } = 0;
        public int WindowWidth { get; set; } = 1113;
        public int WindowHeight { get; set; } = 383;
    }

    /// <summary>
    /// 单条预设消息
    /// </summary>
    internal class PresetMessage
    {
        public string Text { get; set; } = "";
        public bool IsHex { get; set; } = false;
    }
}
