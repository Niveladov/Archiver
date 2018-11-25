using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace archiver
{
    internal sealed class CompressionArchiver : Archiver
    {
        public CompressionArchiver(string sourceFile, string targetFile) : base(sourceFile, targetFile) { }

        protected sealed override void ReadFromFile()
        {
            try
            {
                using (var sourceStream = new FileStream(sourceFile, FileMode.OpenOrCreate))
                {
                    var buffer = new byte[sourceStream.Length % DATA_PORTION_SIZE];
                    while (sourceStream.Position < sourceStream.Length)
                    {
                        sourceStream.Read(buffer, 0, buffer.Length);
                        queueIn.Enqueue(buffer);
                        buffer = new byte[DATA_PORTION_SIZE];
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
                    byte[] comBuffer = null;
                    var blockIn = queueIn.Dequeue();
                    if (blockIn == null) return;
                    using (var memoryStream = new MemoryStream())
                    {
                        using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                        {
                            gZipStream.Write(blockIn.Buffer, 0, blockIn.Buffer.Length);
                        }
                        comBuffer = memoryStream.ToArray();
                    }
                    var decomBufferLengthArray = BitConverter.GetBytes(blockIn.Buffer.Length);
                    var comBufferLengthArray = BitConverter.GetBytes(comBuffer.Length);
                    var fullBuffer = new byte[comBuffer.Length + comBufferLengthArray.Length + decomBufferLengthArray.Length];
                    decomBufferLengthArray.CopyTo(fullBuffer, 0);
                    comBufferLengthArray.CopyTo(fullBuffer, decomBufferLengthArray.Length);
                    comBuffer.CopyTo(fullBuffer, decomBufferLengthArray.Length + comBufferLengthArray.Length);
                    var blockOut = new Block(blockIn.Id, fullBuffer);
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
