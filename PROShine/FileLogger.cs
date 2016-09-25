using System;
using System.IO;

namespace PROShine
{
    public class FileLogger
    {
        private const string LogsDirectory = "Logs";

        private StreamWriter _writer;
        private string _directory;
        private string _currentDate;

        public void OpenFile(string account, string server)
        {
            if (!Directory.Exists(LogsDirectory))
            {
                Directory.CreateDirectory(LogsDirectory);
            }
            _directory = Path.Combine(LogsDirectory, account + "-" + server);
            if (!Directory.Exists(_directory))
            {
                Directory.CreateDirectory(_directory);
            }
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
                string file = Path.Combine(_directory, _currentDate + ".txt");
                _writer = new StreamWriter(file, true, System.Text.Encoding.Default);
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

            string date = DateTime.Now.ToString("yyyy-MM-dd");
            if (date != _currentDate)
            {
                CreateStream();
                if (_writer == null) return;
            }

            _writer.WriteLine(message);
        }
    }
}
