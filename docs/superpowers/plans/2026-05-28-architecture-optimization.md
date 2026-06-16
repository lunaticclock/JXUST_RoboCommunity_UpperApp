# 架构优化实施计划：接口倒置 / 通道按需创建 / 旧体系清理

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 消除 IDisplayAdapter 接口倒置、实现通道按需创建、统一通信器实现方式并移除 BaseCommunicationManager 旧体系。

**Architecture:** 三阶段渐进重构——先解耦 Processor 与 VM 的接口依赖（A），再改造 DeviceService 为懒加载工厂模式（B），最后将蓝牙/CAN/WebSocket 从基类继承改为直接实现 ICommunicator 并删除旧文件（C）。每阶段独立可构建。

**Tech Stack:** C# / .NET 10.0 / WPF / TouchSocket 4.2

---

## Task 1: 创建 ProcessedMessage 结果类型

**Files:**
- Create: `UpperApp/Processing/ProcessedMessage.cs`

- [ ] **Step 1: 创建 ProcessedMessage record**

```csharp
// Processing/ProcessedMessage.cs
namespace UpperApp.Processing
{
    /// <summary>
    /// MessageProcessor 的纯数据处理结果，不包含任何 UI 状态
    /// </summary>
    internal record ProcessedMessage
    {
        /// <summary>来源前缀，如 "from 192.168.1.1:1234:\r\n"</summary>
        public string Prefix { get; init; } = "";

        /// <summary>格式化后的内容（已根据 Hex/Char 模式转换）</summary>
        public string FormattedContent { get; init; } = "";

        /// <summary>原始内容（未经格式化）</summary>
        public string RawContent { get; init; } = "";

        /// <summary>来源标识</summary>
        public string Source { get; init; } = "";

        /// <summary>字节数</summary>
        public int ByteCount { get; init; }

        /// <summary>新对端提示（非空时表示来源切换）</summary>
        public string NewPeerHint { get; init; } = "";

        /// <summary>是否包含姿态数据（/OVER 协议）</summary>
        public bool HasAttitudeData { get; init; }

        /// <summary>姿态原始字符串（仅当 HasAttitudeData 为 true 时有值）</summary>
        public string AttitudeRaw { get; init; } = "";
    }
}
```

- [ ] **Step 2: 构建验证**

Run: `& "C:\Program Files\dotnet\dotnet.exe" build -c Debug` in `d:\Workspace\CSharp\JXUST_RoboCommunity_UpperApp\UpperApp`

Expected: 0 errors

---

## Task 2: 重构 MessageProcessor — 返回 ProcessedMessage 而非操作 IDisplayAdapter

**Files:**
- Modify: `UpperApp/Processing/MessageProcessor.cs`

- [ ] **Step 1: 重写 MessageProcessor**

将 `MessageProcessor` 从依赖 `IDisplayAdapter` 改为纯函数式处理，返回 `ProcessedMessage`。

```csharp
// Processing/MessageProcessor.cs
using System.Text;
using UpperApp.Core;
using UpperApp.Services;

namespace UpperApp.Processing
{
    internal class MessageProcessor
    {
        private readonly ILogger _logger;

        public MessageProcessor(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 处理接收到的消息，返回纯数据结果（不含 UI 操作）
        /// </summary>
        public ProcessedMessage ProcessReceivedMessage(Result result)
        {
            if (result.NetStatus != Result.NETStatus.ReciveMessage)
                return null;

            string prefix = "";
            string rawContent = result.Message;
            string newPeerHint = result.NewPeer ?? "";

            // 构建来源前缀
            if (!string.IsNullOrEmpty(result.IPPort))
            {
                prefix = $"from {result.IPPort}:\r\n";
            }

            // 检测姿态数据
            bool hasAttitude = !string.IsNullOrEmpty(rawContent) && rawContent.Contains("/OVER");
            string attitudeRaw = hasAttitude ? rawContent : "";

            // 添加换行（与旧行为一致）
            string formatted = rawContent;
            if (!rawContent.EndsWith("\r\n") && !rawContent.EndsWith("\n"))
                formatted += "\r\n";

            // 日志记录
            if (!string.IsNullOrEmpty(rawContent))
            {
                _logger.WriteLine($"[{Utils.GetTime()}] RECV [{result.Channel}]: {rawContent.TrimEnd()}");
            }

            return new ProcessedMessage
            {
                Prefix = prefix,
                FormattedContent = formatted,
                RawContent = rawContent,
                Source = result.RemoteIP ?? result.IPPort ?? "",
                ByteCount = result.Num,
                NewPeerHint = newPeerHint,
                HasAttitudeData = hasAttitude,
                AttitudeRaw = attitudeRaw
            };
        }

        /// <summary>
        /// 处理发送的消息，返回纯数据结果（不含 UI 操作）
        /// </summary>
        public ProcessedMessage ProcessSentMessage(Result result)
        {
            if (result.NetStatus != Result.NETStatus.SendMessage)
                return null;

            string prefix = "";
            if (!string.IsNullOrEmpty(result.RemoteIP))
                prefix = $"to {result.RemoteIP}:\r\n";

            string rawContent = result.Message ?? "";
            string formatted = rawContent;
            if (!rawContent.EndsWith("\r\n") && !rawContent.EndsWith("\n"))
                formatted += "\r\n";

            _logger.WriteLine($"[{Utils.GetTime()}] SEND [{result.Channel}]: {rawContent.TrimEnd()}");

            return new ProcessedMessage
            {
                Prefix = prefix,
                FormattedContent = formatted,
                RawContent = rawContent,
                Source = result.RemoteIP ?? "",
                ByteCount = result.Num,
                NewPeerHint = "",
                HasAttitudeData = false,
                AttitudeRaw = ""
            };
        }
    }
}
```

- [ ] **Step 2: 构建验证**

Run: `& "C:\Program Files\dotnet\dotnet.exe" build -c Debug`

Expected: 编译错误——`MainViewModel` 仍引用旧 `MessageProcessor` 构造函数和 `IDisplayAdapter`。这是预期的，将在 Task 3 修复。

---

## Task 3: 重构 MainViewModel — 移除 IDisplayAdapter，消费 ProcessedMessage

**Files:**
- Modify: `UpperApp/ViewModels/MainViewModel.cs`

- [ ] **Step 1: 修改 MainViewModel 类声明和构造函数**

将 `MainViewModel : ViewModelBase, IDisplayAdapter` 改为 `MainViewModel : ViewModelBase`。

修改构造函数中 `_msgProcessor` 初始化：
```csharp
// 旧：
_msgProcessor = new MessageProcessor(this, _logger);
// 新：
_msgProcessor = new MessageProcessor(_logger);
```

- [ ] **Step 2: 移除 IDisplayAdapter 区域**

删除 `#region IDisplayAdapter` 整个区域（约 line 344-377），包括：
- `void IDisplayAdapter.UpdateByteCount(...)`
- `bool IDisplayAdapter.IsCharMode`
- `bool IDisplayAdapter.IsHexMode`
- `bool IDisplayAdapter.IsLocalEchoEnabled`
- `bool IDisplayAdapter.IsAngleDisplayEnabled`
- `bool IDisplayAdapter.IsSaveDataEnabled`
- `void IDisplayAdapter.AppendToReceiveBox(...)`
- `void IDisplayAdapter.UpdateAngleDisplay(...)`
- `void IDisplayAdapter.OnNewPeer(...)`

- [ ] **Step 3: 重写 UnifiedStatusChanged 中的消息处理**

将 `DispatchReceivedData` 回调改为处理 `ProcessedMessage`：

```csharp
private void DispatchReceivedData(Result result)
{
    var processed = _msgProcessor.ProcessReceivedMessage(result);
    if (processed == null) return;

    Application.Current.Dispatcher.BeginInvoke(() =>
    {
        // 新对端提示
        if (!string.IsNullOrEmpty(processed.NewPeerHint))
            AppendRecvText(processed.NewPeerHint);

        // 来源前缀
        if (!string.IsNullOrEmpty(processed.Prefix))
            AppendRecvText(processed.Prefix);

        // 根据 Hex/Char 模式格式化内容
        string displayContent = IsHexMode
            ? Utils.StringToHexString(processed.RawContent)
            : processed.FormattedContent;
        AppendRecvText(displayContent);

        // 更新字节计数
        _rxCount += processed.ByteCount;
        RxCount = _rxCount.ToString();

        // 本地回显
        if (LocalEcho)
            AppendRecvText(displayContent);

        // 保存数据
        if (SaveDataEnabled && !string.IsNullOrEmpty(processed.RawContent))
            _logger.WriteLine($"[{Utils.GetTime()}] DATA: {processed.RawContent.TrimEnd()}");

        // 姿态显示
        if (AngleDisplayEnabled && processed.HasAttitudeData)
            SetAngDisp(processed.AttitudeRaw);
    });
}
```

- [ ] **Step 4: 重写 SendMessage 处理**

在 `UnifiedStatusChanged` 的 `SendMessage` 分支中：

```csharp
case Result.NETStatus.SendMessage:
    var sent = _msgProcessor.ProcessSentMessage(status);
    if (sent != null)
    {
        _txCount += sent.ByteCount;
        TxCount = _txCount.ToString();
        if (LocalEcho)
        {
            string displayContent = IsHexMode
                ? Utils.StringToHexString(sent.RawContent)
                : sent.FormattedContent;
            AppendRecvText(displayContent);
        }
    }
    break;
```

- [ ] **Step 5: 移除 using UpperApp.Processing 中对 IDisplayAdapter 的隐式依赖**

确保 `using UpperApp.Processing;` 保留（因为 `MessageProcessor` 和 `ProcessedMessage` 在此命名空间）。

- [ ] **Step 6: 构建验证**

Run: `& "C:\Program Files\dotnet\dotnet.exe" build -c Debug`

Expected: 0 errors

---

## Task 4: 删除 IDisplayAdapter 接口文件

**Files:**
- Delete: `UpperApp/Processing/IDisplayAdapter.cs`

- [ ] **Step 1: 确认无引用**

搜索 `IDisplayAdapter` 关键字，确认仅存在于 `IDisplayAdapter.cs` 自身。如果还有引用，回到 Task 3 修复。

- [ ] **Step 2: 删除文件**

删除 `UpperApp/Processing/IDisplayAdapter.cs`

- [ ] **Step 3: 构建验证**

Run: `& "C:\Program Files\dotnet\dotnet.exe" build -c Debug`

Expected: 0 errors

---

## Task 5: DeviceService 改为懒加载工厂模式

**Files:**
- Modify: `UpperApp/Services/DeviceService.cs`
- Modify: `UpperApp/Services/AppServices.cs`

- [ ] **Step 1: 重写 DeviceService 构造函数和通道管理**

将 `DeviceService` 从接收预创建的 `Dictionary<ChannelType, ICommunicator>` 改为持有 `ICommunicatorFactory`，按需创建。

```csharp
// Services/DeviceService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UpperApp.Communication;
using UpperApp.Core;

namespace UpperApp.Services
{
    internal class DeviceService : IDisposable
    {
        private readonly ICommunicatorFactory _factory;
        private readonly IBluetoothCommunicator _bluetoothComm;
        private readonly Dictionary<ChannelType, ICommunicator> _cache = new();
        private readonly Lock _cacheLock = new();
        private ChannelType _activeChannel = ChannelType.Serial;
        private string _pendingTarget;
        private string _pendingBthTarget;

        public event Action<Result> StatusChanged;

        public DeviceService(ICommunicatorFactory factory, IBluetoothCommunicator bluetoothComm)
        {
            _factory = factory;
            _bluetoothComm = bluetoothComm;

            // 蓝牙需要预创建（因为 IBluetoothCommunicator 有额外属性）
            _cache[ChannelType.Bluetooth] = (ICommunicator)_bluetoothComm;
            _bluetoothComm.StatusChanged += OnCommunicatorStatusChanged;
        }

        public ChannelType ActiveChannel
        {
            get => _activeChannel;
            set => _activeChannel = value;
        }

        private ICommunicator GetOrCreate(ChannelType channel)
        {
            lock (_cacheLock)
            {
                if (_cache.TryGetValue(channel, out var comm))
                    return comm;
            }

            var instance = _factory.Create(channel);
            instance.StatusChanged += OnCommunicatorStatusChanged;

            lock (_cacheLock)
            {
                // 双检锁：防止并发创建
                if (_cache.TryGetValue(channel, out var existing))
                {
                    instance.StatusChanged -= OnCommunicatorStatusChanged;
                    return existing;
                }
                _cache[channel] = instance;
                return instance;
            }
        }

        public void StartChannel(ChannelType channel, CommunicationParams parameters)
        {
            var comm = GetOrCreate(channel);
            comm.Start(parameters);
        }

        public void StopChannel(ChannelType channel)
        {
            lock (_cacheLock)
            {
                if (!_cache.TryGetValue(channel, out var comm)) return;
                comm.Stop();
            }
        }

        public bool IsChannelReady(ChannelType channel)
        {
            lock (_cacheLock)
            {
                if (!_cache.TryGetValue(channel, out var comm)) return false;
                return comm.State == DeviceState.Connected;
            }
        }

        public bool IsAnyChannelReady()
        {
            lock (_cacheLock)
            {
                return _cache.Values.Any(c => c.State == DeviceState.Connected);
            }
        }

        public void SetTarget(string target) => _pendingTarget = target;
        public void SetBluetoothTarget(string target) => _pendingBthTarget = target;

        public bool TryExecuteCommand(IDeviceCommand command)
        {
            var channel = _activeChannel;
            ICommunicator comm;
            lock (_cacheLock)
            {
                if (!_cache.TryGetValue(channel, out comm)) return false;
            }
            if (comm.State != DeviceState.Connected) return false;

            string target = ResolveTarget(channel);
            command.Execute(comm, target);
            return true;
        }

        private string ResolveTarget(ChannelType channel)
        {
            return channel switch
            {
                ChannelType.TCP or ChannelType.UDP => _pendingTarget,
                ChannelType.Bluetooth => _pendingBthTarget,
                _ => null
            };
        }

        public IReadOnlyList<string> GetPeerList(ChannelType channel)
        {
            lock (_cacheLock)
            {
                if (!_cache.TryGetValue(channel, out var comm)) return [];
                return comm.GetPeerList();
            }
        }

        // 蓝牙专用方法
        public bool IsBluetoothReady => _bluetoothComm.State == DeviceState.Connected;
        public bool IsBluetoothRadioAvailable => _bluetoothComm.IsRadioAvailable;
        public bool IsBluetoothRadioPoweredOn => _bluetoothComm.IsRadioPoweredOn;
        public string BluetoothRadioAddress => _bluetoothComm.RadioAddress;
        public string BluetoothRadioMode => _bluetoothComm.RadioMode;

        public void StartBluetooth(BluetoothParams param) => _bluetoothComm.Start(param);
        public void StopBluetooth() => _bluetoothComm.Stop();
        public void SendBluetooth(string data, string target = null) => _bluetoothComm.Send(data, target);
        public void ConnectBluetoothDevice(string name) => _bluetoothComm.ConnectToDevice(name);
        public void DisconnectBluetoothClient() => _bluetoothComm.DisconnectClient();
        public async System.Threading.Tasks.Task<List<InTheHand.Net.BluetoothDeviceInfo>> DiscoverBluetoothDevicesAsync()
            => await _bluetoothComm.DiscoverDevicesAsync();

        private void OnCommunicatorStatusChanged(Result result)
        {
            StatusChanged?.Invoke(result);
        }

        public void DisposeAll()
        {
            lock (_cacheLock)
            {
                foreach (var comm in _cache.Values)
                {
                    try { comm.Stop(); } catch { }
                    try { comm.DisposeAsync().GetAwaiter().GetResult(); } catch { }
                }
                _cache.Clear();
            }
        }

        public void Dispose()
        {
            DisposeAll();
            GC.SuppressFinalize(this);
        }
    }
}
```

- [ ] **Step 2: 简化 AppServices.ConfigureServices()**

```csharp
// Services/AppServices.cs
public static void ConfigureServices()
{
    var factory = new CommunicatorFactory();
    RegisterSingleton<ICommunicatorFactory>(factory);
    RegisterSingleton<IConfigStorage>(new JsonFileConfigStorage());

    // 只预创建蓝牙（因为 IBluetoothCommunicator 有额外属性需要直接访问）
    var bluetoothComm = (IBluetoothCommunicator)factory.Create(ChannelType.Bluetooth);
    var deviceService = new DeviceService(factory, bluetoothComm);
    RegisterSingleton(deviceService);
}
```

- [ ] **Step 3: 构建验证**

Run: `& "C:\Program Files\dotnet\dotnet.exe" build -c Debug`

Expected: 0 errors

---

## Task 6: 重构 BthManager — 移除 BaseCommunicationManager 继承

**Files:**
- Modify: `UpperApp/Communication/BthManager.cs`

- [ ] **Step 1: 重写 BthManager 直接实现 ICommunicator**

将 `BthManager : BaseCommunicationManager, IBluetoothCommunicator` 改为 `BthManager : ICommunicator, IBluetoothCommunicator`。

将 `BaseCommunicationManager` 中的必要逻辑内联到 `BthManager`：
- `_cts` CancellationTokenSource 管理
- `_isMonitoring` / `_isStopping` 状态管理
- `State` 属性管理
- `OnStatusChanged` 辅助方法（含 Channel 标注和 Error 状态设置）
- `encoding` 字段

完整代码见下方。关键变化：
1. 不再继承 `BaseCommunicationManager`
2. 不再调用 `StartCore()` / `OnStopping()`
3. `Start()` 方法自行管理状态和 CTS
4. `Stop()` 方法自行管理清理逻辑

```csharp
// Communication/BthManager.cs
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UpperApp.Core;
using UpperApp.Services;

namespace UpperApp.Communication
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    internal class BthManager : ICommunicator, IBluetoothCommunicator
    {
        private CancellationTokenSource _cts;
        private bool _isMonitoring;
        private bool _isStopping;
        private DeviceState _state = DeviceState.Disconnected;
        private readonly Encoding _encoding = Encoding.GetEncoding("GB2312");

        public BluetoothRadio Br { get; private set; }
        private BluetoothListener _listener;
        private BluetoothClient _manualClient;
        private readonly Dictionary<string, BluetoothDeviceInfo> BthDevices = [];
        private readonly BindingDic<BluetoothClient> BthClients = new();

        public bool IsRadioAvailable => Br != null;
        public bool IsRadioPoweredOn => Br != null && Br.Mode != InTheHand.Net.Bluetooth.RadioMode.PowerOff;
        public string RadioAddress => Br?.LocalAddress.ToString() ?? "";
        public string RadioMode => Br?.Mode.ToString() ?? "";

        public event Action<Result> StatusChanged;
        public ChannelType Channel => ChannelType.Bluetooth;

        public DeviceState State
        {
            get => _state;
            private set
            {
                if (_state != value)
                    _state = value;
            }
        }

        static BthManager()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public BthManager()
        {
            Br = BluetoothRadio.Default;
        }

        public void Start(CommunicationParams parameters)
        {
            if (parameters is not BluetoothParams btParams)
                throw new ArgumentException("参数类型必须为 BluetoothParams");

            Stop();
            State = DeviceState.Connecting;
            _cts = new CancellationTokenSource();
            _isStopping = false;

            if (btParams.IsServerMode)
            {
                _listener = new BluetoothListener(BluetoothService.SerialPort);
                _listener.Start();
                _ = Task.Run(() => AcceptLoopAsync(_cts.Token));
            }
            else
            {
                if (string.IsNullOrEmpty(btParams.TargetDeviceName))
                {
                    OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, "未指定要连接的蓝牙设备名称"));
                    Stop();
                    return;
                }
                SetMaster(btParams.TargetDeviceName);
            }

            State = DeviceState.Connected;
            OnStatusChanged(new Result(Result.NETStatus.MonitorStart, "蓝牙监听开始"));
            _isMonitoring = true;
        }

        public void Stop()
        {
            if (!_isMonitoring && _state == DeviceState.Disconnected) return;

            _isStopping = true;
            State = DeviceState.Disconnecting;
            _isMonitoring = false;
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

            try { _listener?.Stop(); } catch { }
            _listener = null;
            foreach (string key in BthClients.connectionKeys.ToArray())
            {
                try { BthClients.Remove(key)?.Close(); } catch { }
            }
            try { _manualClient?.Dispose(); } catch { }
            _manualClient = null;

            OnStatusChanged(new Result(Result.NETStatus.MonitorStop, "蓝牙已停止"));
            State = DeviceState.Disconnected;
            _isStopping = false;
        }

        private async Task AcceptLoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    BluetoothClient client = await Task.Run(() => _listener!.AcceptBluetoothClient(), token);
                    string deviceName = client.RemoteMachineName ?? "unknown";
                    BthClients.Add(deviceName, client);
                    OnStatusChanged(new Result(Result.NETStatus.NewRemote, "Got a request!\r\n"));

                    try
                    {
                        byte[] welcome = Encoding.UTF8.GetBytes("Hello from service!\r\n");
                        await client.GetStream().WriteAsync(welcome, token);
                        await client.GetStream().FlushAsync(token);
                    }
                    catch { }

                    _ = ReceiveLoopAsync(client, token);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, $"监听异常: {ex.Message}"));
            }
            finally
            {
                _isMonitoring = false;
            }
        }

        private async Task ReceiveLoopAsync(BluetoothClient client, CancellationToken token)
        {
            byte[] buffer = new byte[2000];
            var stream = client.GetStream();
            string deviceName = client.RemoteMachineName ?? "unknown";

            try
            {
                while (!token.IsCancellationRequested)
                {
                    int bytesRead = await stream.ReadAsync(buffer, token);
                    if (bytesRead == 0) break;

                    string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    OnStatusChanged(new Result(Result.NETStatus.ReciveMessage, data, bytesRead));
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) when (ex is IOException or SocketException)
            {
                System.Diagnostics.Debug.WriteLine($"蓝牙接收异常 {deviceName}: {ex.Message}");
            }
            finally
            {
                BthClients.Remove(deviceName);
                client.Close();
                OnStatusChanged(new Result(Result.NETStatus.RemoteStop, deviceName));
            }
        }

        public void Send(string data, string target = null)
        {
            BluetoothClient client = target == null ? _manualClient : GetSlaveClient(target);
            if (client == null || !client.Connected)
            {
                OnStatusChanged(new Result(Result.NETStatus.RemoteStop, "连接断开", 0));
                return;
            }

            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(data);
                client.GetStream().Write(buffer, 0, buffer.Length);
                client.GetStream().Flush();
                OnStatusChanged(new Result(Result.NETStatus.SendMessage, data, buffer.Length));
            }
            catch (Exception ex)
            {
                OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, ex.Message));
            }
        }

        public void SetMaster(string bluetoothDeviceName)
        {
            _manualClient?.Close();
            _manualClient = new BluetoothClient();
            if (!BthDevices.TryGetValue(bluetoothDeviceName, out var device))
            {
                OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, $"未找到蓝牙设备: {bluetoothDeviceName}"));
                return;
            }
            _manualClient.Connect(device.DeviceAddress, BluetoothService.SerialPort);
            _ = Task.Run(() => ReceiveLoopAsync(_manualClient, _cts.Token));
        }

        public BluetoothClient GetSlaveClient(string name)
        {
            BthClients.TryGet(name, out BluetoothClient client);
            return client;
        }

        public void DisconnectClient()
        {
            _manualClient?.Close();
            _manualClient = null;
        }

        public void ConnectToDevice(string deviceName)
        {
            var param = new BluetoothParams { IsServerMode = false, TargetDeviceName = deviceName };
            if (_isMonitoring) Stop();
            Start(param);
        }

        public async Task<List<BluetoothDeviceInfo>> DiscoverDevicesAsync()
        {
            return await Task.Run(() =>
            {
                using var cli = new BluetoothClient();
                List<BluetoothDeviceInfo> devices = [.. cli.DiscoverDevices()];
                foreach (var device in devices)
                {
                    BthDevices.TryAdd(device.DeviceName, device);
                }
                return devices;
            });
        }

        public IReadOnlyList<string> GetPeerList() => BthClients.connectionKeys;

        public ValueTask DisposeAsync()
        {
            Stop();
            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }

        private void OnStatusChanged(Result result)
        {
            if (result.Channel == ChannelType.Unknown)
                result = result with { Channel = ChannelType.Bluetooth };
            if (result.NetStatus == Result.NETStatus.ExceptionStop)
                State = DeviceState.Error;
            StatusChanged?.Invoke(result);
        }
    }
}
```

- [ ] **Step 2: 构建验证**

Run: `& "C:\Program Files\dotnet\dotnet.exe" build -c Debug`

Expected: 0 errors

---

## Task 7: 重构 CANManager — 移除 BaseCommunicationManager 继承

**Files:**
- Modify: `UpperApp/Communication/CanManager.cs`

- [ ] **Step 1: 重写 CANManager 直接实现 ICommunicator**

与 Task 6 同理，将 `CANManager : BaseCommunicationManager` 改为 `CANManager : ICommunicator`。

关键变化：
1. 不再继承 `BaseCommunicationManager`
2. 内联 `_cts`、`_isMonitoring`、`_isStopping`、`State`、`OnStatusChanged` 逻辑
3. `Start()` 不再调用 `StartCore()`，自行管理状态
4. `Stop()` 不再调用 `OnStopping()`，内联清理逻辑

```csharp
// Communication/CanManager.cs
using Peak.Can.Basic;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UpperApp.Core;
using UpperApp.Services;

namespace UpperApp.Communication
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    internal class CANManager : ICommunicator
    {
        private PcanChannel _pcanChannel;
        private readonly BindingDic<string> _canDevices = new();
        private CancellationTokenSource _cts;
        private bool _isMonitoring;
        private bool _isStopping;
        private DeviceState _state = DeviceState.Disconnected;
        private readonly Encoding _encoding = Encoding.GetEncoding("GB2312");

        public event Action<Result> StatusChanged;
        public ChannelType Channel => ChannelType.CAN;

        public DeviceState State
        {
            get => _state;
            private set
            {
                if (_state != value)
                    _state = value;
            }
        }

        static CANManager()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void Start(CommunicationParams parameters)
        {
            if (parameters is not CanParams canParams)
                throw new ArgumentException("参数类型必须为 CanParams");

            Stop();
            State = DeviceState.Connecting;
            _cts = new CancellationTokenSource();
            _isStopping = false;

            if (!Enum.TryParse(canParams.ChannelName, true, out _pcanChannel))
            {
                OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, $"无效的 CAN 通道: {canParams.ChannelName}"));
                Stop();
                return;
            }

            var status = Api.Initialize(_pcanChannel, Bitrate.Pcan500);
            if (status != PcanStatus.OK)
            {
                OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, $"CAN 初始化失败: {GetErrorMessage(status)}"));
                Stop();
                return;
            }

            _ = ReceiveLoopAsync(_cts.Token);
            State = DeviceState.Connected;
            OnStatusChanged(new Result(Result.NETStatus.MonitorStart, $"CAN 通道 {canParams.ChannelName} 启动"));
            _isMonitoring = true;
        }

        public void Stop()
        {
            if (!_isMonitoring && _state == DeviceState.Disconnected) return;

            _isStopping = true;
            State = DeviceState.Disconnecting;
            _isMonitoring = false;
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

            if (_pcanChannel != 0)
            {
                try { Api.Uninitialize(_pcanChannel); } catch { }
            }

            OnStatusChanged(new Result(Result.NETStatus.MonitorStop, "CAN 已停止"));
            State = DeviceState.Disconnected;
            _isStopping = false;
        }

        private async Task ReceiveLoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var readTask = Task.Run(() =>
                    {
                        PcanStatus status = Api.Read(_pcanChannel, out PcanMessage msg);
                        return (status, msg);
                    });

                    var completedTask = await Task.WhenAny(readTask, Task.Delay(100, token));
                    if (completedTask == readTask)
                    {
                        (PcanStatus status, PcanMessage msg) = await readTask;
                        if (status == PcanStatus.OK)
                        {
                            string canId = msg.ID.ToString("X");
                            string data = Convert.ToHexString(msg.Data);
                            string message = $"{canId}:{data}";
                            OnStatusChanged(new Result(Result.NETStatus.ReciveMessage, message, (int)msg.Length, canId));
                        }
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, $"接收循环异常: {ex.Message}"));
            }
            finally
            {
                if (_isMonitoring) Stop();
            }
        }

        public void Send(string data, string target = null)
        {
            if (string.IsNullOrWhiteSpace(data))
                return;

            string[] parts = data.Split(':');
            if (parts.Length != 2)
            {
                OnStatusChanged(new Result(Result.NETStatus.SendMessage, "CAN 发送格式错误，应为 ID:数据", 0) with { Status = Result.ResStatus.Error });
                return;
            }

            if (!uint.TryParse(parts[0], System.Globalization.NumberStyles.HexNumber, null, out uint id))
            {
                OnStatusChanged(new Result(Result.NETStatus.SendMessage, $"无效的 CAN ID: {parts[0]}", 0) with { Status = Result.ResStatus.Error });
                return;
            }

            string hexData = parts[1];
            if (hexData.Length % 2 != 0)
            {
                OnStatusChanged(new Result(Result.NETStatus.SendMessage, "CAN 数据长度必须为偶数", 0) with { Status = Result.ResStatus.Error });
                return;
            }

            byte[] dataBytes = new byte[hexData.Length / 2];
            for (int i = 0; i < dataBytes.Length; i++)
                dataBytes[i] = Convert.ToByte(hexData.Substring(i * 2, 2), 16);

            var msg = new PcanMessage
            {
                ID = id,
                MsgType = MessageType.Standard,
                Data = new DataBytes(dataBytes)
            };

            PcanStatus status = Api.Write(_pcanChannel, msg);
            if (status == PcanStatus.OK)
            {
                var result = new Result(Result.NETStatus.SendMessage, data, dataBytes.Length, id.ToString())
                    with { Status = Result.ResStatus.SetNum };
                OnStatusChanged(result);
            }
            else
            {
                OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, $"CAN 发送失败: {GetErrorMessage(status)}", 0, id.ToString()));
            }
        }

        private static string GetErrorMessage(PcanStatus status)
        {
            Api.GetErrorText(status, out string errText);
            return errText;
        }

        public IReadOnlyList<string> GetPeerList() => _canDevices.connectionKeys;

        public ValueTask DisposeAsync()
        {
            Stop();
            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }

        private void OnStatusChanged(Result result)
        {
            if (result.Channel == ChannelType.Unknown)
                result = result with { Channel = ChannelType.CAN };
            if (result.NetStatus == Result.NETStatus.ExceptionStop)
                State = DeviceState.Error;
            StatusChanged?.Invoke(result);
        }
    }
}
```

- [ ] **Step 2: 构建验证**

Run: `& "C:\Program Files\dotnet\dotnet.exe" build -c Debug`

Expected: 0 errors

---

## Task 8: 重构 WebSocketManager — 移除 BaseCommunicationManager 继承

**Files:**
- Modify: `UpperApp/Communication/WebSocketManager.cs`

- [ ] **Step 1: 重写 WebSocketManager 直接实现 ICommunicator**

与 Task 6/7 同理。关键变化同上。

```csharp
// Communication/WebSocketManager.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UpperApp.Core;
using UpperApp.Services;

namespace UpperApp.Communication
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    internal class WebSocketManager : ICommunicator
    {
        private HttpListener _listener;
        private readonly BindingDic<WebSocket> _serverClients = new();
        private ClientWebSocket _clientSocket;
        private bool _isClientMode;
        private string _clientTarget;
        private CancellationTokenSource _cts;
        private bool _isMonitoring;
        private bool _isStopping;
        private DeviceState _state = DeviceState.Disconnected;
        private readonly Encoding _encoding = Encoding.GetEncoding("GB2312");

        public event Action<Result> StatusChanged;
        public ChannelType Channel => ChannelType.WebSocket;

        public DeviceState State
        {
            get => _state;
            private set
            {
                if (_state != value)
                    _state = value;
            }
        }

        static WebSocketManager()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void Start(CommunicationParams parameters)
        {
            if (parameters is not WebSocketParams wsParams)
                throw new ArgumentException("参数类型必须为 WebSocketParams");

            Stop();
            State = DeviceState.Connecting;
            _cts = new CancellationTokenSource();
            _isStopping = false;

            if (wsParams.IsServerMode)
            {
                StartServer(wsParams.Url);
            }
            else
            {
                _ = ConnectAsync(wsParams.Url);
            }
        }

        private void StartServer(string url)
        {
            if (_listener == null)
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add(url);
            }
            _listener.Start();
            _ = AcceptLoopAsync(_cts.Token);
            State = DeviceState.Connected;
            OnStatusChanged(new Result(Result.NETStatus.MonitorStart, $"WebSocket 监听 {url}"));
            _isMonitoring = true;
        }

        private async Task ConnectAsync(string serverUrl)
        {
            _isClientMode = true;
            _clientSocket = new ClientWebSocket();
            try
            {
                await _clientSocket.ConnectAsync(new Uri(serverUrl), _cts.Token);
                _clientTarget = serverUrl;
                _ = ReceiveLoopAsync(_clientSocket, "Server", _cts.Token);
                State = DeviceState.Connected;
                OnStatusChanged(new Result(Result.NETStatus.MonitorStart, $"WebSocket 客户端已连接 {serverUrl}"));
                _isMonitoring = true;
            }
            catch (Exception ex)
            {
                OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, $"WebSocket 连接失败: {ex.Message}"));
                Stop();
            }
        }

        private async Task AcceptLoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var context = await _listener!.GetContextAsync();
                    if (context.Request.IsWebSocketRequest)
                    {
                        var wsContext = await context.AcceptWebSocketAsync(null);
                        var socket = wsContext.WebSocket;
                        string clientId = context.Request.RemoteEndPoint.ToString();
                        _serverClients.Add(clientId, socket);
                        OnStatusChanged(new Result(Result.NETStatus.NewRemote, $"WebSocket 客户端连接: {clientId}"));
                        _ = ReceiveLoopAsync(socket, clientId, token);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                        context.Response.Close();
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, $"监听异常: {ex.Message}"));
            }
            finally
            {
                _isMonitoring = false;
            }
        }

        private async Task ReceiveLoopAsync(WebSocket socket, string clientId, CancellationToken token)
        {
            var buffer = new byte[4096];
            try
            {
                while (socket.State == WebSocketState.Open && !token.IsCancellationRequested)
                {
                    var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), token);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", token);
                        break;
                    }
                    string receivedData = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    OnStatusChanged(new Result(Result.NETStatus.ReciveMessage, receivedData, result.Count, clientId));
                }
            }
            catch (OperationCanceledException) { }
            catch (WebSocketException ex)
            {
                OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, $"{clientId} 通信异常: {ex.Message}", 0, clientId));
            }
            finally
            {
                if (!_isClientMode)
                    _serverClients.Remove(clientId);
                socket.Dispose();
                OnStatusChanged(new Result(Result.NETStatus.RemoteStop, clientId));
            }
        }

        public void Stop()
        {
            if (!_isMonitoring && _state == DeviceState.Disconnected) return;

            _isStopping = true;
            State = DeviceState.Disconnecting;
            _isMonitoring = false;
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

            try { _listener?.Stop(); } catch { }
            _listener = null;

            foreach (string key in _serverClients.connectionKeys.ToArray())
            {
                try { _serverClients.Remove(key)?.Dispose(); } catch { }
            }

            if (_clientSocket != null && (_clientSocket.State == WebSocketState.Open || _clientSocket.State == WebSocketState.Connecting))
            {
                try { _clientSocket.Abort(); } catch { }
            }
            try { _clientSocket?.Dispose(); } catch { }
            _clientSocket = null;

            OnStatusChanged(new Result(Result.NETStatus.MonitorStop, "WebSocket 已停止"));
            State = DeviceState.Disconnected;
            _isStopping = false;
        }

        public void Send(string data, string target = null)
        {
            if (_isClientMode)
            {
                if (_clientSocket == null || _clientSocket.State != WebSocketState.Open)
                {
                    OnStatusChanged(new Result(Result.NETStatus.SendMessage, "WebSocket 客户端未连接", 0) with { Status = Result.ResStatus.Error });
                    return;
                }
                try
                {
                    var buffer = Encoding.UTF8.GetBytes(data);
                    _clientSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, _cts.Token).Wait();
                    var result = new Result(Result.NETStatus.SendMessage, data, buffer.Length, _clientTarget) with { Status = Result.ResStatus.SetNum };
                    OnStatusChanged(result);
                }
                catch (Exception ex)
                {
                    OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, $"发送失败: {ex.Message}", 0, _clientTarget));
                }
            }
            else
            {
                if (string.IsNullOrEmpty(target))
                {
                    OnStatusChanged(new Result(Result.NETStatus.SendMessage, "WebSocket 服务器模式需要指定客户端标识", 0) with { Status = Result.ResStatus.Error });
                    return;
                }
                if (_serverClients.TryGet(target, out WebSocket socket))
                {
                    try
                    {
                        byte[] buffer = Encoding.UTF8.GetBytes(data);
                        socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, _cts.Token).Wait();
                        var result = new Result(Result.NETStatus.SendMessage, data, buffer.Length, target) with { Status = Result.ResStatus.SetNum };
                        OnStatusChanged(result);
                    }
                    catch (Exception ex)
                    {
                        OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, $"发送到 {target} 失败: {ex.Message}", 0, target));
                        _serverClients.Remove(target)?.Dispose();
                        OnStatusChanged(new Result(Result.NETStatus.RemoteStop, target));
                    }
                }
                else
                {
                    OnStatusChanged(new Result(Result.NETStatus.SendMessage, $"未找到客户端标识: {target}", 0) with { Status = Result.ResStatus.Error });
                }
            }
        }

        public IReadOnlyList<string> GetPeerList()
        {
            if (_isClientMode) return [];
            return _serverClients.connectionKeys;
        }

        public ValueTask DisposeAsync()
        {
            Stop();
            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }

        private void OnStatusChanged(Result result)
        {
            if (result.Channel == ChannelType.Unknown)
                result = result with { Channel = ChannelType.WebSocket };
            if (result.NetStatus == Result.NETStatus.ExceptionStop)
                State = DeviceState.Error;
            StatusChanged?.Invoke(result);
        }
    }
}
```

- [ ] **Step 2: 构建验证**

Run: `& "C:\Program Files\dotnet\dotnet.exe" build -c Debug`

Expected: 0 errors

---

## Task 9: 删除旧文件

**Files:**
- Delete: `UpperApp/Communication/BaseCommunicationManager.cs`
- Delete: `UpperApp/Communication/TCPManager.cs`
- Delete: `UpperApp/Communication/UDPManager.cs`
- Delete: `UpperApp/Communication/SerManager.cs`

- [ ] **Step 1: 确认无引用**

搜索 `BaseCommunicationManager`、`TCPManager`、`UDPManager`、`SerManager` 关键字，确认无残留引用。

- [ ] **Step 2: 删除 4 个文件**

- [ ] **Step 3: 构建验证**

Run: `& "C:\Program Files\dotnet\dotnet.exe" build -c Debug`

Expected: 0 errors

---

## Task 10: 最终验证

- [ ] **Step 1: 完整构建**

Run: `& "C:\Program Files\dotnet\dotnet.exe" build -c Debug`

Expected: 0 errors

- [ ] **Step 2: 搜索残留引用**

搜索 `IDisplayAdapter`、`BaseCommunicationManager`、`TCPManager`、`UDPManager`、`SerManager`，确认项目中无残留引用。

- [ ] **Step 3: 更新计划文档**

在 `docs/superpowers/plans/2026-05-28-architecture-optimization.md` 顶部添加完成标记。
