using System;

namespace PROBot.Modules
{
    public class OptionSlider
    {
        public event Action<bool> EnabledStateChanged;
        public event Action<string> NameChanged, DescriptionChanged;

        private bool _isEnabled = false;
        private string _name, _description;

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    EnabledStateChanged?.Invoke(value);
                }
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    NameChanged?.Invoke(value);
                }
            }
        }

        public string Description
        {
            get { return _description; }
            set
            {
                if (_description != value)
                {
                    _description = value;
                    DescriptionChanged?.Invoke(value);
                }
            }
        }

        public OptionSlider (string name, string description)
        {
            _name = name;
            _description = description;
        }
    }
}
