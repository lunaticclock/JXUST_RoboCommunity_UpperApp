using UpperApp.Core;

namespace UpperApp.Commands
{
    internal interface IDeviceCommand
    {
        string Name { get; }
        ChannelType TargetChannel { get; }
        string Encode();
    }
}
