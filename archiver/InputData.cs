using System.IO;
using System.Text.RegularExpressions;

namespace archiver
{
    internal sealed class InputData
    {
        private const string COMPRESS_MODE = "compress";
        private const string DECOMPRESS_MODE = "decompress";
        private const string FILENAME_PATTERN = @"\w+\.\w+";

        private string _mode;
        private string _sourceFile;
        private string _targetFile;

        public bool IsValid
        {
            get
            {
                return (_mode.ToLower().Equals(COMPRESS_MODE) || _mode.ToLower().Equals(DECOMPRESS_MODE))
                    && File.Exists(_sourceFile) && Regex.IsMatch(_targetFile, FILENAME_PATTERN);
            }
        }

        public InputData(string mode, string sourceFile, string targetFile)
        {
            _mode = mode;
            _sourceFile = sourceFile;
            _targetFile = targetFile;
        }

        public Archiver GetArchiver()
        {
            switch (_mode.ToLower())
            {
                case COMPRESS_MODE:
                    return new CompressionArchiver(_sourceFile, _targetFile);
                case DECOMPRESS_MODE:
                    return new DecompressionArchiver(_sourceFile, _targetFile);
                default:
                    return null;
            }
        }


    }
}
