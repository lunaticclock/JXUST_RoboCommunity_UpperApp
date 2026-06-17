using UpperApp.Core;

namespace UpperApp.Commands
{
    internal class MoveCommand : IDeviceCommand
    {
        public string Name => "Move";
        public ChannelType TargetChannel { get; }
        public int Speed { get; }
        public int Direction { get; }
        public MoveType Type { get; }

        public enum MoveType
        {
            ForwardBackward,
            RightLeft,
            FullControl
        }

        public MoveCommand(MoveType type, int speed, int direction = 50, ChannelType channel = ChannelType.Unknown)
        {
            Type = type;
            Speed = speed;
            Direction = direction;
            TargetChannel = channel;
        }

        public string Encode() => Type switch
        {
            MoveType.ForwardBackward => ProtocolFormatter.ForwardBackward(Speed),
            MoveType.RightLeft => ProtocolFormatter.RightLeft(Direction),
            MoveType.FullControl => ProtocolFormatter.FullControl(Speed, Direction),
            _ => ""
        };
    }
}
