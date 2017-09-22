using System;
using System.IO;
using System.Text;

namespace PROShine
{
    public class FileLogger
    {
        private const string LogsDirectory = "Logs";
        private string _currentDate;
        private string _directory;

        private StreamWriter _writer;

        public void OpenFile(string account, string server)
        {
            if (!Directory.Exists(LogsDirectory))
                Directory.CreateDirectory(LogsDirectory);
            _directory = Path.Combine(LogsDirectory, account + "-" + server);
            if (!Directory.Exists(_directory))
                Directory.CreateDirectory(_directory);
            CreateStream();
        }

        public void CloseFile()
        {
            if (_writer != null)
            {
                _writer.Close();
                _writer = null;
            }
        }

        private void CreateStream()
        {
            try
            {
                CloseFile();

                _currentDate = DateTime.Now.ToString("yyyy-MM-dd");
                var file = Path.Combine(_directory, _currentDate + ".txt");
                _writer = new StreamWriter(file, true, Encoding.Default);
                _writer.AutoFlush = true;
            }
            catch (Exception)
            {
                _writer = null;
            }
        }

        public void Append(string message)
        {
            if (_writer == null) return;

            var date = DateTime.Now.ToString("yyyy-MM-dd");
            if (date != _currentDate)
            {
                CreateStream();
                if (_writer == null) return;
            }

            _writer.WriteLine(message);
        }
    }
}