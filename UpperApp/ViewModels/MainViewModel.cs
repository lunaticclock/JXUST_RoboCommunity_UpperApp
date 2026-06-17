using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using UpperApp.Commands;
using UpperApp.Communication;
using UpperApp.Core;
using UpperApp.Processing;
using UpperApp.Services;
using UpperApp.UI;

namespace UpperApp.ViewModels
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    internal class MainViewModel : ViewModelBase
    {
        private readonly FileLogger _logger = new();
        private readonly MessageProcessor _msgProcessor;
        private readonly DeviceService _deviceService;
        private readonly DataPipeline _receivePipeline;
        private readonly IConfigStorage _configStorage;
        private MapTracker _mapTracker;
        private TrafficChart _trafficChart;
        private readonly DispatcherTimer _sendTimer;
        private readonly DispatcherTimer _memTimer;
        private readonly DispatcherTimer _rockerSendTimer;
        private readonly DispatcherTimer _monitorTimer;
        private volatile bool _rockerDirty;
        private int _cnt, _counter;
        private int _rxCount, _txCount;
        private int _lastRxCount, _lastTxCount;

        #region Bindable Properties

        private string _recvText = "";
        public string RecvText { get => _recvText; set => SetField(ref _recvText, value); }

        private string _sendText = "";
        public string SendText { get => _sendText; set => SetField(ref _sendText, value); }

        private string _statusText = "就绪";
        public string StatusText { get => _statusText; set => SetField(ref _statusText, value); }

        private string _rxCountStr = "0";
        public string RxCount { get => _rxCountStr; set => SetField(ref _rxCountStr, value); }

        private string _txCountStr = "0";
        public string TxCount { get => _txCountStr; set => SetField(ref _txCountStr, value); }

        private string _memUsage = "0.0M";
        public string MemUsage { get => _memUsage; set => SetField(ref _memUsage, value); }

        // Serial
        private ObservableCollection<string> _serialPorts = [];
        public ObservableCollection<string> SerialPorts { get => _serialPorts; set => SetField(ref _serialPorts, value); }

        private string _selectedSerialPort = "";
        public string SelectedSerialPort { get => _selectedSerialPort; set => SetField(ref _selectedSerialPort, value); }

        private string _selectedBaudRate = "115200";
        public string SelectedBaudRate { get => _selectedBaudRate; set => SetField(ref _selectedBaudRate, value); }

        public ObservableCollection<string> BaudRates { get; } = ["9600", "19200", "38400", "115200", "256000", "460800", "512000", "921600"];

        private string _serialButtonText = "打开串口";
        public string SerialButtonText { get => _serialButtonText; set => SetField(ref _serialButtonText, value); }

        private bool _isSerialOpen;
        public bool IsSerialOpen { get => _isSerialOpen; set { if (SetField(ref _isSerialOpen, value)) OnPropertyChanged(nameof(IsSerialConfigEnabled)); } }
        public bool IsSerialConfigEnabled => !_isSerialOpen;

        // Network
        private ObservableCollection<string> _localIPs = [];
        public ObservableCollection<string> LocalIPs { get => _localIPs; set => SetField(ref _localIPs, value); }

        private string _selectedHostIP = "";
        public string SelectedHostIP { get => _selectedHostIP; set => SetField(ref _selectedHostIP, value); }

        private string _port = "1234";
        public string Port { get => _port; set => SetField(ref _port, value); }

        private string _selectedNetType = "TCP";
        public string SelectedNetType
        {
            get => _selectedNetType;
            set
            {
                if (SetField(ref _selectedNetType, value) && _isNetChannel)
                    _deviceService.ActiveChannel = value == "TCP" ? ChannelType.TCP : ChannelType.UDP;
            }
        }

        public ObservableCollection<string> NetTypes { get; } = ["TCP", "UDP"];

        private string _listenButtonText = "开始监听";
        public string ListenButtonText { get => _listenButtonText; set => SetField(ref _listenButtonText, value); }

        private bool _isNetListening;
        public bool IsNetListening { get => _isNetListening; set { if (SetField(ref _isNetListening, value)) OnPropertyChanged(nameof(IsNetConfigEnabled)); } }
        public bool IsNetConfigEnabled => !_isNetListening;

        private ObservableCollection<string> _peerList = [];
        public ObservableCollection<string> PeerList { get => _peerList; set => SetField(ref _peerList, value); }

        private string _selectedPeer = "";
        public string SelectedPeer { get => _selectedPeer; set { if (SetField(ref _selectedPeer, value)) _deviceService.SetTarget(value); } }

        // Bluetooth
        private string _bthListenButtonText = "启动监听";
        public string BthListenButtonText { get => _bthListenButtonText; set => SetField(ref _bthListenButtonText, value); }

        private string _bthServerSendText = "";
        public string BthServerSendText { get => _bthServerSendText; set => SetField(ref _bthServerSendText, value); }

        private string _bthClientSendText = "";
        public string BthClientSendText { get => _bthClientSendText; set => SetField(ref _bthClientSendText, value); }

        private string _bthScanButtonText = "扫描蓝牙";
        public string BthScanButtonText { get => _bthScanButtonText; set => SetField(ref _bthScanButtonText, value); }

        private ObservableCollection<string> _bthDeviceList = [];
        public ObservableCollection<string> BthDeviceList { get => _bthDeviceList; set => SetField(ref _bthDeviceList, value); }

        private string _selectedBthDevice = "";
        public string SelectedBthDevice { get => _selectedBthDevice; set => SetField(ref _selectedBthDevice, value); }

        private ObservableCollection<string> _bthSlaveList = [];
        public ObservableCollection<string> BthSlaveList { get => _bthSlaveList; set => SetField(ref _bthSlaveList, value); }

        private string _selectedBthSlave = "";
        public string SelectedBthSlave { get => _selectedBthSlave; set { if (SetField(ref _selectedBthSlave, value)) _deviceService.SetBluetoothTarget(value); } }

        private string _bthConnectButtonText = "连接";
        public string BthConnectButtonText { get => _bthConnectButtonText; set => SetField(ref _bthConnectButtonText, value); }

        private string _bthRadioStatus = "蓝牙未启动";
        public string BthRadioStatus { get => _bthRadioStatus; set => SetField(ref _bthRadioStatus, value); }

        private System.Windows.Media.Brush _bthStatusColor = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x66, 0x66, 0x66));
        public System.Windows.Media.Brush BthStatusColor { get => _bthStatusColor; set => SetField(ref _bthStatusColor, value); }

        // Channel
        private bool _isSerialChannel = true;
        public bool IsSerialChannel
        {
            get => _isSerialChannel;
            set
            {
                if (value && !_isSerialChannel)
                {
                    _isSerialChannel = true; _isNetChannel = false; _isBtChannel = false;
                    _deviceService.ActiveChannel = ChannelType.Serial;
                    OnPropertyChanged(nameof(IsSerialChannel));
                    OnPropertyChanged(nameof(IsNetChannel));
                    OnPropertyChanged(nameof(IsBtChannel));
                }
            }
        }

        private bool _isNetChannel;
        public bool IsNetChannel
        {
            get => _isNetChannel;
            set
            {
                if (value && !_isNetChannel)
                {
                    _isSerialChannel = false; _isNetChannel = true; _isBtChannel = false;
                    _deviceService.ActiveChannel = SelectedNetType == "TCP" ? ChannelType.TCP : ChannelType.UDP;
                    OnPropertyChanged(nameof(IsSerialChannel));
                    OnPropertyChanged(nameof(IsNetChannel));
                    OnPropertyChanged(nameof(IsBtChannel));
                }
            }
        }

        private bool _isBtChannel;
        public bool IsBtChannel
        {
            get => _isBtChannel;
            set
            {
                if (value && !_isBtChannel)
                {
                    _isSerialChannel = false; _isNetChannel = false; _isBtChannel = true;
                    _deviceService.ActiveChannel = ChannelType.Bluetooth;
                    OnPropertyChanged(nameof(IsSerialChannel));
                    OnPropertyChanged(nameof(IsNetChannel));
                    OnPropertyChanged(nameof(IsBtChannel));
                }
            }
        }

        // Display mode
        private bool _isHexMode;
        public bool IsHexMode { get => _isHexMode; set => SetField(ref _isHexMode, value); }

        private bool _isCharMode = true;
        public bool IsCharMode { get => _isCharMode; set => SetField(ref _isCharMode, value); }

        // Auto send
        private bool _autoSendEnabled;
        public bool AutoSendEnabled { get => _autoSendEnabled; set => SetField(ref _autoSendEnabled, value); }

        private string _autoSendInterval = "1000";
        public string AutoSendInterval { get => _autoSendInterval; set => SetField(ref _autoSendInterval, value); }

        private string _autoSendButtonText = "开始";
        public string AutoSendButtonText { get => _autoSendButtonText; set => SetField(ref _autoSendButtonText, value); }

        // Local echo / save
        private bool _localEcho = true;
        public bool LocalEcho { get => _localEcho; set => SetField(ref _localEcho, value); }

        private bool _saveDataEnabled;
        public bool SaveDataEnabled { get => _saveDataEnabled; set => SetField(ref _saveDataEnabled, value); }

        // Motion
        private int _fbValue = 50;
        public int FbValue { get => _fbValue; set { if (SetField(ref _fbValue, value)) OnSliderChanged(); } }

        private int _rlValue = 50;
        public int RlValue { get => _rlValue; set { if (SetField(ref _rlValue, value)) OnSliderChanged(); } }

        /// <summary>
        /// 同时更新速度和方向，只触发一次 OnSliderChanged 节流标记，
        /// 避免摇杆 MouseMove 同时设两个属性导致两次属性变更通知。
        /// </summary>
        public void UpdateMotionValues(int speed, int direction)
        {
            bool changed = false;
            if (_fbValue != speed) { _fbValue = speed; changed = true; }
            if (_rlValue != direction) { _rlValue = direction; changed = true; }
            if (changed)
            {
                OnPropertyChanged(nameof(FbValue));
                OnPropertyChanged(nameof(RlValue));
                OnSliderChanged();
            }
        }

        private double _stepSize = 25;
        public double StepSize { get => _stepSize; set => SetField(ref _stepSize, value); }

        private string _rockerText = "摇杆开";
        public string RockerText { get => _rockerText; set => SetField(ref _rockerText, value); }

        private bool _rockerActive;

        // Attitude
        private string _yaw = "0";
        public string Yaw { get => _yaw; set => SetField(ref _yaw, value); }

        private string _roll = "0";
        public string Roll { get => _roll; set => SetField(ref _roll, value); }

        private string _pitch = "0";
        public string Pitch { get => _pitch; set => SetField(ref _pitch, value); }

        private string _distance = "0";
        public string Distance { get => _distance; set => SetField(ref _distance, value); }

        private bool _angleDisplayEnabled = true;
        public bool AngleDisplayEnabled { get => _angleDisplayEnabled; set => SetField(ref _angleDisplayEnabled, value); }

        // 通道状态码字符串（分号分隔，顺序 Serial/TCP/UDP/BT/WS/CAN；0=断开 1=连接中 2=已连接 3=异常）
        private string _channelStates = "0;0;0;0;0;0";
        public string ChannelStates { get => _channelStates; set => SetField(ref _channelStates, value); }

        // Batch messages
        public ObservableCollection<PresetMessageViewModel> PresetMessages { get; } = [];

        // Map
        private string _mapStartPoint = "0,0";
        public string MapStartPoint { get => _mapStartPoint; set => SetField(ref _mapStartPoint, value); }

        private string _mapEndPoint = "0,0";
        public string MapEndPoint { get => _mapEndPoint; set => SetField(ref _mapEndPoint, value); }

        private string _mousePosition = "0,0";
        public string MousePosition { get => _mousePosition; set => SetField(ref _mousePosition, value); }

        private string _calibratedDistance = "1.00";
        public string CalibratedDistance { get => _calibratedDistance; set { if (SetField(ref _calibratedDistance, value)) OnCalibratedDistanceChanged(); } }

        #endregion

        #region Commands

        public ICommand ToggleSerialCommand { get; }
        public ICommand ToggleListenCommand { get; }
        public ICommand ToggleBluetoothCommand { get; }
        public ICommand BthServerSendCommand { get; }
        public ICommand BthScanCommand { get; }
        public ICommand BthConnectCommand { get; }
        public ICommand BthClientSendCommand { get; }
        public ICommand SendCommand { get; }
        public ICommand ToggleAutoSendCommand { get; }
        public ICommand ClearRecvCommand { get; }
        public ICommand ClearSendCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand ResetDirectionCommand { get; }
        public ICommand ToggleRockerCommand { get; }
        public ICommand ClearAttitudeCommand { get; }
        public ICommand OpenMapCommand { get; }
        public ICommand ClearMapCommand { get; }

        #endregion
        public MainViewModel()
        {
            _configStorage = AppServices.GetService<IConfigStorage>();
            _deviceService = AppServices.GetService<DeviceService>();
            _deviceService.StatusChanged += UnifiedStatusChanged;
            _deviceService.ActiveChannel = ChannelType.Serial;

            _msgProcessor = new MessageProcessor(_logger, () => SaveDataEnabled);
            _receivePipeline = new DataPipeline(DispatchReceivedData);
            _receivePipeline.Start();

            _sendTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            _sendTimer.Tick += SendTimer_Tick;

            _memTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            _memTimer.Tick += MemTimer_Tick;
            _memTimer.Start();

            // 摇杆节流定时器：50ms 间隔批量发送，避免 MouseMove 高频触发同步 I/O 阻塞 UI 线程
            _rockerSendTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            _rockerSendTimer.Tick += RockerSendTimer_Tick;
            _rockerSendTimer.Start();

            // 通信监控定时器：每秒采样 Rx/Tx 增量推入流量曲线 + 轮询通道状态
            _monitorTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _monitorTimer.Tick += MonitorTimer_Tick;
            _monitorTimer.Start();

            ToggleSerialCommand = new RelayCommand(ToggleSerial);
            ToggleListenCommand = new RelayCommand(ToggleListen);
            ToggleBluetoothCommand = new RelayCommand(ToggleBluetooth);
            BthServerSendCommand = new RelayCommand(BthServerSend);
            BthScanCommand = new AsyncRelayCommand(async _ => await BthScan());
            BthConnectCommand = new RelayCommand(BthConnect);
            BthClientSendCommand = new RelayCommand(BthClientSend);
            SendCommand = new RelayCommand(_ => StrSend(SendText));
            ToggleAutoSendCommand = new RelayCommand(ToggleAutoSend);
            ClearRecvCommand = new RelayCommand(_ => { _rxCount = 0; RxCount = "0"; ClearRecvText(); StatusText = "接收区已清空"; });
            ClearSendCommand = new RelayCommand(_ => { _txCount = 0; TxCount = "0"; SendText = ""; StatusText = "发送区已清空"; });
            StopCommand = new RelayCommand(_ => {
                FbValue = 50;
                var cmd = new MoveCommand(MoveCommand.MoveType.ForwardBackward, 50);
                SendAndEcho(cmd, cmd.Encode());
            });
            ResetDirectionCommand = new RelayCommand(_ => {
                RlValue = 50;
                var cmd = new MoveCommand(MoveCommand.MoveType.RightLeft, 50, 50);
                SendAndEcho(cmd, cmd.Encode());
            });
            ToggleRockerCommand = new RelayCommand(_ => {
                _rockerActive = !_rockerActive;
                RockerText = _rockerActive ? "摇杆关" : "摇杆开";
                if (!_rockerActive)
                {
                    FbValue = 50;
                    RlValue = 50;
                    var cmd = new MoveCommand(MoveCommand.MoveType.FullControl, FbValue, RlValue);
                    SendAndEcho(cmd, cmd.Encode());
                }
            });
            ClearAttitudeCommand = new RelayCommand(_ => { Yaw = Roll = Pitch = Distance = "0"; StatusText = "数据清除成功"; });
            OpenMapCommand = new RelayCommand(OpenMap);
            ClearMapCommand = new RelayCommand(_ => { _mapTracker?.Clear(); MapStartPoint = MapEndPoint = "0,0"; });

            for (int i = 0; i < 8; i++)
            {
                PresetMessages.Add(new PresetMessageViewModel((str, isHex) => StrSend(str, isHex)));
            }

            RefreshSerialPorts();
            RefreshLocalIPs();

            _ = LoadSettingsAsync();
        }

        public void SetMapTracker(MapTracker tracker)
        {
            _mapTracker = tracker;
        }

        /// <summary>
        /// 注入流量曲线控件。View 在 Loaded 时调用。
        /// 注入后监控定时器每秒调用 PushSample 推入 Rx/Tx 增量。
        /// </summary>
        public void SetTrafficChart(TrafficChart chart)
        {
            _trafficChart = chart;
        }

        private ILogSink _logSink;

        /// <summary>
        /// 注入流式日志输出。View 在 Loaded 时调用。
        /// 注入后 AppendRecvText 走增量 AppendText，不再全量赋值 RecvText。
        /// </summary>
        public void SetLogSink(ILogSink sink)
        {
            _logSink = sink;
        }

        public void AppendRecvText(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            // 优先走流式增量输出
            if (_logSink != null)
            {
                _logSink.Append(text);
                return;
            }

            // 兜底：未注入 sink 时退化为全量字符串
            const int MaxLength = 500000;
            var newValue = _recvText + text;
            if (newValue.Length > MaxLength)
                newValue = newValue[^MaxLength..];
            RecvText = newValue;
        }

        public void ClearRecvText()
        {
            if (_logSink != null)
            {
                _logSink.Clear();
                _recvText = "";
                OnPropertyChanged(nameof(RecvText));
            }
            else
            {
                RecvText = "";
            }
        }

        private void UnifiedStatusChanged(StatusEvent evt)
        {
            // 接收消息：I/O 线程直接入队 Channel，无需切到 UI 线程（Channel<T> 线程安全）
            if (evt is MessageReceivedEvent recv)
            {
                _receivePipeline.TryEnqueue(recv);
                return;
            }

            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                switch (evt)
                {
                    case MessageSentEvent:
                        // 发送回显已在 SendAndEcho 中直接处理，此处无需操作
                        break;
                    case ExceptionOccurredEvent e:
                        _logger.WriteLine($"[{Utils.GetTime()}] ExceptionStop [{e.Channel}]: {e.Message}");
                        string title = e.Channel switch
                        {
                            ChannelType.Serial => "串口错误",
                            ChannelType.Bluetooth => "蓝牙错误",
                            ChannelType.TCP or ChannelType.UDP => string.IsNullOrEmpty(e.RemoteIP) ? "网络错误" : "远端关闭",
                            _ => "错误"
                        };
                        AppendRecvText($"[{Utils.GetTime()}] [ERROR] {title}: {e.Message}\r\n");
                        StatusText = $"{title}: {e.Message}";
                        break;
                    case MonitorStartedEvent e:
                        UpdateMonitorUI(e.Channel, isStart: true, e.Message);
                        break;
                    case MonitorStoppedEvent e:
                        UpdateMonitorUI(e.Channel, isStart: false, e.Message);
                        break;
                    case PeerConnectedEvent e:
                        if (e.Channel == ChannelType.TCP || e.Channel == ChannelType.UDP)
                        {
                            StatusText = "连接成功！";
                            RefreshPeerList(e.Channel, e.Peer);
                        }
                        else if (e.Channel == ChannelType.Bluetooth)
                        {
                            if (!string.IsNullOrEmpty(e.Message))
                                AppendRecvText(e.Message);
                            RefreshPeerList(ChannelType.Bluetooth, null);
                        }
                        break;
                    case PeerDisconnectedEvent e:
                        if (e.Channel == ChannelType.TCP || e.Channel == ChannelType.UDP)
                        {
                            RefreshPeerList(e.Channel, "");
                            StatusText = e.Reason;
                        }
                        else if (e.Channel == ChannelType.Bluetooth)
                        {
                            StatusText = "连接断开!";
                            BthConnectButtonText = "连接";
                            RefreshPeerList(ChannelType.Bluetooth, null);
                        }
                        break;
                    case ManualStoppedEvent e:
                        if (e.Channel == ChannelType.TCP || e.Channel == ChannelType.UDP)
                            SelectedPeer = "";
                        break;
                }
            });
        }

        private void DispatchReceivedData(MessageReceivedEvent evt)
        {
            var processed = _msgProcessor.ProcessReceivedMessage(evt);
            if (processed == null) return;

            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                if (!string.IsNullOrEmpty(processed.NewPeerHint))
                    AppendRecvText(processed.NewPeerHint);

                if (!string.IsNullOrEmpty(processed.Prefix))
                    AppendRecvText(processed.Prefix);

                string displayContent = IsHexMode
                    ? Utils.StringToHexString(processed.RawContent)
                    : processed.FormattedContent;
                AppendRecvText(displayContent);

                _rxCount += processed.ByteCount;
                RxCount = _rxCount.ToString();

                // 日志已由 MessageProcessor.ProcessReceivedMessage 统一写入，此处不再重复

                if (AngleDisplayEnabled && processed.HasAttitudeData)
                    SetAngDisp(processed.AttitudeRaw);
            });
        }

        private void UpdateMonitorUI(ChannelType channel, bool isStart, string message)
        {
            switch (channel)
            {
                case ChannelType.Serial:
                    SerialButtonText = isStart ? "关闭串口" : "打开串口";
                    IsSerialOpen = isStart;
                    if (isStart) IsSerialChannel = true;
                    StatusText = message;
                    break;
                case ChannelType.TCP:
                    ListenButtonText = isStart ? "停止监听" : "开始监听";
                    IsNetListening = isStart;
                    if (isStart) { IsNetChannel = true; SelectedNetType = "TCP"; _deviceService.ActiveChannel = ChannelType.TCP; SelectedPeer = ""; }
                    StatusText = message;
                    break;
                case ChannelType.UDP:
                    ListenButtonText = isStart ? "停止监听" : "开始监听";
                    IsNetListening = isStart;
                    if (isStart) { IsNetChannel = true; SelectedNetType = "UDP"; _deviceService.ActiveChannel = ChannelType.UDP; SelectedPeer = ""; }
                    StatusText = message;
                    break;
                case ChannelType.Bluetooth:
                    BthListenButtonText = isStart ? "关闭监听" : "启动监听";
                    if (isStart)
                    {
                        BthRadioStatus = $"蓝牙服务端运行中 · {_deviceService.BluetoothRadioAddress}";
                        BthStatusColor = System.Windows.Media.Brushes.ForestGreen;
                    }
                    else
                    {
                        BthRadioStatus = "蓝牙已停止";
                        BthStatusColor = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x66, 0x66, 0x66));
                    }
                    break;
            }
        }

        /// <summary>
        /// 刷新对端列表（TCP/UDP 的 PeerList 或蓝牙的 BthSlaveList）。
        /// 提取自 UnifiedStatusChanged 中 NewRemote/RemoteStop 的内联代码，消除重复。
        /// </summary>
        private void RefreshPeerList(ChannelType channel, string selectedPeer)
        {
            var peers = _deviceService.GetPeerList(channel);
            if (channel == ChannelType.Bluetooth)
            {
                BthSlaveList.Clear();
                foreach (var p in peers)
                    if (!BthSlaveList.Contains(p))
                        BthSlaveList.Add(p);
            }
            else
            {
                PeerList.Clear();
                foreach (var p in peers)
                    if (!PeerList.Contains(p))
                        PeerList.Add(p);
                SelectedPeer = selectedPeer ?? "";
            }
        }

        private void SetAngDisp(string str)
        {
            var parsed = ProtocolHandler.TryParse(str);
            if (!parsed.HasValue) return;
            switch (parsed.Value.Type)
            {
                case ProtocolHandler.DataType.Yaw: Yaw = parsed.Value.Value; break;
                case ProtocolHandler.DataType.Pitch: Pitch = parsed.Value.Value; break;
                case ProtocolHandler.DataType.Roll: Roll = parsed.Value.Value; break;
                case ProtocolHandler.DataType.Distance:
                    Distance = parsed.Value.Value;
                    _mapTracker?.OnDistanceChanged(Distance, Yaw, AppendRecvText);
                    break;
            }
        }

        /// <param name="hexOverride">是否强制 hex 模式（批量字串独立开关）；null 表示用主页面 IsHexMode</param>
        private void StrSend(string buf, bool? hexOverride = null)
        {
            _deviceService.SetTarget(SelectedPeer);
            _deviceService.SetBluetoothTarget(SelectedBthSlave);

            bool useHex = hexOverride ?? IsHexMode;

            // Hex 模式：把用户输入的 hex 串解析为原始字节直接发送，绕过字符编码
            if (useHex)
            {
                byte[] bytes = Utils.ParseHexString(buf);
                if (bytes == null)
                {
                    StatusText = "Hex 格式错误，无法解析";
                    AppendRecvText("[ERROR] Hex 格式错误，无法解析\r\n");
                    return;
                }
                SendBytesAndEcho(bytes, buf);
                return;
            }

            // 字符模式：走原命令链路
            var command = new RawSendCommand(buf, _deviceService.ActiveChannel);
            SendAndEcho(command, buf);
        }

        /// <summary>
        /// 字符模式发送+回显：执行命令发送，成功后直接在 VM 内回显 + 计数。
        /// 回显内容就是用户输入的字符串（不再二次转 hex）。
        /// </summary>
        private void SendAndEcho(IDeviceCommand command, string displayContent)
        {
            if (!_deviceService.TryExecuteCommand(command))
            {
                StatusText = "发送失败：通道未连接或未找到";
                return;
            }

            // 发送字节计数（按 UTF-8 估算，与 Adapter 内部编码可能不同，仅作统计参考）
            int byteCount = Encoding.UTF8.GetByteCount(displayContent);
            _txCount += byteCount;
            TxCount = _txCount.ToString();

            // 本地回显：字符模式下直接显示用户输入的字符串
            if (LocalEcho)
            {
                AppendRecvText(EnsureLineEnding(displayContent));
            }

            // 文件日志
            _logger.WriteLine($"[{Utils.GetTime()}] SEND [{_deviceService.ActiveChannel}]: {displayContent.TrimEnd()}");
        }

        /// <summary>
        /// Hex 模式发送+回显：直接发送原始字节，回显用户输入的 hex 串。
        /// </summary>
        /// <param name="bytes">从 hex 串解析出的原始字节</param>
        /// <param name="hexInput">用户输入的原始 hex 字符串（用于回显）</param>
        private void SendBytesAndEcho(byte[] bytes, string hexInput)
        {
            if (!_deviceService.TrySendBytes(bytes))
            {
                StatusText = "发送失败：通道未连接或未找到";
                return;
            }

            // 发送字节计数：实际发送的字节数
            _txCount += bytes.Length;
            TxCount = _txCount.ToString();

            // 本地回显：hex 模式下直接显示用户输入的 hex 串
            if (LocalEcho)
            {
                AppendRecvText(EnsureLineEnding(hexInput));
            }

            // 文件日志
            _logger.WriteLine($"[{Utils.GetTime()}] SEND [{_deviceService.ActiveChannel}]: {hexInput.TrimEnd()}");
        }

        private static string EnsureLineEnding(string content)
        {
            if (string.IsNullOrEmpty(content)) return content;
            return content.EndsWith("\r\n") || content.EndsWith('\n') ? content : content + "\r\n";
        }

        private void ToggleSerial(object _)
        {
            RefreshSerialPorts();
            if (string.IsNullOrEmpty(SelectedSerialPort))
            {
                StatusText = "未选中串口";
                return;
            }
            if (!_deviceService.IsChannelReady(ChannelType.Serial))
            {
                int baudRate = int.TryParse(SelectedBaudRate, out int b) ? b : 115200;
                _deviceService.StartChannel(ChannelType.Serial, new SerialParams
                {
                    PortName = SelectedSerialPort,
                    BaudRate = baudRate,
                    Parity = Parity.None,
                    DataBits = 8,
                    StopBits = StopBits.One
                });
            }
            else
            {
                _deviceService.StopChannel(ChannelType.Serial);
            }
        }

        private void ToggleListen(object _)
        {
            RefreshLocalIPs();
            bool tcpReady = _deviceService.IsChannelReady(ChannelType.TCP);
            bool udpReady = _deviceService.IsChannelReady(ChannelType.UDP);

            if (tcpReady)
            {
                _deviceService.StopChannel(ChannelType.TCP);
                PeerList.Clear();
                SelectedPeer = "";
                return;
            }
            if (udpReady)
            {
                _deviceService.StopChannel(ChannelType.UDP);
                PeerList.Clear();
                SelectedPeer = "";
                return;
            }

            ChannelType netType = SelectedNetType == "TCP" ? ChannelType.TCP : ChannelType.UDP;
            if (string.IsNullOrEmpty(SelectedHostIP))
            {
                StatusText = "请选择IP";
                return;
            }
            if (!int.TryParse(Port, out int port) || port <= 0 || port > 65535)
            {
                StatusText = "端口无效";
                return;
            }
            if (netType == ChannelType.TCP)
                _deviceService.StartChannel(netType, new TcpServerParams { LocalIP = SelectedHostIP, Port = port });
            else
                _deviceService.StartChannel(netType, new UdpParams { LocalIP = SelectedHostIP, Port = port });
        }

        private void ToggleBluetooth(object _)
        {
            if (!_deviceService.IsBluetoothRadioAvailable)
            {
                StatusText = "检测不到本机蓝牙设备";
                AppendRecvText("[ERROR] 检测不到本机蓝牙设备\r\n");
                return;
            }
            if (!_deviceService.IsBluetoothRadioPoweredOn)
            {
                StatusText = "请先在系统中打开蓝牙";
                AppendRecvText("[WARN] 请先在系统中打开蓝牙\r\n");
                return;
            }
            if (!_deviceService.IsChannelReady(ChannelType.Bluetooth))
            {
                _deviceService.StartChannel(ChannelType.Bluetooth, new BluetoothParams { IsServerMode = true });
                BthListenButtonText = "关闭监听";
                BthRadioStatus = $"蓝牙服务端运行中 · {_deviceService.BluetoothRadioAddress}";
                BthStatusColor = System.Windows.Media.Brushes.ForestGreen;
                AppendRecvText($"[蓝牙] Radio address: {_deviceService.BluetoothRadioAddress}\r\n");
                AppendRecvText($"[蓝牙] Mode: {_deviceService.BluetoothRadioMode}\r\n");
                AppendRecvText("[蓝牙] Service started!\r\n");
            }
            else
            {
                _deviceService.StopChannel(ChannelType.Bluetooth);
                BthListenButtonText = "启动监听";
                BthRadioStatus = "蓝牙已停止";
                BthStatusColor = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x66, 0x66, 0x66));
            }
        }

        private void BthServerSend(object _)
        {
            if (string.IsNullOrEmpty(SelectedBthSlave))
            {
                StatusText = "请先选择已连接的从设备";
                AppendRecvText("[WARN] 请先选择已连接的从设备\r\n");
                return;
            }
            _deviceService.SetBluetoothTarget(SelectedBthSlave);
            var command = new RawSendCommand(BthServerSendText, ChannelType.Bluetooth);
            SendAndEcho(command, BthServerSendText);
        }

        private void BthClientSend(object _)
        {
            if (!_deviceService.IsChannelReady(ChannelType.Bluetooth))
            {
                StatusText = "请先连接设备";
                AppendRecvText("[WARN] 请先连接设备\r\n");
                return;
            }
            _deviceService.SetBluetoothTarget(null);
            var command = new RawSendCommand(BthClientSendText, ChannelType.Bluetooth);
            SendAndEcho(command, BthClientSendText);
        }

        private async Task BthScan()
        {
            BthScanButtonText = "扫描中";
            try
            {
                var devices = await _deviceService.DiscoverBluetoothDevicesAsync();
                BthDeviceList.Clear();
                foreach (var d in devices)
                    if (!BthDeviceList.Contains(d.DeviceName))
                        BthDeviceList.Add(d.DeviceName);
                AppendRecvText($"[蓝牙] 扫描完成，发现 {devices.Count} 个设备。\r\n");
            }
            catch (Exception ex)
            {
                StatusText = $"扫描失败: {ex.Message}";
                AppendRecvText($"[ERROR] 蓝牙扫描失败: {ex.Message}\r\n");
            }
            finally
            {
                BthScanButtonText = "扫描蓝牙";
            }
        }

        private void BthConnect(object _)
        {
            if (string.IsNullOrEmpty(SelectedBthDevice))
            {
                if (!_deviceService.IsChannelReady(ChannelType.Bluetooth))
                {
                    StatusText = "请选择设备";
                    AppendRecvText("[WARN] 请选择设备\r\n");
                }
                else
                {
                    _deviceService.DisconnectBluetoothClient();
                    BthConnectButtonText = "连接";
                }
            }
            else
            {
                _deviceService.ConnectBluetoothDevice(SelectedBthDevice);
                BthConnectButtonText = "断开";
            }
        }

        private void ToggleAutoSend(object _)
        {
            if (AutoSendButtonText == "开始")
            {
                if (!_deviceService.IsAnyChannelReady())
                {
                    StatusText = "端口未打开!";
                    return;
                }
                if (!int.TryParse(AutoSendInterval, out int interval) || interval < 100)
                {
                    StatusText = "时间间隔无效";
                    return;
                }
                AutoSendEnabled = true;
                _counter = interval / 100;
                _cnt = 0;
                _sendTimer.Start();
                AutoSendButtonText = "停止";
            }
            else
            {
                AutoSendEnabled = false;
                _sendTimer.Stop();
                AutoSendButtonText = "开始";
            }
        }

        private void SendTimer_Tick(object sender, EventArgs e)
        {
            if (AutoSendEnabled && ++_cnt >= _counter)
            {
                StrSend(SendText);
                _cnt = 0;
            }
        }

        private void MemTimer_Tick(object sender, EventArgs e)
        {
            double usemem = Process.GetCurrentProcess().PrivateMemorySize64 / 1024.0 / 1024.0;
            MemUsage = $"{usemem:F1}M";
        }

        private void MonitorTimer_Tick(object sender, EventArgs e)
        {
            // 流量采样：计算与上一秒的 Rx/Tx 字节增量
            int rxDelta = _rxCount - _lastRxCount;
            int txDelta = _txCount - _lastTxCount;
            if (rxDelta < 0) rxDelta = 0;
            if (txDelta < 0) txDelta = 0;
            _lastRxCount = _rxCount;
            _lastTxCount = _txCount;
            _trafficChart?.PushSample(rxDelta, txDelta);

            // 通道状态轮询：Serial/TCP/UDP/BT/WS/CAN
            var channels = new ChannelType[]
            {
                ChannelType.Serial,
                ChannelType.TCP,
                ChannelType.UDP,
                ChannelType.Bluetooth,
                ChannelType.WebSocket,
                ChannelType.CAN
            };
            var codes = new int[channels.Length];
            for (int i = 0; i < channels.Length; i++)
            {
                codes[i] = _deviceService.GetChannelState(channels[i]) switch
                {
                    DeviceState.Connected => 2,
                    DeviceState.Connecting => 1,
                    DeviceState.Error => 3,
                    _ => 0
                };
            }
            ChannelStates = string.Join(';', codes);
        }

        private void OnSliderChanged()
        {
            // 节流：只标记需要发送，由 _rockerSendTimer 定时批量发送最新值
            _rockerDirty = true;
        }

        private void RockerSendTimer_Tick(object sender, EventArgs e)
        {
            if (!_rockerDirty) return;
            _rockerDirty = false;
            var command = new MoveCommand(MoveCommand.MoveType.FullControl, FbValue, RlValue);
            SendAndEcho(command, command.Encode());
        }

        private void OnCalibratedDistanceChanged()
        {
            if (float.TryParse(CalibratedDistance, out float dist))
                _mapTracker?.SetCalibratedDistance(dist);
        }

        private void OpenMap(object _)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "所有文件|*.*|图片文件|*.jpg;*.png;*.bmp;*.JPG;*.BMP;*.PNG"
            };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    var bmp = new System.Windows.Media.Imaging.BitmapImage(new Uri(dlg.FileName));
                    _mapTracker?.Clear();
                    _mapTracker?.SetBackgroundImage(bmp);
                }
                catch (Exception ex)
                {
                    StatusText = $"加载图片失败: {ex.Message}";
                    AppendRecvText($"[ERROR] 加载图片失败: {ex.Message}\r\n");
                }
            }
        }

        private void RefreshSerialPorts()
        {
            var saved = _selectedSerialPort;
            var ports = SerialPort.GetPortNames();
            SerialPorts.Clear();
            foreach (var p in ports) SerialPorts.Add(p);
            if (!string.IsNullOrEmpty(saved) && SerialPorts.Contains(saved))
                SelectedSerialPort = saved;
            else if (SerialPorts.Count > 0)
                SelectedSerialPort = SerialPorts[0];
        }

        private void RefreshLocalIPs()
        {
            var saved = _selectedHostIP;
            var ips = Utils.GetLocalIPv4Addresses();
            LocalIPs.Clear();
            foreach (var ip in ips) LocalIPs.Add(ip);
            if (!string.IsNullOrEmpty(saved) && LocalIPs.Contains(saved))
                SelectedHostIP = saved;
            else if (LocalIPs.Count > 0)
                SelectedHostIP = LocalIPs[0];
        }

        private async Task LoadSettingsAsync()
        {
            var settings = await _configStorage.LoadAsync();
            Application.Current.Dispatcher.Invoke(() => ApplySettings(settings));
        }

        private void ApplySettings(AppSettings settings)
        {
            StepSize = settings.SliderSmallChange;
            AutoSendInterval = settings.AutoSendIntervalMs.ToString();
            AutoSendEnabled = settings.AutoSendEnabled;
            LocalEcho = settings.LocalEcho;
            AngleDisplayEnabled = settings.AngleDisplayEnabled;
            SaveDataEnabled = settings.SaveDataEnabled;
            SelectedNetType = settings.SelectedNetType;
            IsHexMode = settings.IsHexMode;
            IsCharMode = settings.IsCharMode;

            if (!string.IsNullOrEmpty(settings.SerialConfig.PortName))
                SelectedSerialPort = settings.SerialConfig.PortName;
            SelectedBaudRate = settings.SerialConfig.BaudRate.ToString();

            if (!string.IsNullOrEmpty(settings.TcpConfig.LocalIP))
                SelectedHostIP = settings.TcpConfig.LocalIP;
            Port = settings.TcpConfig.Port > 0 ? settings.TcpConfig.Port.ToString() : "1234";

            switch (settings.LastActiveSendChannel)
            {
                case ChannelType.Serial: IsSerialChannel = true; break;
                case ChannelType.TCP:
                case ChannelType.UDP: IsNetChannel = true; break;
                case ChannelType.Bluetooth: IsBtChannel = true; break;
            }

            for (int i = 0; i < PresetMessages.Count && i < settings.PresetMessages.Count; i++)
            {
                PresetMessages[i].Message = settings.PresetMessages[i].Text;
                PresetMessages[i].IsHex = settings.PresetMessages[i].IsHex;
            }

            CalibratedDistance = settings.CalibratedDistance.ToString("F2");
        }

        public AppSettings CollectCurrentSettings()
        {
            var settings = new AppSettings
            {
                SliderSmallChange = (int)StepSize,
                AutoSendIntervalMs = int.TryParse(AutoSendInterval, out int interval) ? interval : 1000,
                AutoSendEnabled = AutoSendEnabled,
                LocalEcho = LocalEcho,
                AngleDisplayEnabled = AngleDisplayEnabled,
                SaveDataEnabled = SaveDataEnabled,
                SelectedNetType = SelectedNetType,
                IsHexMode = IsHexMode,
                IsCharMode = IsCharMode,
                LastActiveSendChannel = _deviceService.ActiveChannel,
                SerialConfig = new SerialParams
                {
                    PortName = SelectedSerialPort,
                    BaudRate = int.TryParse(SelectedBaudRate, out int baud) ? baud : 115200,
                    Parity = Parity.None,
                    DataBits = 8,
                    StopBits = StopBits.One
                },
                TcpConfig = new TcpServerParams { LocalIP = SelectedHostIP, Port = int.TryParse(Port, out int tcpPort) ? tcpPort : 1234 },
                UdpConfig = new UdpParams { LocalIP = SelectedHostIP, Port = int.TryParse(Port, out int udpPort) ? udpPort : 1234 },
                BthConfig = new BluetoothParams { IsServerMode = true },
                PresetMessages = [],
                CalibratedDistance = float.TryParse(CalibratedDistance, out float dist) ? dist : 1.0f
            };
            foreach (var pm in PresetMessages)
                settings.PresetMessages.Add(new PresetMessage { Text = pm.Message, IsHex = pm.IsHex });
            return settings;
        }

        public void OnClosing()
        {
            // 停止定时器并注销事件，避免关闭后回调访问已释放资源
            _sendTimer?.Stop();
            _sendTimer.Tick -= SendTimer_Tick;
            _memTimer?.Stop();
            _memTimer.Tick -= MemTimer_Tick;
            _rockerSendTimer?.Stop();
            _rockerSendTimer.Tick -= RockerSendTimer_Tick;
            _monitorTimer?.Stop();
            _monitorTimer.Tick -= MonitorTimer_Tick;

            _deviceService.StatusChanged -= UnifiedStatusChanged;

            _configStorage.SaveSync(CollectCurrentSettings());
            _receivePipeline.Dispose();
            _deviceService.DisposeAll();
            (_logger as IDisposable)?.Dispose();
        }
    }

    [SupportedOSPlatform("windows10.0.19041.0")]
    internal class PresetMessageViewModel : ViewModelBase
    {
        private string _message = "";
        public string Message { get => _message; set => SetField(ref _message, value); }

        private bool _isHex;
        public bool IsHex { get => _isHex; set => SetField(ref _isHex, value); }

        private readonly Action<string, bool> _sendAction;

        public ICommand SendCommand { get; }

        public PresetMessageViewModel(Action<string, bool> sendAction)
        {
            _sendAction = sendAction;
            SendCommand = new RelayCommand(_ =>
            {
                if (string.IsNullOrWhiteSpace(Message)) return;
                _sendAction(Message, IsHex);
            });
        }
    }
}
