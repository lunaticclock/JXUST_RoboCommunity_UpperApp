using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace UpperApp.UI
{
    // ================================================================
    //  航程仪表盘：半圆弧指针式，显示累计行驶距离
    //  - 自适应量程：当前值超过量程 80% 时自动翻倍扩量程
    //  - 量程档位：1/2/5/10/20/50/100/200/500/1000... 米
    //  - 半圆弧从 -135° 到 +135°（共 270°）
    // ================================================================

    /// <summary>
    /// 航程仪表盘。半圆弧指针式，自适应量程显示累计距离。
    /// </summary>
    public class DistanceGauge : Control
    {
        static DistanceGauge()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DistanceGauge),
                new FrameworkPropertyMetadata(typeof(DistanceGauge)));
        }

        public static readonly DependencyProperty DistanceProperty =
            DependencyProperty.Register(nameof(Distance), typeof(string), typeof(DistanceGauge),
                new FrameworkPropertyMetadata("0", FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>距离字符串（米）</summary>
        public string Distance
        {
            get => (string)GetValue(DistanceProperty);
            set => SetValue(DistanceProperty, value);
        }

        // 自适应量程：当前值超过 80% 时扩容
        private double _range = 10;

        protected override void OnRender(DrawingContext dc)
        {
            double dist = ParseDouble(Distance);
            if (dist < 0) dist = 0;

            // 自适应量程
            while (dist > _range * 0.8) _range *= 2;
            while (_range > 10 && dist < _range * 0.2) _range /= 2;

            double w = ActualWidth;
            double h = ActualHeight;
            double cx = w / 2;
            double cy = h * 0.75; // 圆心偏下，给半圆弧留空间
            double r = Math.Min(w / 2 - 4, h - 8);

            // 半圆弧角度：-135° 到 +135°（共 270°），0° 在正上方
            const double StartAngle = -135;
            const double SweepAngle = 270;

            // 背景弧（灰色）
            DrawArc(dc, cx, cy, r, StartAngle, SweepAngle, TrackBrush, 4);

            // 进度弧（青色）
            double progress = _range > 0 ? Math.Min(dist / _range, 1.0) : 0;
            if (progress > 0)
            {
                DrawArc(dc, cx, cy, r, StartAngle, SweepAngle * progress, ProgressBrush, 4);
            }

            // 刻度
            int tickCount = 5;
            for (int i = 0; i <= tickCount; i++)
            {
                double t = (double)i / tickCount;
                double angle = StartAngle + SweepAngle * t;
                double rad = (angle - 90) * Math.PI / 180;
                double x1 = cx + Math.Cos(rad) * (r - 8);
                double y1 = cy + Math.Sin(rad) * (r - 8);
                double x2 = cx + Math.Cos(rad) * (r - 2);
                double y2 = cy + Math.Sin(rad) * (r - 2);
                dc.DrawLine(TickPen, new Point(x1, y1), new Point(x2, y2));

                // 刻度标签
                double labelVal = _range * t;
                string label = FormatDistance(labelVal);
                double lx = cx + Math.Cos(rad) * (r - 18);
                double ly = cy + Math.Sin(rad) * (r - 18);
                var fmt = new FormattedText(label, CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight, FontFace, 8, TextMuted, 1);
                dc.DrawText(fmt, new Point(lx - fmt.Width / 2, ly - fmt.Height / 2));
            }

            // 指针
            double pointerAngle = StartAngle + SweepAngle * progress;
            double prad = (pointerAngle - 90) * Math.PI / 180;
            double px = cx + Math.Cos(prad) * (r - 4);
            double py = cy + Math.Sin(prad) * (r - 4);
            dc.DrawLine(PointerPen, new Point(cx, cy), new Point(px, py));
            // 指针中心圆
            dc.DrawEllipse(Brushes.OrangeRed, null, new Point(cx, cy), 4, 4);

            // 中心数值
            string distText = FormatDistance(dist);
            var text = new FormattedText(distText, CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight, FontFace, 14, TextPrimary, 1);
            dc.DrawText(text, new Point(cx - text.Width / 2, cy - r * 0.45 - text.Height / 2));

            // 单位
            var unit = new FormattedText("m", CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight, FontFace, 9, TextMuted, 1);
            dc.DrawText(unit, new Point(cx - unit.Width / 2, cy - r * 0.45 + text.Height / 2 - 2));
        }

        private static void DrawArc(DrawingContext dc, double cx, double cy, double r,
            double startAngleDeg, double sweepAngleDeg, Brush brush, double thickness)
        {
            if (Math.Abs(sweepAngleDeg) < 0.1) return;
            double startRad = (startAngleDeg - 90) * Math.PI / 180;
            double endRad = (startAngleDeg + sweepAngleDeg - 90) * Math.PI / 180;
            var start = new Point(cx + Math.Cos(startRad) * r, cy + Math.Sin(startRad) * r);
            var end = new Point(cx + Math.Cos(endRad) * r, cy + Math.Sin(endRad) * r);
            var size = new Size(r, r);
            bool isLargeArc = Math.Abs(sweepAngleDeg) > 180;
            var sweep = sweepAngleDeg > 0 ? SweepDirection.Clockwise : SweepDirection.Counterclockwise;
            var geo = new PathGeometry();
            var fig = new PathFigure { StartPoint = start };
            fig.Segments.Add(new ArcSegment(end, size, 0, isLargeArc, sweep, true));
            geo.Figures.Add(fig);
            dc.DrawGeometry(null, new Pen(brush, thickness), geo);
        }

        private static string FormatDistance(double v)
        {
            if (v >= 1000) return (v / 1000).ToString("F1") + "k";
            if (v >= 100) return v.ToString("F0");
            if (v >= 10) return v.ToString("F1");
            return v.ToString("F2");
        }

        private static double ParseDouble(string s)
        {
            return double.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture, out double v) ? v : 0;
        }

        private static readonly Brush TrackBrush = new SolidColorBrush(Color.FromRgb(0x3A, 0x3A, 0x40));
        private static readonly Brush ProgressBrush = new SolidColorBrush(Color.FromRgb(0x00, 0xD4, 0xAA));
        private static readonly Brush TextPrimary = new SolidColorBrush(Color.FromRgb(0xF0, 0xF0, 0xF5));
        private static readonly Brush TextMuted = new SolidColorBrush(Color.FromRgb(0x6E, 0x6E, 0x78));
        private static readonly Pen TickPen = new(new SolidColorBrush(Color.FromRgb(0x6E, 0x6E, 0x78)), 1);
        private static readonly Pen PointerPen = new(Brushes.OrangeRed, 2);
        private static readonly Typeface FontFace = new("Microsoft YaHei UI, Segoe UI, sans-serif");
    }
}
