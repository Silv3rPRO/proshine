using PROBot;
using PROProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PROShine
{
    public partial class SoundsWindow : Window
    {
        private BotClient bot;

        public SoundsWindow(BotClient _bot)
        {
            InitializeComponent();
            bot = _bot;
            List<Sound> Sounds = bot.Settings.Sounds;

            // Battle
            CaptureFailCheckBox.IsChecked = GetSound(Sounds, "CaptureFail").ShouldPlay;
            CapturedCheckBox.IsChecked = GetSound(Sounds, "Captured").ShouldPlay;
            CaptureAttemptCheckBox.IsChecked = GetSound(Sounds, "CaptureAttempt").ShouldPlay;
            EscapedCheckBox.IsChecked = GetSound(Sounds, "Escaped").ShouldPlay;
            LevelUpCheckBox.IsChecked = GetSound(Sounds, "LevelUp").ShouldPlay;
            ShinyEncounterCheckBox.IsChecked = GetSound(Sounds, "ShinyEncounter").ShouldPlay;
            // Questing
            SelectCheckBox.IsChecked = GetSound(Sounds, "Select").ShouldPlay;
            HiddenItemFoundCheckBox.IsChecked = GetSound(Sounds, "HiddenItemFound").ShouldPlay;
            ItemFoundCheckBox.IsChecked = GetSound(Sounds, "ItemFound").ShouldPlay;
            PcUsageCheckBox.IsChecked = GetSound(Sounds, "PcUsage").ShouldPlay;
            PurchaseCheckBox.IsChecked = GetSound(Sounds, "Purchase").ShouldPlay;
            // ProShine
            PauseCheckBox.IsChecked = GetSound(Sounds, "Pause").ShouldPlay;
            LogOutCheckBox.IsChecked = GetSound(Sounds, "LogOut").ShouldPlay;

            Title = App.Name + " - " + Title;

        }
        private void SaveSoundsButton_Click(object sender, RoutedEventArgs e)
        {
            List<Sound> Sounds = new List<Sound>()
            {
                // Battle
                new Sound("CaptureFail", (bool)CaptureFailCheckBox.IsChecked, "capture_fail.wav"),
                new Sound("Captured", (bool)CapturedCheckBox.IsChecked, "captured.wav"),
                new Sound("CaptureAttempt", (bool)CaptureAttemptCheckBox.IsChecked, "capture_attempt.wav"),
                new Sound("Escaped", (bool)EscapedCheckBox.IsChecked, "escaped.wav"),
                new Sound("LevelUp", (bool)LevelUpCheckBox.IsChecked, "levelup.wav"),
                new Sound("ShinyEncounter", (bool)ShinyEncounterCheckBox.IsChecked, "shiny.wav"),
                // Questing
                new Sound("Select", (bool)SelectCheckBox.IsChecked, "select.wav"),
                new Sound("HiddenItemFound", (bool)HiddenItemFoundCheckBox.IsChecked, "item_hidden.wav"),
                new Sound("ItemFound", (bool)ItemFoundCheckBox.IsChecked, "item.wav"),
                new Sound("PcUsage", (bool)PcUsageCheckBox.IsChecked, "pc_turningon.wav"),
                new Sound("Purchase", (bool)PurchaseCheckBox.IsChecked, "purchase.wav"),
                // Proshine
                new Sound("Pause", (bool)PauseCheckBox.IsChecked, "pause.wav"),
                new Sound("LogOut", (bool)LogOutCheckBox.IsChecked, "logout.wav")
            };
            bot.Settings.Sounds = Sounds;
            Close();
        }
        private Sound GetSound(List<Sound> register, string snd)
        {
            return register.FirstOrDefault(i => i.Name.Equals(snd.ToLowerInvariant(), StringComparison.InvariantCultureIgnoreCase));
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
