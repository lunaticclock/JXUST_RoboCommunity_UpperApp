using Avalonia;
using Avalonia.Controls;

namespace UpperApp.UI
{
    /// <summary>
    /// 指标显示项: 标签 + 值。统一姿态监控、速度方向等显示样式。
    /// </summary>
    public partial class MetricItem : UserControl
    {
        public static readonly StyledProperty<string> LabelProperty =
            AvaloniaProperty.Register<MetricItem, string>(nameof(Label), "");

        public static readonly StyledProperty<object> ValueProperty =
            AvaloniaProperty.Register<MetricItem, object>(nameof(Value), "");

        public static readonly StyledProperty<double> ValueFontSizeProperty =
            AvaloniaProperty.Register<MetricItem, double>(nameof(ValueFontSize), 18.0);

        public string Label
        {
            get => GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public object Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public double ValueFontSize
        {
            get => GetValue(ValueFontSizeProperty);
            set => SetValue(ValueFontSizeProperty, value);
        }

        public MetricItem()
        {
            InitializeComponent();
        }
    }
}
