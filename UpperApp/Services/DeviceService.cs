using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using UpperApp.Communication;
using UpperApp.Core;

namespace UpperApp.Services
{
    internal class DeviceService : IDisposable
    {
        private readonly ConcurrentDictionary<ChannelType, Lazy<ICommunicator>> _cache = [];
        private string _pendingTarget;
        private string _pendingBthTarget;

        public event Action<StatusEvent> StatusChanged;

        public ChannelType ActiveChannel { get; set; } = ChannelType.Serial;

        [SupportedOSPlatform("windows10.0.19041.0")]
        private static ICommunicator Create(ChannelType channel) => channel switch
        {
            ChannelType.Serial => new TouchSocketSerialAdapter(),
            ChannelType.TCP => new TouchSocketTcpAdapter(),
            ChannelType.UDP => new TouchSocketUdpAdapter(),
            ChannelType.Bluetooth => new BthManager(),
            ChannelType.CAN => new CANManager(),
            ChannelType.WebSocket => new WebSocketManager(),
            _ => throw new NotSupportedException($"不支持的通道类型: {channel}")
        };

        private ICommunicator GetOrCreate(ChannelType channel)
            => _cache.GetOrAdd(channel, c => new Lazy<ICommunicator>(() => CreateWithHook(c))).Value;

        private ICommunicator CreateWithHook(ChannelType channel)
        {
            var instance = Create(channel);
            instance.StatusChanged += OnCommunicatorStatusChanged;
            return instance;
        }

        public void StartChannel(ChannelType channel, CommunicationParams parameters)
            => GetOrCreate(channel).Start(parameters);

        public void StopChannel(ChannelType channel)
        {
            if (_cache.TryGetValue(channel, out var lazy))
                lazy.Value.Stop();
        }

        public bool IsChannelReady(ChannelType channel)
            => _cache.TryGetValue(channel, out var lazy) && lazy.Value.State == DeviceState.Connected;

        /// <summary>
        /// 获取通道当前状态（未创建时返回 Disconnected）。
        /// 用于通道状态指示灯条轮询。
        /// </summary>
        public DeviceState GetChannelState(ChannelType channel)
            => _cache.TryGetValue(channel, out var lazy) ? lazy.Value.State : DeviceState.Disconnected;

        /// <summary>
        /// 懒加载蓝牙通信器：首次访问时通过工厂创建并缓存，避免启动时初始化蓝牙栈。
        /// </summary>
        private IBluetoothCommunicator GetOrCreateBluetooth()
            => (IBluetoothCommunicator)GetOrCreate(ChannelType.Bluetooth);

        public bool IsAnyChannelReady()
            => _cache.Values.Any(l => l.Value.State == DeviceState.Connected);

        public void SetTarget(string target) => _pendingTarget = target;
        public void SetBluetoothTarget(string target) => _pendingBthTarget = target;

        public bool TrySendString(string protocol, ChannelType? channelOverride = null)
            => TryWith(channelOverride, (comm, target) => comm.Send(protocol, target));

        /// <summary>
        /// 直接发送原始字节（用于 Hex 模式，绕过字符编码）。
        /// </summary>
        public bool TrySendBytes(byte[] data, ChannelType? channelOverride = null)
            => TryWith(channelOverride, (comm, target) => comm.Send(data, target));

        /// <summary>
        /// 通道已创建且处于连接态时，用解析好的目标执行发送。
        /// </summary>
        private bool TryWith(ChannelType? channelOverride, Action<ICommunicator, string> send)
        {
            var channel = channelOverride ?? ActiveChannel;
            if (!_cache.TryGetValue(channel, out var lazy)) return false;

            var comm = lazy.Value;
            if (comm.State != DeviceState.Connected) return false;

            send(comm, ResolveTarget(channel));
            return true;
        }

        private string ResolveTarget(ChannelType channel) => channel switch
        {
            ChannelType.TCP or ChannelType.UDP => _pendingTarget,
            ChannelType.Bluetooth => _pendingBthTarget,
            _ => null
        };

        public IReadOnlyList<string> GetPeerList(ChannelType channel)
            => _cache.TryGetValue(channel, out var lazy) ? lazy.Value.GetPeerList() : [];

        public bool IsBluetoothRadioAvailable => GetOrCreateBluetooth().IsRadioAvailable;
        public bool IsBluetoothRadioPoweredOn => GetOrCreateBluetooth().IsRadioPoweredOn;
        public string BluetoothRadioAddress => GetOrCreateBluetooth().RadioAddress;
        public string BluetoothRadioMode => GetOrCreateBluetooth().RadioMode;

        public void ConnectBluetoothDevice(string name) => GetOrCreateBluetooth().ConnectToDevice(name);
        public void DisconnectBluetoothClient() => GetOrCreateBluetooth().DisconnectClient();
        public async System.Threading.Tasks.Task<List<InTheHand.Net.Sockets.BluetoothDeviceInfo>> DiscoverBluetoothDevicesAsync()
            => await GetOrCreateBluetooth().DiscoverDevicesAsync();

        private void OnCommunicatorStatusChanged(StatusEvent evt) => StatusChanged?.Invoke(evt);

        public void DisposeAll()
        {
            foreach (var lazy in _cache.Values)
            {
                if (!lazy.IsValueCreated) continue;

                var comm = lazy.Value;
                try { comm.Stop(); } catch { }
                try { comm.DisposeAsync().GetAwaiter().GetResult(); } catch { }
            }
            _cache.Clear();
        }

        public void Dispose()
        {
            DisposeAll();
            GC.SuppressFinalize(this);
        }
    }
}
