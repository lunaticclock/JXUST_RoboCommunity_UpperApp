using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UpperApp.Commands;
using UpperApp.Communication;
using UpperApp.Core;

namespace UpperApp.Services
{
    internal class DeviceService : IDisposable
    {
        private readonly ICommunicatorFactory _factory;
        private readonly Dictionary<ChannelType, ICommunicator> _cache = [];
        private readonly Lock _cacheLock = new();
        private ChannelType _activeChannel = ChannelType.Serial;
        private string _pendingTarget;
        private string _pendingBthTarget;

        public event Action<StatusEvent> StatusChanged;

        public DeviceService(ICommunicatorFactory factory)
        {
            _factory = factory;
        }

        public ChannelType ActiveChannel
        {
            get => _activeChannel;
            set => _activeChannel = value;
        }

        private ICommunicator GetOrCreate(ChannelType channel)
        {
            lock (_cacheLock)
            {
                if (_cache.TryGetValue(channel, out var comm))
                    return comm;
            }

            var instance = _factory.Create(channel);
            instance.StatusChanged += OnCommunicatorStatusChanged;

            lock (_cacheLock)
            {
                if (_cache.TryGetValue(channel, out var existing))
                {
                    instance.StatusChanged -= OnCommunicatorStatusChanged;
                    return existing;
                }
                _cache[channel] = instance;
                return instance;
            }
        }

        public void StartChannel(ChannelType channel, CommunicationParams parameters)
        {
            var comm = GetOrCreate(channel);
            comm.Start(parameters);
        }

        public void StopChannel(ChannelType channel)
        {
            lock (_cacheLock)
            {
                if (!_cache.TryGetValue(channel, out var comm)) return;
                comm.Stop();
            }
        }

        public bool IsChannelReady(ChannelType channel)
        {
            lock (_cacheLock)
            {
                if (!_cache.TryGetValue(channel, out var comm)) return false;
                return comm.State == DeviceState.Connected;
            }
        }

        /// <summary>
        /// 获取通道当前状态（未创建时返回 Disconnected）。
        /// 用于通道状态指示灯条轮询。
        /// </summary>
        public DeviceState GetChannelState(ChannelType channel)
        {
            lock (_cacheLock)
            {
                if (!_cache.TryGetValue(channel, out var comm)) return DeviceState.Disconnected;
                return comm.State;
            }
        }

        /// <summary>
        /// 懒加载蓝牙通信器：首次访问时通过工厂创建并缓存，避免启动时初始化蓝牙栈。
        /// </summary>
        private IBluetoothCommunicator GetOrCreateBluetooth()
        {
            return (IBluetoothCommunicator)GetOrCreate(ChannelType.Bluetooth);
        }

        public bool IsAnyChannelReady()
        {
            lock (_cacheLock)
            {
                return _cache.Values.Any(c => c.State == DeviceState.Connected);
            }
        }

        public void SetTarget(string target) => _pendingTarget = target;
        public void SetBluetoothTarget(string target) => _pendingBthTarget = target;

        public bool TryExecuteCommand(IDeviceCommand command)
        {
            var channel = command.TargetChannel == ChannelType.Unknown
                ? _activeChannel
                : command.TargetChannel;

            ICommunicator comm;
            lock (_cacheLock)
            {
                if (!_cache.TryGetValue(channel, out comm)) return false;
            }
            if (comm.State != DeviceState.Connected) return false;

            string target = ResolveTarget(channel);
            comm.Send(command.Encode(), target);
            return true;
        }

        /// <summary>
        /// 直接发送原始字节（用于 Hex 模式，绕过字符编码）。
        /// </summary>
        public bool TrySendBytes(byte[] data, ChannelType? channelOverride = null)
        {
            var channel = channelOverride ?? _activeChannel;
            ICommunicator comm;
            lock (_cacheLock)
            {
                if (!_cache.TryGetValue(channel, out comm)) return false;
            }
            if (comm.State != DeviceState.Connected) return false;

            string target = ResolveTarget(channel);
            comm.Send(data, target);
            return true;
        }

        private string ResolveTarget(ChannelType channel)
        {
            return channel switch
            {
                ChannelType.TCP or ChannelType.UDP => _pendingTarget,
                ChannelType.Bluetooth => _pendingBthTarget,
                _ => null
            };
        }

        public IReadOnlyList<string> GetPeerList(ChannelType channel)
        {
            lock (_cacheLock)
            {
                if (!_cache.TryGetValue(channel, out var comm)) return [];
                return comm.GetPeerList();
            }
        }

        public bool IsBluetoothRadioAvailable => GetOrCreateBluetooth().IsRadioAvailable;
        public bool IsBluetoothRadioPoweredOn => GetOrCreateBluetooth().IsRadioPoweredOn;
        public string BluetoothRadioAddress => GetOrCreateBluetooth().RadioAddress;
        public string BluetoothRadioMode => GetOrCreateBluetooth().RadioMode;

        public void ConnectBluetoothDevice(string name) => GetOrCreateBluetooth().ConnectToDevice(name);
        public void DisconnectBluetoothClient() => GetOrCreateBluetooth().DisconnectClient();
        public async System.Threading.Tasks.Task<List<InTheHand.Net.Sockets.BluetoothDeviceInfo>> DiscoverBluetoothDevicesAsync()
            => await GetOrCreateBluetooth().DiscoverDevicesAsync();

        private void OnCommunicatorStatusChanged(StatusEvent evt)
        {
            StatusChanged?.Invoke(evt);
        }

        public void DisposeAll()
        {
            lock (_cacheLock)
            {
                foreach (var comm in _cache.Values)
                {
                    try { comm.Stop(); } catch { }
                    try { comm.DisposeAsync()
                              .GetAwaiter()
                              .GetResult(); } catch { }
                }
                _cache.Clear();
            }
        }

        public void Dispose()
        {
            DisposeAll();
            GC.SuppressFinalize(this);
        }
    }
}
