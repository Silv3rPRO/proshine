using PROProtocol;

namespace PROBot.Modules
{
    public class MoveTeacher
    {
        public bool IsLearning { get; private set; }
        public int PokemonDBid { get; private set; }
        public int MoveToForget { get; set; }

        private readonly BotClient _bot;

        private Timeout _learningTimeout = new Timeout();

        public MoveTeacher(BotClient bot)
        {
            _bot = bot;
            _bot.ClientChanged += Bot_ClientChanged;
        }

        public bool Update()
        {
            if (_learningTimeout.IsActive && !_learningTimeout.Update())
            {
                IsLearning = false;
                _bot.Game.LearnMove(PokemonDBid, MoveToForget);
                return true;
            }
            return _learningTimeout.IsActive;
        }

        private void Bot_ClientChanged()
        {
            if (_bot.Game != null)
            {
                _bot.Game.LearningMove += Game_LearningMove;
            }
        }

        private void Game_LearningMove(int moveId, string moveName, int pokemonDBid)
        {
            if (_bot.Game == null || _bot.Script == null) return;

            IsLearning = true;
            PokemonDBid = pokemonDBid;
            MoveToForget = 0;
            _learningTimeout.Set(_bot.Rand.Next(1000, 3000));
            _bot.Script.OnLearningMove(moveName, _bot.Game.GetPokemonFromDBId(pokemonDBid).Uid);
        }
    }
}
