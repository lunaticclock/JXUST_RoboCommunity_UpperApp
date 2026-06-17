using System.Windows;
using System.Windows.Controls;

namespace UpperApp.UI
{
    /// <summary>
    /// 小型指标标签: 灰色标签 + 等宽值。用于行走路线等紧凑信息显示。
    /// </summary>
    public partial class MetricLabel : UserControl
    {
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label), typeof(string), typeof(MetricLabel),
                new PropertyMetadata(""));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(object), typeof(MetricLabel),
                new PropertyMetadata(""));

        public string Label
        {
            get => (string)GetValue(LabelProperty);
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
