# UI 现代化改造实施计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 将上位机 UI 从 8 年前的固定尺寸灰色主题改造为现代深色工业风三栏自适应布局

**Architecture:** 三栏布局（左:连接配置 | 中:数据收发 | 右:控制面板），深色主题通过自绘控件实现（不依赖第三方暗色库），TableLayoutPanel + SplitContainer 实现自适应缩放

**Tech Stack:** WinForms / .NET 10 / 自绘控件 (OwnerDraw) / TableLayoutPanel / SplitContainer / Anchor+Dock

---

## 文件结构

| 操作 | 文件 | 职责 |
|------|------|------|
| 创建 | `UpperApp/UI/ThemeColors.cs` | 深色主题颜色常量，统一管理所有颜色 |
| 创建 | `UpperApp/UI/DarkRenderer.cs` | 自绘控件渲染器，统一绘制按钮/输入框/标签等 |
| 修改 | `UpperApp/Form1.Designer.cs` | 全面重构布局：三栏、SplitContainer、语义化命名 |
| 修改 | `UpperApp/Form1.cs` | 适配新布局和控件命名，添加自绘事件处理 |
| 修改 | `UpperApp/UpperApp.csproj` | 确保无额外依赖 |

---

### Task 1: 创建深色主题颜色常量类

**Files:**
- Create: `UpperApp/UI/ThemeColors.cs`

- [ ] **Step 1: 创建 ThemeColors.cs**

```csharp
using System.Drawing;

namespace UpperApp.UI
{
    internal static class ThemeColors
    {
        public static readonly Color BackgroundPrimary = Color.FromArgb(13, 17, 23);
        public static readonly Color BackgroundSecondary = Color.FromArgb(22, 27, 34);
        public static readonly Color BackgroundTertiary = Color.FromArgb(28, 35, 51);
        public static readonly Color BackgroundCard = Color.FromArgb(26, 34, 51);

        public static readonly Color Border = Color.FromArgb(48, 54, 61);
        public static readonly Color BorderActive = Color.FromArgb(88, 166, 255);

        public static readonly Color TextPrimary = Color.FromArgb(230, 237, 243);
        public static readonly Color TextSecondary = Color.FromArgb(139, 148, 158);
        public static readonly Color TextMuted = Color.FromArgb(72, 79, 88);

        public static readonly Color AccentBlue = Color.FromArgb(88, 166, 255);
        public static readonly Color AccentGreen = Color.FromArgb(63, 185, 80);
        public static readonly Color AccentOrange = Color.FromArgb(210, 153, 34);
        public static readonly Color AccentRed = Color.FromArgb(248, 81, 73);
        public static readonly Color AccentPurple = Color.FromArgb(188, 140, 255);
        public static readonly Color AccentCyan = Color.FromArgb(57, 210, 192);

        public static readonly Color GlowBlue = Color.FromArgb(22, 43, 68);
        public static readonly Color GlowGreen = Color.FromArgb(16, 46, 20);

        public static readonly Color ButtonPrimary = AccentBlue;
        public static readonly Color ButtonPrimaryHover = Color.FromArgb(121, 184, 255);
        public static readonly Color ButtonDanger = AccentRed;
        public static readonly Color ButtonGhost = BackgroundTertiary;

        public static readonly Color InputBackground = BackgroundPrimary;
        public static readonly Color InputBorder = Border;
        public static readonly Color InputFocusBorder = AccentBlue;

        public static readonly Color StatusBarBg = BackgroundTertiary;

        public static readonly Color TrackBarTrack = BackgroundPrimary;
        public static readonly Color TrackBarFill = Color.FromArgb(88, 166, 255);
        public static readonly Color TrackBarThumb = AccentCyan;
    }
}
```

- [ ] **Step 2: 构建验证**

Run: `dotnet build UpperApp/UpperApp.csproj --no-restore`
Expected: 0 errors

---

### Task 2: 创建自绘控件渲染器

**Files:**
- Create: `UpperApp/UI/DarkRenderer.cs`

- [ ] **Step 1: 创建 DarkRenderer.cs**

此渲染器为所有需要自绘的控件提供统一的绘制方法。包括：按钮、GroupBox、TabControl、TextBox 边框、状态指示灯等。

```csharp
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace UpperApp.UI
{
    internal static class DarkRenderer
    {
        public static void DrawButton(PaintEventArgs e, Button btn, bool isHovered = false, bool isPressed = false)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Color bgColor, borderColor, textColor;

            if (btn.FlatStyle == FlatStyle.Standard && btn.Tag as string == "Primary")
            {
                bgColor = isPressed ? ThemeColors.ButtonPrimaryHover : (isHovered ? ThemeColors.ButtonPrimaryHover : ThemeColors.ButtonPrimary);
                borderColor = ThemeColors.ButtonPrimary;
                textColor = Color.White;
            }
            else if (btn.Tag as string == "Danger")
            {
                bgColor = isPressed ? Color.FromArgb(180, 50, 45) : (isHovered ? Color.FromArgb(200, 60, 55) : Color.FromArgb(60, 30, 30));
                borderColor = ThemeColors.ButtonDanger;
                textColor = ThemeColors.ButtonDanger;
            }
            else
            {
                bgColor = isPressed ? ThemeColors.BackgroundSecondary : (isHovered ? ThemeColors.BackgroundTertiary : ThemeColors.ButtonGhost);
                borderColor = ThemeColors.Border;
                textColor = isHovered ? ThemeColors.TextPrimary : ThemeColors.TextSecondary;
            }

            using var brush = new SolidBrush(bgColor);
            using var pen = new Pen(borderColor, 1);
            var rect = new Rectangle(0, 0, btn.Width - 1, btn.Height - 1);
            e.Graphics.FillRectangle(brush, rect);
            e.Graphics.DrawRectangle(pen, rect);

            using var textBrush = new SolidBrush(textColor);
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            e.Graphics.DrawString(btn.Text, btn.Font, textBrush, btn.ClientRectangle, sf);
        }

        public static void DrawGroupBox(PaintEventArgs e, GroupBox grp)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using var bgBrush = new SolidBrush(ThemeColors.BackgroundSecondary);
            e.Graphics.FillRectangle(bgBrush, grp.ClientRectangle);

            using var borderPen = new Pen(ThemeColors.Border, 1);
            var rect = new Rectangle(0, grp.Font.Height / 2, grp.Width - 1, grp.Height - grp.Font.Height / 2 - 1);
            e.Graphics.DrawRectangle(borderPen, rect);

            using var textBrush = new SolidBrush(ThemeColors.AccentBlue);
            var textRect = new Rectangle(6, 0, grp.Width - 12, grp.Font.Height);
            e.Graphics.DrawString(grp.Text, grp.Font, textBrush, textRect);
        }

        public static void DrawTabControl(PaintEventArgs e, TabControl tabCtrl)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using var bgBrush = new SolidBrush(ThemeColors.BackgroundPrimary);
            e.Graphics.FillRectangle(bgBrush, tabCtrl.ClientRectangle);
        }

        public static void DrawTabPage(PaintEventArgs e, TabPage page)
        {
            using var bgBrush = new SolidBrush(ThemeColors.BackgroundSecondary);
            e.Graphics.FillRectangle(bgBrush, page.ClientRectangle);
        }

        public static void DrawStatusIndicator(Graphics g, Point location, bool isConnected)
        {
            var color = isConnected ? ThemeColors.AcccentGreen : ThemeColors.TextMuted;
            using var brush = new SolidBrush(color);
            g.FillEllipse(brush, location.X, location.Y, 8, 8);

            if (isConnected)
            {
                using var glowBrush = new SolidBrush(Color.FromArgb(60, ThemeColors.AccentGreen));
                g.FillEllipse(glowBrush, location.X - 2, location.Y - 2, 12, 12);
            }
        }

        public static void ApplyThemeToForm(Form form)
        {
            form.BackColor = ThemeColors.BackgroundPrimary;
            form.ForeColor = ThemeColors.TextPrimary;
            ApplyThemeToControl(form);
        }

        public static void ApplyThemeToControl(Control control)
        {
            switch (control)
            {
                case Button btn:
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                    btn.FlatAppearance.MouseOverBackColor = Color.Transparent;
                    btn.FlatAppearance.MouseDownBackColor = Color.Transparent;
                    btn.BackColor = ThemeColors.ButtonGhost;
                    btn.ForeColor = ThemeColors.TextSecondary;
                    btn.Paint += (s, e) => DrawButton(e, btn, btn.ClientRectangle.Contains(btn.PointToClient(Cursor.Position)));
                    break;

                case TextBox txt:
                    txt.BackColor = ThemeColors.InputBackground;
                    txt.ForeColor = ThemeColors.TextPrimary;
                    txt.BorderStyle = BorderStyle.FixedSingle;
                    break;

                case ComboBox cbo:
                    cbo.BackColor = ThemeColors.InputBackground;
                    cbo.ForeColor = ThemeColors.TextPrimary;
                    cbo.FlatStyle = FlatStyle.Flat;
                    break;

                case CheckBox chk:
                    chk.BackColor = Color.Transparent;
                    chk.ForeColor = ThemeColors.TextSecondary;
                    break;

                case RadioButton rdo:
                    rdo.BackColor = Color.Transparent;
                    rdo.ForeColor = ThemeColors.TextSecondary;
                    break;

                case Label lbl:
                    lbl.BackColor = Color.Transparent;
                    lbl.ForeColor = lbl.Tag as string == "Value" ? ThemeColors.AccentOrange : ThemeColors.TextSecondary;
                    break;

                case GroupBox grp:
                    grp.BackColor = ThemeColors.BackgroundSecondary;
                    grp.ForeColor = ThemeColors.AccentBlue;
                    grp.Paint += (s, e) => { DrawGroupBox(e, grp); };
                    break;

                case TabControl tab:
                    tab.BackColor = ThemeColors.BackgroundPrimary;
                    tab.ForeColor = ThemeColors.TextSecondary;
                    break;

                case TabPage page:
                    page.BackColor = ThemeColors.BackgroundSecondary;
                    page.ForeColor = ThemeColors.TextPrimary;
                    break;

                case TrackBar track:
                    track.BackColor = ThemeColors.BackgroundSecondary;
                    break;

                case Panel pnl:
                    pnl.BackColor = ThemeColors.BackgroundSecondary;
                    break;

                case SplitContainer split:
                    split.BackColor = ThemeColors.Border;
                    split.Panel1.BackColor = ThemeColors.BackgroundSecondary;
                    split.Panel2.BackColor = ThemeColors.BackgroundSecondary;
                    break;

                case PictureBox pic:
                    pic.BackColor = ThemeColors.BackgroundPrimary;
                    break;

                case MaskedTextBox mtb:
                    mtb.BackColor = ThemeColors.InputBackground;
                    mtb.ForeColor = ThemeColors.TextPrimary;
                    break;
            }

            foreach (Control child in control.Controls)
                ApplyThemeToControl(child);
        }
    }
}
```

注意：上面代码中 `ThemeColors.AcccentGreen` 有拼写错误，应为 `ThemeColors.AccentGreen`。实际编写时需修正。

- [ ] **Step 2: 构建验证**

Run: `dotnet build UpperApp/UpperApp.csproj --no-restore`
Expected: 0 errors

---

### Task 3: 重构 Form1.Designer.cs — 窗口属性与三栏容器

**Files:**
- Modify: `UpperApp/Form1.Designer.cs`

这是最核心的改造任务。将绝对坐标布局改为 SplitContainer 三栏布局，并将窗口改为可缩放。

- [ ] **Step 1: 修改窗体顶级属性**

在 `InitializeComponent()` 末尾的 `UpperApp` 配置区域，将：
- `FormBorderStyle` 从 `FixedSingle` 改为 `Sizable`
- 移除 `MaximizeBox = false`
- `ClientSize` 改为合理默认值如 `1400, 700`
- `MinimumSize` 设为 `1024, 600`
- 移除 `AutoSize = true`
- 添加 `StartPosition = FormStartPosition.CenterScreen`

- [ ] **Step 2: 创建三栏 SplitContainer 结构**

替换原来直接添加到窗体的 `groupBox3` + `tabControl1` 等顶级控件，改为：

```
SplitContainer (mainSplit)
├── SplitterDistance = 280
├── Panel1 (左: 连接配置)
│   └── 连接配置相关控件
└── Panel2
    ├── SplitContainer (rightSplit)
    │   ├── SplitterDistance = 计算值(中间栏宽度)
    │   ├── Panel1 (中: 数据收发)
    │   │   └── 收发相关控件
    │   └── Panel2 (右: 控制面板)
    │       └── TabControl
```

- [ ] **Step 3: 构建验证**

Run: `dotnet build UpperApp/UpperApp.csproj --no-restore`
Expected: 0 errors (此时布局可能不完整但应编译通过)

---

### Task 4: 重构左侧面板 — 连接配置

**Files:**
- Modify: `UpperApp/Form1.Designer.cs`

将原来散落在 groupBox3 中的串口/网络配置提取到左侧面板，使用 TabControl 切换。

- [ ] **Step 1: 创建左侧面板内部结构**

在 `mainSplit.Panel1` 中创建：
- `TabControl` (tabConnConfig) — 三个 TabPage: 串口/网络/蓝牙
- 每个 TabPage 内放对应配置控件
- 底部放通道选择和自动发送设置

- [ ] **Step 2: 迁移串口配置控件**

将 SerPortItem, Baud, btnSerial, label2, label3 移入"串口"TabPage

- [ ] **Step 3: 迁移网络配置控件**

将 HostIP, Port, NetType, btnListen, Peer, label26, label27, label28 移入"网络"TabPage

- [ ] **Step 4: 迁移蓝牙配置控件**

将 BthListenBtn, BthConnectBtn, BthDeviceScanBtn, BthDeviceList, ChoseSlaveBthList 等移入"蓝牙"TabPage

- [ ] **Step 5: 添加底部设置区**

在左侧面板底部添加：
- 通道选择 (rbtnSerial, rbtnNET, rbtnBluetooth)
- 自动发送 (btnAutoSend, Tim, label16)
- 显示模式 (rbtnHex, rbtnChar)
- 本地回显 (ReDisp)

- [ ] **Step 6: 构建验证**

---

### Task 5: 重构中间面板 — 数据收发区

**Files:**
- Modify: `UpperApp/Form1.Designer.cs`

- [ ] **Step 1: 创建中间面板结构**

在 `rightSplit.Panel1` 中创建：
- 上方: RecvBox (Dock=Fill, 带标题栏和 Rx 计数)
- 下方: SendBox + 发送/开始按钮 (Dock=Bottom)
- 底部: 状态栏 (连接状态、通道、Rx/Tx 字节)

- [ ] **Step 2: 迁移收发控件**

将 RecvBox, SendBox, btnSend, btnBegin, btnclRecv, btnclSend, label17, label18, label20, label22 移入中间面板

- [ ] **Step 3: 添加状态栏**

创建底部状态栏面板，显示：连接状态指示灯、当前通道、Rx/Tx 字节计数

- [ ] **Step 4: 构建验证**

---

### Task 6: 重构右侧面板 — 控制面板

**Files:**
- Modify: `UpperApp/Form1.Designer.cs`

- [ ] **Step 1: 创建右侧 TabControl**

在 `rightSplit.Panel2` 中创建 TabControl，包含：
- 运动控制 Tab
- 批量字串 Tab
- 行走路线 Tab

- [ ] **Step 2: 迁移运动控制控件**

将 FBBar, RLBar, FBtext, RLtext, label4, label6, label19, smallChange, Rocker, btnNoRL, Stop 移入"运动控制"TabPage

- [ ] **Step 3: 迁移姿态显示到运动控制 Tab 底部**

将 LabYaw, LabRoll, LabPitch, LabDist, label8, label9, label10, label14, AngDirDisp, ClearAngDisp 移入"运动控制"TabPage 底部区域

- [ ] **Step 4: 迁移批量字串控件**

将 MsgBox1~8, MsgHex1~8, btnMsg1~8, label30, label31 移入"批量字串"TabPage

- [ ] **Step 5: 迁移行走路线控件**

将 MapBox, OpenImage, ClearImage, RealDist, label33~label39 移入"行走路线"TabPage

- [ ] **Step 6: 构建验证**

---

### Task 7: 语义化控件命名

**Files:**
- Modify: `UpperApp/Form1.Designer.cs`
- Modify: `UpperApp/Form1.cs`

- [ ] **Step 1: 重命名 Label 控件**

| 旧名 | 新名 |
|------|------|
| label2 | lblPort |
| label3 | lblBaud |
| label4 | lblSpeed |
| label6 | lblDirection |
| label8 | lblYawTitle |
| label9 | lblRollTitle |
| label10 | lblPitchTitle |
| label14 | lblDistTitle |
| label16 | lblMs |
| label17 | lblRx |
| label18 | lblRxCount |
| label19 | lblStep |
| label20 | lblTx |
| label21 | lblRecvFormat |
| label22 | lblTxCount |
| label23 | lblFbFormat |
| label24 | lblRlFormat |
| label25 | lblFrFormat |
| label26 | lblPortLabel |
| label27 | lblHostIP |
| label28 | lblPeer |
| label30 | lblHexHeader |
| label31 | lblMsgHeader |
| label32 | lblFirewallHint |
| label33 | lblStartPointTitle |
| label34 | lblStartPoint |
| label35 | lblEndPointTitle |
| label36 | lblEndPoint |
| label37 | lblMousePosTitle |
| label38 | lblMousePos |
| label39 | lblDistTitle2 |
| label40 | lblMemTitle |
| label41 | lblMemValue |

- [ ] **Step 2: 重命名 GroupBox 控件**

| 旧名 | 新名 |
|------|------|
| groupBox1 | grpSendFormat |
| groupBox2 | grpRecvFormat |
| groupBox3 | grpCommWorkspace |
| groupBox4 | grpMotionControl |
| groupBox5 | grpAttitude |

- [ ] **Step 3: 重命名 Panel 控件**

| 旧名 | 新名 |
|------|------|
| panel1 | pnlDisplayMode |
| panel2 | pnlChannelSelect |
| flowLayoutPanel2 | pnlFlow (或删除如无用) |

- [ ] **Step 4: 更新 Form1.cs 中所有引用**

在 Form1.cs 中全局替换所有旧控件名为新名

- [ ] **Step 5: 构建验证**

---

### Task 8: 应用深色主题

**Files:**
- Modify: `UpperApp/Form1.cs`

- [ ] **Step 1: 在 Form 构造函数中调用主题应用**

在 `InitializeComponent()` 之后、业务初始化之前，添加：

```csharp
DarkRenderer.ApplyThemeToForm(this);
```

- [ ] **Step 2: 设置字体**

将等宽字体应用于数据显示区域：

```csharp
RecvBox.Font = new Font("Consolas", 9F);
SendBox.Font = new Font("Consolas", 9F);
BthRecvBox.Font = new Font("Consolas", 9F);
```

- [ ] **Step 3: 设置标题栏和状态栏颜色**

- [ ] **Step 4: 构建验证并运行测试**

Run: `dotnet build UpperApp/UpperApp.csproj`
Expected: 0 errors

---

### Task 9: 清理废弃控件和代码

**Files:**
- Modify: `UpperApp/Form1.Designer.cs`
- Modify: `UpperApp/Form1.cs`

- [ ] **Step 1: 删除不再需要的控件**

- 删除 grpSendFormat (groupBox1) — 协议格式提示移到状态栏
- 删除 grpRecvFormat (groupBox2) — 同上
- 删除 flowLayoutPanel2 — 如已无用
- 删除 lblFirewallHint (label32) — 不再需要管理员权限

- [ ] **Step 2: 删除 Form1.cs 中未使用的字段**

- 删除 `_currentParams` 字段（已有 CS0169 警告）

- [ ] **Step 3: 最终构建验证**

Run: `dotnet build UpperApp/UpperApp.csproj`
Expected: 0 errors, 尽可能减少 warnings

---

## 注意事项

1. **Designer.cs 是自动生成代码**：所有修改需手动精确操作，不能使用设计器重新生成
2. **Anchor/Dock 策略**：中间面板的 RecvBox 用 Dock=Fill，SendBox 用 Dock=Bottom；左右面板内部控件用 Anchor=Top|Left|Right
3. **SplitContainer**：设置 `Dock=Fill`，`SplitterWidth=1`，`SplitterDistance` 控制初始比例
4. **自绘控件**：Button 的 Paint 事件需要在 ApplyThemeToForm 中统一注册，避免遗漏
5. **事件处理**：控件重命名后，所有事件处理器引用必须同步更新
