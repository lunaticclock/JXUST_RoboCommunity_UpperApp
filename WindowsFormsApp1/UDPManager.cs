using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Windows.UI.StartScreen;

namespace UpperApp
{
    class UDPManager
    {
        private UdpClient UdpClientSend = null;
        private UdpClient UdpClientReceive = null;
        IPEndPoint remoteIpAndPort = new IPEndPoint(IPAddress.Any, 0);
        string receiveFromOld = "";
        private BindingList<string> UDPlist = new();
        private Encoding encoding = null;
        public event Action<Result> StatusChanged;

        public UDPManager()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            encoding = Encoding.GetEncoding("GB2312");
        }

        public void UDP_Send(string Buf, string peer)
        {
            Result result = new Result();
            //远端 IP                
            int RemotePort;      //远端 Port
            IPEndPoint RemoteIPEndPoint; //远端 IP&Port
            int num = peer.IndexOf(':');
            result.NetStatus = Result.NETStatus.SendMessage;
            if (IPAddress.TryParse(peer.Substring(0, num), out IPAddress RemoteIP) == false)//远端 IP
            {
                result.status = Result.ResStatus.Error;
                result.Message = "远端IP错误!";
                OnStatusChanged(result);
                return;
            }

            RemotePort = int.Parse(peer.Remove(0, num + 1));//远端 Port
            RemoteIPEndPoint = new IPEndPoint(RemoteIP, RemotePort);//远端 IP和Port
            //Get Data
            byte[] sendBytes = encoding.GetBytes(Buf);
            int cnt = sendBytes.Length;

            if (0 == cnt)
            {
                result.status = Result.ResStatus.Alert;
                result.Message = "未发出信息!";
                OnStatusChanged(result);
                return;
            }

            try
            {
                //Send
                UdpClientSend.Send(sendBytes, cnt, RemoteIPEndPoint);
            }
            catch(SocketException)
            {
                if(UDPlist.Contains(RemoteIPEndPoint.ToString()))
                    UDPlist.Remove(RemoteIPEndPoint.ToString());
                OnStatusChanged(new Result(Result.NETStatus.RemoteStop, "远端关闭"));
            }

            result.status = Result.ResStatus.SetNum;
            result.Num = cnt;
            result.Message = Buf;
            OnStatusChanged(result);
        }

        public BindingList<string> GetUDPPeer()
        {
            return UDPlist;
        }

        public void CloseUDP()
        {
            //UDPMonitor.CancelAsync();
            UdpClientSend.Close();
            UdpClientReceive.Close();
            OnStatusChanged(new Result(Result.NETStatus.MonitorStop, "停止监听"));
        }

        public void StartUDP(IPEndPoint LocalIPEndPoint)
        {
            UDPlist.Clear();
            //Bind
            UdpClientSend = new UdpClient(LocalIPEndPoint.Port);//Bind Send UDP = Local some IP&Port
            UdpClientReceive = new UdpClient(LocalIPEndPoint);//Bind Receive UDP = Local IP&Port
            UdpClientReceive.BeginReceive(new AsyncCallback(ReceiveCallback), null);
            OnStatusChanged(new Result(Result.NETStatus.MonitorStart, "监听开始"));
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                byte[] ReceiveBytes = UdpClientReceive.EndReceive(ar, ref remoteIpAndPort);
                int cnt = ReceiveBytes.Length;
                if (encoding == null)
                {
                    encoding = Encoding.GetEncoding("GB2312");
                }
                string Message = encoding.GetString(ReceiveBytes, 0, cnt) + "\r\n";
                Result result = new Result(Result.NETStatus.ReciveMessage, Message, cnt, remoteIpAndPort.ToString());
                result.IPPort = remoteIpAndPort.ToString();
                if (!UDPlist.Contains(result.IPPort))
                {
                    UDPlist.Add(result.IPPort);
                }
                if (!result.IPPort.Equals(receiveFromOld))
                {
                    result.NewPeer = string.Format("\r\nfrom {0}:\r\n", result.IPPort);
                }
                result.Num = cnt;
                receiveFromOld = result.IPPort;
                OnStatusChanged(result);
                // 继续监听
                UdpClientReceive.BeginReceive(new AsyncCallback(ReceiveCallback), null);
            }
            catch (ObjectDisposedException)
            {
                // Socket 已关闭，忽略此异常
            }
            catch (Exception ex)
            {
                OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, ex.Message));
                //Console.WriteLine($"Exception: {ex.Message}");
            }
        }

        protected virtual void OnStatusChanged(Result status)
        {
            StatusChanged?.Invoke(status);
        }

        private void Monitor_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            OnStatusChanged(new Result(Result.NETStatus.MonitorStop, "监听停止"));
        }
    }
}
