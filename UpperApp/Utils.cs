using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Windows.Forms;


namespace UpperApp
{

    internal enum ChannelType
    {
        Unknown,
        Serial,
        TCP,
        UDP,
        Bluetooth
    }
    internal enum RecvOrSend
    {
        Recv = 0,
        Send = 1
    }

    [SupportedOSPlatform("windows10.0.19041.0")]
    public static class Utils
    {
        public static string getTime()
        {
            return "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "]";
        }

        // ==================== 字符串与十六进制转换 ====================

        /// <summary>
        /// 将普通字符串转换为十六进制显示格式（每个字符转两位十六进制，空格分隔）
        /// </summary>
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

        /// <summary>
        /// 将十六进制字符串（空格分隔）转换为普通字符串
        /// </summary>
        /// <returns>转换失败返回 null</returns>
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

        // ==================== 本地 IP 获取 ====================

        /// <summary>
        /// 获取本机所有 IPv4 地址
        /// </summary>
        public static List<string> GetLocalIPv4Addresses()
        {
            var list = new List<string>();
            try
            {
                string hostName = Dns.GetHostName();
                IPHostEntry ipEntry = Dns.GetHostEntry(hostName);
                foreach (var ip in ipEntry.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                        list.Add(ip.ToString());
                }
            }
            catch
            {
                // 忽略异常，返回空列表
            }
            return list;
        }

        // ==================== 窗口吸附 ====================

        /// <summary>
        /// 将窗体吸附到屏幕边缘
        /// </summary>
        /// <param name="form">要吸附的窗体</param>
        /// <param name="threshold">吸附阈值（像素）</param>
        public static void SnapToScreenEdge(Form form, int threshold = 10)
        {
            var screen = Screen.PrimaryScreen.Bounds;
            int screenRight = screen.Right;
            int screenBottom = screen.Bottom;
            int workspace = Screen.PrimaryScreen.WorkingArea.Bottom;
            int formRight = form.Left + form.Width;
            int formBottom = form.Top + form.Height;

            if (Math.Abs(screenRight - formRight) <= threshold)
                form.Left = screenRight - form.Width + 8;
            if (Math.Abs(form.Left) <= threshold)
                form.Left = -7;
            if (Math.Abs(screenBottom - formBottom) <= threshold)
                form.Top = screenBottom - form.Height + 8;
            if (Math.Abs(workspace - formBottom) <= threshold)
                form.Top = workspace - form.Height + 8;
            if (Math.Abs(form.Top) <= threshold)
                form.Top = 0;
        }

        // ==================== 端口输入验证 ====================

        /// <summary>
        /// 验证并修正端口号输入（1-65535）
        /// </summary>
        /// <returns>修正后的端口字符串</returns>
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

        // ==================== 记忆功能（文件/注册表） ====================

        /// <summary>
        /// 保存消息配置到文件
        /// </summary>
        public static void SaveMessagesToFile(string filePath, CheckBox[] checks, TextBox[] texts)
        {
            using (StreamWriter sw = new StreamWriter(filePath, false))
            {
                for (int i = 0; i < checks.Length; i++)
                {
                    string hexFlag = checks[i].Checked ? "H" : "A";
                    sw.WriteLine($"U{i + 1};{texts[i].Text};{hexFlag}");
                }
            }
        }

        /// <summary>
        /// 从文件加载消息配置
        /// </summary>
        public static void LoadMessagesFromFile(string filePath, CheckBox[] checks, TextBox[] texts)
        {
            if (!File.Exists(filePath)) return;
            using (StreamReader sr = new StreamReader(filePath, true))
            {
                string line;
                int i = 0;
                while ((line = sr.ReadLine()) != null && i < checks.Length)
                {
                    string[] parts = line.Split(';');
                    if (parts.Length >= 3)
                    {
                        texts[i].Text = parts[1];
                        checks[i].CheckState = parts[2] == "H" ? CheckState.Checked : CheckState.Unchecked;
                    }
                    i++;
                }
            }
        }

        /// <summary>
        /// 保存消息配置到注册表
        /// </summary>
        public static void SaveMessagesToRegistry(string registryPath, CheckBox[] checks, TextBox[] texts, string port)
        {
            using (RegistryKey hkSoftWare = Registry.LocalMachine.CreateSubKey(registryPath, true))
            using (RegistryKey regkey = hkSoftWare.CreateSubKey("Buf", true))
            {
                for (int i = 0; i < checks.Length; i++)
                {
                    regkey.SetValue($"UH{i + 1}", Convert.ToInt32(checks[i].Checked).ToString(), RegistryValueKind.String);
                    regkey.SetValue($"UB{i + 1}", texts[i].Text, RegistryValueKind.String);
                }
                regkey.SetValue("Port", port, RegistryValueKind.String);
            }
        }

        /// <summary>
        /// 从注册表加载消息配置
        /// </summary>
        /// <returns>返回保存的端口号，若未找到则返回 null</returns>
        public static string LoadMessagesFromRegistry(string registryPath, CheckBox[] checks, TextBox[] texts)
        {
            using (RegistryKey hkSoftWare = Registry.LocalMachine.OpenSubKey(registryPath))
            {
                if (hkSoftWare == null) return null;
                using (RegistryKey regkey = hkSoftWare.OpenSubKey("Buf"))
                {
                    if (regkey == null) return null;
                    for (int i = 0; i < checks.Length; i++)
                    {
                        string uhValue = regkey.GetValue($"UH{i + 1}")?.ToString();
                        if (uhValue != null)
                            checks[i].CheckState = (CheckState)int.Parse(uhValue);
                        string ubValue = regkey.GetValue($"UB{i + 1}")?.ToString();
                        if (ubValue != null)
                            texts[i].Text = ubValue;
                    }
                    return regkey.GetValue("Port")?.ToString();
                }
            }
        }
    }

    class BindingDic<T> where T : class
    {
        private readonly ConcurrentDictionary<string, T> ConnectDic = [];
        public readonly BindingList<string> connectionKeys;
        private readonly SynchronizationContext _sync;

        public int Count
        {
            get { return ConnectDic.Count; }
        }

        public bool TryGet(string key, out T value)
        {
            return ConnectDic.TryGetValue(key, out value);
        }

        public BindingDic()
        {
            connectionKeys = new BindingList<string>(ConnectDic.Keys.ToList());
            // 必须在 UI 线程创建
            _sync = SynchronizationContext.Current;
        }

        private void PostUI(Action action)
        {
            if (_sync == null)
            {
                // 非 UI 环境 fallback（一般不会发生）
                action();
            }
            else
            {
                _sync.Post(_ => action(), null);
            }
        }

        public void Add(string name, T obj)
        {
            if(ConnectDic.TryAdd(name, obj))
            {
                PostUI(() => connectionKeys.Add(name));
            }
        }

        public T Remove(string name)
        {
            if(ConnectDic.TryRemove(name, out T obj))
            {
                PostUI(() => connectionKeys.Remove(name));
                return obj;
            }
            return null;
        }
    }
}
