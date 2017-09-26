﻿using System;
using PROProtocol;

namespace PROBot.Modules
{
    public class IsTrainerBattlesActive
    {
        private readonly BotClient _bot;

        private bool _isEnabled = true;

        public IsTrainerBattlesActive(BotClient bot)
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
            GameClient game = _bot.Game;
            if(_bot.Game != null)
               _bot.Game.IsTrainerBattlesActive = _isEnabled;          
        }
    }
}
