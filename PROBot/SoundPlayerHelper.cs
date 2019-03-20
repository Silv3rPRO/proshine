using System;
using System.Media;
using System.Linq;
using System.IO;
using System.Collections.Generic;
namespace PROBot
{
    public class SoundPlayerHelper
    {
        public BotClient Bot { get; set; }

        public SoundPlayerHelper(BotClient _bot)
        {
            Bot = _bot;
        }
        private Sound GetSound(List<Sound> register, string snd)
        {
            return register.FirstOrDefault(i => i.Name.Equals(snd.ToLowerInvariant(), StringComparison.InvariantCultureIgnoreCase));
        }
        public void Play(string _filePath)
        {
            try
            {
                if (Bot.Settings.Sounds.Where(x => x.Name.Equals(_filePath, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault() != null)
                {
                    if (GetSound(Bot.Settings.Sounds, _filePath).ShouldPlay)
                    {
                        using (SoundPlayer player = new SoundPlayer("Assets/Sounds/" + GetSound(Bot.Settings.Sounds, _filePath).File))
                        try { player.Play(); } catch (FileNotFoundException ex) { throw ex; }
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                throw ex;
            }
        }
    }
}
