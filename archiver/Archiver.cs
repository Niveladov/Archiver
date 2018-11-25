using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace archiver
{
    internal abstract class Archiver
    {
        protected const int DATA_PORTION_SIZE = 1000000; // 1 Мб

        private int _pocessorCount = Environment.ProcessorCount;
        
        protected ProducerConsumer queueIn { get; } = new ProducerConsumer();
        protected ProducerConsumer queueOut { get; } = new ProducerConsumer();
        protected string sourceFile { get; }
        protected string targetFile { get; }
        protected bool isCancel { get; set; } = false;
        
        public Archiver(string sourceFile, string targetFile)
        {
            this.sourceFile = sourceFile;
            this.targetFile = targetFile;
        }

        public void Run()
        {
            Console.WriteLine("Ждите, идёт работа...");

            var readingThread = new Thread(ReadFromFile);
            readingThread.Start();

            var threadPool = new Thread[_pocessorCount];
            for (int i = 0; i < _pocessorCount; i++)
            {
                threadPool[i] = new Thread(DoWork);
                threadPool[i].Start();
            }

            var writingThread = new Thread(WriteToFile);
            writingThread.Start(); 

            threadPool.WaitAll();
            queueOut.Stop();

            Console.WriteLine("Завершено!");
        }

        protected abstract void ReadFromFile();

        protected abstract void DoWork();

        protected abstract void WriteToFile();
    }
}
