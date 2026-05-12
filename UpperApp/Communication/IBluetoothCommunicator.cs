using System.Collections.Generic;
using System.Threading.Tasks;
using InTheHand.Net.Sockets;

namespace UpperApp.Communication
{
    internal interface IBluetoothCommunicator : ICommunicator
    {
        bool IsRadioAvailable { get; }
        bool IsRadioPoweredOn { get; }
        string RadioAddress { get; }
        string RadioMode { get; }
        Task<List<BluetoothDeviceInfo>> DiscoverDevicesAsync();
        void ConnectToDevice(string deviceName);
        void DisconnectClient();
    }
}
