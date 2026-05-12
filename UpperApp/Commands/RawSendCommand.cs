using UpperApp.Core;

namespace UpperApp.Commands
{
    internal class RawSendCommand : IDeviceCommand
    {
        public string Name => "RawSend";
        public ChannelType TargetChannel { get; }
        public string RawData { get; }

        public RawSendCommand(string rawData, ChannelType channel)
        {
            RawData = rawData;
            TargetChannel = channel;
        }

        public string Encode() => RawData;
    }
}
