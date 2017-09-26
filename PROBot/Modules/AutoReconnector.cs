using PROProtocol;
using System;

namespace PROBot.Modules
{
    public class AutoReconnector
    {
        public const int MinDelay = 15;
        public const int MaxDelay = 101;
        private DateTime _autoReconnectTimeout;

        private readonly BotClient _bot;

        private bool _isEnabled;
        private bool _reconnecting;

        public AutoReconnector(BotClient bot)
        {
            _bot = bot;
            _bot.ClientChanged += Bot_ClientChanged;
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    StateChanged?.Invoke(value);
                }
            }
        }

        public event Action<bool> StateChanged;

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
            if (IsEnabled && _reconnecting && (_bot.Game == null || !_bot.Game.IsConnected))
                if (_autoReconnectTimeout < DateTime.UtcNow)
                {
                    _bot.LogMessage("Reconnecting...");
                    _bot.Login(_bot.Account);
                    _autoReconnectTimeout = DateTime.UtcNow.AddSeconds(_bot.Rand.Next(MinDelay, MaxDelay + 1));
                }
        }

        private void Client_ConnectionClosed(Exception ex)
        {
            if (IsEnabled)
            {
                _reconnecting = true;
                var seconds = _bot.Rand.Next(MinDelay, MaxDelay + 1);
                _autoReconnectTimeout = DateTime.UtcNow.AddSeconds(seconds);
                _bot.LogMessage("Reconnecting in " + seconds + " seconds.");
            }
        }

        private void Client_LoggedIn()
        {
            if (_reconnecting)
            {
                _bot.Start();
                _reconnecting = false;
            }
        }

        private void Client_AuthenticationFailed(AuthenticationResult result)
        {
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