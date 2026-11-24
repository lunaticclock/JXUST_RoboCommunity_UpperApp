using Peak.Can.Basic;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
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

        public override ChannelType Channel => ChannelType.CAN;

        protected override string StoppedMessage => "CAN 已停止";

        protected override void StopCore()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

            if (_pcanChannel != 0)
            {
                try { Api.Uninitialize(_pcanChannel); } catch { }
            }
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
                if (IsMonitoring && !IsStopping) Stop();
            }
        }

        public override void Send(string data, string target = null)
        {
            if (string.IsNullOrWhiteSpace(data))
                return;

            int colonIndex = data.IndexOf(':');
            if (colonIndex < 0 || colonIndex != data.LastIndexOf(':'))
            {
                NotifyMessageSendError("CAN 发送格式错误，应为 ID:数据");
                return;
            }

            string idText = data[..colonIndex];
            if (!uint.TryParse(idText, System.Globalization.NumberStyles.HexNumber, null, out uint id))
            {
                NotifyMessageSendError($"无效的 CAN ID: {idText}");
                return;
            }

            string hexData = data[(colonIndex + 1)..];
            if (hexData.Length % 2 != 0)
            {
                NotifyMessageSendError("CAN 数据长度必须为偶数");
                return;
            }

            byte[] dataBytes = Convert.FromHexString(hexData);

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

        /// <summary>
        /// CAN 通道不支持原始字节直发（需 ID:数据 格式），Hex 模式不适用于 CAN。
        /// </summary>
        public override void Send(byte[] data, string target = null)
        {
            NotifyMessageSendError("CAN 通道不支持原始字节发送，请使用 ID:hex数据 格式");
        }

        private static string GetErrorMessage(PcanStatus status)
        {
            Api.GetErrorText(status, out string errText);
            return errText;
        }

        public override IReadOnlyList<string> GetPeerList() => _canDevices.Keys;
    }
}
