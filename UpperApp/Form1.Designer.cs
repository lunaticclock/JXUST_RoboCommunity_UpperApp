using System.Text;

namespace UpperApp
{
    partial class UpperApp
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        private const string proname = "小车上位机";
        private const string prover = "V6.0";

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            ASCIIEncoding asciiEncoding1 = new ASCIIEncoding();
            DecoderReplacementFallback decoderReplacementFallback1 = new DecoderReplacementFallback();
            EncoderReplacementFallback encoderReplacementFallback1 = new EncoderReplacementFallback();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpperApp));
            Infotext = new System.Windows.Forms.Label();
            btnSend = new System.Windows.Forms.Button();
            RecvBox = new System.Windows.Forms.TextBox();
            SendBox = new System.Windows.Forms.TextBox();
            SerPortItem = new System.Windows.Forms.ComboBox();
            btnAutoSend = new System.Windows.Forms.CheckBox();
            panel1 = new System.Windows.Forms.Panel();
            rbtnChar = new System.Windows.Forms.RadioButton();
            rbtnHex = new System.Windows.Forms.RadioButton();
            SendTimer = new System.Windows.Forms.Timer(components);
            btnBegin = new System.Windows.Forms.Button();
            serialPort1 = new System.IO.Ports.SerialPort(components);
            Baud = new System.Windows.Forms.ComboBox();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            btnSerial = new System.Windows.Forms.Button();
            FBBar = new System.Windows.Forms.TrackBar();
            label4 = new System.Windows.Forms.Label();
            FBtext = new System.Windows.Forms.Label();
            RLBar = new System.Windows.Forms.TrackBar();
            label6 = new System.Windows.Forms.Label();
            RLtext = new System.Windows.Forms.Label();
            btnNoRL = new System.Windows.Forms.Button();
            Stop = new System.Windows.Forms.Button();
            label8 = new System.Windows.Forms.Label();
            label9 = new System.Windows.Forms.Label();
            label10 = new System.Windows.Forms.Label();
            LabYaw = new System.Windows.Forms.Label();
            LabRoll = new System.Windows.Forms.Label();
            LabPitch = new System.Windows.Forms.Label();
            label14 = new System.Windows.Forms.Label();
            LabDist = new System.Windows.Forms.Label();
            btnclRecv = new System.Windows.Forms.Button();
            btnclSend = new System.Windows.Forms.Button();
            Tim = new System.Windows.Forms.TextBox();
            label16 = new System.Windows.Forms.Label();
            label17 = new System.Windows.Forms.Label();
            label18 = new System.Windows.Forms.Label();
            flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            label19 = new System.Windows.Forms.Label();
            smallChange = new System.Windows.Forms.TextBox();
            Rocker = new System.Windows.Forms.Button();
            ReDisp = new System.Windows.Forms.CheckBox();
            label21 = new System.Windows.Forms.Label();
            label23 = new System.Windows.Forms.Label();
            label24 = new System.Windows.Forms.Label();
            label25 = new System.Windows.Forms.Label();
            groupBox1 = new System.Windows.Forms.GroupBox();
            groupBox2 = new System.Windows.Forms.GroupBox();
            groupBox3 = new System.Windows.Forms.GroupBox();
            Port = new System.Windows.Forms.TextBox();
            SaveData = new System.Windows.Forms.CheckBox();
            Peer = new System.Windows.Forms.ComboBox();
            HostIP = new System.Windows.Forms.ComboBox();
            label28 = new System.Windows.Forms.Label();
            NetType = new System.Windows.Forms.ComboBox();
            panel2 = new System.Windows.Forms.Panel();
            rbtnSerial = new System.Windows.Forms.RadioButton();
            rbtnNET = new System.Windows.Forms.RadioButton();
            label27 = new System.Windows.Forms.Label();
            label26 = new System.Windows.Forms.Label();
            btnListen = new System.Windows.Forms.Button();
            label22 = new System.Windows.Forms.Label();
            label20 = new System.Windows.Forms.Label();
            groupBox4 = new System.Windows.Forms.GroupBox();
            AngDirDisp = new System.Windows.Forms.CheckBox();
            groupBox5 = new System.Windows.Forms.GroupBox();
            ClearAngDisp = new System.Windows.Forms.Button();
            tabControl1 = new System.Windows.Forms.TabControl();
            tabPage1 = new System.Windows.Forms.TabPage();
            tabPage2 = new System.Windows.Forms.TabPage();
            label31 = new System.Windows.Forms.Label();
            btnMsg8 = new System.Windows.Forms.Button();
            MsgBox8 = new System.Windows.Forms.TextBox();
            MsgHex8 = new System.Windows.Forms.CheckBox();
            btnMsg7 = new System.Windows.Forms.Button();
            MsgBox7 = new System.Windows.Forms.TextBox();
            MsgHex7 = new System.Windows.Forms.CheckBox();
            btnMsg6 = new System.Windows.Forms.Button();
            MsgBox6 = new System.Windows.Forms.TextBox();
            MsgHex6 = new System.Windows.Forms.CheckBox();
            btnMsg5 = new System.Windows.Forms.Button();
            MsgBox5 = new System.Windows.Forms.TextBox();
            MsgHex5 = new System.Windows.Forms.CheckBox();
            btnMsg4 = new System.Windows.Forms.Button();
            MsgBox4 = new System.Windows.Forms.TextBox();
            MsgHex4 = new System.Windows.Forms.CheckBox();
            btnMsg3 = new System.Windows.Forms.Button();
            MsgBox3 = new System.Windows.Forms.TextBox();
            MsgHex3 = new System.Windows.Forms.CheckBox();
            btnMsg2 = new System.Windows.Forms.Button();
            MsgBox2 = new System.Windows.Forms.TextBox();
            MsgHex2 = new System.Windows.Forms.CheckBox();
            label30 = new System.Windows.Forms.Label();
            btnMsg1 = new System.Windows.Forms.Button();
            MsgBox1 = new System.Windows.Forms.TextBox();
            MsgHex1 = new System.Windows.Forms.CheckBox();
            tabPage3 = new System.Windows.Forms.TabPage();
            RealDist = new System.Windows.Forms.MaskedTextBox();
            label39 = new System.Windows.Forms.Label();
            ClearImage = new System.Windows.Forms.Button();
            label38 = new System.Windows.Forms.Label();
            label37 = new System.Windows.Forms.Label();
            label36 = new System.Windows.Forms.Label();
            label35 = new System.Windows.Forms.Label();
            label34 = new System.Windows.Forms.Label();
            label33 = new System.Windows.Forms.Label();
            MapBox = new System.Windows.Forms.PictureBox();
            OpenImage = new System.Windows.Forms.Button();
            tabPage4 = new System.Windows.Forms.TabPage();
            ChoseSlaveBthLabel = new System.Windows.Forms.Label();
            ChoseSlaveBthList = new System.Windows.Forms.ComboBox();
            BthDeviceScanBtn = new System.Windows.Forms.Button();
            BthDeviceLabel = new System.Windows.Forms.Label();
            BthDeviceList = new System.Windows.Forms.ComboBox();
            BthSendBox = new System.Windows.Forms.TextBox();
            BthRecvBox = new System.Windows.Forms.TextBox();
            BthSendBtn = new System.Windows.Forms.Button();
            BthConnectBtn = new System.Windows.Forms.Button();
            BthListenBtn = new System.Windows.Forms.Button();
            label32 = new System.Windows.Forms.Label();
            openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            MemTimer = new System.Windows.Forms.Timer(components);
            label40 = new System.Windows.Forms.Label();
            label41 = new System.Windows.Forms.Label();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)FBBar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)RLBar).BeginInit();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            panel2.SuspendLayout();
            groupBox4.SuspendLayout();
            groupBox5.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)MapBox).BeginInit();
            tabPage4.SuspendLayout();
            SuspendLayout();
            // 
            // Infotext
            // 
            Infotext.AutoSize = true;
            Infotext.Font = new System.Drawing.Font("华文新魏", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            Infotext.ForeColor = System.Drawing.Color.Red;
            Infotext.Location = new System.Drawing.Point(402, 26);
            Infotext.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            Infotext.Name = "Infotext";
            Infotext.Size = new System.Drawing.Size(50, 21);
            Infotext.TabIndex = 0;
            Infotext.Text = "状态";
            Infotext.DoubleClick += Info_DoubleClick;
            // 
            // btnSend
            // 
            btnSend.Location = new System.Drawing.Point(396, 294);
            btnSend.Margin = new System.Windows.Forms.Padding(2);
            btnSend.Name = "btnSend";
            btnSend.Size = new System.Drawing.Size(78, 32);
            btnSend.TabIndex = 1;
            btnSend.Text = "发送";
            btnSend.UseVisualStyleBackColor = true;
            // 
            // RecvBox
            // 
            RecvBox.Location = new System.Drawing.Point(162, 24);
            RecvBox.Margin = new System.Windows.Forms.Padding(2);
            RecvBox.Multiline = true;
            RecvBox.Name = "RecvBox";
            RecvBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            RecvBox.Size = new System.Drawing.Size(219, 230);
            RecvBox.TabIndex = 2;
            // 
            // SendBox
            // 
            SendBox.Location = new System.Drawing.Point(161, 294);
            SendBox.Margin = new System.Windows.Forms.Padding(2);
            SendBox.Multiline = true;
            SendBox.Name = "SendBox";
            SendBox.Size = new System.Drawing.Size(219, 63);
            SendBox.TabIndex = 3;
            // 
            // SerPortItem
            // 
            SerPortItem.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            SerPortItem.FormattingEnabled = true;
            SerPortItem.Location = new System.Drawing.Point(67, 26);
            SerPortItem.Margin = new System.Windows.Forms.Padding(2);
            SerPortItem.Name = "SerPortItem";
            SerPortItem.Size = new System.Drawing.Size(91, 28);
            SerPortItem.TabIndex = 4;
            // 
            // btnAutoSend
            // 
            btnAutoSend.AutoSize = true;
            btnAutoSend.Location = new System.Drawing.Point(387, 55);
            btnAutoSend.Margin = new System.Windows.Forms.Padding(2);
            btnAutoSend.Name = "btnAutoSend";
            btnAutoSend.Size = new System.Drawing.Size(91, 24);
            btnAutoSend.TabIndex = 5;
            btnAutoSend.Text = "自动发送";
            btnAutoSend.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            panel1.Controls.Add(rbtnChar);
            panel1.Controls.Add(rbtnHex);
            panel1.Location = new System.Drawing.Point(385, 128);
            panel1.Margin = new System.Windows.Forms.Padding(2);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(75, 118);
            panel1.TabIndex = 6;
            // 
            // rbtnChar
            // 
            rbtnChar.AutoSize = true;
            rbtnChar.Checked = true;
            rbtnChar.Location = new System.Drawing.Point(2, 71);
            rbtnChar.Margin = new System.Windows.Forms.Padding(2);
            rbtnChar.Name = "rbtnChar";
            rbtnChar.Size = new System.Drawing.Size(73, 24);
            rbtnChar.TabIndex = 1;
            rbtnChar.TabStop = true;
            rbtnChar.Text = "CHAR";
            rbtnChar.UseVisualStyleBackColor = true;
            // 
            // rbtnHex
            // 
            rbtnHex.AutoSize = true;
            rbtnHex.Location = new System.Drawing.Point(2, 19);
            rbtnHex.Margin = new System.Windows.Forms.Padding(2);
            rbtnHex.Name = "rbtnHex";
            rbtnHex.Size = new System.Drawing.Size(60, 24);
            rbtnHex.TabIndex = 0;
            rbtnHex.Text = "HEX";
            rbtnHex.UseVisualStyleBackColor = true;
            // 
            // SendTimer
            // 
            SendTimer.Tick += SendTimer_Tick;
            // 
            // btnBegin
            // 
            btnBegin.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            btnBegin.Location = new System.Drawing.Point(396, 326);
            btnBegin.Margin = new System.Windows.Forms.Padding(2);
            btnBegin.Name = "btnBegin";
            btnBegin.Size = new System.Drawing.Size(78, 31);
            btnBegin.TabIndex = 7;
            btnBegin.Tag = "";
            btnBegin.Text = "开始";
            btnBegin.UseVisualStyleBackColor = true;
            btnBegin.Click += btnBegin_Click;
            // 
            // serialPort1
            // 
            serialPort1.BaudRate = 115200;
            serialPort1.DataBits = 8;
            serialPort1.DiscardNull = false;
            serialPort1.DtrEnable = false;
            //asciiEncoding1.DecoderFallback = decoderReplacementFallback1;
            //asciiEncoding1.EncoderFallback = encoderReplacementFallback1;
            serialPort1.Encoding = asciiEncoding1;
            serialPort1.Handshake = System.IO.Ports.Handshake.None;
            serialPort1.NewLine = "\n";
            serialPort1.Parity = System.IO.Ports.Parity.None;
            serialPort1.ParityReplace = 63;
            serialPort1.PortName = "COM1";
            serialPort1.ReadBufferSize = 4096;
            serialPort1.ReadTimeout = -1;
            serialPort1.ReceivedBytesThreshold = 1;
            serialPort1.RtsEnable = false;
            serialPort1.StopBits = System.IO.Ports.StopBits.One;
            serialPort1.WriteBufferSize = 2048;
            serialPort1.WriteTimeout = -1;
            // 
            // Baud
            // 
            Baud.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            Baud.FormattingEnabled = true;
            Baud.Location = new System.Drawing.Point(67, 60);
            Baud.Margin = new System.Windows.Forms.Padding(2);
            Baud.Name = "Baud";
            Baud.Size = new System.Drawing.Size(91, 28);
            Baud.TabIndex = 8;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(3, 32);
            label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(39, 20);
            label2.TabIndex = 11;
            label2.Text = "端口";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(3, 64);
            label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(54, 20);
            label3.TabIndex = 12;
            label3.Text = "波特率";
            // 
            // btnSerial
            // 
            btnSerial.Location = new System.Drawing.Point(67, 94);
            btnSerial.Margin = new System.Windows.Forms.Padding(2);
            btnSerial.Name = "btnSerial";
            btnSerial.Size = new System.Drawing.Size(93, 31);
            btnSerial.TabIndex = 13;
            btnSerial.Text = "打开串口";
            btnSerial.UseVisualStyleBackColor = true;
            btnSerial.Click += btnSerial_Click;
            // 
            // FBBar
            // 
            FBBar.Location = new System.Drawing.Point(32, 68);
            FBBar.Margin = new System.Windows.Forms.Padding(2);
            FBBar.Maximum = 100;
            FBBar.Name = "FBBar";
            FBBar.Orientation = System.Windows.Forms.Orientation.Vertical;
            FBBar.Size = new System.Drawing.Size(56, 151);
            FBBar.SmallChange = 25;
            FBBar.TabIndex = 14;
            FBBar.Value = 50;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(29, 47);
            label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(43, 20);
            label4.TabIndex = 15;
            label4.Text = "速度:";
            // 
            // FBtext
            // 
            FBtext.AutoSize = true;
            FBtext.Location = new System.Drawing.Point(72, 47);
            FBtext.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            FBtext.Name = "FBtext";
            FBtext.Size = new System.Drawing.Size(27, 20);
            FBtext.TabIndex = 16;
            FBtext.Text = "50";
            // 
            // RLBar
            // 
            RLBar.Location = new System.Drawing.Point(138, 135);
            RLBar.Margin = new System.Windows.Forms.Padding(2);
            RLBar.Maximum = 100;
            RLBar.Name = "RLBar";
            RLBar.Size = new System.Drawing.Size(140, 56);
            RLBar.SmallChange = 25;
            RLBar.TabIndex = 17;
            RLBar.Value = 50;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(148, 104);
            label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(54, 20);
            label6.TabIndex = 18;
            label6.Text = "方向：";
            // 
            // RLtext
            // 
            RLtext.AutoSize = true;
            RLtext.Location = new System.Drawing.Point(192, 104);
            RLtext.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            RLtext.Name = "RLtext";
            RLtext.Size = new System.Drawing.Size(27, 20);
            RLtext.TabIndex = 19;
            RLtext.Text = "50";
            // 
            // btnNoRL
            // 
            btnNoRL.Location = new System.Drawing.Point(169, 222);
            btnNoRL.Margin = new System.Windows.Forms.Padding(2);
            btnNoRL.Name = "btnNoRL";
            btnNoRL.Size = new System.Drawing.Size(70, 29);
            btnNoRL.TabIndex = 20;
            btnNoRL.Text = "回正";
            btnNoRL.UseVisualStyleBackColor = true;
            // 
            // Stop
            // 
            Stop.Location = new System.Drawing.Point(24, 222);
            Stop.Margin = new System.Windows.Forms.Padding(2);
            Stop.Name = "Stop";
            Stop.Size = new System.Drawing.Size(70, 29);
            Stop.TabIndex = 21;
            Stop.Text = "停车";
            Stop.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(5, 24);
            label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(58, 20);
            label8.TabIndex = 23;
            label8.Text = "偏航角:";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new System.Drawing.Point(78, 24);
            label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(58, 20);
            label9.TabIndex = 24;
            label9.Text = "横滚角:";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new System.Drawing.Point(154, 24);
            label10.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label10.Name = "label10";
            label10.Size = new System.Drawing.Size(58, 20);
            label10.TabIndex = 25;
            label10.Text = "俯仰角:";
            // 
            // LabYaw
            // 
            LabYaw.AutoSize = true;
            LabYaw.Location = new System.Drawing.Point(5, 65);
            LabYaw.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            LabYaw.Name = "LabYaw";
            LabYaw.Size = new System.Drawing.Size(18, 20);
            LabYaw.TabIndex = 26;
            LabYaw.Text = "0";
            // 
            // LabRoll
            // 
            LabRoll.AutoSize = true;
            LabRoll.Location = new System.Drawing.Point(78, 65);
            LabRoll.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            LabRoll.Name = "LabRoll";
            LabRoll.Size = new System.Drawing.Size(18, 20);
            LabRoll.TabIndex = 27;
            LabRoll.Text = "0";
            // 
            // LabPitch
            // 
            LabPitch.AutoSize = true;
            LabPitch.Location = new System.Drawing.Point(153, 65);
            LabPitch.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            LabPitch.Name = "LabPitch";
            LabPitch.Size = new System.Drawing.Size(18, 20);
            LabPitch.TabIndex = 28;
            LabPitch.Text = "0";
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new System.Drawing.Point(230, 24);
            label14.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label14.Name = "label14";
            label14.Size = new System.Drawing.Size(43, 20);
            label14.TabIndex = 29;
            label14.Text = "航程:";
            // 
            // LabDist
            // 
            LabDist.AutoSize = true;
            LabDist.Location = new System.Drawing.Point(230, 65);
            LabDist.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            LabDist.Name = "LabDist";
            LabDist.Size = new System.Drawing.Size(18, 20);
            LabDist.TabIndex = 30;
            LabDist.Text = "0";
            LabDist.TextChanged += LabDist_TextChanged;
            // 
            // btnclRecv
            // 
            btnclRecv.Location = new System.Drawing.Point(67, 294);
            btnclRecv.Margin = new System.Windows.Forms.Padding(2);
            btnclRecv.Name = "btnclRecv";
            btnclRecv.Size = new System.Drawing.Size(88, 31);
            btnclRecv.TabIndex = 31;
            btnclRecv.Text = "清空接收";
            btnclRecv.UseVisualStyleBackColor = true;
            // 
            // btnclSend
            // 
            btnclSend.Location = new System.Drawing.Point(67, 326);
            btnclSend.Margin = new System.Windows.Forms.Padding(2);
            btnclSend.Name = "btnclSend";
            btnclSend.Size = new System.Drawing.Size(88, 31);
            btnclSend.TabIndex = 32;
            btnclSend.Text = "清空发送";
            btnclSend.UseVisualStyleBackColor = true;
            // 
            // Tim
            // 
            Tim.Location = new System.Drawing.Point(387, 83);
            Tim.Margin = new System.Windows.Forms.Padding(2);
            Tim.Name = "Tim";
            Tim.Size = new System.Drawing.Size(95, 27);
            Tim.TabIndex = 33;
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Font = new System.Drawing.Font("宋体", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            label16.Location = new System.Drawing.Point(486, 83);
            label16.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label16.Name = "label16";
            label16.Size = new System.Drawing.Size(38, 25);
            label16.TabIndex = 34;
            label16.Text = "ms";
            // 
            // label17
            // 
            label17.AutoSize = true;
            label17.Location = new System.Drawing.Point(179, 360);
            label17.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label17.Name = "label17";
            label17.Size = new System.Drawing.Size(31, 20);
            label17.TabIndex = 35;
            label17.Text = "Rx:";
            // 
            // label18
            // 
            label18.AutoSize = true;
            label18.Location = new System.Drawing.Point(214, 360);
            label18.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label18.Name = "label18";
            label18.Size = new System.Drawing.Size(18, 20);
            label18.TabIndex = 36;
            label18.Text = "0";
            // 
            // flowLayoutPanel2
            // 
            flowLayoutPanel2.AutoSize = true;
            flowLayoutPanel2.Location = new System.Drawing.Point(9, 6);
            flowLayoutPanel2.Margin = new System.Windows.Forms.Padding(2);
            flowLayoutPanel2.Name = "flowLayoutPanel2";
            flowLayoutPanel2.Size = new System.Drawing.Size(0, 0);
            flowLayoutPanel2.TabIndex = 37;
            // 
            // label19
            // 
            label19.AutoSize = true;
            label19.Location = new System.Drawing.Point(138, 44);
            label19.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label19.Name = "label19";
            label19.Size = new System.Drawing.Size(84, 20);
            label19.TabIndex = 40;
            label19.Text = "按键步进：";
            // 
            // smallChange
            // 
            smallChange.Location = new System.Drawing.Point(140, 68);
            smallChange.Margin = new System.Windows.Forms.Padding(2);
            smallChange.Name = "smallChange";
            smallChange.Size = new System.Drawing.Size(94, 27);
            smallChange.TabIndex = 41;
            smallChange.Text = "25";
            // 
            // Rocker
            // 
            Rocker.Location = new System.Drawing.Point(289, 47);
            Rocker.Margin = new System.Windows.Forms.Padding(2);
            Rocker.Name = "Rocker";
            Rocker.Size = new System.Drawing.Size(188, 200);
            Rocker.TabIndex = 42;
            Rocker.Text = "摇杆开";
            Rocker.UseVisualStyleBackColor = true;
            Rocker.Click += Rocker_Click;
            Rocker.MouseMove += Rocker_MouseMove;
            // 
            // ReDisp
            // 
            ReDisp.AutoSize = true;
            ReDisp.Checked = true;
            ReDisp.CheckState = System.Windows.Forms.CheckState.Checked;
            ReDisp.Location = new System.Drawing.Point(396, 360);
            ReDisp.Margin = new System.Windows.Forms.Padding(2);
            ReDisp.Name = "ReDisp";
            ReDisp.Size = new System.Drawing.Size(91, 24);
            ReDisp.TabIndex = 43;
            ReDisp.Text = "本地回显";
            ReDisp.UseVisualStyleBackColor = true;
            // 
            // label21
            // 
            label21.AutoSize = true;
            label21.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
            label21.Location = new System.Drawing.Point(10, 24);
            label21.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label21.Name = "label21";
            label21.Size = new System.Drawing.Size(502, 16);
            label21.TabIndex = 47;
            label21.Text = "YAW:25(此为数据)/OVER  ROLL:70/OVER  PITCH:36/OVER  DISTANCE:99/OVER";
            // 
            // label23
            // 
            label23.AutoSize = true;
            label23.Location = new System.Drawing.Point(5, 24);
            label23.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label23.Name = "label23";
            label23.Size = new System.Drawing.Size(92, 20);
            label23.TabIndex = 49;
            label23.Text = "FB:25:OVER";
            // 
            // label24
            // 
            label24.AutoSize = true;
            label24.Location = new System.Drawing.Point(5, 49);
            label24.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label24.Name = "label24";
            label24.Size = new System.Drawing.Size(93, 20);
            label24.TabIndex = 50;
            label24.Text = "RL:50:OVER";
            // 
            // label25
            // 
            label25.AutoSize = true;
            label25.Location = new System.Drawing.Point(5, 74);
            label25.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label25.Name = "label25";
            label25.Size = new System.Drawing.Size(195, 20);
            label25.TabIndex = 51;
            label25.Text = "FR:25(速度):50(方向):OVER";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(label25);
            groupBox1.Controls.Add(label23);
            groupBox1.Controls.Add(label24);
            groupBox1.Location = new System.Drawing.Point(848, 342);
            groupBox1.Margin = new System.Windows.Forms.Padding(2);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new System.Windows.Forms.Padding(2);
            groupBox1.Size = new System.Drawing.Size(206, 100);
            groupBox1.TabIndex = 52;
            groupBox1.TabStop = false;
            groupBox1.Text = "发送数据格式:";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(label21);
            groupBox2.Location = new System.Drawing.Point(5, 396);
            groupBox2.Margin = new System.Windows.Forms.Padding(2);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new System.Windows.Forms.Padding(2);
            groupBox2.Size = new System.Drawing.Size(541, 46);
            groupBox2.TabIndex = 53;
            groupBox2.TabStop = false;
            groupBox2.Text = "接收数据格式:";
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(Port);
            groupBox3.Controls.Add(SaveData);
            groupBox3.Controls.Add(Peer);
            groupBox3.Controls.Add(HostIP);
            groupBox3.Controls.Add(Infotext);
            groupBox3.Controls.Add(label28);
            groupBox3.Controls.Add(NetType);
            groupBox3.Controls.Add(panel2);
            groupBox3.Controls.Add(ReDisp);
            groupBox3.Controls.Add(btnclSend);
            groupBox3.Controls.Add(label17);
            groupBox3.Controls.Add(label18);
            groupBox3.Controls.Add(label27);
            groupBox3.Controls.Add(label26);
            groupBox3.Controls.Add(btnListen);
            groupBox3.Controls.Add(label3);
            groupBox3.Controls.Add(btnSerial);
            groupBox3.Controls.Add(label2);
            groupBox3.Controls.Add(SerPortItem);
            groupBox3.Controls.Add(Baud);
            groupBox3.Controls.Add(label16);
            groupBox3.Controls.Add(label22);
            groupBox3.Controls.Add(Tim);
            groupBox3.Controls.Add(label20);
            groupBox3.Controls.Add(btnclRecv);
            groupBox3.Controls.Add(btnAutoSend);
            groupBox3.Controls.Add(panel1);
            groupBox3.Controls.Add(btnSend);
            groupBox3.Controls.Add(btnBegin);
            groupBox3.Controls.Add(SendBox);
            groupBox3.Controls.Add(RecvBox);
            groupBox3.Location = new System.Drawing.Point(4, 6);
            groupBox3.Margin = new System.Windows.Forms.Padding(2);
            groupBox3.Name = "groupBox3";
            groupBox3.Padding = new System.Windows.Forms.Padding(2);
            groupBox3.Size = new System.Drawing.Size(541, 393);
            groupBox3.TabIndex = 54;
            groupBox3.TabStop = false;
            groupBox3.Text = "串口工作区:";
            // 
            // Port
            // 
            Port.Location = new System.Drawing.Point(97, 189);
            Port.Margin = new System.Windows.Forms.Padding(2);
            Port.Name = "Port";
            Port.Size = new System.Drawing.Size(60, 27);
            Port.TabIndex = 54;
            Port.Text = "1234";
            Port.TextChanged += Port_TextChanged;
            // 
            // SaveData
            // 
            SaveData.AutoSize = true;
            SaveData.Location = new System.Drawing.Point(76, 360);
            SaveData.Margin = new System.Windows.Forms.Padding(2);
            SaveData.Name = "SaveData";
            SaveData.Size = new System.Drawing.Size(91, 24);
            SaveData.TabIndex = 52;
            SaveData.Text = "数据转存";
            SaveData.UseVisualStyleBackColor = true;
            SaveData.CheckedChanged += SaveData_CheckedChanged;
            // 
            // Peer
            // 
            Peer.FormattingEnabled = true;
            Peer.Location = new System.Drawing.Point(201, 258);
            Peer.Margin = new System.Windows.Forms.Padding(2);
            Peer.Name = "Peer";
            Peer.Size = new System.Drawing.Size(179, 28);
            Peer.TabIndex = 51;
            // 
            // HostIP
            // 
            HostIP.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            HostIP.FormattingEnabled = true;
            HostIP.Location = new System.Drawing.Point(5, 153);
            HostIP.Margin = new System.Windows.Forms.Padding(2);
            HostIP.Name = "HostIP";
            HostIP.Size = new System.Drawing.Size(152, 28);
            HostIP.TabIndex = 50;
            // 
            // label28
            // 
            label28.AutoSize = true;
            label28.Location = new System.Drawing.Point(154, 262);
            label28.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label28.Name = "label28";
            label28.Size = new System.Drawing.Size(46, 20);
            label28.TabIndex = 47;
            label28.Text = "Peer:";
            // 
            // NetType
            // 
            NetType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            NetType.FormattingEnabled = true;
            NetType.Location = new System.Drawing.Point(5, 189);
            NetType.Margin = new System.Windows.Forms.Padding(2);
            NetType.Name = "NetType";
            NetType.Size = new System.Drawing.Size(52, 28);
            NetType.TabIndex = 45;
            // 
            // panel2
            // 
            panel2.Controls.Add(rbtnSerial);
            panel2.Controls.Add(rbtnNET);
            panel2.Location = new System.Drawing.Point(462, 128);
            panel2.Margin = new System.Windows.Forms.Padding(2);
            panel2.Name = "panel2";
            panel2.Size = new System.Drawing.Size(79, 118);
            panel2.TabIndex = 44;
            // 
            // rbtnSerial
            // 
            rbtnSerial.AutoSize = true;
            rbtnSerial.Checked = true;
            rbtnSerial.Location = new System.Drawing.Point(2, 71);
            rbtnSerial.Margin = new System.Windows.Forms.Padding(2);
            rbtnSerial.Name = "rbtnSerial";
            rbtnSerial.Size = new System.Drawing.Size(70, 24);
            rbtnSerial.TabIndex = 1;
            rbtnSerial.TabStop = true;
            rbtnSerial.Text = "Serial";
            rbtnSerial.UseVisualStyleBackColor = true;
            // 
            // rbtnNET
            // 
            rbtnNET.AutoSize = true;
            rbtnNET.Location = new System.Drawing.Point(2, 19);
            rbtnNET.Margin = new System.Windows.Forms.Padding(2);
            rbtnNET.Name = "rbtnNET";
            rbtnNET.Size = new System.Drawing.Size(59, 24);
            rbtnNET.TabIndex = 0;
            rbtnNET.Text = "NET";
            rbtnNET.UseVisualStyleBackColor = true;
            // 
            // label27
            // 
            label27.AutoSize = true;
            label27.Location = new System.Drawing.Point(3, 130);
            label27.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label27.Name = "label27";
            label27.Size = new System.Drawing.Size(61, 20);
            label27.TabIndex = 36;
            label27.Text = "HostIP:";
            // 
            // label26
            // 
            label26.AutoSize = true;
            label26.Location = new System.Drawing.Point(54, 192);
            label26.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label26.Name = "label26";
            label26.Size = new System.Drawing.Size(44, 20);
            label26.TabIndex = 35;
            label26.Text = "Port:";
            // 
            // btnListen
            // 
            btnListen.Location = new System.Drawing.Point(65, 221);
            btnListen.Margin = new System.Windows.Forms.Padding(2);
            btnListen.Name = "btnListen";
            btnListen.Size = new System.Drawing.Size(93, 30);
            btnListen.TabIndex = 4;
            btnListen.Text = "开始监听";
            btnListen.UseVisualStyleBackColor = true;
            btnListen.Click += btnListen_Click;
            // 
            // label22
            // 
            label22.AutoSize = true;
            label22.Location = new System.Drawing.Point(328, 360);
            label22.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label22.Name = "label22";
            label22.Size = new System.Drawing.Size(18, 20);
            label22.TabIndex = 1;
            label22.Text = "0";
            // 
            // label20
            // 
            label20.AutoSize = true;
            label20.Location = new System.Drawing.Point(292, 360);
            label20.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label20.Name = "label20";
            label20.Size = new System.Drawing.Size(30, 20);
            label20.TabIndex = 0;
            label20.Text = "Tx:";
            // 
            // groupBox4
            // 
            groupBox4.BackColor = System.Drawing.Color.WhiteSmoke;
            groupBox4.Controls.Add(Rocker);
            groupBox4.Controls.Add(smallChange);
            groupBox4.Controls.Add(btnNoRL);
            groupBox4.Controls.Add(Stop);
            groupBox4.Controls.Add(label19);
            groupBox4.Controls.Add(label4);
            groupBox4.Controls.Add(RLBar);
            groupBox4.Controls.Add(RLtext);
            groupBox4.Controls.Add(FBtext);
            groupBox4.Controls.Add(label6);
            groupBox4.Controls.Add(FBBar);
            groupBox4.Location = new System.Drawing.Point(5, 6);
            groupBox4.Margin = new System.Windows.Forms.Padding(2);
            groupBox4.Name = "groupBox4";
            groupBox4.Padding = new System.Windows.Forms.Padding(2);
            groupBox4.Size = new System.Drawing.Size(485, 286);
            groupBox4.TabIndex = 55;
            groupBox4.TabStop = false;
            groupBox4.Text = "运动状态控制:";
            // 
            // AngDirDisp
            // 
            AngDirDisp.AutoSize = true;
            AngDirDisp.Location = new System.Drawing.Point(5, 100);
            AngDirDisp.Margin = new System.Windows.Forms.Padding(2);
            AngDirDisp.Name = "AngDirDisp";
            AngDirDisp.Size = new System.Drawing.Size(121, 24);
            AngDirDisp.TabIndex = 39;
            AngDirDisp.Text = "角度路程显示";
            AngDirDisp.UseVisualStyleBackColor = true;
            // 
            // groupBox5
            // 
            groupBox5.Controls.Add(ClearAngDisp);
            groupBox5.Controls.Add(AngDirDisp);
            groupBox5.Controls.Add(label14);
            groupBox5.Controls.Add(LabDist);
            groupBox5.Controls.Add(label10);
            groupBox5.Controls.Add(LabPitch);
            groupBox5.Controls.Add(LabYaw);
            groupBox5.Controls.Add(LabRoll);
            groupBox5.Controls.Add(label8);
            groupBox5.Controls.Add(label9);
            groupBox5.Location = new System.Drawing.Point(555, 342);
            groupBox5.Margin = new System.Windows.Forms.Padding(2);
            groupBox5.Name = "groupBox5";
            groupBox5.Padding = new System.Windows.Forms.Padding(2);
            groupBox5.Size = new System.Drawing.Size(282, 134);
            groupBox5.TabIndex = 56;
            groupBox5.TabStop = false;
            groupBox5.Text = "小车姿态:";
            // 
            // ClearAngDisp
            // 
            ClearAngDisp.Location = new System.Drawing.Point(191, 93);
            ClearAngDisp.Margin = new System.Windows.Forms.Padding(2);
            ClearAngDisp.Name = "ClearAngDisp";
            ClearAngDisp.Size = new System.Drawing.Size(86, 32);
            ClearAngDisp.TabIndex = 40;
            ClearAngDisp.Text = "清除数据";
            ClearAngDisp.UseVisualStyleBackColor = true;
            ClearAngDisp.Click += ClearAngDisp_Click;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Controls.Add(tabPage3);
            tabControl1.Controls.Add(tabPage4);
            tabControl1.Location = new System.Drawing.Point(550, 6);
            tabControl1.Margin = new System.Windows.Forms.Padding(2);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new System.Drawing.Size(508, 332);
            tabControl1.TabIndex = 57;
            // 
            // tabPage1
            // 
            tabPage1.BackColor = System.Drawing.Color.WhiteSmoke;
            tabPage1.Controls.Add(groupBox4);
            tabPage1.Location = new System.Drawing.Point(4, 29);
            tabPage1.Margin = new System.Windows.Forms.Padding(2);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new System.Windows.Forms.Padding(2);
            tabPage1.Size = new System.Drawing.Size(500, 299);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "运动控制";
            // 
            // tabPage2
            // 
            tabPage2.BackColor = System.Drawing.Color.WhiteSmoke;
            tabPage2.Controls.Add(label31);
            tabPage2.Controls.Add(btnMsg8);
            tabPage2.Controls.Add(MsgBox8);
            tabPage2.Controls.Add(MsgHex8);
            tabPage2.Controls.Add(btnMsg7);
            tabPage2.Controls.Add(MsgBox7);
            tabPage2.Controls.Add(MsgHex7);
            tabPage2.Controls.Add(btnMsg6);
            tabPage2.Controls.Add(MsgBox6);
            tabPage2.Controls.Add(MsgHex6);
            tabPage2.Controls.Add(btnMsg5);
            tabPage2.Controls.Add(MsgBox5);
            tabPage2.Controls.Add(MsgHex5);
            tabPage2.Controls.Add(btnMsg4);
            tabPage2.Controls.Add(MsgBox4);
            tabPage2.Controls.Add(MsgHex4);
            tabPage2.Controls.Add(btnMsg3);
            tabPage2.Controls.Add(MsgBox3);
            tabPage2.Controls.Add(MsgHex3);
            tabPage2.Controls.Add(btnMsg2);
            tabPage2.Controls.Add(MsgBox2);
            tabPage2.Controls.Add(MsgHex2);
            tabPage2.Controls.Add(label30);
            tabPage2.Controls.Add(btnMsg1);
            tabPage2.Controls.Add(MsgBox1);
            tabPage2.Controls.Add(MsgHex1);
            tabPage2.Location = new System.Drawing.Point(4, 29);
            tabPage2.Margin = new System.Windows.Forms.Padding(2);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new System.Windows.Forms.Padding(2);
            tabPage2.Size = new System.Drawing.Size(500, 299);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "批量字串";
            // 
            // label31
            // 
            label31.AutoSize = true;
            label31.Location = new System.Drawing.Point(185, 5);
            label31.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label31.Name = "label31";
            label31.Size = new System.Drawing.Size(84, 20);
            label31.TabIndex = 26;
            label31.Text = "待发送字串";
            // 
            // btnMsg8
            // 
            btnMsg8.Location = new System.Drawing.Point(425, 255);
            btnMsg8.Margin = new System.Windows.Forms.Padding(2);
            btnMsg8.Name = "btnMsg8";
            btnMsg8.Size = new System.Drawing.Size(70, 30);
            btnMsg8.TabIndex = 25;
            btnMsg8.Text = "发送";
            btnMsg8.UseVisualStyleBackColor = true;
            // 
            // MsgBox8
            // 
            MsgBox8.Location = new System.Drawing.Point(28, 254);
            MsgBox8.Margin = new System.Windows.Forms.Padding(2);
            MsgBox8.Name = "MsgBox8";
            MsgBox8.Size = new System.Drawing.Size(392, 27);
            MsgBox8.TabIndex = 24;
            // 
            // MsgHex8
            // 
            MsgHex8.AutoSize = true;
            MsgHex8.Location = new System.Drawing.Point(5, 260);
            MsgHex8.Margin = new System.Windows.Forms.Padding(2);
            MsgHex8.Name = "MsgHex8";
            MsgHex8.Size = new System.Drawing.Size(18, 17);
            MsgHex8.TabIndex = 23;
            MsgHex8.UseVisualStyleBackColor = true;
            // 
            // btnMsg7
            // 
            btnMsg7.Location = new System.Drawing.Point(425, 224);
            btnMsg7.Margin = new System.Windows.Forms.Padding(2);
            btnMsg7.Name = "btnMsg7";
            btnMsg7.Size = new System.Drawing.Size(70, 30);
            btnMsg7.TabIndex = 22;
            btnMsg7.Text = "发送";
            btnMsg7.UseVisualStyleBackColor = true;
            // 
            // MsgBox7
            // 
            MsgBox7.Location = new System.Drawing.Point(28, 223);
            MsgBox7.Margin = new System.Windows.Forms.Padding(2);
            MsgBox7.Name = "MsgBox7";
            MsgBox7.Size = new System.Drawing.Size(392, 27);
            MsgBox7.TabIndex = 21;
            // 
            // MsgHex7
            // 
            MsgHex7.AutoSize = true;
            MsgHex7.Location = new System.Drawing.Point(5, 230);
            MsgHex7.Margin = new System.Windows.Forms.Padding(2);
            MsgHex7.Name = "MsgHex7";
            MsgHex7.Size = new System.Drawing.Size(18, 17);
            MsgHex7.TabIndex = 20;
            MsgHex7.UseVisualStyleBackColor = true;
            // 
            // btnMsg6
            // 
            btnMsg6.Location = new System.Drawing.Point(425, 193);
            btnMsg6.Margin = new System.Windows.Forms.Padding(2);
            btnMsg6.Name = "btnMsg6";
            btnMsg6.Size = new System.Drawing.Size(70, 30);
            btnMsg6.TabIndex = 19;
            btnMsg6.Text = "发送";
            btnMsg6.UseVisualStyleBackColor = true;
            // 
            // MsgBox6
            // 
            MsgBox6.Location = new System.Drawing.Point(28, 192);
            MsgBox6.Margin = new System.Windows.Forms.Padding(2);
            MsgBox6.Name = "MsgBox6";
            MsgBox6.Size = new System.Drawing.Size(392, 27);
            MsgBox6.TabIndex = 18;
            // 
            // MsgHex6
            // 
            MsgHex6.AutoSize = true;
            MsgHex6.Location = new System.Drawing.Point(5, 199);
            MsgHex6.Margin = new System.Windows.Forms.Padding(2);
            MsgHex6.Name = "MsgHex6";
            MsgHex6.Size = new System.Drawing.Size(18, 17);
            MsgHex6.TabIndex = 17;
            MsgHex6.UseVisualStyleBackColor = true;
            // 
            // btnMsg5
            // 
            btnMsg5.Location = new System.Drawing.Point(425, 162);
            btnMsg5.Margin = new System.Windows.Forms.Padding(2);
            btnMsg5.Name = "btnMsg5";
            btnMsg5.Size = new System.Drawing.Size(70, 30);
            btnMsg5.TabIndex = 16;
            btnMsg5.Text = "发送";
            btnMsg5.UseVisualStyleBackColor = true;
            // 
            // MsgBox5
            // 
            MsgBox5.Location = new System.Drawing.Point(28, 161);
            MsgBox5.Margin = new System.Windows.Forms.Padding(2);
            MsgBox5.Name = "MsgBox5";
            MsgBox5.Size = new System.Drawing.Size(392, 27);
            MsgBox5.TabIndex = 15;
            // 
            // MsgHex5
            // 
            MsgHex5.AutoSize = true;
            MsgHex5.Location = new System.Drawing.Point(5, 168);
            MsgHex5.Margin = new System.Windows.Forms.Padding(2);
            MsgHex5.Name = "MsgHex5";
            MsgHex5.Size = new System.Drawing.Size(18, 17);
            MsgHex5.TabIndex = 14;
            MsgHex5.UseVisualStyleBackColor = true;
            // 
            // btnMsg4
            // 
            btnMsg4.Location = new System.Drawing.Point(425, 131);
            btnMsg4.Margin = new System.Windows.Forms.Padding(2);
            btnMsg4.Name = "btnMsg4";
            btnMsg4.Size = new System.Drawing.Size(70, 30);
            btnMsg4.TabIndex = 13;
            btnMsg4.Text = "发送";
            btnMsg4.UseVisualStyleBackColor = true;
            // 
            // MsgBox4
            // 
            MsgBox4.Location = new System.Drawing.Point(28, 130);
            MsgBox4.Margin = new System.Windows.Forms.Padding(2);
            MsgBox4.Name = "MsgBox4";
            MsgBox4.Size = new System.Drawing.Size(392, 27);
            MsgBox4.TabIndex = 12;
            // 
            // MsgHex4
            // 
            MsgHex4.AutoSize = true;
            MsgHex4.Location = new System.Drawing.Point(5, 137);
            MsgHex4.Margin = new System.Windows.Forms.Padding(2);
            MsgHex4.Name = "MsgHex4";
            MsgHex4.Size = new System.Drawing.Size(18, 17);
            MsgHex4.TabIndex = 11;
            MsgHex4.UseVisualStyleBackColor = true;
            // 
            // btnMsg3
            // 
            btnMsg3.Location = new System.Drawing.Point(425, 99);
            btnMsg3.Margin = new System.Windows.Forms.Padding(2);
            btnMsg3.Name = "btnMsg3";
            btnMsg3.Size = new System.Drawing.Size(70, 30);
            btnMsg3.TabIndex = 10;
            btnMsg3.Text = "发送";
            btnMsg3.UseVisualStyleBackColor = true;
            // 
            // MsgBox3
            // 
            MsgBox3.Location = new System.Drawing.Point(28, 98);
            MsgBox3.Margin = new System.Windows.Forms.Padding(2);
            MsgBox3.Name = "MsgBox3";
            MsgBox3.Size = new System.Drawing.Size(392, 27);
            MsgBox3.TabIndex = 9;
            // 
            // MsgHex3
            // 
            MsgHex3.AutoSize = true;
            MsgHex3.Location = new System.Drawing.Point(5, 105);
            MsgHex3.Margin = new System.Windows.Forms.Padding(2);
            MsgHex3.Name = "MsgHex3";
            MsgHex3.Size = new System.Drawing.Size(18, 17);
            MsgHex3.TabIndex = 8;
            MsgHex3.UseVisualStyleBackColor = true;
            // 
            // btnMsg2
            // 
            btnMsg2.Location = new System.Drawing.Point(425, 67);
            btnMsg2.Margin = new System.Windows.Forms.Padding(2);
            btnMsg2.Name = "btnMsg2";
            btnMsg2.Size = new System.Drawing.Size(70, 30);
            btnMsg2.TabIndex = 7;
            btnMsg2.Text = "发送";
            btnMsg2.UseVisualStyleBackColor = true;
            // 
            // MsgBox2
            // 
            MsgBox2.Location = new System.Drawing.Point(28, 66);
            MsgBox2.Margin = new System.Windows.Forms.Padding(2);
            MsgBox2.Name = "MsgBox2";
            MsgBox2.Size = new System.Drawing.Size(392, 27);
            MsgBox2.TabIndex = 6;
            // 
            // MsgHex2
            // 
            MsgHex2.AutoSize = true;
            MsgHex2.Location = new System.Drawing.Point(5, 73);
            MsgHex2.Margin = new System.Windows.Forms.Padding(2);
            MsgHex2.Name = "MsgHex2";
            MsgHex2.Size = new System.Drawing.Size(18, 17);
            MsgHex2.TabIndex = 5;
            MsgHex2.UseVisualStyleBackColor = true;
            // 
            // label30
            // 
            label30.AutoSize = true;
            label30.Location = new System.Drawing.Point(2, 5);
            label30.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label30.Name = "label30";
            label30.Size = new System.Drawing.Size(39, 20);
            label30.TabIndex = 3;
            label30.Text = "HEX";
            // 
            // btnMsg1
            // 
            btnMsg1.Location = new System.Drawing.Point(425, 35);
            btnMsg1.Margin = new System.Windows.Forms.Padding(2);
            btnMsg1.Name = "btnMsg1";
            btnMsg1.Size = new System.Drawing.Size(70, 30);
            btnMsg1.TabIndex = 2;
            btnMsg1.Text = "发送";
            btnMsg1.UseVisualStyleBackColor = true;
            // 
            // MsgBox1
            // 
            MsgBox1.Location = new System.Drawing.Point(28, 34);
            MsgBox1.Margin = new System.Windows.Forms.Padding(2);
            MsgBox1.Name = "MsgBox1";
            MsgBox1.Size = new System.Drawing.Size(392, 27);
            MsgBox1.TabIndex = 1;
            // 
            // MsgHex1
            // 
            MsgHex1.AutoSize = true;
            MsgHex1.Location = new System.Drawing.Point(5, 41);
            MsgHex1.Margin = new System.Windows.Forms.Padding(2);
            MsgHex1.Name = "MsgHex1";
            MsgHex1.Size = new System.Drawing.Size(18, 17);
            MsgHex1.TabIndex = 0;
            MsgHex1.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            tabPage3.BackColor = System.Drawing.Color.WhiteSmoke;
            tabPage3.Controls.Add(RealDist);
            tabPage3.Controls.Add(label39);
            tabPage3.Controls.Add(ClearImage);
            tabPage3.Controls.Add(label38);
            tabPage3.Controls.Add(label37);
            tabPage3.Controls.Add(label36);
            tabPage3.Controls.Add(label35);
            tabPage3.Controls.Add(label34);
            tabPage3.Controls.Add(label33);
            tabPage3.Controls.Add(MapBox);
            tabPage3.Controls.Add(OpenImage);
            tabPage3.Location = new System.Drawing.Point(4, 29);
            tabPage3.Margin = new System.Windows.Forms.Padding(2);
            tabPage3.Name = "tabPage3";
            tabPage3.Size = new System.Drawing.Size(500, 299);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "行走路线";
            // 
            // RealDist
            // 
            RealDist.Location = new System.Drawing.Point(436, 229);
            RealDist.Margin = new System.Windows.Forms.Padding(2);
            RealDist.Mask = "99.99";
            RealDist.Name = "RealDist";
            RealDist.RejectInputOnFirstFailure = true;
            RealDist.Size = new System.Drawing.Size(54, 27);
            RealDist.TabIndex = 12;
            RealDist.TextChanged += RealDist_TextChanged;
            // 
            // label39
            // 
            label39.AutoSize = true;
            label39.Location = new System.Drawing.Point(415, 204);
            label39.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label39.Name = "label39";
            label39.Size = new System.Drawing.Size(67, 20);
            label39.TabIndex = 10;
            label39.Text = "距离(m):";
            // 
            // ClearImage
            // 
            ClearImage.Location = new System.Drawing.Point(406, 263);
            ClearImage.Margin = new System.Windows.Forms.Padding(2);
            ClearImage.Name = "ClearImage";
            ClearImage.Size = new System.Drawing.Size(92, 31);
            ClearImage.TabIndex = 9;
            ClearImage.Text = "清除锚点";
            ClearImage.UseVisualStyleBackColor = true;
            ClearImage.Click += ClearImage_Click;
            // 
            // label38
            // 
            label38.AutoSize = true;
            label38.Location = new System.Drawing.Point(431, 179);
            label38.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label38.Name = "label38";
            label38.Size = new System.Drawing.Size(31, 20);
            label38.TabIndex = 8;
            label38.Text = "0,0";
            // 
            // label37
            // 
            label37.AutoSize = true;
            label37.Location = new System.Drawing.Point(415, 157);
            label37.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label37.Name = "label37";
            label37.Size = new System.Drawing.Size(73, 20);
            label37.TabIndex = 7;
            label37.Text = "鼠标位置:";
            // 
            // label36
            // 
            label36.AutoSize = true;
            label36.Location = new System.Drawing.Point(433, 121);
            label36.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label36.Name = "label36";
            label36.Size = new System.Drawing.Size(31, 20);
            label36.TabIndex = 6;
            label36.Text = "0,0";
            // 
            // label35
            // 
            label35.AutoSize = true;
            label35.Location = new System.Drawing.Point(417, 99);
            label35.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label35.Name = "label35";
            label35.Size = new System.Drawing.Size(58, 20);
            label35.TabIndex = 5;
            label35.Text = "末尾点:";
            // 
            // label34
            // 
            label34.AutoSize = true;
            label34.Location = new System.Drawing.Point(432, 61);
            label34.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label34.Name = "label34";
            label34.Size = new System.Drawing.Size(31, 20);
            label34.TabIndex = 4;
            label34.Text = "0,0";
            // 
            // label33
            // 
            label33.AutoSize = true;
            label33.Location = new System.Drawing.Point(416, 42);
            label33.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label33.Name = "label33";
            label33.Size = new System.Drawing.Size(58, 20);
            label33.TabIndex = 3;
            label33.Text = "起始点:";
            // 
            // MapBox
            // 
            MapBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            MapBox.Location = new System.Drawing.Point(2, 2);
            MapBox.Margin = new System.Windows.Forms.Padding(2);
            MapBox.Name = "MapBox";
            MapBox.Size = new System.Drawing.Size(400, 292);
            MapBox.TabIndex = 2;
            MapBox.TabStop = false;
            MapBox.Click += MapBox_Click;
            MapBox.MouseMove += MapBox_MouseMove;
            // 
            // OpenImage
            // 
            OpenImage.Location = new System.Drawing.Point(418, 2);
            OpenImage.Margin = new System.Windows.Forms.Padding(2);
            OpenImage.Name = "OpenImage";
            OpenImage.Size = new System.Drawing.Size(80, 31);
            OpenImage.TabIndex = 1;
            OpenImage.Text = "打开";
            OpenImage.UseVisualStyleBackColor = true;
            OpenImage.Click += OpenImage_Click;
            // 
            // tabPage4
            // 
            tabPage4.BackColor = System.Drawing.Color.WhiteSmoke;
            tabPage4.Controls.Add(ChoseSlaveBthLabel);
            tabPage4.Controls.Add(ChoseSlaveBthList);
            tabPage4.Controls.Add(BthDeviceScanBtn);
            tabPage4.Controls.Add(BthDeviceLabel);
            tabPage4.Controls.Add(BthDeviceList);
            tabPage4.Controls.Add(BthSendBox);
            tabPage4.Controls.Add(BthRecvBox);
            tabPage4.Controls.Add(BthSendBtn);
            tabPage4.Controls.Add(BthConnectBtn);
            tabPage4.Controls.Add(BthListenBtn);
            tabPage4.Location = new System.Drawing.Point(4, 29);
            tabPage4.Margin = new System.Windows.Forms.Padding(4);
            tabPage4.Name = "tabPage4";
            tabPage4.Size = new System.Drawing.Size(500, 299);
            tabPage4.TabIndex = 3;
            tabPage4.Text = "蓝牙模块";
            // 
            // ChoseSlaveBthLabel
            // 
            ChoseSlaveBthLabel.AutoSize = true;
            ChoseSlaveBthLabel.Location = new System.Drawing.Point(9, 262);
            ChoseSlaveBthLabel.Name = "ChoseSlaveBthLabel";
            ChoseSlaveBthLabel.Size = new System.Drawing.Size(129, 20);
            ChoseSlaveBthLabel.TabIndex = 10;
            ChoseSlaveBthLabel.Text = "选择蓝牙从设备：";
            // 
            // ChoseSlaveBthList
            // 
            ChoseSlaveBthList.FormattingEnabled = true;
            ChoseSlaveBthList.Location = new System.Drawing.Point(144, 259);
            ChoseSlaveBthList.Name = "ChoseSlaveBthList";
            ChoseSlaveBthList.Size = new System.Drawing.Size(139, 28);
            ChoseSlaveBthList.TabIndex = 9;
            // 
            // BthDeviceScanBtn
            // 
            BthDeviceScanBtn.Location = new System.Drawing.Point(391, 150);
            BthDeviceScanBtn.Name = "BthDeviceScanBtn";
            BthDeviceScanBtn.Size = new System.Drawing.Size(101, 33);
            BthDeviceScanBtn.TabIndex = 8;
            BthDeviceScanBtn.Text = "扫描蓝牙";
            BthDeviceScanBtn.UseVisualStyleBackColor = true;
            BthDeviceScanBtn.Click += BthDeviceScanBtn_Click;
            // 
            // BthDeviceLabel
            // 
            BthDeviceLabel.AutoSize = true;
            BthDeviceLabel.Location = new System.Drawing.Point(363, 189);
            BthDeviceLabel.Name = "BthDeviceLabel";
            BthDeviceLabel.Size = new System.Drawing.Size(129, 20);
            BthDeviceLabel.TabIndex = 7;
            BthDeviceLabel.Text = "选择蓝牙主设备：";
            // 
            // BthDeviceList
            // 
            BthDeviceList.FormattingEnabled = true;
            BthDeviceList.Location = new System.Drawing.Point(363, 220);
            BthDeviceList.Name = "BthDeviceList";
            BthDeviceList.Size = new System.Drawing.Size(129, 28);
            BthDeviceList.TabIndex = 6;
            // 
            // BthSendBox
            // 
            BthSendBox.Location = new System.Drawing.Point(6, 186);
            BthSendBox.Margin = new System.Windows.Forms.Padding(4);
            BthSendBox.Name = "BthSendBox";
            BthSendBox.Size = new System.Drawing.Size(273, 27);
            BthSendBox.TabIndex = 5;
            // 
            // BthRecvBox
            // 
            BthRecvBox.Location = new System.Drawing.Point(6, 5);
            BthRecvBox.Margin = new System.Windows.Forms.Padding(4);
            BthRecvBox.Multiline = true;
            BthRecvBox.Name = "BthRecvBox";
            BthRecvBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            BthRecvBox.Size = new System.Drawing.Size(365, 169);
            BthRecvBox.TabIndex = 4;
            // 
            // BthSendBtn
            // 
            BthSendBtn.Location = new System.Drawing.Point(194, 220);
            BthSendBtn.Margin = new System.Windows.Forms.Padding(4);
            BthSendBtn.Name = "BthSendBtn";
            BthSendBtn.Size = new System.Drawing.Size(86, 30);
            BthSendBtn.TabIndex = 3;
            BthSendBtn.Text = "发送";
            BthSendBtn.UseVisualStyleBackColor = true;
            BthSendBtn.Click += BthSendBtn_Click;
            // 
            // BthConnectBtn
            // 
            BthConnectBtn.Location = new System.Drawing.Point(100, 220);
            BthConnectBtn.Margin = new System.Windows.Forms.Padding(4);
            BthConnectBtn.Name = "BthConnectBtn";
            BthConnectBtn.Size = new System.Drawing.Size(86, 30);
            BthConnectBtn.TabIndex = 2;
            BthConnectBtn.Text = "连接";
            BthConnectBtn.UseVisualStyleBackColor = true;
            BthConnectBtn.Click += BthConnectBtn_Click;
            // 
            // BthListenBtn
            // 
            BthListenBtn.Location = new System.Drawing.Point(6, 220);
            BthListenBtn.Margin = new System.Windows.Forms.Padding(4);
            BthListenBtn.Name = "BthListenBtn";
            BthListenBtn.Size = new System.Drawing.Size(86, 30);
            BthListenBtn.TabIndex = 1;
            BthListenBtn.Text = "监听";
            BthListenBtn.UseVisualStyleBackColor = true;
            BthListenBtn.Click += BthListenBtn_Click;
            // 
            // label32
            // 
            label32.AutoSize = true;
            label32.Location = new System.Drawing.Point(4, 447);
            label32.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label32.Name = "label32";
            label32.Size = new System.Drawing.Size(459, 20);
            label32.TabIndex = 58;
            label32.Text = "若无法从其他终端上通过网络连接此程序，请允许此程序通过防火墙";
            // 
            // openFileDialog1
            // 
            openFileDialog1.FileName = "Default";
            openFileDialog1.Filter = "所有文件|*.*|图片文件|*.jpg;*.png;*.bmp;*.JPG;*.BMP;*.PNG";
            // 
            // MemTimer
            // 
            MemTimer.Enabled = true;
            MemTimer.Interval = 500;
            MemTimer.Tick += MemTimer_Tick;
            // 
            // label40
            // 
            label40.AutoSize = true;
            label40.Location = new System.Drawing.Point(854, 442);
            label40.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label40.Name = "label40";
            label40.Size = new System.Drawing.Size(73, 20);
            label40.TabIndex = 59;
            label40.Text = "内存用量:";
            // 
            // label41
            // 
            label41.AutoSize = true;
            label41.Location = new System.Drawing.Point(930, 442);
            label41.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label41.Name = "label41";
            label41.Size = new System.Drawing.Size(44, 20);
            label41.TabIndex = 60;
            label41.Text = "0.0%";
            // 
            // UpperApp
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            ClientSize = new System.Drawing.Size(1062, 479);
            Controls.Add(label41);
            Controls.Add(label40);
            Controls.Add(label32);
            Controls.Add(tabControl1);
            Controls.Add(flowLayoutPanel2);
            Controls.Add(groupBox1);
            Controls.Add(groupBox2);
            Controls.Add(groupBox3);
            Controls.Add(groupBox5);
            ForeColor = System.Drawing.SystemColors.ControlText;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(2);
            MaximizeBox = false;
            Name = "UpperApp";
            Opacity = 0.98D;
            Text = "小车上位机V5.0";
            FormClosing += UpperApp_FormClosing;
            Load += UpperApp_Load;
            Move += UpperApp_Move;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)FBBar).EndInit();
            ((System.ComponentModel.ISupportInitialize)RLBar).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            groupBox5.ResumeLayout(false);
            groupBox5.PerformLayout();
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            tabPage3.ResumeLayout(false);
            tabPage3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)MapBox).EndInit();
            tabPage4.ResumeLayout(false);
            tabPage4.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label Infotext;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.TextBox RecvBox;
        private System.Windows.Forms.TextBox SendBox;
        private System.Windows.Forms.ComboBox SerPortItem;
        private System.Windows.Forms.CheckBox btnAutoSend;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RadioButton rbtnChar;
        private System.Windows.Forms.RadioButton rbtnHex;
        private System.Windows.Forms.Timer SendTimer;
        private System.Windows.Forms.Button btnBegin;
        private System.IO.Ports.SerialPort serialPort1;
        private System.Windows.Forms.ComboBox Baud;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnSerial;
        private System.Windows.Forms.TrackBar FBBar;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label FBtext;
        private System.Windows.Forms.TrackBar RLBar;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label RLtext;
        private System.Windows.Forms.Button btnNoRL;
        private System.Windows.Forms.Button Stop;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label LabYaw;
        private System.Windows.Forms.Label LabRoll;
        private System.Windows.Forms.Label LabPitch;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label LabDist;
        private System.Windows.Forms.Button btnclRecv;
        private System.Windows.Forms.Button btnclSend;
        private System.Windows.Forms.TextBox Tim;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.TextBox smallChange;
        private System.Windows.Forms.Button Rocker;
        private System.Windows.Forms.CheckBox ReDisp;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.CheckBox AngDirDisp;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.Button btnListen;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.RadioButton rbtnSerial;
        private System.Windows.Forms.RadioButton rbtnNET;
        private System.Windows.Forms.ComboBox NetType;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.ComboBox HostIP;
        private System.Windows.Forms.Button ClearAngDisp;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        //private System.Windows.Forms.CheckBox[] checks = new System.Windows.Forms.CheckBox[8];
        private System.Windows.Forms.Button btnMsg1;
        private System.Windows.Forms.TextBox MsgBox1;
        private System.Windows.Forms.CheckBox MsgHex1;
        private System.Windows.Forms.Button btnMsg8;
        private System.Windows.Forms.TextBox MsgBox8;
        private System.Windows.Forms.CheckBox MsgHex8;
        private System.Windows.Forms.Button btnMsg7;
        private System.Windows.Forms.TextBox MsgBox7;
        private System.Windows.Forms.CheckBox MsgHex7;
        private System.Windows.Forms.Button btnMsg6;
        private System.Windows.Forms.TextBox MsgBox6;
        private System.Windows.Forms.CheckBox MsgHex6;
        private System.Windows.Forms.Button btnMsg5;
        private System.Windows.Forms.TextBox MsgBox5;
        private System.Windows.Forms.CheckBox MsgHex5;
        private System.Windows.Forms.Button btnMsg4;
        private System.Windows.Forms.TextBox MsgBox4;
        private System.Windows.Forms.CheckBox MsgHex4;
        private System.Windows.Forms.Button btnMsg3;
        private System.Windows.Forms.TextBox MsgBox3;
        private System.Windows.Forms.CheckBox MsgHex3;
        private System.Windows.Forms.Button btnMsg2;
        private System.Windows.Forms.TextBox MsgBox2;
        private System.Windows.Forms.CheckBox MsgHex2;
        private System.Windows.Forms.Label label30;
        private System.Windows.Forms.Label label32;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Button OpenImage;
        private System.Windows.Forms.PictureBox MapBox;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Label label36;
        private System.Windows.Forms.Label label35;
        private System.Windows.Forms.Label label34;
        private System.Windows.Forms.Label label33;
        private System.Windows.Forms.Label label38;
        private System.Windows.Forms.Label label37;
        private System.Windows.Forms.Button ClearImage;
        private System.Windows.Forms.Label label39;
        private System.Windows.Forms.MaskedTextBox RealDist;
        private System.Windows.Forms.Timer MemTimer;
        private System.Windows.Forms.Label label40;
        private System.Windows.Forms.Label label41;
        private System.Windows.Forms.Label label31;
        private System.Windows.Forms.ComboBox Peer;
        private System.Windows.Forms.CheckBox SaveData;
        private System.Windows.Forms.TextBox Port;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.Button BthListenBtn;
        private System.Windows.Forms.Button BthConnectBtn;
        private System.Windows.Forms.Button BthSendBtn;
        private System.Windows.Forms.TextBox BthSendBox;
        private System.Windows.Forms.TextBox BthRecvBox;
        private System.Windows.Forms.Label BthDeviceLabel;
        private System.Windows.Forms.ComboBox BthDeviceList;
        private System.Windows.Forms.Button BthDeviceScanBtn;
        private System.Windows.Forms.Label ChoseSlaveBthLabel;
        private System.Windows.Forms.ComboBox ChoseSlaveBthList;
    }
}

