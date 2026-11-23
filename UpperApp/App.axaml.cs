using System;
using System.Threading;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Logging;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.Threading;
using ReactiveUI.Primitives.Concurrency;

namespace UpperApp
{
    public partial class App : Application
    {
        /// <summary>
        /// UI 线程执行器。ReactiveUI 28 移除了 RxApp，这里用 Avalonia UI 线程的
        /// SynchronizationContext 构造 <see cref="ISequencer"/> 供定时/事件回到 UI 线程。
        /// </summary>
        public static ISequencer UiSequencer { get; private set; }

        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace(LogEventLevel.Warning);

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            RequestedThemeVariant = ThemeVariant.Dark;

            AvaloniaSynchronizationContext.InstallIfNeeded();
            var uiContext = SynchronizationContext.Current;
            if (uiContext != null)
                UiSequencer = new SynchronizationContextSequencer(uiContext);

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }
            base.OnFrameworkInitializationCompleted();
        }
    }
}
