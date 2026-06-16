using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using UpperApp.Communication;
using UpperApp.Core;

namespace UpperApp.Services
{
    /// <summary>
    /// 简易服务定位器（手动实现轻量级 IoC）
    /// </summary>
    [SupportedOSPlatform("windows10.0.19041.0")]
    internal static class AppServices
    {
        private static readonly Dictionary<Type, Func<object>> _registrations = [];
        private static readonly Dictionary<Type, object> _singletons = [];

        /// <summary>
        /// 注册单例服务
        /// </summary>
        public static void RegisterSingleton<TInterface>(TInterface instance) where TInterface : class
        {
            _singletons[typeof(TInterface)] = instance;
        }

        /// <summary>
        /// 注册工厂服务（每次调用返回新实例）
        /// </summary>
        public static void RegisterTransient<TInterface>(Func<TInterface> factory) where TInterface : class
        {
            _registrations[typeof(TInterface)] = () => factory();
        }

        /// <summary>
        /// 获取服务实例
        /// </summary>
        public static TInterface GetService<TInterface>() where TInterface : class
        {
            if (_singletons.TryGetValue(typeof(TInterface), out var singleton))
                return (TInterface)singleton;

            if (_registrations.TryGetValue(typeof(TInterface), out var factory))
                return (TInterface)factory();

            throw new InvalidOperationException($"未注册服务类型: {typeof(TInterface)}");
        }

        /// <summary>
        /// 配置所有服务（在程序入口调用）
        /// </summary>
        public static void ConfigureServices()
        {
            var factory = new CommunicatorFactory();
            RegisterSingleton<ICommunicatorFactory>(factory);
            RegisterSingleton<IConfigStorage>(new JsonFileConfigStorage());

            var bluetoothComm = (IBluetoothCommunicator)factory.Create(ChannelType.Bluetooth);
            var deviceService = new DeviceService(factory, bluetoothComm);
            RegisterSingleton(deviceService);
        }
    }
}
