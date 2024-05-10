using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UpperApp
{
    class UDPManager
    {
        private UdpClient UdpClientSend = null;
        private UdpClient UdpClientReceive = null;
        private List<string> UDPlist = new List<string>();

        public UDPResult UDP_Send(string Buf, string peer)
        {
            UDPResult result = new UDPResult();
            //远端 IP                
            int RemotePort;      //远端 Port
            IPEndPoint RemoteIPEndPoint; //远端 IP&Port
            int num = peer.IndexOf(':');

            if (IPAddress.TryParse(peer.Substring(0, num), out IPAddress RemoteIP) == false)//远端 IP
            {
                result.status = Result.ResStatus.Error;
                result.Message = "远端IP错误!";
                return result;
            }

            RemotePort = int.Parse(peer.Remove(0, num + 1));//远端 Port
            RemoteIPEndPoint = new IPEndPoint(RemoteIP, RemotePort);//远端 IP和Port

            //Get Data
            byte[] sendBytes = Encoding.Default.GetBytes(Buf);
            int cnt = sendBytes.Length;

            if (0 == cnt)
            {
                result.status = Result.ResStatus.Alert;
                result.Message = "未发出信息!";
                return result;
            }

            //Send
            UdpClientSend.Send(sendBytes, cnt, RemoteIPEndPoint);

            result.status = Result.ResStatus.SetNum;
            result.Num = cnt;
            result.Message = Buf;
            return result;
        }

        public string[] GetUDPPeer()
        {
            return UDPlist.ToArray();
        }

        public void CloseUDP()
        {
            UdpClientSend.Close();
            UdpClientReceive.Close();
        }

        public void StartUDP(IPEndPoint LocalIPEndPoint)
        {
            UDPlist.Clear();
            //Bind
            UdpClientSend = new UdpClient(LocalIPEndPoint.Port);//Bind Send UDP = Local some IP&Port
            UdpClientReceive = new UdpClient(LocalIPEndPoint);//Bind Receive UDP = Local IP&Port
        }

        public UDPResult Receive(ref IPEndPoint remoteIpAndPort, string receiveFromOld)
        {
            UDPResult result = new UDPResult();
            byte[] ReceiveBytes = UdpClientReceive.Receive(ref remoteIpAndPort);
            int cnt = ReceiveBytes.Length;
            result.IPPort = remoteIpAndPort.ToString();
            if (UDPlist.IndexOf(result.IPPort) == -1)
            {
                UDPlist.Add(result.IPPort);
            }
            if (!result.IPPort.Equals(receiveFromOld))
            {
                result.NewPeer = string.Format("\r\nfrom {0}:\r\n", result.IPPort);
            }
            result.Num = cnt;
            result.Message = Encoding.Default.GetString(ReceiveBytes, 0, cnt) + "\r\n";
            return result;
        }
    }
}
