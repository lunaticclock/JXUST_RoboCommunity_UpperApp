using System;
using System.Collections.Generic;
using UpperApp.Core;
using UpperApp.Services;

namespace UpperApp.Communication
{
    /// <summary>
    /// 通信管理器接口（每个通道独立实现）
    /// </summary>
    internal interface ICommunicator : IAsyncDisposable
    {
        /// <summary>
        /// 状态变化事件（接收消息、发送消息、异常、连接变化等）
        /// </summary>
        event Action<StatusEvent> StatusChanged;

        /// <summary>
        /// 通信通道类型
        /// </summary>
        ChannelType Channel { get; }

        DeviceState State { get; }

        /// <summary>
        /// 启动通信（根据参数不同可多次调用，内部应先停止旧连接）
        /// </summary>
        /// <param name="parameters">通信参数，类型需与通道匹配</param>
        void Start(CommunicationParams parameters);

        /// <summary>
        /// 停止通信，释放资源
        /// </summary>
        void Stop();

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data">字符串数据（编码由管理器内部决定）</param>
        /// <param name="target">目标标识：TCP/UDP时为 IP:Port，蓝牙时为设备名，串口/CAN时可为null</param>
        void Send(string data, string target = null);

        /// <summary>
        /// 发送原始字节（用于 Hex 模式，绕过字符编码，直接发送用户指定的字节序列）
        /// </summary>
        /// <param name="data">原始字节</param>
        /// <param name="target">目标标识</param>
        void Send(byte[] data, string target = null);

        /// <summary>
        /// 获取当前连接的对端列表（用于 UI 绑定）
        /// </summary>
        IReadOnlyList<string> GetPeerList();
    }
}
