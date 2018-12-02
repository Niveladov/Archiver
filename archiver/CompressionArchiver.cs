using System;
using System.IO;
using System.IO.Compression;

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
                    byte[] buffer = null;
                    while (sourceStream.Position < sourceStream.Length && !isCancel)
                    {
                        if (sourceStream.Length - sourceStream.Position < Program.DATA_PORTION_SIZE)
                        {
                            buffer = new byte[(int)(sourceStream.Length - sourceStream.Position)];
                        }
                        else
                        {
                            buffer = new byte[Program.DATA_PORTION_SIZE];
                        }
                        sourceStream.Read(buffer, 0, buffer.Length);
                        queueIn.Enqueue(buffer);
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
                    BitConverter.GetBytes(comBuffer.Length).CopyTo(comBuffer, 4);
                    var blockOut = new Block(blockIn.Id, comBuffer);
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
