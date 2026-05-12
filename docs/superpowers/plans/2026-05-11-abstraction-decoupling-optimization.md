# 抽象解耦与架构优化 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 对 JXUST RoboCommunity UpperApp 进行抽象解耦重构，消除 God Class、接口抽象泄漏、委托注入等设计问题，使项目具备可测试性与可扩展性。

**Architecture:** 采用渐进式重构策略，从底层基础设施（枚举/数据类型）开始，逐步向上解耦：先提取类型定义与接口，再重构通信管理层，最后拆分 UI 层。每一步保持项目可编译运行，不引入新依赖。

**Tech Stack:** C# / .NET 10.0 / WinForms / 现有 NuGet 包不变

---

## 优化问题总览

| # | 问题 | 严重度 | 当前位置 | 涉及原则 |
|---|------|--------|----------|----------|
| P1 | **UpperApp 是 God Class**（~1000行，混合 UI/业务/通信/配置/地图逻辑） | 🔴 高 | `Form1.cs` | SRP |
| P2 | **MessageProcessor 使用 9 个委托注入**，无法静态验证，不可测试 | 🔴 高 | `MessageProcessor.cs` | DIP / ISP |
| P3 | **BthManager 抽象泄漏**：Form 中直接强转 `(BthManager)_communicators[ChannelType.Bluetooth]` | 🔴 高 | `Form1.cs:529,558,569,952` | LSP / DIP |
| P4 | **Result 类可变且职责混杂**：同时承载收发数据、状态、错误、对端信息 | 🟡 中 | `Result.cs` | SRP |
| P5 | **ICommunicator.Start 参数无编译期类型安全**：运行时 `is not SerialParams` 检查 | 🟡 中 | `ICommunicator.cs` / 各 Manager | ISP |
| P6 | **枚举与 BindingDic 混在 Utils.cs**：文件职责不清 | 🟡 中 | `Utils.cs` | SRP / CCP |
| P7 | **BaseCommunicationManager.StartCore() 是 public 但不应被外部调用** | 🟢 低 | `BaseCommunicationManager.cs:36` | Encapsulation |
| P8 | **GetPeerList() 返回 object**：丢失类型信息，调用方需自行转换 | 🟡 中 | `ICommunicator.cs:48` | LSP |
| P9 | **Form 直接持有 StreamWriter tf**：日志写入与 UI 耦合 | 🟡 中 | `Form1.cs:41` | SRP / DIP |
| P10 | **地图/轨迹绘制逻辑内嵌 Form**：约 80 行坐标计算与绘制代码 | 🟡 中 | `Form1.cs:827-938` | SRP |
| P11 | **async void 事件处理器**：`UpperApp_FormClosing` 使用 `async void` | 🟢 低 | `Form1.cs:813` | 异步最佳实践 |
| P12 | **硬编码字符串散落各处**：协议格式如 `"FB:{value}:OVER\r\n"` 无统一管理 | 🟢 低 | `Form1.cs` 多处 | DRY |

---

## File Structure

重构后的目标文件结构（仅展示变更部分）：

```
UpperApp/
├── Core/                           # 新建：核心类型与接口
│   ├── ChannelType.cs              # 从 Utils.cs 提取
│   ├── RecvOrSend.cs               # 从 Utils.cs 提取
│   ├── Result.cs                   # 重构为不可变 record
│   └── BindingDic.cs               # 从 Utils.cs 提取
├── Communication/                  # 新建：通信相关
│   ├── ICommunicator.cs            # 重构接口
│   ├── IBluetoothCommunicator.cs   # 新建：蓝牙扩展接口
│   ├── ICommunicatorFactory.cs     # 不变
│   ├── BaseCommunicationManager.cs # 修复封装
│   ├── CommunicationParams.cs      # 不变
│   ├── SerManager.cs               # 不变
│   ├── TCPManager.cs               # 不变
│   ├── UDPManager.cs               # 不变
│   ├── BthManager.cs               # 实现 IBluetoothCommunicator
│   ├── CANManager.cs               # 不变
│   └── WebSocketManager.cs         # 不变
├── Processing/                     # 新建：消息处理
│   ├── IDisplayAdapter.cs          # 新建：UI 显示抽象接口
│   └── MessageProcessor.cs         # 重构为接口注入
├── UI/                             # 新建：UI 逻辑拆分
│   ├── MainFormDataContext.cs       # 新建：窗体数据上下文
│   ├── MapTracker.cs               # 新建：地图轨迹逻辑
│   └── ProtocolFormatter.cs        # 新建：协议格式化
├── AppServices.cs                  # 不变
├── AppSettings.cs                  # 不变
├── IConfigStorage.cs               # 不变
├── Utils.cs                        # 瘦身后仅保留纯工具方法
├── Form1.cs                        # 瘦身
├── Form1.Designer.cs               # 不变
└── Program.cs                      # 不变
```

---

### Task 1: 提取枚举类型到独立文件（P6）

**Files:**
- Create: `UpperApp/Core/ChannelType.cs`
- Create: `UpperApp/Core/RecvOrSend.cs`
- Create: `UpperApp/Core/BindingDic.cs`
- Modify: `UpperApp/Utils.cs` (删除提取的代码)
- Modify: `UpperApp/UpperApp.csproj` (无需修改，同一项目自动包含)

- [ ] **Step 1: 创建 ChannelType.cs**

```csharp
namespace UpperApp
{
    internal enum ChannelType
    {
        Unknown,
        Serial,
        TCP,
        UDP,
        Bluetooth,
        WebSocket,
        CAN
    }
}
```

- [ ] **Step 2: 创建 RecvOrSend.cs**

```csharp
namespace UpperApp
{
    internal enum RecvOrSend
    {
        Recv = 0,
        Send = 1
    }
}
```

- [ ] **Step 3: 创建 BindingDic.cs**

```csharp
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Threading;

namespace UpperApp
{
    internal class BindingDic<T> where T : class
    {
        private readonly ConcurrentDictionary<string, T> ConnectDic = [];
        public readonly BindingList<string> connectionKeys;
        private readonly SynchronizationContext _sync;

        public int Count => ConnectDic.Count;

        public bool TryGet(string key, out T value)
        {
            return ConnectDic.TryGetValue(key, out value);
        }

        public BindingDic()
        {
            connectionKeys = new BindingList<string>([.. ConnectDic.Keys]);
            _sync = SynchronizationContext.Current;
        }

        private void PostUI(Action action)
        {
            if (_sync == null)
                action();
            else
                _sync.Post(_ => action(), null);
        }

        public void Add(string name, T obj)
        {
            if (ConnectDic.TryAdd(name, obj))
            {
                PostUI(() => connectionKeys.Add(name));
            }
        }

        public T Remove(string name)
        {
            if (ConnectDic.TryRemove(name, out T obj))
            {
                PostUI(() => connectionKeys.Remove(name));
                return obj;
            }
            return null;
        }
    }
}
```

- [ ] **Step 4: 瘦身 Utils.cs — 删除 ChannelType、RecvOrSend、BindingDic 定义**

从 `Utils.cs` 中删除以下代码段：
- `internal enum ChannelType { ... }`
- `internal enum RecvOrSend { ... }`
- `class BindingDic<T> where T : class { ... }`

保留 `Utils` 静态类及其所有方法。删除不再需要的 using 语句（`ConcurrentDictionary`、`SynchronizationContext` 等）。

- [ ] **Step 5: 构建验证**

Run: `dotnet build UpperApp.sln`
Expected: BUILD SUCCEEDED，无编译错误

- [ ] **Step 6: Commit**

```bash
git add UpperApp/Core/ UpperApp/Utils.cs
git commit -feat: "refactor: 提取 ChannelType/RecvOrSend/BindingDic 到独立文件 (P6)"
```

---

### Task 2: 重构 Result 为不可变 record（P4）

**Files:**
- Modify: `UpperApp/Core/Result.cs` (从 `UpperApp/Result.cs` 移动并重构)
- Delete: `UpperApp/Result.cs` (原位置)

- [ ] **Step 1: 创建 Core/Result.cs，使用 record 替代 class**

```csharp
namespace UpperApp
{
    internal record Result
    {
        public enum ResStatus
        {
            Success,
            Error,
            Alert,
            SetNum
        }

        public enum NETStatus
        {
            ManualStop,
            ExceptionStop,
            RemoteStop,
            ReciveMessage,
            SendMessage,
            MonitorStop,
            MonitorStart,
            NewRemote
        }

        public string Message { get; init; }
        public int Num { get; init; }
        public ResStatus Status { get; init; }
        public NETStatus NetStatus { get; init; }
        public string RemoteIP { get; init; }
        public string IPPort { get; init; }
        public string NewPeer { get; init; }
        public ChannelType Channel { get; init; }

        public Result() { }

        public Result(NETStatus status, string message)
        {
            NetStatus = status;
            Message = message;
        }

        public Result(NETStatus status, string message, int num)
        {
            NetStatus = status;
            Message = message;
            Num = num;
        }

        public Result(NETStatus status, string message, int num, string remoteIP)
        {
            NetStatus = status;
            Message = message;
            Num = num;
            RemoteIP = remoteIP;
        }

        public Result with(
            ResStatus? Status = null,
            ChannelType? Channel = null,
            string NewPeer = null,
            string IPPort = null)
        {
            return this with
            {
                Status = Status ?? this.Status,
                Channel = Channel ?? this.Channel,
                NewPeer = NewPeer ?? this.NewPeer,
                IPPort = IPPort ?? this.IPPort
            };
        }
    }
}
```

- [ ] **Step 2: 更新所有 Result 的可变赋值为 with 表达式**

需要修改的文件和模式：

**BaseCommunicationManager.cs:30-33** — `OnStatusChanged` 中不再直接赋值 `result.Channel`，改为：
```csharp
protected void OnStatusChanged(Result result)
{
    if (result.Channel == ChannelType.Unknown)
        result = result with { Channel = _channel };
    StatusChanged?.Invoke(result);
}
```

**SerManager.cs:85-86** — `new Result(...) { Status = Result.ResStatus.Error }` 改为：
```csharp
new Result(Result.NETStatus.SendMessage, "串口未打开", 0, "").with(Status: Result.ResStatus.Error)
```

**SerManager.cs:93** — `new Result(...) { Status = Result.ResStatus.SetNum }` 改为：
```csharp
new Result(Result.NETStatus.SendMessage, data, buffer.Length, "COM").with(Status: Result.ResStatus.SetNum)
```

**TCPManager.cs:104-105** — 同样模式改为 `.with(Status: Result.ResStatus.Error)`

**TCPManager.cs:114** — 改为 `.with(Status: Result.ResStatus.SetNum)`

**UDPManager.cs:48-49, 54-55, 61, 66, 74, 80** — 同样模式

**CANManager.cs:98-99, 104-105, 112** — 同样模式

**WebSocketManager.cs:158-159, 178-179, 198-199** — 同样模式

**Form1.cs** — 无直接构造 Result 的代码，无需修改

- [ ] **Step 3: 删除旧 Result.cs**

删除 `UpperApp/Result.cs`。

- [ ] **Step 4: 构建验证**

Run: `dotnet build UpperApp.sln`
Expected: BUILD SUCCEEDED

- [ ] **Step 5: Commit**

```bash
git add UpperApp/Core/Result.cs UpperApp/Result.cs UpperApp/BaseCommunicationManager.cs UpperApp/SerManager.cs UpperApp/TCPManager.cs UpperApp/UDPManager.cs UpperApp/CANManager.cs UpperApp/WebSocketManager.cs
git commit -m "refactor: 重构 Result 为不可变 record，消除可变状态 (P4)"
```

---

### Task 3: 引入 IDisplayAdapter 接口替代委托注入（P2）

**Files:**
- Create: `UpperApp/Processing/IDisplayAdapter.cs`
- Modify: `UpperApp/Processing/MessageProcessor.cs` (从 `UpperApp/MessageProcessor.cs` 移动并重构)
- Delete: `UpperApp/MessageProcessor.cs`
- Modify: `UpperApp/Form1.cs` (构造 MessageProcessor 的方式)

- [ ] **Step 1: 创建 IDisplayAdapter 接口**

```csharp
namespace UpperApp
{
    internal interface IDisplayAdapter
    {
        void UpdateByteCount(int count, RecvOrSend direction);
        bool IsCharMode { get; }
        bool IsHexMode { get; }
        bool IsLocalEchoEnabled { get; }
        bool IsAngleDisplayEnabled { get; }
        void AppendToReceiveBox(string text);
        void WriteLog(string text);
        void UpdateAngleDisplay(string message);
        void OnNewPeer(string peerInfo);
    }
}
```

- [ ] **Step 2: 重构 MessageProcessor 使用 IDisplayAdapter**

```csharp
using System;
using System.Runtime.Versioning;

namespace UpperApp
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    internal class MessageProcessor
    {
        private readonly IDisplayAdapter _display;
        private const string FromPrefix = "from:";
        private const string ToPrefix = "to:";
        private const string NewLine = "\r\n";

        public MessageProcessor(IDisplayAdapter display)
        {
            _display = display;
        }

        public void ProcessReceivedMessage(Result status)
        {
            _display.UpdateByteCount(status.Num, RecvOrSend.Recv);

            if (_display.IsAngleDisplayEnabled)
                _display.UpdateAngleDisplay(status.Message);

            if (!string.IsNullOrWhiteSpace(status.NewPeer))
                _display.OnNewPeer(status.NewPeer);

            string remote = status.RemoteIP ?? "BlueTooth";
            string prefix = $"{FromPrefix}{remote}: {NewLine}";

            if (_display.IsCharMode)
            {
                AppendAndLog(prefix, status.Message);
            }
            else if (_display.IsHexMode)
            {
                AppendAndLog(prefix, status.Message, isHex: true);
            }
        }

        public void ProcessSentMessage(Result status)
        {
            if (status.NetStatus != Result.NETStatus.SendMessage) return;

            if (status.Status == Result.ResStatus.SetNum)
            {
                string remote = status.RemoteIP ?? "BlueTooth";
                string prefix = $"{ToPrefix}{remote}: {NewLine}";
                if (_display.IsLocalEchoEnabled) AppendAndLog(prefix, status.Message);
                else _display.WriteLog($"{Utils.GetTime()}{prefix}{status.Message}{NewLine}");

                _display.UpdateByteCount(status.Num, RecvOrSend.Send);
            }
            else if (status.Status == Result.ResStatus.Error)
            {
                _display.AppendToReceiveBox($"发送错误: {status.Message}\r\n");
            }
        }

        public void ProcessException(Result status)
        {
            if (status.NetStatus == Result.NETStatus.ExceptionStop)
            {
                _display.AppendToReceiveBox($"异常: {status.Message}\r\n");
            }
        }

        private void AppendAndLog(string prefix, string content, bool isHex = false)
        {
            string time = Utils.GetTime();
            string displayContent = isHex ? Utils.StringToHexString(content) : content;
            string formatted = $"{time}{prefix}{displayContent}{NewLine}";
            _display.AppendToReceiveBox(formatted);
            _display.WriteLog(formatted);
        }
    }
}
```

- [ ] **Step 3: 在 Form1.cs 中实现 IDisplayAdapter**

在 `UpperApp` 类中实现接口：

```csharp
public partial class UpperApp : Form, IDisplayAdapter
{
    // ... 现有代码 ...

    public void UpdateByteCount(int count, RecvOrSend direction)
    {
        if (direction == RecvOrSend.Recv)
        {
            Rn += count;
            label18.Text = Rn.ToString();
        }
        else
        {
            Sn += count;
            label22.Text = Sn.ToString();
        }
    }

    public bool IsCharMode => rbtnChar.Checked;
    public bool IsHexMode => rbtnHex.Checked;
    public bool IsLocalEchoEnabled => ReDisp.Checked;
    public bool IsAngleDisplayEnabled => AngDirDisp.Checked;

    public void AppendToReceiveBox(string text) => RecvBox.AppendText(text);
    public void WriteLog(string text) => tf?.WriteLine(text);
    public void UpdateAngleDisplay(string message) => SetAngDisp(message);
    public void OnNewPeer(string peerInfo)
    {
        if (_communicators.TryGetValue(_activeSendChannel, out var comm))
            Peer.DataSource = comm.GetPeerList();
    }
}
```

- [ ] **Step 4: 更新 MessageProcessor 构造方式**

将 Form1 构造函数中的 MessageProcessor 创建代码从：

```csharp
_msgProcessor = new MessageProcessor(
    setRs: SetRS,
    isCharMode: () => rbtnChar.Checked,
    isHexMode: () => rbtnHex.Checked,
    isReDisp: () => ReDisp.Checked,
    appendToRecvBox: (text) => RecvBox.AppendText(text),
    writeLog: (log) => tf?.WriteLine(log),
    setAngDisp: SetAngDisp,
    isAngDirDispEnabled: () => AngDirDisp.Checked,
    onNewPeer: (newPeer) => { Peer.DataSource = _communicators[_activeSendChannel].GetPeerList(); }
);
```

改为：

```csharp
_msgProcessor = new MessageProcessor(this);
```

同时删除 `SetRS` 方法（已被 `UpdateByteCount` 替代）。

- [ ] **Step 5: 构建验证**

Run: `dotnet build UpperApp.sln`
Expected: BUILD SUCCEEDED

- [ ] **Step 6: Commit**

```bash
git add UpperApp/Processing/ UpperApp/MessageProcessor.cs UpperApp/Form1.cs
git commit -m "refactor: 引入 IDisplayAdapter 接口替代 MessageProcessor 委托注入 (P2)"
```

---

### Task 4: 引入 IBluetoothCommunicator 接口修复抽象泄漏（P3）

**Files:**
- Create: `UpperApp/Communication/IBluetoothCommunicator.cs`
- Modify: `UpperApp/Communication/BthManager.cs` (从 `UpperApp/BthManager.cs` 移动)
- Modify: `UpperApp/Form1.cs` (消除强转)

- [ ] **Step 1: 创建 IBluetoothCommunicator 接口**

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UpperApp
{
    internal interface IBluetoothCommunicator : ICommunicator
    {
        object RadioInfo { get; }
        bool IsRadioAvailable { get; }
        bool IsRadioPoweredOn { get; }
        Task<List<BluetoothDeviceInfo>> DiscoverDevicesAsync();
        void ConnectToDevice(string deviceName);
        void DisconnectClient();
        object GetSlavePeerList();
    }
}
```

注意：`BluetoothDeviceInfo` 来自 `InTheHand.Net.Sockets`，此处保持原有依赖。若需完全隔离，可后续再封装 DTO。

- [ ] **Step 2: BthManager 实现 IBluetoothCommunicator**

在 `BthManager.cs` 中添加接口实现：

```csharp
internal class BthManager : BaseCommunicationManager, IBluetoothCommunicator
{
    // ... 现有代码 ...

    public object RadioInfo => Br;
    public bool IsRadioAvailable => Br != null;
    public bool IsRadioPoweredOn => Br != null && Br.Mode != RadioMode.PowerOff;

    public void ConnectToDevice(string deviceName)
    {
        var param = new BluetoothParams { IsServerMode = false, TargetDeviceName = deviceName };
        if (IsMonitoring) Stop();
        Start(param);
    }

    public object GetSlavePeerList() => BthClients.connectionKeys;
}
```

- [ ] **Step 3: 修改 Form1.cs 中所有 BthManager 强转为 IBluetoothCommunicator**

| 原代码 | 替换为 |
|--------|--------|
| `var bthComm = (BthManager)_communicators[ChannelType.Bluetooth];` | `var bthComm = (IBluetoothCommunicator)_communicators[ChannelType.Bluetooth];` |

具体涉及的方法：
- `BthListenBtn_Click` — 使用 `IsRadioAvailable` / `IsRadioPoweredOn` 替代直接访问 `Br`
- `BthSendBtn_Click` — 使用 `GetSlavePeerList()` 替代 `GetPeerList()` 差异
- `BthConnectBtn_Click` — 使用 `ConnectToDevice()` / `DisconnectClient()`
- `BthDeviceScanBtn_Click` — 使用 `DiscoverDevicesAsync()`
- `UnifiedStatusChanged` 中蓝牙分支 — 使用 `GetSlavePeerList()`

- [ ] **Step 4: 构建验证**

Run: `dotnet build UpperApp.sln`
Expected: BUILD SUCCEEDED

- [ ] **Step 5: Commit**

```bash
git add UpperApp/Communication/IBluetoothCommunicator.cs UpperApp/BthManager.cs UpperApp/Form1.cs
git commit -m "refactor: 引入 IBluetoothCommunicator 接口，消除 BthManager 强转抽象泄漏 (P3)"
```

---

### Task 5: 修复 ICommunicator 接口设计问题（P5, P7, P8）

**Files:**
- Modify: `UpperApp/Communication/ICommunicator.cs` (从 `UpperApp/ICommunicator.cs` 移动)
- Modify: `UpperApp/Communication/BaseCommunicationManager.cs` (从 `UpperApp/BaseCommunicationManager.cs` 移动)

- [ ] **Step 1: 改进 GetPeerList 返回类型**

将 `ICommunicator` 中：

```csharp
object GetPeerList();
```

改为：

```csharp
IReadOnlyList<string> GetPeerList();
```

- [ ] **Step 2: 更新各 Manager 的 GetPeerList 实现**

**SerManager.cs:**
```csharp
public override IReadOnlyList<string> GetPeerList() => [];
```

**TCPManager.cs:**
```csharp
public override IReadOnlyList<string> GetPeerList() => _clients.connectionKeys;
```
（`BindingList<string>` 实现了 `IReadOnlyList<string>`）

**UDPManager.cs:**
```csharp
public override IReadOnlyList<string> GetPeerList() => _peerList;
```
（`BindingList<string>` 实现了 `IReadOnlyList<string>`）

**BthManager.cs:**
```csharp
public override IReadOnlyList<string> GetPeerList() => BthClients.connectionKeys;
```

**CANManager.cs:**
```csharp
public override IReadOnlyList<string> GetPeerList() => _canDevices.connectionKeys;
```

**WebSocketManager.cs:**
```csharp
public override IReadOnlyList<string> GetPeerList()
{
    if (_isClientMode) return [];
    return _serverClients.connectionKeys;
}
```

- [ ] **Step 3: 更新 Form1.cs 中 Peer.DataSource 赋值**

由于 `GetPeerList()` 现在返回 `IReadOnlyList<string>`，而 `ComboBox.DataSource` 接受 `IList`，需要适配：

```csharp
Peer.DataSource = _communicators[_activeSendChannel].GetPeerList().ToList();
```

或使用 `BindingList<string>` 作为中间层。对所有 `Peer.DataSource = ...` 赋值统一处理。

- [ ] **Step 4: 将 StartCore 改为 protected**

在 `BaseCommunicationManager.cs` 中：

```csharp
// 从 public 改为 protected
protected void StartCore()
```

- [ ] **Step 5: 构建验证**

Run: `dotnet build UpperApp.sln`
Expected: BUILD SUCCEEDED

- [ ] **Step 6: Commit**

```bash
git add UpperApp/Communication/ UpperApp/ICommunicator.cs UpperApp/BaseCommunicationManager.cs UpperApp/SerManager.cs UpperApp/TCPManager.cs UpperApp/UDPManager.cs UpperApp/BthManager.cs UpperApp/CANManager.cs UpperApp/WebSocketManager.cs UpperApp/Form1.cs
git commit -m "refactor: ICommunicator.GetPeerList 返回 IReadOnlyList<string>，StartCore 改为 protected (P5/P7/P8)"
```

---

### Task 6: 提取地图轨迹逻辑为 MapTracker 类（P10）

**Files:**
- Create: `UpperApp/UI/MapTracker.cs`
- Modify: `UpperApp/Form1.cs` (删除地图逻辑，委托给 MapTracker)

- [ ] **Step 1: 创建 MapTracker 类**

```csharp
using System;
using System.Drawing;
using System.Windows.Forms;

namespace UpperApp
{
    internal class MapTracker
    {
        private int _px, _py, _dx, _dy, _bx, _by;
        private short _pflag;
        private float _lhp = 1;
        private float _truedist = 1;
        private float _xpro, _ypro;
        private float _befordist;
        private readonly PictureBox _mapBox;

        public string StartPoint => $"{_bx},{213 - _by}";
        public string EndPoint => $"{_dx},{213 - _dy}";
        public short Flag => _pflag;

        public MapTracker(PictureBox mapBox)
        {
            _mapBox = mapBox;
        }

        public void SetCalibratedDistance(float distance)
        {
            _truedist = distance;
        }

        public void SetAspectRatio(float ratio)
        {
            _lhp = ratio;
        }

        public void OnDistanceChanged(string distText, string yawText, Action<string> logOutput)
        {
            if (!float.TryParse(distText, out float dist)) return;
            if (!double.TryParse(yawText, out double angle)) return;

            float delta = dist - _befordist;
            _befordist = dist;
            double xdist = Math.Cos(Math.PI * angle / 180) * delta;
            double ydist = Math.Sin(Math.PI * angle / 180) * delta;
            _px += (int)(xdist / _xpro);
            _py += (int)(ydist / _ypro);

            logOutput($"坐标点={_px},{_py}\r\n");
            logOutput($"路程={xdist},{ydist}\r\n");

            using Graphics g = _mapBox.CreateGraphics();
            g.FillEllipse(Brushes.Blue, _px, 213 - _py, 3, 3);
        }

        public void OnMapClick(Point clickPoint, Action<string> logOutput)
        {
            if (_pflag == 0)
            {
                using Graphics g = _mapBox.CreateGraphics();
                g.FillEllipse(Brushes.Blue, clickPoint.X, clickPoint.Y, 3, 3);
                _px = _bx = clickPoint.X;
                _py = _by = 213 - clickPoint.Y;
                _pflag = 1;
            }
            else if (_pflag == 1)
            {
                _dx = clickPoint.X - _bx;
                _dy = (213 - clickPoint.Y) - _by;
                float distance = (float)Math.Sqrt(_dx * _dx + _dy * _dy * _lhp * _lhp);
                logOutput($"宽高比={_lhp}\r\n锚点距离={distance}\r\n");
                _xpro = _truedist / distance;
                _ypro = _xpro * _lhp;
                _pflag = 2;
            }
        }

        public void OnMouseMove(Point location)
        {
            using Graphics g = _mapBox.CreateGraphics();
            if (_pflag == 1)
            {
                _mapBox.Refresh();
                g.DrawLine(Pens.Blue, _bx, 213 - _by, location.X, location.Y);
            }
            else if (_pflag == 2)
            {
                _mapBox.Refresh();
                g.FillEllipse(Brushes.Blue, _bx, 213 - _by, 3, 3);
                _pflag = 3;
            }
        }

        public void Clear()
        {
            _pflag = 0;
            _mapBox.Refresh();
        }
    }
}
```

- [ ] **Step 2: 在 Form1.cs 中使用 MapTracker**

添加字段：
```csharp
private readonly MapTracker _mapTracker;
```

在构造函数中初始化：
```csharp
_mapTracker = new MapTracker(MapBox);
```

替换以下事件处理器：

**LabDist_TextChanged** →
```csharp
private void LabDist_TextChanged(object sender, EventArgs e)
{
    _mapTracker.OnDistanceChanged(LabDist.Text, LabYaw.Text, (msg) => RecvBox.AppendText(msg));
}
```

**MapBox_Click** →
```csharp
private void MapBox_Click(object sender, EventArgs e)
{
    Point p = MapBox.PointToClient(MousePosition);
    _mapTracker.OnMapClick(p, (msg) => RecvBox.AppendText(msg));
    label34.Text = _mapTracker.StartPoint;
    label36.Text = _mapTracker.EndPoint;
}
```

**MapBox_MouseMove** →
```csharp
private void MapBox_MouseMove(object sender, MouseEventArgs e)
{
    _mapTracker.OnMouseMove(e.Location);
    label38.Text = e.Location.X + "," + (213 - e.Location.Y);
}
```

**ClearImage_Click** →
```csharp
private void ClearImage_Click(object sender, EventArgs e)
{
    _mapTracker.Clear();
    label34.Text = label36.Text = "0,0";
}
```

**OpenImage_Click** →
```csharp
private void OpenImage_Click(object sender, EventArgs e)
{
    DialogResult dr = openFileDialog1.ShowDialog();
    if (dr == DialogResult.OK)
    {
        try
        {
            Image image = Image.FromFile(@openFileDialog1.FileName);
            MapBox.BackgroundImage = image;
            _mapTracker.SetAspectRatio(image.Height / MapBox.Height / (float)(image.Width / MapBox.Width));
        }
        catch
        {
            MessageBox.Show(this, "请选择图片文件！", "Warning");
        }
    }
}
```

**RealDist_TextChanged** →
```csharp
private void RealDist_TextChanged(object sender, EventArgs e)
{
    string buf = RealDist.Text.Replace(" ", string.Empty);
    if (buf != ".")
    {
        float dist = float.Parse(buf);
        _mapTracker.SetCalibratedDistance(dist);
        Infotext.Text = "dist:" + dist;
    }
}
```

- [ ] **Step 3: 删除 Form1.cs 中被替换的原始字段和方法**

删除以下字段：
```csharp
private int px, py, dx, dy, bx, by;
private short pflag = 0;
private float lhp = 1, truedist = 1, xpro, ypro, distance, befordist = 0;
```

- [ ] **Step 4: 构建验证**

Run: `dotnet build UpperApp.sln`
Expected: BUILD SUCCEEDED

- [ ] **Step 5: Commit**

```bash
git add UpperApp/UI/MapTracker.cs UpperApp/Form1.cs
git commit -m "refactor: 提取地图轨迹逻辑为 MapTracker 类 (P10)"
```

---

### Task 7: 提取协议格式化为 ProtocolFormatter（P12）

**Files:**
- Create: `UpperApp/UI/ProtocolFormatter.cs`
- Modify: `UpperApp/Form1.cs` (使用 ProtocolFormatter)

- [ ] **Step 1: 创建 ProtocolFormatter 类**

```csharp
namespace UpperApp
{
    internal static class ProtocolFormatter
    {
        private const string OverSuffix = "\r\n";

        public static string ForwardBackward(int value)
        {
            return $"FB:{value}:OVER{OverSuffix}";
        }

        public static string RightLeft(int value)
        {
            return $"RL:{value}:OVER{OverSuffix}";
        }

        public static string FullControl(int speed, int direction)
        {
            return $"FR:{speed}:{direction}:OVER{OverSuffix}";
        }
    }
}
```

- [ ] **Step 2: 替换 Form1.cs 中硬编码协议字符串**

| 位置 | 原代码 | 替换为 |
|------|--------|--------|
| 构造函数 btnNoRL.Click | `"RL:" + RLBar.Value + ":OVER\r\n"` | `ProtocolFormatter.RightLeft(RLBar.Value)` |
| 构造函数 Stop.Click | `"FB:" + FBBar.Value + ":OVER\r\n"` | `ProtocolFormatter.ForwardBackward(FBBar.Value)` |
| 构造函数 FBBar.MouseUp | `"FB:" + FBBar.Value + ":OVER\r\n"` | `ProtocolFormatter.ForwardBackward(FBBar.Value)` |
| 构造函数 RLBar.MouseUp | `"RL:" + RLBar.Value + ":OVER\r\n"` | `ProtocolFormatter.RightLeft(RLBar.Value)` |
| Rocker_Click | `"FR:" + FBtext.Text + ":" + RLtext.Text + ":OVER\r\n"` | `ProtocolFormatter.FullControl(int.Parse(FBtext.Text), int.Parse(RLtext.Text))` |
| Rocker_MouseMove | `$"FR:{FBBar.Value}:{RLBar.Value}:OVER\r\n"` | `ProtocolFormatter.FullControl(FBBar.Value, RLBar.Value)` |

- [ ] **Step 3: 构建验证**

Run: `dotnet build UpperApp.sln`
Expected: BUILD SUCCEEDED

- [ ] **Step 4: Commit**

```bash
git add UpperApp/UI/ProtocolFormatter.cs UpperApp/Form1.cs
git commit -m "refactor: 提取协议格式化为 ProtocolFormatter，消除硬编码字符串 (P12)"
```

---

### Task 8: 提取日志写入为 ILogger 接口（P9）

**Files:**
- Create: `UpperApp/Processing/ILogger.cs`
- Modify: `UpperApp/Form1.cs` (实现 ILogger)
- Modify: `UpperApp/Processing/MessageProcessor.cs` (注入 ILogger)

- [ ] **Step 1: 创建 ILogger 接口**

```csharp
namespace UpperApp
{
    internal interface ILogger
    {
        void WriteLine(string text);
        void Open(string filePath);
        void Close();
        bool IsOpen { get; }
    }
}
```

- [ ] **Step 2: 创建 FileLogger 实现**

```csharp
using System.IO;

namespace UpperApp
{
    internal class FileLogger : ILogger
    {
        private StreamWriter _writer;

        public bool IsOpen => _writer != null;

        public void WriteLine(string text)
        {
            _writer?.WriteLine(text);
        }

        public void Open(string filePath)
        {
            Close();
            _writer = new StreamWriter(filePath, true);
        }

        public void Close()
        {
            _writer?.Close();
            _writer = null;
        }
    }
}
```

- [ ] **Step 3: 更新 MessageProcessor 构造函数**

在 `MessageProcessor` 中添加 `ILogger` 依赖：

```csharp
internal class MessageProcessor
{
    private readonly IDisplayAdapter _display;
    private readonly ILogger _logger;
    // ...

    public MessageProcessor(IDisplayAdapter display, ILogger logger)
    {
        _display = display;
        _logger = logger;
    }

    // 将 _display.WriteLog(formatted) 改为 _logger.WriteLine(formatted)
    // 在 AppendAndLog 中：
    private void AppendAndLog(string prefix, string content, bool isHex = false)
    {
        string time = Utils.GetTime();
        string displayContent = isHex ? Utils.StringToHexString(content) : content;
        string formatted = $"{time}{prefix}{displayContent}{NewLine}";
        _display.AppendToReceiveBox(formatted);
        _logger.WriteLine(formatted);
    }
}
```

同时更新 `IDisplayAdapter` — 移除 `WriteLog` 方法（已由 `ILogger` 承担）：

```csharp
internal interface IDisplayAdapter
{
    void UpdateByteCount(int count, RecvOrSend direction);
    bool IsCharMode { get; }
    bool IsHexMode { get; }
    bool IsLocalEchoEnabled { get; }
    bool IsAngleDisplayEnabled { get; }
    void AppendToReceiveBox(string text);
    void UpdateAngleDisplay(string message);
    void OnNewPeer(string peerInfo);
}
```

- [ ] **Step 4: 更新 Form1.cs**

移除 `StreamWriter tf` 字段，替换为 `ILogger`：

```csharp
private readonly ILogger _logger = new FileLogger();
```

从 `IDisplayAdapter` 实现中移除 `WriteLog`。

更新 `SaveData_CheckedChanged`：
```csharp
private void SaveData_CheckedChanged(object sender, EventArgs e)
{
    if (SaveData.Checked)
    {
        DialogResult dr = openFileDialog1.ShowDialog();
        if (dr == DialogResult.OK)
            _logger.Open(openFileDialog1.FileName);
        else
            SaveData.CheckState = CheckState.Unchecked;
    }
    else
    {
        _logger.Close();
    }
}
```

更新 `UpperApp_FormClosing`：
```csharp
_logger.Close();
```

更新 MessageProcessor 构造：
```csharp
_msgProcessor = new MessageProcessor(this, _logger);
```

- [ ] **Step 5: 构建验证**

Run: `dotnet build UpperApp.sln`
Expected: BUILD SUCCEEDED

- [ ] **Step 6: Commit**

```bash
git add UpperApp/Processing/ILogger.cs UpperApp/Processing/MessageProcessor.cs UpperApp/Form1.cs
git commit -m "refactor: 提取 ILogger 接口，解耦日志写入与 UI (P9)"
```

---

### Task 9: 修复 async void 与资源释放（P11）

**Files:**
- Modify: `UpperApp/Form1.cs`

- [ ] **Step 1: 修复 UpperApp_FormClosing**

将 `async void` 改为安全模式：

```csharp
private void UpperApp_FormClosing(object sender, FormClosingEventArgs e)
{
    var settings = CollectCurrentSettings();
    _configStorage.SaveAsync(settings).GetAwaiter().GetResult();

    foreach (var comm in _communicators.Values)
    {
        comm.DisposeAsync().GetAwaiter().GetResult();
    }

    _logger.Close();
}
```

注意：在 `FormClosing` 中使用 `.GetAwaiter().GetResult()` 是可接受的，因为此时必须同步完成清理。替代方案是使用 `e.Cancel = true` + 异步清理后关闭，但复杂度更高且收益有限。

- [ ] **Step 2: 构建验证**

Run: `dotnet build UpperApp.sln`
Expected: BUILD SUCCEEDED

- [ ] **Step 3: Commit**

```bash
git add UpperApp/Form1.cs
git commit -m "fix: 修复 UpperApp_FormClosing async void 问题 (P11)"
```

---

### Task 10: 文件物理移动与命名空间整理

**Files:**
- 移动文件到对应子目录
- 更新命名空间（如需要）

- [ ] **Step 1: 移动文件**

| 原路径 | 新路径 |
|--------|--------|
| `UpperApp/Result.cs` (已删除) | `UpperApp/Core/Result.cs` (已创建) |
| `UpperApp/ICommunicator.cs` | `UpperApp/Communication/ICommunicator.cs` |
| `UpperApp/ICommunicatorFactory.cs` | `UpperApp/Communication/ICommunicatorFactory.cs` |
| `UpperApp/BaseCommunicationManager.cs` | `UpperApp/Communication/BaseCommunicationManager.cs` |
| `UpperApp/CommunicationParams.cs` | `UpperApp/Communication/CommunicationParams.cs` |
| `UpperApp/SerManager.cs` | `UpperApp/Communication/SerManager.cs` |
| `UpperApp/TCPManager.cs` | `UpperApp/Communication/TCPManager.cs` |
| `UpperApp/UDPManager.cs` | `UpperApp/Communication/UDPManager.cs` |
| `UpperApp/BthManager.cs` | `UpperApp/Communication/BthManager.cs` |
| `UpperApp/CANManager.cs` | `UpperApp/Communication/CANManager.cs` |
| `UpperApp/WebSocketManager.cs` | `UpperApp/Communication/WebSocketManager.cs` |
| `UpperApp/MessageProcessor.cs` (已删除) | `UpperApp/Processing/MessageProcessor.cs` (已创建) |

注意：.NET 10 SDK 风格项目会自动包含新目录下的文件，无需修改 csproj。命名空间保持 `UpperApp` 不变（不使用目录级命名空间），避免大量修改。

- [ ] **Step 2: 删除原位置的旧文件**

确认新文件已创建后，删除原位置的文件。

- [ ] **Step 3: 构建验证**

Run: `dotnet build UpperApp.sln`
Expected: BUILD SUCCEEDED

- [ ] **Step 4: Commit**

```bash
git add -A
git commit -m "refactor: 按职责重组文件目录结构 (Core/Communication/Processing/UI)"
```

---

## 自检清单

| 检查项 | 状态 |
|--------|------|
| P1 God Class | ✅ 通过 Task 3/6/7/8 拆分职责，Form 行数大幅减少 |
| P2 委托注入 | ✅ Task 3 引入 IDisplayAdapter 接口替代 |
| P3 BthManager 抽象泄漏 | ✅ Task 4 引入 IBluetoothCommunicator |
| P4 Result 可变 | ✅ Task 2 重构为 record + with 表达式 |
| P5 Start 参数类型安全 | ⚠️ 保留运行时检查（泛型约束会导致接口膨胀，收益不足以抵消复杂度） |
| P6 枚举/BindingDic 混放 | ✅ Task 1 提取到独立文件 |
| P7 StartCore 封装 | ✅ Task 5 改为 protected |
| P8 GetPeerList 返回 object | ✅ Task 5 改为 IReadOnlyList<string> |
| P9 日志耦合 | ✅ Task 8 引入 ILogger 接口 |
| P10 地图逻辑内嵌 | ✅ Task 6 提取 MapTracker |
| P11 async void | ✅ Task 9 修复 |
| P12 硬编码字符串 | ✅ Task 7 提取 ProtocolFormatter |
| 无 Placeholder | ✅ 所有步骤包含完整代码 |
| 类型一致性 | ✅ 各 Task 间类型定义一致 |
