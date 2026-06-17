using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace UpperApp.UI
{
    /// <summary>
    /// 面板标题栏: 发光圆点 + 标题文字。统一三个面板的标题样式。
    /// </summary>
    public partial class PanelHeader : UserControl
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(PanelHeader),
                new PropertyMetadata(""));

        public static readonly DependencyProperty DotColorProperty =
            DependencyProperty.Register(nameof(DotColor), typeof(Brush), typeof(PanelHeader),
                new PropertyMetadata(null));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public Brush DotColor
        {
            get => (Brush)GetValue(DotColorProperty);
            set => SetValue(DotColorProperty, value);
        }

        public PanelHeader()
        {
            InitializeComponent();
        }
    }
}
