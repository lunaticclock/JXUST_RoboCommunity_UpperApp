namespace UpperApp.UI
{
    internal static class ProtocolParser
    {
        public static bool TryParseAngleData(string input, out string key, out string value)
        {
            key = null;
            value = null;

            if (string.IsNullOrEmpty(input) || !input.Contains("/OVER"))
                return false;

            int colonIndex = input.IndexOf(':');
            if (colonIndex < 0) return false;

            int slashIndex = input.IndexOf('/', colonIndex);
            if (slashIndex < 0) return false;

            key = input[..colonIndex];
            value = input[(colonIndex + 1)..slashIndex];

            return key is "YAW" or "PITCH" or "ROLL" or "DISTANCE";
        }
    }
}
