using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace UpperApp.Core
{
    public static class Utils
    {
        private static readonly Encoding TextEncoding;

        static Utils()
        {
            // 必须先注册编码提供程序，再创建 GB2312 编码实例
            // （字段初始化器会先于静态构造函数体执行，所以不能在字段声明处直接 GetEncoding）
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            TextEncoding = Encoding.GetEncoding("GB2312");
        }

        public static string GetTime()
        {
            return "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "]";
        }

        public static string StringToHexString(string str)
        {
            if (string.IsNullOrEmpty(str)) return string.Empty;
            var sb = new StringBuilder();
            byte[] bytes = TextEncoding.GetBytes(str);
            for (int i = 0; i < bytes.Length; i++)
            {
                if (i > 0) sb.Append(' ');
                sb.Append(bytes[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public static string HexStringToString(string hexString)
        {
            if (string.IsNullOrWhiteSpace(hexString)) return null;
            string[] parts = hexString.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            byte[] bytes = new byte[parts.Length];
            try
            {
                for (int i = 0; i < parts.Length; i++)
                {
                    bytes[i] = Convert.ToByte(parts[i], 16);
                }
                return TextEncoding.GetString(bytes);
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
