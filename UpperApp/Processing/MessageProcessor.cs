using System.Text;
using UpperApp.Core;
using UpperApp.Services;

namespace UpperApp.Processing
{
    internal class MessageProcessor
    {
        private readonly ILogger _logger;

        public MessageProcessor(ILogger logger)
        {
            _logger = logger;
        }

        public ProcessedMessage ProcessReceivedMessage(Result result)
        {
            if (result.NetStatus != Result.NETStatus.ReciveMessage)
                return null;

            string prefix = "";
            string rawContent = result.Message;
            string newPeerHint = result.NewPeer ?? "";

            if (!string.IsNullOrEmpty(result.IPPort))
            {
                prefix = $"from {result.IPPort}:\r\n";
            }

            bool hasAttitude = !string.IsNullOrEmpty(rawContent) && rawContent.Contains("/OVER");
            string attitudeRaw = hasAttitude ? rawContent : "";

            string formatted = rawContent;
            if (!rawContent.EndsWith("\r\n") && !rawContent.EndsWith("\n"))
                formatted += "\r\n";

            if (!string.IsNullOrEmpty(rawContent))
            {
                _logger.WriteLine($"[{Utils.GetTime()}] RECV [{result.Channel}]: {rawContent.TrimEnd()}");
            }

            return new ProcessedMessage
            {
                Prefix = prefix,
                FormattedContent = formatted,
                RawContent = rawContent,
                Source = result.RemoteIP ?? result.IPPort ?? "",
                ByteCount = result.Num,
                NewPeerHint = newPeerHint,
                HasAttitudeData = hasAttitude,
                AttitudeRaw = attitudeRaw
            };
        }

        public ProcessedMessage ProcessSentMessage(Result result)
        {
            if (result.NetStatus != Result.NETStatus.SendMessage)
                return null;

            string prefix = "";
            if (!string.IsNullOrEmpty(result.RemoteIP))
                prefix = $"to {result.RemoteIP}:\r\n";

            string rawContent = result.Message ?? "";
            string formatted = rawContent;
            if (!rawContent.EndsWith("\r\n") && !rawContent.EndsWith("\n"))
                formatted += "\r\n";

            _logger.WriteLine($"[{Utils.GetTime()}] SEND [{result.Channel}]: {rawContent.TrimEnd()}");

            return new ProcessedMessage
            {
                Prefix = prefix,
                FormattedContent = formatted,
                RawContent = rawContent,
                Source = result.RemoteIP ?? "",
                ByteCount = result.Num,
                NewPeerHint = "",
                HasAttitudeData = false,
                AttitudeRaw = ""
            };
        }
    }
}
