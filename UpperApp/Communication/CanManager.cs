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
    internal class CANManager : CommunicatorBase
    {
        private PcanChannel _pcanChannel;
        private readonly BindingDic<string> _canDevices = new();
        private CancellationTokenSource _cts;
        private bool _isMonitoring;
        private readonly Encoding _encoding = Encoding.GetEncoding("GB2312");

        public override ChannelType Channel => ChannelType.CAN;

        static CANManager()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public override void Start(CommunicationParams parameters)
        {
            if (parameters is not CanParams canParams)
                throw new ArgumentException("参数类型必须为 CanParams");

            Stop();
            State = DeviceState.Connecting;
            _cts = new CancellationTokenSource();

            if (!Enum.TryParse(canParams.ChannelName, true, out _pcanChannel))
            {
                NotifyException($"无效的 CAN 通道: {canParams.ChannelName}");
                Stop();
                return;
            }

            var status = Api.Initialize(_pcanChannel, Bitrate.Pcan500);
            if (status != PcanStatus.OK)
            {
                NotifyException($"CAN 初始化失败: {GetErrorMessage(status)}");
                Stop();
                return;
            }

            _ = ReceiveLoopAsync(_cts.Token);
            NotifyMonitorStarted($"CAN 通道 {canParams.ChannelName} 启动");
            _isMonitoring = true;
        }

        public override void Stop()
        {
            if (IsStopping) return;
            if (!_isMonitoring && State == DeviceState.Disconnected) return;

            BeginStop();
            _isMonitoring = false;
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

            if (_pcanChannel != 0)
            {
                try { Api.Uninitialize(_pcanChannel); } catch { }
            }

            NotifyMonitorStopped("CAN 已停止");
            EndStop();
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
                            NotifyMessageReceived(message, (int)msg.Length, canId);
                        }
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                if (!IsStopping)
                    NotifyException($"接收循环异常: {ex.Message}");
            }
            finally
            {
                if (_isMonitoring && !IsStopping) Stop();
            }
        }

        public override void Send(string data, string target = null)
        {
            if (string.IsNullOrWhiteSpace(data))
                return;

            string[] parts = data.Split(':');
            if (parts.Length != 2)
            {
                NotifyMessageSendError("CAN 发送格式错误，应为 ID:数据");
                return;
            }

            if (!uint.TryParse(parts[0], System.Globalization.NumberStyles.HexNumber, null, out uint id))
            {
                NotifyMessageSendError($"无效的 CAN ID: {parts[0]}");
                return;
            }

            string hexData = parts[1];
            if (hexData.Length % 2 != 0)
            {
                NotifyMessageSendError("CAN 数据长度必须为偶数");
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
                NotifyMessageSent(data, dataBytes.Length, id.ToString());
            }
            else
            {
                NotifyException($"CAN 发送失败: {GetErrorMessage(status)}", id.ToString());
            }
        }

        private static string GetErrorMessage(PcanStatus status)
        {
            Api.GetErrorText(status, out string errText);
            return errText;
        }

        public override IReadOnlyList<string> GetPeerList() => _canDevices.connectionKeys;
    }
}
