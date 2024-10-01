using System;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace UpperApp
{



    class TCPManager
    {
        public Socket socket { get; private set; } = null;
        private Encoding encoding;
        public event Action<Result> StatusChanged;
        public readonly BindingDic<Socket> TCPdic = new();

        private class StateObject
        {
            public Socket WorkSocket { get; set; }
            public byte[] Buffer { get; set; }
        }

        public TCPManager() 
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            encoding = Encoding.GetEncoding("GB2312");
        }

        public void SetMonitorSocket(Socket s)
        {
            socket = s;
            BeginMonitor();
        }

        public void BeginMonitor()
        {
            socket.BeginAccept(new AsyncCallback(AcceptCallback), null);
            OnStatusChanged(new Result(Result.NETStatus.MonitorStart, "监听开始"));
        }

        public void StopMonitor()
        {
            socket.Close();
            OnStatusChanged(new Result(Result.NETStatus.MonitorStop, "停止监听"));
            string[] cks = [.. TCPdic.connectionKeys];
            foreach (string b in cks)
            {
                TCPdic.Remove(b).Close();
            }
        }

        public void Send(string ip, string Buf)
        {
            byte[] buffer = encoding.GetBytes(Buf);
            TCPdic[ip].Send(buffer);
            Result rs = new Result(Result.NETStatus.SendMessage, Buf, buffer.Length, ip);
            rs.status = Result.ResStatus.SetNum;
            OnStatusChanged(rs);
        }

        public void Remove(string peer)
        {
            TCPdic.Remove(peer);
        }

        //public bool RemoveBySocket(Socket client)
        //{
        //    List<string> keys = TCPdic.Where(pairs => pairs.Value.Equals(client)).Select(pairs => pairs.Key).ToList();
        //    if (keys.Count > 0)
        //    {
        //        TCPdic.Remove(keys[0]);
        //        return true;
        //    }
        //    else
        //        return false;
        //}

        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                Socket clientSocket = socket.EndAccept(ar);
                lock (TCPdic)
                {
                    TCPdic.Add(clientSocket.RemoteEndPoint.ToString(), clientSocket);
                }

                byte[] buffer = new byte[1024];
                clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), new StateObject { WorkSocket = clientSocket, Buffer = buffer });

                // 继续接受下一个连接
                socket.BeginAccept(new AsyncCallback(AcceptCallback), null);
            }
            catch (SocketException)
            {
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket clientSocket = state.WorkSocket;
            try
            {
                int bytesRead = clientSocket.EndReceive(ar);
                if (bytesRead > 0)
                {
                    // 处理接收到的数据
                    string receivedData = encoding.GetString(state.Buffer, 0, bytesRead);
                    //Console.WriteLine("Received: " + receivedData);
                    OnStatusChanged(new Result(Result.NETStatus.ReciveMessage, receivedData, bytesRead, clientSocket.RemoteEndPoint.ToString()));
                    // 继续接收数据
                    clientSocket.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    // 连接已关闭
                    lock (TCPdic)
                    {
                        TCPdic.Remove(clientSocket);
                    }
                    clientSocket.Close();
                    OnStatusChanged(new Result(Result.NETStatus.RemoteStop, ""));
                }
            }
            catch (SocketException)
            {
                //lock (TCPdic)
                //{
                //    TCPdic.Remove(clientSocket);
                //}
            }
        }

        protected virtual void OnStatusChanged(Result status)
        {
            StatusChanged?.Invoke(status);
        }

        private void Recive_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //if (e.Cancelled)
            //{
            //    OnStatusChanged(new TCPResult(TCPStatus.ManualStop, "已停止监听此地址"));
            //}
            //else if (e.Error != null)
            //{
            //    OnStatusChanged(new TCPResult(TCPStatus.ExceptionStop, e.Error.Message));
            //}
            //else
            //{
            //    OnStatusChanged(new TCPResult(TCPStatus.RemoteStop, "已停止监听此地址"));
            //}
        }

        private void Monitor_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            OnStatusChanged(new Result(Result.NETStatus.MonitorStop, "监听停止"));
        }
    }
}
