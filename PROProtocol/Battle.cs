using System;
using System.Collections.Generic;

namespace PROProtocol
{
    public class Battle
    {
        private readonly string _playerName;

        public Battle(string playerName, string[] data)
        {
            _playerName = playerName;

            OpponentId = Convert.ToInt32(data[0]);
            OpponentHealth = Convert.ToInt32(data[1]);
            CurrentHealth = OpponentHealth;
            OpponentLevel = Convert.ToInt32(data[2]);
            SelectedPokemonIndex = Convert.ToInt32(data[3]) - 1;
            BattleText = data[4];
            IsShiny = data[5] != "0";
            IsWild = data[6] == string.Empty && data[8] == string.Empty;
            TrainerName = data[6];
            IsPvp = data[7] == string.Empty;
            PokemonCount = 1;
            if (data[8] != string.Empty)
                PokemonCount = Convert.ToInt32(data[8]);
            OpponentGender = data[9];
            OpponentStatus = data[10];
            AlreadyCaught = data[11] == "1";
            AlternateForm = int.Parse(data[12]);
        }

        public int OpponentId { get; private set; }
        public int OpponentHealth { get; private set; }
        public int CurrentHealth { get; private set; }
        public int OpponentLevel { get; private set; }
        public int SelectedPokemonIndex { get; private set; }
        public string BattleText { get; }
        public bool IsShiny { get; private set; }
        public bool IsWild { get; }
        public string TrainerName { get; }
        public bool IsPvp { get; }
        public int PokemonCount { get; }
        public string OpponentGender { get; private set; }
        public string OpponentStatus { get; private set; }
        public bool AlreadyCaught { get; private set; }
        public int AlternateForm { get; private set; }

        public bool IsFinished { get; private set; }

        public bool RepeatAttack { get; set; }

        public bool ProcessMessage(List<Pokemon> team, string message)
        {
            if (message.Length == 0)
                return true;

            if (message == "E-B")
            {
                IsFinished = true;
                return true;
            }

            var data = message.Split(':');

            if (message.StartsWith("D:"))
            {
                var currentHealth = Convert.ToInt32(data[2]);
                var maxHealth = Convert.ToInt32(data[3]);
                var status = data[6];

                if (data[1] == _playerName)
                {
                    team[SelectedPokemonIndex].UpdateHealth(maxHealth, currentHealth);
                    team[SelectedPokemonIndex].Status = status;
                }
                else
                {
                    CurrentHealth = currentHealth;
                    OpponentHealth = maxHealth;
                    OpponentStatus = status;
                }
                return true;
            }

            if (message.StartsWith("S:"))
            {
                if (data[1] == _playerName)
                    team[SelectedPokemonIndex].Status = data[2];
                else
                    OpponentStatus = data[2];
                return true;
            }

            if (message.StartsWith("P:"))
            {
                if (data[1] == _playerName)
                {
                    var index = Convert.ToInt32(data[2]) - 1;
                    for (var i = 0; i < 4; ++i)
                        team[index].Moves[i].CurrentPoints = Convert.ToInt32(data[i + 3]);
                }
                return true;
            }

            if (message.StartsWith("C:"))
            {
                var pokemonId = Convert.ToInt32(data[2]);
                var level = Convert.ToInt32(data[3]);
                var isShiny = data[4] == "1";
                var maxHealth = Convert.ToInt32(data[5]);
                var currentHealth = Convert.ToInt32(data[6]);
                var index = Convert.ToInt32(data[7]) - 1;
                var status = data[8];
                var gender = data[9];
                var alreadyCaught = data[10] == "1";
                var alternateForm = data[11];

                if (data[1] == _playerName)
                {
                    SelectedPokemonIndex = index;
                }
                else
                {
                    OpponentId = pokemonId;
                    OpponentLevel = level;
                    IsShiny = isShiny;
                    OpponentHealth = maxHealth;
                    CurrentHealth = currentHealth;
                    OpponentStatus = status;
                    OpponentGender = gender;
                    AlreadyCaught = alreadyCaught;
                    AlternateForm = int.Parse(alternateForm);
                }
                return true;
            }

            if (message.StartsWith("F:"))
            {
                // Fainted
                var player = data[1];
                return true;
            }

            if (message.StartsWith("R:"))
            {
                var player = data[1];
                if (player == _playerName)
                    RepeatAttack = true;
                return true;
            }

            if (message.StartsWith("X:"))
            {
                // Hazard
                var player = data[1];
                var spikes = Convert.ToInt32(data[2]);
                var toxicSpikes = Convert.ToInt32(data[3]);
                var stealthRock = Convert.ToInt32(data[4]);
                var stickyWeb = Convert.ToInt32(data[5]);
                return true;
            }

            if (message.StartsWith("CP") && message.Length == 3)
                return true;

            return false;
        }
    }
}