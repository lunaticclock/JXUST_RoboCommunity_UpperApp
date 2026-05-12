using Peak.Can.Basic;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UpperApp.Core;

namespace UpperApp.Communication
{
    internal class CANManager : BaseCommunicationManager
    {
        private PcanChannel _pcanChannel;
        private readonly BindingDic<string> _canDevices = new();

        public CANManager() : base(ChannelType.CAN) { }

        public override void Start(CommunicationParams parameters)
        {
            if (parameters is not CanParams canParams)
                throw new ArgumentException("参数类型必须为 CanParams");

            StartCore();

            if (!Enum.TryParse(canParams.ChannelName, true, out _pcanChannel))
            {
                OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, $"无效的 CAN 通道: {canParams.ChannelName}"));
                Stop();
                return;
            }

            // 使用新 API 初始化
            var status = Api.Initialize(_pcanChannel, Bitrate.Pcan500);
            if (status != PcanStatus.OK)
            {
                OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, $"CAN 初始化失败: {GetErrorMessage(status)}"));
                Stop();
                return;
            }

            // 启动接收任务
            _ = ReceiveLoopAsync(_cts.Token);
            OnStatusChanged(new Result(Result.NETStatus.MonitorStart, $"CAN 通道 {canParams.ChannelName} 启动"));
        }

        protected override void OnStopping()
        {
            if (_pcanChannel != 0)
            {
                try { Api.Uninitialize(_pcanChannel); } catch { }
            }
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
                OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, $"接收循环异常: {ex.Message}"));
            }
            finally
            {
                if (_isMonitoring) Stop();
            }
        }

        public override void Send(string data, string target = null)
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

        public override IReadOnlyList<string> GetPeerList() => _canDevices.connectionKeys;
    }
}