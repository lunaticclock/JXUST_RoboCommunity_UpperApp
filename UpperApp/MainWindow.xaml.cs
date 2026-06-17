using System.Windows;
using System.Windows.Input;
using UpperApp.UI;
using UpperApp.ViewModels;

namespace UpperApp
{
    public partial class MainWindow : Window
    {
        private MapTracker _mapTracker;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _mapTracker = new MapTracker(MapCanvas);

            if (DataContext is MainViewModel vm)
            {
                vm.SetMapTracker(_mapTracker);
                // 注入流式日志输出，利用 TextBox.AppendText 增量更新
                vm.SetLogSink(new TextBoxLogSink(RecvTextBox, maxLength: 500_000));
                // 注入流量曲线控件，VM 定时器每秒推入 Rx/Tx 采样
                vm.SetTrafficChart(TrafficChartControl);
            }
        }

        private void MapCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var p = e.GetPosition(MapCanvas);
            _mapTracker?.OnMapClick(p, msg =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (DataContext is MainViewModel vm)
                        vm.AppendRecvText(msg);
                });
            });

            if (DataContext is MainViewModel vm2)
            {
                vm2.MapStartPoint = _mapTracker?.StartPoint ?? "0,0";
                vm2.MapEndPoint = _mapTracker?.EndPoint ?? "0,0";
            }
        }

        private void MapCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            var p = e.GetPosition(MapCanvas);
            _mapTracker?.OnMouseMove(p);
            if (DataContext is MainViewModel vm)
                vm.MousePosition = _mapTracker?.GetMousePosition(p) ?? "0,0";
        }

        private void MapCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (MapBorder.ActualWidth > 0)
            {
                var desiredHeight = MapBorder.ActualWidth * 9.0 / 16.0;
                MapBorder.Height = desiredHeight;
            }
        }
    }
}
