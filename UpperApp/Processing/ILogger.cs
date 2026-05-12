namespace UpperApp.Processing
{
    internal interface ILogger
    {
        void WriteLine(string text);
        void Open(string filePath);
        void Close();
        bool IsOpen { get; }
    }
}
