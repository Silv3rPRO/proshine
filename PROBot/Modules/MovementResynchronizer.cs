namespace PROBot.Modules
{
    public class MovementResynchronizer
    {
        private BotClient _bot;
        private int _lastMovementSourceX;
        private int _lastMovementSourceY;
        private int _lastMovementDestinationX;
        private int _lastMovementDestinationY;
        private bool _requestedResync;

        public MovementResynchronizer(BotClient bot)
        {
            _bot = bot;
            _bot.StateChanged += Bot_StateChanged;
            _bot.ClientChanged += Bot_ClientChanged;
        }

        private void Bot_StateChanged(BotClient.State state)
        {
            if (state == BotClient.State.Started)
            {
                Reset();
            }
        }

        private void Bot_ClientChanged()
        {
            if (_bot.Game != null)
            {
                _bot.Game.BattleEnded += Game_BattleEnded;
                _bot.Game.DialogOpened += Game_DialogOpened;
            }
        }

        private void Game_BattleEnded()
        {
            Reset();
        }

        private void Game_DialogOpened(string message, string[] options)
        {
            Reset();
        }

        public bool CheckMovement(int x, int y)
        {
            if (_lastMovementSourceX == _bot.Game.PlayerX && _lastMovementSourceY == _bot.Game.PlayerY
                && _lastMovementDestinationX == x && _lastMovementDestinationY == y)
            {
                if (_requestedResync)
                {
                    _bot.LogMessage("Bot still stuck, stopping the script.");
                    _bot.Stop();
                }
                else
                {
                    _bot.LogMessage("Bot stuck, sending resynchronization request.");
                    _requestedResync = true;
                    _bot.Game.RequestResync();
                }
                return false;
            }
            return true;
        }

        public void ApplyMovement(int x, int y)
        {
            _lastMovementSourceX = _bot.Game.PlayerX;
            _lastMovementSourceY = _bot.Game.PlayerY;
            _lastMovementDestinationX = x;
            _lastMovementDestinationY = y;
        }

        public void Reset()
        {
            _requestedResync = false;
            _lastMovementSourceX = -1;
        }
    }
}
