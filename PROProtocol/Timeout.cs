using System;

namespace PROProtocol
{
    public class Timeout
    {
        private DateTime _expirationTime;
        public bool IsActive { get; private set; }

        public bool Update()
        {
            if (IsActive && DateTime.UtcNow >= _expirationTime)
                IsActive = false;
            return IsActive;
        }

        public void Set(int milliseconds = 10000)
        {
            IsActive = true;
            _expirationTime = DateTime.UtcNow.AddMilliseconds(milliseconds);
        }

        public void Cancel()
        {
            IsActive = false;
        }
    }
}