using System;
using System.IO;

namespace UpperApp.Processing
{
    internal class FileLogger : ILogger, IDisposable
    {
        private StreamWriter _writer;

        public bool IsOpen => _writer != null;

        public void WriteLine(string text)
        {
            if (_writer == null)
                Open(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt"));
            _writer?.WriteLine(text);
            _writer?.Flush();
        }

        public void Open(string filePath)
        {
            Close();
            _writer = new StreamWriter(filePath, true);
        }

        public void Close()
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
