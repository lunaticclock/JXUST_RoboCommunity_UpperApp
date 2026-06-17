namespace UpperApp.Core
{
    internal static class ProtocolFormatter
    {
        private const string OverSuffix = "\r\n";

        public static string ForwardBackward(int value)
        {
            return $"FB:{value}:OVER{OverSuffix}";
        }

        public static string RightLeft(int value)
        {
            return $"RL:{value}:OVER{OverSuffix}";
        }

        public static string FullControl(int speed, int direction)
        {
            return $"FR:{speed}:{direction}:OVER{OverSuffix}";
        }
    }
}
