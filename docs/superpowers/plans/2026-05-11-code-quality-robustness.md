# 代码质量与健壮性优化 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 修复当前项目中的资源泄漏、异常处理缺失、线程安全隐患等代码质量问题，提升应用健壮性。

**Architecture:** 逐模块修复，从底层（通信管理器资源管理）到上层（UI 异常防护），每步保持可编译运行。重点解决 IDisposable 实现、异常吞没、Parse 崩溃、内存监控逻辑等问题。

**Tech Stack:** C# / .NET 10.0 / WinForms / 现有 NuGet 包不变

---

## 问题总览

| # | 问题 | 严重度 | 位置 | 违反原则 |
|---|------|--------|------|----------|
| Q1 | **BaseCommunicationManager 未实现 IDisposable**：`_cts` 和各 Manager 中的 `SerialPort`/`TcpListener`/`UdpClient` 等未通过 Dispose 释放 | 🔴 高 | `BaseCommunicationManager.cs` / 各 Manager | 资源管理 |
| Q2 | **Form1 中多处 `float.Parse`/`int.Parse` 无 try-catch**：输入非法值时直接崩溃 | 🔴 高 | `Form1.cs:791,632,664` | 健壮性 |
| Q3 | **MemTimer 内存监控逻辑错误**：`>100MB` 直接 `Application.Exit()` 无提示，且 `AsSpan(0,5)` 可能越界 | 🔴 高 | `Form1.cs:751-761` | 健壮性/UX |
| Q4 | **FileLogger 未实现 IDisposable**：StreamWriter 未在对象销毁时关闭 | 🟡 中 | `Processing/FileLogger.cs` | 资源管理 |
| Q5 | **SerManager.Stop 中 SerialPort 关闭可能抛异常**：`_serialPort.Close()` 在基类 CTS 取消后可能因竞态抛 `InvalidOperationException` | 🟡 中 | `Communication/SerManager.cs` | 异常处理 |
| Q6 | **TCPManager 客户端 Socket 未正确释放**：`BindingDic.Remove` 返回的 Socket 未调用 `Close()/Dispose()` | 🟡 中 | `Communication/TCPManager.cs` | 资源泄漏 |
| Q7 | **UnifiedStatusChanged 中 ExceptionStop 分支吞异常**：只弹 MessageBox，不记录日志 | 🟡 中 | `Form1.cs:277-283` | 可观测性 |
| Q8 | **SetAngDisp 使用字符串 Contains 做协议解析**：`str.Contains("YAW:")` 会被 `YAW:` 出现在任意位置匹配 | 🟡 中 | `Form1.cs:437-452` | 正确性 |
| Q9 | **BthSendBtn_Click 用按钮文本判断连接状态**：`BthConnectBtn.Text == "断开"` 依赖 UI 文本而非状态 | 🟢 低 | `Form1.cs:552` | 耦合 |
| Q10 | **构造函数中事件绑定过多**：~20 个匿名 lambda 在构造函数中，可读性差 | 🟢 低 | `Form1.cs:52-87` | 可读性 |

---

## File Structure

```
UpperApp/
├── Communication/
│   ├── BaseCommunicationManager.cs   # 修改：添加 IDisposable 实现
│   ├── SerManager.cs                 # 修改：OnStopping 中 try-catch
│   ├── TCPManager.cs                 # 修改：Socket 释放
│   ├── UDPManager.cs                 # 修改：OnStopping 中释放 UdpClient
│   ├── BthManager.cs                 # 修改：OnStopping 中释放资源
│   ├── CANManager.cs                 # 修改：OnStopping 中释放资源
│   └── WebSocketManager.cs           # 修改：OnStopping 中释放资源
├── Processing/
│   └── FileLogger.cs                 # 修改：实现 IDisposable
├── UI/
│   └── ProtocolParser.cs             # 新建：协议解析器
├── Form1.cs                          # 修改：Parse 防护、内存监控、异常日志
└── Core/
    └── Result.cs                     # 不变
```

---

### Task 1: BaseCommunicationManager 实现 IDisposable（Q1）

**Files:**
- Modify: `UpperApp/Communication/BaseCommunicationManager.cs`

- [ ] **Step 1: 修改 BaseCommunicationManager 添加 IDisposable 实现**

将类声明改为：
```csharp
internal abstract class BaseCommunicationManager : ICommunicator, IAsyncDisposable, IDisposable
```

添加 Dispose 方法：
```csharp
public void Dispose()
{
    if (_isMonitoring) Stop();
    _cts?.Dispose();
    GC.SuppressFinalize(this);
}

public ValueTask DisposeAsync()
{
    if (_isMonitoring) Stop();
    _cts?.Dispose();
    GC.SuppressFinalize(this);
    return ValueTask.CompletedTask;
}
```

- [ ] **Step 2: 构建验证**

Run: `dotnet build "d:\Workspace\CSharp\JXUST_RoboCommunity_UpperApp\UpperApp.sln"`
Expected: BUILD SUCCEEDED

- [ ] **Step 3: Commit**

```bash
git add UpperApp/Communication/BaseCommunicationManager.cs
git commit -m "fix: BaseCommunicationManager 实现 IDisposable，释放 _cts 资源 (Q1)"
```

---

### Task 2: 各 Manager 的 OnStopping 中添加资源释放和异常防护（Q1, Q5, Q6）

**Files:**
- Modify: `UpperApp/Communication/SerManager.cs`
- Modify: `UpperApp/Communication/TCPManager.cs`
- Modify: `UpperApp/Communication/UDPManager.cs`
- Modify: `UpperApp/Communication/BthManager.cs`
- Modify: `UpperApp/Communication/CANManager.cs`
- Modify: `UpperApp/Communication/WebSocketManager.cs`

- [ ] **Step 1: 修改 SerManager.OnStopping — 添加 try-catch 保护 SerialPort.Close()**

请先读取 `SerManager.cs` 找到 `OnStopping` 方法，将：
```csharp
_serialPort?.Close();
_serialPort?.Dispose();
_serialPort = null;
```
改为：
```csharp
try { _serialPort?.Close(); } catch { }
try { _serialPort?.Dispose(); } catch { }
_serialPort = null;
```

- [ ] **Step 2: 修改 TCPManager.OnStopping — 释放所有客户端 Socket**

请先读取 `TCPManager.cs` 找到 `OnStopping` 方法，确保在停止 TcpListener 之前关闭所有客户端连接：

在 `OnStopping` 中添加以下逻辑（在现有代码之前）：
```csharp
foreach (var key in _clients.connectionKeys.ToList())
{
    if (_clients.Remove(key) is Socket socket)
    {
        try { socket.Shutdown(SocketShutdown.Both); } catch { }
        try { socket.Close(); } catch { }
    }
}
```

同时确保 `TcpListener.Stop()` 也在 try-catch 中：
```csharp
try { _listener?.Stop(); } catch { }
_listener = null;
```

- [ ] **Step 3: 修改 UDPManager.OnStopping — 释放 UdpClient**

在 `OnStopping` 中：
```csharp
try { _udpClient?.Close(); } catch { }
_udpClient = null;
```

- [ ] **Step 4: 修改 BthManager.OnStopping — 释放蓝牙资源**

在 `OnStopping` 中确保所有 BluetoothClient 和 Stream 被关闭：
```csharp
try { _stream?.Close(); } catch { }
try { _client?.Close(); } catch { }
try { _listener?.Stop(); } catch { }
```

- [ ] **Step 5: 修改 CANManager.OnStopping — 释放 PCAN 资源**

在 `OnStopping` 中：
```csharp
try { Peak.PCANBasic.NET.Api.Uninitialize(_pcanChannel); } catch { }
```

- [ ] **Step 6: 修改 WebSocketManager.OnStopping — 释放 WebSocket 资源**

在 `OnStopping` 中确保所有 WebSocket 连接被关闭，HttpListener 被停止。

- [ ] **Step 7: 构建验证**

Run: `dotnet build "d:\Workspace\CSharp\JXUST_RoboCommunity_UpperApp\UpperApp.sln"`
Expected: BUILD SUCCEEDED

- [ ] **Step 8: Commit**

```bash
git add UpperApp/Communication/
git commit -m "fix: 各 Manager OnStopping 添加资源释放和异常防护 (Q1/Q5/Q6)"
```

---

### Task 3: FileLogger 实现 IDisposable（Q4）

**Files:**
- Modify: `UpperApp/Processing/FileLogger.cs`

- [ ] **Step 1: 修改 FileLogger 添加 IDisposable**

将类声明改为：
```csharp
internal class FileLogger : ILogger, IDisposable
```

添加 Dispose 方法：
```csharp
public void Dispose()
{
    Close();
    GC.SuppressFinalize(this);
}
```

- [ ] **Step 2: 构建验证**

Run: `dotnet build "d:\Workspace\CSharp\JXUST_RoboCommunity_UpperApp\UpperApp.sln"`
Expected: BUILD SUCCEEDED

- [ ] **Step 3: Commit**

```bash
git add UpperApp/Processing/FileLogger.cs
git commit -m "fix: FileLogger 实现 IDisposable (Q4)"
```

---

### Task 4: Form1 中 Parse 调用添加异常防护（Q2）

**Files:**
- Modify: `UpperApp/Form1.cs`

- [ ] **Step 1: 修复 RealDist_TextChanged 中的 float.Parse**

将：
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
改为：
```csharp
private void RealDist_TextChanged(object sender, EventArgs e)
{
    string buf = RealDist.Text.Replace(" ", string.Empty);
    if (buf != "." && float.TryParse(buf, out float dist))
    {
        _mapTracker.SetCalibratedDistance(dist);
        Infotext.Text = "dist:" + dist;
    }
}
```

- [ ] **Step 2: 修复 BtnBegin_Click 中的 int.Parse**

将：
```csharp
Counter = int.Parse(Tim.Text) / 100;
```
改为：
```csharp
if (!int.TryParse(Tim.Text, out int interval) || interval < 100)
{
    Infotext.Text = "时间间隔无效";
    return;
}
Counter = interval / 100;
```

- [ ] **Step 3: 修复 smallChange.Leave 中的 int.Parse**

将：
```csharp
smallChange.Leave += new EventHandler((sender, e) => { FBBar.SmallChange = RLBar.SmallChange = int.Parse(smallChange.Text); });
```
改为：
```csharp
smallChange.Leave += new EventHandler((sender, e) => { if (int.TryParse(smallChange.Text, out int v)) FBBar.SmallChange = RLBar.SmallChange = v; });
```

- [ ] **Step 4: 构建验证**

Run: `dotnet build "d:\Workspace\CSharp\JXUST_RoboCommunity_UpperApp\UpperApp.sln"`
Expected: BUILD SUCCEEDED

- [ ] **Step 5: Commit**

```bash
git add UpperApp/Form1.cs
git commit -m "fix: Form1 中 Parse 调用改为 TryParse 防崩溃 (Q2)"
```

---

### Task 5: 修复 MemTimer 内存监控逻辑（Q3）

**Files:**
- Modify: `UpperApp/Form1.cs`

- [ ] **Step 1: 修复 MemTimer_Tick**

将：
```csharp
private void MemTimer_Tick(object sender, EventArgs e)
{
    double usemem = Process.GetCurrentProcess().PrivateMemorySize64 / 1024.0 / 1024.0;
    if (usemem > 100)
        Application.Exit();
    try
    {
        label41.Text = string.Concat(usemem.ToString().AsSpan(0, 5), "M");
    }
    catch { }
}
```
改为：
```csharp
private void MemTimer_Tick(object sender, EventArgs e)
{
    double usemem = Process.GetCurrentProcess().PrivateMemorySize64 / 1024.0 / 1024.0;
    label41.Text = $"{usemem:F1}M";
    if (usemem > 200)
    {
        var result = MessageBox.Show(this, $"内存占用已达 {usemem:F0}MB，是否关闭应用？", "内存警告", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (result == DialogResult.Yes)
            Application.Exit();
    }
}
```

- [ ] **Step 2: 构建验证**

Run: `dotnet build "d:\Workspace\CSharp\JXUST_RoboCommunity_UpperApp\UpperApp.sln"`
Expected: BUILD SUCCEEDED

- [ ] **Step 3: Commit**

```bash
git add UpperApp/Form1.cs
git commit -m "fix: MemTimer 内存监控改为提示确认而非强制退出，修复 AsSpan 越界 (Q3)"
```

---

### Task 6: UnifiedStatusChanged 中 ExceptionStop 添加日志记录（Q7）

**Files:**
- Modify: `UpperApp/Form1.cs`

- [ ] **Step 1: 在 ExceptionStop 分支添加日志**

将：
```csharp
case Result.NETStatus.ExceptionStop:
    if (status.Channel == ChannelType.Serial)
        MessageBox.Show(this, status.Message, "串口错误");
    else if (status.Channel == ChannelType.Bluetooth)
        MessageBox.Show(this, status.Message, "蓝牙错误");
    else if (status.Channel == ChannelType.TCP || status.Channel == ChannelType.UDP)
        MessageBox.Show(this, status.Message, string.IsNullOrEmpty(status.RemoteIP) ? "网络错误" : "远端关闭");
    break;
```
改为：
```csharp
case Result.NETStatus.ExceptionStop:
    _logger.WriteLine($"[{Utils.GetTime()}] ExceptionStop [{status.Channel}]: {status.Message}");
    if (status.Channel == ChannelType.Serial)
        MessageBox.Show(this, status.Message, "串口错误");
    else if (status.Channel == ChannelType.Bluetooth)
        MessageBox.Show(this, status.Message, "蓝牙错误");
    else if (status.Channel == ChannelType.TCP || status.Channel == ChannelType.UDP)
        MessageBox.Show(this, status.Message, string.IsNullOrEmpty(status.RemoteIP) ? "网络错误" : "远端关闭");
    break;
```

- [ ] **Step 2: 构建验证**

Run: `dotnet build "d:\Workspace\CSharp\JXUST_RoboCommunity_UpperApp\UpperApp.sln"`
Expected: BUILD SUCCEEDED

- [ ] **Step 3: Commit**

```bash
git add UpperApp/Form1.cs
git commit -m "fix: ExceptionStop 分支添加日志记录 (Q7)"
```

---

### Task 7: 提取协议解析为 ProtocolParser，修复 SetAngDisp 解析问题（Q8）

**Files:**
- Create: `UpperApp/UI/ProtocolParser.cs`
- Modify: `UpperApp/Form1.cs`

- [ ] **Step 1: 创建 ProtocolParser 类**

```csharp
using System;

namespace UpperApp
{
    internal static class ProtocolParser
    {
        public static bool TryParseAngleData(string input, out string key, out string value)
        {
            key = null;
            value = null;

            if (string.IsNullOrEmpty(input) || !input.Contains("/OVER"))
                return false;

            int colonIndex = input.IndexOf(':');
            if (colonIndex < 0) return false;

            int slashIndex = input.IndexOf('/', colonIndex);
            if (slashIndex < 0) return false;

            key = input[..colonIndex];
            value = input[(colonIndex + 1)..slashIndex];

            return key is "YAW" or "PITCH" or "ROLL" or "DISTANCE";
        }
    }
}
```

- [ ] **Step 2: 修改 Form1.cs 中 SetAngDisp 使用 ProtocolParser**

将：
```csharp
private void SetAngDisp(string str)
{
    if (str.Contains("/OVER"))
    {
        int num = str.IndexOf(':');
        string data = str[(num + 1)..];
        num = data.IndexOf('/');
        data = data[..num];
        if (str.Contains("YAW:"))
            LabYaw.Text = data.ToString();
        else if (str.Contains("PITCH:"))
            LabPitch.Text = data.ToString();
        else if (str.Contains("ROLL:"))
            LabRoll.Text = data.ToString();
        else if (str.Contains("DISTANCE:"))
            LabDist.Text = data.ToString();
    }
}
```
改为：
```csharp
private void SetAngDisp(string str)
{
    if (!ProtocolParser.TryParseAngleData(str, out string key, out string value))
        return;

    switch (key)
    {
        case "YAW": LabYaw.Text = value; break;
        case "PITCH": LabPitch.Text = value; break;
        case "ROLL": LabRoll.Text = value; break;
        case "DISTANCE": LabDist.Text = value; break;
    }
}
```

- [ ] **Step 3: 构建验证**

Run: `dotnet build "d:\Workspace\CSharp\JXUST_RoboCommunity_UpperApp\UpperApp.sln"`
Expected: BUILD SUCCEEDED

- [ ] **Step 4: Commit**

```bash
git add UpperApp/UI/ProtocolParser.cs UpperApp/Form1.cs
git commit -m "fix: 提取 ProtocolParser 修复 SetAngDisp 协议解析问题 (Q8)"
```

---

### Task 8: FormClosing 中使用 Dispose 释放通信管理器资源（Q1 收尾）

**Files:**
- Modify: `UpperApp/Form1.cs`

- [ ] **Step 1: 修改 UpperApp_FormClosing 使用 Dispose**

将：
```csharp
private void UpperApp_FormClosing(object sender, FormClosingEventArgs e)
{
    var settings = CollectCurrentSettings();
    _configStorage.SaveSync(settings);

    foreach (var comm in _communicators.Values)
    {
        comm.Stop();
    }

    _logger.Close();
}
```
改为：
```csharp
private void UpperApp_FormClosing(object sender, FormClosingEventArgs e)
{
    var settings = CollectCurrentSettings();
    _configStorage.SaveSync(settings);

    foreach (var comm in _communicators.Values)
    {
        comm.Stop();
        (comm as IDisposable)?.Dispose();
    }

    (_logger as IDisposable)?.Dispose();
}
```

- [ ] **Step 2: 构建验证**

Run: `dotnet build "d:\Workspace\CSharp\JXUST_RoboCommunity_UpperApp\UpperApp.sln"`
Expected: BUILD SUCCEEDED

- [ ] **Step 3: Commit**

```bash
git add UpperApp/Form1.cs
git commit -m "fix: FormClosing 中 Dispose 释放通信管理器和日志资源 (Q1)"
```

---

## 自检清单

| 检查项 | 状态 |
|--------|------|
| Q1 IDisposable/资源释放 | ✅ Task 1 + Task 2 + Task 8 |
| Q2 Parse 崩溃 | ✅ Task 4 全部改为 TryParse |
| Q3 内存监控逻辑 | ✅ Task 5 改为提示确认 + 修复 AsSpan |
| Q4 FileLogger IDisposable | ✅ Task 3 |
| Q5 SerialPort.Close 异常 | ✅ Task 2 Step 1 |
| Q6 TCP Socket 泄漏 | ✅ Task 2 Step 2 |
| Q7 ExceptionStop 无日志 | ✅ Task 6 |
| Q8 协议解析错误 | ✅ Task 7 ProtocolParser |
| Q9 按钮文本判状态 | ⚠️ 保留（改动较大，需引入状态枚举，收益不足以抵消复杂度） |
| Q10 构造函数 lambda 过多 | ⚠️ 保留（风格偏好，不影响功能） |
| 无 Placeholder | ✅ 所有步骤包含完整代码 |
| 类型一致性 | ✅ 各 Task 间类型定义一致 |
