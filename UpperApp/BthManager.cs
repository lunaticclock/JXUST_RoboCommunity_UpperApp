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

namespace UpperApp
{
    [SupportedOSPlatform("windows10.0.17763.0")]
    class BthManager : BaseCommunicationManager, IAsyncDisposable
    {
        public BluetoothRadio br { get; private set; }
        private BluetoothListener _listener;
        private BluetoothClient _manualClient;
        public readonly Dictionary<string, BluetoothDeviceInfo> BthDevices = new Dictionary<string, BluetoothDeviceInfo>();
        public readonly BindingDic<BluetoothClient> BthClients = new();

        private class StateObject
        {
            public BluetoothClient Client { get; set; }
            public byte[] Buffer { get; set; }
        }

        public BthManager() : base(ChannelType.Bluetooth)
        {
            br = BluetoothRadio.Default;
        }

        // 启动监听（兼容原方法）
        public override void StartMonitor()
        {
            if (_isMonitoring) StopMonitor();
            _listener = new BluetoothListener(BluetoothService.SerialPort);
            _listener.Start();
            _isMonitoring = true;
            _ = Task.Run(() => AcceptLoopAsync(_cts.Token));
            OnStatusChanged(new Result(Result.NETStatus.MonitorStart, "监听开始"));
        }

        private async Task AcceptLoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var client = await Task.Run(() => _listener!.AcceptBluetoothClient(), token);
                    string deviceName = client.RemoteMachineName ?? "unknown";
                    BthClients.Add(deviceName, client);
                    OnStatusChanged(new Result(Result.NETStatus.NewRemote, "Got a request!\r\n"));

                    // 发送欢迎消息（同步发送，避免复杂）
                    try
                    {
                        byte[] welcome = Encoding.UTF8.GetBytes("Hello from service!\r\n");
                        client.GetStream().Write(welcome, 0, welcome.Length);
                    }
                    catch { /* 忽略发送失败 */ }

                    // 启动接收循环
                    _ = Task.Run(() => ReceiveLoopAsync(client, token), token);
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
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token);
                    if (bytesRead == 0) break; // 连接关闭

                    string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    OnStatusChanged(new Result(Result.NETStatus.ReciveMessage, data, bytesRead));
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) when (ex is IOException or SocketException)
            {
                // 记录日志（可选）
                System.Diagnostics.Debug.WriteLine($"蓝牙接收异常 {deviceName}: {ex.Message}");
            }
            finally
            {
                BthClients.Remove(deviceName);
                client.Close();
                OnStatusChanged(new Result(Result.NETStatus.RemoteStop, deviceName));
            }
        }

        public override void StopMonitor()
        {
            _cts.Cancel();
            _listener?.Stop();
            _listener = null;
            _isMonitoring = false;

            OnStatusChanged(new Result(Result.NETStatus.MonitorStop, "停止监听"));

            // 关闭所有客户端连接
            foreach (string key in BthClients.connectionKeys.ToArray())
            {
                BthClients.Remove(key)?.Close();
            }

            // 重置 CancellationTokenSource 以便重启
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
        }

        // 发送字符串（兼容原方法，支持指定客户端或使用 ManualClient）
        public void StrSend(string Buf, BluetoothClient client = null)
        {
            var target = client ?? _manualClient;
            if (target == null || !target.Connected)
            {
                OnStatusChanged(new Result(Result.NETStatus.RemoteStop, "连接断开", 0));
                return;
            }

            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(Buf);
                target.GetStream().Write(buffer, 0, buffer.Length);
                OnStatusChanged(new Result(Result.NETStatus.SendMessage, Buf, buffer.Length));
            }
            catch (Exception ex)
            {
                OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, ex.Message));
            }
        }

        // 设置主动连接的客户端（兼容原方法）
        public void SetClient(BluetoothDeviceInfo bluetoothDevice)
        {
            _manualClient?.Close();
            _manualClient = new BluetoothClient();
            _manualClient.Connect(bluetoothDevice.DeviceAddress, BluetoothService.SerialPort);
            // 启动接收循环
            _ = Task.Run(() => ReceiveLoopAsync(_manualClient, _cts.Token));
        }

        // 获取已连接的从设备（兼容原方法）
        public BluetoothClient GetSlaveClient(string name)
        {
            BthClients.TryGet(name, out BluetoothClient client);
            return client;
        }

        // 断开主动连接的客户端（兼容原方法）
        public void DisconnectClient()
        {
            _manualClient?.Close();
            _manualClient = null;
        }

        public async Task<List<BluetoothDeviceInfo>> DiscoverDevicesAsync()
        {
            return await Task.Run(() =>
            {
                using (var cli = new BluetoothClient())
                {
                    return cli.DiscoverDevices().ToList();
                }
            });
        }

        public async ValueTask DisposeAsync()
        {
            StopMonitor();
            _manualClient?.Dispose();
            await Task.CompletedTask;
        }
    }
}
