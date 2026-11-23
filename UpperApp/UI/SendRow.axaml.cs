using Avalonia;
using Avalonia.Controls;
using System.Windows.Input;

namespace UpperApp.UI
{
    /// <summary>
    /// 发送行: TextBox + 发送按钮。统一蓝牙服务端/客户端发送框样式。
    /// </summary>
    public partial class SendRow : UserControl
    {
        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<SendRow, string>(nameof(Text), "",
                defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        public static readonly StyledProperty<ICommand> SendCommandProperty =
            AvaloniaProperty.Register<SendRow, ICommand>(nameof(SendCommand));

        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public ICommand SendCommand
        {
            get => GetValue(SendCommandProperty);
            set => SetValue(SendCommandProperty, value);
        }

        public SendRow()
        {
            InitializeComponent();
        }
    }
}
