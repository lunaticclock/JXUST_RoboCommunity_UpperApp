using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using UpperApp.Core;

namespace UpperApp.Services
{
    internal class DataPipeline : IDisposable
    {
        private readonly Channel<Result> _channel;
        private readonly CancellationTokenSource _cts;
        private readonly Action<Result> _processor;
        private Task _consumerTask;

        public DataPipeline(Action<Result> processor, int capacity = 1024)
        {
            _processor = processor;
            _channel = Channel.CreateBounded<Result>(new BoundedChannelOptions(capacity)
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

        public bool TryEnqueue(Result result)
        {
            return _channel.Writer.TryWrite(result);
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
