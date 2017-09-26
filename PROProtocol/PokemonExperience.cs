using System;

namespace PROProtocol
{
    public class PokemonExperience
    {
        public PokemonExperience(int currentLevel, int baseExperience, int totalLevelExperience)
        {
            CurrentLevel = currentLevel;
            BaseLevelExperience = baseExperience;
            if (baseExperience <= 0)
                BaseLevelExperience = 3;
            TotalLevelExperience = totalLevelExperience;
        }

        public int CurrentLevel { get; }
        public int TotalLevelExperience { get; }

        public int RemainingExperience
        {
            get
            {
                // DSSocks.BoxLoadMon(string data)
                if (CurrentLevel == 100)
                    return 0;
                var num = Math.Pow(210.0 / (105.0 - CurrentLevel), 4.0);
                double num2 = (int)((num + Math.Pow(CurrentLevel, 3.0)) * (BaseLevelExperience / 20.0));
                return (int)(num2 - TotalLevelExperience);
            }
        }

        public int BaseLevelExperience { get; }
    }
}