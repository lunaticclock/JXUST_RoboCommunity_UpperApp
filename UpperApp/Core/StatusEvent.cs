namespace UpperApp.Core
{
    /// <summary>
    /// 通信状态事件的抽象基类。每个子类对应一种语义明确的事件，
    /// 替代原 Result 万能对象。Channel 由 CommunicatorBase 工厂方法自动注入。
    /// </summary>
    internal abstract record StatusEvent(ChannelType Channel);

    /// <summary>接收到数据。</summary>
    /// <param name="Content">接收到的字符串内容。</param>
    /// <param name="ByteCount">字节计数。</param>
    /// <param name="Source">数据来源标识（IP:Port / COM / CAN ID / 设备名），可为空。</param>
    /// <param name="PeerHint">切换到新对端时的提示前缀（如 "\r\nfrom xxx:\r\n"），可为空。</param>
    internal sealed record MessageReceivedEvent(
        ChannelType Channel, string Content, int ByteCount, string Source, string PeerHint = "")
        : StatusEvent(Channel);

    /// <summary>发送结果。</summary>
    internal enum SendResult { Success, Error, Alert }

    /// <summary>发送数据的结果上报。</summary>
    /// <param name="Content">发送的内容（成功时）或错误描述（失败时）。</param>
    /// <param name="ByteCount">实际发送字节数，失败时为 0。</param>
    /// <param name="Target">目标标识（IP:Port / 设备名 / CAN ID），可为空。</param>
    /// <param name="Result">发送结果。</param>
    internal sealed record MessageSentEvent(
        ChannelType Channel, string Content, int ByteCount, string Target, SendResult Result)
        : StatusEvent(Channel);

    /// <summary>新对端连接（TCP/UDP 客户端接入、蓝牙从设备连接等）。</summary>
    /// <param name="Peer">对端标识。</param>
    /// <param name="Message">附加描述信息，可为空。</param>
    internal sealed record PeerConnectedEvent(ChannelType Channel, string Peer, string Message = "")
        : StatusEvent(Channel);

    /// <summary>对端断开。</summary>
    /// <param name="Reason">断开原因/描述。</param>
    /// <param name="Peer">对端标识，可为空。</param>
    internal sealed record PeerDisconnectedEvent(ChannelType Channel, string Reason, string Peer = "")
        : StatusEvent(Channel);

    /// <summary>监听/连接已启动。</summary>
    internal sealed record MonitorStartedEvent(ChannelType Channel, string Message)
        : StatusEvent(Channel);

    /// <summary>监听/连接已停止。</summary>
    internal sealed record MonitorStoppedEvent(ChannelType Channel, string Message)
        : StatusEvent(Channel);

    /// <summary>通信异常。</summary>
    /// <param name="Message">异常描述。</param>
    /// <param name="RemoteIP">关联的远端标识，可为空。</param>
    internal sealed record ExceptionOccurredEvent(ChannelType Channel, string Message, string RemoteIP = "")
        : StatusEvent(Channel);

    /// <summary>手动停止（用户主动操作触发）。</summary>
    internal sealed record ManualStoppedEvent(ChannelType Channel)
        : StatusEvent(Channel);
}
