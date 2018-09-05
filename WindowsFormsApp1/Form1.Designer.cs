namespace WindowsFormsApp1
{
    partial class Form1
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
        const string proname = "小车上位机";
        const string prover = "V5.0";
        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.button2 = new System.Windows.Forms.Button();
            this.serialPort1 = new System.IO.Ports.SerialPort(this.components);
            this.comboBox2 = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.trackBar2 = new System.Windows.Forms.TrackBar();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.button6 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.label19 = new System.Windows.Forms.Label();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.button8 = new System.Windows.Forms.Button();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.label21 = new System.Windows.Forms.Label();
            this.label23 = new System.Windows.Forms.Label();
            this.label24 = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.comboBox4 = new System.Windows.Forms.ComboBox();
            this.label29 = new System.Windows.Forms.Label();
            this.textBox8 = new System.Windows.Forms.TextBox();
            this.label28 = new System.Windows.Forms.Label();
            this.textBox7 = new System.Windows.Forms.TextBox();
            this.comboBox3 = new System.Windows.Forms.ComboBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.radioButton4 = new System.Windows.Forms.RadioButton();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.label27 = new System.Windows.Forms.Label();
            this.label26 = new System.Windows.Forms.Label();
            this.button9 = new System.Windows.Forms.Button();
            this.textBox6 = new System.Windows.Forms.TextBox();
            this.label22 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.button10 = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.button18 = new System.Windows.Forms.Button();
            this.textBox15 = new System.Windows.Forms.TextBox();
            this.checkBox11 = new System.Windows.Forms.CheckBox();
            this.button17 = new System.Windows.Forms.Button();
            this.textBox14 = new System.Windows.Forms.TextBox();
            this.checkBox10 = new System.Windows.Forms.CheckBox();
            this.button16 = new System.Windows.Forms.Button();
            this.textBox13 = new System.Windows.Forms.TextBox();
            this.checkBox9 = new System.Windows.Forms.CheckBox();
            this.button15 = new System.Windows.Forms.Button();
            this.textBox12 = new System.Windows.Forms.TextBox();
            this.checkBox8 = new System.Windows.Forms.CheckBox();
            this.button14 = new System.Windows.Forms.Button();
            this.textBox11 = new System.Windows.Forms.TextBox();
            this.checkBox7 = new System.Windows.Forms.CheckBox();
            this.button13 = new System.Windows.Forms.Button();
            this.textBox10 = new System.Windows.Forms.TextBox();
            this.checkBox6 = new System.Windows.Forms.CheckBox();
            this.button12 = new System.Windows.Forms.Button();
            this.textBox9 = new System.Windows.Forms.TextBox();
            this.checkBox5 = new System.Windows.Forms.CheckBox();
            this.label31 = new System.Windows.Forms.Label();
            this.label30 = new System.Windows.Forms.Label();
            this.button11 = new System.Windows.Forms.Button();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.label32 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar2)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("华文新魏", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.ForeColor = System.Drawing.Color.Red;
            this.label1.Location = new System.Drawing.Point(435, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "状态";
            this.label1.DoubleClick += new System.EventHandler(this.label1_DoubleClick);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(423, 268);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(83, 33);
            this.button1.TabIndex = 1;
            this.button1.Text = "发送";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(173, 24);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(233, 216);
            this.textBox1.TabIndex = 2;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(173, 270);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(233, 63);
            this.textBox2.TabIndex = 3;
            this.textBox2.Leave += new System.EventHandler(this.textBox2_Leave);
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(81, 26);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(86, 23);
            this.comboBox1.TabIndex = 4;
            this.comboBox1.DropDown += new System.EventHandler(this.comboBox1_DropDown);
            this.comboBox1.Leave += new System.EventHandler(this.comboBox1_Leave);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(431, 56);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(89, 19);
            this.checkBox1.TabIndex = 5;
            this.checkBox1.Text = "自动发送";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.radioButton2);
            this.panel1.Controls.Add(this.radioButton1);
            this.panel1.Location = new System.Drawing.Point(431, 128);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(68, 117);
            this.panel1.TabIndex = 6;
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Checked = true;
            this.radioButton2.Location = new System.Drawing.Point(3, 71);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(60, 19);
            this.radioButton2.TabIndex = 1;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "CHAR";
            this.radioButton2.UseVisualStyleBackColor = true;
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(3, 19);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(52, 19);
            this.radioButton1.TabIndex = 0;
            this.radioButton1.Text = "HEX";
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // button2
            // 
            this.button2.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.button2.Location = new System.Drawing.Point(423, 302);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(83, 31);
            this.button2.TabIndex = 7;
            this.button2.Tag = "";
            this.button2.Text = "开始";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // serialPort1
            // 
            this.serialPort1.BaudRate = 115200;
            // 
            // comboBox2
            // 
            this.comboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox2.FormattingEnabled = true;
            this.comboBox2.Location = new System.Drawing.Point(81, 57);
            this.comboBox2.Name = "comboBox2";
            this.comboBox2.Size = new System.Drawing.Size(86, 23);
            this.comboBox2.TabIndex = 8;
            this.comboBox2.SelectionChangeCommitted += new System.EventHandler(this.comboBox2_SelectionChangeCommitted);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(38, 32);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 15);
            this.label2.TabIndex = 11;
            this.label2.Text = "端口";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(23, 60);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(52, 15);
            this.label3.TabIndex = 12;
            this.label3.Text = "波特率";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(81, 86);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(86, 31);
            this.button3.TabIndex = 13;
            this.button3.Text = "打开串口";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // trackBar1
            // 
            this.trackBar1.Location = new System.Drawing.Point(34, 45);
            this.trackBar1.Maximum = 100;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.trackBar1.Size = new System.Drawing.Size(56, 151);
            this.trackBar1.SmallChange = 25;
            this.trackBar1.TabIndex = 14;
            this.trackBar1.Value = 50;
            this.trackBar1.Scroll += new System.EventHandler(this.trackBar1_Scroll);
            this.trackBar1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.trackBar1_MouseUp);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(22, 24);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(52, 15);
            this.label4.TabIndex = 15;
            this.label4.Text = "速度：";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(77, 24);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(23, 15);
            this.label5.TabIndex = 16;
            this.label5.Text = "50";
            // 
            // trackBar2
            // 
            this.trackBar2.Location = new System.Drawing.Point(146, 113);
            this.trackBar2.Maximum = 100;
            this.trackBar2.Name = "trackBar2";
            this.trackBar2.Size = new System.Drawing.Size(150, 56);
            this.trackBar2.SmallChange = 25;
            this.trackBar2.TabIndex = 17;
            this.trackBar2.Value = 50;
            this.trackBar2.Scroll += new System.EventHandler(this.trackBar2_Scroll);
            this.trackBar2.MouseUp += new System.Windows.Forms.MouseEventHandler(this.trackBar2_MouseUp);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(157, 81);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(52, 15);
            this.label6.TabIndex = 18;
            this.label6.Text = "方向：";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(206, 81);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(23, 15);
            this.label7.TabIndex = 19;
            this.label7.Text = "50";
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(180, 199);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 29);
            this.button4.TabIndex = 20;
            this.button4.Text = "回正";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(25, 199);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(75, 29);
            this.button5.TabIndex = 21;
            this.button5.Text = "停车";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 24);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(60, 15);
            this.label8.TabIndex = 23;
            this.label8.Text = "偏航角:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(89, 24);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(60, 15);
            this.label9.TabIndex = 24;
            this.label9.Text = "横滚角:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(171, 24);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(60, 15);
            this.label10.TabIndex = 25;
            this.label10.Text = "俯仰角:";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(6, 65);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(15, 15);
            this.label11.TabIndex = 26;
            this.label11.Text = "0";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(89, 65);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(15, 15);
            this.label12.TabIndex = 27;
            this.label12.Text = "0";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(177, 65);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(15, 15);
            this.label13.TabIndex = 28;
            this.label13.Text = "0";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(251, 24);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(45, 15);
            this.label14.TabIndex = 29;
            this.label14.Text = "航程:";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(251, 65);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(15, 15);
            this.label15.TabIndex = 30;
            this.label15.Text = "0";
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(80, 268);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(86, 31);
            this.button6.TabIndex = 31;
            this.button6.Text = "清空接收";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(80, 302);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(86, 31);
            this.button7.TabIndex = 32;
            this.button7.Text = "清空发送";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(431, 83);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(101, 25);
            this.textBox3.TabIndex = 33;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("宋体", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label16.Location = new System.Drawing.Point(537, 76);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(38, 25);
            this.label16.TabIndex = 34;
            this.label16.Text = "ms";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(191, 338);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(31, 15);
            this.label17.TabIndex = 35;
            this.label17.Text = "Rx:";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(228, 338);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(15, 15);
            this.label18.TabIndex = 36;
            this.label18.Text = "0";
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.AutoSize = true;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(9, 6);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(0, 0);
            this.flowLayoutPanel2.TabIndex = 37;
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(147, 21);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(82, 15);
            this.label19.TabIndex = 40;
            this.label19.Text = "按键步进：";
            // 
            // textBox4
            // 
            this.textBox4.Location = new System.Drawing.Point(150, 45);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(100, 25);
            this.textBox4.TabIndex = 41;
            this.textBox4.Text = "25";
            this.textBox4.Leave += new System.EventHandler(this.textBox4_Leave);
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(308, 27);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(200, 200);
            this.button8.TabIndex = 42;
            this.button8.Text = "摇杆开";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            this.button8.MouseMove += new System.Windows.Forms.MouseEventHandler(this.button8_MouseMove);
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.Checked = true;
            this.checkBox3.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox3.Location = new System.Drawing.Point(431, 339);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(89, 19);
            this.checkBox3.TabIndex = 43;
            this.checkBox3.Text = "本地回显";
            this.checkBox3.UseVisualStyleBackColor = true;
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(6, 21);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(547, 15);
            this.label21.TabIndex = 47;
            this.label21.Text = "YAW:25(此为数据)/OVER  ROLL:70/OVER  PITCH:36/OVER  DISTANCE:99/OVER";
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(6, 24);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(87, 15);
            this.label23.TabIndex = 49;
            this.label23.Text = "FB:25:OVER";
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(6, 49);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(87, 15);
            this.label24.TabIndex = 50;
            this.label24.Text = "RL:50:OVER";
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(6, 74);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(203, 15);
            this.label25.TabIndex = 51;
            this.label25.Text = "FR:25(速度):50(方向):OVER";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label25);
            this.groupBox1.Controls.Add(this.label23);
            this.groupBox1.Controls.Add(this.label24);
            this.groupBox1.Location = new System.Drawing.Point(905, 315);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(220, 100);
            this.groupBox1.TabIndex = 52;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "发送数据格式:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label21);
            this.groupBox2.Location = new System.Drawing.Point(4, 370);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(577, 46);
            this.groupBox2.TabIndex = 53;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "接收数据格式:";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.comboBox4);
            this.groupBox3.Controls.Add(this.label29);
            this.groupBox3.Controls.Add(this.textBox8);
            this.groupBox3.Controls.Add(this.label28);
            this.groupBox3.Controls.Add(this.textBox7);
            this.groupBox3.Controls.Add(this.comboBox3);
            this.groupBox3.Controls.Add(this.panel2);
            this.groupBox3.Controls.Add(this.checkBox3);
            this.groupBox3.Controls.Add(this.button7);
            this.groupBox3.Controls.Add(this.label17);
            this.groupBox3.Controls.Add(this.label18);
            this.groupBox3.Controls.Add(this.label27);
            this.groupBox3.Controls.Add(this.label26);
            this.groupBox3.Controls.Add(this.button9);
            this.groupBox3.Controls.Add(this.textBox6);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.button3);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.comboBox1);
            this.groupBox3.Controls.Add(this.comboBox2);
            this.groupBox3.Controls.Add(this.label16);
            this.groupBox3.Controls.Add(this.label22);
            this.groupBox3.Controls.Add(this.textBox3);
            this.groupBox3.Controls.Add(this.label20);
            this.groupBox3.Controls.Add(this.button6);
            this.groupBox3.Controls.Add(this.checkBox1);
            this.groupBox3.Controls.Add(this.panel1);
            this.groupBox3.Controls.Add(this.button1);
            this.groupBox3.Controls.Add(this.button2);
            this.groupBox3.Controls.Add(this.textBox2);
            this.groupBox3.Controls.Add(this.textBox1);
            this.groupBox3.Location = new System.Drawing.Point(4, 0);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(577, 364);
            this.groupBox3.TabIndex = 54;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "串口工作区:";
            // 
            // comboBox4
            // 
            this.comboBox4.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox4.FormattingEnabled = true;
            this.comboBox4.Location = new System.Drawing.Point(6, 143);
            this.comboBox4.Name = "comboBox4";
            this.comboBox4.Size = new System.Drawing.Size(161, 23);
            this.comboBox4.TabIndex = 50;
            this.comboBox4.DropDown += new System.EventHandler(this.comboBox4_DropDown);
            // 
            // label29
            // 
            this.label29.AutoSize = true;
            this.label29.Location = new System.Drawing.Point(305, 245);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(47, 15);
            this.label29.TabIndex = 49;
            this.label29.Text = "Port:";
            // 
            // textBox8
            // 
            this.textBox8.Location = new System.Drawing.Point(352, 242);
            this.textBox8.Name = "textBox8";
            this.textBox8.Size = new System.Drawing.Size(54, 25);
            this.textBox8.TabIndex = 48;
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Location = new System.Drawing.Point(104, 245);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(63, 15);
            this.label28.TabIndex = 47;
            this.label28.Text = "PeerIP:";
            // 
            // textBox7
            // 
            this.textBox7.Location = new System.Drawing.Point(173, 242);
            this.textBox7.Name = "textBox7";
            this.textBox7.Size = new System.Drawing.Size(126, 25);
            this.textBox7.TabIndex = 46;
            // 
            // comboBox3
            // 
            this.comboBox3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox3.FormattingEnabled = true;
            this.comboBox3.Location = new System.Drawing.Point(6, 177);
            this.comboBox3.Name = "comboBox3";
            this.comboBox3.Size = new System.Drawing.Size(54, 23);
            this.comboBox3.TabIndex = 45;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.radioButton4);
            this.panel2.Controls.Add(this.radioButton3);
            this.panel2.Location = new System.Drawing.Point(500, 128);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(75, 117);
            this.panel2.TabIndex = 44;
            // 
            // radioButton4
            // 
            this.radioButton4.AutoSize = true;
            this.radioButton4.Checked = true;
            this.radioButton4.Location = new System.Drawing.Point(0, 71);
            this.radioButton4.Name = "radioButton4";
            this.radioButton4.Size = new System.Drawing.Size(76, 19);
            this.radioButton4.TabIndex = 1;
            this.radioButton4.TabStop = true;
            this.radioButton4.Text = "Serial";
            this.radioButton4.UseVisualStyleBackColor = true;
            // 
            // radioButton3
            // 
            this.radioButton3.AutoSize = true;
            this.radioButton3.Location = new System.Drawing.Point(0, 19);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(52, 19);
            this.radioButton3.TabIndex = 0;
            this.radioButton3.Text = "NET";
            this.radioButton3.UseVisualStyleBackColor = true;
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(8, 125);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(63, 15);
            this.label27.TabIndex = 36;
            this.label27.Text = "HostIP:";
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(60, 180);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(47, 15);
            this.label26.TabIndex = 35;
            this.label26.Text = "Port:";
            // 
            // button9
            // 
            this.button9.Location = new System.Drawing.Point(81, 207);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(86, 30);
            this.button9.TabIndex = 4;
            this.button9.Text = "开始监听";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.btnListen_Click);
            // 
            // textBox6
            // 
            this.textBox6.Location = new System.Drawing.Point(107, 175);
            this.textBox6.Name = "textBox6";
            this.textBox6.Size = new System.Drawing.Size(60, 25);
            this.textBox6.TabIndex = 3;
            this.textBox6.Text = "8080";
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(349, 338);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(15, 15);
            this.label22.TabIndex = 1;
            this.label22.Text = "0";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(312, 338);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(31, 15);
            this.label20.TabIndex = 0;
            this.label20.Text = "Tx:";
            // 
            // groupBox4
            // 
            this.groupBox4.BackColor = System.Drawing.Color.WhiteSmoke;
            this.groupBox4.Controls.Add(this.button8);
            this.groupBox4.Controls.Add(this.textBox4);
            this.groupBox4.Controls.Add(this.button4);
            this.groupBox4.Controls.Add(this.button5);
            this.groupBox4.Controls.Add(this.label19);
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Controls.Add(this.trackBar2);
            this.groupBox4.Controls.Add(this.label7);
            this.groupBox4.Controls.Add(this.label5);
            this.groupBox4.Controls.Add(this.label6);
            this.groupBox4.Controls.Add(this.trackBar1);
            this.groupBox4.Location = new System.Drawing.Point(6, 6);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(517, 252);
            this.groupBox4.TabIndex = 55;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "运动状态控制:";
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(6, 97);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(119, 19);
            this.checkBox2.TabIndex = 39;
            this.checkBox2.Text = "角度路程显示";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.button10);
            this.groupBox5.Controls.Add(this.checkBox2);
            this.groupBox5.Controls.Add(this.label14);
            this.groupBox5.Controls.Add(this.label15);
            this.groupBox5.Controls.Add(this.label10);
            this.groupBox5.Controls.Add(this.label13);
            this.groupBox5.Controls.Add(this.label11);
            this.groupBox5.Controls.Add(this.label12);
            this.groupBox5.Controls.Add(this.label8);
            this.groupBox5.Controls.Add(this.label9);
            this.groupBox5.Location = new System.Drawing.Point(587, 311);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(302, 128);
            this.groupBox5.TabIndex = 56;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "小车姿态:";
            // 
            // button10
            // 
            this.button10.Location = new System.Drawing.Point(204, 89);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(92, 33);
            this.button10.TabIndex = 40;
            this.button10.Text = "清除数据";
            this.button10.UseVisualStyleBackColor = true;
            this.button10.Click += new System.EventHandler(this.button10_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(587, 6);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(542, 303);
            this.tabControl1.TabIndex = 57;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.WhiteSmoke;
            this.tabPage1.Controls.Add(this.groupBox4);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(534, 274);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "运动控制";
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.Color.WhiteSmoke;
            this.tabPage2.Controls.Add(this.button18);
            this.tabPage2.Controls.Add(this.textBox15);
            this.tabPage2.Controls.Add(this.checkBox11);
            this.tabPage2.Controls.Add(this.button17);
            this.tabPage2.Controls.Add(this.textBox14);
            this.tabPage2.Controls.Add(this.checkBox10);
            this.tabPage2.Controls.Add(this.button16);
            this.tabPage2.Controls.Add(this.textBox13);
            this.tabPage2.Controls.Add(this.checkBox9);
            this.tabPage2.Controls.Add(this.button15);
            this.tabPage2.Controls.Add(this.textBox12);
            this.tabPage2.Controls.Add(this.checkBox8);
            this.tabPage2.Controls.Add(this.button14);
            this.tabPage2.Controls.Add(this.textBox11);
            this.tabPage2.Controls.Add(this.checkBox7);
            this.tabPage2.Controls.Add(this.button13);
            this.tabPage2.Controls.Add(this.textBox10);
            this.tabPage2.Controls.Add(this.checkBox6);
            this.tabPage2.Controls.Add(this.button12);
            this.tabPage2.Controls.Add(this.textBox9);
            this.tabPage2.Controls.Add(this.checkBox5);
            this.tabPage2.Controls.Add(this.label31);
            this.tabPage2.Controls.Add(this.label30);
            this.tabPage2.Controls.Add(this.button11);
            this.tabPage2.Controls.Add(this.textBox5);
            this.tabPage2.Controls.Add(this.checkBox4);
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(534, 274);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "批量字串";
            // 
            // button18
            // 
            this.button18.Location = new System.Drawing.Point(454, 243);
            this.button18.Name = "button18";
            this.button18.Size = new System.Drawing.Size(75, 26);
            this.button18.TabIndex = 25;
            this.button18.Text = "发送";
            this.button18.UseVisualStyleBackColor = true;
            this.button18.Click += new System.EventHandler(this.button18_Click);
            // 
            // textBox15
            // 
            this.textBox15.Location = new System.Drawing.Point(30, 243);
            this.textBox15.Name = "textBox15";
            this.textBox15.Size = new System.Drawing.Size(418, 25);
            this.textBox15.TabIndex = 24;
            // 
            // checkBox11
            // 
            this.checkBox11.AutoSize = true;
            this.checkBox11.Location = new System.Drawing.Point(6, 246);
            this.checkBox11.Name = "checkBox11";
            this.checkBox11.Size = new System.Drawing.Size(18, 17);
            this.checkBox11.TabIndex = 23;
            this.checkBox11.UseVisualStyleBackColor = true;
            // 
            // button17
            // 
            this.button17.Location = new System.Drawing.Point(454, 212);
            this.button17.Name = "button17";
            this.button17.Size = new System.Drawing.Size(75, 26);
            this.button17.TabIndex = 22;
            this.button17.Text = "发送";
            this.button17.UseVisualStyleBackColor = true;
            this.button17.Click += new System.EventHandler(this.button17_Click);
            // 
            // textBox14
            // 
            this.textBox14.Location = new System.Drawing.Point(30, 212);
            this.textBox14.Name = "textBox14";
            this.textBox14.Size = new System.Drawing.Size(418, 25);
            this.textBox14.TabIndex = 21;
            // 
            // checkBox10
            // 
            this.checkBox10.AutoSize = true;
            this.checkBox10.Location = new System.Drawing.Point(6, 215);
            this.checkBox10.Name = "checkBox10";
            this.checkBox10.Size = new System.Drawing.Size(18, 17);
            this.checkBox10.TabIndex = 20;
            this.checkBox10.UseVisualStyleBackColor = true;
            // 
            // button16
            // 
            this.button16.Location = new System.Drawing.Point(454, 181);
            this.button16.Name = "button16";
            this.button16.Size = new System.Drawing.Size(75, 26);
            this.button16.TabIndex = 19;
            this.button16.Text = "发送";
            this.button16.UseVisualStyleBackColor = true;
            this.button16.Click += new System.EventHandler(this.button16_Click);
            // 
            // textBox13
            // 
            this.textBox13.Location = new System.Drawing.Point(30, 181);
            this.textBox13.Name = "textBox13";
            this.textBox13.Size = new System.Drawing.Size(418, 25);
            this.textBox13.TabIndex = 18;
            // 
            // checkBox9
            // 
            this.checkBox9.AutoSize = true;
            this.checkBox9.Location = new System.Drawing.Point(6, 184);
            this.checkBox9.Name = "checkBox9";
            this.checkBox9.Size = new System.Drawing.Size(18, 17);
            this.checkBox9.TabIndex = 17;
            this.checkBox9.UseVisualStyleBackColor = true;
            // 
            // button15
            // 
            this.button15.Location = new System.Drawing.Point(454, 150);
            this.button15.Name = "button15";
            this.button15.Size = new System.Drawing.Size(75, 26);
            this.button15.TabIndex = 16;
            this.button15.Text = "发送";
            this.button15.UseVisualStyleBackColor = true;
            this.button15.Click += new System.EventHandler(this.button15_Click);
            // 
            // textBox12
            // 
            this.textBox12.Location = new System.Drawing.Point(30, 150);
            this.textBox12.Name = "textBox12";
            this.textBox12.Size = new System.Drawing.Size(418, 25);
            this.textBox12.TabIndex = 15;
            // 
            // checkBox8
            // 
            this.checkBox8.AutoSize = true;
            this.checkBox8.Location = new System.Drawing.Point(6, 153);
            this.checkBox8.Name = "checkBox8";
            this.checkBox8.Size = new System.Drawing.Size(18, 17);
            this.checkBox8.TabIndex = 14;
            this.checkBox8.UseVisualStyleBackColor = true;
            // 
            // button14
            // 
            this.button14.Location = new System.Drawing.Point(454, 119);
            this.button14.Name = "button14";
            this.button14.Size = new System.Drawing.Size(75, 26);
            this.button14.TabIndex = 13;
            this.button14.Text = "发送";
            this.button14.UseVisualStyleBackColor = true;
            this.button14.Click += new System.EventHandler(this.button14_Click);
            // 
            // textBox11
            // 
            this.textBox11.Location = new System.Drawing.Point(30, 119);
            this.textBox11.Name = "textBox11";
            this.textBox11.Size = new System.Drawing.Size(418, 25);
            this.textBox11.TabIndex = 12;
            // 
            // checkBox7
            // 
            this.checkBox7.AutoSize = true;
            this.checkBox7.Location = new System.Drawing.Point(6, 122);
            this.checkBox7.Name = "checkBox7";
            this.checkBox7.Size = new System.Drawing.Size(18, 17);
            this.checkBox7.TabIndex = 11;
            this.checkBox7.UseVisualStyleBackColor = true;
            // 
            // button13
            // 
            this.button13.Location = new System.Drawing.Point(454, 86);
            this.button13.Name = "button13";
            this.button13.Size = new System.Drawing.Size(75, 26);
            this.button13.TabIndex = 10;
            this.button13.Text = "发送";
            this.button13.UseVisualStyleBackColor = true;
            this.button13.Click += new System.EventHandler(this.button13_Click);
            // 
            // textBox10
            // 
            this.textBox10.Location = new System.Drawing.Point(30, 86);
            this.textBox10.Name = "textBox10";
            this.textBox10.Size = new System.Drawing.Size(418, 25);
            this.textBox10.TabIndex = 9;
            // 
            // checkBox6
            // 
            this.checkBox6.AutoSize = true;
            this.checkBox6.Location = new System.Drawing.Point(6, 89);
            this.checkBox6.Name = "checkBox6";
            this.checkBox6.Size = new System.Drawing.Size(18, 17);
            this.checkBox6.TabIndex = 8;
            this.checkBox6.UseVisualStyleBackColor = true;
            // 
            // button12
            // 
            this.button12.Location = new System.Drawing.Point(454, 55);
            this.button12.Name = "button12";
            this.button12.Size = new System.Drawing.Size(75, 26);
            this.button12.TabIndex = 7;
            this.button12.Text = "发送";
            this.button12.UseVisualStyleBackColor = true;
            this.button12.Click += new System.EventHandler(this.button12_Click);
            // 
            // textBox9
            // 
            this.textBox9.Location = new System.Drawing.Point(30, 55);
            this.textBox9.Name = "textBox9";
            this.textBox9.Size = new System.Drawing.Size(418, 25);
            this.textBox9.TabIndex = 6;
            // 
            // checkBox5
            // 
            this.checkBox5.AutoSize = true;
            this.checkBox5.Location = new System.Drawing.Point(6, 58);
            this.checkBox5.Name = "checkBox5";
            this.checkBox5.Size = new System.Drawing.Size(18, 17);
            this.checkBox5.TabIndex = 5;
            this.checkBox5.UseVisualStyleBackColor = true;
            // 
            // label31
            // 
            this.label31.AutoSize = true;
            this.label31.Location = new System.Drawing.Point(197, 3);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(82, 15);
            this.label31.TabIndex = 4;
            this.label31.Text = "待发送字串";
            // 
            // label30
            // 
            this.label30.AutoSize = true;
            this.label30.Location = new System.Drawing.Point(3, 5);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(31, 15);
            this.label30.TabIndex = 3;
            this.label30.Text = "HEX";
            // 
            // button11
            // 
            this.button11.Location = new System.Drawing.Point(454, 23);
            this.button11.Name = "button11";
            this.button11.Size = new System.Drawing.Size(75, 26);
            this.button11.TabIndex = 2;
            this.button11.Text = "发送";
            this.button11.UseVisualStyleBackColor = true;
            this.button11.Click += new System.EventHandler(this.button11_Click);
            // 
            // textBox5
            // 
            this.textBox5.Location = new System.Drawing.Point(30, 23);
            this.textBox5.Name = "textBox5";
            this.textBox5.Size = new System.Drawing.Size(418, 25);
            this.textBox5.TabIndex = 1;
            // 
            // checkBox4
            // 
            this.checkBox4.AutoSize = true;
            this.checkBox4.Location = new System.Drawing.Point(6, 26);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(18, 17);
            this.checkBox4.TabIndex = 0;
            this.checkBox4.UseVisualStyleBackColor = true;
            // 
            // label32
            // 
            this.label32.AutoSize = true;
            this.label32.Location = new System.Drawing.Point(6, 418);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(457, 15);
            this.label32.TabIndex = 58;
            this.label32.Text = "若无法从其他终端上通过网络连接此程序，请允许此程序通过防火墙";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1134, 439);
            this.Controls.Add(this.label32);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.flowLayoutPanel2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox5);
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "小车上位机V5.0";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Move += new System.EventHandler(this.Form1_Move);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar2)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button button2;
        private System.IO.Ports.SerialPort serialPort1;
        private System.Windows.Forms.ComboBox comboBox2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TrackBar trackBar2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.CheckBox checkBox3;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.TextBox textBox6;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.RadioButton radioButton4;
        private System.Windows.Forms.RadioButton radioButton3;
        private System.Windows.Forms.ComboBox comboBox3;
        private System.Windows.Forms.Label label29;
        private System.Windows.Forms.TextBox textBox8;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.TextBox textBox7;
        private System.Windows.Forms.ComboBox comboBox4;
        private System.Windows.Forms.Button button10;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button button11;
        private System.Windows.Forms.TextBox textBox5;
        private System.Windows.Forms.CheckBox checkBox4;
        private System.Windows.Forms.Button button18;
        private System.Windows.Forms.TextBox textBox15;
        private System.Windows.Forms.CheckBox checkBox11;
        private System.Windows.Forms.Button button17;
        private System.Windows.Forms.TextBox textBox14;
        private System.Windows.Forms.CheckBox checkBox10;
        private System.Windows.Forms.Button button16;
        private System.Windows.Forms.TextBox textBox13;
        private System.Windows.Forms.CheckBox checkBox9;
        private System.Windows.Forms.Button button15;
        private System.Windows.Forms.TextBox textBox12;
        private System.Windows.Forms.CheckBox checkBox8;
        private System.Windows.Forms.Button button14;
        private System.Windows.Forms.TextBox textBox11;
        private System.Windows.Forms.CheckBox checkBox7;
        private System.Windows.Forms.Button button13;
        private System.Windows.Forms.TextBox textBox10;
        private System.Windows.Forms.CheckBox checkBox6;
        private System.Windows.Forms.Button button12;
        private System.Windows.Forms.TextBox textBox9;
        private System.Windows.Forms.CheckBox checkBox5;
        private System.Windows.Forms.Label label31;
        private System.Windows.Forms.Label label30;
        private System.Windows.Forms.Label label32;
    }
}

