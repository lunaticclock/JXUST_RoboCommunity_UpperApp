using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace UpperApp.UI
{
    // ================================================================
    //  收发流量曲线：实时显示 Rx/Tx 每秒字节数
    //  - 内部维护 60 秒滑动窗口
    //  - 纵轴自适应：当前最大值 ×1.2
    //  - Rx 绿线 / Tx 蓝线
    //  - VM 每秒调用 PushSample(rxBytes, txBytes) 推入新数据
    // ================================================================

    /// <summary>
    /// 收发流量曲线。显示最近 60 秒的 Rx/Tx 每秒字节数。
    /// </summary>
    public class TrafficChart : Control
    {
        private const int MaxSamples = 60;
        private readonly Queue<int> _rx = new();
        private readonly Queue<int> _tx = new();

        static TrafficChart()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TrafficChart),
                new FrameworkPropertyMetadata(typeof(TrafficChart)));
        }

        /// <summary>推入一秒的流量采样（由 VM 定时器调用）</summary>
        public void PushSample(int rxBytes, int txBytes)
        {
            _rx.Enqueue(rxBytes);
            _tx.Enqueue(txBytes);
            while (_rx.Count > MaxSamples) _rx.Dequeue();
            while (_tx.Count > MaxSamples) _tx.Dequeue();
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc)
        {
            double w = ActualWidth;
            double h = ActualHeight;
            double padL = 32, padR = 6, padT = 6, padB = 12;
            double chartW = w - padL - padR;
            double chartH = h - padT - padB;

            // 背景
            dc.DrawRectangle(BgBrush, null, new Rect(0, 0, w, h));

            // 求 Y 轴最大值（自适应）
            int maxVal = 0;
            foreach (var v in _rx) maxVal = Math.Max(maxVal, v);
            foreach (var v in _tx) maxVal = Math.Max(maxVal, v);
            if (maxVal == 0) maxVal = 100;
            double yMax = maxVal * 1.2;

            // 网格线 + Y 轴标签（4 条）
            for (int i = 0; i <= 4; i++)
            {
                double y = padT + chartH * i / 4;
                dc.DrawLine(GridPen, new Point(padL, y), new Point(padL + chartW, y));
                double labelVal = yMax * (4 - i) / 4;
                string label = FormatBytes(labelVal);
                var fmt = new FormattedText(label, CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight, FontFace, 8, TextMuted, 1);
                dc.DrawText(fmt, new Point(padL - fmt.Width - 2, y - fmt.Height / 2));
            }

            // X 轴标签
            var xLabel = new FormattedText("60s", CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight, FontFace, 8, TextMuted, 1);
            dc.DrawText(xLabel, new Point(padL, h - padB + 2));

            // 画 Rx 曲线（绿）
            if (_rx.Count > 1)
            {
                var geo = BuildPolyline(_rx, padL, padT, chartW, chartH, yMax, MaxSamples);
                dc.DrawGeometry(null, RxPen, geo);
            }

            // 画 Tx 曲线（蓝）
            if (_tx.Count > 1)
            {
                var geo = BuildPolyline(_tx, padL, padT, chartW, chartH, yMax, MaxSamples);
                dc.DrawGeometry(null, TxPen, geo);
            }

            // 图例
            DrawLegend(dc, w - padR - 70, padT + 2);
        }

        private void DrawLegend(DrawingContext dc, double x, double y)
        {
            // Rx
            dc.DrawLine(RxPen, new Point(x, y + 4), new Point(x + 12, y + 4));
            var rxLabel = new FormattedText("Rx", CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight, FontFace, 9, TextPrimary, 1);
            dc.DrawText(rxLabel, new Point(x + 16, y));
            // Tx
            dc.DrawLine(TxPen, new Point(x + 36, y + 4), new Point(x + 48, y + 4));
            var txLabel = new FormattedText("Tx", CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight, FontFace, 9, TextPrimary, 1);
            dc.DrawText(txLabel, new Point(x + 52, y));
        }

        private static PathGeometry BuildPolyline(Queue<int> data, double padL, double padT,
            double chartW, double chartH, double yMax, int maxSamples)
        {
            var geo = new PathGeometry();
            var fig = new PathFigure();
            bool first = true;
            int count = data.Count;
            int idx = 0;
            foreach (var v in data)
            {
                double x = padL + chartW * idx / (maxSamples - 1);
                double y = padT + chartH * (1 - v / yMax);
                if (first) { fig.StartPoint = new Point(x, y); first = false; }
                else fig.Segments.Add(new LineSegment(new Point(x, y), true));
                idx++;
            }
            // 如果数据不足 maxSamples，最后补到右边界
            if (count > 0 && count < maxSamples)
            {
                double x = padL + chartW;
                double y = padT + chartH * (1 - 0 / yMax);
                fig.Segments.Add(new LineSegment(new Point(x, y), true));
            }
            geo.Figures.Add(fig);
            return geo;
        }

        private static string FormatBytes(double v)
        {
            if (v >= 1024) return (v / 1024).ToString("F1") + "K";
            return v.ToString("F0");
        }

        private static readonly Brush BgBrush = new SolidColorBrush(Color.FromRgb(0x16, 0x16, 0x1A));
        private static readonly Brush TextPrimary = new SolidColorBrush(Color.FromRgb(0xF0, 0xF0, 0xF5));
        private static readonly Brush TextMuted = new SolidColorBrush(Color.FromRgb(0x6E, 0x6E, 0x78));
        private static readonly Pen GridPen = new(new SolidColorBrush(Color.FromRgb(0x2A, 0x2A, 0x30)), 1);
        private static readonly Pen RxPen = new(new SolidColorBrush(Color.FromRgb(0x00, 0xD4, 0xAA)), 1.5);
        private static readonly Pen TxPen = new(new SolidColorBrush(Color.FromRgb(0x4A, 0x9E, 0xFF)), 1.5);
        private static readonly Typeface FontFace = new("Microsoft YaHei UI, Segoe UI, sans-serif");
    }
}
