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
                    var emptyBuffer = new byte[SYSTEM_DATA_START_POSITION];
                    var systemDataLength = SYSTEM_DATA_START_POSITION + BitConverter.GetBytes(Int32.MaxValue).Length;
                    var systemDataBuffer = new byte[systemDataLength];
                    while (sourceStream.Position < sourceStream.Length)
                    {
                        sourceStream.Read(systemDataBuffer, 0, systemDataLength);
                        var blockLength = BitConverter.ToInt32(systemDataBuffer, SYSTEM_DATA_START_POSITION);
                        var blockBuffer = new byte[blockLength];

                        systemDataBuffer.CopyTo(blockBuffer, 0);
                        emptyBuffer.CopyTo(blockBuffer, SYSTEM_DATA_START_POSITION);
                        sourceStream.Read(blockBuffer, systemDataLength, blockLength - systemDataLength);

                        var blockSize = BitConverter.ToInt32(blockBuffer, blockLength - 4);
                        queueIn.Enqueue(blockBuffer);
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
                    using (var memoryStream = new MemoryStream(blockIn.Buffer))
                    {
                        using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                        {
                            var decompressBuffer = new byte[DATA_PORTION_SIZE];
                            var bufferLength = gZipStream.Read(decompressBuffer, 0, DATA_PORTION_SIZE);
                            //if (bufferLength != DATA_PORTION_SIZE)
                            //{
                            //    Array.Resize(ref decompressBuffer, bufferLength);
                            //}
                            var blockOut = new Block(blockIn.Id, decompressBuffer);
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
