using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.Versioning;
using System.Text;

namespace UpperApp
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    public static class Utils
    {
        public static string GetTime()
        {
            return "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "]";
        }

        public static string StringToHexString(string str)
        {
            if (string.IsNullOrEmpty(str)) return string.Empty;
            var sb = new StringBuilder();
            foreach (char c in str)
            {
                sb.Append(' ');
                sb.Append(((int)c).ToString("X2"));
            }
            return sb.ToString().TrimStart();
        }

        public static string HexStringToString(string hexString)
        {
            if (string.IsNullOrWhiteSpace(hexString)) return null;
            string[] parts = hexString.Trim().Split(' ');
            byte[] bytes = new byte[parts.Length];
            try
            {
                for (int i = 0; i < parts.Length; i++)
                {
                    bytes[i] = Convert.ToByte(parts[i], 16);
                }
                return Encoding.ASCII.GetString(bytes);
            }
            catch
            {
                return null;
            }
        }

        public static List<string> GetLocalIPv4Addresses()
        {
            var list = new List<string>();
            try
            {
                foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus != OperationalStatus.Up)
                        continue;
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                        continue;
                    foreach (var ua in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ua.Address.AddressFamily == AddressFamily.InterNetwork &&
                            !IPAddress.IsLoopback(ua.Address))
                        {
                            list.Add(ua.Address.ToString());
                        }
                    }
                }
            }
            catch
            {
            }
            return list;
        }

        public static string ValidatePortInput(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            if (int.TryParse(input, out int port))
            {
                if (port < 1) return string.Empty;
                if (port > 65535) return "65535";
                return port.ToString();
            }
            return string.Empty;
        }
    }
}
