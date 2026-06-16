using Peak.Can.Basic;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UpperApp.Core;
using UpperApp.Services;

namespace UpperApp.Communication
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    internal class CANManager : ICommunicator
    {
        private PcanChannel _pcanChannel;
        private readonly BindingDic<string> _canDevices = new();
        private CancellationTokenSource _cts;
        private bool _isMonitoring;
        private bool _isStopping;
        private DeviceState _state = DeviceState.Disconnected;
        private readonly Encoding _encoding = Encoding.GetEncoding("GB2312");

        public event Action<Result> StatusChanged;
        public ChannelType Channel => ChannelType.CAN;

        public DeviceState State
        {
            get => _state;
            private set
            {
                if (_state != value)
                    _state = value;
            }
        }

        static CANManager()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void Start(CommunicationParams parameters)
        {
            if (parameters is not CanParams canParams)
                throw new ArgumentException("参数类型必须为 CanParams");

            Stop();
            State = DeviceState.Connecting;
            _cts = new CancellationTokenSource();
            _isStopping = false;

            if (!Enum.TryParse(canParams.ChannelName, true, out _pcanChannel))
            {
                OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, $"无效的 CAN 通道: {canParams.ChannelName}"));
                Stop();
                return;
            }

            var status = Api.Initialize(_pcanChannel, Bitrate.Pcan500);
            if (status != PcanStatus.OK)
            {
                OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, $"CAN 初始化失败: {GetErrorMessage(status)}"));
                Stop();
                return;
            }

            _ = ReceiveLoopAsync(_cts.Token);
            State = DeviceState.Connected;
            OnStatusChanged(new Result(Result.NETStatus.MonitorStart, $"CAN 通道 {canParams.ChannelName} 启动"));
            _isMonitoring = true;
        }

        public void Stop()
        {
            if (!_isMonitoring && _state == DeviceState.Disconnected) return;

            _isStopping = true;
            State = DeviceState.Disconnecting;
            _isMonitoring = false;
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

            if (_pcanChannel != 0)
            {
                try { Api.Uninitialize(_pcanChannel); } catch { }
            }

            OnStatusChanged(new Result(Result.NETStatus.MonitorStop, "CAN 已停止"));
            State = DeviceState.Disconnected;
            _isStopping = false;
        }

        private async Task ReceiveLoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var readTask = Task.Run(() =>
                    {
                        PcanStatus status = Api.Read(_pcanChannel, out PcanMessage msg);
                        return (status, msg);
                    });

                    var completedTask = await Task.WhenAny(readTask, Task.Delay(100, token));
                    if (completedTask == readTask)
                    {
                        (PcanStatus status, PcanMessage msg) = await readTask;
                        if (status == PcanStatus.OK)
                        {
                            string canId = msg.ID.ToString("X");
                            string data = Convert.ToHexString(msg.Data);
                            string message = $"{canId}:{data}";
                            OnStatusChanged(new Result(Result.NETStatus.ReciveMessage, message, (int)msg.Length, canId));
                        }
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                if (!_isStopping)
                    OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, $"接收循环异常: {ex.Message}"));
            }
            finally
            {
                if (_isMonitoring && !_isStopping) Stop();
            }
        }

        public void Send(string data, string target = null)
        {
            if (string.IsNullOrWhiteSpace(data))
                return;

            string[] parts = data.Split(':');
            if (parts.Length != 2)
            {
                OnStatusChanged(new Result(Result.NETStatus.SendMessage, "CAN 发送格式错误，应为 ID:数据", 0) with { Status = Result.ResStatus.Error });
                return;
            }

            if (!uint.TryParse(parts[0], System.Globalization.NumberStyles.HexNumber, null, out uint id))
            {
                OnStatusChanged(new Result(Result.NETStatus.SendMessage, $"无效的 CAN ID: {parts[0]}", 0) with { Status = Result.ResStatus.Error });
                return;
            }

            string hexData = parts[1];
            if (hexData.Length % 2 != 0)
            {
                OnStatusChanged(new Result(Result.NETStatus.SendMessage, "CAN 数据长度必须为偶数", 0) with { Status = Result.ResStatus.Error });
                return;
            }

            byte[] dataBytes = new byte[hexData.Length / 2];
            for (int i = 0; i < dataBytes.Length; i++)
                dataBytes[i] = Convert.ToByte(hexData.Substring(i * 2, 2), 16);

            var msg = new PcanMessage
            {
                ID = id,
                MsgType = MessageType.Standard,
                Data = new DataBytes(dataBytes)
            };

            PcanStatus status = Api.Write(_pcanChannel, msg);
            if (status == PcanStatus.OK)
            {
                var result = new Result(Result.NETStatus.SendMessage, data, dataBytes.Length, id.ToString())
                    with { Status = Result.ResStatus.SetNum };
                OnStatusChanged(result);
            }
            else
            {
                OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, $"CAN 发送失败: {GetErrorMessage(status)}", 0, id.ToString()));
            }
        }

        private static string GetErrorMessage(PcanStatus status)
        {
            Api.GetErrorText(status, out string errText);
            return errText;
        }

        public IReadOnlyList<string> GetPeerList() => _canDevices.connectionKeys;

        public ValueTask DisposeAsync()
        {
            Stop();
            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }

        private void OnStatusChanged(Result result)
        {
            if (result.Channel == ChannelType.Unknown)
                result = result with { Channel = ChannelType.CAN };
            if (result.NetStatus == Result.NETStatus.ExceptionStop)
                State = DeviceState.Error;
            StatusChanged?.Invoke(result);
        }
    }
}
