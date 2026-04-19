using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Text;

namespace UpperApp
{
    [SupportedOSPlatform("windows10.0.17763.0")]
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

            // 4. 显示消息（字符/十六进制）
            if (_isCharMode())
            {
                // 字符模式直接追加消息内容
                _appendToRecvBox(Utils.getTime() + "from:" + status.RemoteIP + ": " + "\r\n" + status.Message + "\r\n");
                // 写入日志（带时间戳）
                _writeLog(Utils.getTime() + "from:" + status.RemoteIP + ": " + "\r\n" + status.Message + "\r\n");
            }
            else if (_isHexMode())
            {
                // 十六进制模式调用转换显示
                _appendToRecvBox(Utils.getTime() + "from:" + status.RemoteIP + ": " + "\r\n" + Utils.StringToHexString(status.Message) + "\r\n");
                _writeLog(Utils.getTime() + "from:" + status.RemoteIP + ": " + "\r\n" + Utils.StringToHexString(status.Message) + "\r\n");
            }
        }

        public void ProcessSentMessage(Result status)
        {
            if (status.NetStatus != Result.NETStatus.SendMessage) return;

            if (status.status == Result.ResStatus.SetNum)
            {
                // 发送成功，可选是否回显
                if (_isReDisp()) _appendToRecvBox(Utils.getTime() + "to:" + status.RemoteIP + ": " + "\r\n" + status.Message + "\r\n");
                _writeLog(Utils.getTime() + "to:" + status.RemoteIP + ": " + "\r\n" + status.Message + "\r\n");
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
    }
}
