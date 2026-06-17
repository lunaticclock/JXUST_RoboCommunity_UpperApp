using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using UpperApp.Core;

namespace UpperApp.Services
{
    internal class DataPipeline : IDisposable
    {
        private readonly Channel<MessageReceivedEvent> _channel;
        private readonly CancellationTokenSource _cts;
        private readonly Action<MessageReceivedEvent> _processor;
        private readonly int _capacity;
        private Task _consumerTask;
        private long _droppedCount;

        public long DroppedCount => Interlocked.Read(ref _droppedCount);

        public DataPipeline(Action<MessageReceivedEvent> processor, int capacity = 1024)
        {
            _processor = processor;
            _capacity = capacity;
            _channel = Channel.CreateBounded<MessageReceivedEvent>(new BoundedChannelOptions(capacity)
            {
                SingleReader = true,
                SingleWriter = false,
                FullMode = BoundedChannelFullMode.DropOldest
            });
            _cts = new CancellationTokenSource();
        }

        public void Start()
        {
            _consumerTask = Task.Run(ConsumeLoop, _cts.Token);
        }

        public bool TryEnqueue(MessageReceivedEvent evt)
        {
            // DropOldest 模式下 TryWrite 总是返回 true，但通道满时会丢弃最旧数据
            // 在写入前检测是否将触发丢弃，记录日志便于排查数据丢失
            if (_channel.Reader.Count >= _capacity)
            {
                Interlocked.Increment(ref _droppedCount);
                Debug.WriteLine($"[WARN] DataPipeline 通道已满，丢弃最旧数据（累计丢弃 {Interlocked.Read(ref _droppedCount)} 条）");
            }
            return _channel.Writer.TryWrite(evt);
        }

        private async Task ConsumeLoop()
        {
            try
            {
                await foreach (var item in _channel.Reader.ReadAllAsync(_cts.Token))
                {
                    _processor(item);
                }
            }
            catch (OperationCanceledException) { }
        }

        public void Dispose()
        {
            _cts.Cancel();
            _channel.Writer.TryComplete();
            _consumerTask?.Wait(TimeSpan.FromSeconds(2));
            _cts.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
