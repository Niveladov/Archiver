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
        private static Archiver _archiver = new DecompressionArchiver(@"F:/MyProjects/IMG.gz", @"F:/MyProjects/IMG_n.jpg");
        //protected string sourceFile { get; } = @"F:/MyProjects/models.cs";
        //protected string compressedFile { get; } = @"F:/MyProjects/models.gz";
        //protected string targetFile { get; } = @"F:/MyProjects/models_new.cs";
        //protected string _sourceFile = @"D:/Niveladov/models.txt";
        //protected string _compressedFile = @"D:/Niveladov/models.gz";
        //protected string _targetFile = @"D:/Niveladov/models_new.txt";

        private static void Main(string[] args)
        {
            //var i = 1000000;
            //var a = new byte[8];
            //var b = BitConverter.GetBytes(i);
            //b.CopyTo(a, 4);
            //var c = BitConverter.ToInt32(a, 4); 
            _archiver.Run();
            Console.ReadLine();
        }


    }
}

