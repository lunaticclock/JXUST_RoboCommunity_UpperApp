using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Text;

namespace UpperApp
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    internal class MessageProcessor
    {
        // 主窗体提供的依赖
        private readonly Action<int, RecvOrSend> _setRs;
        private readonly Func<bool> _isCharMode;
        private readonly Func<bool> _isHexMode;
        private readonly Func<bool> _isReDisp;
        private readonly Action<string> _appendToRecvBox;
        private readonly Action<string> _writeLog;
        private readonly Action<string> _setAngDisp;
        private readonly Func<bool> _isAngDirDispEnabled;
        private readonly Action<string> _onNewPeer;          // 可选，用于处理新对端标识
        private const string FromPrefix = "from:";
        private const string ToPrefix = "to:";
        private const string NewLine = "\r\n";

        public MessageProcessor(
            Action<int, RecvOrSend> setRs,
            Func<bool> isCharMode,
            Func<bool> isHexMode,
            Func<bool> isReDisp,
            Action<string> appendToRecvBox,
            Action<string> writeLog,
            Action<string> setAngDisp,
            Func<bool> isAngDirDispEnabled,
            Action<string> onNewPeer = null)
        {
            _setRs = setRs;
            _isCharMode = isCharMode;
            _isHexMode = isHexMode;
            _isReDisp = isReDisp;
            _appendToRecvBox = appendToRecvBox;
            _writeLog = writeLog;
            _setAngDisp = setAngDisp;
            _isAngDirDispEnabled = isAngDirDispEnabled;
            _onNewPeer = onNewPeer;
        }

        public void ProcessReceivedMessage(Result status)
        {
            // 1. 计数
            _setRs(status.Num, RecvOrSend.Recv);

            // 2. 角度显示（如果启用）
            if (_isAngDirDispEnabled())
                _setAngDisp(status.Message);

            // 3. 处理新对端（如果有回调）
            if (!string.IsNullOrWhiteSpace(status.NewPeer) && _onNewPeer != null)
                _onNewPeer(status.NewPeer);

            string remote = status.RemoteIP ?? "BlueTooth";
            string prefix = $"{FromPrefix}{remote}: {NewLine}";

            if (_isCharMode())
            {
                AppendAndLog(prefix, status.Message);
            }
            else if (_isHexMode())
            {
                AppendAndLog(prefix, status.Message, isHex: true);
            }
        }

        public void ProcessSentMessage(Result status)
        {
            if (status.NetStatus != Result.NETStatus.SendMessage) return;

            if (status.status == Result.ResStatus.SetNum)
            {
                string remote = status.RemoteIP ?? "BlueTooth";
                string prefix = $"{ToPrefix}{remote}: {NewLine}";
                if (_isReDisp()) AppendAndLog(prefix, status.Message);
                else _writeLog($"{Utils.getTime()}{prefix}{status.Message}{NewLine}");

                _setRs(status.Num, RecvOrSend.Send);
            }
            else if (status.status == Result.ResStatus.Error)
            {
                _appendToRecvBox($"发送错误: {status.Message}\r\n");
            }
        }

        public void ProcessException(Result status)
        {
            if (status.NetStatus == Result.NETStatus.ExceptionStop)
            {
                _appendToRecvBox($"异常: {status.Message}\r\n");
            }
        }

        private void AppendAndLog(string prefix, string content, bool isHex = false)
        {
            string time = Utils.getTime();
            string displayContent = isHex ? Utils.StringToHexString(content) : content;
            string formatted = $"{time}{prefix}{displayContent}{NewLine}";
            _appendToRecvBox(formatted);
            _writeLog(formatted);
        }
    }
}
