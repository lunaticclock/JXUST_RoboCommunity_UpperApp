using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace UpperApp.UI
{
    // ================================================================
    //  姿态仪表三件套（方案 B：飞机姿态仪风格）
    //  - CompassGauge : 罗盘，刻度盘随 YAW 反向旋转，顶部固定指针
    //  - RollGauge    : 横滚，地平线随 ROLL 倾斜，顶部固定三角标
    //  - PitchGauge   : 俯仰，地平线随 PITCH 上下平移，中心固定参考线
    //
    //  共同约定：Angle 属性接收字符串（来自 VM 绑定），内部 double.TryParse 解析。
    //  绘制方式：OnRender 直接画，避免 ControlTemplate 中大量变换绑定的复杂度。
    // ================================================================

    /// <summary>
    /// 罗盘仪表：显示偏航角（YAW）。
    /// 刻度盘随偏航角反向旋转（飞机朝东，E 转到顶部），顶部红色指针固定指向当前航向。
    /// </summary>
    public class CompassGauge : Control
    {
        static CompassGauge()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CompassGauge),
                new FrameworkPropertyMetadata(typeof(CompassGauge)));
        }

        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.Register(nameof(Angle), typeof(string), typeof(CompassGauge),
                new FrameworkPropertyMetadata("0", FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>偏航角度字符串（0-360）</summary>
        public string Angle
        {
            get => (string)GetValue(AngleProperty);
            set => SetValue(AngleProperty, value);
        }

        protected override void OnRender(DrawingContext dc)
        {
            double yaw = ParseAngle(Angle);
            double size = Math.Min(ActualWidth, ActualHeight);
            double cx = ActualWidth / 2;
            double cy = ActualHeight / 2;
            double r = size / 2 - 2;

            // 外圈
            dc.DrawEllipse(BgSecondary, BorderPen, new Point(cx, cy), r, r);

            // 刻度盘（随 YAW 反向旋转）
            var rotate = new RotateTransform(-yaw, cx, cy);
            dc.PushTransform(rotate);

            // 主刻度 N/E/S/W
            var directions = new[] { ("N", 0.0), ("E", 90.0), ("S", 180.0), ("W", 270.0) };
            foreach (var (label, deg) in directions)
            {
                double rad = (deg - 90) * Math.PI / 180;
                double tx = cx + Math.Cos(rad) * (r - 12);
                double ty = cy + Math.Sin(rad) * (r - 12);
                var fmt = new FormattedText(label, CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight, FontFace, 10, TextMuted, 1);
                dc.DrawText(fmt, new Point(tx - fmt.Width / 2, ty - fmt.Height / 2));
            }

            // 小刻度线（每 30°）
            for (int i = 0; i < 12; i++)
            {
                double deg = i * 30;
                double rad = (deg - 90) * Math.PI / 180;
                double x1 = cx + Math.Cos(rad) * (r - 4);
                double y1 = cy + Math.Sin(rad) * (r - 4);
                double x2 = cx + Math.Cos(rad) * r;
                double y2 = cy + Math.Sin(rad) * r;
                dc.DrawLine(BorderPen, new Point(x1, y1), new Point(x2, y2));
            }

            dc.Pop();

            // 顶部固定指针（红色三角）
            var pointer = new PathGeometry();
            var fig = new PathFigure { StartPoint = new Point(cx, cy - r + 2) };
            fig.Segments.Add(new LineSegment(new Point(cx - 5, cy - r + 10), true));
            fig.Segments.Add(new LineSegment(new Point(cx + 5, cy - r + 10), true));
            fig.IsClosed = true;
            pointer.Figures.Add(fig);
            dc.DrawGeometry(Brushes.OrangeRed, null, pointer);

            // 中心数值
            var text = new FormattedText($"{yaw:F0}°", CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight, FontFace, 12, TextPrimary, 1);
            dc.DrawText(text, new Point(cx - text.Width / 2, cy - text.Height / 2));
        }

        private static double ParseAngle(string s)
        {
            return double.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture, out double v) ? v : 0;
        }

        private static readonly Brush BgSecondary = new SolidColorBrush(Color.FromRgb(0x1E, 0x1E, 0x22));
        private static readonly Brush TextMuted = new SolidColorBrush(Color.FromRgb(0x6E, 0x6E, 0x78));
        private static readonly Brush TextPrimary = new SolidColorBrush(Color.FromRgb(0xF0, 0xF0, 0xF5));
        private static readonly Pen BorderPen = new(new SolidColorBrush(Color.FromRgb(0x3A, 0x3A, 0x40)), 1);
        private static readonly Typeface FontFace = new("Microsoft YaHei UI, Segoe UI, sans-serif");
    }

    /// <summary>
    /// 横滚仪表：显示横滚角（ROLL）。
    /// 上半圆弧带刻度，内部地平线（天蓝/棕）随 ROLL 倾斜，顶部固定三角标指示当前倾斜量。
    /// </summary>
    public class RollGauge : Control
    {
        static RollGauge()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RollGauge),
                new FrameworkPropertyMetadata(typeof(RollGauge)));
        }

        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.Register(nameof(Angle), typeof(string), typeof(RollGauge),
                new FrameworkPropertyMetadata("0", FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>横滚角度字符串（-90 到 90）</summary>
        public string Angle
        {
            get => (string)GetValue(AngleProperty);
            set => SetValue(AngleProperty, value);
        }

        protected override void OnRender(DrawingContext dc)
        {
            double roll = ParseAngle(Angle);
            double size = Math.Min(ActualWidth, ActualHeight);
            double cx = ActualWidth / 2;
            double cy = ActualHeight / 2;
            double r = size / 2 - 2;

            // 圆形裁剪：确保旋转的地平线矩形不会超出圆形边界
            var clip = new EllipseGeometry(new Point(cx, cy), r, r);
            dc.PushClip(clip);

            // 外圈背景
            dc.DrawEllipse(BgSecondary, null, new Point(cx, cy), r, r);

            // 地平线区域（随 ROLL 旋转）
            var rotate = new RotateTransform(roll, cx, cy);
            dc.PushTransform(rotate);

            // 上半天蓝色（画一个足够大的矩形，旋转后仍能覆盖圆形上半部）
            dc.DrawRectangle(SkyBrush, null, new Rect(cx - r * 2, cy - r * 2, r * 4, r * 2));
            // 下半棕色
            dc.DrawRectangle(GroundBrush, null, new Rect(cx - r * 2, cy, r * 4, r * 2));
            // 地平线
            dc.DrawLine(HorizonPen, new Point(cx - r * 2, cy), new Point(cx + r * 2, cy));

            // 横滚刻度（每 30°）
            for (int i = -2; i <= 2; i++)
            {
                double deg = i * 30;
                double rad = (deg - 90) * Math.PI / 180;
                double x1 = cx + Math.Cos(rad) * (r - 6);
                double y1 = cy + Math.Sin(rad) * (r - 6);
                double x2 = cx + Math.Cos(rad) * r;
                double y2 = cy + Math.Sin(rad) * r;
                dc.DrawLine(BorderPen, new Point(x1, y1), new Point(x2, y2));
            }

            dc.Pop(); // 退出旋转
            dc.Pop(); // 退出裁剪

            // 外圈边框（在裁剪外画，保证边框完整）
            dc.DrawEllipse(null, BorderPen, new Point(cx, cy), r, r);

            // 顶部固定三角标（橙色）
            var pointer = new PathGeometry();
            var fig = new PathFigure { StartPoint = new Point(cx, cy - r + 2) };
            fig.Segments.Add(new LineSegment(new Point(cx - 5, cy - r + 10), true));
            fig.Segments.Add(new LineSegment(new Point(cx + 5, cy - r + 10), true));
            fig.IsClosed = true;
            pointer.Figures.Add(fig);
            dc.DrawGeometry(Brushes.OrangeRed, null, pointer);

            // 中心数值
            var text = new FormattedText($"{roll:F0}°", CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight, FontFace, 12, TextPrimary, 1);
            dc.DrawText(text, new Point(cx - text.Width / 2, cy - text.Height / 2));
        }

        private static double ParseAngle(string s)
        {
            return double.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture, out double v) ? v : 0;
        }

        private static readonly Brush BgSecondary = new SolidColorBrush(Color.FromRgb(0x1E, 0x1E, 0x22));
        private static readonly Brush TextPrimary = new SolidColorBrush(Color.FromRgb(0xF0, 0xF0, 0xF5));
        private static readonly Brush SkyBrush = new SolidColorBrush(Color.FromRgb(0x1A, 0x5C, 0x8A));
        private static readonly Brush GroundBrush = new SolidColorBrush(Color.FromRgb(0x6B, 0x4A, 0x2A));
        private static readonly Pen BorderPen = new(new SolidColorBrush(Color.FromRgb(0x3A, 0x3A, 0x40)), 1);
        private static readonly Pen HorizonPen = new(Brushes.White, 1.5);
        private static readonly Typeface FontFace = new("Microsoft YaHei UI, Segoe UI, sans-serif");
    }

    /// <summary>
    /// 俯仰仪表：显示俯仰角（PITCH）。
    /// 圆形窗口裁剪，内部地平线随 PITCH 上下平移，中心固定水平参考线 + 刻度。
    /// </summary>
    public class PitchGauge : Control
    {
        static PitchGauge()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PitchGauge),
                new FrameworkPropertyMetadata(typeof(PitchGauge)));
        }

        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.Register(nameof(Angle), typeof(string), typeof(PitchGauge),
                new FrameworkPropertyMetadata("0", FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>俯仰角度字符串（-90 到 90）</summary>
        public string Angle
        {
            get => (string)GetValue(AngleProperty);
            set => SetValue(AngleProperty, value);
        }

        protected override void OnRender(DrawingContext dc)
        {
            double pitch = ParseAngle(Angle);
            double size = Math.Min(ActualWidth, ActualHeight);
            double cx = ActualWidth / 2;
            double cy = ActualHeight / 2;
            double r = size / 2 - 2;

            // 圆形裁剪
            var clip = new EllipseGeometry(new Point(cx, cy), r, r);
            dc.PushClip(clip);

            // 地平线偏移：pitch 正值 = 机头上仰 = 地平线下移
            double offset = pitch / 45.0 * r; // 45° 映射到半径
            double horizonY = cy + offset;

            // 上半天蓝色
            dc.DrawRectangle(SkyBrush, null, new Rect(cx - r, cy - r, r * 2, horizonY - (cy - r)));
            // 下半棕色
            dc.DrawRectangle(GroundBrush, null, new Rect(cx - r, horizonY, r * 2, (cy + r) - horizonY));
            // 地平线
            dc.DrawLine(HorizonPen, new Point(cx - r, horizonY), new Point(cx + r, horizonY));

            // 俯仰刻度（每 10°）
            for (int i = -4; i <= 4; i++)
            {
                if (i == 0) continue;
                double tickY = cy + i * 10 / 45.0 * r + offset;
                double tickLen = (i % 2 == 0) ? 20 : 10;
                dc.DrawLine(TickPen, new Point(cx - tickLen, tickY), new Point(cx + tickLen, tickY));
            }

            dc.Pop();

            // 外圈
            dc.DrawEllipse(null, BorderPen, new Point(cx, cy), r, r);

            // 中心固定参考线（黄色，带翼形标记）
            dc.DrawLine(ReferencePen, new Point(cx - r + 8, cy), new Point(cx - 12, cy));
            dc.DrawLine(ReferencePen, new Point(cx + 12, cy), new Point(cx + r - 8, cy));
            // 中心圆点
            dc.DrawEllipse(Brushes.OrangeRed, null, new Point(cx, cy), 3, 3);

            // 底部数值
            var text = new FormattedText($"{pitch:F0}°", CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight, FontFace, 12, TextPrimary, 1);
            dc.DrawText(text, new Point(cx - text.Width / 2, cy + r - text.Height - 4));
        }

        private static double ParseAngle(string s)
        {
            return double.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture, out double v) ? v : 0;
        }

        private static readonly Brush SkyBrush = new SolidColorBrush(Color.FromRgb(0x1A, 0x5C, 0x8A));
        private static readonly Brush GroundBrush = new SolidColorBrush(Color.FromRgb(0x6B, 0x4A, 0x2A));
        private static readonly Brush TextPrimary = new SolidColorBrush(Color.FromRgb(0xF0, 0xF0, 0xF5));
        private static readonly Pen BorderPen = new(new SolidColorBrush(Color.FromRgb(0x3A, 0x3A, 0x40)), 1);
        private static readonly Pen HorizonPen = new(Brushes.White, 1.5);
        private static readonly Pen TickPen = new(new SolidColorBrush(Color.FromRgb(0xB0, 0xB0, 0xB8)), 1);
        private static readonly Pen ReferencePen = new(Brushes.OrangeRed, 2);
        private static readonly Typeface FontFace = new("Microsoft YaHei UI, Segoe UI, sans-serif");
    }
}
