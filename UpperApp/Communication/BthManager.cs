using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UpperApp.Core;
using UpperApp.Services;

namespace UpperApp.Communication
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    internal class BthManager : ICommunicator, IBluetoothCommunicator
    {
        private CancellationTokenSource _cts;
        private bool _isMonitoring;
        private bool _isStopping;
        private DeviceState _state = DeviceState.Disconnected;
        private readonly Encoding _encoding = Encoding.GetEncoding("GB2312");

        public BluetoothRadio Br { get; private set; }
        private BluetoothListener _listener;
        private BluetoothClient _manualClient;
        private readonly Dictionary<string, BluetoothDeviceInfo> BthDevices = [];
        private readonly BindingDic<BluetoothClient> BthClients = new();

        public bool IsRadioAvailable => Br != null;
        public bool IsRadioPoweredOn => Br != null && Br.Mode != InTheHand.Net.Bluetooth.RadioMode.PowerOff;
        public string RadioAddress => Br?.LocalAddress.ToString() ?? "";
        public string RadioMode => Br?.Mode.ToString() ?? "";

        public event Action<Result> StatusChanged;
        public ChannelType Channel => ChannelType.Bluetooth;

        public DeviceState State
        {
            get => _state;
            private set
            {
                if (_state != value)
                    _state = value;
            }
        }

        static BthManager()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public BthManager()
        {
            Br = BluetoothRadio.Default;
        }

        public void Start(CommunicationParams parameters)
        {
            if (parameters is not BluetoothParams btParams)
                throw new ArgumentException("参数类型必须为 BluetoothParams");

            Stop();
            State = DeviceState.Connecting;
            _cts = new CancellationTokenSource();
            _isStopping = false;

            if (btParams.IsServerMode)
            {
                _listener = new BluetoothListener(BluetoothService.SerialPort);
                _listener.Start();
                _ = Task.Run(() => AcceptLoopAsync(_cts.Token));
            }
            else
            {
                if (string.IsNullOrEmpty(btParams.TargetDeviceName))
                {
                    OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, "未指定要连接的蓝牙设备名称"));
                    Stop();
                    return;
                }
                SetMaster(btParams.TargetDeviceName);
            }

            State = DeviceState.Connected;
            OnStatusChanged(new Result(Result.NETStatus.MonitorStart, "蓝牙监听开始"));
            _isMonitoring = true;
        }

        public void Stop()
        {
            if (!_isMonitoring && _state == DeviceState.Disconnected) return;

            _isStopping = true;
            State = DeviceState.Disconnecting;
            _isMonitoring = false;
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

            try { _listener?.Stop(); } catch { }
            _listener = null;
            foreach (string key in BthClients.connectionKeys.ToArray())
            {
                try { BthClients.Remove(key)?.Close(); } catch { }
            }
            try { _manualClient?.Dispose(); } catch { }
            _manualClient = null;

            OnStatusChanged(new Result(Result.NETStatus.MonitorStop, "蓝牙已停止"));
            State = DeviceState.Disconnected;
            _isStopping = false;
        }

        private async Task AcceptLoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    BluetoothClient client = await Task.Run(() => _listener!.AcceptBluetoothClient(), token);
                    string deviceName = client.RemoteMachineName ?? "unknown";
                    BthClients.Add(deviceName, client);
                    OnStatusChanged(new Result(Result.NETStatus.NewRemote, "Got a request!\r\n"));

                    try
                    {
                        byte[] welcome = Encoding.UTF8.GetBytes("Hello from service!\r\n");
                        await client.GetStream().WriteAsync(welcome, token);
                        await client.GetStream().FlushAsync(token);
                    }
                    catch { }

                    _ = ReceiveLoopAsync(client, token);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, $"监听异常: {ex.Message}"));
            }
            finally
            {
                _isMonitoring = false;
            }
        }

        private async Task ReceiveLoopAsync(BluetoothClient client, CancellationToken token)
        {
            byte[] buffer = new byte[2000];
            var stream = client.GetStream();
            string deviceName = client.RemoteMachineName ?? "unknown";

            try
            {
                while (!token.IsCancellationRequested)
                {
                    int bytesRead = await stream.ReadAsync(buffer, token);
                    if (bytesRead == 0) break;

                    string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    OnStatusChanged(new Result(Result.NETStatus.ReciveMessage, data, bytesRead));
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) when (ex is IOException or SocketException)
            {
                System.Diagnostics.Debug.WriteLine($"蓝牙接收异常 {deviceName}: {ex.Message}");
            }
            finally
            {
                BthClients.Remove(deviceName);
                client.Close();
                if (!_isStopping)
                    OnStatusChanged(new Result(Result.NETStatus.RemoteStop, deviceName));
            }
        }

        public void Send(string data, string target = null)
        {
            BluetoothClient client = target == null ? _manualClient : GetSlaveClient(target);
            if (client == null || !client.Connected)
            {
                OnStatusChanged(new Result(Result.NETStatus.RemoteStop, "连接断开", 0));
                return;
            }

            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(data);
                client.GetStream().Write(buffer, 0, buffer.Length);
                client.GetStream().Flush();
                OnStatusChanged(new Result(Result.NETStatus.SendMessage, data, buffer.Length));
            }
            catch (Exception ex)
            {
                OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, ex.Message));
            }
        }

        public void SetMaster(string bluetoothDeviceName)
        {
            _manualClient?.Close();
            _manualClient = new BluetoothClient();
            if (!BthDevices.TryGetValue(bluetoothDeviceName, out var device))
            {
                OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, $"未找到蓝牙设备: {bluetoothDeviceName}"));
                return;
            }
            _manualClient.Connect(device.DeviceAddress, BluetoothService.SerialPort);
            _ = Task.Run(() => ReceiveLoopAsync(_manualClient, _cts.Token));
        }

        public BluetoothClient GetSlaveClient(string name)
        {
            BthClients.TryGet(name, out BluetoothClient client);
            return client;
        }

        public void DisconnectClient()
        {
            _manualClient?.Close();
            _manualClient = null;
        }

        public void ConnectToDevice(string deviceName)
        {
            var param = new BluetoothParams { IsServerMode = false, TargetDeviceName = deviceName };
            if (_isMonitoring) Stop();
            Start(param);
        }

        public async Task<List<BluetoothDeviceInfo>> DiscoverDevicesAsync()
        {
            return await Task.Run(() =>
            {
                using var cli = new BluetoothClient();
                List<BluetoothDeviceInfo> devices = [.. cli.DiscoverDevices()];
                foreach (var device in devices)
                {
                    BthDevices.TryAdd(device.DeviceName, device);
                }
                return devices;
            });
        }

        public IReadOnlyList<string> GetPeerList() => BthClients.connectionKeys;

        public ValueTask DisposeAsync()
        {
            Stop();
            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }

        private void OnStatusChanged(Result result)
        {
            if (result.Channel == ChannelType.Unknown)
                result = result with { Channel = ChannelType.Bluetooth };
            if (result.NetStatus == Result.NETStatus.ExceptionStop)
                State = DeviceState.Error;
            StatusChanged?.Invoke(result);
        }
    }
}
