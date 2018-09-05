using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
//using NetFwTypeLib;
//using System.Runtime.InteropServices;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {

        string Buffer="";
    //    List<byte> buffer = new List<byte>(4096);
        Point p;
    //    int n = 0;
        string xs="50";
        string ys="50";
        string peerip;
        Thread th = null;
        Thread thread = null;
        UdpClient m_UdpClientSend = null;
        UdpClient m_UdpClientReceive = null;
        Thread m_ReceThread = null;
        //Socket client;

        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            string[] Com = new string[8];
            Com[0] = "9600";
            Com[1] = "19200";
            Com[2] = "38400";
            Com[3] = "115200";
            Com[4] = "256000";
            Com[5] = "460800";
            Com[6] = "512000";
            Com[7] = "921600";
            comboBox2.Items.AddRange(Com);
            string[] NetType = new string[2];
            NetType[0] = "TCP";
            NetType[1] = "UDP";
            comboBox3.Items.AddRange(NetType);
            serialPort1.DataBits = 8;
            serialPort1.Parity = System.IO.Ports.Parity.None;
            serialPort1.Handshake = System.IO.Ports.Handshake.None;
            serialPort1.StopBits = System.IO.Ports.StopBits.One;
            serialPort1.BaudRate = 115200;
            serialPort1.NewLine = "/r/n";
            serialPort1.Encoding = System.Text.Encoding.GetEncoding("GB2312");
            //string[] Port = System.IO.Ports.SerialPort.GetPortNames();
            //comboBox1.Items.AddRange(Port);
            textBox3.Text = "1000";
            GetLocalIP();
            if (!File.Exists("Buffer.log"))
            {
                FileStream fs = new FileStream("Buffer.log", FileMode.OpenOrCreate);
                fs.Close();
            }
            filerd();
            //NetFwAddApps(proname, Application.ExecutablePath);
        }
        /*
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
        }*/

        private void filerd()
        {
            string data = string.Empty;
            string addr = string.Empty;
            string hex = string.Empty;
            StreamReader sr = new StreamReader("Buffer.log", true);
            data = sr.ReadLine();
            while(data != null)
            {
                int num = data.IndexOf(';');
                addr = data.Substring(0, num);
                data = data.Remove(0, num + 1);
                num = data.IndexOf(';');
                hex = data.Remove(0, num + 1);
                data = data.Substring(0, num);
                switch (addr)
                {
                    case "U1": textBox5.Text = data;
                        if (hex == "H")
                            checkBox4.CheckState = System.Windows.Forms.CheckState.Checked;
                        else if (hex == "A")
                            checkBox4.CheckState = System.Windows.Forms.CheckState.Unchecked;
                        break;
                    case "U2": textBox9.Text = data;
                        if (hex == "H")
                            checkBox5.CheckState = System.Windows.Forms.CheckState.Checked;
                        else if (hex == "A")
                            checkBox5.CheckState = System.Windows.Forms.CheckState.Unchecked;
                        break;
                    case "U3": textBox10.Text = data;
                        if (hex == "H")
                            checkBox6.CheckState = System.Windows.Forms.CheckState.Checked;
                        else if (hex == "A")
                            checkBox6.CheckState = System.Windows.Forms.CheckState.Unchecked;
                        break;
                    case "U4": textBox11.Text = data;
                        if (hex == "H")
                            checkBox7.CheckState = System.Windows.Forms.CheckState.Checked;
                        else if (hex == "A")
                            checkBox7.CheckState = System.Windows.Forms.CheckState.Unchecked;
                        break;
                    case "U5": textBox12.Text = data;
                        if (hex == "H")
                            checkBox8.CheckState = System.Windows.Forms.CheckState.Checked;
                        else if (hex == "A")
                            checkBox8.CheckState = System.Windows.Forms.CheckState.Unchecked;
                        break;
                    case "U6": textBox13.Text = data;
                        if (hex == "H")
                            checkBox9.CheckState = System.Windows.Forms.CheckState.Checked;
                        else if (hex == "A")
                            checkBox9.CheckState = System.Windows.Forms.CheckState.Unchecked;
                        break;
                    case "U7": textBox14.Text = data;
                        if (hex == "H")
                            checkBox10.CheckState = System.Windows.Forms.CheckState.Checked;
                        else if (hex == "A")
                            checkBox10.CheckState = System.Windows.Forms.CheckState.Unchecked;
                        break;
                    case "U8": textBox15.Text = data;
                        if (hex == "H")
                            checkBox11.CheckState = System.Windows.Forms.CheckState.Checked;
                        else if (hex == "A")
                            checkBox11.CheckState = System.Windows.Forms.CheckState.Unchecked;
                        break;
                    default: break;
                }
                data = sr.ReadLine();
            }
            sr.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            serialPort1.DataReceived += new SerialDataReceivedEventHandler(serialPort1_Rx);//必须手动添加事件处理程序
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
                        comboBox4.Items.Add(IpEntry.AddressList[i].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("获取本机IP出错:" + ex.Message);
            }
        }

        Socket socket = null;

        private void btnListen_Click(object sender, EventArgs e)
        {
            
            if (button9.Text == "开始监听")
            {
                if (comboBox3.Text == "TCP")
                {
                    if (comboBox4.Text != "")
                    {
                        IPAddress ip = IPAddress.Parse(comboBox4.Text);

                        // IPAddress ip = IPAddress.Any;

                        //端口号

                        IPEndPoint point = new IPEndPoint(ip, int.Parse(textBox6.Text));

                        //创建监听用的Socket

                        /*

                            * AddressFamily.InterNetWork：使用 IP4地址。

            SocketType.Stream：支持可靠、双向、基于连接的字节流，而不重复数据。此类型的 Socket 与单个对方主机进行通信，并且在通信开始之前需要远程主机连接。Stream 使用传输控制协议 (Tcp) ProtocolType 和 InterNetworkAddressFamily。

            ProtocolType.Tcp：使用传输控制协议。

                            */

                        //使用IPv4地址，流式socket方式，tcp协议传递数据

                        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                        //创建好socket后，必须告诉socket绑定的IP地址和端口号。

                        //让socket监听point
                        try
                        {

                            //socket监听哪个端口

                            socket.Bind(point);

                            //同一个时间点过来10个客户端，排队

                            socket.Listen(10);

                            label1.Text = "开始监听";
                            button9.Text = "停止监听";

                            thread = new Thread(new ParameterizedThreadStart(AcceptInfo));

                            thread.IsBackground = true;

                            thread.Start(socket);
                            comboBox3.Enabled = false;
                            comboBox4.Enabled = false;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(this, ex.Message, "error");
                        }
                    }
                    else
                        label1.Text = "请选择IP";
                }
                else if (comboBox3.Text == "UDP")
                {
                    if (comboBox4.Text != "")
                    {
                        IPAddress LocalIP = IPAddress.Parse(comboBox4.Text);//本地IP
                        int LocalPort = Convert.ToInt32(textBox6.Text);//本地Port
                        IPEndPoint m_LocalIPEndPoint = new IPEndPoint(LocalIP, LocalPort);//本地IP和Port



                        //Bind
                        m_UdpClientSend = new UdpClient(LocalPort);//Bind Send UDP = Local some IP&Port
                        m_UdpClientReceive = new UdpClient(m_LocalIPEndPoint);//Bind Receive UDP = Local IP&Port

                        /*
                        发送的UdpClient对象是m_UdpClientSend，绑定的地址是 0.0.0.0:8010
                        接收的UdpClient对象是m_UdpClientReceive，绑定的地址是 10.13.68.220:8010
                        */


                        //============================
                        //Start UDP Receive Thread
                        //============================
                        m_ReceThread = new Thread(new ThreadStart(ReceProcess));//线程处理程序为 ReceProcess
                        m_ReceThread.IsBackground = true;//后台线程，前台线程GG，它也GG
                        m_ReceThread.Start();

                        //============================
                        //界面处理
                        //============================
                        label1.Text = "开始监听";
                        button9.Text = "停止监听";
                        comboBox3.Enabled = false;
                        comboBox4.Enabled = false;
                    }
                    else
                        label1.Text = "请选择IP";
                }
                else
                    label1.Text = "请选择模式";
            }
            else if (button9.Text == "停止监听")
            {
                if (comboBox3.Text == "TCP")
                {
                    button9.Text = "开始监听";
                    label1.Text = "停止监听";
                    //socket.Close();
                    try
                    {
                        comboBox3.Enabled = true;
                        comboBox4.Enabled = true;
                        //comboBox3.Text = "";
                        socket.Close();
                        thread.Abort();
                        th.Abort();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, ex.Message, "error");
                    }
                }
                else if (comboBox3.Text == "UDP")
                {//已经绑定

                    //关闭 UDP
                    m_UdpClientSend.Close();
                    m_UdpClientReceive.Close();

                    //关闭 线程
                    m_ReceThread.Abort();

                    //界面处理
                    button9.Text = "开始监听";
                    label1.Text = "停止监听";
                    comboBox3.Enabled = true;
                    comboBox4.Enabled = true;
                    //comboBox3.Text = "";
                }

            }
        }

        private void Str2Int(string str)
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
            textBox1.AppendText(result);
        }

        private void ReceProcess()
        {
            int cnt = 0;
            string receiveFromOld = "";
            string receiveFromNew = "";

            //定义IPENDPOINT，装载远程IP地址和端口 
            IPEndPoint remoteIpAndPort = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                byte[] ReceiveBytes = m_UdpClientReceive.Receive(ref remoteIpAndPort);

                cnt = ReceiveBytes.Length;
                receiveFromNew = remoteIpAndPort.ToString();
                if (!receiveFromNew.Equals(receiveFromOld))
                {
                    receiveFromOld = receiveFromNew;
                    string str_From = String.Format("\r\nfrom {0}:\r\n", receiveFromNew);
                    int num = receiveFromNew.IndexOf(':');
                    string data = receiveFromNew.Substring(0, num);
                    textBox7.Text = data;
                    data = receiveFromNew.Remove(0, num+1);
                    textBox8.Text = data;
                    textBox1.AppendText(str_From);
                }

                string str = System.Text.Encoding.Default.GetString(ReceiveBytes, 0, cnt);

                //界面显示
                if (radioButton2.Checked)
                {
                    textBox1.AppendText(str);
                    if (checkBox2.Checked)
                    {
                        if (str.Contains("/OVER"))
                        {
                            int num = str.IndexOf(':');
                            string data = str.Remove(0, num + 1);
                            num = data.IndexOf('/');
                            data = data.Substring(0, num);
                            if (str.Contains("YAW:"))
                                label11.Text = data.ToString();
                            else if (str.Contains("PITCH:"))
                                label13.Text = data.ToString();
                            else if (str.Contains("ROLL:"))
                                label12.Text = data.ToString();
                            else if (str.Contains("DISTANCE:"))
                                label14.Text = data.ToString();
                        }
                    }
                }
                else if (radioButton1.Checked)
                    Str2Int(str);
                label18.Text = (Convert.ToUInt32(label18.Text) + cnt).ToString();
            }
        }

        Dictionary<string, Socket> dic = new Dictionary<string, Socket>();
  
         // private Socket client;
  
        void AcceptInfo(object o)
  
        {
  
            Socket socket = o as Socket;
  
            while (true)
  
            {

            //通信用socket

            //创建通信用的Socket
                try
                {
                    Socket tSocket = socket.Accept();

                    string point = tSocket.RemoteEndPoint.ToString();

                    //IPEndPoint endPoint = (IPEndPoint)client.RemoteEndPoint;

                    //string me = Dns.GetHostName();//得到本机名称

                    //MessageBox.Show(me);

                    label1.Text = "连接成功！";

                    peerip = point;

                    dic.Add(point, tSocket);

                    //接收消息

                    th = new Thread(ReceiveMsg);

                    th.IsBackground = true;

                    th.Start(tSocket);
                }
                catch //(Exception ex)
                {
                    //MessageBox.Show(this, ex.Message, "error");
                    break;
                }
            }
        }

        //接收消息
        void ReceiveMsg(object o)
 
         {
 
             Socket client = o as Socket;

            while (true)
 
            {

            //接收客户端发送过来的数据

                try
                {
                    //定义byte数组存放从客户端接收过来的数据

                    byte[] buffer = new byte[1024 * 1024];

                    //将接收过来的数据放到buffer中，并返回实际接受数据的长度

                    int n = client.Receive(buffer);

                    //将字节转换成字符串
                    if (n == 0)
                    {
                        break;
                    }
                    else
                    {
                        string str = Encoding.UTF8.GetString(buffer, 0, n);
                        label18.Text = (Convert.ToUInt32(label18.Text) + n).ToString();
                        if (radioButton2.Checked)
                        {
                            textBox1.AppendText(client.RemoteEndPoint.ToString() + ":\r\n" + str + "\r\n");
                        }
                        else if (radioButton1.Checked)
                        {
                            Str2Int(str);
                        }

                        if (checkBox2.Checked)
                        {
                            if (str.Contains("/OVER"))
                            {
                                int num = str.IndexOf(':');
                                string data = str.Remove(0, num+1);
                                num = data.IndexOf('/');
                                data = data.Substring(0, num);
                                if (str.Contains("YAW:"))
                                    label11.Text = data.ToString();
                                else if (str.Contains("PITCH:"))
                                    label13.Text = data.ToString();
                                else if (str.Contains("ROLL:"))
                                    label12.Text = data.ToString();
                                else if (str.Contains("DISTANCE:"))
                                    label14.Text = data.ToString();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "error");
                    break;
                }
            }
        }

        private void UDP_Send(string Buf)
        {
            IPAddress RemoteIP;   //远端 IP                
            int RemotePort;      //远端 Port
            IPEndPoint RemoteIPEndPoint; //远端 IP&Port

            if (IPAddress.TryParse(textBox7.Text, out RemoteIP) == false)//远端 IP
            {
                label1.Text = "远端IP错误!";
                return;
            }
            RemotePort = Convert.ToInt32(textBox8.Text);//远端 Port
            RemoteIPEndPoint = new IPEndPoint(RemoteIP, RemotePort);//远端 IP和Port


            //Get Data
            byte[] sendBytes = System.Text.Encoding.Default.GetBytes(Buf);
            int cnt = sendBytes.Length;

            if (0 == cnt)
            {
                return;
            }

            //Send
            m_UdpClientSend.Send(sendBytes, cnt, RemoteIPEndPoint);
            //下面的代码也可以，但是接收和发送分开，更好
            //m_UdpClientReceive.Send(sendBytes, cnt, RemoteIPEndPoint);

            //CNT
            label22.Text = (Convert.ToUInt32(label22.Text) + cnt).ToString();

            if (checkBox3.Checked)
                textBox1.AppendText(Buf);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string Buf = textBox2.Text; 
            Encoding gb = System.Text.Encoding.GetEncoding("gb2312");
            StrSend(Buf, gb);
        }

        int Cnt = 0,Counter;
        private void timer1_Tick(object sender, EventArgs e)
        {
            Cnt++;
            label1.Text = Cnt.ToString();
            if(checkBox1.Checked)
                if(Cnt >= Counter)
                {
                    Encoding gb = System.Text.Encoding.GetEncoding("gb2312");
                    StrSend(label1.Text, gb);
                    Cnt = 0;
                }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (button2.Text == "开始")
            {
                if (button3.Text != "打开串口" || button9.Text != "开始监听")
                {
                    Counter = (Convert.ToInt32(textBox3.Text, 10) / 100);
                    if (Counter < 1)
                    {
                        label1.Text = "时间间隔过小";
                        textBox3.Text = "1000";
                        Counter = 10;
                    }
                    timer1.Start(); //开启定时器
                    button2.Text = "停止";
                }
                else
                    label1.Text = "端口未打开!";
            }
            else
            {
                timer1.Stop(); //停止定时器
                button2.Text = "开始";
            }
        }

        private void serialPort1_Rx(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
    //        string str = serialPort1.ReadExisting();
    //        string str = serialPort1.Read();
            int n = serialPort1.BytesToRead;
            byte[] Buf = new byte[n];
            serialPort1.Read(Buf, 0, n);
            string str=System.Text.Encoding.Default.GetString(Buf);
            label18.Text = (Convert.ToUInt32(label18.Text) + str.Length).ToString();

            if (checkBox2.Checked)
            {
                if (str.Contains("/OVER"))
                {
                    int num = str.IndexOf(':');
                    string data = str.Remove(0, num + 1);
                    num = data.IndexOf('/');
                    data = data.Substring(0, num);
                    if (str.Contains("YAW:"))
                        label11.Text = data.ToString();
                    else if (str.Contains("PITCH:"))
                        label13.Text = data.ToString();
                    else if (str.Contains("ROLL:"))
                        label12.Text = data.ToString();
                    else if (str.Contains("DISTANCE:"))
                        label14.Text = data.ToString();
                }
            }
            
            if (radioButton2.Checked)
            {
                textBox1.AppendText(str);
            }
            else if (radioButton1.Checked)
            {
                Str2Int(str);
                /*
                string result = string.Empty;
                for (int i = 0; i < str.Length; i++)//逐字节变为16进制字符，以%隔开
                {
                    string add = Convert.ToString(str[i], 16).ToUpper();
                    if (add.Length == 1)
                        add = "0" + add;
                    result += " " + add;
                }
                result += "\r\n";
                textBox1.AppendText(result);*/
            }
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            
            Buffer = textBox2.Text;
        }

        private void comboBox2_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if(comboBox2.Text != "")
                serialPort1.BaudRate = (Convert.ToInt32(comboBox2.Text, 10));
            else
                serialPort1.BaudRate = 115200;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text != "")
                if (button3.Text == "打开串口")
                {
                    serialPort1.Open();
                    button3.Text = "关闭串口";
                    label1.Text = "串口打开！";
                    comboBox1.Enabled = false;
                    comboBox2.Enabled = false;
                }
                else
                {
                    serialPort1.Close();
                    if (!serialPort1.IsOpen)
                        label1.Text = "串口关闭！";
                    button3.Text = "打开串口";
                    comboBox1.Enabled = true;
                    comboBox2.Enabled = true;
                }
            else
                label1.Text = "未选中串口";
        }

        private void comboBox1_Leave(object sender, EventArgs e)
        {
            if(comboBox1.Text != "")
                serialPort1.PortName = comboBox1.Text;
        }

        private void comboBox1_DropDown(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            string[] Port = System.IO.Ports.SerialPort.GetPortNames();
            comboBox1.Items.AddRange(Port);
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
                label5.Text = Convert.ToString(trackBar1.Value);
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            label7.Text = Convert.ToString(trackBar2.Value);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            trackBar2.Value = 50;
            label7.Text = Convert.ToString(trackBar2.Value);
            string Buf = "RL:" + label7.Text + ":OVER\r\n";
            Encoding gb = System.Text.Encoding.GetEncoding("gb2312");
            StrSend(Buf, gb);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            trackBar1.Value = 50;
            label5.Text = Convert.ToString(trackBar1.Value);
            string Buf = "FB:" + label5.Text + ":OVER\r\n";
            Encoding gb = System.Text.Encoding.GetEncoding("gb2312");
            StrSend(Buf, gb);
        }

        private void trackBar1_MouseUp(object sender, MouseEventArgs e)
        {
            string Buf = "FB:" + label5.Text + ":OVER\r\n";
            Encoding gb = System.Text.Encoding.GetEncoding("gb2312");
            StrSend(Buf, gb);
        }

        private void trackBar2_MouseUp(object sender, MouseEventArgs e)
        {
            string Buf = "RL:" + label7.Text + ":OVER\r\n";
            Encoding gb = System.Text.Encoding.GetEncoding("gb2312");
            StrSend(Buf, gb);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            label18.Text = "0";
            label1.Text = "接收区已清空";
        }

        private void label1_DoubleClick(object sender, EventArgs e)
        {
            MessageBox.Show(("Project Version " + prover + " By ClockSR\r\n                                       2018.8.25"), this.Text);
        }

        private void textBox4_Leave(object sender, EventArgs e)
        {
            trackBar1.SmallChange = Convert.ToInt32(textBox4.Text, 10);
            trackBar2.SmallChange = Convert.ToInt32(textBox4.Text, 10);
        }

        public static void Delay(int milliSecond)
        {
            int start = Environment.TickCount;
            while (Math.Abs(Environment.TickCount - start) < milliSecond)
                Application.DoEvents();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (button8.Text == "摇杆开")
            {
                p = this.PointToClient(Control.MousePosition);
                p.X = 742;
                p.Y = 110;
                button8.Text = "摇杆关";
            }
            else
            {
                button8.Text = "摇杆开";
                trackBar1.Value = 50;
                label5.Text = Convert.ToString(trackBar1.Value);
                trackBar2.Value = 50;
                label7.Text = Convert.ToString(trackBar2.Value);
                string Buf = "FR:" + label5.Text + ":" + label7.Text + ":OVER\r\n";
                Encoding gb = System.Text.Encoding.GetEncoding("gb2312");
                StrSend(Buf,gb);
            }
        }

        private void button8_MouseMove(object sender, MouseEventArgs e)
        {
            if (button8.Text == "摇杆关")
            {
                Point move = this.PointToClient(Control.MousePosition);
                int x = (int)((double)(move.X - p.X) / 1.4) + 50;
                int y = ((int)(-(double)(move.Y - p.Y) / 1.5) + 50);

                if (x < 0)
                    x = 0;
                else if (x > 100)
                    x = 100;
                if (y < 0)
                    y = 0;
                else if (y > 100)
                    y = 100;
            
                if (x % 5 == 0)
                {
                    label7.Text = x.ToString();
                    trackBar2.Value = x;
                }
                if (y % 5 == 0)
                {
                    label5.Text = y.ToString();
                    trackBar1.Value = y;
                }
                if (xs != label7.Text || ys != label5.Text)
                {
                    string Buf = "FR:" + label5.Text + ":" + label7.Text + ":OVER\r\n";
                    if (radioButton4.Checked)
                    {
                        if (serialPort1.IsOpen)
                        {
                            Encoding gb = System.Text.Encoding.GetEncoding("gb2312");
                            byte[] bytes = gb.GetBytes(Buf);
                            serialPort1.Write(bytes, 0, bytes.Length);
                            label22.Text = (Convert.ToUInt32(label22.Text) + Buf.Length).ToString();
                            if (checkBox3.Checked)
                            {
                                textBox1.AppendText(Buf);
                            }
                        }
                    }
                    else if (radioButton3.Checked)
                    {
                        if (button9.Text == "停止监听")
                        {
                            if (comboBox3.Text == "TCP")
                            {
                                try
                                {
                                    if (th.IsAlive)
                                    {
                                        string ip = peerip;

                                        byte[] buffer = Encoding.UTF8.GetBytes(Buf);

                                        dic[ip].Send(buffer);
                                        label22.Text = (Convert.ToUInt32(label22.Text) + Buf.Length).ToString();
                                        if (checkBox3.Checked)
                                        {
                                            textBox1.AppendText(Buf);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    button8.Text = "摇杆开";
                                    trackBar1.Value = 50;
                                    label5.Text = Convert.ToString(trackBar1.Value);
                                    trackBar2.Value = 50;
                                    label7.Text = Convert.ToString(trackBar2.Value);
                                    MessageBox.Show(this, ex.Message, "error");
                                }
                            }
                            else if (comboBox3.Text == "UDP")
                            try
                            {
                                UDP_Send(Buf);
                            }
                            catch (Exception ex)
                            {
                                button8.Text = "摇杆开";
                                trackBar1.Value = 50;
                                label5.Text = Convert.ToString(trackBar1.Value);
                                trackBar2.Value = 50;
                                label7.Text = Convert.ToString(trackBar2.Value);
                                MessageBox.Show(this, ex.Message, "error");
                            }
                        }
                    }
                }
                xs = label7.Text;
                ys = label5.Text;
            }
        }

        private void comboBox4_DropDown(object sender, EventArgs e)
        {
            comboBox4.Items.Clear();
            GetLocalIP();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            label11.Text = "0";
            label12.Text = "0";
            label13.Text = "0";
            label15.Text = "0";
            label1.Text = "数据清除成功";
        }

        private string StrToHex(string s)
        {
            string[] st = s.Trim().Split(' ');
            byte[] b = new byte[st.Length];//按照指定编码将string编程字节数组
            string result = string.Empty;
            for (int i = 0; i < st.Length; i++)//逐字节变为16进制字符，以%隔开
            {
                b[i] = Convert.ToByte(st[i],16);
            }
            result = Convert.ToString(System.Text.Encoding.ASCII.GetString(b));
            return result;
        }

        private void StrSend(string Buf, Encoding gb)
        {
            if (radioButton4.Checked)
            {
                if (serialPort1.IsOpen)
                {
                    byte[] bytes = gb.GetBytes(Buf);
                    serialPort1.Write(bytes, 0, bytes.Length);
                    label22.Text = (Convert.ToUInt32(label22.Text) + Buf.Length).ToString();
                    if (checkBox3.Checked)
                    {
                        textBox1.AppendText(Buf);
                    }
                }
                else
                    label1.Text = "串口未打开！";
            }
            else if (radioButton3.Checked)
            {
                if (button9.Text == "停止监听")
                {
                    if (comboBox3.Text == "TCP")
                    {
                        try
                        {
                            if (th.IsAlive)
                            {
                                string ip = peerip;

                                byte[] buffer = gb.GetBytes(Buf);
                                dic[ip].Send(buffer);
                                label22.Text = (Convert.ToUInt32(label22.Text) + Buf.Length).ToString();
                                if (checkBox3.Checked)
                                {
                                    textBox1.AppendText(Buf);
                                }
                            }
                            else if (!th.IsAlive)
                                label1.Text = "未连接到终端！";
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(this, ex.Message, "error");
                        }
                    }
                    else if (comboBox3.Text == "UDP")
                    {
                        UDP_Send(Buf);
                    }
                }
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            string Buf = textBox5.Text;
            Encoding gb = System.Text.Encoding.GetEncoding("gb2312");
            if (checkBox4.Checked)
            {
                Buf = StrToHex(Buf);
            }
            StrSend(Buf, gb);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            string Buf = textBox9.Text;
            Encoding gb = System.Text.Encoding.GetEncoding("gb2312");
            if (checkBox5.Checked)
            {
                Buf = StrToHex(Buf);
            }
            StrSend(Buf, gb);
        }

        private void button13_Click(object sender, EventArgs e)
        {
            string Buf = textBox10.Text;
            Encoding gb = System.Text.Encoding.GetEncoding("gb2312");
            if (checkBox6.Checked)
            {
                Buf = StrToHex(Buf);
            }
            StrSend(Buf, gb);
        }

        private void button14_Click(object sender, EventArgs e)
        {
            string Buf = textBox11.Text;
            Encoding gb = System.Text.Encoding.GetEncoding("gb2312");
            if (checkBox7.Checked)
            {
                Buf = StrToHex(Buf);
            }
            StrSend(Buf, gb);
        }

        private void button15_Click(object sender, EventArgs e)
        {
            string Buf = textBox12.Text;
            Encoding gb = System.Text.Encoding.GetEncoding("gb2312");
            if (checkBox8.Checked)
            {
                Buf = StrToHex(Buf);
            }
            StrSend(Buf, gb);
        }

        private void button16_Click(object sender, EventArgs e)
        {
            string Buf = textBox13.Text;
            Encoding gb = System.Text.Encoding.GetEncoding("gb2312");
            if (checkBox9.Checked)
            {
                Buf = StrToHex(Buf);
            }
            StrSend(Buf, gb);
        }

        private void button17_Click(object sender, EventArgs e)
        {
            string Buf = textBox14.Text;
            Encoding gb = System.Text.Encoding.GetEncoding("gb2312");
            if (checkBox10.Checked)
            {
                Buf = StrToHex(Buf);
            }
            StrSend(Buf, gb);
        }

        private void button18_Click(object sender, EventArgs e)
        {
            string Buf = textBox15.Text;
            Encoding gb = System.Text.Encoding.GetEncoding("gb2312");
            if (checkBox11.Checked)
            {
                Buf = StrToHex(Buf);
            }
            StrSend(Buf, gb);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            StreamWriter sw = new StreamWriter("Buffer.log", false);
            if (checkBox4.Checked)
                sw.WriteLine("U1;" + textBox5.Text + ";H");
            else
                sw.WriteLine("U1;" + textBox5.Text + ";A");
            if (checkBox5.Checked)
                sw.WriteLine("U2;" + textBox9.Text + ";H");
            else
                sw.WriteLine("U2;" + textBox9.Text + ";A");
            if (checkBox6.Checked)
                sw.WriteLine("U3;" + textBox10.Text + ";H");
            else
                sw.WriteLine("U3;" + textBox10.Text + ";A");
            if (checkBox7.Checked)
                sw.WriteLine("U4;" + textBox11.Text + ";H");
            else
                sw.WriteLine("U4;" + textBox11.Text + ";A");
            if (checkBox8.Checked)
                sw.WriteLine("U5;" + textBox12.Text + ";H");
            else
                sw.WriteLine("U5;" + textBox12.Text + ";A");
            if (checkBox9.Checked)
                sw.WriteLine("U6;" + textBox13.Text + ";H");
            else
                sw.WriteLine("U6;" + textBox13.Text + ";A");
            if (checkBox10.Checked)
                sw.WriteLine("U7;" + textBox14.Text + ";H");
            else
                sw.WriteLine("U7;" + textBox14.Text + ";A");
            if (checkBox11.Checked)
                sw.WriteLine("U8;" + textBox15.Text + ";H");
            else
                sw.WriteLine("U8;" + textBox15.Text + ";A");
            sw.Close();
        }

        private void Form1_Move(object sender, EventArgs e)
        {
            int screenRight = Screen.PrimaryScreen.Bounds.Right;//屏幕右边缘
            int formRight = this.Left + this.Size.Width;//窗口右边缘=窗口左上角x+窗口宽度
            if (Math.Abs(screenRight - formRight) <= 10)//往右靠
                this.Location = new System.Drawing.Point(screenRight+8 - this.Size.Width, this.Top);//当前窗口坐标赋值，实现吸附

            if (Math.Abs(this.Left) <= 10)//往左靠
                this.Location = new System.Drawing.Point(0-7, this.Top);

            int screenBottom = Screen.PrimaryScreen.Bounds.Bottom;//屏幕下边缘
            int workspace = Screen.PrimaryScreen.WorkingArea.Bottom;
            int formBottom = this.Top + this.Size.Height;//窗口下边缘
            if (Math.Abs(screenBottom - formBottom) <= 10)//往下靠
                this.Location = new System.Drawing.Point(this.Left, screenBottom - this.Size.Height);

            if (Math.Abs(workspace - formBottom) <= 10)//往下靠
                this.Location = new System.Drawing.Point(this.Left, workspace - this.Size.Height+8);

            if (Math.Abs(this.Top) <= 10)//往上靠
                this.Location = new System.Drawing.Point(this.Left, 0);

        }

        private void button7_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";
            label22.Text = "0";
            label1.Text = "发送区已清空";
        }
    }
}
