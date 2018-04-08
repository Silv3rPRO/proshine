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

        // A value between 0 and 54 that represents how much exp the Pokemon has gained since its last level.
        public int RatioToNextLevel
        {
            get
            {
                if (CurrentLevel == 100)
                    return 54;
                double num = Math.Pow(210.0 / (105.0 - CurrentLevel), 4.0);
                double num2 = ((int)((num + Math.Pow(CurrentLevel, 3.0)) * (BaseLevelExperience / 20.0)));
                num = Math.Pow(210.0 / (105.0 - (CurrentLevel - 1)), 4.0);
                int num3 = (int)((num + Math.Pow(CurrentLevel - 1.0, 3.0)) * (BaseLevelExperience / 20.0));
                num2 -= num3;
                double num4 = TotalLevelExperience - num3;
                if (num4 >= num2)
                    num4 = num2;
                int ratio = (int)(54.0 / (num2 / num4 * 100.0) * 100.0 - 1.0);
                if (ratio <= 0)
                    ratio = 1;
                if (ratio >= 54)
                    ratio = 54;
                return ratio;
            }
        }
    }
}
