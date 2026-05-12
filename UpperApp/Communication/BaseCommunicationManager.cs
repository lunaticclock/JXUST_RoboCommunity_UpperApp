using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UpperApp.Core;
using UpperApp.Services;

namespace UpperApp.Communication
{
    internal abstract class BaseCommunicationManager(ChannelType channel) : ICommunicator, IAsyncDisposable, IDisposable
    {
        protected readonly ChannelType _channel = channel;
        public event Action<Result> StatusChanged;
        protected Encoding encoding = Encoding.GetEncoding("GB2312");
        protected CancellationTokenSource _cts;
        protected bool _isMonitoring;
        protected bool _isStopping;
        private DeviceState _state = DeviceState.Disconnected;
        public DeviceState State
        {
            get => _state;
            protected set
            {
                if (_state != value)
                {
                    var old = _state;
                    _state = value;
                    OnStateChanged(old, value);
                }
            }
        }

        protected virtual void OnStateChanged(DeviceState oldState, DeviceState newState)
        {
        }

        public ChannelType Channel => _channel;

        static BaseCommunicationManager()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        protected void OnStatusChanged(Result result)
        {
            if (result.Channel == ChannelType.Unknown)
                result = result with { Channel = _channel };
            if (result.NetStatus == Result.NETStatus.ExceptionStop)
                State = DeviceState.Error;
            StatusChanged?.Invoke(result);
        }

        public void Dispose()
        {
            if (_isMonitoring) Stop();
            _cts?.Dispose();
            GC.SuppressFinalize(this);
        }

        public ValueTask DisposeAsync()
        {
            if (_isMonitoring) Stop();
            _cts?.Dispose();
            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }

        protected void StartCore()
        {
            State = DeviceState.Connecting;
            if (_isMonitoring) Stop();
            _cts = new CancellationTokenSource();
            State = DeviceState.Connected;
            OnStatusChanged(new Result(Result.NETStatus.MonitorStart, "监听开始"));
            _isMonitoring = true;
        }

        public virtual void Stop()
        {
            _isStopping = true;
            State = DeviceState.Disconnecting;
            if (!_isMonitoring) return;
            _isMonitoring = false;
            _cts.Cancel();
            _cts.Dispose();
            OnStopping();
            OnStatusChanged(new Result(Result.NETStatus.MonitorStop, GetStopMessage()));
            State = DeviceState.Disconnected;
            _isStopping = false;
        }

        protected virtual string GetStopMessage()
        {
            return _channel switch
            {
                ChannelType.Serial => "串口已停止",
                ChannelType.TCP => "TCP 已停止",
                ChannelType.UDP => "UDP 已停止",
                ChannelType.Bluetooth => "蓝牙已停止",
                _ => "通信已停止"
            };
        }

        protected abstract void OnStopping();
        public abstract void Send(string data, string target = null);
        public abstract void Start(CommunicationParams parameters);
        public abstract IReadOnlyList<string> GetPeerList();

    }
}
