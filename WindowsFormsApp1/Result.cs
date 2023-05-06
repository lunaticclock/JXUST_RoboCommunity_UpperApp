using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public string Message { get; set; }
        public int Num { get; set; } = 0;
        public ResStatus status { get; set; }
    }

    class UDPResult: Result
    {
        public string IPPort { get; set; }
        public string NewPeer { get; set; }
    }
}
