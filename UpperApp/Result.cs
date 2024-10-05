

namespace UpperApp
{
    class Result
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
        public string Message { get; set; }
        public int Num { get; set; } = 0;
        public ResStatus status { get; set; }
        public NETStatus NetStatus { get; set; }
        public string RemoteIP { get; set; }
        public string IPPort { get; set; }
        public string NewPeer { get; set; }
        public Result(){}

        public Result(NETStatus status, string Mes)
        {
            NetStatus = status;
            Message = Mes;
        }
        public Result(NETStatus status, string Mes, int MesLen)
        {
            NetStatus = status;
            Message = Mes;
            Num = MesLen;
        }
        public Result(NETStatus status, string Mes, int MesLen, string Remote)
        {
            NetStatus = status;
            Message = Mes;
            Num = MesLen;
            RemoteIP = Remote;
        }
    }
}
