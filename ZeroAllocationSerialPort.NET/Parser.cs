using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroAllocationSerialPort.NET
{
    internal class Parser(PipeReader reader,int bufferSize=1024)
    {
        private readonly PipeReader reader = reader;
        private byte[] buffer= new byte[bufferSize];
        private Task _processingTask;


        public void Start()
        {
            _processingTask = Task.Run(() => ProcessDataAsync());
        }
        public void Stop()
        {
            if (_processingTask != null)
                _processingTask.Dispose();
        }

        private Task ProcessDataAsync()
        {
            while (true)
            {
                if (reader.TryRead(out var result))
                {
                    var buff = result.Buffer;
                    SequencePosition consumed;
                    SequencePosition examined;
                    ProcessBuffer(buff, out consumed, out examined);
                    reader.AdvanceTo(buff.End);
                }
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
