using UpperApp.Core;

namespace UpperApp.Services
{
    internal class ProtocolHandler
    {
        public enum DataType
        {
            Unknown,
            Yaw,
            Pitch,
            Roll,
            Distance
        }

        public readonly struct ParsedData
        {
            public DataType Type { get; init; }
            public string Key { get; init; }
            public string Value { get; init; }
        }

        public static ParsedData? TryParse(string input)
        {
            if (string.IsNullOrEmpty(input) || !input.Contains("/OVER"))
                return null;

            int colonIndex = input.IndexOf(':');
            if (colonIndex < 0) return null;

            int slashIndex = input.IndexOf('/', colonIndex);
            if (slashIndex < 0) return null;

            string key = input[..colonIndex];
            string value = input[(colonIndex + 1)..slashIndex];

            var type = key switch
            {
                "YAW" => DataType.Yaw,
                "PITCH" => DataType.Pitch,
                "ROLL" => DataType.Roll,
                "DISTANCE" => DataType.Distance,
                _ => DataType.Unknown
            };

            if (type == DataType.Unknown) return null;

            return new ParsedData { Type = type, Key = key, Value = value };
        }

        public static string EncodeMove(int speed, int direction)
        {
            return ProtocolFormatter.FullControl(speed, direction);
        }

        public static string EncodeForwardBackward(int value)
        {
            return ProtocolFormatter.ForwardBackward(value);
        }

        public static string EncodeRightLeft(int value)
        {
            return ProtocolFormatter.RightLeft(value);
        }
    }
}
