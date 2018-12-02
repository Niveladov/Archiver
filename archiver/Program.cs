using System;

namespace archiver
{
    class Program
    {
        internal static int DATA_PORTION_SIZE = 1000000; // 1 Мб

        private const int RATIO = 3;
        private static Archiver _archiver;

        private static void Main(string[] args)
        {
#if DEBUG
            args = new string[3];
            args[0] = "compress";
            args[1] = @"F:/MyProjects/IMG.jpg";
            args[2] = @"F:/MyProjects/IMG.gz";
#endif
            if (args.Length == 3)
            {
                var inputData = new InputData(args[0], args[1], args[2]);
                if (inputData.IsValid)
                {
                    Console.CancelKeyPress += Console_CancelKeyPress;
                    _archiver = inputData.GetArchiver();
                    _archiver.Run();
                    Console.ReadLine();
                }
                else
                {
                    Console.WriteLine("Ошибка входных данных!");
                }
            }
            else
            {
                Console.WriteLine("Неверное количество входных параметров!");
            }
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            if (e.SpecialKey == ConsoleSpecialKey.ControlC)
            {
                Console.WriteLine("Отмена работы...");
                _archiver.Cancel();
            }
        }

        public static int GetQueueLimit()
        {
            var totalMemory = new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory;
            var memoryForOneBlock = (ulong)(DATA_PORTION_SIZE * RATIO);
            return (int)(totalMemory / memoryForOneBlock);
        }
    }
}

