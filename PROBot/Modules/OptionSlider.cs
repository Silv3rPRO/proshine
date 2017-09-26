using System.ComponentModel;

namespace PROBot.Modules
{
    public class OptionSlider : INotifyPropertyChanged
    {
        private bool _isEnabled;
        private string _name, _description;

        public OptionSlider(string name, string description)
        {
            _name = name;
            _description = description;
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsEnabled"));
                }
            }
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

        public event PropertyChangedEventHandler PropertyChanged;
    }
}