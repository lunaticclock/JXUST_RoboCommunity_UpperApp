using System.Windows;
using System.Windows.Controls;

namespace UpperApp.UI
{
    /// <summary>
    /// 指标显示项: 标签 + 值。统一姿态监控、速度方向等显示样式。
    /// </summary>
    public partial class MetricItem : UserControl
    {
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label), typeof(string), typeof(MetricItem),
                new PropertyMetadata(""));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(object), typeof(MetricItem),
                new PropertyMetadata(""));

        public static readonly DependencyProperty ValueFontSizeProperty =
            DependencyProperty.Register(nameof(ValueFontSize), typeof(double), typeof(MetricItem),
                new PropertyMetadata(18.0));

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

        public double ValueFontSize
        {
            get => (double)GetValue(ValueFontSizeProperty);
            set => SetValue(ValueFontSizeProperty, value);
        }

        public MetricItem()
        {
            InitializeComponent();
        }
    }
}
