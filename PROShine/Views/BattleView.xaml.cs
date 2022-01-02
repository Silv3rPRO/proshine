﻿using FontAwesome.WPF;
using PROBot;
using PROProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using XamlAnimatedGif;

namespace PROShine
{
    public partial class BattleView : UserControl
    {
        private BotClient _bot;
        private MainWindow _parent;
        private GradientStopCollection _hpGradient;
        private double _opponentHPWidth;
        private double _activeHPWidth;
        private double _expBarWidth;

        private string _lastActiveName;
        private string _lastOpponentName;
        private Regex _nameCleaner = new Regex("'| |\\."); // Removes invalid characters
        private const string _spriteDatabasePrefix = "http://pokestadium.com/sprites/xy";

        public BattleView(BotClient bot, MainWindow parent)
        {
            _bot = bot;
            _parent = parent;
            DataContext = this;
            InitializeComponent();

            _hpGradient = new GradientStopCollection
            {
                new GradientStop(Colors.Red, 0),
                new GradientStop(Colors.Red, 0.15),
                new GradientStop(Colors.Yellow, 0.3),
                new GradientStop(Colors.Yellow, 0.5),
                new GradientStop(Colors.Green, 0.6),
                new GradientStop(Colors.Green, 1)
            };
        }

        public void BattleUpdated()
        {
            Dispatcher.InvokeAsync(delegate
            {
                lock (_bot)
                {
                    if (_bot.Game != null && _bot.Game.ActiveBattle != null)
                    {
                        string opponent = PokemonNamesManager.Instance.Names[_bot.Game.ActiveBattle.OpponentId];

                        if (_lastOpponentName != opponent)
                        {
                            _lastOpponentName = opponent;
                            string sprite = _nameCleaner.Replace(opponent.ToLowerInvariant(), "");
                            if (_bot.Game.ActiveBattle.IsShiny)
                                sprite = $"{_spriteDatabasePrefix}/shiny/{sprite}.gif";
                            else
                                sprite = $"{_spriteDatabasePrefix}/{sprite}.gif";
                            AnimationBehavior.SetSourceUri(OpponentGraphic, new Uri(sprite));
                        }

                        if (_bot.Game.ActiveBattle.IsShiny)
                            opponent = "Shiny " + opponent;
                        if (_bot.Game.ActiveBattle.IsWild)
                            opponent = "Wild " + opponent;
                        OpponentName.Text = opponent;
                        OpponentCaughtIcon.Visibility = _bot.Game.ActiveBattle.AlreadyCaught ? Visibility.Visible : Visibility.Hidden;
                        OpponentLevel.Text = _bot.Game.ActiveBattle.OpponentLevel.ToString();
                        OpponentMaxHealth.Text = _bot.Game.ActiveBattle.OpponentHealth.ToString();
                        OpponentCurrentHealth.Text = _bot.Game.ActiveBattle.CurrentHealth.ToString();

                        if (_bot.Game.ActiveBattle.OpponentStatus.ToLowerInvariant() == "none")
                            OpponentStatus.Text = "";
                        else
                            OpponentStatus.Text = _bot.Game.ActiveBattle.OpponentStatus;

                        OpponentType1.Text = TypesManager.Instance.Type1[_bot.Game.ActiveBattle.OpponentId].ToString();
                        OpponentType2.Text = TypesManager.Instance.Type2[_bot.Game.ActiveBattle.OpponentId].ToString();

                        string gender = _bot.Game.ActiveBattle.OpponentGender;
                        OpponentGender.Icon = gender == "M" ? FontAwesomeIcon.Mars : gender == "F" ? FontAwesomeIcon.Venus : FontAwesomeIcon.Question;
                        OpponentForm.Text = _bot.Game.ActiveBattle.AlternateForm.ToString();

                        PokemonStats stats = EffortValuesManager.Instance.BattleValues[_bot.Game.ActiveBattle.OpponentId];
                        List<string> evs = new List<string>();

                        foreach (StatType type in Enum.GetValues(typeof(StatType)).Cast<StatType>())
                        {
                            int ev = stats.GetStat(type);
                            if (ev > 0)
                                evs.Add($"{type}: {ev}");
                        }

                        OpponentEVs.ToolTip = string.Join(Environment.NewLine, evs);

                        Pokemon active = _bot.Game.Team[_bot.Game.ActiveBattle.SelectedPokemonIndex];
                        ActiveName.Text = active.Name;
                        ActiveLevel.Text = active.Level.ToString();
                        ActiveMaxHealth.Text = active.MaxHealth.ToString();
                        ActiveCurrentHealth.Text = active.CurrentHealth.ToString();

                        if (_lastActiveName != active.Name)
                        {
                            _lastActiveName = active.Name;
                            string sprite = _nameCleaner.Replace(active.Name.ToLowerInvariant(), "");
                            if (active.IsShiny)
                                sprite = $"{_spriteDatabasePrefix}/shiny/back/{sprite}.gif";
                            else
                                sprite = $"{_spriteDatabasePrefix}/back/{sprite}.gif";
                            AnimationBehavior.SetSourceUri(PlayerGraphic, new Uri(sprite));
                        }

                        if (active.Experience.CurrentLevel == 100)
                        {
                            NextLevel.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            NextLevel.Visibility = Visibility.Visible;
                            NextLevel.Text = $"To level {active.Experience.CurrentLevel + 1}: {active.Experience.RemainingExperience}";
                        }

                        if (active.Status.ToLowerInvariant() == "none")
                            ActiveStatus.Text = "";
                        else
                            ActiveStatus.Text = active.Status;

                        ActiveType1.Text = TypesManager.Instance.Type1[active.Id].ToString();
                        ActiveType2.Text = TypesManager.Instance.Type2[active.Id].ToString();

                        ActiveGender.Icon = active.Gender == "M" ? FontAwesomeIcon.Mars : active.Gender == "F" ? FontAwesomeIcon.Venus : FontAwesomeIcon.Question;
                    }
                }
            });
        }

        public void UpdateBattleHUD()
        {
            Dispatcher.InvokeAsync(delegate
            {
                lock (_bot)
                {
                    if (_bot.Game != null && _bot.Game.ActiveBattle != null)
                    {
                        // All buttons are enabled only if client is inactive and the bot's script isn't running
                        Buttons.IsEnabled = _bot.Game.IsInactive && _bot.Running != BotClient.State.Started;

                        Pokemon active = _bot.Game.Team[_bot.Game.ActiveBattle.SelectedPokemonIndex];

                        // Only the 'Pokemon' button is enabled if active Pokemon has been knocked out
                        AttackButton.IsEnabled = ItemButton.IsEnabled = active.CurrentHealth > 0;

                        RunButton.IsEnabled = active.CurrentHealth > 0 && _bot.Game.ActiveBattle.IsWild;

                        // Hide the graphics if the window is too small
                        OpponentGraphic.Visibility = PlayerGraphic.Visibility = _parent.Width < 530 ? Visibility.Hidden : Visibility.Visible;

                        // Lerp health bar widths and colors to target positions

                        int targetWidth = _bot.Game.ActiveBattle.CurrentHealth;
                        int maxHealth = _bot.Game.ActiveBattle.OpponentHealth;
                        if (_opponentHPWidth != targetWidth)
                        {
                            _opponentHPWidth -= (_opponentHPWidth - targetWidth) * 0.035;
                            OpponentHealthBar.Width = _opponentHPWidth / maxHealth * 200;
                            OpponentHealthBar.Background = new SolidColorBrush(EvaluateGradient(_opponentHPWidth / maxHealth));
                        }

                        if (_activeHPWidth != active.CurrentHealth)
                        {
                            _activeHPWidth -= (_activeHPWidth - active.CurrentHealth) * 0.035;
                            ActiveHealthBar.Width = _activeHPWidth / active.MaxHealth * 200;
                            ActiveHealthBar.Background = new SolidColorBrush(EvaluateGradient(_activeHPWidth / active.MaxHealth));
                        }

                        if (active.Experience.CurrentLevel == 100)
                        {
                            ExpBar.Width = 200;
                        }
                        else
                        {
                            int ratio = active.Experience.RatioToNextLevel;

                            // Small half unit buffer so _expBarWidth won't be greater than ratio unless the Pokemon levels up
                            if (_expBarWidth < ratio - 0.5)
                            {
                                _expBarWidth += (ratio - _expBarWidth) * 0.035;
                            }
                            else if (_expBarWidth > ratio)
                            {
                                // Pokemon just leveled up. Fill bar all the way, then empty it immediately
                                if (_expBarWidth < 54)
                                    _expBarWidth += (54 - _expBarWidth) * 0.035;
                                else
                                    _expBarWidth = 0;
                            }
                            ExpBar.Width = _expBarWidth * 3.7;
                        }
                    }
                }
            });
        }

        public void BattleStarted()
        {
            Dispatcher.InvokeAsync(delegate
            {
                lock (_bot)
                {
                    if (_bot.Game != null && _bot.Game.ActiveBattle != null)
                    {
                        PROShineLogo.Visibility = Visibility.Hidden;
                        UIGrid.Visibility = Visibility.Visible;
                        _opponentHPWidth = _bot.Game.ActiveBattle.CurrentHealth;
                        int maxHealth = _bot.Game.ActiveBattle.OpponentHealth;
                        OpponentHealthBar.Width = _opponentHPWidth / maxHealth * 200;
                        OpponentHealthBar.Background = new SolidColorBrush(EvaluateGradient(_opponentHPWidth / maxHealth));

                        Pokemon active = _bot.Game.Team[_bot.Game.ActiveBattle.SelectedPokemonIndex];
                        _activeHPWidth = active.CurrentHealth;
                        ActiveHealthBar.Width = _activeHPWidth / active.MaxHealth * 200;
                        ActiveHealthBar.Background = new SolidColorBrush(EvaluateGradient(_activeHPWidth / active.MaxHealth));
                        if (active.Experience.CurrentLevel == 100)
                        {
                            ExpBar.Width = 200;
                        }
                        else
                        {
                            _expBarWidth = active.Experience.RatioToNextLevel - 0.5;
                            ExpBar.Width = _expBarWidth * 3.7;
                        }
                    }
                }
            });
        }

        public void BattleEnded()
        {
            Dispatcher.InvokeAsync(delegate
            {
                PROShineLogo.Visibility = Visibility.Visible;
                UIGrid.Visibility = Visibility.Hidden;
            });
        }

        public void ConnectionClosed()
        {
            Dispatcher.InvokeAsync(delegate
            {
                PROShineLogo.Visibility = Visibility.Visible;
                UIGrid.Visibility = Visibility.Hidden;
            });
        }

        public void ActivePokemonChanged()
        {
            Dispatcher.InvokeAsync(delegate
            {
                lock (_bot)
                {
                    if (_bot.Game != null && _bot.Game.ActiveBattle != null)
                    {
                        Pokemon active = _bot.Game.Team[_bot.Game.ActiveBattle.SelectedPokemonIndex];
                        _activeHPWidth = active.CurrentHealth;
                        ActiveHealthBar.Width = _activeHPWidth / active.MaxHealth * 200;
                        ActiveHealthBar.Background = new SolidColorBrush(EvaluateGradient(_activeHPWidth / active.MaxHealth));
                        if (active.Experience.CurrentLevel == 100)
                        {
                            ExpBar.Width = 200;
                        }
                        else
                        {
                            _expBarWidth = active.Experience.RatioToNextLevel - 0.5;
                            ExpBar.Width = _expBarWidth * 3.7;
                        }
                    }
                }
            });
        }

        public void OpponentChanged()
        {
            Dispatcher.InvokeAsync(delegate
            {
                lock (_bot)
                {
                    if (_bot.Game != null && _bot.Game.ActiveBattle != null)
                    {
                        _opponentHPWidth = _bot.Game.ActiveBattle.CurrentHealth;
                        int maxHealth = _bot.Game.ActiveBattle.OpponentHealth;
                        OpponentHealthBar.Width = _opponentHPWidth / maxHealth * 200;
                        OpponentHealthBar.Background = new SolidColorBrush(EvaluateGradient(_opponentHPWidth / maxHealth));
                    }
                }
            });
        }

        private void AttackButton_Click(object sender, RoutedEventArgs e)
        {
            Pokemon active = _bot.Game.Team[_bot.Game.ActiveBattle.SelectedPokemonIndex];
            ContextMenu attacks = new ContextMenu();
            bool hasPP = false;

            for (int i = 0; i < active.Moves.Length; i++)
            {
                PokemonMove move = active.Moves[i];
                if (move == null || string.IsNullOrEmpty(move.Name))
                    continue;
                MenuItem menuItem = new MenuItem
                {
                    Header = move.Name,
                    InputGestureText = $"({move.PP})",
                };
                menuItem.Tag = i + 1;
                if (move.CurrentPoints > 0)
                {
                    menuItem.Click += Attack_Click;
                    hasPP = true;
                }
                else
                {
                    menuItem.IsEnabled = false;
                }
                attacks.Items.Add(menuItem);
            }

            if (!hasPP)
            {
                MenuItem menuItem = new MenuItem
                {
                    Header = "Struggle"
                };
                menuItem.Tag = 1;
                menuItem.Click += Attack_Click;
                attacks.Items.Add(menuItem);
            }

            attacks.PlacementTarget = sender as Button;
            attacks.IsOpen = true;
        }

        private void Attack_Click(object sender, RoutedEventArgs e)
        {
            int index = (int)((MenuItem)e.OriginalSource).Tag;
            _bot.Game.UseAttack(index);
        }

        private void ItemButton_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu itemsMenu = new ContextMenu();
            _bot.Game.Items
                .Where(i => i.Quantity > 0 && (i.CanBeUsedInBattle || i.CanBeUsedOnPokemonInBattle))
                .OrderBy(i => i.Name)
                .ToList()
                .ForEach(item =>
                {
                    MenuItem menuItem = new MenuItem
                    {
                        Header = item.Name,
                        InputGestureText = $"({item.Quantity})",
                    };
                    if (item.CanBeUsedOnPokemonInBattle)
                    {
                        foreach (Pokemon pokemon in _bot.Game.Team)
                        {
                            MenuItem pokemonItem = new MenuItem
                            {
                                Header = pokemon.Name,
                                InputGestureText = $"Lv. {pokemon.Level} ({pokemon.Health} HP)",
                            };
                            pokemonItem.Tag = $"{item.Id}@{pokemon.Uid}";
                            pokemonItem.Click += Item_Click;
                            menuItem.Items.Add(pokemonItem);
                        }
                    }
                    else
                    {
                        menuItem.Tag = item.Id.ToString();
                        menuItem.Click += Item_Click;
                    }
                    itemsMenu.Items.Add(menuItem);
                });
            itemsMenu.PlacementTarget = sender as Button;
            itemsMenu.IsOpen = true;
        }

        private void Item_Click(object sender, RoutedEventArgs e)
        {
            string tag = ((MenuItem)e.OriginalSource).Tag as string;
            int pokemonIndex = 0;
            if (tag.Contains('@'))
            {
                string[] data = tag.Split('@');
                tag = data[0];
                pokemonIndex = int.Parse(data[1]);
            }
            int itemId = int.Parse(tag);
            _bot.Game.UseItem(itemId, pokemonIndex);
        }

        private void PokemonButton_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu pokemonMenu = new ContextMenu();
            foreach (Pokemon pokemon in _bot.Game.Team)
            {
                MenuItem pokemonItem = new MenuItem
                {
                    Header = pokemon.Name,
                    InputGestureText = $"Lv. {pokemon.Level} ({pokemon.Health} HP)",
                };
                pokemonItem.Tag = pokemon.Uid;
                if (pokemon.Uid == _bot.Game.ActiveBattle.SelectedPokemonIndex + 1 || pokemon.CurrentHealth <= 0)
                    pokemonItem.IsEnabled = false;
                else
                    pokemonItem.Click += Pokemon_Click;
                pokemonMenu.Items.Add(pokemonItem);
            }
            pokemonMenu.PlacementTarget = sender as Button;
            pokemonMenu.IsOpen = true;
        }

        private void Pokemon_Click(object sender, RoutedEventArgs e)
        {
            int index = (int)((MenuItem)e.OriginalSource).Tag;
            _bot.Game.ChangePokemon(index);
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            _bot.Game.RunFromBattle();
        }

        private Color EvaluateGradient(double offset)
        {
            // Praise be to StackOverflow

            // https://stackoverflow.com/questions/9650049/get-color-in-specific-location-on-gradient

            GradientStop before = _hpGradient.Where(w => w.Offset == _hpGradient.Min(m => m.Offset)).First();
            GradientStop after = _hpGradient.Where(w => w.Offset == _hpGradient.Max(m => m.Offset)).First();

            foreach (GradientStop gs in _hpGradient)
            {
                if (gs.Offset < offset && gs.Offset > before.Offset)
                {
                    before = gs;
                }
                if (gs.Offset > offset && gs.Offset < after.Offset)
                {
                    after = gs;
                }
            }

            Color color = new Color
            {
                ScA = (float)((offset - before.Offset) * (after.Color.ScA - before.Color.ScA) / (after.Offset - before.Offset) + before.Color.ScA),
                ScR = (float)((offset - before.Offset) * (after.Color.ScR - before.Color.ScR) / (after.Offset - before.Offset) + before.Color.ScR),
                ScG = (float)((offset - before.Offset) * (after.Color.ScG - before.Color.ScG) / (after.Offset - before.Offset) + before.Color.ScG),
                ScB = (float)((offset - before.Offset) * (after.Color.ScB - before.Color.ScB) / (after.Offset - before.Offset) + before.Color.ScB)
            };

            return color;
        }

        private void ErrorLoadingGraphic(DependencyObject d, AnimationErrorEventArgs e)
        {
            // Set graphic to Decamark icon if the url couldn't be loaded
            AnimationBehavior.SetSourceUri((Image)d, new Uri("https://cdn.bulbagarden.net/upload/archive/8/8e/20090709005535%21Spr_3r_000.png"));
        }
    }
}
