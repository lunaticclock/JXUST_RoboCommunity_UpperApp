using System;
using System.IO;
using System.Threading;

namespace UpperApp.Processing
{
    internal class FileLogger : ILogger, IDisposable
    {
        private StreamWriter _writer;
        private readonly object _lock = new();

        public bool IsOpen
        {
            get
            {
                lock (_lock) { return _writer != null; }
            }
        }

        public void WriteLine(string text)
        {
            lock (_lock)
            {
                if (_writer == null)
                    OpenInternal(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt"));
                _writer?.WriteLine(text);
                _writer?.Flush();
            }
        }

        public void Open(string filePath)
        {
            lock (_lock)
            {
                OpenInternal(filePath);
            }
        }

        private void OpenInternal(string filePath)
        {
            CloseInternal();
            _writer = new StreamWriter(filePath, true);
        }

        public void Close()
        {
            lock (_lock)
            {
                CloseInternal();
            }
        }

        private void CloseInternal()
        {
            _writer?.Close();
            _writer = null;
        }

        public void Dispose()
        {
            Close();
            GC.SuppressFinalize(this);
        }
    }
}
