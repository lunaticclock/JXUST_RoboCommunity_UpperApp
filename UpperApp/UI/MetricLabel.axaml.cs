using Avalonia;
using Avalonia.Controls;

namespace UpperApp.UI
{
    /// <summary>
    /// 小型指标标签: 灰色标签 + 等宽值。用于行走路线等紧凑信息显示。
    /// </summary>
    public partial class MetricLabel : UserControl
    {
        public static readonly StyledProperty<string> LabelProperty =
            AvaloniaProperty.Register<MetricLabel, string>(nameof(Label), "");

        public static readonly StyledProperty<object> ValueProperty =
            AvaloniaProperty.Register<MetricLabel, object>(nameof(Value), "");

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

        public MetricLabel()
        {
            InitializeComponent();
        }
    }
}
