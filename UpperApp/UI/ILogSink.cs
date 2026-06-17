using System;
using System.Windows.Controls;

namespace UpperApp.UI
{
    /// <summary>
    /// 日志输出接口。View 注入实现，ViewModel 通过此接口增量追加文本，
    /// 避免持有大字符串导致的全量重绘。
    /// </summary>
    public interface ILogSink
    {
        /// <summary>增量追加文本（内部走 TextBox.AppendText，O(log n)）</summary>
        void Append(string text);

        /// <summary>清空全部内容</summary>
        void Clear();
    }

    /// <summary>
    /// 基于 TextBox.AppendText 的流式日志实现。
    /// 利用 WPF TextContainer 的增量插入，避免全量字符串赋值带来的 UI 卡顿。
    /// </summary>
    public sealed class TextBoxLogSink : ILogSink
    {
        private readonly TextBox _textBox;
        private readonly int _maxLength;
        private bool _autoScroll = true;

        /// <param name="textBox">目标 TextBox</param>
        /// <param name="maxLength">最大字符数，超出后从头部截断</param>
        public TextBoxLogSink(TextBox textBox, int maxLength = 500_000)
        {
            _textBox = textBox ?? throw new ArgumentNullException(nameof(textBox));
            _maxLength = maxLength > 0 ? maxLength : 500_000;
        }

        public void Append(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            // 用户手动滚动查看历史时暂停自动滚动
            _autoScroll = IsAtBottom();

            _textBox.AppendText(text);

            // 超长截断：从头部移除多余字符
            if (_textBox.Text.Length > _maxLength)
            {
                _textBox.Text = _textBox.Text[^_maxLength..];
            }

            if (_autoScroll)
                _textBox.ScrollToEnd();
        }

        public void Clear()
        {
            _textBox.Clear();
        }

        private bool IsAtBottom()
        {
            if (_textBox.Template.FindName("PART_ContentHost", _textBox) is not ScrollViewer sv) return true;
            return sv.VerticalOffset >= sv.ScrollableHeight - 1;
        }
    }
}
