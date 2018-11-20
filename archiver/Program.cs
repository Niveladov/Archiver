using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;

namespace archiver
{
    class Program
    {
        static ProducerConsumer queueIn = new ProducerConsumer();
        static ProducerConsumer queueOut = new ProducerConsumer();
        static string sourceFile = @"F:/MyProjects/models.cs"; // исходный файл
        static string compressedFile = @"F:/MyProjects/models.gz"; // сжатый файл
        static string targetFile = @"F:/MyProjects/models_new.cs"; // восстановленный файл
        static int _pocessorCount = Environment.ProcessorCount;

        private static void Main(string[] args)
        {
            Run();
            Console.ReadLine();
        }

        private static void Run()
        {
            var readingThread = new Thread(ReadFromFile);
            readingThread.Start();

            var threadPool = new Thread[_pocessorCount];
            for (int i = 0; i < _pocessorCount; i++)
            {
                threadPool[i] = new Thread(DoWork);
                threadPool[i].Start();
            }

            threadPool.WaitAll();
            //readingThread.Join();

            var writingThread = new Thread(WriteToFile);
            writingThread.Start();

            //qqqqqqqqq.Stop();
        }
        
        
        public static void ReadFromFile()
        {
            try
            {
                using (FileStream sourceStream = new FileStream(sourceFile, FileMode.OpenOrCreate))
                {
                    byte[] buffer = new byte[1024];
                    int dataLength;
                    int blockId = 1;
                    while (sourceStream.Position < sourceStream.Length)
                    {
                        dataLength = sourceStream.Read(buffer, 0, buffer.Length);
                        var block = new Block(blockId, buffer);
                        queueIn.Enqueue(block);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        
        public static void DoWork()
        {
            try
            {
                var blockIn = queueIn.Dequeue();
                if (blockIn == null) return;
                using (var memoryStream = new MemoryStream())
                {
                    using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                    {
                        gZipStream.Write(blockIn.Buffer, 0, blockIn.Buffer.Length);
                        var bytes = memoryStream.ToArray();
                        var blockOut = new Block(blockIn.Id, bytes);
                        queueOut.Enqueue(blockOut);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static void WriteToFile()
        {
            try
            {
                using (FileStream targetStream = File.Create(compressedFile))
                {
                    var block = queueOut.Dequeue();
                    if (block == null) return;
                    targetStream.Write(block.Buffer, 0, block.Buffer.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //public static void Decompress(string compressedFile, string targetFile)
        //{
        //    // поток для чтения из сжатого файла
        //    using (FileStream sourceStream = new FileStream(compressedFile, FileMode.OpenOrCreate))
        //    {
        //        // поток для записи восстановленного файла
        //        using (FileStream targetStream = File.Create(targetFile))
        //        {
        //            // поток разархивации
        //            using (GZipStream decompressionStream = new GZipStream(sourceStream, CompressionMode.Decompress))
        //            {
        //                decompressionStream.CopyTo(targetStream);
        //                Console.WriteLine("Восстановлен файл: {0}", targetFile);
        //            }
        //        }
        //    }
        //}


    }
}

