using System;

namespace PROProtocol
{
    public class PokemonExperience
    {
        public int CurrentLevel { get; private set; }
        public int TotalLevelExperience { get; private set; }

        public int RemainingExperience
        {
            get
            {
                // DSSocks.BoxLoadMon(string data)
                if (CurrentLevel == 100)
                {
                    return 0;
                }
                double num = Math.Pow(210.0 / (105.0 - CurrentLevel), 4.0);
                double num2 = ((int)((num + Math.Pow(CurrentLevel, 3.0)) * (BaseLevelExperience / 20.0)));
                return (int)(num2 - TotalLevelExperience);
            }
        }

        public int BaseLevelExperience { get; private set; }

        public PokemonExperience(int currentLevel, int baseExperience, int totalLevelExperience)
        {
            CurrentLevel = currentLevel;
            BaseLevelExperience = baseExperience;
            if (baseExperience <= 0)
            {
                BaseLevelExperience = 3;
            }
            TotalLevelExperience = totalLevelExperience;
        }
    }
}
