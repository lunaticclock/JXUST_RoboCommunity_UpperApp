using System;
using System.Runtime.Versioning;
using UpperApp.Core;

namespace UpperApp.Communication
{
    internal interface ICommunicatorFactory
    {
        ICommunicator Create(ChannelType channel);
    }

    [SupportedOSPlatform("windows10.0.19041.0")]
    internal class CommunicatorFactory : ICommunicatorFactory
    {
        public ICommunicator Create(ChannelType channel)
        {
            return channel switch
            {
                ChannelType.Serial => new SerManager(),
                ChannelType.TCP => new TCPManager(),     // 直接使用原生管理器，不再用 Adapter
                ChannelType.UDP => new UDPManager(),
                ChannelType.Bluetooth => new BthManager(),
                ChannelType.CAN => new CANManager(),
                ChannelType.WebSocket => new WebSocketManager(),
                _ => throw new NotSupportedException($"不支持的通道类型: {channel}")
            };
        }
    }
}
