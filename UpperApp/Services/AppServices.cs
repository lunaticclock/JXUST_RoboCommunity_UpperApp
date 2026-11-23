using System.Runtime.Versioning;

namespace UpperApp.Services
{
    /// <summary>
    /// 应用级服务入口（静态单例）。
    /// </summary>
    [SupportedOSPlatform("windows10.0.19041.0")]
    internal static class AppServices
    {
        public static IConfigStorage Config { get; } = new JsonFileConfigStorage();

        // ponytail: 蓝牙通信器懒加载，首次访问蓝牙时由 DeviceService 按需创建
        public static DeviceService Device { get; } = new();
    }
}