using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeroAllocationSerialPort.NET
{
    internal class SerialConnection
    {
        private SerialPort serialPort;
        private readonly Pipe dataPipe = new();
        private string comPort { get; set; }
        private int BaudRate { get; set; }
        private Parser parser;

        public bool IsConnected() => serialPort.IsOpen;
        public bool Connect(string com, int baudRate, CancellationToken cancellationToken = default)
        {
            if (serialPort == null || !serialPort.IsOpen)
            {
                comPort = com;
                BaudRate = baudRate;
                serialPort = new SerialPort(comPort, baudRate);
                serialPort.Open();
                parser = new Parser(dataPipe.Reader);
                parser.Start();
                var writerTask = WriteToBufferAsync(dataPipe.Writer, cancellationToken);
                _ = writerTask;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Disconnect()
        {
            if (serialPort.IsOpen)
            {
                parser.Stop();
                serialPort.Close();
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool SendData(byte[] data)
        {
            if (serialPort.IsOpen)
            {
                serialPort.Write(data, 0, data.Length);
                return true;
            }
            else
            {
                return false;
            }
        }

        private async Task WriteToBufferAsync(PipeWriter writer, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    Memory<byte> memory = writer.GetMemory(1024);
                    int bytesRead = await serialPort.BaseStream.ReadAsync(memory, cancellationToken);

                    if (bytesRead > 0)
                    {
                        writer.Advance(bytesRead);
                        FlushResult result = await writer.FlushAsync(cancellationToken);

                        if (result.IsCompleted) break;
                    }
                }
            }
            finally
            {
                writer.Complete();
            }
        }


    }
}
