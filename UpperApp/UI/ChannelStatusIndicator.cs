using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace UpperApp.UI
{
    // ================================================================
    //  通道连接状态指示：6 个通道的状态灯条
    //  - 通道顺序：Serial / TCP / UDP / BT / WS / CAN
    //  - 颜色：灰=未连接 / 黄=连接中 / 绿=已连接 / 红=异常
    //  - States 属性接收分号分隔的状态码字符串，如 "2;0;0;1;0;0"
    //    每位：0=Disconnected, 1=Connecting, 2=Connected, 3=Error
    // ================================================================

    /// <summary>
    /// 通道连接状态指示灯条。横向排列 6 个通道状态点 + 标签。
    /// </summary>
    public class ChannelStatusIndicator : Control
    {
        private static readonly string[] Labels = { "Serial", "TCP", "UDP", "BT", "WS", "CAN" };

        static ChannelStatusIndicator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ChannelStatusIndicator),
                new FrameworkPropertyMetadata(typeof(ChannelStatusIndicator)));
        }

        public static readonly DependencyProperty StatesProperty =
            DependencyProperty.Register(nameof(States), typeof(string), typeof(ChannelStatusIndicator),
                new FrameworkPropertyMetadata("0;0;0;0;0;0", FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>6 个通道状态码，分号分隔（0=断开 1=连接中 2=已连接 3=异常）</summary>
        public string States
        {
            get => (string)GetValue(StatesProperty);
            set => SetValue(StatesProperty, value);
        }

        protected override void OnRender(DrawingContext dc)
        {
            double w = ActualWidth;
            double h = ActualHeight;
            dc.DrawRectangle(BgBrush, null, new Rect(0, 0, w, h));

            int count = Labels.Length;
            double cellW = w / count;
            double dotR = Math.Min(cellW * 0.18, h * 0.28);
            double dotCy = h * 0.42;
            double labelY = h * 0.72;

            int[] codes = ParseStates(States);

            for (int i = 0; i < count; i++)
            {
                double cx = cellW * (i + 0.5);
                Brush dotBrush = GetBrush(codes[i]);

                // 状态点（带发光效果：先画一层半透明大圆）
                if (codes[i] == 2) // 已连接才发光
                {
                    dc.DrawEllipse(GlowGreen, null, new Point(cx, dotCy), dotR * 1.8, dotR * 1.8);
                }
                dc.DrawEllipse(dotBrush, null, new Point(cx, dotCy), dotR, dotR);

                // 标签
                var fmt = new FormattedText(Labels[i], CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight, FontFace, 9, TextMuted, 1);
                dc.DrawText(fmt, new Point(cx - fmt.Width / 2, labelY - fmt.Height / 2));
            }
        }

        private static int[] ParseStates(string s)
        {
            int[] result = new int[Labels.Length];
            if (string.IsNullOrEmpty(s)) return result;
            var parts = s.Split(';');
            for (int i = 0; i < parts.Length && i < result.Length; i++)
            {
                if (int.TryParse(parts[i].Trim(), out int v))
                    result[i] = v;
            }
            return result;
        }

        private static Brush GetBrush(int code) => code switch
        {
            1 => ConnectingBrush,
            2 => ConnectedBrush,
            3 => ErrorBrush,
            _ => DisconnectedBrush
        };

        private static readonly Brush BgBrush = new SolidColorBrush(Color.FromRgb(0x16, 0x16, 0x1A));
        private static readonly Brush DisconnectedBrush = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x5C));
        private static readonly Brush ConnectingBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x3D));
        private static readonly Brush ConnectedBrush = new SolidColorBrush(Color.FromRgb(0x3D, 0xDC, 0x84));
        private static readonly Brush ErrorBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0x5C, 0x5C));
        private static readonly Brush GlowGreen = new SolidColorBrush(Color.FromRgb(0x3D, 0xDC, 0x84)) { Opacity = 0.25 };
        private static readonly Brush TextMuted = new SolidColorBrush(Color.FromRgb(0x8E, 0x8E, 0x98));
        private static readonly Typeface FontFace = new("Microsoft YaHei UI, Segoe UI, sans-serif");
    }
}
