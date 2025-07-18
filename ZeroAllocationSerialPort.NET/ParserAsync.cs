using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeroAllocationSerialPort.NET
{
    internal class ParserAsync(PipeReader reader, int bufferSize = 1024)
    {
        private readonly PipeReader reader = reader;
        private byte[] buffer = new byte[bufferSize];
        private Task _processingTask;
        private CancellationTokenSource _cancellationTokenSource;


        public void Start()
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();
            _processingTask = Task.Run(() => ProcessDataAsync());
        }
        public async void Stop()
        {
            if (_cancellationTokenSource == null || _cancellationTokenSource.Token.IsCancellationRequested)
            {
                return;
            }
            _cancellationTokenSource.Cancel();

            try
            {
                if (_processingTask != null)
                {
                    await _processingTask;
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
                _processingTask = null;
            }
        }

        private async Task ProcessDataAsync()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                var result = await reader.ReadAsync();
                var buff = result.Buffer;
                SequencePosition consumed;
                SequencePosition examined;
                ProcessBuffer(buff, out consumed, out examined);
                reader.AdvanceTo(buff.End);
            }
        }

        private void ProcessBuffer(in ReadOnlySequence<byte> sequence, out SequencePosition consumed, out SequencePosition examined)
        {
            consumed = sequence.Start;
            examined = sequence.End;
            if (sequence.IsSingleSegment)
            {
                var span = sequence.First.Span;
                Consume(in span);
            }
            else
            {
                foreach (var segment in sequence)
                {
                    var span = segment.Span;
                    Consume(in span);

                }
            }
        }

        private void Consume(in ReadOnlySpan<byte> data)
        {
            var buffer = this.buffer.AsSpan();
            // Consume data
        }
    }
}
