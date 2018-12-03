using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace archiver
{
    internal abstract class Archiver
    {
        private int _pocessorCount = Environment.ProcessorCount;
        
        protected ProducerConsumer queueIn { get; } 
        protected ProducerConsumer queueOut { get; }
        protected string sourceFile { get; }
        protected string targetFile { get; }
        protected bool isCancel { get; set; } = false;
        
        public Archiver(string sourceFile, string targetFile)
        {
            this.sourceFile = sourceFile;
            this.targetFile = targetFile;
            var queueLimit = Program.GetQueueLimit();
            queueIn = new ProducerConsumer(queueLimit);
            queueOut = new ProducerConsumer(queueLimit);
        }

        public void Cancel()
        {
            isCancel = true;
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

            if (!isCancel)
            {
                Console.WriteLine("Успешно завершено!");
            }
            else
            {
                Console.WriteLine("Завершено! Работа не выполнена!");
            }
        }

        protected abstract void ReadFromFile();

        protected abstract void DoWork();

        protected abstract void WriteToFile();
    }
}
