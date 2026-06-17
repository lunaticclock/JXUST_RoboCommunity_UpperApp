using System;
using System.Text;
using UpperApp.Core;
using UpperApp.Services;

namespace UpperApp.Processing
{
    internal class MessageProcessor
    {
        private readonly ILogger _logger;
        private readonly Func<bool> _logEnabled;

        public MessageProcessor(ILogger logger, Func<bool> logEnabled = null)
        {
            _logger = logger;
            _logEnabled = logEnabled ?? (() => true);
        }

        public ProcessedMessage ProcessReceivedMessage(MessageReceivedEvent evt)
        {
            string prefix = "";
            string rawContent = evt.Content;
            string newPeerHint = evt.PeerHint ?? "";

            if (!string.IsNullOrEmpty(evt.Source))
            {
                prefix = $"from {evt.Source}:\r\n";
            }

            // 统一通过 ProtocolHandler 解析姿态数据，避免 Contains + TryParse 重复检测
            var parsed = ProtocolHandler.TryParse(rawContent);
            bool hasAttitude = parsed.HasValue;
            string attitudeRaw = hasAttitude ? rawContent : "";

            string formatted = rawContent;
            if (!rawContent.EndsWith("\r\n") && !rawContent.EndsWith("\n"))
                formatted += "\r\n";

            // 日志受 SaveDataEnabled 开关控制，避免无条件写入
            if (_logEnabled() && !string.IsNullOrEmpty(rawContent))
            {
                _logger.WriteLine($"[{Utils.GetTime()}] RECV [{evt.Channel}]: {rawContent.TrimEnd()}");
            }

            return new ProcessedMessage
            {
                Prefix = prefix,
                FormattedContent = formatted,
                RawContent = rawContent,
                Source = evt.Source ?? "",
                ByteCount = evt.ByteCount,
                NewPeerHint = newPeerHint,
                HasAttitudeData = hasAttitude,
                AttitudeRaw = attitudeRaw
            };
        }
    }
}
