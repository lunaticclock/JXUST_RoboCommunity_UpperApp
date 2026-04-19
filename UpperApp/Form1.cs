//#define Firewall
#define File

using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Windows.Forms;
using Button = System.Windows.Forms.Button;
using TextBox = System.Windows.Forms.TextBox;
#if Firewall
    using NetFwTypeLib;
#endif

namespace UpperApp
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    public partial class UpperApp : Form
    {
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
        private CheckBox[] checks;
        private TextBox[] texts;
        private Button[] btn;
        private StreamWriter tf = null;
        private INetworkManager network;
        private BthManager bth = new BthManager();
        private SerManager ser = new SerManager();
        private MessageProcessor _msgProcessor;

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

            SerPortItem.DropDown += new EventHandler((sender, e) =>
            {
                SerPortItem.Items.Clear();
                SerPortItem.Items.AddRange(SerialPort.GetPortNames());
            });
            HostIP.DropDown += new EventHandler((sender, e) =>
            {
                HostIP.Items.Clear();
                GetLocalIP();
            });

            checks = [MsgHex1, MsgHex2, MsgHex3, MsgHex4, MsgHex5, MsgHex6, MsgHex7, MsgHex8];
            texts = [MsgBox1, MsgBox2, MsgBox3, MsgBox4, MsgBox5, MsgBox6, MsgBox7, MsgBox8];
            btn = [btnMsg1, btnMsg2, btnMsg3, btnMsg4, btnMsg5, btnMsg6, btnMsg7, btnMsg8];
            for (int i = 0; i < btn.Length; i++)
            {
                int index = i;
                btn[i].Click += (s, e) => btn_Click(index);
            }
            //*/
            Text = proname + prover;

            string[] Com = { "9600", "19200", "38400", "115200", "256000", "460800", "512000", "921600" };
            Baud.Items.AddRange(Com);
            Baud.SelectedIndex = 3;

            string[] NetType = ["TCP", "UDP"];
            this.NetType.Items.AddRange(NetType);
            this.NetType.SelectedIndex = 0;
            Tim.Text = "1000";
            GetLocalIP();
            ChoseSlaveBthList.DataSource = bth.BthClients.connectionKeys;

            _msgProcessor = new MessageProcessor(
                setRs: SetRS,
                isCharMode: () => rbtnChar.Checked,
                isHexMode: () => rbtnHex.Checked,
                isReDisp: () => ReDisp.Checked,
                appendToRecvBox: (text) => RecvBox.AppendText(text),
                writeLog: (log) => tf?.WriteLine(log),
                setAngDisp: SetAngDisp,
                isAngDirDispEnabled: () => AngDirDisp.Checked,
                onNewPeer: (newPeer) => { /* 可选：更新 UI 中的对端显示 */ }
            );
            bth.StatusChanged += UnifiedStatusChanged;
            ser.StatusChanged += UnifiedStatusChanged;

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

        private void UnifiedStatusChanged(Result status)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => UnifiedStatusChanged(status)));
                return;
            }

            switch (status.NetStatus)
            {
                case Result.NETStatus.ReciveMessage:
                    _msgProcessor.ProcessReceivedMessage(status);
                    break;
                case Result.NETStatus.SendMessage:
                    _msgProcessor.ProcessSentMessage(status);
                    break;
                case Result.NETStatus.ExceptionStop:
                    // 异常提示框
                    if (status.Channel == ChannelType.Serial)
                        MessageBox.Show(this, status.Message, "串口错误");
                    else if (status.Channel == ChannelType.Bluetooth)
                        MessageBox.Show(this, status.Message, "蓝牙错误");
                    else if (status.Channel == ChannelType.TCP || status.Channel == ChannelType.UDP)
                        if (!status.Message.Contains("你的主机中的软件中止了一个已建立的连接"))
                            MessageBox.Show(this, status.Message, "网络错误");
                    break;
                case Result.NETStatus.MonitorStart:
                case Result.NETStatus.MonitorStop:
                    UpdateMonitorUI(status);
                    break;
                case Result.NETStatus.NewRemote:
                    // 新连接（TCP/UDP 服务器收到客户端连接，或蓝牙连接）
                    if (status.Channel == ChannelType.TCP || status.Channel == ChannelType.UDP)
                    {
                        Infotext.Text = "连接成功！";
                        Peer.Text = status.Message;   // status.Message 为远程终结点
                    }
                    else if (status.Channel == ChannelType.Bluetooth)
                    {
                        BthRecvBox.AppendText(status.Message);
                        // 可选：刷新蓝牙客户端列表
                        ChoseSlaveBthList.DataSource = null;
                        ChoseSlaveBthList.DataSource = bth.BthClients.connectionKeys;
                    }
                    break;
                case Result.NETStatus.RemoteStop:
                    // 远端断开连接
                    if (status.Channel == ChannelType.TCP || status.Channel == ChannelType.UDP)
                    {
                        Peer.Text = "";
                        Infotext.Text = status.Message;
                    }
                    else if (status.Channel == ChannelType.Bluetooth)
                    {
                        Infotext.Text = "连接断开!";
                        BthConnectBtn.Text = "连接";
                        // 可选：刷新蓝牙客户端列表
                        ChoseSlaveBthList.DataSource = null;
                        ChoseSlaveBthList.DataSource = bth.BthClients.connectionKeys;
                    }
                    break;
                case Result.NETStatus.ManualStop:
                    // 手动停止（目前仅 TCP/UDP 需要清空 Peer）
                    if (status.Channel == ChannelType.TCP || status.Channel == ChannelType.UDP)
                        Peer.Text = "";
                    break;
                default:
                    // 其他状态按需处理
                    break;
            }
        }

        private void UpdateMonitorUI(Result status)
        {
            bool isStart = (status.NetStatus == Result.NETStatus.MonitorStart);

            switch (status.Channel)
            {
                case ChannelType.Serial:
                    btnSerial.Text = isStart ? "关闭串口" : "打开串口";
                    SerPortItem.Enabled = !isStart;
                    Baud.Enabled = !isStart;
                    if (isStart) rbtnSerial.Checked = true;
                    Infotext.Text = status.Message;
                    break;
                case ChannelType.TCP:
                case ChannelType.UDP:
                    btnListen.Text = isStart ? "停止监听" : "开始监听";
                    NetType.Enabled = !isStart;
                    HostIP.Enabled = !isStart;
                    if (isStart)
                    {
                        rbtnNET.Checked = true;
                        Peer.Text = "";
                    }
                    Infotext.Text = status.Message;
                    break;
                case ChannelType.Bluetooth:
                    BthListenBtn.Text = isStart ? "关闭" : "监听";
                    // 可添加蓝牙相关控件启用/禁用
                    if (isStart)
                    {
                        BthRecvBox.AppendText("Service started!\r\n");
                    }
                    break;
                default:
                    break;
            }
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

        private void filerd()
        {
#if File
            Utils.LoadMessagesFromFile("Buffer.log", checks, texts);
#else
            string port = Utils.LoadMessagesFromRegistry(@"SOFTWARE\JXUST", checks, texts);
            if (port != null) Port.Text = port;
#endif
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

        private void UpperApp_Load(object sender, EventArgs e)
        {
            FBtext.DataBindings.Add("Text", FBBar, "Value", false, DataSourceUpdateMode.OnPropertyChanged);
            RLtext.DataBindings.Add("Text", RLBar, "Value", false, DataSourceUpdateMode.OnPropertyChanged);

        }

        public void GetLocalIP()
        {
            HostIP.Items.Clear();
            foreach (string ip in Utils.GetLocalIPv4Addresses())
            {
                HostIP.Items.Add(ip);
            }
        }

        //发送数据  串口TCP和UDP
        private void StrSend(string Buf)
        {
            if (rbtnSerial.Checked)
            {
                ser.Send(Buf);
            }
            else if (rbtnNET.Checked)
            {
                if (network != null && network.IsMonitoring)
                {
                    try
                    {
                        string ip = Peer.Text;
                        if (ip == "")
                            Infotext.Text = "未选定端口";
                        else
                        {
                            network.Send(Buf, ip);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, ex.Message, "error");
                        network.Remove(Peer.Text);
                        Peer.Text = "";
                    }
                }
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
                if (!bth.IsMonitoring)
                {
                    bth.StartMonitor();
                    BthRecvBox.AppendText("Service started!\r\n");
                    BthListenBtn.Text = "关闭";
                }
                else
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
                bth.SetClient(bth.BthDevices[BthDeviceList.SelectedItem.ToString()]);
                BthConnectBtn.Text = "断开";
            }
        }

        private void btnListen_Click(object sender, EventArgs e)
        {
            if (btnListen.Text == "开始监听")
            {
                if (string.IsNullOrEmpty(HostIP.Text))
                {
                    Infotext.Text = "请选择IP";
                    return;
                }
                if (!IPAddress.TryParse(HostIP.Text, out var ip) || !int.TryParse(Port.Text, out var port) || port <= 0 || port > 65535)
                {
                    Infotext.Text = "IP或端口无效";
                    return;
                }
                var localEndpoint = new IPEndPoint(ip, port);
                network = NetType.Text == "TCP" ? new TcpManagerAdapter() : new UdpManagerAdapter();
                network.StatusChanged += UnifiedStatusChanged;
                try
                {
                    network.StartMonitor(localEndpoint);
                    Peer.DataSource = network.GetPeerDataSource();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "错误");
                }
            }//if (button9.Text == "开始监听")
            else if (btnListen.Text == "停止监听")
            {
                try
                {
                    network.StopMonitor();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "错误");
                }

            }//if (button9.Text == "停止监听")
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
            if (string.IsNullOrEmpty(SerPortItem.Text))
            {
                Infotext.Text = "未选中串口";
                return;
            }

            if (!ser.IsMonitoring)
            {
                int baudRate = 115200;
                if (!string.IsNullOrEmpty(Baud.Text))
                    int.TryParse(Baud.Text, out baudRate);

                ser.StartMonitor(SerPortItem.Text, baudRate);
            }
            else
            {
                ser.StopMonitor();
            }
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
                    try
                    {
                        StrSend($"FR:{FBBar.Value}:{RLBar.Value}:OVER\r\n");
                    }
                    catch (Exception ex)
                    {
                        Rocker.Text = "摇杆开";
                        setV(50, 50);
                        MessageBox.Show(this, ex.Message, "error");
                        network.Remove(Peer.Text);
                        Peer.Text = "";
                    }
                    xs = RLBar.Value;
                    ys = FBBar.Value;
                }
            }
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
            Port.Text = Utils.ValidatePortInput(Port.Text);
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

        private void UpperApp_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 1. 保存配置（根据编译符号选择文件或注册表）
#if File
            using (StreamWriter sw = new StreamWriter("Buffer.log", false))
            {
                for (int i = 0; i < checks.Length; i++)
                {
                    string suffix = checks[i].Checked ? "H" : "A";
                    sw.WriteLine($"U{i + 1};{texts[i].Text};{suffix}");
                }
            }
#else
            Utils.SaveMessagesToRegistry(@"SOFTWARE\JXUST", checks, texts, Port.Text);
#endif

            // 2. 取消事件订阅（避免内存泄漏）
            bth.StatusChanged -= UnifiedStatusChanged;
            ser.StatusChanged -= UnifiedStatusChanged;

            // 3. 异步释放资源（避免阻塞 UI 线程）
            _ = DisposeManagersAsync();

            // 4. 关闭日志文件
            tf?.Close();
        }

        private async Task DisposeManagersAsync()
        {
            // 并行释放多个管理器，提高效率
            await Task.WhenAll(
                network?.DisposeAsync().AsTask(),
                bth?.DisposeAsync().AsTask(),
                ser?.DisposeAsync().AsTask()
            ).ConfigureAwait(false);
        }

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
            string text = texts[i].Text;
            if (string.IsNullOrWhiteSpace(text)) return;

            string sendStr;
            if (checks[i].Checked)
            {
                sendStr = Utils.HexStringToString(text);
                if (sendStr == null)
                {
                    MessageBox.Show(this, "请将每字节间以空格分开");
                    return; // 转换失败，不发送
                }
            }
            else
            {
                sendStr = text;
            }
            StrSend(sendStr);
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
            Utils.SnapToScreenEdge(this);
        }

        private async void BthDeviceScanBtn_Click(object sender, EventArgs e)
        {
            if (!BthDeviceScanBtn.Enabled) return;
            BthDeviceScanBtn.Enabled = false;
            BthDeviceScanBtn.Text = "扫描中";
            try
            {
                List<BluetoothDeviceInfo> devices = await bth.DiscoverDevicesAsync();
                BthDeviceList.Items.Clear();
                foreach (var device in devices)
                {
                    bth.BthDevices.TryAdd(device.DeviceName, device);
                    if (!BthDeviceList.Items.Contains(device.DeviceName))
                        BthDeviceList.Items.Add(device.DeviceName);
                }
                BthRecvBox.AppendText($"扫描完成，发现 {devices.Count} 个设备。\r\n");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"扫描失败: {ex.Message}");
            }
            finally
            {
                BthDeviceScanBtn.Enabled = true;
                BthDeviceScanBtn.Text = "扫描蓝牙";
            }
        }
    }//public partial class Form1 : Form
}//namespace WindowsFormsApp1
