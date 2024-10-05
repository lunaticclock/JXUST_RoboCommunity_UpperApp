using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Sockets;
using System.Runtime.Versioning;
using System.Text;

namespace UpperApp
{
    [SupportedOSPlatform("windows10.0.16299.0")]
    class BthManager
    {
        public BluetoothRadio br = null;
        public BackgroundWorker BthMonitor;
        private BluetoothListener listener = null;
        private BluetoothClient ManualClient = null;
        public readonly Dictionary<string, BluetoothDeviceInfo> BthDevices = new Dictionary<string, BluetoothDeviceInfo>();
        public readonly BindingDic<BluetoothClient> BthClients = new();
        private Encoding encoding;
        public event Action<Result> StatusChanged;
        private class StateObject
        {
            public BluetoothClient Client { get; set; }
            public byte[] Buffer { get; set; }
        }
        public BthManager()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            encoding = Encoding.GetEncoding("GB2312");
            br = BluetoothRadio.Default;
        }

        public void StartMonitor()
        {
            listener = new BluetoothListener(BluetoothService.SerialPort);
            listener.Start();
            BthMonitor = new BackgroundWorker();
            BthMonitor.WorkerSupportsCancellation = true;
            BthMonitor.DoWork += BthListener;
            BthMonitor.RunWorkerCompleted += Monitor_RunWorkerCompleted;
            BthMonitor.RunWorkerAsync();
        }

        public void StopMonitor()
        {
            BthMonitor.CancelAsync();
            BthMonitor = null;
            listener.Stop();
            listener = null;
            string[] cks = [.. BthClients.connectionKeys];
            foreach (string b in cks) 
            {
                BthClients.Remove(b).Close();
            }
        }

        private void BthListener(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = sender as BackgroundWorker;
            while (!bw.CancellationPending)
            {
                BluetoothClient BthClient = listener.AcceptBluetoothClient();
                OnStatusChanged(new Result(Result.NETStatus.NewRemote, "Got a request!\r\n"));
                BthClients.Add(BthClient.RemoteMachineName, BthClient);
                StrSend("Hello from service!\r\n", BthClient);
                byte[] buffer = new byte[2000];
                BthClient.GetStream().BeginRead(buffer, 0, buffer.Length, new AsyncCallback(BthRecv), new StateObject { Client = BthClient, Buffer = buffer });
            }
        }

        public void StrSend(string Buf, BluetoothClient client = null)
        {
            if (client == null)
            {
                client = ManualClient;
            }
            if (client != null && client.Connected)
            {
                try
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(Buf);
                    client.GetStream().WriteAsync(buffer, 0, buffer.Length);
                    OnStatusChanged(new Result(Result.NETStatus.SendMessage, Buf, buffer.Length));
                }
                catch (Exception ex)
                {
                    OnStatusChanged(new Result(Result.NETStatus.ExceptionStop, ex.Message));
                }
            }
            else
            {
                OnStatusChanged(new Result(Result.NETStatus.RemoteStop, "连接断开", 0));
            }
        }

        private void BthRecv(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            BluetoothClient BthClient = state.Client;
            try
            {
                int bytesRead = BthClient.GetStream().EndRead(ar);
                if (bytesRead > 0)
                {
                    string data = Encoding.UTF8.GetString(state.Buffer, 0, bytesRead);
                    OnStatusChanged(new Result(Result.NETStatus.ReciveMessage, data, bytesRead));
                    BthClient.GetStream().BeginRead(state.Buffer, 0, state.Buffer.Length, new AsyncCallback(BthRecv), state);
                }
                else
                {
                    OnStatusChanged(new Result(Result.NETStatus.RemoteStop, ""));
                    BthClients.Remove(BthClient.RemoteMachineName);
                    return;
                }
            }
            catch (SocketException)
            {
            }
            catch (Exception)
            {
            }
        }

        public void SetClient(BluetoothDeviceInfo bluetoothDevice)
        {
            ManualClient = new BluetoothClient();
            ManualClient.Connect(bluetoothDevice.DeviceAddress, BluetoothService.SerialPort);
            byte[] buffer = new byte[2000];
            ManualClient.GetStream().BeginRead(buffer, 0, buffer.Length, new AsyncCallback(BthRecv), new StateObject { Client = ManualClient, Buffer = buffer });
        }

        public BluetoothClient GetSlaveClient(string name)
        {
            return BthClients[name];
        }

        public void DisconnectClient()
        {
            ManualClient.Close();
        }

        protected virtual void OnStatusChanged(Result status)
        {
            StatusChanged?.Invoke(status);
        }

        private void Monitor_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            OnStatusChanged(new Result(Result.NETStatus.MonitorStop, "监听停止"));
        }
    }
}
