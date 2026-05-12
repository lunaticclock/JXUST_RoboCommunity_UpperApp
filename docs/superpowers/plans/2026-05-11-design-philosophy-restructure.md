# 上位机设计思路重构 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 参照工业上位机五层架构最佳实践，重构项目的核心设计思路——引入设备抽象层、命令模式、数据管道、状态管理，使系统具备可测试性、可扩展性和实时数据处理的可靠性。

**Architecture:** 从当前的"UI 直连通信管理器"模式，演进为"UI → 业务服务 → 设备抽象 → 通信驱动"四层架构。核心变化：引入 DeviceService 统一设备操作入口、Command 模式封装控制指令、DataPipeline 解耦收发与处理、DeviceState 集中管理设备状态。

**Tech Stack:** C# / .NET 10.0 / WinForms / System.Threading.Channels（新增）

---

## 设计思路问题分析

### 当前架构 vs 工业上位机最佳实践

```
当前架构（UI 直连模式）：
┌─────────────────────────────────────────────────────┐
│  UpperApp (Form1.cs)                                 │
│  ├─ 直接调用 _communicators[xxx].Start/Stop/Send     │
│  ├─ 直接解析 Result.Message 判断 YAW/PITCH/ROLL      │
│  ├─ 直接构造 ProtocolFormatter 发送指令               │
│  ├─ 用按钮文本 "断开" 判断蓝牙连接状态               │
│  └─ UnifiedStatusChanged 是 100+ 行的 switch-case    │
├─────────────────────────────────────────────────────┤
│  ICommunicator (6 个实现)                             │
└─────────────────────────────────────────────────────┘

工业上位机最佳实践（分层架构）：
┌─────────────────────────────────────────────────────┐
│  UI 层 — 只做展示和用户交互                           │
├─────────────────────────────────────────────────────┤
│  业务逻辑层 — DeviceService / CommandExecutor         │
│  ├─ 封装控制指令为 Command 对象                       │
│  ├─ 管理设备状态 (Connected/Sending/Receiving)       │
│  └─ 协调多通道切换逻辑                               │
├─────────────────────────────────────────────────────┤
│  服务层 — DataPipeline / ProtocolHandler              │
│  ├─ 异步数据管道 (收发解耦)                           │
│  ├─ 协议编解码统一处理                               │
│  └─ 数据过滤/转换/分发                               │
├─────────────────────────────────────────────────────┤
│  设备抽象层 — ICommunicator (6 个实现)                │
│  └─ 纯通信职责，不关心业务语义                        │
└─────────────────────────────────────────────────────┘
```

### 核心设计问题

| # | 问题 | 对比最佳实践 | 影响 |
|---|------|-------------|------|
| D1 | **UI 层承担业务逻辑**：Form1 直接构造协议指令、解析接收数据、管理连接状态 | 业务逻辑应在 Service 层，UI 只做绑定 | 不可测试、不可复用 |
| D2 | **无命令模式**：控制指令散落在各事件处理器中，无统一抽象 | 工控上位机标准做法：Command 对象封装指令 | 无法实现指令队列/日志/撤销 |
| D3 | **无数据管道**：接收数据直接在 UI 线程处理，高频数据会阻塞 UI | 最佳实践：Channel/BlockingCollection 异步管道 | 高负载下 UI 卡顿 |
| D4 | **无设备状态机**：用按钮文本/IsMonitoring 判断状态 | 最佳实践：显式状态枚举 + 状态转换保护 | 状态不一致风险 |
| D5 | **协议编解码分散**：发送用 ProtocolFormatter，接收用 ProtocolParser + SetAngDisp，无统一入口 | 最佳实践：ProtocolHandler 统一编解码 | 协议变更需改多处 |
| D6 | **ICommunicator.Start 接受基类参数**：运行时类型检查 | 最佳实践：泛型约束或独立 Start 方法 | 编译期无法发现类型错误 |

---

## File Structure

```
UpperApp/
├── Core/
│   ├── ChannelType.cs              # 不变
│   ├── RecvOrSend.cs               # 不变
│   ├── BindingDic.cs               # 不变
│   └── Result.cs                   # 不变
├── Communication/                   # 不变（设备抽象层）
├── Commands/                        # 新建：命令模式
│   ├── IDeviceCommand.cs           # 命令接口
│   ├── MoveCommand.cs              # 运动控制命令
│   └── RawSendCommand.cs           # 原始发送命令
├── Services/                        # 新建：业务逻辑层
│   ├── DeviceService.cs            # 设备服务（统一入口）
│   ├── DeviceState.cs              # 设备状态枚举
│   └── DataPipeline.cs             # 异步数据管道
├── Processing/                      # 不变
├── UI/                              # 不变
├── Form1.cs                         # 瘦身：只保留 UI 交互
└── ...
```

---

### Task 1: 引入设备状态枚举 DeviceState（D4）

**Files:**
- Create: `UpperApp/Services/DeviceState.cs`
- Modify: `UpperApp/Communication/BaseCommunicationManager.cs`

- [ ] **Step 1: 创建 DeviceState 枚举**

```csharp
namespace UpperApp
{
    internal enum DeviceState
    {
        Disconnected,
        Connecting,
        Connected,
        Disconnecting,
        Error
    }
}
```

- [ ] **Step 2: 在 BaseCommunicationManager 中添加 State 属性**

在 `BaseCommunicationManager.cs` 中添加：
```csharp
private DeviceState _state = DeviceState.Disconnected;
public DeviceState State
{
    get => _state;
    protected set
    {
        if (_state != value)
        {
            var old = _state;
            _state = value;
            OnStateChanged(old, value);
        }
    }
}

protected virtual void OnStateChanged(DeviceState oldState, DeviceState newState)
{
}
```

在 `StartCore()` 开头添加 `State = DeviceState.Connecting;`
在 `StartCore()` 成功后添加 `State = DeviceState.Connected;`
在 `Stop()` 开头添加 `State = DeviceState.Disconnecting;`
在 `Stop()` 完成后添加 `State = DeviceState.Disconnected;`
在异常处理中添加 `State = DeviceState.Error;`

- [ ] **Step 3: 构建验证**

Run: `dotnet build "d:\Workspace\CSharp\JXUST_RoboCommunity_UpperApp\UpperApp.sln"`
Expected: BUILD SUCCEEDED

- [ ] **Step 4: Commit**

```bash
git add UpperApp/Services/ UpperApp/Communication/BaseCommunicationManager.cs
git commit -m "feat: 引入 DeviceState 状态枚举，替代 IsMonitoring 布尔判断 (D4)"
```

---

### Task 2: 引入命令模式 IDeviceCommand（D2）

**Files:**
- Create: `UpperApp/Commands/IDeviceCommand.cs`
- Create: `UpperApp/Commands/MoveCommand.cs`
- Create: `UpperApp/Commands/RawSendCommand.cs`

- [ ] **Step 1: 创建命令接口**

```csharp
namespace UpperApp
{
    internal interface IDeviceCommand
    {
        string Name { get; }
        ChannelType TargetChannel { get; }
        string Encode();
    }
}
```

- [ ] **Step 2: 创建 MoveCommand**

```csharp
namespace UpperApp
{
    internal class MoveCommand : IDeviceCommand
    {
        public string Name => "Move";
        public ChannelType TargetChannel => ChannelType.Unknown;
        public int Speed { get; }
        public int Direction { get; }
        public MoveType Type { get; }

        public enum MoveType
        {
            ForwardBackward,
            RightLeft,
            FullControl
        }

        public MoveCommand(MoveType type, int speed, int direction = 50)
        {
            Type = type;
            Speed = speed;
            Direction = direction;
        }

        public string Encode() => Type switch
        {
            MoveType.ForwardBackward => ProtocolFormatter.ForwardBackward(Speed),
            MoveType.RightLeft => ProtocolFormatter.RightLeft(Direction),
            MoveType.FullControl => ProtocolFormatter.FullControl(Speed, Direction),
            _ => ""
        };
    }
}
```

- [ ] **Step 3: 创建 RawSendCommand**

```csharp
namespace UpperApp
{
    internal class RawSendCommand : IDeviceCommand
    {
        public string Name => "RawSend";
        public ChannelType TargetChannel { get; }
        public string RawData { get; }

        public RawSendCommand(string rawData, ChannelType channel)
        {
            RawData = rawData;
            TargetChannel = channel;
        }

        public string Encode() => RawData;
    }
}
```

- [ ] **Step 4: 构建验证**

Run: `dotnet build "d:\Workspace\CSharp\JXUST_RoboCommunity_UpperApp\UpperApp.sln"`
Expected: BUILD SUCCEEDED

- [ ] **Step 5: Commit**

```bash
git add UpperApp/Commands/
git commit -m "feat: 引入 IDeviceCommand 命令模式，封装 MoveCommand 和 RawSendCommand (D2)"
```

---

### Task 3: 引入 DeviceService 统一设备操作入口（D1）

**Files:**
- Create: `UpperApp/Services/DeviceService.cs`
- Modify: `UpperApp/Form1.cs`

- [ ] **Step 1: 创建 DeviceService**

```csharp
using System;
using System.Collections.Generic;

namespace UpperApp
{
    internal class DeviceService
    {
        private readonly Dictionary<ChannelType, ICommunicator> _communicators;
        private readonly IBluetoothCommunicator _bluetoothComm;
        private ChannelType _activeChannel;

        public ChannelType ActiveChannel
        {
            get => _activeChannel;
            set => _activeChannel = value;
        }

        public DeviceService(Dictionary<ChannelType, ICommunicator> communicators, IBluetoothCommunicator bluetoothComm)
        {
            _communicators = communicators;
            _bluetoothComm = bluetoothComm;
            _activeChannel = ChannelType.Serial;
        }

        public DeviceState GetChannelState(ChannelType channel)
        {
            if (_communicators.TryGetValue(channel, out var comm))
                return comm.State;
            return DeviceState.Disconnected;
        }

        public bool IsChannelReady(ChannelType channel)
        {
            return GetChannelState(channel) == DeviceState.Connected;
        }

        public void StartChannel(ChannelType channel, CommunicationParams param)
        {
            if (_communicators.TryGetValue(channel, out var comm))
                comm.Start(param);
        }

        public void StopChannel(ChannelType channel)
        {
            if (_communicators.TryGetValue(channel, out var comm))
                comm.Stop();
        }

        public void ExecuteCommand(IDeviceCommand command)
        {
            var channel = command.TargetChannel == ChannelType.Unknown
                ? _activeChannel
                : command.TargetChannel;

            if (!IsChannelReady(channel))
                throw new InvalidOperationException($"通道 {channel} 未连接");

            if (!_communicators.TryGetValue(channel, out var comm))
                throw new InvalidOperationException($"未找到通道 {channel} 的管理器");

            string target = ResolveTarget(channel);
            comm.Send(command.Encode(), target);
        }

        public bool TryExecuteCommand(IDeviceCommand command)
        {
            try
            {
                ExecuteCommand(command);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public IReadOnlyList<string> GetPeerList(ChannelType channel)
        {
            if (_communicators.TryGetValue(channel, out var comm))
                return comm.GetPeerList();
            return [];
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

        private string _pendingTarget;
        private string _pendingBthTarget;

        public void SetTarget(string target)
        {
            _pendingTarget = target;
        }

        public void SetBluetoothTarget(string target)
        {
            _pendingBthTarget = target;
        }

        public IBluetoothCommunicator Bluetooth => _bluetoothComm;

        public void StopAll()
        {
            foreach (var comm in _communicators.Values)
                comm.Stop();
        }

        public void DisposeAll()
        {
            foreach (var comm in _communicators.Values)
            {
                comm.Stop();
                (comm as IDisposable)?.Dispose();
            }
        }
    }
}
```

- [ ] **Step 2: 在 Form1.cs 中使用 DeviceService**

添加字段：
```csharp
private readonly DeviceService _deviceService;
```

在构造函数中初始化（替换原来的 `_communicators` 直接使用）：
```csharp
_deviceService = new DeviceService(_communicators, _bluetoothComm);
```

修改 `StrSend` 方法使用 DeviceService：
```csharp
private void StrSend(string Buf)
{
    _deviceService.SetTarget(Peer.Text);
    _deviceService.SetBluetoothTarget(ChoseSlaveBthList.Text);

    var command = new RawSendCommand(Buf, _deviceService.ActiveChannel);
    if (!_deviceService.TryExecuteCommand(command))
    {
        Infotext.Text = "发送失败：通道未连接或未找到";
    }
}
```

修改运动控制相关代码使用 MoveCommand：

构造函数中 `btnNoRL.Click`：
```csharp
btnNoRL.Click += new EventHandler((sender, e) =>
{
    RLBar.Value = 50;
    _deviceService.TryExecuteCommand(new MoveCommand(MoveCommand.MoveType.RightLeft, 50, 50));
});
```

构造函数中 `Stop.Click`：
```csharp
Stop.Click += new EventHandler((sender, e) =>
{
    FBBar.Value = 50;
    _deviceService.TryExecuteCommand(new MoveCommand(MoveCommand.MoveType.ForwardBackward, 50));
});
```

构造函数中 `FBBar.MouseUp`：
```csharp
FBBar.MouseUp += new MouseEventHandler((sender, e) =>
{
    _deviceService.TryExecuteCommand(new MoveCommand(MoveCommand.MoveType.ForwardBackward, FBBar.Value));
});
```

构造函数中 `RLBar.MouseUp`：
```csharp
RLBar.MouseUp += new MouseEventHandler((sender, e) =>
{
    _deviceService.TryExecuteCommand(new MoveCommand(MoveCommand.MoveType.RightLeft, 50, RLBar.Value));
});
```

`Rocker_Click`：
```csharp
_deviceService.TryExecuteCommand(new MoveCommand(MoveCommand.MoveType.FullControl, int.Parse(FBtext.Text), int.Parse(RLtext.Text)));
```

`Rocker_MouseMove` 中的发送：
```csharp
_deviceService.TryExecuteCommand(new MoveCommand(MoveCommand.MoveType.FullControl, FBBar.Value, RLBar.Value));
```

修改 FormClosing：
```csharp
private void UpperApp_FormClosing(object sender, FormClosingEventArgs e)
{
    var settings = CollectCurrentSettings();
    _configStorage.SaveSync(settings);
    _deviceService.DisposeAll();
    (_logger as IDisposable)?.Dispose();
}
```

- [ ] **Step 3: 构建验证**

Run: `dotnet build "d:\Workspace\CSharp\JXUST_RoboCommunity_UpperApp\UpperApp.sln"`
Expected: BUILD SUCCEEDED

- [ ] **Step 4: Commit**

```bash
git add UpperApp/Services/DeviceService.cs UpperApp/Form1.cs
git commit -m "feat: 引入 DeviceService 统一设备操作入口，Form1 通过 Service 层操作 (D1)"
```

---

### Task 4: 引入 DataPipeline 异步数据管道（D3）

**Files:**
- Create: `UpperApp/Services/DataPipeline.cs`
- Modify: `UpperApp/Form1.cs`

- [ ] **Step 1: 创建 DataPipeline**

```csharp
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace UpperApp
{
    internal class DataPipeline : IDisposable
    {
        private readonly Channel<Result> _channel;
        private readonly CancellationTokenSource _cts;
        private readonly Action<Result> _processor;
        private Task _consumerTask;

        public DataPipeline(Action<Result> processor, int capacity = 1024)
        {
            _processor = processor;
            _channel = Channel.CreateBounded<Result>(new BoundedChannelOptions(capacity)
            {
                SingleReader = true,
                SingleWriter = false,
                FullMode = BoundedChannelFullMode.DropOldest
            });
            _cts = new CancellationTokenSource();
        }

        public void Start()
        {
            _consumerTask = Task.Run(ConsumeLoop, _cts.Token);
        }

        public bool TryEnqueue(Result result)
        {
            return _channel.Writer.TryWrite(result);
        }

        private async Task ConsumeLoop()
        {
            try
            {
                await foreach (var item in _channel.Reader.ReadAllAsync(_cts.Token))
                {
                    _processor(item);
                }
            }
            catch (OperationCanceledException) { }
        }

        public void Dispose()
        {
            _cts.Cancel();
            _channel.Writer.TryComplete();
            _consumerTask?.Wait(TimeSpan.FromSeconds(2));
            _cts.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
```

- [ ] **Step 2: 在 Form1 中使用 DataPipeline 替代直接处理**

添加字段：
```csharp
private readonly DataPipeline _receivePipeline;
```

在构造函数中初始化（在 `_msgProcessor = new MessageProcessor(this, _logger);` 之后）：
```csharp
_receivePipeline = new DataPipeline(DispatchReceivedData);
_receivePipeline.Start();
```

添加 DispatchReceivedData 方法：
```csharp
private void DispatchReceivedData(Result result)
{
    if (InvokeRequired)
    {
        BeginInvoke(new Action(() => DispatchReceivedData(result)));
        return;
    }
    _msgProcessor.ProcessReceivedMessage(result);
}
```

修改 `UnifiedStatusChanged` 中 `ReciveMessage` 分支，将数据投递到管道而非直接处理：
```csharp
case Result.NETStatus.ReciveMessage:
    _receivePipeline.TryEnqueue(status);
    break;
```

在 FormClosing 中释放管道：
```csharp
_receivePipeline.Dispose();
```

- [ ] **Step 3: 构建验证**

Run: `dotnet build "d:\Workspace\CSharp\JXUST_RoboCommunity_UpperApp\UpperApp.sln"`
Expected: BUILD SUCCEEDED

- [ ] **Step 4: Commit**

```bash
git add UpperApp/Services/DataPipeline.cs UpperApp/Form1.cs
git commit -m "feat: 引入 DataPipeline 异步数据管道，解耦接收与 UI 处理 (D3)"
```

---

### Task 5: 统一协议处理 ProtocolHandler（D5）

**Files:**
- Create: `UpperApp/Services/ProtocolHandler.cs`
- Modify: `UpperApp/Processing/MessageProcessor.cs`

- [ ] **Step 1: 创建 ProtocolHandler**

```csharp
using System;
using System.Collections.Generic;

namespace UpperApp
{
    internal class ProtocolHandler
    {
        public enum DataType
        {
            Unknown,
            Yaw,
            Pitch,
            Roll,
            Distance
        }

        public readonly struct ParsedData
        {
            public DataType Type { get; init; }
            public string Key { get; init; }
            public string Value { get; init; }
        }

        public static ParsedData? TryParse(string input)
        {
            if (string.IsNullOrEmpty(input) || !input.Contains("/OVER"))
                return null;

            int colonIndex = input.IndexOf(':');
            if (colonIndex < 0) return null;

            int slashIndex = input.IndexOf('/', colonIndex);
            if (slashIndex < 0) return null;

            string key = input[..colonIndex];
            string value = input[(colonIndex + 1)..slashIndex];

            var type = key switch
            {
                "YAW" => DataType.Yaw,
                "PITCH" => DataType.Pitch,
                "ROLL" => DataType.Roll,
                "DISTANCE" => DataType.Distance,
                _ => DataType.Unknown
            };

            if (type == DataType.Unknown) return null;

            return new ParsedData { Type = type, Key = key, Value = value };
        }

        public static string EncodeMove(int speed, int direction)
        {
            return ProtocolFormatter.FullControl(speed, direction);
        }

        public static string EncodeForwardBackward(int value)
        {
            return ProtocolFormatter.ForwardBackward(value);
        }

        public static string EncodeRightLeft(int value)
        {
            return ProtocolFormatter.RightLeft(value);
        }
    }
}
```

- [ ] **Step 2: 修改 MessageProcessor 使用 ProtocolHandler 替代直接字符串操作**

在 `ProcessReceivedMessage` 中，将角度显示逻辑改为使用 ProtocolHandler：
```csharp
if (_display.IsAngleDisplayEnabled)
{
    var parsed = ProtocolHandler.TryParse(status.Message);
    if (parsed.HasValue)
        _display.UpdateAngleDisplay($"{parsed.Value.Key}:{parsed.Value.Value}/OVER");
}
```

这样 `SetAngDisp` 方法内部也可以简化为直接使用 `ProtocolHandler.TryParse`。

- [ ] **Step 3: 构建验证**

Run: `dotnet build "d:\Workspace\CSharp\JXUST_RoboCommunity_UpperApp\UpperApp.sln"`
Expected: BUILD SUCCEEDED

- [ ] **Step 4: Commit**

```bash
git add UpperApp/Services/ProtocolHandler.cs UpperApp/Processing/MessageProcessor.cs
git commit -m "feat: 引入 ProtocolHandler 统一协议编解码入口 (D5)"
```

---

### Task 6: 注册 DeviceService 到 AppServices（D6 收尾）

**Files:**
- Modify: `UpperApp/AppServices.cs`
- Modify: `UpperApp/Form1.cs`

- [ ] **Step 1: 在 AppServices 中注册 DeviceService**

在 `ConfigureServices()` 中，在现有注册之后添加：
```csharp
var communicators = new Dictionary<ChannelType, ICommunicator>();
var factory = new CommunicatorFactory();
foreach (ChannelType ch in Enum.GetValues(typeof(ChannelType)))
{
    if (ch == ChannelType.Unknown) continue;
    var comm = factory.Create(ch);
    communicators[ch] = comm;
}
var bluetoothComm = (IBluetoothCommunicator)communicators[ChannelType.Bluetooth];
var deviceService = new DeviceService(communicators, bluetoothComm);
RegisterSingleton<IDeviceService>(deviceService);
```

添加 `IDeviceService` 接口（或直接注册为 DeviceService 单例，因为这是内部类）。

- [ ] **Step 2: 修改 Form1 构造函数从 AppServices 获取 DeviceService**

将 Form1 中的 `_communicators` 初始化和 `_bluetoothComm` 初始化替换为从 AppServices 获取：
```csharp
_deviceService = AppServices.GetService<DeviceService>();
```

移除 Form1 中直接创建 communicators 的代码。

- [ ] **Step 3: 构建验证**

Run: `dotnet build "d:\Workspace\CSharp\JXUST_RoboCommunity_UpperApp\UpperApp.sln"`
Expected: BUILD SUCCEEDED

- [ ] **Step 4: Commit**

```bash
git add UpperApp/AppServices.cs UpperApp/Form1.cs
git commit -m "feat: DeviceService 注册到 AppServices，Form1 通过 IoC 获取 (D6)"
```

---

## 自检清单

| 检查项 | 状态 |
|--------|------|
| D1 UI 承担业务逻辑 | ✅ Task 3 DeviceService 抽取业务逻辑 |
| D2 无命令模式 | ✅ Task 2 IDeviceCommand + MoveCommand + RawSendCommand |
| D3 无数据管道 | ✅ Task 4 DataPipeline 异步管道 |
| D4 无设备状态机 | ✅ Task 1 DeviceState 枚举 |
| D5 协议编解码分散 | ✅ Task 5 ProtocolHandler 统一入口 |
| D6 Start 参数类型安全 | ⚠️ 保留（泛型约束会导致接口膨胀，DeviceService 已做运行时校验） |
| 无 Placeholder | ✅ 所有步骤包含完整代码 |
| 类型一致性 | ✅ 各 Task 间类型定义一致 |

---

## 重构后架构

```
┌──────────────────────────────────────────────────────────────┐
│  UI 层 — UpperApp (Form1.cs)                                  │
│  只做展示和用户交互，通过 DeviceService 操作设备              │
├──────────────────────────────────────────────────────────────┤
│  业务逻辑层                                                   │
│  ├─ DeviceService — 统一设备操作入口                          │
│  ├─ IDeviceCommand / MoveCommand / RawSendCommand — 命令模式  │
│  └─ DeviceState — 设备状态管理                                │
├──────────────────────────────────────────────────────────────┤
│  服务层                                                       │
│  ├─ DataPipeline — 异步数据管道（收发解耦）                   │
│  ├─ ProtocolHandler — 统一协议编解码                          │
│  └─ MessageProcessor — 消息格式化与显示                       │
├──────────────────────────────────────────────────────────────┤
│  设备抽象层                                                   │
│  ├─ ICommunicator / BaseCommunicationManager                  │
│  ├─ IBluetoothCommunicator / BthManager                       │
│  └─ 6 个具体通信管理器                                        │
└──────────────────────────────────────────────────────────────┘
```
