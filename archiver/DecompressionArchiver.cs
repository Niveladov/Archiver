using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace archiver
{
    internal sealed class DecompressionArchiver : Archiver
    {
        public DecompressionArchiver(string sourceFile, string targetFile) : base(sourceFile, targetFile) { }

        protected sealed override void ReadFromFile()
        {
            try
            {
                using (var sourceStream = new FileStream(sourceFile, FileMode.Open))
                {
                    var lengthDataBuffer = new byte[4];
                    while (sourceStream.Position < sourceStream.Length)
                    {
                        sourceStream.Read(lengthDataBuffer, 0, lengthDataBuffer.Length);
                        var decomBufferLengthArray = lengthDataBuffer;
                        var decomBufferLength = BitConverter.ToInt32(lengthDataBuffer, 0);

                        sourceStream.Read(lengthDataBuffer, 0, lengthDataBuffer.Length);
                        var comBufferLength = BitConverter.ToInt32(lengthDataBuffer, 0);
                        
                        var buffer = new byte[comBufferLength];
                        sourceStream.Read(buffer, 0, buffer.Length);

                        var comBuffer = new byte[decomBufferLengthArray.Length + buffer.Length];
                        decomBufferLengthArray.CopyTo(comBuffer, 0);
                        buffer.CopyTo(comBuffer, decomBufferLengthArray.Length);
                        
                        queueIn.Enqueue(comBuffer);
                    }
                    queueIn.Stop();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        protected sealed override void DoWork()
        {
            try
            {
                while (!isCancel)
                {
                    var blockIn = queueIn.Dequeue();
                    if (blockIn == null) return;
                    var lengthDataBuffer = new byte[4];
                    blockIn.Buffer.Take(4).ToArray().CopyTo(lengthDataBuffer, 0);
                    var decomLength = BitConverter.ToInt32(lengthDataBuffer, 0);
                    var decomBuffer = new byte[decomLength];
                    using (var memoryStream = new MemoryStream(blockIn.Buffer.Skip(4).ToArray()))
                    {
                        using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                        {
                            var bufferLength = gZipStream.Read(decomBuffer, 0, decomLength);
                            var blockOut = new Block(blockIn.Id, decomBuffer);
                            queueOut.Enqueue(blockOut);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        protected sealed override void WriteToFile()
        {
            try
            {
                using (var targetStream = File.Create(targetFile))
                {
                    while (!isCancel)
                    {
                        var block = queueOut.Dequeue();
                        if (block == null) return;
                        targetStream.Write(block.Buffer, 0, block.Buffer.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
