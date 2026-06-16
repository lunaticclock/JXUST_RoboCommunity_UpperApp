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
        private readonly IBluetoothCommunicator _bluetoothComm;
        private readonly Dictionary<ChannelType, ICommunicator> _cache = [];
        private readonly Lock _cacheLock = new();
        private ChannelType _activeChannel = ChannelType.Serial;
        private string _pendingTarget;
        private string _pendingBthTarget;

        public event Action<Result> StatusChanged;

        public DeviceService(ICommunicatorFactory factory, IBluetoothCommunicator bluetoothComm)
        {
            _factory = factory;
            _bluetoothComm = bluetoothComm;

            _cache[ChannelType.Bluetooth] = _bluetoothComm;
            _bluetoothComm.StatusChanged += OnCommunicatorStatusChanged;
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

        public bool IsBluetoothReady => _bluetoothComm.State == DeviceState.Connected;
        public bool IsBluetoothRadioAvailable => _bluetoothComm.IsRadioAvailable;
        public bool IsBluetoothRadioPoweredOn => _bluetoothComm.IsRadioPoweredOn;
        public string BluetoothRadioAddress => _bluetoothComm.RadioAddress;
        public string BluetoothRadioMode => _bluetoothComm.RadioMode;

        public void StartBluetooth(BluetoothParams param) => _bluetoothComm.Start(param);
        public void StopBluetooth() => _bluetoothComm.Stop();
        public void SendBluetooth(string data, string target = null) => _bluetoothComm.Send(data, target);
        public void ConnectBluetoothDevice(string name) => _bluetoothComm.ConnectToDevice(name);
        public void DisconnectBluetoothClient() => _bluetoothComm.DisconnectClient();
        public async System.Threading.Tasks.Task<List<InTheHand.Net.Sockets.BluetoothDeviceInfo>> DiscoverBluetoothDevicesAsync()
            => await _bluetoothComm.DiscoverDevicesAsync();

        private void OnCommunicatorStatusChanged(Result result)
        {
            StatusChanged?.Invoke(result);
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
