using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

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
                    var lengthBuffer = new byte[8];
                    while (sourceStream.Position < sourceStream.Length && !isCancel)
                    {
                        sourceStream.Read(lengthBuffer, 0, lengthBuffer.Length);
                        var comBufferLength = BitConverter.ToInt32(lengthBuffer, 4);
                        
                        var comBuffer = new byte[comBufferLength];
                        lengthBuffer.Take(4).ToArray().CopyTo(comBuffer, 0);
                        sourceStream.Read(comBuffer, 8, comBuffer.Length - 8);
                        
                        queueIn.Enqueue(comBuffer);
                    }
                    queueIn.Stop();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                isCancel = true;
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
                    var decomBuffer = new byte[Program.DATA_PORTION_SIZE];
                    int readedData = 0;
                    Block blockOut = null;
                    using (var memoryStream = new MemoryStream(blockIn.Buffer))
                    {
                        using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                        {
                            readedData = gZipStream.Read(decomBuffer, 0, decomBuffer.Length);
                        }
                    }
                    if (readedData < Program.DATA_PORTION_SIZE)
                    {
                        blockOut = new Block(blockIn.Id, decomBuffer.Take(readedData).ToArray());
                    }
                    else
                    {
                        blockOut = new Block(blockIn.Id, decomBuffer);
                    }
                    queueOut.Enqueue(blockOut);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                isCancel = true;
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
                isCancel = true;
            }
        }
    }
}
