namespace UpperApp.Core
{
    internal record Result
    {
        public enum ResStatus
        {
            Success,
            Error,
            Alert,
            SetNum
        }

        public enum NETStatus
        {
            ManualStop,
            ExceptionStop,
            RemoteStop,
            ReciveMessage,
            SendMessage,
            MonitorStop,
            MonitorStart,
            NewRemote
        }

        public string Message { get; init; }
        public int Num { get; init; }
        public ResStatus Status { get; init; }
        public NETStatus NetStatus { get; init; }
        public string RemoteIP { get; init; }
        public string IPPort { get; init; }
        public string NewPeer { get; init; }
        public ChannelType Channel { get; init; }

        public Result() { }

        public Result(NETStatus status, string message)
        {
            NetStatus = status;
            Message = message;
        }

        public Result(NETStatus status, string message, int num)
        {
            NetStatus = status;
            Message = message;
            Num = num;
        }

        public Result(NETStatus status, string message, int num, string remoteIP)
        {
            NetStatus = status;
            Message = message;
            Num = num;
            RemoteIP = remoteIP;
        }
    }
}
