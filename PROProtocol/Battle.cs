using System;
using System.Collections.Generic;

namespace PROProtocol
{
    public class Battle
    {
        public event Action OpponentChanged;
        public event Action ActivePokemonChanged;

        public int OpponentId { get; private set; }
        public int OpponentHealth { get; private set; }
        public int CurrentHealth { get; private set; }
        public int OpponentLevel { get; private set; }
        public int SelectedPokemonIndex { get; private set; }
        public string BattleText { get; private set; }
        public bool IsShiny { get; private set; }
        public bool IsWild { get; private set; }
        public string TrainerName { get; private set; }
        public bool IsPvp { get; private set; }
        public int PokemonCount { get; private set; }
        public string OpponentGender { get; private set; }
        public string OpponentStatus { get; private set; }
        public bool AlreadyCaught { get; private set; }
        public int AlternateForm { get; private set; }

        public bool IsFinished { get; private set; }

        public bool IsTrapped { get; private set; }

        public bool RepeatAttack { get; set; }

        private readonly string _playerName;

        private bool _opponentFainted;

        public Battle(string playerName, string[] data)
        {
            _playerName = playerName;

            OpponentId = Convert.ToInt32(data[0]);
            OpponentHealth = Convert.ToInt32(data[1]);
            CurrentHealth = OpponentHealth;
            OpponentLevel = Convert.ToInt32(data[2]);
            // data[3] = mega form? or something
            SelectedPokemonIndex = Convert.ToInt32(data[4]) - 1;
            BattleText = data[5];
            IsShiny = (data[6] != "0");
            IsWild = data[7] == string.Empty && data[9] == string.Empty;
            TrainerName = data[7];
            IsPvp = data[8] == string.Empty;
            PokemonCount = 1;
            if (data[9] != string.Empty)
            {
                PokemonCount = Convert.ToInt32(data[8]);
            }
            OpponentGender = data[10];
            OpponentStatus = data[11];
            AlreadyCaught = (data[12] == "1");
            AlternateForm = int.Parse(data[13]);
        }

        public bool ProcessMessage(List<Pokemon> team, string message)
        {
            if (message.Length == 0)
                return true;

            if (message == "E-B")
            {
                IsFinished = true;
                return true;
            }

            string[] data = message.Split(':');

            if (message.StartsWith("D:"))
            {
                int currentHealth = Convert.ToInt32(data[2]);
                int maxHealth = Convert.ToInt32(data[3]);
                string status = data[6];

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
                {
                    team[SelectedPokemonIndex].Status = data[2];
                }
                else
                {
                    OpponentStatus = data[2];
                }
                return true;
            }

            if (message.StartsWith("P:"))
            {
                IsTrapped = false;

                if (data[1] == _playerName)
                {
                    int index = Convert.ToInt32(data[2]) - 1;
                    for (int i = 0; i < 4; ++i)
                    {
                        team[index].Moves[i].CurrentPoints = Convert.ToInt32(data[i + 3]);
                    }
                }
                return true;
            }

            if (message.StartsWith("C:"))
            {
                IsTrapped = false;

                int pokemonId = Convert.ToInt32(data[2]);
                int level = Convert.ToInt32(data[3]);
                // data[4] = mega form? or something
                bool isShiny = data[5] == "1";
                int maxHealth = Convert.ToInt32(data[6]);
                int currentHealth = Convert.ToInt32(data[7]);
                int index = Convert.ToInt32(data[8]) - 1;
                string status = data[9];
                string gender = data[10];
                bool alreadyCaught = (data[11] == "1");
                string alternateForm = data[12];

                if (data[1] == _playerName)
                {
                    if (SelectedPokemonIndex != index)
                    {
                        SelectedPokemonIndex = index;
                        ActivePokemonChanged?.Invoke();
                    }
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
                    if (_opponentFainted)
                    {
                        _opponentFainted = false;
                        OpponentChanged?.Invoke();
                    }
                }
                return true;
            }

            if (message.StartsWith("F:"))
            {
                // Fainted
                if (data[1] != _playerName)
                    _opponentFainted = true;
                return true;
            }

            if (message.StartsWith("R:"))
            {
                string player = data[1];
                if (player == _playerName)
                {
                    RepeatAttack = true;
                }
                return true;
            }

            if (message.StartsWith("X:"))
            {
                // Hazard
                string player = data[1];
                int spikes = Convert.ToInt32(data[2]);
                int toxicSpikes = Convert.ToInt32(data[3]);
                int stealthRock = Convert.ToInt32(data[4]);
                int stickyWeb = Convert.ToInt32(data[5]);
                return true;
            }

            if (message.StartsWith("T:") || message.StartsWith("U:") || message.StartsWith("EXP:"))
            {
                // exp animation
                // may be blackout animation?
                return true;
            }

            if (message.StartsWith("W:"))
            {
                // Capture animation (ball shake number):(1 = success, 0 = fail).
                return true;
            }

            if (message.Contains("$CantRun") || message.Contains("$NoSwitch"))
            {
                IsTrapped = true;
            }

            return false;
        }
    }
}
