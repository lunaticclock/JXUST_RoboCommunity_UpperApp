# JXUST RoboCommunity UpperApp — Code Wiki

> **项目名称**: 小车上位机 V7.0  
> **所属组织**: 江西理工大学 RoboCommunity（江理ClockSR）  
> **技术栈**: C# / .NET 10.0 / WinForms  
> **目标平台**: Windows 10 19041+ (x64)

---

## 目录

1. [项目概述](#1-项目概述)
2. [项目整体架构](#2-项目整体架构)
3. [目录结构](#3-目录结构)
4. [主要模块职责](#4-主要模块职责)
5. [关键类与函数说明](#5-关键类与函数说明)
6. [依赖关系](#6-依赖关系)
7. [通信协议与数据格式](#7-通信协议与数据格式)
8. [项目构建与运行方式](#8-项目构建与运行方式)

---

## 1. 项目概述

本项目是江西理工大学 RoboCommunity 的**机器人小车上位机控制软件**，用于通过多种通信协议（串口、TCP、UDP、蓝牙、CAN 总线、WebSocket）与下位机进行数据交互，实现以下核心功能：

- **多通道通信**：支持串口、TCP 服务端、UDP、蓝牙（SPP）、CAN 总线（PCAN）、WebSocket 六种通信方式
- **运动控制**：通过滑块/摇杆实时发送运动指令（速度、方向），支持按键步进调节
- **数据收发与显示**：接收下位机数据并以字符/十六进制模式展示，支持本地回显
- **姿态显示**：解析并显示 YAW/PITCH/ROLL 角度及航程距离
- **批量字串发送**：预设最多 8 条消息，支持 HEX/ASCII 模式一键发送
- **行走路线绘制**：基于地图图片与距离标定，实时绘制小车行走轨迹
- **自动发送**：定时循环发送指定内容
- **配置持久化**：所有参数（串口、网络、蓝牙、UI 偏好等）自动保存至 JSON 文件

---

## 2. 项目整体架构

项目采用**分层架构 + Facade + 策略模式**设计，整体分为四层：

```
┌──────────────────────────────────────────────────────────────┐
│                    UI 层 (WinForms)                           │
│            UpperApp (Form1.cs) : IDisplayAdapter             │
│            仅通过 DeviceService 交互，不直接访问通信管理器      │
├──────────────────────────────────────────────────────────────┤
│                 业务逻辑层 (Service Layer)                     │
│  ┌─────────────────────────────────────────────────────┐      │
│  │ DeviceService (Facade)                               │      │
│  │  ├ 聚合 StatusChanged 事件                           │      │
│  │  ├ 通道状态查询 (IsChannelReady / IsAnyChannelReady) │      │
│  │  ├ 通道启停 (StartChannel / StopChannel)             │      │
│  │  ├ 命令执行 (ExecuteCommand / TryExecuteCommand)     │      │
│  │  ├ 蓝牙代理 (StartBluetooth / SendBluetooth / ...)   │      │
│  │  └ 生命周期 (StopAll / DisposeAll)                   │      │
│  ├─────────────────────────────────────────────────────┤      │
│  │ DataPipeline (异步数据管道)                           │      │
│  │ ProtocolHandler (协议编解码)                          │      │
│  │ DeviceState (设备状态枚举)                             │      │
│  └─────────────────────────────────────────────────────┘      │
├──────────────────────────────────────────────────────────────┤
│              通信管理器层 (Strategy Pattern)                   │
│  ┌──────────┬──────────┬──────────┬────────┬──────────┐      │
│  │SerManager│TCPManager│UDPManager│BthMgr  │CANManager│      │
│  │          │          │          │        │WebSocket │      │
│  └──────────┴──────────┴──────────┴────────┴──────────┘      │
│  BaseCommunicationManager ← ICommunicator                     │
│  IBluetoothCommunicator ← BthManager (扩展接口)               │
│  CommunicatorFactory ← ICommunicatorFactory                   │
├──────────────────────────────────────────────────────────────┤
│           基础设施层 (IoC / Config / Utils)                    │
│  AppServices │ IConfigStorage / JsonFileConfigStorage          │
│  ILogger / FileLogger │ ProtocolFormatter / ProtocolParser    │
│  MapTracker │ CommunicationParams │ Result (record) │ Utils   │
│  IDeviceCommand / MoveCommand / RawSendCommand                │
└──────────────────────────────────────────────────────────────┘
```

**核心设计模式**：

| 模式 | 应用位置 | 说明 |
|------|---------|------|
| 外观模式 (Facade) | `DeviceService` | 统一设备操作入口，UI 层仅与 DeviceService 交互 |
| 策略模式 (Strategy) | `ICommunicator` + 各 `*Manager` | 统一通信接口，运行时切换通道 |
| 命令模式 (Command) | `IDeviceCommand` / `MoveCommand` / `RawSendCommand` | 封装发送指令为对象 |
| 工厂模式 (Factory) | `ICommunicatorFactory` / `CommunicatorFactory` | 根据 `ChannelType` 创建对应管理器 |
| 服务定位器 (Service Locator) | `AppServices` | 轻量级 IoC 容器，管理单例与工厂注册 |
| 观察者模式 (Observer) | `DeviceService.StatusChanged` 事件 | 聚合所有通信状态变化通知 UI |
| 模板方法 (Template Method) | `BaseCommunicationManager` | 抽象基类定义 Start/Stop 骨架，子类实现细节 |
| 接口隔离 (ISP) | `IDisplayAdapter` / `ILogger` / `IBluetoothCommunicator` | 按职责拆分接口，避免胖接口 |
| 状态模式 (State) | `DeviceState` 枚举 | 替代布尔/按钮文本判断，显式状态机 |

---

## 3. 目录结构

```
JXUST_RoboCommunity_UpperApp/
├── UpperApp.sln                              # Visual Studio 解决方案文件
├── CODE_WIKI.md                              # 代码 Wiki 文档
└── UpperApp/                                 # 唯一项目目录
    ├── UpperApp.csproj                       # 项目配置文件
    ├── Program.cs                            # 程序入口点
    ├── AppServices.cs                        # 轻量级 IoC 服务定位器
    ├── AppSettings.cs                        # 应用配置模型
    ├── IConfigStorage.cs                     # 配置存储接口与 JSON 实现
    ├── Utils.cs                              # 静态工具方法
    ├── Form1.cs                              # 主窗体逻辑代码
    ├── Form1.Designer.cs                     # 主窗体设计器代码
    ├── Form1.resx                            # 窗体资源文件
    ├── App.config                            # 应用运行时配置
    ├── jxust.ico                             # 应用图标
    │
    ├── Core/                                 # 核心类型与数据结构
    │   ├── ChannelType.cs                    # 通信通道类型枚举
    │   ├── RecvOrSend.cs                     # 收发方向枚举
    │   ├── BindingDic.cs                     # 线程安全可观察字典
    │   └── Result.cs                         # 不可变通信结果 record
    │
    ├── Commands/                             # 命令模式
    │   ├── IDeviceCommand.cs                 # 设备命令接口
    │   ├── MoveCommand.cs                    # 运动控制命令
    │   └── RawSendCommand.cs                 # 原始数据发送命令
    │
    ├── Communication/                        # 通信管理器
    │   ├── ICommunicator.cs                  # 通信管理器统一接口
    │   ├── ICommunicatorFactory.cs           # 通信管理器工厂接口与实现
    │   ├── IBluetoothCommunicator.cs         # 蓝牙扩展接口
    │   ├── BaseCommunicationManager.cs       # 通信管理器抽象基类
    │   ├── CommunicationParams.cs            # 各通道参数模型
    │   ├── SerManager.cs                     # 串口通信管理器
    │   ├── TCPManager.cs                     # TCP 服务端通信管理器
    │   ├── UDPManager.cs                     # UDP 通信管理器
    │   ├── BthManager.cs                     # 蓝牙通信管理器
    │   ├── CanManager.cs                     # CAN 总线通信管理器
    │   └── WebSocketManager.cs               # WebSocket 通信管理器
    │
    ├── Processing/                           # 消息处理与日志
    │   ├── IDisplayAdapter.cs                # UI 显示抽象接口
    │   ├── ILogger.cs                        # 日志接口
    │   ├── FileLogger.cs                     # 文件日志实现
    │   └── MessageProcessor.cs               # 消息处理逻辑
    │
    ├── Services/                             # 业务服务层
    │   ├── DeviceService.cs                  # 设备服务 Facade（统一入口）
    │   ├── DeviceState.cs                    # 设备状态枚举
    │   ├── DataPipeline.cs                   # 异步数据管道
    │   └── ProtocolHandler.cs                # 协议编解码统一入口
    │
    ├── UI/                                   # UI 辅助组件
    │   ├── MapTracker.cs                     # 地图轨迹绘制逻辑
    │   ├── ProtocolFormatter.cs              # 通信协议格式化
    │   └── ProtocolParser.cs                 # 通信协议解析
    │
    └── Properties/
        ├── app.manifest                      # UAC 清单（asInvoker，无需管理员权限）
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

#### `Program.cs`

应用程序入口点，执行以下操作：
1. 启用 WinForms 视觉样式
2. 调用 `AppServices.ConfigureServices()` 注册全局服务
3. 创建并运行主窗体 `UpperApp`

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
- `DeviceService`（单例，内部持有所有通信管理器和蓝牙接口）

---

### 4.2 设备服务层（Facade）

#### `DeviceService` — 统一设备操作入口

`DeviceService` 是 UI 层与通信管理器之间的**唯一中介**，UI 层不直接访问任何 `ICommunicator` 或 `IBluetoothCommunicator`。

**事件**：

| 事件 | 说明 |
|------|------|
| `StatusChanged` | 聚合所有通道的状态变化事件，UI 只需订阅此事件 |

**通道状态查询**：

| 方法/属性 | 说明 |
|------|------|
| `GetChannelState(ChannelType)` | 获取指定通道状态（含蓝牙） |
| `IsChannelReady(ChannelType)` | 指定通道是否已连接 |
| `IsAnyChannelReady()` | 是否有任意通道已连接 |
| `ActiveChannel` | 当前活跃发送通道 |

**通道操作**：

| 方法 | 说明 |
|------|------|
| `StartChannel(ChannelType, CommunicationParams)` | 启动指定通道 |
| `StopChannel(ChannelType)` | 停止指定通道 |
| `GetPeerList(ChannelType)` | 获取指定通道的对端列表（含蓝牙） |

**命令执行**：

| 方法 | 说明 |
|------|------|
| `ExecuteCommand(IDeviceCommand)` | 执行命令（未连接则抛异常） |
| `TryExecuteCommand(IDeviceCommand)` | 尝试执行命令（返回 bool） |
| `SetTarget(string)` | 设置 TCP/UDP 目标 |
| `SetBluetoothTarget(string)` | 设置蓝牙目标 |

**蓝牙代理**：

| 方法/属性 | 说明 |
|------|------|
| `IsBluetoothReady` | 蓝牙是否已连接 |
| `IsBluetoothRadioAvailable` | 蓝牙适配器是否可用 |
| `IsBluetoothRadioPoweredOn` | 蓝牙是否已开启 |
| `BluetoothRadioAddress` / `BluetoothRadioMode` | 适配器信息 |
| `StartBluetooth(CommunicationParams)` | 启动蓝牙 |
| `StopBluetooth()` | 停止蓝牙 |
| `SendBluetooth(string, string)` | 蓝牙发送数据 |
| `DiscoverBluetoothDevicesAsync()` | 扫描蓝牙设备 |
| `ConnectBluetoothDevice(string)` | 连接蓝牙设备 |
| `DisconnectBluetoothClient()` | 断开蓝牙客户端 |

**生命周期**：

| 方法 | 说明 |
|------|------|
| `StopAll()` | 停止所有通道（含蓝牙） |
| `DisposeAll()` | 停止并释放所有通道资源（含蓝牙） |

#### `DeviceState` — 设备状态枚举

```
Disconnected  → 未连接
Connecting    → 连接中
Connected     → 已连接
Disconnecting → 断开中
Error         → 异常状态
```

替代了原有的 `IsMonitoring` 布尔值和按钮文本判断，提供显式状态机语义。

#### `DataPipeline` — 异步数据管道

基于 `System.Threading.Channels` 的有界异步管道，解耦数据接收与 UI 处理：

| 成员 | 说明 |
|------|------|
| `Start()` | 启动后台消费循环 |
| `TryEnqueue(Result)` | 非阻塞入队（满时丢弃最旧） |
| `Dispose()` | 停止管道并释放资源 |

#### `ProtocolHandler` — 协议编解码统一入口

| 方法 | 说明 |
|------|------|
| `TryParse(string)` | 尝试解析协议字符串，返回 `ParsedData?` |
| `EncodeMove(...)` | 编码运动指令 |
| `EncodeForwardBackward(int)` | 编码前后速度指令 |
| `EncodeRightLeft(int)` | 编码左右方向指令 |

---

### 4.3 命令模式

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

### 4.4 通信管理器体系

通信管理器是本项目的核心，采用**策略模式 + 模板方法**实现多通道统一管理。

#### 接口层

**`ICommunicator`** — 通信管理器统一接口

| 成员 | 类型 | 说明 |
|------|------|------|
| `StatusChanged` | `event Action<Result>` | 状态变化事件 |
| `Channel` | `ChannelType` | 通信通道类型 |
| `State` | `DeviceState` | 当前设备状态 |
| `Start(CommunicationParams)` | `void` | 启动通信 |
| `Stop()` | `void` | 停止通信，释放资源 |
| `Send(string, string)` | `void` | 发送数据 |
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

**`BaseCommunicationManager`** — 所有通信管理器的公共基类

| 成员 | 说明 |
|------|------|
| `_channel` | 通道类型（构造时指定） |
| `encoding` | 默认编码 GB2312 |
| `_cts` | `CancellationTokenSource`，用于取消异步接收循环 |
| `_isMonitoring` | 监听状态标志（protected，不对外暴露） |
| `State` | `DeviceState` 设备状态属性 |
| `StartCore()` | `protected` 公共启动逻辑 |
| `Stop()` | 公共停止逻辑：取消 CTS，调用 `OnStopping()` |
| `OnStopping()` | 抽象方法，子类实现资源释放 |
| `OnStatusChanged(Result)` | 触发 StatusChanged 事件，自动填充 Channel |
| `Dispose()` / `DisposeAsync()` | IDisposable 实现 |

#### 具体实现

| 类 | 通道 | 关键特性 |
|----|------|---------|
| **`SerManager`** | Serial | `SerialPort` + `BaseStream.ReadAsync`；GB2312 编码 |
| **`TCPManager`** | TCP | `TcpListener` + `BindingDic<Socket>` 多客户端管理 |
| **`UDPManager`** | UDP | `UdpClient` + `BindingList<string>` 对端列表 |
| **`BthManager`** | Bluetooth | `InTheHand.Net.Bluetooth`；服务端/客户端双模式；UTF-8 |
| **`CANManager`** | CAN | `Peak.PCANBasic.NET`；500kbps；`ID:HexData` 格式 |
| **`WebSocketManager`** | WebSocket | `HttpListener` 服务端 + `ClientWebSocket` 客户端；UTF-8 |

---

### 4.5 消息处理层

#### `IDisplayAdapter` — UI 显示抽象接口

| 成员 | 说明 |
|------|------|
| `UpdateByteCount(int, RecvOrSend)` | 更新收发字节计数 |
| `IsCharMode` / `IsHexMode` | 显示模式 |
| `IsLocalEchoEnabled` | 本地回显开关 |
| `IsAngleDisplayEnabled` | 角度显示开关 |
| `AppendToReceiveBox(string)` | 追加文本到接收区 |
| `UpdateAngleDisplay(string)` | 设置角度/距离显示 |
| `OnNewPeer(string)` | 新对端通知 |

#### `ILogger` / `FileLogger` — 日志接口与实现

| 方法 | 说明 |
|------|------|
| `WriteLine(string)` | 写入一行日志 |
| `Open(string)` | 打开日志文件 |
| `Close()` | 关闭日志文件 |
| `IsOpen` | 日志文件是否已打开 |

#### `MessageProcessor` — 消息处理器

依赖 `IDisplayAdapter` + `ILogger`，将消息处理逻辑从窗体中解耦。

| 方法 | 说明 |
|------|------|
| `ProcessReceivedMessage(Result)` | 处理接收消息：计数 → 角度显示 → 新对端通知 → 格式化显示 |
| `ProcessSentMessage(Result)` | 处理发送消息：成功时回显/日志 + 计数，失败时显示错误 |
| `ProcessException(Result)` | 处理异常：显示异常信息到接收区 |

---

### 4.6 配置持久化层

#### `IConfigStorage` / `JsonFileConfigStorage`

| 方法 | 说明 |
|------|------|
| `LoadAsync()` | 从 `settings.json` 异步加载配置 |
| `SaveAsync(AppSettings)` | 异步保存配置 |
| `SaveSync(AppSettings)` | 同步保存配置（用于 FormClosing） |

---

### 4.7 UI 辅助组件

#### `MapTracker` — 地图轨迹绘制

| 方法 | 说明 |
|------|------|
| `SetCalibratedDistance(float)` | 设置标定距离 |
| `SetAspectRatio(float)` | 设置宽高比 |
| `OnDistanceChanged(...)` | 距离变化时绘制轨迹点 |
| `OnMapClick(...)` | 点击地图设置锚点 |
| `OnMouseMove(Point)` | 鼠标移动时绘制预览线 |
| `Clear()` | 清除轨迹 |

#### `ProtocolFormatter` — 通信协议格式化

| 方法 | 格式 |
|------|------|
| `ForwardBackward(int)` | `FB:{value}:OVER\r\n` |
| `RightLeft(int)` | `RL:{value}:OVER\r\n` |
| `FullControl(int, int)` | `FR:{speed}:{direction}:OVER\r\n` |

#### `ProtocolParser` — 通信协议解析

| 方法 | 说明 |
|------|------|
| `TryParse(string, out ParsedData)` | 尝试解析协议字符串，返回数据类型和值 |

---

### 4.8 主窗体（UI 层）

#### `UpperApp` (Form1.cs) : `IDisplayAdapter`

主窗体是整个应用的**UI 协调中心**，通过 `DeviceService` 与通信层交互，不直接访问任何通信管理器。

**核心字段**：

| 字段 | 类型 | 说明 |
|------|------|------|
| `_deviceService` | `DeviceService` | 设备服务 Facade（唯一通信入口） |
| `_receivePipeline` | `DataPipeline` | 异步数据接收管道 |
| `_configStorage` | `IConfigStorage` | 配置存储服务 |
| `_logger` | `ILogger` | 日志服务 |
| `_msgProcessor` | `MessageProcessor` | 消息处理器 |
| `_mapTracker` | `MapTracker` | 地图轨迹追踪器 |
| `_currentSettings` | `AppSettings` | 当前配置 |

**关键交互模式**：

- **事件订阅**：`_deviceService.StatusChanged += UnifiedStatusChanged`（聚合事件，非逐个通道）
- **通道启停**：`_deviceService.StartChannel()` / `_deviceService.StopChannel()`
- **状态查询**：`_deviceService.IsChannelReady()` / `_deviceService.IsAnyChannelReady()`
- **命令发送**：`_deviceService.TryExecuteCommand(new MoveCommand(...))`
- **蓝牙操作**：`_deviceService.StartBluetooth()` / `_deviceService.SendBluetooth()` 等

---

## 5. 关键类与函数说明

### 5.1 `Result` — 不可变通信结果 record

```
Result (record, init-only properties)
├── Message: string          # 消息内容
├── Num: int                 # 数据长度
├── Status: ResStatus        # Success / Error / Alert / SetNum
├── NetStatus: NETStatus     # 网络状态枚举
├── RemoteIP: string         # 远端标识
├── IPPort: string           # 端口信息
├── NewPeer: string          # 新对端标识
└── Channel: ChannelType     # 通道类型
```

修改时使用 `with` 表达式：`result with { Channel = _channel }`

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
Connecting    → 连接中（StartCore 调用时）
Connected     → 已连接（StartCore 完成后）
Disconnecting → 断开中（Stop 调用时）
Error         → 异常状态（ExceptionStop 时）
```

状态转换由 `BaseCommunicationManager` 内部管理，外部通过 `ICommunicator.State` 只读访问。

---

## 6. 依赖关系

### 6.1 NuGet 包依赖

| 包名 | 版本 | 用途 |
|------|------|------|
| `InTheHand.Net.Bluetooth` | 4.2.4 | 蓝牙 SPP 通信 |
| `Peak.PCANBasic.NET` | 5.0.1.1131 | PEAK PCAN USB CAN 总线通信 |
| `System.IO.Ports` | 10.0.6 | 串口通信 |
| `System.Data.DataSetExtensions` | 4.6.0-preview3 | DataSet 扩展 |

### 6.2 模块间依赖关系图

```
Program.cs
  └→ AppServices.ConfigureServices()
       ├→ CommunicatorFactory (ICommunicatorFactory 单例)
       ├→ JsonFileConfigStorage (IConfigStorage 单例)
       └→ DeviceService (单例)
            ├→ Dictionary<ChannelType, ICommunicator> (各通道管理器)
            └→ IBluetoothCommunicator (蓝牙接口)

UpperApp (主窗体, 实现 IDisplayAdapter)
  ├→ AppServices.GetService<DeviceService>()
  │     └→ DeviceService.StatusChanged (聚合事件)
  │     └→ DeviceService.StartChannel / StopChannel / IsChannelReady / ...
  │     └→ DeviceService.StartBluetooth / SendBluetooth / ...
  │     └→ DeviceService.TryExecuteCommand(IDeviceCommand)
  ├→ AppServices.GetService<IConfigStorage>()
  ├→ DataPipeline(DispatchReceivedData)
  ├→ MessageProcessor(this, _logger)
  ├→ MapTracker(MapBox)
  └→ ProtocolHandler (静态方法)

各 *Manager
  ├→ BaseCommunicationManager (继承)
  ├→ ICommunicator (实现)
  ├→ CommunicationParams (参数类型)
  ├→ Result (事件数据, record)
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

### 7.2 接收数据格式

```
YAW:{value}/OVER
ROLL:{value}/OVER
PITCH:{value}/OVER
DISTANCE:{value}/OVER
```

### 7.3 编码约定

| 通道 | 编码 |
|------|------|
| 串口 / TCP / UDP | GB2312 |
| 蓝牙 / WebSocket | UTF-8 |
| CAN | 十六进制原始字节 |

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

> **文档版本**: 3.0  
> **更新日期**: 2026-05-11  
> **对应项目版本**: V7.0
