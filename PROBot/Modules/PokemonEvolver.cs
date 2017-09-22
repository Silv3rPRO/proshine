using System;
using PROProtocol;

namespace PROBot.Modules
{
    public class PokemonEvolver
    {
        private readonly BotClient _bot;

        private readonly Timeout _evolutionTimeout = new Timeout();

        private bool _isEnabled = true;
        public int EvolvingItem;
        public int EvolvingPokemonUid;

        public PokemonEvolver(BotClient bot)
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

        public bool Update()
        {
            if (_evolutionTimeout.IsActive && !_evolutionTimeout.Update())
            {
                if (IsEnabled)
                    _bot.Game.SendAcceptEvolution(EvolvingPokemonUid, EvolvingItem);
                else
                    _bot.Game.SendCancelEvolution(EvolvingPokemonUid, EvolvingItem);
                return true;
            }
            return _evolutionTimeout.IsActive;
        }

        private void Bot_ClientChanged()
        {
            if (_bot.Game != null)
                _bot.Game.Evolving += Game_Evolving;
        }

        private void Game_Evolving(int evolvingPokemonUid, int evolvingItem)
        {
            EvolvingPokemonUid = evolvingPokemonUid;
            EvolvingItem = evolvingItem;
            _evolutionTimeout.Set(_bot.Rand.Next(2000, 3000));
        }
    }
}