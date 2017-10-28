using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PROBot;
using PROProtocol;
using System.ComponentModel;
using System.Windows.Input;

namespace PROShine
{
    /// <summary>
    /// Interaction logic for PlayersView.xaml
    /// </summary>
    public partial class PlayersView : UserControl
    {
        private GridViewColumnHeader _lastColumn;
        private ListSortDirection _lastDirection;

        private BotClient _bot;

        public PlayersView(BotClient bot)
        {
            InitializeComponent();

            _bot = bot;
        }

        class PlayerInfosView
        {
            public int Distance { get; set; }
            public string Name { get; set; }
            public string Position { get; set; }
            public string Status { get; set; }
            public string Follower { get; set; }
            public string Guild { get; set; }
            public string LastSeen { get; set; }
        }

        public void RefreshView()
        {
            lock (_bot)
            {
                if (_bot.Game != null && _bot.Game.IsMapLoaded && _bot.Game.Players != null)
                {
                    IEnumerable<PlayerInfos> playersList = _bot.Game.Players.Values.OrderBy(e => e.Added);
                    List<PlayerInfosView> listToDisplay = new List<PlayerInfosView>();
                    foreach (PlayerInfos player in playersList)
                    {
                        string petName = "";
                        if (player.PokemonPetId < PokemonNamesManager.Instance.Names.Length)
                        {
                            petName = PokemonNamesManager.Instance.Names[player.PokemonPetId];
                            if (player.IsPokemonPetShiny)
                            {
                                petName = "(s)" + petName;
                            }
                        }
                        listToDisplay.Add(new PlayerInfosView
                        {
                            Distance = _bot.Game.DistanceTo(player.PosX, player.PosY),
                            Name = player.Name,
                            Position = "(" + player.PosX + ", " + player.PosY + ")",
                            Status = player.IsAfk ? "AFK" : (player.IsInBattle ? "BATTLE" : ""),
                            Follower = petName,
                            Guild = player.GuildId.ToString(),
                            LastSeen = (DateTime.UtcNow - player.Updated).Seconds.ToString() + "s"
                        });
                    }
                    int selected = PlayerListView.SelectedIndex;
                    PlayerListView.ItemsSource = listToDisplay;
                    PlayerListView.Items.Refresh();
                    PlayerListView.SelectedIndex = selected;
                }
            }
        }

        private void GridViewHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);

            ListSortDirection direction = ListSortDirection.Ascending;
            if (column == _lastColumn && direction == _lastDirection)
            {
                direction = ListSortDirection.Descending;
            }

            PlayerListView.Items.SortDescriptions.Clear();
            PlayerListView.Items.SortDescriptions.Add(new SortDescription((string)column.Content, direction));

            _lastColumn = column;
            _lastDirection = direction;
        }

        private void PlayerListView_PreviewMouseRightButtonDown(object sender, MouseEventArgs e)
        {
            if (PlayerListView.SelectedItems.Count <= 0)
                return;

            lock (_bot)
            {
                if (_bot.Game != null)
                {
                    if (_bot.Game.IsConnected)
                    {
                        ContextMenu contextMenu = new ContextMenu();

                        MenuItem message = new MenuItem();
                        message.Header = "Message";
                        message.Click += MenuItemMessage_Click;
                        contextMenu.Items.Add(message);

                        MenuItem friendUnfriend = new MenuItem();
                        friendUnfriend.Header = "Friend/Unfriend";
                        friendUnfriend.Click += MenuItemFriendToggle_Click;
                        contextMenu.Items.Add(friendUnfriend);

                        MenuItem ignoreUnignore = new MenuItem();
                        ignoreUnignore.Header = "Ignore/Unignore";
                        ignoreUnignore.Click += MenuItemIgnoreToggle_Click;
                        contextMenu.Items.Add(ignoreUnignore);

                        PlayerListView.ContextMenu = contextMenu;
                    }
                }
            }
        }

        private void MenuItemMessage_Click(object sender, RoutedEventArgs e)
        {
            if (PlayerListView.SelectedItems.Count <= 0)
                return;

            var player = (PlayerInfosView)PlayerListView.SelectedItems[0];
            lock (_bot)
            {
                if (!_bot.Game.Conversations.Contains(player.Name))
                {
                    _bot.Game.SendStartPrivateMessage(player.Name);
                }
            }
        }

        private void MenuItemFriendToggle_Click(object sender, RoutedEventArgs e)
        {
            if (PlayerListView.SelectedItems.Count <= 0)
                return;

            var player = (PlayerInfosView)PlayerListView.SelectedItems[0];
            lock (_bot)
            {
                _bot.Game.SendFriendToggle(player.Name);
            }
        }

        private void MenuItemIgnoreToggle_Click(object sender, RoutedEventArgs e)
        {
            if (PlayerListView.SelectedItems.Count <= 0)
                return;

            var player = (PlayerInfosView)PlayerListView.SelectedItems[0];
            lock (_bot)
            {
                _bot.Game.SendIgnoreToggle(player.Name);
            }
        }
    }
}
