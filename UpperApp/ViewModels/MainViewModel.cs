using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Runtime.Versioning;
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
    internal class MainViewModel : ViewModelBase, IDisplayAdapter
    {
        private readonly ILogger _logger = new FileLogger();
        private MessageProcessor _msgProcessor;
        private readonly DeviceService _deviceService;
        private readonly DataPipeline _receivePipeline;
        private readonly IConfigStorage _configStorage;
        private MapTracker _mapTracker;
        private readonly DispatcherTimer _sendTimer;
        private readonly DispatcherTimer _memTimer;
        private int _cnt, _counter;
        private int _rxCount, _txCount;

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
        private ObservableCollection<string> _serialPorts = new();
        public ObservableCollection<string> SerialPorts { get => _serialPorts; set => SetField(ref _serialPorts, value); }

        private string _selectedSerialPort = "";
        public string SelectedSerialPort { get => _selectedSerialPort; set => SetField(ref _selectedSerialPort, value); }

        private string _selectedBaudRate = "115200";
        public string SelectedBaudRate { get => _selectedBaudRate; set => SetField(ref _selectedBaudRate, value); }

        public ObservableCollection<string> BaudRates { get; } = new() { "9600", "19200", "38400", "115200", "256000", "460800", "512000", "921600" };

        private string _serialButtonText = "打开串口";
        public string SerialButtonText { get => _serialButtonText; set => SetField(ref _serialButtonText, value); }

        private bool _isSerialOpen;
        public bool IsSerialOpen { get => _isSerialOpen; set { if (SetField(ref _isSerialOpen, value)) OnPropertyChanged(nameof(IsSerialConfigEnabled)); } }
        public bool IsSerialConfigEnabled => !_isSerialOpen;

        // Network
        private ObservableCollection<string> _localIPs = new();
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

        public ObservableCollection<string> NetTypes { get; } = new() { "TCP", "UDP" };

        private string _listenButtonText = "开始监听";
        public string ListenButtonText { get => _listenButtonText; set => SetField(ref _listenButtonText, value); }

        private bool _isNetListening;
        public bool IsNetListening { get => _isNetListening; set { if (SetField(ref _isNetListening, value)) OnPropertyChanged(nameof(IsNetConfigEnabled)); } }
        public bool IsNetConfigEnabled => !_isNetListening;

        private ObservableCollection<string> _peerList = new();
        public ObservableCollection<string> PeerList { get => _peerList; set => SetField(ref _peerList, value); }

        private string _selectedPeer = "";
        public string SelectedPeer { get => _selectedPeer; set => SetField(ref _selectedPeer, value); }

        // Bluetooth
        private string _bthListenButtonText = "监听";
        public string BthListenButtonText { get => _bthListenButtonText; set => SetField(ref _bthListenButtonText, value); }

        private string _bthRecvText = "";
        public string BthRecvText { get => _bthRecvText; set => SetField(ref _bthRecvText, value); }

        private string _bthSendText = "";
        public string BthSendText { get => _bthSendText; set => SetField(ref _bthSendText, value); }

        private string _bthScanButtonText = "扫描蓝牙";
        public string BthScanButtonText { get => _bthScanButtonText; set => SetField(ref _bthScanButtonText, value); }

        private ObservableCollection<string> _bthDeviceList = new();
        public ObservableCollection<string> BthDeviceList { get => _bthDeviceList; set => SetField(ref _bthDeviceList, value); }

        private string _selectedBthDevice = "";
        public string SelectedBthDevice { get => _selectedBthDevice; set => SetField(ref _selectedBthDevice, value); }

        private ObservableCollection<string> _bthSlaveList = new();
        public ObservableCollection<string> BthSlaveList { get => _bthSlaveList; set => SetField(ref _bthSlaveList, value); }

        private string _selectedBthSlave = "";
        public string SelectedBthSlave { get => _selectedBthSlave; set => SetField(ref _selectedBthSlave, value); }

        private string _bthConnectButtonText = "连接";
        public string BthConnectButtonText { get => _bthConnectButtonText; set => SetField(ref _bthConnectButtonText, value); }

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

        // Batch messages
        public ObservableCollection<PresetMessageViewModel> PresetMessages { get; } = new();

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
        public ICommand BthSendCommand { get; }
        public ICommand BthScanCommand { get; }
        public ICommand BthConnectCommand { get; }
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

            _msgProcessor = new MessageProcessor(this, _logger);
            _receivePipeline = new DataPipeline(DispatchReceivedData);
            _receivePipeline.Start();

            _sendTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            _sendTimer.Tick += SendTimer_Tick;

            _memTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            _memTimer.Tick += MemTimer_Tick;
            _memTimer.Start();

            ToggleSerialCommand = new RelayCommand(ToggleSerial);
            ToggleListenCommand = new RelayCommand(ToggleListen);
            ToggleBluetoothCommand = new RelayCommand(ToggleBluetooth);
            BthSendCommand = new RelayCommand(BthSend);
            BthScanCommand = new RelayCommand(async _ => await BthScan());
            BthConnectCommand = new RelayCommand(BthConnect);
            SendCommand = new RelayCommand(_ => StrSend(SendText));
            ToggleAutoSendCommand = new RelayCommand(ToggleAutoSend);
            ClearRecvCommand = new RelayCommand(_ => { _rxCount = 0; RxCount = "0"; RecvText = ""; StatusText = "接收区已清空"; });
            ClearSendCommand = new RelayCommand(_ => { _txCount = 0; TxCount = "0"; SendText = ""; StatusText = "发送区已清空"; });
            StopCommand = new RelayCommand(_ => { FbValue = 50; _deviceService.TryExecuteCommand(new MoveCommand(MoveCommand.MoveType.ForwardBackward, 50)); });
            ResetDirectionCommand = new RelayCommand(_ => { RlValue = 50; _deviceService.TryExecuteCommand(new MoveCommand(MoveCommand.MoveType.RightLeft, 50, 50)); });
            ToggleRockerCommand = new RelayCommand(_ => { _rockerActive = !_rockerActive; RockerText = _rockerActive ? "摇杆关" : "摇杆开"; if (!_rockerActive) { FbValue = 50; RlValue = 50; _deviceService.TryExecuteCommand(new MoveCommand(MoveCommand.MoveType.FullControl, FbValue, RlValue)); } });
            ClearAttitudeCommand = new RelayCommand(_ => { Yaw = Roll = Pitch = Distance = "0"; StatusText = "数据清除成功"; });
            OpenMapCommand = new RelayCommand(OpenMap);
            ClearMapCommand = new RelayCommand(_ => { _mapTracker?.Clear(); MapStartPoint = MapEndPoint = "0,0"; });

            for (int i = 0; i < 8; i++)
            {
                var vm = new PresetMessageViewModel(idx => StrSend(PresetMessages[idx].Message));
                vm.SetIndex(i);
                PresetMessages.Add(vm);
            }

            RefreshSerialPorts();
            RefreshLocalIPs();

            _ = LoadSettingsAsync();
        }

        public void SetMapTracker(MapTracker tracker)
        {
            _mapTracker = tracker;
        }

        public void AppendRecvText(string text)
        {
            const int MaxLength = 500000;
            var newValue = _recvText + text;
            if (newValue.Length > MaxLength)
                newValue = newValue[^MaxLength..];
            RecvText = newValue;
        }

        #region IDisplayAdapter

        void IDisplayAdapter.UpdateByteCount(int count, RecvOrSend direction)
        {
            if (direction == RecvOrSend.Recv)
            {
                _rxCount += count;
                RxCount = _rxCount.ToString();
            }
            else
            {
                _txCount += count;
                TxCount = _txCount.ToString();
            }
        }

        bool IDisplayAdapter.IsCharMode => IsCharMode;
        bool IDisplayAdapter.IsHexMode => IsHexMode;
        bool IDisplayAdapter.IsLocalEchoEnabled => LocalEcho;
        bool IDisplayAdapter.IsAngleDisplayEnabled => AngleDisplayEnabled;
        bool IDisplayAdapter.IsSaveDataEnabled => SaveDataEnabled;
        void IDisplayAdapter.AppendToReceiveBox(string text) => RecvText += text;
        void IDisplayAdapter.UpdateAngleDisplay(string message) => SetAngDisp(message);
        void IDisplayAdapter.OnNewPeer(string peerInfo)
        {
            var peers = _deviceService.GetPeerList(_deviceService.ActiveChannel);
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                PeerList.Clear();
                foreach (var p in peers) PeerList.Add(p);
            });
        }

        #endregion

        private void UnifiedStatusChanged(Result status)
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                switch (status.NetStatus)
                {
                    case Result.NETStatus.ReciveMessage:
                        _receivePipeline.TryEnqueue(status);
                        break;
                    case Result.NETStatus.SendMessage:
                        _msgProcessor.ProcessSentMessage(status);
                        break;
                    case Result.NETStatus.ExceptionStop:
                        _logger.WriteLine($"[{Utils.GetTime()}] ExceptionStop [{status.Channel}]: {status.Message}");
                        string title = status.Channel switch
                        {
                            ChannelType.Serial => "串口错误",
                            ChannelType.Bluetooth => "蓝牙错误",
                            ChannelType.TCP or ChannelType.UDP => string.IsNullOrEmpty(status.RemoteIP) ? "网络错误" : "远端关闭",
                            _ => "错误"
                        };
                        MessageBox.Show(status.Message, title);
                        break;
                    case Result.NETStatus.MonitorStart:
                    case Result.NETStatus.MonitorStop:
                        UpdateMonitorUI(status);
                        break;
                    case Result.NETStatus.NewRemote:
                        if (status.Channel == ChannelType.TCP || status.Channel == ChannelType.UDP)
                        {
                            StatusText = "连接成功！";
                            var peers = _deviceService.GetPeerList(status.Channel);
                            PeerList.Clear();
                            foreach (var p in peers) PeerList.Add(p);
                            SelectedPeer = status.Message;
                        }
                        else if (status.Channel == ChannelType.Bluetooth)
                        {
                            BthRecvText += status.Message;
                            var slaves = _deviceService.GetPeerList(ChannelType.Bluetooth);
                            BthSlaveList.Clear();
                            foreach (var s in slaves) BthSlaveList.Add(s);
                        }
                        break;
                    case Result.NETStatus.RemoteStop:
                        if (status.Channel == ChannelType.TCP || status.Channel == ChannelType.UDP)
                        {
                            var peers2 = _deviceService.GetPeerList(status.Channel);
                            PeerList.Clear();
                            foreach (var p in peers2) PeerList.Add(p);
                            SelectedPeer = "";
                            StatusText = status.Message;
                        }
                        else if (status.Channel == ChannelType.Bluetooth)
                        {
                            StatusText = "连接断开!";
                            BthConnectButtonText = "连接";
                            var slaves2 = _deviceService.GetPeerList(ChannelType.Bluetooth);
                            BthSlaveList.Clear();
                            foreach (var s in slaves2) BthSlaveList.Add(s);
                        }
                        break;
                    case Result.NETStatus.ManualStop:
                        if (status.Channel == ChannelType.TCP || status.Channel == ChannelType.UDP)
                            SelectedPeer = "";
                        break;
                }
            });
        }

        private void DispatchReceivedData(Result result)
        {
            Application.Current.Dispatcher.BeginInvoke(() => _msgProcessor.ProcessReceivedMessage(result));
        }

        private void UpdateMonitorUI(Result status)
        {
            bool isStart = status.NetStatus == Result.NETStatus.MonitorStart;
            switch (status.Channel)
            {
                case ChannelType.Serial:
                    SerialButtonText = isStart ? "关闭串口" : "打开串口";
                    IsSerialOpen = isStart;
                    if (isStart) IsSerialChannel = true;
                    StatusText = status.Message;
                    break;
                case ChannelType.TCP:
                    ListenButtonText = isStart ? "停止监听" : "开始监听";
                    IsNetListening = isStart;
                    if (isStart) { IsNetChannel = true; SelectedNetType = "TCP"; _deviceService.ActiveChannel = ChannelType.TCP; SelectedPeer = ""; }
                    StatusText = status.Message;
                    break;
                case ChannelType.UDP:
                    ListenButtonText = isStart ? "停止监听" : "开始监听";
                    IsNetListening = isStart;
                    if (isStart) { IsNetChannel = true; SelectedNetType = "UDP"; _deviceService.ActiveChannel = ChannelType.UDP; SelectedPeer = ""; }
                    StatusText = status.Message;
                    break;
                case ChannelType.Bluetooth:
                    BthListenButtonText = isStart ? "关闭" : "监听";
                    if (isStart) BthRecvText += "Service started!\r\n";
                    break;
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

        private void StrSend(string buf)
        {
            _deviceService.SetTarget(SelectedPeer);
            _deviceService.SetBluetoothTarget(SelectedBthSlave);
            var command = new RawSendCommand(buf, _deviceService.ActiveChannel);
            if (!_deviceService.TryExecuteCommand(command))
                StatusText = "发送失败：通道未连接或未找到";
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
                MessageBox.Show("检测不到本机蓝牙设备", "error");
                return;
            }
            if (!_deviceService.IsBluetoothRadioPoweredOn)
            {
                MessageBox.Show("请先在系统中打开蓝牙", "warning");
                return;
            }
            if (!_deviceService.IsBluetoothReady)
            {
                _deviceService.StartBluetooth(new BluetoothParams { IsServerMode = true });
                BthRecvText += $"Radio address: {_deviceService.BluetoothRadioAddress}\r\n";
                BthRecvText += $"Mode: {_deviceService.BluetoothRadioMode}\r\n";
                BthRecvText += "Service started!\r\n";
                BthListenButtonText = "关闭";
            }
            else
            {
                _deviceService.StopBluetooth();
                BthListenButtonText = "监听";
            }
        }

        private void BthSend(object _)
        {
            if (!string.IsNullOrEmpty(SelectedBthSlave))
                _deviceService.SendBluetooth(BthSendText, SelectedBthSlave);
            else if (_deviceService.IsBluetoothReady)
                _deviceService.SendBluetooth(BthSendText);
            else
                MessageBox.Show("请连接设备", "warning");
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
                BthRecvText += $"扫描完成，发现 {devices.Count} 个设备。\r\n";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"扫描失败: {ex.Message}");
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
                if (!_deviceService.IsBluetoothReady)
                    MessageBox.Show("请选择设备", "warning");
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

        private void OnSliderChanged()
        {
            if (_rockerActive)
                _deviceService.TryExecuteCommand(new MoveCommand(MoveCommand.MoveType.FullControl, FbValue, RlValue));
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
                catch
                {
                    MessageBox.Show("请选择图片文件！", "Warning");
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
                PresetMessages = new List<PresetMessage>(),
                CalibratedDistance = float.TryParse(CalibratedDistance, out float dist) ? dist : 1.0f
            };
            foreach (var pm in PresetMessages)
                settings.PresetMessages.Add(new PresetMessage { Text = pm.Message, IsHex = pm.IsHex });
            return settings;
        }

        public void OnClosing()
        {
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

        private int _index = -1;
        private readonly Action<int> _sendAction;

        public ICommand SendCommand { get; }

        public PresetMessageViewModel(Action<int> sendAction)
        {
            _sendAction = sendAction;
            SendCommand = new RelayCommand(_ =>
            {
                if (string.IsNullOrWhiteSpace(Message)) return;
                string sendStr = IsHex ? Utils.HexStringToString(Message) : Message;
                if (sendStr != null) _sendAction(_index);
            });
        }

        internal void SetIndex(int i) => _index = i;
    }
}
