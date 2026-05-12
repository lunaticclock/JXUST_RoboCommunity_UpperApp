using System;
using System.IO;
using System.Runtime.Versioning;
using UpperApp.Core;
using UpperApp.Services;

namespace UpperApp.Processing
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    internal class MessageProcessor
    {
        private readonly IDisplayAdapter _display;
        private readonly ILogger _logger;
        private readonly FileLogger _dataSaver;
        private const string FromPrefix = "from:";
        private const string ToPrefix = "to:";
        private const string NewLine = "\r\n";

        public MessageProcessor(IDisplayAdapter display, ILogger logger)
        {
            _display = display;
            _logger = logger;
            _dataSaver = new FileLogger();
            _dataSaver.Open(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data.txt"));
        }

        public void ProcessReceivedMessage(Result status)
        {
            _display.UpdateByteCount(status.Num, RecvOrSend.Recv);

            if (_display.IsAngleDisplayEnabled)
            {
                var parsed = ProtocolHandler.TryParse(status.Message);
                if (parsed.HasValue)
                    _display.UpdateAngleDisplay($"{parsed.Value.Key}:{parsed.Value.Value}/OVER");
            }

            if (!string.IsNullOrWhiteSpace(status.NewPeer))
                _display.OnNewPeer(status.NewPeer);

            string remote = status.RemoteIP ?? "BlueTooth";
            string prefix = $"{FromPrefix}{remote}: {NewLine}";

            if (_display.IsCharMode)
            {
                AppendAndLog(prefix, status.Message);
            }
            else if (_display.IsHexMode)
            {
                AppendAndLog(prefix, status.Message, isHex: true);
            }
        }

        public void ProcessSentMessage(Result status)
        {
            if (status.NetStatus != Result.NETStatus.SendMessage) return;

            if (status.Status == Result.ResStatus.SetNum)
            {
                string remote = status.RemoteIP ?? "BlueTooth";
                string prefix = $"{ToPrefix}{remote}: {NewLine}";
                if (_display.IsLocalEchoEnabled) AppendAndLog(prefix, status.Message);
                else _logger.WriteLine($"{Utils.GetTime()}{prefix}{status.Message}{NewLine}");

                _display.UpdateByteCount(status.Num, RecvOrSend.Send);
            }
            else if (status.Status == Result.ResStatus.Error)
            {
                _display.AppendToReceiveBox($"发送错误: {status.Message}\r\n");
            }
        }

        public void ProcessException(Result status)
        {
            if (status.NetStatus == Result.NETStatus.ExceptionStop)
            {
                _display.AppendToReceiveBox($"异常: {status.Message}\r\n");
            }
        }

        private void AppendAndLog(string prefix, string content, bool isHex = false)
        {
            string time = Utils.GetTime();
            string displayContent = isHex ? Utils.StringToHexString(content) : content;
            string formatted = $"{time}{prefix}{displayContent}{NewLine}";
            _display.AppendToReceiveBox(formatted);
            _logger.WriteLine(formatted);
            if (_display.IsSaveDataEnabled)
                _dataSaver.WriteLine(formatted.TrimEnd());
        }
    }
}
