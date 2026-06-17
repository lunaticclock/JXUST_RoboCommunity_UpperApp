using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UpperApp.UI
{
    /// <summary>
    /// 发送行: TextBox + 发送按钮。统一蓝牙服务端/客户端发送框样式。
    /// </summary>
    public partial class SendRow : UserControl
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(SendRow),
                new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty SendCommandProperty =
            DependencyProperty.Register(nameof(SendCommand), typeof(ICommand), typeof(SendRow),
                new PropertyMetadata(null));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public ICommand SendCommand
        {
            get => (ICommand)GetValue(SendCommandProperty);
            set => SetValue(SendCommandProperty, value);
        }

        public SendRow()
        {
            InitializeComponent();
        }
    }
}
