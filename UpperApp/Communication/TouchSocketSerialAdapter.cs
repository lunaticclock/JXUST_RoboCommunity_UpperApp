using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using TouchSocket.Core;
using TouchSocket.SerialPorts;
using UpperApp.Core;
using UpperApp.Services;

namespace UpperApp.Communication
{
    internal class TouchSocketSerialAdapter : CommunicatorBase
    {
        private SerialPortClient _serialClient;

        public override ChannelType Channel => ChannelType.Serial;

        protected override string StoppedMessage => "串口已停止";

        public override void Start(CommunicationParams parameters)
        {
            if (parameters is not SerialParams serialParams)
                throw new ArgumentException("参数类型必须为 SerialParams");

            Stop();
            State = DeviceState.Connecting;

            _serialClient = new SerialPortClient();

            _serialClient.Received = (client, e) =>
            {
                var receivedData = e.Memory.Span.ToString(Utils.TextEncoding);
                NotifyMessageReceived(receivedData, e.Memory.Length, "COM");
                return EasyTask.CompletedTask;
            };

            _serialClient.Closed = (client, e) =>
            {
                if (!IsStopping)
                    NotifyException("串口异常断开");
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
                NotifyMonitorStarted($"串口 {serialParams.PortName} 已打开");
            }
            catch (Exception ex)
            {
                NotifyException(ex.Message);
                _serialClient?.SafeDispose();
                _serialClient = null;
            }
        }

        protected override void StopCore()
        {
            _serialClient?.CloseAsync("").GetAwaiter().GetResult();
            _serialClient?.SafeDispose();
            _serialClient = null;
        }

        public override void Send(string data, string target = null)
        {
            if (!EnsureOpen()) return;
            Transmit(Utils.TextEncoding.GetBytes(data), data);
        }

        public override void Send(byte[] data, string target = null)
        {
            if (!EnsureOpen()) return;
            if (!HasPayload(data)) return;
            Transmit(data, Utils.BytesToHexString(data));
        }

        private bool EnsureOpen()
        {
            if (_serialClient != null && _serialClient.Online) return true;
            NotifyMessageSendError("串口未打开");
            return false;
        }

        private void Transmit(byte[] payload, string display)
            => WriteAndReport(payload, display, "COM",
                p => _serialClient.SendAsync(p.AsMemory()).GetAwaiter().GetResult(),
                ex => NotifyException($"串口写入失败: {ex.Message}"));

        public override IReadOnlyList<string> GetPeerList() => [];
    }
}
