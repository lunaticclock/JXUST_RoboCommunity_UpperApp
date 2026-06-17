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
    internal class BthManager : CommunicatorBase, IBluetoothCommunicator
    {
        private CancellationTokenSource _cts;
        private bool _isMonitoring;
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

        public override ChannelType Channel => ChannelType.Bluetooth;

        static BthManager()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public BthManager()
        {
            Br = BluetoothRadio.Default;
        }

        public override void Start(CommunicationParams parameters)
        {
            if (parameters is not BluetoothParams btParams)
                throw new ArgumentException("参数类型必须为 BluetoothParams");

            Stop();
            State = DeviceState.Connecting;
            _cts = new CancellationTokenSource();

            if (btParams.IsServerMode)
            {
                _listener = new BluetoothListener(BluetoothService.SerialPort);
                _listener.Start();
                _ = Task.Run(() => AcceptLoopAsync(_cts.Token));
                NotifyMonitorStarted("蓝牙服务端监听已开始");
                _isMonitoring = true;
            }
            else
            {
                if (string.IsNullOrEmpty(btParams.TargetDeviceName))
                {
                    NotifyException("未指定要连接的蓝牙设备名称");
                    Stop();
                    return;
                }
                // 客户端模式：异步连接，避免阻塞 UI 线程；连接结果由 SetMasterAsync 内部上报状态
                var targetName = btParams.TargetDeviceName;
                _ = Task.Run(() => SetMasterAsync(targetName, _cts.Token));
            }
        }

        public override void Stop()
        {
            // 防止重复调用：正在停止或已断开则直接返回
            if (IsStopping) return;
            if (!_isMonitoring && State == DeviceState.Disconnected) return;

            BeginStop();
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

            NotifyMonitorStopped("蓝牙已停止");
            EndStop();
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
                    NotifyPeerConnected(deviceName, "Got a request!\r\n");

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
                NotifyException($"监听异常: {ex.Message}");
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
                    NotifyMessageReceived(data, bytesRead, deviceName);
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
                if (!IsStopping)
                    NotifyPeerDisconnected("蓝牙从设备断开", deviceName);
            }
        }

        public override void Send(string data, string target = null)
        {
            BluetoothClient client = target == null ? _manualClient : GetSlaveClient(target);
            if (client == null || !client.Connected)
            {
                NotifyPeerDisconnected("连接断开");
                return;
            }

            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(data);
                client.GetStream().Write(buffer, 0, buffer.Length);
                client.GetStream().Flush();
                NotifyMessageSent(data, buffer.Length, target ?? "");
            }
            catch (Exception ex)
            {
                NotifyException(ex.Message);
            }
        }

        public async Task SetMasterAsync(string bluetoothDeviceName, CancellationToken token)
        {
            _manualClient?.Close();
            _manualClient = new BluetoothClient();
            if (!BthDevices.TryGetValue(bluetoothDeviceName, out var device))
            {
                NotifyException($"未找到蓝牙设备: {bluetoothDeviceName}");
                Stop();
                return;
            }

            try
            {
                // BluetoothClient.Connect 是同步阻塞调用，放到线程池执行
                await Task.Run(() => _manualClient.Connect(device.DeviceAddress, BluetoothService.SerialPort), token);
            }
            catch (OperationCanceledException)
            {
                // Stop 已在别处清理，无需重复
                return;
            }
            catch (Exception ex)
            {
                NotifyException($"连接蓝牙设备失败: {ex.Message}");
                Stop();
                return;
            }

            // 连接成功：更新状态并启动接收循环
            NotifyMonitorStarted($"已连接到 {bluetoothDeviceName}");
            _isMonitoring = true;
            _ = ReceiveLoopAsync(_manualClient, token);
        }

        /// <summary>
        /// 同步连接（保留以兼容 IBluetoothCommunicator.ConnectToDevice 的同步语义）
        /// </summary>
        public void SetMaster(string bluetoothDeviceName)
        {
            _manualClient?.Close();
            _manualClient = new BluetoothClient();
            if (!BthDevices.TryGetValue(bluetoothDeviceName, out var device))
            {
                NotifyException($"未找到蓝牙设备: {bluetoothDeviceName}");
                return;
            }
            _manualClient.Connect(device.DeviceAddress, BluetoothService.SerialPort);
            NotifyMonitorStarted($"已连接到 {bluetoothDeviceName}");
            _isMonitoring = true;
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

        public override IReadOnlyList<string> GetPeerList() => BthClients.connectionKeys;
    }
}
