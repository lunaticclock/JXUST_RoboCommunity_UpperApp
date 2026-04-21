using System;
using System.IO.Ports;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.StartScreen;

namespace UpperApp
{
    internal class SerManager : BaseCommunicationManager
    {

        private SerialPort _serialPort;
        public string PortName { get; private set; }
        public int BaudRate { get; private set; }

        public SerManager() : base(ChannelType.Serial)
        {
        }

        // 启动串口（打开并开始接收）
        public void StartMonitor(string portName, int baudRate, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
        {
            StartCore();

            _serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits)
            {
                Encoding = encoding,
                NewLine = "\r\n",
                WriteTimeout = 1000,
            };
            try
            {
                _serialPort.Open();
                // 启动接收循环（异步）
                _ = ReceiveLoopAsync(_cts.Token);
                PortName = portName;
                BaudRate = baudRate;
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
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
                _serialPort.Dispose();
                _serialPort = null;
            }
        }

        private async Task ReceiveLoopAsync(CancellationToken token)
        {
            byte[] buffer = new byte[4096];
            try
            {
                while (!token.IsCancellationRequested && _serialPort != null && _serialPort.IsOpen)
                {
                    int bytesRead = await _serialPort.BaseStream.ReadAsync(buffer, 0, buffer.Length, token);
                    if (bytesRead == 0) continue;

                    byte[] receivedBytes = new byte[bytesRead];
                    Array.Copy(buffer, receivedBytes, bytesRead);
                    string receivedData = encoding.GetString(receivedBytes);

                    // 触发接收事件
                    OnStatusChanged(new Result(Result.NETStatus.ReciveMessage, receivedData, bytesRead, "COM"));
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, ex.Message));
                StopMonitor(); // 异常时自动关闭
            }
        }

        // 发送字符串
        public override void Send(string data, string target = null)
        {
            if (!_isMonitoring || _serialPort == null || !_serialPort.IsOpen)
            {
                OnStatusChanged(new Result(Result.NETStatus.SendMessage, "串口未打开", 0, "") { status = Result.ResStatus.Error });
                return;
            }

            byte[] buffer = encoding.GetBytes(data);
            try
            {
                _serialPort.Write(buffer, 0, buffer.Length);
                OnStatusChanged(new Result(Result.NETStatus.SendMessage, data, buffer.Length, "COM") { status = Result.ResStatus.SetNum });
            }
            catch (TimeoutException ex)
            {
                OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, $"串口写入超时: {ex.Message}"));
                StopMonitor(); // 自动关闭，避免继续卡死
            }
            catch (Exception ex)
            {
                OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, ex.Message));
            }
        }
    }
}
