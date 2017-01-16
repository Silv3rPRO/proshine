using System;

namespace PROBot.Modules
{
    public class OptionSlider
    {
        public event Action<bool, int> EnabledStateChanged;
        public event Action<string, int> NameChanged, DescriptionChanged;

        private bool _isEnabled = false;
        private string _name, _description;
        private int _index;
        
        public void Reset()
        {
            _isEnabled = false;
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    EnabledStateChanged?.Invoke(value, _index);
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
                    NameChanged?.Invoke(value, _index);
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
                    DescriptionChanged?.Invoke(value, _index);
                }
            }
        }

        public OptionSlider (string name, string description, int index)
        {
            _name = name;
            _description = description;
            _index = index;
        }
    }
}
