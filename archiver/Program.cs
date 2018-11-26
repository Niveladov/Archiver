using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace archiver
{
    class Program
    {
        private const string COMPRESS_MODE = "compress";
        private const string DECOMPRESS_MODE = "decompress";
        private const string FILENAME_PATTERN = @"\w+\.\w+";

        private static string _mode;
        private static string _sourceFile;
        private static string _targetFile;

        private static Archiver _archiver;

        private static bool _isValid
        {
            get
            {
                return (_mode.ToLower().Equals(COMPRESS_MODE) || _mode.ToLower().Equals(DECOMPRESS_MODE))
                    && File.Exists(_sourceFile) && Regex.IsMatch(_targetFile, FILENAME_PATTERN);
            }
        }

        private static void Main(string[] args)
        {
#if DEBUG
            args = new string[3];
            args[0] = "decompress";
            args[1] = @"F:/MyProjects/IMG.gz";
            args[2] = @"F:/MyProjects/IMG_NEW.jpg";
#endif
            if (args.Length == 3)
            {
                _mode = args[0];
                _sourceFile = args[1];
                _targetFile = args[2];
                if (_isValid)
                {
                    switch(_mode.ToLower())
                    {
                        case COMPRESS_MODE:
                            _archiver = new CompressionArchiver(_sourceFile, _targetFile);
                            break;
                        case DECOMPRESS_MODE:
                            _archiver = new DecompressionArchiver(_sourceFile, _targetFile);
                            break;
                    }
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

    }
}

