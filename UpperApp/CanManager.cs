using Peak.Can.Basic;
using Peak.Can.Basic.BackwardCompatibility;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace UpperApp
{
    class CANManager : BaseCommunicationManager
    {
        // 使用新 API 中的强类型枚举
        private PcanChannel pcan_channel;
        private readonly BindingDic<string> _canDevices = new();

        public CANManager() : base(ChannelType.CAN) { }

        // 启动 CAN 适配器，参数为通道字符串（如 "PCAN_USBBUS1"）
        public void StartMonitor(string channelName = "PCAN_USBBUS1")
        {
            StartCore();
            if (!Enum.TryParse(channelName, true, out pcan_channel))
            {
                OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, $"无效的 CAN 通道: {channelName}"));
                return;
            }

            // 使用新 API 初始化
            var status = Api.Initialize(pcan_channel, Bitrate.Pcan500);
            if (status != PcanStatus.OK)
            {
                OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, $"CAN 初始化失败: {GetErrorMessage(status)}"));
                return;
            }

            // 启动接收任务
            _ = ReceiveLoopAsync(_cts.Token);
            OnStatusChanged(new Result(Result.NETStatus.MonitorStart, $"CAN 通道 {channelName} 启动"));
        }

        protected override void OnStopping()
        {
            if (_channel != 0)
            {
                Api.Uninitialize(pcan_channel);
            }
        }

        private async Task ReceiveLoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    Api.Read(pcan_channel, out PcanMessage msg);

                    string canId = msg.ID.ToString("X");
                    // 使用新 API 的 DataBytes 类型来获取数据
                    string data = Convert.ToHexString(msg.Data);
                    string message = $"{canId}:{data}";
                    OnStatusChanged(new Result(Result.NETStatus.ReciveMessage, message, (int)msg.Length, canId));
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, $"接收循环异常: {ex.Message}"));
            }
            finally
            {
                if (_isMonitoring) StopMonitor();
            }
        }

        public override void Send(string data, string target = null)
        {
            // data 格式应为 "ID:数据"，例如 "123:AABBCC"
            if (string.IsNullOrWhiteSpace(data))
                return;

            string[] parts = data.Split(':');
            if (parts.Length != 2)
            {
                OnStatusChanged(new Result(Result.NETStatus.SendMessage, "CAN 发送格式错误，应为 ID:数据", 0) { status = Result.ResStatus.Error });
                return;
            }

            if (!uint.TryParse(parts[0], System.Globalization.NumberStyles.HexNumber, null, out uint id))
            {
                OnStatusChanged(new Result(Result.NETStatus.SendMessage, $"无效的 CAN ID: {parts[0]}", 0) { status = Result.ResStatus.Error });
                return;
            }

            string hexData = parts[1];
            if (hexData.Length % 2 != 0)
            {
                OnStatusChanged(new Result(Result.NETStatus.SendMessage, "CAN 数据长度必须为偶数", 0) { status = Result.ResStatus.Error });
                return;
            }

            byte[] dataBytes = new byte[hexData.Length / 2];
            for (int i = 0; i < dataBytes.Length; i++)
                dataBytes[i] = Convert.ToByte(hexData.Substring(i * 2, 2), 16);

            // 使用新 API 发送消息
            PcanMessage msg = new()
            {
                ID = id,
                MsgType = MessageType.Standard,
                Data = new DataBytes(dataBytes)
            };

            PcanStatus status = Api.Write(pcan_channel, msg);
            if (status == PcanStatus.OK)
            {
                Result rs = new(Result.NETStatus.SendMessage, data, dataBytes.Length, id.ToString())
                {
                    status = Result.ResStatus.SetNum
                };
                OnStatusChanged(rs);
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

        public BindingList<string> GetPeer() => _canDevices.connectionKeys;
    }
}