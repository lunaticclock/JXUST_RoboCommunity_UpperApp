using UpperApp.Core;

namespace UpperApp.Processing
{
    internal interface IDisplayAdapter
    {
        void UpdateByteCount(int count, RecvOrSend direction);
        bool IsCharMode { get; }
        bool IsHexMode { get; }
        bool IsLocalEchoEnabled { get; }
        bool IsAngleDisplayEnabled { get; }
        bool IsSaveDataEnabled { get; }
        void AppendToReceiveBox(string text);
        void UpdateAngleDisplay(string message);
        void OnNewPeer(string peerInfo);
    }
}
