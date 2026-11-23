using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace UpperApp.UI
{
    /// <summary>
    /// 虚拟摇杆控件。拖动圆点或键盘方向键/WASD控制速度和方向。
    /// X 轴: 方向 (0=左, 50=中, 100=右)
    /// Y 轴: 速度 (0=后, 50=停, 100=前)
    /// 松手/松键自动回中并发送停止命令。
    /// </summary>
    public class JoystickControl : TemplatedControl
    {
        #region 依赖属性

        public static readonly StyledProperty<int> SpeedProperty =
            AvaloniaProperty.Register<JoystickControl, int>(nameof(Speed), 50,
                defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        public static readonly StyledProperty<int> DirectionProperty =
            AvaloniaProperty.Register<JoystickControl, int>(nameof(Direction), 50,
                defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        public static readonly StyledProperty<bool> IsActiveProperty =
            AvaloniaProperty.Register<JoystickControl, bool>(nameof(IsActive),
                defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        /// <summary>速度 0-100, 50=停止</summary>
        public int Speed
        {
            get => GetValue(SpeedProperty);
            set => SetValue(SpeedProperty, value);
        }

        /// <summary>方向 0-100, 50=居中</summary>
        public int Direction
        {
            get => GetValue(DirectionProperty);
            set => SetValue(DirectionProperty, value);
        }

        /// <summary>摇杆是否激活（正在拖拽或按键）</summary>
        public bool IsActive
        {
            get => GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        #endregion

        private const double ThumbRadius = 22;
        private double _baseRadius;
        private Ellipse _thumb;
        private bool _isDragging;

        // 键盘控制
        private readonly HashSet<Key> _pressedKeys = [];

        static JoystickControl()
        {
            SpeedProperty.Changed.AddClassHandler<JoystickControl>((c, e) => c.OnValueChanged(e));
            DirectionProperty.Changed.AddClassHandler<JoystickControl>((c, e) => c.OnValueChanged(e));
        }

        public JoystickControl()
        {
            Focusable = true;
            LostFocus += OnLostFocus;
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            Focus();
            e.Handled = true;
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            _thumb = e.NameScope.Find("PART_Thumb") as Ellipse;
            if (_thumb != null)
            {
                _thumb.PointerPressed += Thumb_PointerPressed;
                _thumb.PointerMoved += Thumb_PointerMoved;
                _thumb.PointerReleased += Thumb_PointerReleased;
            }
            UpdateThumbPosition();
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            var size = base.ArrangeOverride(arrangeBounds);
            _baseRadius = (Math.Min(size.Width, size.Height) - ThumbRadius * 2) / 2;
            UpdateThumbPosition(size);
            return size;
        }

        #region 鼠标拖拽

        private void Thumb_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            _isDragging = true;
            IsActive = true;
            e.Pointer.Capture(_thumb);
            e.Handled = true;
        }

        private void Thumb_PointerMoved(object sender, PointerEventArgs e)
        {
            if (!_isDragging) return;

            var pos = e.GetPosition(this);
            var cx = Bounds.Width / 2;
            var cy = Bounds.Height / 2;

            var dx = pos.X - cx;
            var dy = pos.Y - cy;
            var dist = Math.Sqrt(dx * dx + dy * dy);

            // 限制在基座圆内
            if (dist > _baseRadius)
            {
                dx = dx / dist * _baseRadius;
                dy = dy / dist * _baseRadius;
            }

            // 移动 thumb
            Canvas.SetLeft(_thumb, cx + dx - ThumbRadius);
            Canvas.SetTop(_thumb, cy + dy - ThumbRadius);

            // 死区: 中心 ±5% 范围内视为停止，避免微小抖动
            const double DeadZoneRatio = 0.05;
            double deadZone = _baseRadius * DeadZoneRatio;
            if (Math.Abs(dx) < deadZone) dx = 0;
            if (Math.Abs(dy) < deadZone) dy = 0;

            // 映射到 0-100, 中点 50
            // Y 轴反转: 向上 = 速度增大
            int newSpeed = (int)Math.Round(50 - dy / _baseRadius * 50);
            int newDir = (int)Math.Round(50 + dx / _baseRadius * 50);

            // 钳位
            newSpeed = Math.Clamp(newSpeed, 0, 100);
            newDir = Math.Clamp(newDir, 0, 100);

            // 批量更新：只有值真正变化时才设属性，减少绑定通知次数
            if (Speed != newSpeed) Speed = newSpeed;
            if (Direction != newDir) Direction = newDir;
        }

        private void Thumb_PointerReleased(object sender, PointerReleasedEventArgs e)
        {
            ReleaseJoystick();
            e.Handled = true;
        }

        private void ReleaseJoystick()
        {
            if (!_isDragging) return;
            _isDragging = false;
            IsActive = false;

            // Avalonia 中 pointer capture 在 release 时自动释放

            // 松手回中
            Speed = 50;
            Direction = 50;
            UpdateThumbPosition();
        }

        #endregion

        #region 键盘控制

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (_isDragging) { base.OnKeyDown(e); return; }

            var key = NormalizeKey(e.Key);
            if (key == Key.None) { base.OnKeyDown(e); return; }

            _pressedKeys.Add(key);
            ApplyKeyboardState();

            e.Handled = true;
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            var key = NormalizeKey(e.Key);
            if (key == Key.None) { base.OnKeyUp(e); return; }

            _pressedKeys.Remove(key);
            ApplyKeyboardState();

            e.Handled = true;
            base.OnKeyUp(e);
        }

        private void OnLostFocus(object? sender, RoutedEventArgs e)
        {
            if (_pressedKeys.Count > 0)
            {
                _pressedKeys.Clear();
                ApplyKeyboardState();
            }
        }

        private static Key NormalizeKey(Key key)
        {
            return key switch
            {
                Key.Up or Key.W => Key.Up,
                Key.Down or Key.S => Key.Down,
                Key.Left or Key.A => Key.Left,
                Key.Right or Key.D => Key.Right,
                _ => Key.None
            };
        }

        /// <summary>
        /// 根据当前按下的键直接设置摇杆位置。
        /// 按下=对应方向最大值，松开=回中。
        /// </summary>
        private void ApplyKeyboardState()
        {
            if (_pressedKeys.Count == 0)
            {
                IsActive = false;
                Speed = 50;
                Direction = 50;
            }
            else
            {
                IsActive = true;

                // 速度: 上=100, 下=0, 都按或都不按=50
                if (_pressedKeys.Contains(Key.Up) && !_pressedKeys.Contains(Key.Down))
                    Speed = 100;
                else if (_pressedKeys.Contains(Key.Down) && !_pressedKeys.Contains(Key.Up))
                    Speed = 0;
                else
                    Speed = 50;

                // 方向: 左=0, 右=100, 都按或都不按=50
                if (_pressedKeys.Contains(Key.Left) && !_pressedKeys.Contains(Key.Right))
                    Direction = 0;
                else if (_pressedKeys.Contains(Key.Right) && !_pressedKeys.Contains(Key.Left))
                    Direction = 100;
                else
                    Direction = 50;
            }

            UpdateThumbPosition();
        }

        #endregion

        private void OnValueChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (!_isDragging && _pressedKeys.Count == 0)
                UpdateThumbPosition();
        }

        private void UpdateThumbPosition(Size? size = null)
        {
            if (_thumb == null || _baseRadius <= 0) return;

            // 优先使用传入的尺寸（ArrangeOverride 期间 Bounds.Width 还未更新）
            var w = size?.Width ?? Bounds.Width;
            var h = size?.Height ?? Bounds.Height;
            var cx = w / 2;
            var cy = h / 2;

            // 0-100 → -1~1
            var dx = (Direction - 50) / 50.0 * _baseRadius;
            var dy = (50 - Speed) / 50.0 * _baseRadius;

            Canvas.SetLeft(_thumb, cx + dx - ThumbRadius);
            Canvas.SetTop(_thumb, cy + dy - ThumbRadius);
        }
    }
}
