using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpperApp.Core;
using UpperApp.Services;

namespace UpperApp.Communication
{
    /// <summary>
    /// ICommunicator 的抽象基类，封装 State 属性、StatusChanged 事件、状态上报工厂方法和 DisposeAsync 默认实现。
    /// 子类通过 NotifyX 工厂方法上报事件，无需手动构造 StatusEvent，Channel 由基类自动注入。
    /// </summary>
    internal abstract class CommunicatorBase : ICommunicator
    {
        private DeviceState _state = DeviceState.Disconnected;

        public abstract ChannelType Channel { get; }

        public DeviceState State
        {
            get => _state;
            protected set
            {
                if (_state != value)
                    _state = value;
            }
        }

        /// <summary>
        /// 停止重入保护标志：Stop 执行期间为 true，用于回调中判断是否为主动停止。
        /// </summary>
        protected bool IsStopping { get; private set; }

        /// <summary>
        /// 是否处于监听/连接状态。由 NotifyMonitorStarted 置位，Stop 时清零；
        /// 子类可在后台循环结束时手动置 false。
        /// </summary>
        protected bool IsMonitoring { get; set; }

        public event Action<StatusEvent> StatusChanged;

        public abstract void Start(CommunicationParams parameters);
        public abstract void Send(string data, string target = null);
        public abstract void Send(byte[] data, string target = null);
        public abstract IReadOnlyList<string> GetPeerList();

        /// <summary>停止时上报的提示文本。</summary>
        protected abstract string StoppedMessage { get; }

        /// <summary>子类在此释放各自的资源，无需关心状态与事件上报。</summary>
        protected abstract void StopCore();

        /// <summary>
        /// 停止模板：去重判断 → 进入停止态 → 释放资源 → 上报停止 → 结束停止态。
        /// </summary>
        public void Stop()
        {
            if (IsStopping) return;
            if (!IsMonitoring && State == DeviceState.Disconnected) return;

            BeginStop();
            IsMonitoring = false;
            try
            {
                StopCore();
            }
            finally
            {
                NotifyMonitorStopped(StoppedMessage);
                EndStop();
            }
        }

        // ===== 发送骨架 =====
        // 子类只负责"往通道写字节"，成功上报与异常分流由这两个工具方法统一处理。

        /// <summary>空负载校验：为空时上报告警并返回 false，调用方应立即返回。</summary>
        protected bool HasPayload(byte[] data)
        {
            if (data != null && data.Length > 0) return true;
            NotifyMessageSendAlert("未发出信息!");
            return false;
        }

        /// <summary>
        /// 发送骨架：执行写入并统一上报成功；写入抛出异常时交给 onError 处理。
        /// </summary>
        /// <param name="payload">实际写入通道的字节。</param>
        /// <param name="display">上报日志用的展示文本（原文或十六进制）。</param>
        /// <param name="target">目标标识。</param>
        /// <param name="write">通道写入动作。</param>
        /// <param name="onError">写入失败时的异常处理。</param>
        protected void WriteAndReport(byte[] payload, string display, string target,
            Action<byte[]> write, Action<Exception> onError)
        {
            try
            {
                write(payload);
                NotifyMessageSent(display, payload.Length, target);
            }
            catch (Exception ex)
            {
                onError(ex);
            }
        }

        // ===== 状态上报工厂方法 =====
        // 子类调用这些方法代替手动构造 StatusEvent，Channel 自动注入，状态变化封装在内部。

        /// <summary>上报接收到数据。</summary>
        protected void NotifyMessageReceived(string content, int byteCount, string source, string peerHint = "")
            => Raise(new MessageReceivedEvent(Channel, content, byteCount, source, peerHint));

        /// <summary>上报发送成功。</summary>
        protected void NotifyMessageSent(string content, int byteCount, string target)
            => Raise(new MessageSentEvent(Channel, content, byteCount, target, SendResult.Success));

        /// <summary>上报发送失败（错误）。</summary>
        protected void NotifyMessageSendError(string reason, string target = "")
            => Raise(new MessageSentEvent(Channel, reason, 0, target, SendResult.Error));

        /// <summary>上报发送告警（如空数据）。</summary>
        protected void NotifyMessageSendAlert(string reason, string target = "")
            => Raise(new MessageSentEvent(Channel, reason, 0, target, SendResult.Alert));

        /// <summary>上报新对端连接。</summary>
        protected void NotifyPeerConnected(string peer, string message = "")
            => Raise(new PeerConnectedEvent(Channel, peer, message));

        /// <summary>上报对端断开。</summary>
        protected void NotifyPeerDisconnected(string reason, string peer = "")
            => Raise(new PeerDisconnectedEvent(Channel, reason, peer));

        /// <summary>上报监听/连接已启动，同时将 State 置为 Connected、IsMonitoring 置为 true。</summary>
        protected void NotifyMonitorStarted(string message)
        {
            State = DeviceState.Connected;
            IsMonitoring = true;
            Raise(new MonitorStartedEvent(Channel, message));
        }

        /// <summary>上报监听/连接已停止（不修改 State，由 BeginStop/EndStop 控制）。</summary>
        protected void NotifyMonitorStopped(string message)
            => Raise(new MonitorStoppedEvent(Channel, message));

        /// <summary>上报通信异常，同时将 State 置为 Error。</summary>
        protected void NotifyException(string message, string remoteIP = "")
        {
            State = DeviceState.Error;
            Raise(new ExceptionOccurredEvent(Channel, message, remoteIP));
        }

        /// <summary>上报手动停止。</summary>
        protected void NotifyManualStopped()
            => Raise(new ManualStoppedEvent(Channel));

        private void Raise(StatusEvent evt) => StatusChanged?.Invoke(evt);

        /// <summary>
        /// 标记进入停止流程（供子类 Stop 方法调用）。
        /// </summary>
        protected void BeginStop()
        {
            IsStopping = true;
            State = DeviceState.Disconnecting;
        }

        /// <summary>
        /// 标记停止流程结束（供子类 Stop 方法调用）。
        /// </summary>
        protected void EndStop()
        {
            State = DeviceState.Disconnected;
            IsStopping = false;
        }

        public virtual ValueTask DisposeAsync()
        {
            Stop();
            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }
    }
}
