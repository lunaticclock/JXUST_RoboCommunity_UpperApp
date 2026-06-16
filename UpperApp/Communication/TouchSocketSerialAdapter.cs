using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.SerialPorts;
using UpperApp.Core;
using UpperApp.Services;

namespace UpperApp.Communication
{
    internal class TouchSocketSerialAdapter : ICommunicator
    {
        private SerialPortClient _serialClient;
        private DeviceState _state = DeviceState.Disconnected;
        private bool _isStopping;
        private readonly Encoding _encoding = Encoding.GetEncoding("GB2312");

        public event Action<UpperApp.Core.Result> StatusChanged;

        public ChannelType Channel => ChannelType.Serial;

        public DeviceState State
        {
            get => _state;
            private set
            {
                if (_state != value)
                {
                    _state = value;
                }
            }
        }

        static TouchSocketSerialAdapter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void Start(CommunicationParams parameters)
        {
            if (parameters is not SerialParams serialParams)
                throw new ArgumentException("参数类型必须为 SerialParams");

            Stop();
            State = DeviceState.Connecting;

            _serialClient = new SerialPortClient();

            _serialClient.Received = (client, e) =>
            {
                var receivedData = e.Memory.Span.ToString(_encoding);
                OnStatusChanged(new UpperApp.Core.Result(UpperApp.Core.Result.NETStatus.ReciveMessage, receivedData, e.Memory.Length, "COM"));
                return EasyTask.CompletedTask;
            };

            _serialClient.Closed = (client, e) =>
            {
                if (!_isStopping)
                {
                    OnStatusChanged(new UpperApp.Core.Result(UpperApp.Core.Result.NETStatus.ExceptionStop, "串口异常断开"));
                }
                return EasyTask.CompletedTask;
            };

            var config = new TouchSocketConfig()
                .SetSerialPortOption(options =>
                {
                    options.PortName = serialParams.PortName;
                    options.BaudRate = serialParams.BaudRate;
                    options.DataBits = serialParams.DataBits;
                    options.Parity = serialParams.Parity;
                    options.StopBits = serialParams.StopBits;
                });

            try
            {
                _serialClient.SetupAsync(config).GetAwaiter().GetResult();
                _serialClient.ConnectAsync(CancellationToken.None).GetAwaiter().GetResult();
                State = DeviceState.Connected;
                OnStatusChanged(new UpperApp.Core.Result(UpperApp.Core.Result.NETStatus.MonitorStart, $"串口 {serialParams.PortName} 已打开"));
            }
            catch (Exception ex)
            {
                OnStatusChanged(new UpperApp.Core.Result(UpperApp.Core.Result.NETStatus.ExceptionStop, ex.Message));
                _serialClient?.SafeDispose();
                _serialClient = null;
                State = DeviceState.Error;
            }

            _isStopping = false;
        }

        public void Stop()
        {
            if (_serialClient == null && _state == DeviceState.Disconnected) return;

            _isStopping = true;
            State = DeviceState.Disconnecting;

            _serialClient?.CloseAsync("").GetAwaiter().GetResult();
            _serialClient?.SafeDispose();
            _serialClient = null;

            OnStatusChanged(new UpperApp.Core.Result(UpperApp.Core.Result.NETStatus.MonitorStop, "串口已停止"));
            State = DeviceState.Disconnected;
        }

        public void Send(string data, string target = null)
        {
            if (_serialClient == null || !_serialClient.Online)
            {
                OnStatusChanged(new UpperApp.Core.Result(UpperApp.Core.Result.NETStatus.SendMessage, "串口未打开", 0, "") { Status = UpperApp.Core.Result.ResStatus.Error });
                return;
            }

            byte[] buffer = _encoding.GetBytes(data);
            try
            {
                _serialClient.SendAsync(buffer.AsMemory()).GetAwaiter().GetResult();
                OnStatusChanged(new UpperApp.Core.Result(UpperApp.Core.Result.NETStatus.SendMessage, data, buffer.Length, "COM") { Status = UpperApp.Core.Result.ResStatus.SetNum });
            }
            catch (Exception ex)
            {
                OnStatusChanged(new UpperApp.Core.Result(UpperApp.Core.Result.NETStatus.ExceptionStop, $"串口写入失败: {ex.Message}"));
            }
        }

        public IReadOnlyList<string> GetPeerList() => [];

        public ValueTask DisposeAsync()
        {
            Stop();
            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }

        private void OnStatusChanged(UpperApp.Core.Result result)
        {
            if (result.Channel == ChannelType.Unknown)
                result = result with { Channel = ChannelType.Serial };
            if (result.NetStatus == UpperApp.Core.Result.NETStatus.ExceptionStop)
                State = DeviceState.Error;
            StatusChanged?.Invoke(result);
        }
    }
}
