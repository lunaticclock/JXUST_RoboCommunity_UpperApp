using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UpperApp
{
    internal abstract class BaseCommunicationManager: IAsyncDisposable
    {
        protected readonly ChannelType _channel;
        public event Action<Result> StatusChanged;
        protected Encoding encoding;
        protected CancellationTokenSource _cts;
        protected bool _isMonitoring;
        public bool IsMonitoring => _isMonitoring;
        protected bool _isStopping;  // 新增：标记正在主动停止，避免触发 RemoteStop

        // 静态构造，确保编码提供器只注册一次
        static BaseCommunicationManager()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        protected BaseCommunicationManager(ChannelType channel)
        {
            encoding = Encoding.GetEncoding("GB2312");
            _channel = channel;
        }

        protected void OnStatusChanged(Result result)
        {
            if (result.Channel == ChannelType.Unknown)
                result.Channel = _channel;
            StatusChanged?.Invoke(result);
        }

        public void StartCore()
        {
            if (_isMonitoring) StopMonitor();
            _cts = new CancellationTokenSource();
            OnStatusChanged(new Result(Result.NETStatus.MonitorStart, "监听开始"));
            _isMonitoring = true;
        }

        // 停止方法（统一模板）
        public virtual void StopMonitor()
        {
            _isStopping = true;
            if (!_isMonitoring) return;
            _isMonitoring = false;
            _cts.Cancel();
            _cts.Dispose();
            OnStopping(); // 钩子：派生类清理特定资源
            OnStatusChanged(new Result(Result.NETStatus.MonitorStop, GetStopMessage()));
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

        // 派生类实现资源清理（如关闭 Socket、串口等）
        protected abstract void OnStopping();

        // 发送数据（必须实现）
        public abstract void Send(string data, string target = null);

        // 异步释放资源
        public virtual async ValueTask DisposeAsync()
        {
            StopMonitor();
            await Task.CompletedTask;
        }
    }
}
