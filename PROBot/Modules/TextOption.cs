using System.ComponentModel;

namespace PROBot.Modules
{
    public class TextOption : INotifyPropertyChanged
    {
        private string _name, _description, _content;

        public TextOption(string name, string description, string content)
        {
            _name = name;
            _description = description;
            _content = content;
        }

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
                }
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Description"));
                }
            }
        }

        public string Content
        {
            get => _content;
            set
            {
                if (_content != value)
                {
                    _content = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Content"));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}