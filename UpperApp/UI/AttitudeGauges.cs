using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace UpperApp.UI
{
    // ================================================================
    //  姿态仪表控件
    //  - CompassGauge       : 罗盘，刻度盘随 YAW 反向旋转，顶部固定指针
    //  - AttitudeIndicator  : 姿态仪，地平线随 ROLL 旋转 + PITCH 平移（合并横滚俯仰）
    //  - DistanceGauge      : 航程仪表盘（独立文件）
    //
    //  动画方案：指数衰减（Lerp）平滑过渡
    //  - 每帧 current += (target - current) * LerpFactor
    //  - LerpFactor=0.08，60fps 下约 400ms 到达 90%，覆盖 2.5Hz 数据间隔
    //  - 数据频繁更新无冲突，目标变了就朝新目标趋近
    // ================================================================

    /// <summary>
    /// 独立动画值：封装单个值的指数衰减平滑过渡。
    /// 可被任意控件持有多个实例（如 AttitudeIndicator 同时动画 Roll 和 Pitch）。
    /// </summary>
    public sealed class AnimatedValue
    {
        private double _current;
        private double _target;
        private bool _animating;
        private readonly Action _invalidate;
        private readonly double _lerpFactor;
        private readonly double _epsilon;
        private readonly bool _shortestPath;

        /// <param name="invalidate">每帧动画推进时调用的重绘回调</param>
        /// <param name="lerpFactor">每帧逼近比例，0.08=慢平滑，0.15=中，0.2=快</param>
        /// <param name="shortestPath">是否走最短路径（YAW 这种 0-360 循环值）</param>
        public AnimatedValue(Action invalidate, double lerpFactor = 0.08, double epsilon = 0.02, bool shortestPath = false)
        {
            _invalidate = invalidate;
            _lerpFactor = lerpFactor;
            _epsilon = epsilon;
            _shortestPath = shortestPath;
        }

        public double Current => _current;

        public void SetTarget(double value)
        {
            _target = _shortestPath ? NormalizeAngle(value) : value;
            if (!_animating && _current != _target)
            {
                _animating = true;
                CompositionTarget.Rendering += OnRendering;
            }
        }

        private void OnRendering(object sender, EventArgs e)
        {
            double delta = _target - _current;
            if (_shortestPath)
            {
                if (delta > 180) delta -= 360;
                else if (delta < -180) delta += 360;
            }

            if (Math.Abs(delta) < _epsilon)
            {
                _current = _target;
                _animating = false;
                CompositionTarget.Rendering -= OnRendering;
            }
            else
            {
                _current += delta * _lerpFactor;
                if (_shortestPath) _current = NormalizeAngle(_current);
            }
            _invalidate();
        }

        private static double NormalizeAngle(double a) => ((a % 360) + 360) % 360;
    }

    /// <summary>
    /// FormattedText 缓存：避免每帧重建导致 GC 压力。
    /// 相同文字复用同一对象，文字变化时才重建。
    /// </summary>
    public sealed class TextCache
    {
        private string _key = "";
        private FormattedText? _ft;
        private readonly Typeface _face;

        public TextCache(Typeface face) => _face = face;

        public FormattedText Get(string text, double size, Brush brush)
        {
            if (_ft != null && _key == text) return _ft;
            _key = text;
            _ft = new FormattedText(text, CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight, _face, size, brush, 1);
            return _ft;
        }
    }

    /// <summary>
    /// 仪表动画基类：单值动画，子类通过 CurrentValue 读取动画值。
    /// </summary>
    public abstract class AnimatedGaugeBase : Control
    {
        private readonly AnimatedValue _value;

        protected AnimatedGaugeBase()
        {
            _value = new AnimatedValue(InvalidateVisual, LerpFactor, Epsilon, UseShortestPath);
        }

        /// <summary>每帧逼近目标的比例。0.08=慢平滑，0.15=中，0.2=快。</summary>
        protected virtual double LerpFactor => 0.08;

        /// <summary>停止动画的最小差值阈值。</summary>
        protected virtual double Epsilon => 0.02;

        /// <summary>是否走最短路径（用于 YAW 这种 0-360 循环值）。</summary>
        protected virtual bool UseShortestPath => false;

        /// <summary>当前显示值（动画驱动）。</summary>
        protected double CurrentValue => _value.Current;

        /// <summary>设置目标值并启动动画。</summary>
        protected void SetTargetValue(double value) => _value.SetTarget(value);
    }

    /// <summary>
    /// 罗盘仪表：显示偏航角（YAW）。
    /// 刻度盘随偏航角反向旋转（飞机朝东，E 转到顶部），顶部红色指针固定指向当前航向。
    /// </summary>
    public class CompassGauge : AnimatedGaugeBase
    {
        static CompassGauge()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CompassGauge),
                new FrameworkPropertyMetadata(typeof(CompassGauge)));
        }

        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.Register(nameof(Angle), typeof(string), typeof(CompassGauge),
                new FrameworkPropertyMetadata("0", OnAngleChanged));

        /// <summary>偏航角度字符串（0-360）</summary>
        public string Angle
        {
            get => (string)GetValue(AngleProperty);
            set => SetValue(AngleProperty, value);
        }

        private static void OnAngleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CompassGauge g)
                g.SetTargetValue(ParseAngle((string)e.NewValue));
        }

        // YAW 是 0-360 循环值，走最短路径
        protected override bool UseShortestPath => true;

        // FormattedText 缓存：避免每帧重建导致 GC 压力
        private readonly TextCache _dirN = new(FontFace);
        private readonly TextCache _dirE = new(FontFace);
        private readonly TextCache _dirS = new(FontFace);
        private readonly TextCache _dirW = new(FontFace);
        private readonly TextCache _valText = new(FontFace);
        // 指针 Geometry 缓存（r 不变时复用）
        private double _lastR = -1;
        private Geometry _pointerGeo = new PathGeometry();

        protected override void OnRender(DrawingContext dc)
        {
            double yaw = CurrentValue;
            double size = Math.Min(ActualWidth, ActualHeight);
            double cx = ActualWidth / 2;
            double cy = ActualHeight / 2;
            double r = size / 2 - 2;

            // 外圈
            dc.DrawEllipse(BgSecondary, BorderPen, new Point(cx, cy), r, r);

            // 刻度盘（随 YAW 反向旋转）
            var rotate = new RotateTransform(-yaw, cx, cy);
            dc.PushTransform(rotate);

            // 主刻度 N/E/S/W（使用缓存的 FormattedText）
            var labels = new[] { ("N", _dirN, 0.0), ("E", _dirE, 90.0), ("S", _dirS, 180.0), ("W", _dirW, 270.0) };
            foreach (var (label, cache, deg) in labels)
            {
                double rad = (deg - 90) * Math.PI / 180;
                double tx = cx + Math.Cos(rad) * (r - 12);
                double ty = cy + Math.Sin(rad) * (r - 12);
                var fmt = cache.Get(label, 10, TextMuted);
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

            // 顶部固定指针（红色三角，使用缓存的 Geometry）
            dc.DrawGeometry(Brushes.OrangeRed, null, GetPointer(cx, cy, r));

            // 中心数值（缓存）
            var text = _valText.Get($"{yaw:F0}°", 12, TextPrimary);
            dc.DrawText(text, new Point(cx - text.Width / 2, cy - text.Height / 2));
        }

        private Geometry GetPointer(double cx, double cy, double r)
        {
            if (Math.Abs(r - _lastR) < 0.5) return _pointerGeo;
            _lastR = r;
            var pointer = new PathGeometry();
            var fig = new PathFigure { StartPoint = new Point(cx, cy - r + 2) };
            fig.Segments.Add(new LineSegment(new Point(cx - 5, cy - r + 10), true));
            fig.Segments.Add(new LineSegment(new Point(cx + 5, cy - r + 10), true));
            fig.IsClosed = true;
            pointer.Figures.Add(fig);
            _pointerGeo = pointer;
            return _pointerGeo;
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
    /// 姿态仪：合并横滚 + 俯仰，模拟真实飞机 Artificial Horizon。
    /// 地平线随 ROLL 旋转、随 PITCH 上下平移，顶部固定三角标指示横滚，中心固定参考线指示俯仰。
    /// </summary>
    public class AttitudeIndicator : Control
    {
        private static readonly Typeface FontFace = new("Microsoft YaHei UI, Segoe UI, sans-serif");

        static AttitudeIndicator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AttitudeIndicator),
                new FrameworkPropertyMetadata(typeof(AttitudeIndicator)));
        }

        public static readonly DependencyProperty RollProperty =
            DependencyProperty.Register(nameof(Roll), typeof(string), typeof(AttitudeIndicator),
                new FrameworkPropertyMetadata("0", OnRollChanged));

        public static readonly DependencyProperty PitchProperty =
            DependencyProperty.Register(nameof(Pitch), typeof(string), typeof(AttitudeIndicator),
                new FrameworkPropertyMetadata("0", OnPitchChanged));

        /// <summary>横滚角度字符串（-90 到 90）</summary>
        public string Roll
        {
            get => (string)GetValue(RollProperty);
            set => SetValue(RollProperty, value);
        }

        /// <summary>俯仰角度字符串（-90 到 90）</summary>
        public string Pitch
        {
            get => (string)GetValue(PitchProperty);
            set => SetValue(PitchProperty, value);
        }

        private static void OnRollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AttitudeIndicator g)
                g._rollValue.SetTarget(ParseAngle((string)e.NewValue));
        }

        private static void OnPitchChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AttitudeIndicator g)
                g._pitchValue.SetTarget(ParseAngle((string)e.NewValue));
        }

        // 两个独立的动画值（Roll 和 Pitch 各自平滑过渡）
        private readonly AnimatedValue _rollValue;
        private readonly AnimatedValue _pitchValue;

        public AttitudeIndicator()
        {
            _rollValue = new AnimatedValue(InvalidateVisual, lerpFactor: 0.08);
            _pitchValue = new AnimatedValue(InvalidateVisual, lerpFactor: 0.08);
        }

        // FormattedText / Geometry 缓存
        private readonly TextCache _rollText = new(FontFace);
        private readonly TextCache _pitchText = new(FontFace);
        private double _lastR = -1;
        private EllipseGeometry _clipGeo = new();
        private Geometry _rollPointer = new PathGeometry();

        protected override void OnRender(DrawingContext dc)
        {
            double roll = _rollValue.Current;
            double pitch = _pitchValue.Current;
            double size = Math.Min(ActualWidth, ActualHeight);
            double cx = ActualWidth / 2;
            double cy = ActualHeight / 2;
            double r = size / 2 - 2;

            // 圆形裁剪（缓存）
            if (Math.Abs(r - _lastR) >= 0.5)
            {
                _lastR = r;
                _clipGeo = new EllipseGeometry(new Point(cx, cy), r, r);
                RebuildRollPointer(cx, cy, r);
            }
            dc.PushClip(_clipGeo);

            // 外圈背景
            dc.DrawEllipse(BgSecondary, null, new Point(cx, cy), r, r);

            // 地平线区域：先按 ROLL 旋转，再按 PITCH 平移
            var rotate = new RotateTransform(roll, cx, cy);
            dc.PushTransform(rotate);

            // PITCH 偏移：正值=机头上仰=地平线下移
            double offset = pitch / 45.0 * r;
            double horizonY = cy + offset;

            // 上半天蓝色（足够大的矩形，旋转后仍覆盖圆形）
            dc.DrawRectangle(SkyBrush, null, new Rect(cx - r * 2, cy - r * 2 + offset, r * 4, r * 2));
            // 下半棕色
            dc.DrawRectangle(GroundBrush, null, new Rect(cx - r * 2, horizonY, r * 4, r * 2));
            // 地平线
            dc.DrawLine(HorizonPen, new Point(cx - r * 2, horizonY), new Point(cx + r * 2, horizonY));

            // 俯仰刻度（每 10°，跟随地平线平移）
            for (int i = -4; i <= 4; i++)
            {
                if (i == 0) continue;
                double tickY = cy + i * 10 / 45.0 * r + offset;
                double tickLen = (i % 2 == 0) ? 18 : 9;
                dc.DrawLine(TickPen, new Point(cx - tickLen, tickY), new Point(cx + tickLen, tickY));
            }

            dc.Pop(); // 退出旋转
            dc.Pop(); // 退出裁剪

            // 外圈边框
            dc.DrawEllipse(null, BorderPen, new Point(cx, cy), r, r);

            // 横滚刻度（圆弧上，每 30°）
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

            // 顶部固定三角标（橙色，指示横滚）
            dc.DrawGeometry(Brushes.OrangeRed, null, _rollPointer);

            // 中心固定参考线（俯仰指示，橙色翼形）
            dc.DrawLine(ReferencePen, new Point(cx - r + 8, cy), new Point(cx - 14, cy));
            dc.DrawLine(ReferencePen, new Point(cx + 14, cy), new Point(cx + r - 8, cy));
            // 中心圆点
            dc.DrawEllipse(Brushes.OrangeRed, null, new Point(cx, cy), 3, 3);

            // 数值：左上 Roll，右上 Pitch（缓存）
            var rt = _rollText.Get($"R:{roll:F0}°", 10, TextPrimary);
            dc.DrawText(rt, new Point(cx - r + 4, cy - r + 4));
            var pt = _pitchText.Get($"P:{pitch:F0}°", 10, TextPrimary);
            dc.DrawText(pt, new Point(cx + r - 4 - pt.Width, cy - r + 4));
        }

        private void RebuildRollPointer(double cx, double cy, double r)
        {
            var pointer = new PathGeometry();
            var fig = new PathFigure { StartPoint = new Point(cx, cy - r + 2) };
            fig.Segments.Add(new LineSegment(new Point(cx - 5, cy - r + 10), true));
            fig.Segments.Add(new LineSegment(new Point(cx + 5, cy - r + 10), true));
            fig.IsClosed = true;
            pointer.Figures.Add(fig);
            _rollPointer = pointer;
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
        private static readonly Pen TickPen = new(new SolidColorBrush(Color.FromRgb(0xB0, 0xB0, 0xB8)), 1);
        private static readonly Pen ReferencePen = new(Brushes.OrangeRed, 2);
    }
}
