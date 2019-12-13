using PROProtocol;
using System;

namespace PROBot.Modules
{
    public class AutoReconnector
    {
        public const int MinDelay = 180;
        public const int MaxDelay = 420;

        public event Action<bool> StateChanged;

        private bool _isEnabled = false;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    StateChanged?.Invoke(value);
                }
            }
        }

        private BotClient _bot;
        private bool _reconnecting;
        private bool _relogging;
        private double _reloggingDelay;
        private DateTime _autoReconnectTimeout;
        private DateTime _botDelay = DateTime.MaxValue;

        public AutoReconnector(BotClient bot)
        {
            _bot = bot;
            _bot.ClientChanged += Bot_ClientChanged;
        }

        private void Bot_ClientChanged()
        {
            if (_bot.Game != null)
            {
                _bot.Game.ConnectionClosed += Client_ConnectionClosed;
                _bot.Game.ConnectionFailed += Client_ConnectionClosed;
                _bot.Game.LoggedIn += Client_LoggedIn;
                _bot.Game.AuthenticationFailed += Client_AuthenticationFailed;
            }
        }

        public void Update()
        {
            if ((IsEnabled || _relogging) && _reconnecting && (_bot.Game == null || !_bot.Game.IsConnected))
            {
                if (_autoReconnectTimeout < DateTime.UtcNow)
                {
                    _bot.LogMessage("Reconnecting...");
                    _bot.Login(_bot.Account);
                    _autoReconnectTimeout = DateTime.UtcNow.AddSeconds(_bot.Rand.Next(MinDelay, MaxDelay + 1));
                }
            }

            if (_botDelay < DateTime.UtcNow)
            {
                _bot.Start();
                _botDelay = DateTime.MaxValue;
            }
        }

        public void Relog(double delay)
        {
            _relogging = true;
            _reloggingDelay = delay;
        }

        private void Client_ConnectionClosed(Exception ex)
        {
            if (IsEnabled || _relogging)
            {
                _reconnecting = true;
                double seconds = _relogging ? _reloggingDelay : _bot.Rand.Next(MinDelay, MaxDelay + 1);

                _autoReconnectTimeout = DateTime.UtcNow.AddSeconds(seconds);
                _bot.LogMessage("Reconnecting in " + seconds + " seconds.");
            }
        }

        private void Client_LoggedIn()
        {
            if (_reconnecting)
            {
                _reconnecting = false;
                _relogging = false;
                _botDelay = DateTime.UtcNow.AddSeconds(1);
            }
        }

        private void Client_AuthenticationFailed(AuthenticationResult result)
        {
            _relogging = false;
            if (result == AuthenticationResult.InvalidUser || result == AuthenticationResult.InvalidPassword
                || result == AuthenticationResult.InvalidVersion || result == AuthenticationResult.Banned
                || result == AuthenticationResult.EmailNotActivated)
            {
                IsEnabled = false;
                _reconnecting = false;
            }
        }
    }
}
