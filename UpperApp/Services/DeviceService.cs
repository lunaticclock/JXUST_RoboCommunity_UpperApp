using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpperApp.Commands;
using UpperApp.Communication;
using UpperApp.Core;

namespace UpperApp.Services
{
    internal class DeviceService
    {
        private readonly Dictionary<ChannelType, ICommunicator> _communicators;
        private readonly IBluetoothCommunicator _bluetoothComm;
        private ChannelType _activeChannel;
        private string _pendingTarget;
        private string _pendingBthTarget;

        public event Action<Result> StatusChanged;

        public ChannelType ActiveChannel
        {
            get => _activeChannel;
            set => _activeChannel = value;
        }

        public DeviceService(Dictionary<ChannelType, ICommunicator> communicators, IBluetoothCommunicator bluetoothComm)
        {
            _communicators = communicators;
            _bluetoothComm = bluetoothComm;
            _activeChannel = ChannelType.Serial;

            foreach (var kvp in _communicators)
                kvp.Value.StatusChanged += OnCommunicatorStatusChanged;
        }

        private void OnCommunicatorStatusChanged(Result result)
        {
            StatusChanged?.Invoke(result);
        }

        public DeviceState GetChannelState(ChannelType channel)
        {
            if (channel == ChannelType.Bluetooth)
                return _bluetoothComm.State;
            if (_communicators.TryGetValue(channel, out var comm))
                return comm.State;
            return DeviceState.Disconnected;
        }

        public bool IsChannelReady(ChannelType channel)
        {
            return GetChannelState(channel) == DeviceState.Connected;
        }

        public bool IsAnyChannelReady()
        {
            if (_bluetoothComm.State == DeviceState.Connected)
                return true;
            return _communicators.Values.Any(c => c.State == DeviceState.Connected);
        }

        public void StartChannel(ChannelType channel, CommunicationParams param)
        {
            if (_communicators.TryGetValue(channel, out var comm))
                comm.Start(param);
        }

        public void StopChannel(ChannelType channel)
        {
            if (_communicators.TryGetValue(channel, out var comm))
                comm.Stop();
        }

        public IReadOnlyList<string> GetPeerList(ChannelType channel)
        {
            if (channel == ChannelType.Bluetooth)
                return _bluetoothComm.GetPeerList();
            if (_communicators.TryGetValue(channel, out var comm))
                return comm.GetPeerList();
            return [];
        }

        public void ExecuteCommand(IDeviceCommand command)
        {
            var channel = command.TargetChannel == ChannelType.Unknown
                ? _activeChannel
                : command.TargetChannel;

            if (!IsChannelReady(channel))
                throw new InvalidOperationException($"通道 {channel} 未连接");

            ICommunicator comm;
            if (channel == ChannelType.Bluetooth)
                comm = _bluetoothComm;
            else if (!_communicators.TryGetValue(channel, out comm))
                throw new InvalidOperationException($"未找到通道 {channel} 的管理器");

            string target = ResolveTarget(channel);
            comm.Send(command.Encode(), target);
        }

        public bool TryExecuteCommand(IDeviceCommand command)
        {
            try
            {
                ExecuteCommand(command);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void SetTarget(string target)
        {
            _pendingTarget = target;
        }

        public void SetBluetoothTarget(string target)
        {
            _pendingBthTarget = target;
        }

        #region Bluetooth Proxy

        public bool IsBluetoothReady => _bluetoothComm.State == DeviceState.Connected;

        public bool IsBluetoothRadioAvailable => _bluetoothComm.IsRadioAvailable;

        public bool IsBluetoothRadioPoweredOn => _bluetoothComm.IsRadioPoweredOn;

        public string BluetoothRadioAddress => _bluetoothComm.RadioAddress;

        public string BluetoothRadioMode => _bluetoothComm.RadioMode;

        public void StartBluetooth(CommunicationParams param)
        {
            _bluetoothComm.Start(param);
        }

        public void StopBluetooth()
        {
            _bluetoothComm.Stop();
        }

        public void SendBluetooth(string data, string target = null)
        {
            _bluetoothComm.Send(data, target);
        }

        public Task<List<InTheHand.Net.Sockets.BluetoothDeviceInfo>> DiscoverBluetoothDevicesAsync()
        {
            return _bluetoothComm.DiscoverDevicesAsync();
        }

        public void ConnectBluetoothDevice(string deviceName)
        {
            _bluetoothComm.ConnectToDevice(deviceName);
        }

        public void DisconnectBluetoothClient()
        {
            _bluetoothComm.DisconnectClient();
        }

        #endregion

        public void StopAll()
        {
            foreach (var comm in _communicators.Values)
                comm.Stop();
            _bluetoothComm.Stop();
        }

        public void DisposeAll()
        {
            foreach (var comm in _communicators.Values)
            {
                comm.Stop();
                (comm as IDisposable)?.Dispose();
            }
            _bluetoothComm.Stop();
            (_bluetoothComm as IDisposable)?.Dispose();
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
    }
}
