//#define Firewall
//#define File

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Win32;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System.Runtime.Versioning;
using System.Threading.Tasks;
#if Firewall
    using NetFwTypeLib;
#endif

namespace UpperApp
{
    [SupportedOSPlatform("windows10.0.17763.0")]
    public partial class UpperApp : Form
    {
        private enum RecvOrSend
        {
            Recv = 0,
            Send = 1
        }

        [DefaultValue(0)]
        public int rn { get; set; }
        [DefaultValue(0)]
        public int sn { get; set; }
        private int Cnt = 0, Counter;
        private int xs = 50;
        private int ys = 50;
        private float lhp = 1, truedist = 1, xpro, ypro, distance, befordist = 0;
        private int px, py, dx, dy, bx, by;
        private short pflag = 0;
        private CheckBox[] checks = new CheckBox[8];
        private TextBox[] texts = new TextBox[8];
        private Button[] btn = new Button[8];
        private StreamWriter tf = null;
        private UDPManager udp = new UDPManager();
        private TCPManager tcp = new TCPManager();
        private BthManager bth = new BthManager();
        private Encoding encoding;
        //   private delegate void clk(int i);

        public UpperApp()
        {

            InitializeComponent();
            btnSend.Click += new EventHandler((sender, e) => { StrSend(SendBox.Text); });
            btnNoRL.Click += new EventHandler((sender, e) =>
            {
                RLBar.Value = 50;
                StrSend("RL:" + RLBar.Value + ":OVER\r\n");
            });
            Stop.Click += new EventHandler((sender, e) =>
            {
                FBBar.Value = 50;
                StrSend("FB:" + FBBar.Value + ":OVER\r\n");
            });
            btnclRecv.Click += new EventHandler((sender, e) => { ResetRS(RecvOrSend.Recv); });
            btnclSend.Click += new EventHandler((sender, e) => { ResetRS(RecvOrSend.Send); });
            smallChange.Leave += new EventHandler((sender, e) => { FBBar.SmallChange = RLBar.SmallChange = int.Parse(smallChange.Text); });
            FBBar.MouseUp += new MouseEventHandler((sender, e) => { StrSend("FB:" + FBBar.Value + ":OVER\r\n"); });
            RLBar.MouseUp += new MouseEventHandler((sender, e) => { StrSend("RL:" + RLBar.Value + ":OVER\r\n"); });

            SerPortItem.Leave += new EventHandler((sender, e) =>
            {
                if (SerPortItem.Text != "")
                    serialPort1.PortName = SerPortItem.Text;
            });
            SerPortItem.DropDown += new EventHandler((sender, e) =>
            {
                SerPortItem.Items.Clear();
                SerPortItem.Items.AddRange(SerialPort.GetPortNames());
            });
            Baud.SelectionChangeCommitted += new EventHandler((sender, e) => { serialPort1.BaudRate = (Baud.Text != "") ? int.Parse(Baud.Text) : 115200; });
            HostIP.DropDown += new EventHandler((sender, e) =>
            {
                HostIP.Items.Clear();
                GetLocalIP();
            });

            checks[0] = MsgHex1;
            checks[1] = MsgHex2;
            checks[2] = MsgHex3;
            checks[3] = MsgHex4;
            checks[4] = MsgHex5;
            checks[5] = MsgHex6;
            checks[6] = MsgHex7;
            checks[7] = MsgHex8;

            texts[0] = MsgBox1;
            texts[1] = MsgBox2;
            texts[2] = MsgBox3;
            texts[3] = MsgBox4;
            texts[4] = MsgBox5;
            texts[5] = MsgBox6;
            texts[6] = MsgBox7;
            texts[7] = MsgBox8;

            btn[0] = btnMsg1;
            btn[1] = btnMsg2;
            btn[2] = btnMsg3;
            btn[3] = btnMsg4;
            btn[4] = btnMsg5;
            btn[5] = btnMsg6;
            btn[6] = btnMsg7;
            btn[7] = btnMsg8;
            //for (int i = 0; i < 8; i++)
            //{
            //    btn[i].Click += new EventHandler((sender, e) => { btn_Click(i); });
            //}
            btn[0].Click += new EventHandler((sender, e) => { btn_Click(0); });
            btn[1].Click += new EventHandler((sender, e) => { btn_Click(1); });
            btn[2].Click += new EventHandler((sender, e) => { btn_Click(2); });
            btn[3].Click += new EventHandler((sender, e) => { btn_Click(3); });
            btn[4].Click += new EventHandler((sender, e) => { btn_Click(4); });
            btn[5].Click += new EventHandler((sender, e) => { btn_Click(5); });
            btn[6].Click += new EventHandler((sender, e) => { btn_Click(6); });
            btn[7].Click += new EventHandler((sender, e) => { btn_Click(7); });
            //*/
            CheckForIllegalCrossThreadCalls = false;
            Text = proname + prover;

            string[] Com = new string[8];
            Com[0] = "9600";
            Com[1] = "19200";
            Com[2] = "38400";
            Com[3] = "115200";
            Com[4] = "256000";
            Com[5] = "460800";
            Com[6] = "512000";
            Com[7] = "921600";
            Baud.Items.AddRange(Com);

            string[] NetType = new string[2];
            NetType[0] = "TCP";
            NetType[1] = "UDP";
            this.NetType.Items.AddRange(NetType);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            encoding = Encoding.GetEncoding("GB2312");
            //serialPort1.BaudRate = 115200;
            serialPort1.NewLine = "\r\n";
            serialPort1.Encoding = encoding;
            Tim.Text = "1000";
            GetLocalIP();
            ChoseSlaveBthList.DataSource = bth.BthClients.connectionKeys;
            Peer.DataSource = tcp.TCPdic.connectionKeys;

            tcp.StatusChanged += NetManager_StatusChanged;
            udp.StatusChanged += NetManager_StatusChanged;
            bth.StatusChanged += BthManager_StatusChanged;

#if File
                if (!File.Exists("Buffer.log"))
                {
                    FileStream fs = new FileStream("Buffer.log", FileMode.OpenOrCreate);
                    fs.Close();
                }
#endif

            filerd();
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);

#if Firewall
                NetFwAddApps(proname, Application.ExecutablePath);
#endif
        }

        #region 添加防火墙允许列表的代码
#if Firewall
                public static void NetFwAddApps(string name, string executablePath)
                {
                    INetFwMgr netFwMgr = (INetFwMgr)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwMgr"));
                    INetFwAuthorizedApplication app = (INetFwAuthorizedApplication)Activator.CreateInstance(
                        Type.GetTypeFromProgID("HNetCfg.FwAuthorizedApplication"));

                    //在例外列表里，程序显示的名称  
                    app.Name = name;

                    //程序的路径及文件名  
                    app.ProcessImageFileName = executablePath;

                    //是否启用该规则
                    app.Enabled = true;

                    //创建firewall管理类的实例  
                    //加入到防火墙的管理策略  
            
                    bool exist = false;
                    //加入到防火墙的管理策略  
                    foreach (INetFwAuthorizedApplication mApp in netFwMgr.LocalPolicy.CurrentProfile.AuthorizedApplications)
                    {
                        if (app == mApp)
                        {
                            exist = true;
                            break;
                        }
                    }
                    if (!exist)
                        netFwMgr.LocalPolicy.CurrentProfile.AuthorizedApplications.Add(app);
                }
#endif
        #endregion

        #region 记忆功能相关代码
#if File
                private void filerd()
                {
                    string data = string.Empty;
                    string addr = string.Empty;
                    string hex = string.Empty;
                    StreamReader sr = new StreamReader("Buffer.log", true);
                    data = sr.ReadLine();
                    int i = 0;
                    while(data != null)
                    {
                        int num = data.IndexOf(';');
                        addr = data.Substring(0, num);
                        data = data.Remove(0, num + 1);
                        num = data.IndexOf(';');
                        hex = data.Remove(0, num + 1);
                        data = data.Substring(0, num);
                        if (hex == "H")
                            checks[i].CheckState = System.Windows.Forms.CheckState.Checked;
                        else if (hex == "A")
                            checks[i].CheckState = System.Windows.Forms.CheckState.Unchecked;
                        texts[i].Text = data;
                        i++;
                        data = sr.ReadLine();
                    }
                    sr.Close();
                }
#else
        private void filerd()
        {
            RegistryKey hkSoftWare;
            RegistryKey regkey;
            if ((hkSoftWare = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\JXUST")) != null)
            {
                if ((regkey = hkSoftWare.OpenSubKey("Buf")) != null)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        texts[i].Text = regkey.GetValue("UB" + (i + 1)).ToString();
                        checks[i].CheckState = (CheckState)int.Parse(regkey.GetValue("UH" + (i + 1)).ToString());
                    }
                    Port.Text = regkey.GetValue("Port").ToString();
                    regkey.Close();
                }
                hkSoftWare.Close();
            }
        }
#endif
        #endregion

        private string getTime()
        {
            return "[" + DateTime.Now.ToLongTimeString() + "]:\r\n";
        }

        private void SetRS(int n, RecvOrSend rs)
        {
            if (rs == RecvOrSend.Recv)
            {
                rn += n;
                label18.Text = rn.ToString();
            }
            else
            {
                sn += n;
                label22.Text = sn.ToString();
            }
        }

        private void NetManager_StatusChanged(Result status)
        {
            // 确保在UI线程上更新UI
            if (InvokeRequired)
            {
                Invoke(new Action(() => NetManager_StatusChanged(status)));
            }
            else
            {
                switch (status.NetStatus)
                {
                    case Result.NETStatus.ManualStop: Peer.Text = ""; break;
                    case Result.NETStatus.ExceptionStop:
                        {
                            if (!string.Equals(status.Message, "你的主机中的软件中止了一个已建立的连接。"))
                                MessageBox.Show(this, status.Message, "error");
                            Peer.Text = "";
                        }; break;
                    case Result.NETStatus.RemoteStop: Peer.Text = ""; Infotext.Text = status.Message; break;
                    case Result.NETStatus.ReciveMessage:
                        {
                            if (!string.IsNullOrWhiteSpace(status.IPPort))
                            {
                                Peer.Text = status.IPPort;
                            }
                            SetRS(status.Num, RecvOrSend.Recv);
                            if (rbtnChar.Checked)
                            {
                                if (!string.IsNullOrWhiteSpace(status.NewPeer))
                                {
                                    RecvBox.AppendText(status.NewPeer);
                                    RecvBox.AppendText(status.Message);
                                    tf?.WriteLine(getTime() + status.Message);
                                }
                                else
                                {
                                    string EndPoint = status.RemoteIP;
                                    RecvBox.AppendText(EndPoint + ":\r\n" + status.Message + "\r\n");
                                    tf?.WriteLine(getTime() + EndPoint + ":\r\n" + status.Message + "\r\n");
                                }
                            }
                            else if (rbtnHex.Checked)
                            {
                                if (!string.IsNullOrWhiteSpace(status.NewPeer))
                                {
                                    RecvBox.AppendText(status.NewPeer);
                                }
                                Str2Hexstr(status.Message);
                            }

                            if (AngDirDisp.Checked)
                            {
                                SetAngDisp(status.Message);
                            }
                        }; break;
                    case Result.NETStatus.SendMessage:
                        {
                            if (status.status == Result.ResStatus.SetNum)
                            {
                                if (ReDisp.Checked)
                                    RecvBox.AppendText(status.Message);
                                SetRS(status.Num, RecvOrSend.Send);
                            }
                            else if (status.status == Result.ResStatus.Error)
                            {
                                Infotext.Text = status.Message;
                            }
                        }; break;
                    case Result.NETStatus.NewRemote:
                        {
                            Infotext.Text = "连接成功！";
                            Peer.Text = status.Message;
                        }; break;
                    case Result.NETStatus.MonitorStop: SocketIsOpen(false); break;
                    case Result.NETStatus.MonitorStart: SocketIsOpen(true); break;
                    default: break;
                }
            }
        }

        private void BthManager_StatusChanged(Result status)
        {
            // 确保在UI线程上更新UI
            if (InvokeRequired)
            {
                Invoke(new Action(() => BthManager_StatusChanged(status)));
            }
            else
            {
                switch (status.NetStatus)
                {
                    case Result.NETStatus.ManualStop: Peer.Text = ""; break;
                    case Result.NETStatus.ExceptionStop:
                        {
                            MessageBox.Show(this, status.Message, "error");
                            Peer.Text = "";
                        }; break;
                    case Result.NETStatus.RemoteStop:
                        {
                            Infotext.Text = "连接断开!";
                            BthListenBtn.Text = "连接断开";
                        }; break;
                    case Result.NETStatus.ReciveMessage:
                        {
                            BthRecvBox.AppendText("Receiving data: " + status.Message + "\r\n");
                            SetRS(status.Num, RecvOrSend.Recv);
                        }; break;
                    case Result.NETStatus.SendMessage:
                        {
                            BthRecvBox.AppendText(status.Message);
                            SetRS(status.Num, RecvOrSend.Send);
                        }; break;
                    case Result.NETStatus.NewRemote:
                        {
                            BthRecvBox.AppendText(status.Message);
                        }; break;
                    case Result.NETStatus.MonitorStop: break;
                    case Result.NETStatus.MonitorStart: break;
                    default: break;
                }
            }
        }

        private void ResetRS(RecvOrSend rs)
        {
            if (rs == RecvOrSend.Recv)
            {
                rn = 0;
                label18.Text = rn.ToString();
                RecvBox.Text = "";
                Infotext.Text = "接收区已清空";
            }
            else
            {
                sn = 0;
                label22.Text = sn.ToString();
                SendBox.Text = "";
                Infotext.Text = "发送区已清空";
            }
        }

        private void SetAngDisp(string str)
        {
            if (str.Contains("/OVER"))
            {
                int num = str.IndexOf(':');
                string data = str.Remove(0, num + 1);
                num = data.IndexOf('/');
                data = data.Substring(0, num);
                if (str.Contains("YAW:"))
                    LabYaw.Text = data.ToString();
                else if (str.Contains("PITCH:"))
                    LabPitch.Text = data.ToString();
                else if (str.Contains("ROLL:"))
                    LabRoll.Text = data.ToString();
                else if (str.Contains("DISTANCE:"))
                    LabDist.Text = data.ToString();
            }
        }

        private void UpperApp_Load(object sender, EventArgs e)
        {
            //EventHandler<BluetoothWin32AuthenticationEventArgs> handler = new EventHandler<BluetoothWin32AuthenticationEventArgs>(HandleRequests);
            serialPort1.DataReceived += new SerialDataReceivedEventHandler(serialPort1_Rx);//必须手动添加事件处理程序
            FBtext.DataBindings.Add("Text", FBBar, "Value", false, DataSourceUpdateMode.OnPropertyChanged);
            RLtext.DataBindings.Add("Text", RLBar, "Value", false, DataSourceUpdateMode.OnPropertyChanged);

        }

        public void GetLocalIP()
        {
            try
            {
                string HostName = Dns.GetHostName(); //得到主机名
                IPHostEntry IpEntry = Dns.GetHostEntry(HostName);
                for (int i = 0; i < IpEntry.AddressList.Length; i++)
                {
                    //从IP地址列表中筛选出IPv4类型的IP地址
                    //AddressFamily.InterNetwork表示此IP为IPv4,
                    //AddressFamily.InterNetworkV6表示此地址为IPv6类型
                    if (IpEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                    {
                        HostIP.Items.Add(IpEntry.AddressList[i].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("获取本机IP出错:" + ex.Message);
            }
        }

        //将字符串变为16进制字符
        private void Str2Hexstr(string str)
        {
            string result = string.Empty;
            for (int i = 0; i < str.Length; i++)//逐字节变为16进制字符，以%隔开
            {
                string add = Convert.ToString(str[i], 16).ToUpper();
                if (add.Length == 1)
                    add = "0" + add;
                result += " " + add;
            }
            result += "\r\n";
            RecvBox.AppendText(result);
            tf?.WriteLine(getTime() + result);
        }

        //将字符串转换成16进制代码
        private string Str2Hex(string s)
        {
            string[] st = s.Trim().Split(' ');
            byte[] b = new byte[st.Length];
            try//按照指定编码将string编程字节数组
            {
                for (int i = 0; i < st.Length; i++)//以十六进制将字符串转换成字节
                {
                    b[i] = Convert.ToByte(st[i], 16);
                }
            }
            catch
            {
                MessageBox.Show(this, "请将每字节间以空格分开");
            }

            string result = Encoding.ASCII.GetString(b).ToString();
            return result;
        }

        //发送数据  串口TCP和UDP
        private void StrSend(string Buf)
        {
            if (rbtnSerial.Checked)
            {
                if (serialPort1.IsOpen)
                {
                    byte[] buffer = encoding.GetBytes(Buf);
                    serialPort1.Write(buffer, 0, buffer.Length);
                    SetRS(buffer.Length, RecvOrSend.Send);
                    if (ReDisp.Checked)
                        RecvBox.AppendText(Buf);
                }// if (serialPort1.IsOpen)
                else
                    Infotext.Text = "串口未打开！";
            }//if (rbtnSerial.Checked)
            else if (rbtnNET.Checked)
            {
                if (btnListen.Text == "停止监听")
                {
                    if (NetType.Text == "TCP")
                    {
                        try
                        {
                            string ip = Peer.Text;
                            if (ip == "")
                                Infotext.Text = "未选定端口";
                            else
                            {
                                tcp.Send(ip, Buf);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(this, ex.Message, "error");
                            tcp.Remove(Peer.Text);
                            Peer.Text = "";
                        }
                    }//if (NetType.Text == "TCP")
                    else if (NetType.Text == "UDP")
                    {
                        udp.UDP_Send(Buf, Peer.Text);
                    }
                }//if (btnListen.Text == "停止监听")
            }//if (rbtnNET.Checked)
        }

        private void serialPort1_Rx(object sender, SerialDataReceivedEventArgs e)
        {
            int n = serialPort1.BytesToRead;
            byte[] Buf = new byte[n];
            serialPort1.Read(Buf, 0, n);
            string str = encoding.GetString(Buf);
            SetRS(n, RecvOrSend.Recv);

            if (AngDirDisp.Checked)
            {
                SetAngDisp(str);
            }

            if (rbtnChar.Checked)
            {
                RecvBox.AppendText(str);
                tf?.WriteLine(getTime() + str);
            }
            else if (rbtnHex.Checked)
            {
                Str2Hexstr(str);
            }
        }

        private void BthListenBtn_Click(object sender, EventArgs e)
        {
            if (bth.br == null)
            {
                MessageBox.Show(this, "检测不到本机蓝牙设备", "error");
            }
            else if (bth.br.Mode == RadioMode.PowerOff)
            {
                MessageBox.Show(this, "请先在系统中打开蓝牙", "warning");
            }
            else
            {
                BthRecvBox.AppendText(string.Format("* Radio, address: {0:C}\r\n", bth.br.LocalAddress));
                BthRecvBox.AppendText("Mode: " + bth.br.Mode.ToString() + "\r\n");
                if (bth.BthMonitor == null)
                {
                    bth.StartMonitor();
                    BthRecvBox.AppendText("Service started!\r\n");
                    BthListenBtn.Text = "关闭";
                }
                else if (bth.BthMonitor != null)
                {
                    bth.StopMonitor();
                    BthListenBtn.Text = "监听";
                }
            }
        }

        private void BthSendBtn_Click(object sender, EventArgs e)
        {
            if (BthConnectBtn.Text == "断开")
                bth.StrSend(BthSendBox.Text);
            else if (!string.IsNullOrEmpty(ChoseSlaveBthList.Text))
                bth.StrSend(BthSendBox.Text, bth.GetSlaveClient(ChoseSlaveBthList.Text));
            else
                MessageBox.Show(this, "请连接设备", "warning");
        }

        //private void HandleRequests(object that, BluetoothWin32AuthenticationEventArgs e)
        //{
        //    e.Confirm = true;
        //}

        private void BthConnectBtn_Click(object sender, EventArgs e)
        {
            if (BthDeviceList.SelectedIndex == -1)
            {
                if (BthConnectBtn.Text == "连接")
                    MessageBox.Show(this, "请选择设备", "warning");
                else
                {
                    bth.DisconnectClient();
                    BthConnectBtn.Text = "连接";
                }
            }
            else
            {
                bth.SetClient(bth.BthDevices[BthDeviceList.SelectedText]);
                BthConnectBtn.Text = "断开";
            }
        }

        private void btnListen_Click(object sender, EventArgs e)
        {
            if (btnListen.Text == "开始监听")
            {
                if (HostIP.Text != "")
                {
                    IPAddress LocalIP = IPAddress.Parse(HostIP.Text);
                    int LocalPort = int.Parse(Port.Text);
                    IPEndPoint LocalIPEndPoint = new IPEndPoint(LocalIP, LocalPort);
                    if (NetType.Text == "TCP")
                    {
                        Peer.DataSource = tcp.TCPdic.connectionKeys;
                        //使用IPv4地址，流式socket方式，tcp协议传递数据
                        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                        //创建好socket后，必须告诉socket绑定的IP地址和端口号。
                        try
                        {
                            socket.Bind(LocalIPEndPoint);

                            //同一个时间点过来10个客户端，排队
                            socket.Listen(10);
                            tcp.SetMonitorSocket(socket);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(this, ex.Message, "error");
                        }
                    }//if (comboBox3.Text == "TCP")
                    else if (NetType.Text == "UDP")
                    {
                        Peer.DataSource = udp.GetUDPPeer();
                        udp.StartUDP(LocalIPEndPoint);
                    }//if (comboBox3.Text == "UDP")
                    else
                        Infotext.Text = "请选择模式";
                }//if (comboBox4.Text != "")
                else
                    Infotext.Text = "请选择IP";
            }//if (button9.Text == "开始监听")
            else if (btnListen.Text == "停止监听")
            {
                if (NetType.Text == "TCP")
                {
                    try
                    {
                        //comboBox3.Text = "";
                        tcp.StopMonitor();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, ex.Message, "error");
                    }
                }
                else if (NetType.Text == "UDP")
                {
                    udp.CloseUDP();
                }

            }//if (button9.Text == "停止监听")
        }


        private void SocketIsOpen(bool isOpen)
        {
            if (isOpen)
            {
                Infotext.Text = "开始监听";
                btnListen.Text = "停止监听";
                NetType.Enabled = false;
                HostIP.Enabled = false;
                rbtnNET.Checked = true;
                Peer.Text = "";
            }
            else
            {
                Infotext.Text = "停止监听";
                btnListen.Text = "开始监听";
                NetType.Enabled = true;
                HostIP.Enabled = true;
            }
        }

        private void SendTimer_Tick(object sender, EventArgs e)
        {
            Infotext.Text = Cnt++.ToString();
            if (btnAutoSend.Checked && (Cnt >= Counter))
            {
                StrSend(SendBox.Text);
                Cnt = 0;
            }
        }

        private void btnBegin_Click(object sender, EventArgs e)
        {
            if (btnBegin.Text == "开始")
            {
                if (btnSerial.Text != "打开串口" || btnListen.Text != "开始监听")
                {
                    Counter = int.Parse(Tim.Text) / 100;
                    if (Counter < 1)
                    {
                        Infotext.Text = "时间间隔过小";
                        Tim.Text = "1000";
                        Counter = 10;
                    }
                    SendTimer.Start(); //开启定时器
                    btnBegin.Text = "停止";
                }
                else
                    Infotext.Text = "端口未打开!";
            }
            else
            {
                SendTimer.Stop(); //停止定时器
                btnBegin.Text = "开始";
            }
        }

        private void btnSerial_Click(object sender, EventArgs e)
        {
            if (SerPortItem.Text != "")
                if (btnSerial.Text == "打开串口")
                {
                    serialPort1.Open();
                    btnSerial.Text = "关闭串口";
                    Infotext.Text = "串口打开！";
                    SerPortItem.Enabled = false;
                    Baud.Enabled = false;
                    rbtnSerial.Checked = true;
                }
                else
                {
                    serialPort1.Close();
                    if (!serialPort1.IsOpen)
                        Infotext.Text = "串口关闭！";
                    btnSerial.Text = "打开串口";
                    SerPortItem.Enabled = true;
                    Baud.Enabled = true;
                }
            else
                Infotext.Text = "未选中串口";
        }

        private void Rocker_Click(object sender, EventArgs e)
        {
            if (Rocker.Text == "摇杆开")
            {
                Rocker.Text = "摇杆关";
            }
            else
            {
                Rocker.Text = "摇杆开";
                setV(50, 50);
                StrSend("FR:" + FBtext.Text + ":" + RLtext.Text + ":OVER\r\n");
            }
        }

        private void Rocker_MouseMove(object sender, MouseEventArgs e)
        {
            if (Rocker.Text == "摇杆关")
            {
                int x = e.Location.X * 100 / 140;
                int y = (-e.Location.Y + 160) * 100 / 150;

                if (x < 0)
                    x = 0;
                else if (x > 100)
                    x = 100;

                if (y < 0)
                    y = 0;
                else if (y > 100)
                    y = 100;

                if (x % 5 == 0)
                    RLBar.Value = x;
                if (y % 5 == 0)
                    FBBar.Value = y;

                if (xs != RLBar.Value || ys != FBBar.Value)
                {
                    string Buf = "FR:" + FBBar.Value + ":" + RLBar.Value + ":OVER\r\n";
                    if (rbtnSerial.Checked && serialPort1.IsOpen)
                    {
                        Encoding gb = encoding;
                        byte[] buffer = gb.GetBytes(Buf);
                        serialPort1.Write(buffer, 0, buffer.Length);
                        SetRS(buffer.Length, RecvOrSend.Send);
                        if (ReDisp.Checked)
                        {
                            RecvBox.AppendText(Buf);
                        }
                    }//if (radioButton4.Checked)
                    else if (rbtnNET.Checked && btnListen.Text == "停止监听")
                    {
                        if (NetType.Text == "TCP")
                        {
                            try
                            {
                                string ip = Peer.Text;
                                if (ip == "")
                                    Infotext.Text = "未选定端口";
                                else
                                {
                                    tcp.Send(ip, Buf);
                                }
                            }
                            catch (Exception ex)
                            {
                                Rocker.Text = "摇杆开";
                                setV(50, 50);
                                MessageBox.Show(this, ex.Message, "error");
                                tcp.Remove(Peer.Text);
                                Peer.Text = "";
                            }
                        }//if (comboBox3.Text == "TCP")
                        else if (NetType.Text == "UDP")
                        {
                            try
                            {
                                udp.UDP_Send(Buf, Peer.Text);
                            }
                            catch (Exception ex)
                            {
                                Rocker.Text = "摇杆开";
                                setV(50, 50);
                                MessageBox.Show(this, ex.Message, "error");
                            }
                        }//if (comboBox3.Text == "UDP")
                    }//if (radioButton3.Checked)
                }//if (xs != label7.Text || ys != label5.Text)
                xs = RLBar.Value;
                ys = FBBar.Value;
            }//if (button8.Text == "摇杆关")
        }

        private void setV(int Vx, int Vy)
        {
            FBBar.Value = Vx;
            RLBar.Value = Vy;
        }

        private void ClearAngDisp_Click(object sender, EventArgs e)
        {
            string zero = "0";
            LabYaw.Text = zero;
            LabRoll.Text = zero;
            LabPitch.Text = zero;
            LabDist.Text = zero;
            Infotext.Text = "数据清除成功";
        }

        private void MemTimer_Tick(object sender, EventArgs e)
        {
            double usemem = Process.GetCurrentProcess().PrivateMemorySize64 / 1024.0 / 1024.0;
            if (usemem > 100)
                Application.Exit();
            try
            {
                label41.Text = usemem.ToString().Substring(0, 5) + "M";
            }
            catch { }
        }

        private void SaveData_CheckedChanged(object sender, EventArgs e)
        {
            if (SaveData.Checked)
            {
                DialogResult dr = openFileDialog1.ShowDialog();
                if (dr == DialogResult.OK)
                {
                    tf = new StreamWriter(@openFileDialog1.FileName, true);
                }
                else
                    SaveData.CheckState = CheckState.Unchecked;
            }
            else if (!SaveData.Checked)
            {
                if (tf != null)
                {
                    tf.Close();
                    tf = null;
                }
            }
        }

        private void Port_TextChanged(object sender, EventArgs e)
        {
            MatchCollection mc = Regex.Matches(Port.Text, "[0-9]");
            int num = 0;
            foreach (Match s in mc)
            {
                num = num * 10 + int.Parse(s.Value);
            }
            if (num != 0 && num < 65536)
                Port.Text = num.ToString();
            else if (num == 0)
                Port.Text = "";
            else
                Port.Text = "65535";
        }

        private void RealDist_TextChanged(object sender, EventArgs e)
        {
            string buf = RealDist.Text.Replace(" ", string.Empty);
            if (buf != ".")
            {
                truedist = float.Parse(buf);
                Infotext.Text = "dist:" + truedist;
            }
        }

        #region 记忆功能相关函数
#if File
                private void UpperApp_FormClosing(object sender, FormClosingEventArgs e)
                {
                    StreamWriter sw = new StreamWriter("Buffer.log", false);
                    for (int i = 0; i < 8; i++)
                    {
                        if (checks[i].Checked)
                            sw.WriteLine("U" + (i + 1) + ";" + texts[i].Text + ";H");
                        else
                            sw.WriteLine("U" + (i + 1) + ";" + texts[i].Text + ";A");
                    }
                    sw.Close();
                }
#else
        private void UpperApp_FormClosing(object sender, FormClosingEventArgs e)
        {
            RegistryKey hkSoftWare = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\JXUST", true);
            RegistryKey regkey = hkSoftWare.CreateSubKey("Buf", true);
            for (int i = 0; i < 8; i++)
            {
                regkey.SetValue("UH" + (i + 1), Convert.ToInt32(checks[i].Checked).ToString(), RegistryValueKind.String);
                regkey.SetValue("UB" + (i + 1), texts[i].Text, RegistryValueKind.String);
            }
            regkey.SetValue("Port", Port.Text, RegistryValueKind.String);
            regkey.Close();
            hkSoftWare.Close();
        }
#endif
        #endregion

        private void LabDist_TextChanged(object sender, EventArgs e)
        {
            float dist;
            double angle;
            double xdist, ydist;
            Graphics g = MapBox.CreateGraphics();
            dist = float.Parse(LabDist.Text) - befordist;
            befordist = float.Parse(LabDist.Text);
            angle = double.Parse(LabYaw.Text);
            xdist = Math.Cos(Math.PI * angle / 180) * dist;
            ydist = Math.Sin(Math.PI * angle / 180) * dist;
            px += (int)(xdist / xpro);
            py += (int)(ydist / ypro);
            RecvBox.AppendText("坐标点=" + px + "," + py + "\r\n");
            RecvBox.AppendText("路程=" + xdist + "," + ydist + "\r\n");
            g.FillEllipse(Brushes.Blue, px, 213 - py, 3, 3);
        }

        private void Info_DoubleClick(object sender, EventArgs e)
        {
            MessageBox.Show(this, ("Project Version " + prover + " By ClockSR\r\n                                       2018.8.25"), Text);
        }

        private void btn_Click(int i)
        {
            if (checks[i].Checked)
            {
                if (texts[i].Text != "")
                    StrSend(Str2Hex(texts[i].Text));
            }
            else
                StrSend(texts[i].Text);
        }

        private void OpenImage_Click(object sender, EventArgs e)
        {
            DialogResult dr = openFileDialog1.ShowDialog();
            if (dr == DialogResult.OK)
            {
                try
                {
                    Image image = Image.FromFile(@openFileDialog1.FileName);
                    MapBox.BackgroundImage = image;
                    lhp = image.Height / MapBox.Height / (float)(image.Width / MapBox.Width);
                }
                catch
                {
                    MessageBox.Show(this, "请选择图片文件！", "Warning");
                }
            }
        }

        private void MapBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (pflag == 1)
            {
                MapBox.Refresh();
                Graphics g = MapBox.CreateGraphics();
                g.DrawLine(Pens.Blue, bx, (213 - by), e.Location.X, e.Location.Y);
            }
            else if (pflag == 2)
            {
                Graphics g = MapBox.CreateGraphics();
                MapBox.Refresh();
                g.FillEllipse(Brushes.Blue, bx, 213 - by, 3, 3);
                pflag = 3;
            }
            label38.Text = e.Location.X + "," + (213 - e.Location.Y);
        }

        private void ClearImage_Click(object sender, EventArgs e)
        {
            pflag = 0;
            //Graphics g = MapBox.CreateGraphics();
            MapBox.Refresh();
            label34.Text = label36.Text = "0,0";
        }

        private void MapBox_Click(object sender, EventArgs e)
        {
            Point p = MapBox.PointToClient(MousePosition);
            if (pflag == 0)
            {
                Graphics g = MapBox.CreateGraphics();
                g.FillEllipse(Brushes.Blue, p.X, p.Y, 3, 3);
                px = bx = p.X;
                py = by = (213 - p.Y);
                label34.Text = bx + "," + by;
                pflag = 1;
            }
            else if (pflag == 1)
            {
                dx = p.X - bx;
                dy = (213 - p.Y) - by;
                distance = (float)Math.Sqrt(dx * dx + dy * dy * lhp * lhp);
                RecvBox.AppendText("宽高比=" + lhp + "\r\n" + "锚点距离=" + distance + "\r\n");
                xpro = truedist / distance;
                ypro = xpro * lhp;
                label36.Text = p.X + "," + (213 - p.Y);
                RecvBox.AppendText("横向比值=" + xpro + "\r\n" + "纵向比值=" + ypro + "\r\n");
                pflag = 2;
            }
        }

        private void UpperApp_Move(object sender, EventArgs e)
        {
            int screenRight = Screen.PrimaryScreen.Bounds.Right;//屏幕右边缘
            int formRight = Left + Size.Width;//窗口右边缘=窗口左上角x+窗口宽度
            int screenBottom = Screen.PrimaryScreen.Bounds.Bottom;//屏幕下边缘
            //int screenTop = Screen.PrimaryScreen.Bounds.Top;//屏幕上边缘
            //int screenLeft = Screen.PrimaryScreen.Bounds.Left;//屏幕左边缘
            int workspace = Screen.PrimaryScreen.WorkingArea.Bottom;
            int formBottom = Top + Size.Height;//窗口下边缘

            if (Math.Abs(screenRight - formRight) <= 10)//往右靠
                Location = new Point(screenRight + 8 - Size.Width, Top);//当前窗口坐标赋值，实现吸附

            if (Math.Abs(Left) <= 10)//往左靠
                Location = new Point(0 - 7, Top);

            if (Math.Abs(screenBottom - formBottom) <= 10)//往下靠
                Location = new Point(Left, screenBottom - Size.Height + 8);

            if (Math.Abs(workspace - formBottom) <= 10)//往下靠
                Location = new Point(Left, workspace - Size.Height + 8);

            if (Math.Abs(Top) <= 10)//往上靠
                Location = new Point(Left, 0);
        }

        private void BthDeviceScanBtn_Click(object sender, EventArgs e)
        {
            if (BthDeviceScanBtn.Enabled)
            {
                BthDeviceScanBtn.Enabled = false;
                BthDeviceScanBtn.Text = "扫描中";
                BluetoothClient client = new();
                Task.Run(() =>
                {
                    foreach (BluetoothDeviceInfo d in client.DiscoverDevices())
                    {
                        bth.BthDevices.TryAdd(d.DeviceName, d);
                        if (!BthDeviceList.Items.Contains(d.DeviceName))
                            BthDeviceList.Items.Add(d.DeviceName);
                    }
                    BthDeviceScanBtn.Enabled = true;
                    BthDeviceScanBtn.Text = "扫描蓝牙";
                });
            }
        }
    }//public partial class Form1 : Form
}//namespace WindowsFormsApp1
