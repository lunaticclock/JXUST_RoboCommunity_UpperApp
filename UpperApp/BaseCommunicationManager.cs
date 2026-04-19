using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace UpperApp
{
    internal abstract class BaseCommunicationManager
    {
        protected readonly ChannelType _channel;
        public event Action<Result> StatusChanged;
        protected Encoding encoding;
        protected CancellationTokenSource _cts = new();
        protected bool _isMonitoring;
        public bool IsMonitoring => _isMonitoring;

        protected BaseCommunicationManager(ChannelType channel)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            encoding = Encoding.GetEncoding("GB2312");
            _channel = channel;
        }

        protected void OnStatusChanged(Result result)
        {
            if (result.Channel == ChannelType.Unknown)
                result.Channel = _channel;
            StatusChanged?.Invoke(result);
        }

        public virtual void StartMonitor() { }
        public virtual void StartMonitor(IPEndPoint localEndPoint) { }
        public virtual void StopMonitor() { }
    }
}
