using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UpperApp
{
    class TCPManager
    {
        public Socket socket { get; private set; } = null;
        private readonly Dictionary<string, Socket> TCPdic = new Dictionary<string, Socket>();

        public void SetMonitorSocket(Socket s)
        {
            socket = s;
        }

        public void Send(string ip, byte[] buffer)
        {
            TCPdic[ip].Send(buffer);
        }

        public void ClearPeer()
        {
            TCPdic.Clear();
        }

        public string[] GetAllPeerIP()
        {
            return TCPdic.Keys.ToArray();
        }

        public void Remove(string peer)
        {
            TCPdic.Remove(peer);
        }

        public void Add(string point, Socket tSocket)
        {
            TCPdic.Add(point, tSocket);
        }

        public bool RemoveBySocket(Socket client)
        {
            List<string> keys = TCPdic.Where(pairs => pairs.Value.Equals(client)).Select(pairs => pairs.Key).ToList();
            if (keys.Count > 0)
            {
                TCPdic.Remove(keys[0]);
                return true;
            }
            else
                return false;
        }
    }
}
