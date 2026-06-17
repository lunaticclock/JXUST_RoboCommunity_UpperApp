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

        public event Action<StatusEvent> StatusChanged;

        public abstract void Start(CommunicationParams parameters);
        public abstract void Stop();
        public abstract void Send(string data, string target = null);
        public abstract IReadOnlyList<string> GetPeerList();

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

        /// <summary>上报监听/连接已启动，同时将 State 置为 Connected。</summary>
        protected void NotifyMonitorStarted(string message)
        {
            State = DeviceState.Connected;
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
