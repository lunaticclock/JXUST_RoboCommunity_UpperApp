using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using UpperApp.Core;

namespace UpperApp.Communication
{
    internal class SerManager : BaseCommunicationManager
    {
        private SerialPort _serialPort;
        public string PortName { get; private set; }
        public int BaudRate { get; private set; }

        public SerManager() : base(ChannelType.Serial) { }

        public override void Start(CommunicationParams parameters)
        {
            if (parameters is not SerialParams serialParams)
                throw new ArgumentException("参数类型必须为 SerialParams");

            StartCore();

            _serialPort = new SerialPort(serialParams.PortName, serialParams.BaudRate,
                                         serialParams.Parity, serialParams.DataBits, serialParams.StopBits)
            {
                Encoding = encoding,
                NewLine = "\r\n",
                WriteTimeout = 1000
            };
            try
            {
                _serialPort.Open();
                _ = ReceiveLoopAsync(_cts.Token);
                PortName = serialParams.PortName;
                BaudRate = serialParams.BaudRate;
            }
            catch (Exception ex)
            {
                OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, ex.Message));
                _serialPort?.Dispose();
                _serialPort = null;
            }
        }

        protected override void OnStopping()
        {
            try { _serialPort?.Close(); } catch { }
            try { _serialPort?.Dispose(); } catch { }
            _serialPort = null;
        }

        private async Task ReceiveLoopAsync(CancellationToken token)
        {
            byte[] buffer = new byte[4096];
            try
            {
                while (!token.IsCancellationRequested && _serialPort != null && _serialPort.IsOpen)
                {
                    int bytesRead = await _serialPort.BaseStream.ReadAsync(buffer, token);
                    if (bytesRead == 0) continue;

                    byte[] receivedBytes = new byte[bytesRead];
                    Array.Copy(buffer, receivedBytes, bytesRead);
                    string receivedData = encoding.GetString(receivedBytes);

                    OnStatusChanged(new Result(Result.NETStatus.ReciveMessage, receivedData, bytesRead, "COM"));
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, ex.Message));
                Stop();
            }
        }

        public override void Send(string data, string target = null)
        {
            if (!_isMonitoring || _serialPort == null || !_serialPort.IsOpen)
            {
                OnStatusChanged(new Result(Result.NETStatus.SendMessage, "串口未打开", 0, "") with { Status = Result.ResStatus.Error });
                return;
            }

            byte[] buffer = encoding.GetBytes(data);
            try
            {
                _serialPort.Write(buffer, 0, buffer.Length);
                OnStatusChanged(new Result(Result.NETStatus.SendMessage, data, buffer.Length, "COM") with { Status = Result.ResStatus.SetNum });
            }
            catch (TimeoutException ex)
            {
                OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, $"串口写入超时: {ex.Message}"));
                Stop();
            }
            catch (Exception ex)
            {
                OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, ex.Message));
            }
        }

        public override IReadOnlyList<string> GetPeerList() => [];
    }
}
