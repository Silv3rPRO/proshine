using PROBot;
using PROProtocol;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PROShine.Views
{
    /// <summary>
    ///     Interaction logic for PlayersView.xaml
    /// </summary>
    public partial class PlayersView : UserControl
    {
        private readonly BotClient _bot;
        private GridViewColumnHeader _lastColumn;
        private ListSortDirection _lastDirection;

        public PlayersView(BotClient bot)
        {
            InitializeComponent();

            _bot = bot;
        }

        public void RefreshView()
        {
            lock (_bot)
            {
                if (_bot.Game != null && _bot.Game.IsMapLoaded && _bot.Game.Players != null)
                {
                    IEnumerable<PlayerInfos> playersList = _bot.Game.Players.Values.OrderBy(e => e.Added);
                    var listToDisplay = new List<PlayerInfosView>();
                    foreach (var player in playersList)
                    {
                        var petName = "";
                        if (player.PokemonPetId < PokemonNamesManager.Instance.Names.Length)
                        {
                            petName = PokemonNamesManager.Instance.Names[player.PokemonPetId];
                            if (player.IsPokemonPetShiny)
                                petName = "(s)" + petName;
                        }
                        listToDisplay.Add(new PlayerInfosView
                        {
                            Distance = _bot.Game.DistanceTo(player.PosX, player.PosY),
                            Name = player.Name,
                            Position = "(" + player.PosX + ", " + player.PosY + ")",
                            Status = player.IsAfk ? "AFK" : (player.IsInBattle ? "BATTLE" : ""),
                            Follower = petName,
                            Guild = player.GuildId.ToString(),
                            LastSeen = (DateTime.UtcNow - player.Updated).Seconds + "s"
                        });
                    }
                    PlayerListView.ItemsSource = listToDisplay;
                    PlayerListView.Items.Refresh();
                }
            }
        }

        private void GridViewHeader_Click(object sender, RoutedEventArgs e)
        {
            var column = sender as GridViewColumnHeader;

            var direction = ListSortDirection.Ascending;
            if (column == _lastColumn && direction == _lastDirection)
                direction = ListSortDirection.Descending;

            PlayerListView.Items.SortDescriptions.Clear();
            PlayerListView.Items.SortDescriptions.Add(new SortDescription((string)column.Content, direction));

            _lastColumn = column;
            _lastDirection = direction;
        }

        private class PlayerInfosView
        {
            public int Distance { get; set; }
            public string Name { get; set; }
            public string Position { get; set; }
            public string Status { get; set; }
            public string Follower { get; set; }
            public string Guild { get; set; }
            public string LastSeen { get; set; }
        }
    }
}