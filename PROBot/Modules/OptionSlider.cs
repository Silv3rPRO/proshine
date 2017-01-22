using System;

namespace PROBot.Modules
{
    public class OptionSlider
    {
        public event Action StateChanged;

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
                    StateChanged?.Invoke();
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
                    StateChanged?.Invoke();
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
                    StateChanged?.Invoke();
                }
            }
        }

        public OptionSlider(string name, string description)
        {
            _name = name;
            _description = description;
        }
    }
}
