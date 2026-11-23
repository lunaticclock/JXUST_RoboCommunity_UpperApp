using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace UpperApp.UI
{
    /// <summary>
    /// 面板标题栏: 发光圆点 + 标题文字。统一三个面板的标题样式。
    /// </summary>
    public partial class PanelHeader : UserControl
    {
        public static readonly StyledProperty<string> TitleProperty =
            AvaloniaProperty.Register<PanelHeader, string>(nameof(Title), "");

        public static readonly StyledProperty<SolidColorBrush> DotColorProperty =
            AvaloniaProperty.Register<PanelHeader, SolidColorBrush>(nameof(DotColor));

        public string Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public SolidColorBrush DotColor
        {
            get => GetValue(DotColorProperty);
            set => SetValue(DotColorProperty, value);
        }

        public PanelHeader()
        {
            InitializeComponent();
        }
    }
}
