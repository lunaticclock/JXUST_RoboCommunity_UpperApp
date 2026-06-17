# JXUST RoboCommunity UpperApp — Code Wiki

> **项目名称**: 小车上位机 V7.0  
> **所属组织**: 江西理工大学 RoboCommunity（江理ClockSR）  
> **技术栈**: C# / .NET 10.0 / WPF (MVVM)  
> **目标平台**: Windows 10 19041+ (x64)

---

## 目录

1. [项目概述](#1-项目概述)
2. [项目整体架构](#2-项目整体架构)
3. [目录结构](#3-目录结构)
4. [主要模块职责](#4-主要模块职责)
5. [关键类与数据结构](#5-关键类与数据结构)
6. [依赖关系](#6-依赖关系)
7. [通信协议与数据格式](#7-通信协议与数据格式)
8. [项目构建与运行方式](#8-项目构建与运行方式)
9. [架构演进记录](#9-架构演进记录)

---

## 1. 项目概述

本项目是江西理工大学 RoboCommunity 的**机器人小车上位机控制软件**，用于通过多种通信协议（串口、TCP、UDP、蓝牙、CAN 总线、WebSocket）与下位机进行数据交互，实现以下核心功能：

- **多通道通信**：支持串口、TCP 服务端、UDP、蓝牙（SPP）、CAN 总线（PCAN）、WebSocket 六种通信方式
- **运动控制**：通过虚拟摇杆实时发送运动指令（速度、方向），50ms 节流避免高频 I/O
- **数据收发与显示**：接收下位机数据并以字符/十六进制模式展示，支持本地回显
- **Hex 模式原始字节发送**：Hex 模式下直接发送用户指定的字节序列，绕过字符编码
- **姿态显示**：解析并显示 YAW/PITCH/ROLL 角度及航程距离
- **批量字串发送**：预设最多 8 条消息，每条独立 HEX/ASCII 开关
- **行走路线绘制**：基于地图图片与距离标定，实时绘制小车行走轨迹
- **自动发送**：定时循环发送指定内容
- **配置持久化**：所有参数自动保存至 JSON 文件

---

## 2. 项目整体架构

项目采用 **WPF + MVVM + 分层架构 + Facade + 策略模式** 设计，整体分为五层：

```
┌──────────────────────────────────────────────────────────────┐
│                    视图层 (View Layer / WPF)                   │
│            MainWindow.xaml + UI/*.xaml (自定义控件)            │
│            纯 UI 绑定，无业务逻辑                               │
├──────────────────────────────────────────────────────────────┤
│                 视图模型层 (ViewModel Layer)                    │
│  ┌─────────────────────────────────────────────────────┐      │
│  │ MainViewModel (核心 VM)                              │      │
│  │  ├ 绑定属性 (RecvText/SendText/PeerList/...)         │      │
│  │  ├ 命令 (RelayCommand / AsyncRelayCommand)           │      │
│  │  ├ SendAndEcho / SendBytesAndEcho (统一发送入口)     │      │
│  │  ├ UnifiedStatusChanged (事件分发)                   │      │
│  │  ├ 摇杆节流定时器 (50ms dirty 标记)                  │      │
│  │  └ PresetMessageViewModel (批量字串子 VM)            │      │
│  ├─────────────────────────────────────────────────────┤      │
│  │ ViewModelBase (INotifyPropertyChanged)               │      │
│  │ RelayCommand / AsyncRelayCommand (ICommand 实现)     │      │
│  └─────────────────────────────────────────────────────┘      │
├──────────────────────────────────────────────────────────────┤
│                 业务服务层 (Service Layer)                      │
│  ┌─────────────────────────────────────────────────────┐      │
│  │ DeviceService (Facade)                               │      │
│  │  ├ 聚合 StatusChanged 事件 (Action<StatusEvent>)     │      │
│  │  ├ 通道状态查询 (IsChannelReady / IsAnyChannelReady) │      │
│  │  ├ 通道启停 (StartChannel / StopChannel)             │      │
│  │  ├ 命令执行 (TryExecuteCommand / TrySendBytes)       │      │
│  │  ├ 蓝牙代理 (懒加载，首次访问时创建)                 │      │
│  │  └ 生命周期 (DisposeAll)                             │      │
│  ├─────────────────────────────────────────────────────┤      │
│  │ DataPipeline (Channel<MessageReceivedEvent> 异步管道)│      │
│  │ ProtocolHandler (协议编解码)                          │      │
│  │ DeviceState (设备状态枚举)                             │      │
│  └─────────────────────────────────────────────────────┘      │
├──────────────────────────────────────────────────────────────┤
│              通信管理器层 (Strategy Pattern)                   │
│  ┌──────────┬──────────┬──────────┬────────┬──────────┐      │
│  │ Serial   │ TCP      │ UDP      │BthMgr  │CANManager│      │
│  │ Adapter  │ Adapter  │ Adapter  │        │WebSocket │      │
│  └──────────┴──────────┴──────────┴────────┴──────────┘      │
│  CommunicatorBase ← ICommunicator (含 Send(byte[]) 重载)      │
│  IBluetoothCommunicator ← BthManager (扩展接口)               │
│  CommunicatorFactory ← ICommunicatorFactory                   │
│  StatusEvent 多态事件层次 (替代原 Result 万能 record)          │
├──────────────────────────────────────────────────────────────┤
│           基础设施层 (IoC / Config / Utils / Processing)       │
│  AppServices │ IConfigStorage / JsonFileConfigStorage          │
│  ILogger / FileLogger │ MessageProcessor │ ProcessedMessage   │
│  MapTracker │ CommunicationParams │ StatusEvent │ Utils        │
│  IDeviceCommand / MoveCommand / RawSendCommand                │
└──────────────────────────────────────────────────────────────┘
```

**核心设计模式**：

| 模式 | 应用位置 | 说明 |
|------|---------|------|
| MVVM | `MainViewModel` + `MainWindow.xaml` | WPF 数据绑定驱动 UI，View 无业务逻辑 |
| 外观模式 (Facade) | `DeviceService` | 统一设备操作入口，VM 层仅与 DeviceService 交互 |
| 策略模式 (Strategy) | `ICommunicator` + 各 Adapter | 统一通信接口，运行时切换通道 |
| 命令模式 (Command) | `IDeviceCommand` / `MoveCommand` / `RawSendCommand` + `RelayCommand` | 封装发送指令与 UI 操作 |
| 工厂模式 (Factory) | `ICommunicatorFactory` / `CommunicatorFactory` | 根据 `ChannelType` 创建对应管理器 |
| 服务定位器 (Service Locator) | `AppServices` | 轻量级 IoC 容器，管理单例与工厂注册 |
| 观察者模式 (Observer) | `DeviceService.StatusChanged` 事件 | 聚合所有通信状态变化通知 VM |
| 模板方法 (Template Method) | `CommunicatorBase` | 抽象基类定义状态管理骨架，子类实现细节 |
| 多态事件 (Polymorphic Event) | `StatusEvent` 层次 | 替代万能 record，编译器可检查穷尽性 |
| 节流模式 (Throttling) | `_rockerSendTimer` + `_rockerDirty` | 摇杆高频输入批量发送，避免 UI 线程阻塞 |

---

## 3. 目录结构

```
JXUST_RoboCommunity_UpperApp/
├── UpperApp.sln                              # Visual Studio 解决方案文件
├── CODE_WIKI.md                              # 代码 Wiki 文档（本文件）
├── communication-flow.html                   # 通信流程可视化 HTML
└── UpperApp/                                 # 唯一项目目录
    ├── UpperApp.csproj                       # 项目配置文件（WPF, net10.0-windows）
    ├── App.xaml / App.xaml.cs                # WPF 应用入口（注册服务）
    ├── MainWindow.xaml / .cs                 # 主窗体（UI 布局 + 代码后台）
    ├── App.config                            # 应用运行时配置
    ├── jxust.ico                             # 应用图标
    │
    ├── Core/                                 # 核心类型与数据结构
    │   ├── ChannelType.cs                    # 通信通道类型枚举
    │   ├── StatusEvent.cs                    # 多态通信状态事件层次（替代 Result）
    │   ├── AppSettings.cs                    # 应用配置模型
    │   ├── BindingDic.cs                     # 线程安全可观察字典
    │   ├── ProtocolFormatter.cs              # 通信协议格式化
    │   └── Utils.cs                          # 静态工具方法（含 Hex 转换）
    │
    ├── Commands/                             # 命令模式
    │   ├── IDeviceCommand.cs                 # 设备命令接口
    │   ├── MoveCommand.cs                    # 运动控制命令
    │   └── RawSendCommand.cs                 # 原始数据发送命令
    │
    ├── Communication/                        # 通信管理器
    │   ├── ICommunicator.cs                  # 通信管理器统一接口（含 Send(byte[])）
    │   ├── ICommunicatorFactory.cs           # 通信管理器工厂接口与实现
    │   ├── IBluetoothCommunicator.cs         # 蓝牙扩展接口
    │   ├── CommunicatorBase.cs               # 通信管理器抽象基类（含 NotifyX 工厂方法）
    │   ├── CommunicationParams.cs            # 各通道参数模型
    │   ├── TouchSocketSerialAdapter.cs       # 串口通信（基于 TouchSocket）
    │   ├── TouchSocketTcpAdapter.cs          # TCP 服务端通信（基于 TouchSocket）
    │   ├── TouchSocketUdpAdapter.cs          # UDP 通信（基于 TouchSocket）
    │   ├── BthManager.cs                     # 蓝牙通信管理器
    │   ├── CanManager.cs                     # CAN 总线通信管理器
    │   └── WebSocketManager.cs               # WebSocket 通信管理器
    │
    ├── Processing/                           # 消息处理与日志
    │   ├── ILogger.cs                        # 日志接口
    │   ├── FileLogger.cs                     # 文件日志实现（线程安全 lock）
    │   ├── MessageProcessor.cs               # 消息处理逻辑（日志开关注入）
    │   └── ProcessedMessage.cs               # 处理后的消息数据结构
    │
    ├── Services/                             # 业务服务层
    │   ├── AppServices.cs                    # 轻量级 IoC 服务定位器
    │   ├── DeviceService.cs                  # 设备服务 Facade（统一入口）
    │   ├── DeviceState.cs                    # 设备状态枚举
    │   ├── DataPipeline.cs                   # 异步数据管道（Channel<MessageReceivedEvent>）
    │   ├── ProtocolHandler.cs                # 协议编解码统一入口
    │   └── IConfigStorage.cs                 # 配置存储接口与 JSON 实现
    │
    ├── ViewModels/                           # 视图模型层（MVVM）
    │   ├── MainViewModel.cs                  # 主视图模型（核心业务逻辑）
    │   ├── ViewModelBase.cs                  # VM 基类（INotifyPropertyChanged）
    │   └── RelayCommand.cs                   # ICommand 实现（含 AsyncRelayCommand）
    │
    ├── UI/                                   # UI 自定义控件与辅助组件
    │   ├── JoystickControl.cs                # 虚拟摇杆控件（节流优化）
    │   ├── MapTracker.cs                     # 地图轨迹绘制逻辑
    │   ├── ILogSink.cs                       # 日志接收接口
    │   ├── MetricItem.xaml/.cs               # 指标显示控件
    │   ├── MetricLabel.xaml/.cs              # 指标标签控件
    │   ├── PanelHeader.xaml/.cs              # 面板标题栏控件
    │   └── SendRow.xaml/.cs                  # 发送行控件
    │
    ├── Themes/
    │   └── DarkTheme.xaml                    # 深色主题样式定义
    │
    └── Properties/
        ├── app.manifest                      # UAC 清单（asInvoker）
        ├── PublishProfiles/
        │   └── FolderProfile.pubxml          # 发布配置
        ├── Resources.Designer.cs             # 资源自动生成代码
        ├── Resources.resx                    # 资源文件
        ├── Settings.Designer.cs              # 设置自动生成代码
        └── Settings.settings                 # 用户设置
```

---

## 4. 主要模块职责

### 4.1 程序入口与服务配置

#### `App.xaml.cs`

WPF 应用入口，执行以下操作：
1. 调用 `AppServices.ConfigureServices()` 注册全局服务
2. 由 WPF 框架根据 `App.xaml` 的 `StartupUri` 加载 `MainWindow`

#### `AppServices.cs`

轻量级**服务定位器**（手动 IoC），提供两种注册方式：

| 方法 | 说明 |
|------|------|
| `RegisterSingleton<TInterface>(TInterface instance)` | 注册单例服务 |
| `RegisterTransient<TInterface>(Func<TInterface> factory)` | 注册工厂服务（每次返回新实例） |
| `GetService<TInterface>()` | 获取服务实例 |

在 `ConfigureServices()` 中注册了：
- `ICommunicatorFactory` → `CommunicatorFactory`（单例）
- `IConfigStorage` → `JsonFileConfigStorage`（单例）
- `DeviceService`（单例，蓝牙通信器懒加载，不在启动时创建）

---

### 4.2 视图模型层（MVVM）

#### `MainViewModel` — 核心视图模型

`MainViewModel` 是整个应用的**业务协调中心**，通过 `DeviceService` 与通信层交互，不直接访问任何 `ICommunicator`。

**核心字段**：

| 字段 | 类型 | 说明 |
|------|------|------|
| `_deviceService` | `DeviceService` | 设备服务 Facade（唯一通信入口） |
| `_receivePipeline` | `DataPipeline` | 异步数据接收管道（`Channel<MessageReceivedEvent>`） |
| `_configStorage` | `IConfigStorage` | 配置存储服务 |
| `_logger` | `FileLogger` | 文件日志服务（线程安全） |
| `_msgProcessor` | `MessageProcessor` | 消息处理器 |
| `_mapTracker` | `MapTracker` | 地图轨迹追踪器 |
| `_sendTimer` | `DispatcherTimer` | 自动发送定时器 |
| `_rockerSendTimer` | `DispatcherTimer` | 摇杆发送节流定时器（50ms） |
| `_rockerDirty` | `volatile bool` | 摇杆值变化脏标记 |

**统一发送入口**（避免事件回流冗余）：

| 方法 | 说明 |
|------|------|
| `SendAndEcho(IDeviceCommand, string)` | 字符模式：执行命令发送 + 回显 + 计数 + 日志 |
| `SendBytesAndEcho(byte[], string)` | Hex 模式：发送原始字节 + 回显 hex 串 + 计数 + 日志 |
| `StrSend(string, bool?)` | 发送分流入口，根据 hex 标志选择上述两者 |

**事件分发**：

| 方法 | 说明 |
|------|------|
| `UnifiedStatusChanged(StatusEvent)` | 接收 `DeviceService.StatusChanged` 事件，按 `StatusEvent` 子类模式匹配分发 |
| `DispatchReceivedData(MessageReceivedEvent)` | 后台线程处理接收数据（经 DataPipeline 消费） |

**摇杆节流机制**：
- `OnSliderChanged` 只设 `_rockerDirty = true`，不直接发送
- `_rockerSendTimer` 每 50ms 检查 dirty 标记，有变化才发送
- 避免 MouseMove 高频触发同步 I/O 阻塞 UI 线程

#### `PresetMessageViewModel` — 批量字串子 VM

每条预设消息独立持有 `IsHex` 开关，通过 `Action<string, bool>` 回调到 `MainViewModel.StrSend`。

#### `RelayCommand` / `AsyncRelayCommand`

`ICommand` 实现，支持同步/异步命令绑定。

---

### 4.3 设备服务层（Facade）

#### `DeviceService` — 统一设备操作入口

`DeviceService` 是 VM 层与通信管理器之间的**唯一中介**。

**事件**：

| 事件 | 说明 |
|------|------|
| `StatusChanged` | `event Action<StatusEvent>`，聚合所有通道的状态变化事件 |

**通道状态查询**：

| 方法/属性 | 说明 |
|------|------|
| `GetChannelState(ChannelType)` | 获取指定通道状态 |
| `IsChannelReady(ChannelType)` | 指定通道是否已连接 |
| `IsAnyChannelReady()` | 是否有任意通道已连接 |
| `ActiveChannel` | 当前活跃发送通道 |

**通道操作**：

| 方法 | 说明 |
|------|------|
| `StartChannel(ChannelType, CommunicationParams)` | 启动指定通道（懒加载创建管理器） |
| `StopChannel(ChannelType)` | 停止指定通道 |
| `GetPeerList(ChannelType)` | 获取指定通道的对端列表 |

**命令执行**：

| 方法 | 说明 |
|------|------|
| `TryExecuteCommand(IDeviceCommand)` | 尝试执行命令（字符模式，返回 bool） |
| `TrySendBytes(byte[], ChannelType?)` | 直接发送原始字节（Hex 模式，绕过编码） |
| `SetTarget(string)` | 设置 TCP/UDP 目标 |
| `SetBluetoothTarget(string)` | 设置蓝牙目标 |

**蓝牙代理**（懒加载）：

| 方法/属性 | 说明 |
|------|------|
| `GetOrCreateBluetooth()` | 懒加载蓝牙通信器（首次访问时创建） |
| `IsBluetoothRadioAvailable` | 蓝牙适配器是否可用 |
| `IsBluetoothRadioPoweredOn` | 蓝牙是否已开启 |
| `BluetoothRadioAddress` / `BluetoothRadioMode` | 适配器信息 |
| `ConnectBluetoothDevice(string)` | 连接蓝牙设备 |
| `DisconnectBluetoothClient()` | 断开蓝牙客户端 |
| `DiscoverBluetoothDevicesAsync()` | 扫描蓝牙设备 |

**生命周期**：

| 方法 | 说明 |
|------|------|
| `DisposeAll()` | 停止并释放所有通道资源 |

#### `DeviceState` — 设备状态枚举

```
Disconnected  → 未连接
Connecting    → 连接中
Connected     → 已连接
Disconnecting → 断开中
Error         → 异常状态
```

#### `DataPipeline` — 异步数据管道

基于 `System.Threading.Channels` 的有界异步管道，解耦数据接收与 UI 处理：

| 成员 | 说明 |
|------|------|
| `Start()` | 启动后台消费循环 |
| `TryEnqueue(MessageReceivedEvent)` | 非阻塞入队（满时丢弃最旧，记录丢弃计数） |
| `DroppedCount` | 累计丢弃数据条数 |
| `Dispose()` | 停止管道并释放资源 |

通道类型为 `Channel<MessageReceivedEvent>`，编译期保证只有接收事件入队。

#### `ProtocolHandler` — 协议编解码统一入口

| 方法 | 说明 |
|------|------|
| `TryParse(string)` | 尝试解析协议字符串，返回 `ParsedData?` |
| `EncodeMove(...)` | 编码运动指令 |
| `EncodeForwardBackward(int)` | 编码前后速度指令 |
| `EncodeRightLeft(int)` | 编码左右方向指令 |

---

### 4.4 命令模式

#### `IDeviceCommand` — 设备命令接口

| 属性 | 说明 |
|------|------|
| `Name` | 命令名称 |
| `TargetChannel` | 目标通道 |
| `Encode()` | 编码为发送字符串 |

#### `MoveCommand` — 运动控制命令

| MoveType | 说明 |
|----------|------|
| `ForwardBackward` | 前后速度控制 |
| `RightLeft` | 左右方向控制 |
| `FullControl` | 组合控制（速度+方向） |

#### `RawSendCommand` — 原始数据发送命令

直接透传用户输入的字符串数据，指定目标通道。

---

### 4.5 通信管理器体系

通信管理器是本项目的核心，采用**策略模式 + 模板方法**实现多通道统一管理。

#### 接口层

**`ICommunicator`** — 通信管理器统一接口

| 成员 | 类型 | 说明 |
|------|------|------|
| `StatusChanged` | `event Action<StatusEvent>` | 状态变化事件（多态事件） |
| `Channel` | `ChannelType` | 通信通道类型 |
| `State` | `DeviceState` | 当前设备状态 |
| `Start(CommunicationParams)` | `void` | 启动通信 |
| `Stop()` | `void` | 停止通信，释放资源 |
| `Send(string, string)` | `void` | 发送字符串数据（经字符编码） |
| `Send(byte[], string)` | `void` | 发送原始字节（Hex 模式，绕过编码） |
| `GetPeerList()` | `IReadOnlyList<string>` | 获取当前连接对端列表 |

**`IBluetoothCommunicator`** — 蓝牙扩展接口（继承 ICommunicator）

| 成员 | 说明 |
|------|------|
| `IsRadioAvailable` | 蓝牙适配器是否可用 |
| `IsRadioPoweredOn` | 蓝牙是否已开启 |
| `RadioAddress` / `RadioMode` | 适配器地址和模式 |
| `DiscoverDevicesAsync()` | 扫描蓝牙设备 |
| `ConnectToDevice(string)` | 主动连接指定设备 |
| `DisconnectClient()` | 断开客户端连接 |

**`ICommunicatorFactory`** — 工厂接口

| 方法 | 说明 |
|------|------|
| `Create(ChannelType)` | 根据通道类型创建对应 `ICommunicator` 实例 |

#### 抽象基类

**`CommunicatorBase`** — 所有通信管理器的公共基类

| 成员 | 说明 |
|------|------|
| `State` | `DeviceState` 设备状态属性 |
| `IsStopping` | 停止重入保护标志 |
| `BeginStop()` / `EndStop()` | 停止流程状态管理 |
| `NotifyX(...)` 工厂方法 | 9 个状态上报工厂方法（见下表） |
| `DisposeAsync()` | 默认调用 Stop() |

**状态上报工厂方法**（替代手动构造 StatusEvent）：

| 方法 | 说明 |
|------|------|
| `NotifyMessageReceived(...)` | 上报接收到数据 |
| `NotifyMessageSent(...)` | 上报发送成功 |
| `NotifyMessageSendError(...)` | 上报发送失败 |
| `NotifyMessageSendAlert(...)` | 上报发送告警（如空数据） |
| `NotifyPeerConnected(...)` | 上报新对端连接 |
| `NotifyPeerDisconnected(...)` | 上报对端断开 |
| `NotifyMonitorStarted(...)` | 上报监听启动（State→Connected） |
| `NotifyMonitorStopped(...)` | 上报监听停止 |
| `NotifyException(...)` | 上报异常（State→Error） |
| `NotifyManualStopped()` | 上报手动停止 |

工厂方法自动注入 `Channel`，子类无需手动传 Channel 参数。

#### 具体实现

| 类 | 通道 | 关键特性 |
|----|------|---------|
| **`TouchSocketSerialAdapter`** | Serial | 基于 TouchSocket.SerialPorts；GB2312 编码；`Send(byte[])` 直写字节 |
| **`TouchSocketTcpAdapter`** | TCP | 基于 TouchSocket；多客户端管理；GB2312 编码 |
| **`TouchSocketUdpAdapter`** | UDP | 基于 TouchSocket；对端列表管理；GB2312 编码 |
| **`BthManager`** | Bluetooth | `InTheHand.Net.Bluetooth`；服务端/客户端双模式；UTF-8 |
| **`CanManager`** | CAN | `Peak.PCANBasic.NET`；500kbps；`ID:HexData` 格式 |
| **`WebSocketManager`** | WebSocket | `HttpListener` 服务端 + `ClientWebSocket` 客户端；UTF-8 |

---

### 4.6 消息处理层

#### `ILogger` / `FileLogger` — 日志接口与实现

| 方法 | 说明 |
|------|------|
| `WriteLine(string)` | 写入一行日志（线程安全 lock） |
| `Open(string)` | 打开日志文件 |
| `Close()` | 关闭日志文件 |
| `IsOpen` | 日志文件是否已打开 |

#### `MessageProcessor` — 消息处理器

依赖 `ILogger` + `Func<bool>`（日志开关），将消息处理逻辑从 VM 中解耦。

| 方法 | 说明 |
|------|------|
| `ProcessReceivedMessage(MessageReceivedEvent)` | 处理接收消息：协议解析 → 格式化 → 日志（受开关控制） |

返回 `ProcessedMessage` 对象，包含前缀、格式化内容、姿态数据等。

#### `ProcessedMessage` — 处理后的消息数据结构

| 属性 | 说明 |
|------|------|
| `Prefix` | 来源前缀（如 "from 192.168.1.1:1234:\r\n"） |
| `FormattedContent` | 格式化后的内容（确保 \r\n 结尾） |
| `RawContent` | 原始内容 |
| `Source` | 数据来源标识 |
| `ByteCount` | 字节计数 |
| `NewPeerHint` | 新对端提示 |
| `HasAttitudeData` | 是否包含姿态数据 |
| `AttitudeRaw` | 姿态数据原始字符串 |

---

### 4.7 配置持久化层

#### `IConfigStorage` / `JsonFileConfigStorage`

| 方法 | 说明 |
|------|------|
| `LoadAsync()` | 从 `settings.json` 异步加载配置 |
| `SaveAsync(AppSettings)` | 异步保存配置 |
| `SaveSync(AppSettings)` | 同步保存配置（用于窗体关闭时） |

---

### 4.8 UI 辅助组件

#### `MapTracker` — 地图轨迹绘制

| 方法 | 说明 |
|------|------|
| `SetCalibratedDistance(float)` | 设置标定距离 |
| `SetAspectRatio(float)` | 设置宽高比 |
| `OnDistanceChanged(...)` | 距离变化时绘制轨迹点 |
| `OnMapClick(...)` | 点击地图设置锚点 |
| `OnMouseMove(Point)` | 鼠标移动时绘制预览线 |
| `Clear()` | 清除轨迹 |

#### `JoystickControl` — 虚拟摇杆控件

| 特性 | 说明 |
|------|------|
| `Speed` / `Direction` | 双向绑定属性（0-100） |
| 节流优化 | MouseMove 中只在值变化时才设属性，避免冗余通知 |
| 值变化检测 | `if (Speed != newSpeed) Speed = newSpeed;` |

---

## 5. 关键类与数据结构

### 5.1 `StatusEvent` — 多态通信状态事件层次

替代原 `Result` 万能 record，每个子类对应一种语义明确的事件：

```
StatusEvent (abstract record)
├── MessageReceivedEvent      # 接收到数据
│   ├── Content, ByteCount, Source, PeerHint
├── MessageSentEvent          # 发送结果
│   ├── Content, ByteCount, Target, Result(Success/Error/Alert)
├── PeerConnectedEvent        # 新对端连接
│   ├── Peer, Message
├── PeerDisconnectedEvent     # 对端断开
│   ├── Reason, Peer
├── MonitorStartedEvent       # 监听/连接已启动
│   └── Message
├── MonitorStoppedEvent       # 监听/连接已停止
│   └── Message
├── ExceptionOccurredEvent    # 通信异常
│   ├── Message, RemoteIP
└── ManualStoppedEvent        # 手动停止
```

**优势**：
- 编译器可检查 switch 穷尽性
- 类型安全，避免运行时类型判断
- 每个子类字段语义明确，消除原 Result 的死代码字段

### 5.2 `CommunicationParams` — 通道参数体系

```
CommunicationParams (abstract)
├── SerialParams        # PortName, BaudRate, Parity, DataBits, StopBits
├── TcpServerParams     # LocalIP, Port
├── UdpParams           # LocalIP, Port
├── BluetoothParams     # IsServerMode, TargetDeviceName
├── CanParams           # ChannelName
└── WebSocketParams     # IsServerMode, Url
```

### 5.3 `DeviceState` — 设备状态枚举

```
Disconnected  → 未连接（初始状态）
Connecting    → 连接中
Connected     → 已连接
Disconnecting → 断开中
Error         → 异常状态
```

状态转换由 `CommunicatorBase` 内部管理（`BeginStop`/`EndStop`/`NotifyMonitorStarted`/`NotifyException`），外部通过 `ICommunicator.State` 只读访问。

### 5.4 `Utils` — 静态工具方法

| 方法 | 说明 |
|------|------|
| `BytesToHexString(byte[])` | 字节序列 → 空格分隔 hex 字符串（不经编码） |
| `ParseHexString(string)` | hex 字符串 → 原始字节数组（不经编码） |
| `HexStringToString(string)` | hex 字符串 → 字符串（经 GB2312 解码） |
| `StringToHexString(string)` | 字符串 → hex 字符串（经 GB2312 编码） |
| `GetTime()` | 当前时间字符串 |
| `GetLocalIPv4Addresses()` | 本机 IPv4 地址列表 |

> **注意**：`Utils` 的静态构造函数先注册 `CodePagesEncodingProvider`，再创建 GB2312 编码实例，避免字段初始化顺序问题。

---

## 6. 依赖关系

### 6.1 NuGet 包依赖

| 包名 | 版本 | 用途 |
|------|------|------|
| `InTheHand.Net.Bluetooth` | 4.2.4 | 蓝牙 SPP 通信 |
| `Peak.PCANBasic.NET` | 5.0.1.1131 | PEAK PCAN USB CAN 总线通信 |
| `System.IO.Ports` | 10.0.6 | 串口通信 |
| `TouchSocket` | 4.2.11 | TCP/UDP 通信框架 |
| `TouchSocket.SerialPorts` | 4.2.11 | 串口通信 TouchSocket 适配 |
| `System.Data.DataSetExtensions` | 4.6.0-preview3 | DataSet 扩展 |

### 6.2 模块间依赖关系图

```
App.xaml.cs
  └→ AppServices.ConfigureServices()
       ├→ CommunicatorFactory (ICommunicatorFactory 单例)
       ├→ JsonFileConfigStorage (IConfigStorage 单例)
       └→ DeviceService (单例)
            └→ Dictionary<ChannelType, ICommunicator> (懒加载缓存)

MainViewModel
  ├→ AppServices.GetService<DeviceService>()
  │     └→ DeviceService.StatusChanged (Action<StatusEvent> 聚合事件)
  │     └→ DeviceService.StartChannel / StopChannel / IsChannelReady / ...
  │     └→ DeviceService.TryExecuteCommand(IDeviceCommand)
  │     └→ DeviceService.TrySendBytes(byte[])  (Hex 模式)
  ├→ AppServices.GetService<IConfigStorage>()
  ├→ DataPipeline(DispatchReceivedData)  (Channel<MessageReceivedEvent>)
  ├→ MessageProcessor(_logger, () => SaveDataEnabled)
  ├→ MapTracker
  └→ ProtocolHandler (静态方法)

各 Adapter
  ├→ CommunicatorBase (继承，调用 NotifyX 工厂方法)
  ├→ ICommunicator (实现，含 Send(byte[]))
  ├→ CommunicationParams (参数类型)
  ├→ StatusEvent (多态事件数据)
  └→ BindingDic<T> (对端管理)
```

---

## 7. 通信协议与数据格式

### 7.1 发送指令格式

| 指令 | 格式 | 说明 |
|------|------|------|
| 前后速度 | `FB:{value}:OVER\r\n` | value: 0-100, 50=停止 |
| 左右方向 | `RL:{value}:OVER\r\n` | value: 0-100, 50=居中 |
| 组合控制 | `FR:{speed}:{direction}:OVER\r\n` | 同时设置速度与方向 |
| CAN 数据 | `{ID}:{HexData}` | ID 为十六进制 |
| Hex 模式 | 原始字节 | 用户输入 hex 串直接解析为字节发送，不经字符编码 |

### 7.2 接收数据格式

```
YAW:{value}/OVER
ROLL:{value}/OVER
PITCH:{value}/OVER
DISTANCE:{value}/OVER
```

### 7.3 编码约定

| 通道 | 字符模式编码 | Hex 模式 |
|------|------------|---------|
| 串口 / TCP / UDP | GB2312 | 原始字节直发 |
| 蓝牙 / WebSocket | UTF-8 | 原始字节直发 |
| CAN | 十六进制原始字节 | 不支持（需 ID:数据 格式） |

### 7.4 Hex 模式发送流程

```
用户输入 "FF 00"
  → Utils.ParseHexString("FF 00")  → byte[] { 0xFF, 0x00 }
  → DeviceService.TrySendBytes(bytes)
  → ICommunicator.Send(byte[], target)  (绕过字符编码)
  → Adapter 直接写入底层流
  → 回显原始 hex 串 "FF 00"
```

---

## 8. 项目构建与运行方式

### 8.1 环境要求

| 项目 | 要求 |
|------|------|
| 操作系统 | Windows 10 19041+ |
| .NET SDK | .NET 10.0 |
| IDE | Visual Studio 2022 v18.6+ |
| 运行时权限 | 普通用户权限（无需管理员） |
| 硬件（可选） | PEAK PCAN USB 适配器、蓝牙适配器 |

### 8.2 构建命令

```bash
dotnet restore UpperApp.sln
dotnet build UpperApp.sln --configuration Debug
dotnet build UpperApp.sln --configuration Release
dotnet publish UpperApp/UpperApp.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

### 8.3 运行方式

```bash
dotnet run --project UpperApp/UpperApp.csproj
```

---

## 9. 架构演进记录

### V7.0 → 当前版本的主要重构

#### 9.1 UI 框架迁移：WinForms → WPF + MVVM

- `Form1.cs` → `MainWindow.xaml` + `MainViewModel.cs`
- 事件驱动 → 数据绑定驱动
- UI 逻辑与业务逻辑彻底分离

#### 9.2 通信库迁移：原生 Socket → TouchSocket

- `SerManager` → `TouchSocketSerialAdapter`
- `TCPManager` → `TouchSocketTcpAdapter`
- `UDPManager` → `TouchSocketUdpAdapter`
- 基类 `BaseCommunicationManager` → `CommunicatorBase`

#### 9.3 事件类型重构：Result 万能 record → StatusEvent 多态层次

- 原 `Result` record 承载 8 种语义，60+ 调用点手动构造
- 新 `StatusEvent` 抽象基类 + 8 个子类，编译器检查穷尽性
- `CommunicatorBase` 提供 9 个 `NotifyX` 工厂方法，Channel 自动注入

#### 9.4 冗余优化（R1-R10）

| 编号 | 优化内容 |
|------|---------|
| R1 | 接收流程去除双重 Dispatcher 切换，I/O 线程直接入队 |
| R2 | 发送流程去除事件回流回显，VM 调用 SendAndEcho 直接回显 |
| R3 | 摇杆双属性合并更新，减少冗余通知 |
| R4 | Result 对象职责过载（已通过 StatusEvent 重构解决） |
| R5+R6 | DeviceService 职责拆分，统一接口引用 |
| R7 | 去除日志重复写入 |
| R8 | 统一姿态数据解析（ProtocolHandler.TryParse） |
| R9 | 提取 RefreshPeerList 方法 |
| R10 | 蓝牙懒加载，启动时不初始化蓝牙栈 |

#### 9.5 Hex 模式原始字节发送

- 新增 `ICommunicator.Send(byte[], string)` 接口
- 6 个 Adapter 各自实现原始字节发送
- `DeviceService.TrySendBytes(byte[])` 统一入口
- `Utils.ParseHexString` / `BytesToHexString` 直接操作 byte[]，不经编码
- `MainViewModel.SendBytesAndEcho` 处理 Hex 模式发送+回显

#### 9.6 摇杆节流优化

- MouseMove 高频触发（30-60 次/秒）→ 同步 I/O 阻塞 UI 线程 → 内存飙升
- 修复：50ms `DispatcherTimer` + `_rockerDirty` 标记，批量发送最新值
- `JoystickControl` 只在值变化时才设属性

#### 9.7 编码注册修复

- `Utils` 静态字段初始化顺序问题导致 GB2312 编码获取失败
- 修复：`RegisterProvider` 移到静态构造函数体最前，字段改为构造函数内赋值

---

> **文档版本**: 4.0  
> **更新日期**: 2026-06-17  
> **对应项目版本**: V7.0 (WPF + MVVM 重构后)
