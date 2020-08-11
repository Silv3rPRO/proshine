using PROProtocol;
using System;

namespace PROBot.Modules
{
    public class PokemonEvolver
    {
        public event Action<bool> StateChanged;

        private bool _isEnabled = true;
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

        private readonly BotClient _bot;

        private Timeout _evolutionTimeout = new Timeout();
        public int _evolvingPokemonDBid;
        public int _evolvingItem;

        public PokemonEvolver(BotClient bot)
        {
            _bot = bot;
            _bot.ClientChanged += Bot_ClientChanged;
        }

        public bool Update()
        {
            if (_evolutionTimeout.IsActive && !_evolutionTimeout.Update())
            {
                if (IsEnabled)
                {
                    _bot.Game.SendAcceptEvolution(_evolvingPokemonDBid);
                }
                else
                {
                    _bot.Game.SendCancelEvolution(_evolvingPokemonDBid);
                }
                return true;
            }
            return _evolutionTimeout.IsActive;
        }

        private void Bot_ClientChanged()
        {
            if (_bot.Game != null)
            {
                _bot.Game.Evolving += Game_Evolving;
            }
        }

        private void Game_Evolving(int evolvingPokemonDBid, int evolvingItem)
        {
            _evolvingPokemonDBid = evolvingPokemonDBid;
            _evolvingItem = evolvingItem;
            _evolutionTimeout.Set(_bot.Rand.Next(2000, 3000));
        }
    }
}
