namespace UpperApp.Processing
{
    internal record ProcessedMessage
    {
        public string Prefix { get; init; } = "";
        public string FormattedContent { get; init; } = "";
        public string RawContent { get; init; } = "";
        public string Source { get; init; } = "";
        public int ByteCount { get; init; }
        public string NewPeerHint { get; init; } = "";
        public bool HasAttitudeData { get; init; }
        public string AttitudeRaw { get; init; } = "";
    }
}
