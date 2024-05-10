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
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;

#if Firewall
    using NetFwTypeLib;
#endif

namespace UpperApp
{
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
        //private Socket socket = null;
        private Socket bthsocket = null;
        private Thread thread = null;
        private Thread BthCon = null;
        private BluetoothListener listener = null;
        private BluetoothClient BthClient = null;
        private Thread Bthlisten = null;
        private Thread ReceThread = null;
        //private Dictionary<string, Socket> TCPdic = new Dictionary<string, Socket>();
        private CheckBox[] checks = new CheckBox[8];
        private TextBox[] texts = new TextBox[8];
        private Button[] btn = new Button[8];
        private StreamWriter tf =null;
        private BluetoothRadio br = null;
        private UDPManager udp = new UDPManager();
        private TCPManager tcp = new TCPManager();
        //   private delegate void clk(int i);

        public UpperApp()
        {
            InitializeComponent();
            btnSend.Click += new EventHandler((sender, e) => { StrSend(SendBox.Text); });
            btnNoRL.Click += new EventHandler((sender, e) => {
                RLBar.Value = 50;
                StrSend("RL:" + RLBar.Value + ":OVER\r\n");
            });
            Stop.Click += new EventHandler((sender, e) => {
                FBBar.Value = 50;
                StrSend("FB:" + FBBar.Value + ":OVER\r\n");
            });
            btnclRecv.Click += new EventHandler((sender, e) => { ResetRS(RecvOrSend.Recv); });
            btnclSend.Click += new EventHandler((sender, e) => { ResetRS(RecvOrSend.Send); });
            smallChange.Leave += new EventHandler((sender, e) => { FBBar.SmallChange = RLBar.SmallChange = int.Parse(smallChange.Text); });
            FBBar.MouseUp += new MouseEventHandler((sender, e) => { StrSend("FB:" + FBBar.Value + ":OVER\r\n"); });
            RLBar.MouseUp += new MouseEventHandler((sender, e) => { StrSend("RL:" + RLBar.Value + ":OVER\r\n"); });

            SerPortItem.Leave += new EventHandler((sender, e) => {
                if (SerPortItem.Text != "")
                    serialPort1.PortName = SerPortItem.Text;
            });
            SerPortItem.DropDown += new EventHandler((sender, e) => {
                SerPortItem.Items.Clear();
                SerPortItem.Items.AddRange(SerialPort.GetPortNames());
            });
            Baud.SelectionChangeCommitted += new EventHandler((sender, e) => { serialPort1.BaudRate = (Baud.Text != "") ? int.Parse(Baud.Text) : 115200; });
            HostIP.DropDown += new EventHandler((sender, e) => {
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

            //serialPort1.BaudRate = 115200;
            serialPort1.NewLine = "\r\n";
            serialPort1.Encoding = Encoding.GetEncoding("GB2312");
            Tim.Text = "1000";
            GetLocalIP();
            
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

        private void SetInfoAfterUDPSend(UDPResult result)
        {
            if (result.status == Result.ResStatus.SetNum)
            {
                if (ReDisp.Checked)
                    RecvBox.AppendText(result.Message);
                SetRS(result.Num, RecvOrSend.Send);
            }
            else if (result.status == Result.ResStatus.Error)
            {
                Infotext.Text = result.Message;
            }
        }

        private void UpperApp_Load(object sender, EventArgs e)
        {
            EventHandler<BluetoothWin32AuthenticationEventArgs> handler = new EventHandler<BluetoothWin32AuthenticationEventArgs>(HandleRequests);
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
                MessageBox.Show(this,"请将每字节间以空格分开");
            }

            string result = Encoding.ASCII.GetString(b).ToString();
            return result;
        }

        //发送数据  串口TCP和UDP
        private void StrSend(string Buf)
        {
            Encoding gb = Encoding.GetEncoding("gb2312");
            byte[] buffer = gb.GetBytes(Buf);
            if (rbtnSerial.Checked)
            {
                if (serialPort1.IsOpen)
                {
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
                                tcp.Send(ip, buffer);
                                SetRS(buffer.Length,RecvOrSend.Send);
                                if (ReDisp.Checked)
                                {
                                    RecvBox.AppendText(Buf);
                                }
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
                        UDPResult result = udp.UDP_Send(Buf, Peer.Text);
                        SetInfoAfterUDPSend(result);
                    }
                }//if (btnListen.Text == "停止监听")
            }//if (rbtnNET.Checked)
        }

        private void StrSend(string Buf,bool IsBth)
        {
            if(BthCon != null && BthCon.IsAlive)
            {
                try
                {
                    Encoding gb = Encoding.GetEncoding("gb2312");
                    BthRecvBox.AppendText(Buf);
                    byte[] buffer = gb.GetBytes(Buf);
                    bthsocket.Send(buffer);
                    SetRS(buffer.Length, RecvOrSend.Send);
                }
                catch(Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "error");
                }
            }
        }

        private void serialPort1_Rx(object sender, SerialDataReceivedEventArgs e)
        {
            int n = serialPort1.BytesToRead;
            byte[] Buf = new byte[n];
            serialPort1.Read(Buf, 0, n);
            string str = Encoding.Default.GetString(Buf);
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

        private void ReceProcess()//UDPRecv
        {
            //int cnt = 0;
            string receiveFromOld = "";
            //string receiveFromNew = "";
            UDPResult ur;
            //定义IPENDPOINT，装载远程IP地址和端口 
            IPEndPoint remoteIpAndPort = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                try
                {
                    ur = udp.Receive(ref remoteIpAndPort, receiveFromOld);

                    receiveFromOld = ur.IPPort;
                    Peer.Text = ur.IPPort;

                    if (!string.IsNullOrWhiteSpace(ur.NewPeer))
                    {
                        RecvBox.AppendText(ur.NewPeer);
                    }

                    //界面显示

                    if (AngDirDisp.Checked)
                    {
                        SetAngDisp(ur.Message);
                    }

                    if (rbtnChar.Checked)
                    {
                        RecvBox.AppendText(ur.Message);
                        tf?.WriteLine(getTime() + ur.Message);
                    }
                    else if (rbtnHex.Checked)
                        Str2Hexstr(ur.Message);

                    //label18.Text = (int.Parse(label18.Text) + cnt).ToString();
                    SetRS(ur.Num, RecvOrSend.Recv);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "error");
                }
            }//while(True)
        }

        void AcceptInfo(object o)//TCPLink
        {

            Socket aSocket = o as Socket;

            while (true)
            {
                //创建通信用的Socket
                try
                {
                    Socket tSocket = aSocket.Accept();
                    string point = tSocket.RemoteEndPoint.ToString();

                    Infotext.Text = "连接成功！";

                    tcp.Add(point, tSocket);
                    Peer.Text = point;
                    //接收消息
                    ThreadPool.QueueUserWorkItem(ReceiveMsg, tSocket);
                    //    Thread th = new Thread(ReceiveMsg);
                    //    th.IsBackground = true;
                    //    th.Start(tSocket);
                }
                catch //(Exception ex)
                {
                    //MessageBox.Show(this, ex.Message, "error");
                    break;
                }
            }
        }

        void ReceiveMsg(object o)//TCPRecv
        {
            Socket client = o as Socket;
            while (true)
            {
                try
                {
                    byte[] buffer = new byte[1024 * 1024];
                    int n = client.Receive(buffer);
                    if (n == 0)
                    {
                        if(tcp.RemoveBySocket(client))
                        {
                            Peer.Text = "";
                        }
                        break;
                    }
                    else
                    {
                        string str = Encoding.UTF8.GetString(buffer, 0, n);
                        //label18.Text = (int.Parse(label18.Text) + n).ToString();
                        SetRS(n, RecvOrSend.Recv);
                        if (rbtnChar.Checked)
                        {
                            string EndPoint = client.RemoteEndPoint.ToString();
                            RecvBox.AppendText(EndPoint + ":\r\n" + str + "\r\n");
                            tf?.WriteLine(getTime() + EndPoint + ":\r\n" + str + "\r\n");
                        }
                        else if (rbtnHex.Checked)
                        {
                            Str2Hexstr(str);
                        }

                        if (AngDirDisp.Checked)
                        {
                            SetAngDisp(str);
                        }//if (checkBox2.Checked)
                    }//if (n == 0)
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "error");
                    if (tcp.RemoveBySocket(client))
                    {
                        Peer.Text = "";
                    }
                    break;
                }
            }//while (true)
        }

        private void BthDispBtn_Click(object sender, EventArgs e)
        {
            if (br == null)
            {
                br = BluetoothRadio.PrimaryRadio;
                if (br == null)
                {
                    BthRecvBox.AppendText("No radio hardware or unsupported software stack\r\n");
                    return;
                }
                // Warning: LocalAddress is null if the radio is powered-off.
                BthRecvBox.AppendText(String.Format("* Radio, address: {0:C}\r\n", br.LocalAddress));
                BthRecvBox.AppendText("Mode: " + br.Mode.ToString() + "\r\n");
                BthRecvBox.AppendText("Name: " + br.Name + ", LmpSubversion: " + br.LmpSubversion + "\r\n");
                BthRecvBox.AppendText("ClassOfDevice: " + br.ClassOfDevice.ToString() + ", device: " + br.ClassOfDevice.Device.ToString() + " / service: " + br.ClassOfDevice.Service.ToString() + "\r\n");

                // Enable discoverable mode
                br.Mode = RadioMode.Discoverable;
                BthRecvBox.AppendText("Radio Mode now: " + br.Mode.ToString() + "\r\n");
                BthDispBtn.Text = "隐藏设备";
            }
            else
            {
                br.Mode = RadioMode.PowerOff;
                br = null;
                BthRecvBox.AppendText("Blutooth Device is PowerDown!\r\n");
                BthDispBtn.Text = "设备可见";
            }
        }

        private void BthListenBtn_Click(object sender, EventArgs e)
        {
            if (BthCon == null && listener == null)
            {
                listener = new BluetoothListener(BluetoothService.SerialPort);
                listener.Start();
                BthRecvBox.AppendText("Service started!\r\n");
                Bthlisten = new Thread(BthListener)
                {
                    IsBackground = true
                };
                Bthlisten.Start();
                BthListenBtn.Text = "关闭连接";
            }
			else if(BthCon == null)
			{
				
			}
            else if(BthCon.IsAlive)
            {
                BthClient.Close();
                BthClient = null;
                BthCon.Abort();
                BthCon = null;
                listener.Stop();
                listener = null;
                BthListenBtn.Text = "监听";
            }
            else if(BthCon != null)
            {
                BthCon = null;
                BthClient.Close();
                BthClient = null;
                BthRecvBox.AppendText("Service started!\r\n");
                Bthlisten = new Thread(BthListener)
                {
                    IsBackground = true
                };
                Bthlisten.Start();
                BthListenBtn.Text = "关闭连接";
            }
        }

        private void BthListener()
        {
            BthClient = listener.AcceptBluetoothClient();
            BthRecvBox.AppendText("Got a request!\r\n");
            bthsocket = BthClient.Client;
            string dataToSend = "Hello from service!\r\n";
            // Convert dataToSend into a byte array
            byte[] dataBuffer = Encoding.ASCII.GetBytes(dataToSend);
            // Output data to stream
            bthsocket.Send(dataBuffer);
            SetRS(dataBuffer.Length, RecvOrSend.Send);
            BthCon = new Thread(BthRecv)
            {
                IsBackground = true
            };
            BthCon.Start(bthsocket);
        }

        private void BthRecv(object peers)
        {
            byte[] buffer = new byte[1024];
            Socket peer = (Socket)peers;
            while (true)
            {
                try
                {
                    int n = peer.Receive(buffer);
                    if (n == 0)
                    {
                        Infotext.Text = "连接断开!";
                        BthListenBtn.Text = "连接断开";
                        return;
                    } 
                    string data = Encoding.UTF8.GetString(buffer, 0, n);
                    BthRecvBox.AppendText("Receiving data: " + data + "\r\n");
                    SetRS(n, RecvOrSend.Recv);
                }
                catch//(Exception ex)
                {
                    //MessageBox.Show(this, ex.Message, "error");
                    break;
                }
            }
        }

        private void BthSendBtn_Click(object sender, EventArgs e)
        {
            if (BthCon != null && BthCon.IsAlive)
            {
                StrSend(BthSendBox.Text, true);
            }
        }

        private void HandleRequests(object that, BluetoothWin32AuthenticationEventArgs e)
        {
            e.Confirm = true;
        }

        private void BthConnectBtn_Click(object sender, EventArgs e)
        {
            BluetoothClient client = new BluetoothClient();
            BluetoothDeviceInfo[] devices = client.DiscoverDevices();
            BluetoothDeviceInfo device = null;
            foreach (BluetoothDeviceInfo d in devices)
            {
                RecvBox.AppendText(d.DeviceName + "\r\n");
                if (d.DeviceName == "HUAWEI P30")
                {
                    device = d;
                    break;
                }
            }
            if (device != null)
            {
                RecvBox.AppendText(string.Format("Name:{0} Address:{1:C}", device.DeviceName, device.DeviceAddress));
                try
                {
                    //BluetoothClient client = new BluetoothClient(this.CreateNewEndpoint(localAddress));
                    //BluetoothEndPoint ep = this.CreateNewEndpoint(device.DeviceAddress);

                    EventHandler<BluetoothWin32AuthenticationEventArgs> handler = new EventHandler<BluetoothWin32AuthenticationEventArgs>(HandleRequests);
                    BluetoothWin32Authentication auth = new BluetoothWin32Authentication(handler);

                    BluetoothSecurity.PairRequest(device.DeviceAddress, null);
                }
                catch
                {
                    return;
                }
                client.Connect(device.DeviceAddress, BluetoothService.SerialPort);
                Stream peerStream = client.GetStream();

                // Create storage for receiving data
                byte[] buffer = new byte[2000];

                // Read Data
                peerStream.Read(buffer, 0, 50);

                // Convert Data to String
                string data = Encoding.ASCII.GetString(buffer, 0, 50);
                RecvBox.AppendText("Receiving data: " + data);

                int i = 0;
                while (true)
                {
                    RecvBox.AppendText("Writing: " + i.ToString());
                    byte[] dataBuffer = Encoding.ASCII.GetBytes(i.ToString());

                    peerStream.Write(dataBuffer, 0, dataBuffer.Length);
                    ++i;
                    if (i >= int.MaxValue)
                    {
                        i = 0;
                    }
                    Thread.Sleep(500);
                }
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
                        //使用IPv4地址，流式socket方式，tcp协议传递数据
                        tcp.SetMonitorSocket(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));

                        //创建好socket后，必须告诉socket绑定的IP地址和端口号。
                        try
                        {
                            tcp.socket.Bind(LocalIPEndPoint);

                            //同一个时间点过来10个客户端，排队
                            tcp.socket.Listen(10);
                            thread = new Thread(AcceptInfo)
                            {
                                IsBackground = true
                            };
                            thread.Start(tcp.socket);

                            SocketIsOpen(true);

                            tcp.ClearPeer();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(this, ex.Message, "error");
                        }
                    }//if (comboBox3.Text == "TCP")
                    else if (NetType.Text == "UDP")
                    {
                        udp.StartUDP(LocalIPEndPoint);

                        ReceThread = new Thread(ReceProcess)
                        {
                            IsBackground = true//后台线程，前台线程GG，它也GG
                        };//线程处理程序为 ReceProcess
                        ReceThread.Start();

                        SocketIsOpen(true);
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
                    SocketIsOpen(false);
                    try
                    {
                        //comboBox3.Text = "";
                        tcp.socket.Close();
                        if (thread.IsAlive)
                            thread.Abort();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, ex.Message, "error");
                    }
                }
                else if (NetType.Text == "UDP")
                {
                    udp.CloseUDP();
                    //关闭 线程
                    ReceThread.Abort();

                    SocketIsOpen(false);
                    //comboBox3.Text = "";
                }

            }//if (button9.Text == "停止监听")
        }

        private void SocketIsOpen(bool isOpen)
        {
            if(isOpen)
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
                        Encoding gb = Encoding.GetEncoding("gb2312");
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
                                    byte[] buffer = Encoding.UTF8.GetBytes(Buf);
                                    tcp.Send(ip, buffer);
                                    SetRS(Buf.Length, RecvOrSend.Send);
                                    if (ReDisp.Checked)
                                    {
                                        RecvBox.AppendText(Buf);
                                    }
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
                                UDPResult result = udp.UDP_Send(Buf, Peer.Text);
                                SetInfoAfterUDPSend(result);
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

        private void Peer_DropDown(object sender, EventArgs e)
        {
            if (btnListen.Text == "停止监听")
            {
                if (NetType.Text == "UDP")
                {
                    Peer.Items.Clear();
                    Peer.Items.AddRange(udp.GetUDPPeer());
                }
                else if (NetType.Text == "TCP")
                {
                    Peer.Items.Clear();
                    Peer.Items.AddRange(tcp.GetAllPeerIP());
                }
            }
        }

        private void SaveData_CheckedChanged(object sender, EventArgs e)
        {
            if(SaveData.Checked)
            {
                DialogResult dr = openFileDialog1.ShowDialog();
                if (dr == DialogResult.OK)
                {
                    tf = new StreamWriter(@openFileDialog1.FileName, true);
                }
                else
                    SaveData.CheckState = CheckState.Unchecked;
            }
            else if(!SaveData.Checked)
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
            MatchCollection mc = Regex.Matches(Port.Text,"[0-9]");
            int num=0;
            foreach (Match s in mc)
            {
                num=num*10+int.Parse(s.Value);
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
    }//public partial class Form1 : Form
}//namespace WindowsFormsApp1
