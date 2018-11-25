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
                    var decomLengthBuffer = new byte[4];
                    var comLengthBuffer = new byte[4];
                    while (sourceStream.Position < sourceStream.Length)
                    {
                        sourceStream.Read(decomLengthBuffer, 0, decomLengthBuffer.Length);

                        sourceStream.Read(comLengthBuffer, 0, comLengthBuffer.Length);
                        var comBufferLength = BitConverter.ToInt32(comLengthBuffer, 0);
                        
                        var comBuffer = new byte[comBufferLength];
                        sourceStream.Read(comBuffer, 0, comBuffer.Length);

                        var buffer = new byte[decomLengthBuffer.Length + comBufferLength];
                        decomLengthBuffer.CopyTo(buffer, 0);
                        comBuffer.CopyTo(buffer, decomLengthBuffer.Length);
                        
                        queueIn.Enqueue(buffer);
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
                            gZipStream.Read(decomBuffer, 0, decomLength);
                        }
                    }
                    var blockOut = new Block(blockIn.Id, decomBuffer);
                    queueOut.Enqueue(blockOut);
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
