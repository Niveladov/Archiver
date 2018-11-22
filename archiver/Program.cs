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
        static ProducerConsumer _queueIn = new ProducerConsumer();
        static ProducerConsumer _queueOut = new ProducerConsumer();
        //static string _sourceFile = @"F:/MyProjects/models.cs"; // исходный файл
        //static string _compressedFile = @"F:/MyProjects/models.gz"; // сжатый файл
        //static string _targetFile = @"F:/MyProjects/models_new.cs"; // восстановленный файл

        static string _sourceFile = @"D:/Niveladov/models.txt"; // исходный файл
        static string _compressedFile = @"D:/Niveladov/models.gz"; // сжатый файл
        //static string _targetFile = @"D:/Niveladov/models_new.txt"; // восстановленный файл
        static int _pocessorCount = Environment.ProcessorCount;
        static bool _isCancel = false;

        private static void Main(string[] args)
        {
            Run();
            Console.ReadLine();
        }

        private static void Run()
        {
            var readingThread = new Thread(ReadFromFile);
            readingThread.Start();
            Console.WriteLine("Погнали!!!♦♦♦♦♦");

            var threadPool = new Thread[_pocessorCount];
            for (int i = 0; i < _pocessorCount; i++)
            {
                threadPool[i] = new Thread(DoWork);
                threadPool[i].Start();
            }

            Console.WriteLine("Ждём-с!!!");
            threadPool.WaitAll();

            _queueOut.Stop();

            WriteToFile();
            //var writingThread = new Thread(WriteToFile);
            //writingThread.Start();
            //writingThread.Join();

            Console.WriteLine("Усё!!!");

            //readingThread.Join();

            //qqqqqqqqq.Stop();
        }
        
        
        public static void ReadFromFile()
        {
            try
            {
                using (FileStream sourceStream = new FileStream(_sourceFile, FileMode.OpenOrCreate))
                {
                    byte[] buffer = new byte[sourceStream.Length % 1000000];
                    int dataLength;
                    while (sourceStream.Position < sourceStream.Length)
                    {
                        dataLength = sourceStream.Read(buffer, 0, buffer.Length);
                        _queueIn.Enqueue(buffer);
                        buffer = new byte[1000000];
                    }
                    _queueIn.Stop();
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
                while (!_isCancel)
                {
                    var blockIn = _queueIn.Dequeue();
                    if (blockIn == null) return;
                    using (var memoryStream = new MemoryStream())
                    {
                        using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                        {
                            gZipStream.Write(blockIn.Buffer, 0, blockIn.Buffer.Length);
                            var bytes = memoryStream.ToArray();
                            var blockOut = new Block(blockIn.Id, bytes);
                            _queueOut.Enqueue(blockOut);
                        }
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
                using (FileStream targetStream = File.Create(_compressedFile))
                {
                    while (!_isCancel)
                    {
                        var block = _queueOut.Dequeue();
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

