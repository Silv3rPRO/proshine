using PROBot;
using PROProtocol;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PROShine
{
    public partial class TeamView : UserControl
    {
        private readonly BotClient _bot;

        private Point _startPoint;
        private Pokemon _selectedPokemon;

        public TeamView(BotClient bot)
        {
            _bot = bot;
            InitializeComponent();
        }

        private void List_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
        }

        private void List_MouseMove(object sender, MouseEventArgs e)
        {
            // Get the current mouse position
            Point mousePos = e.GetPosition(null);
            Vector diff = _startPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                // Get the dragged ListViewItem
                ListView listView = sender as ListView;
                ListViewItem listViewItem =
                    FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);

                if (listViewItem != null)
                {
                    // Find the data behind the ListViewItem
                    Pokemon pokemon = (Pokemon)listView.ItemContainerGenerator.ItemFromContainer(listViewItem);

                    // Initialize the drag & drop operation
                    DataObject dragData = new DataObject("PROShinePokemon", pokemon);
                    DragDrop.DoDragDrop(listViewItem, dragData, DragDropEffects.Move);
                }
            }
        }

        private static T FindAnchestor<T>(DependencyObject current) where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }

        private void List_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("PROShinePokemon"))
            {
                Pokemon sourcePokemon = e.Data.GetData("PROShinePokemon") as Pokemon;

                // Get the dragged ListViewItem
                ListView listView = sender as ListView;
                ListViewItem listViewItem =
                    FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);

                if (listViewItem != null)
                {
                    // Find the data behind the ListViewItem
                    Pokemon destinationPokemon = (Pokemon)listView.ItemContainerGenerator.ItemFromContainer(listViewItem);

                    lock (_bot)
                    {
                        if (_bot.Game != null)
                        {
                            _bot.Game.SwapPokemon(sourcePokemon.Uid, destinationPokemon.Uid);
                        }
                    }
                }
            }
        }

        private void List_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("PROShinePokemon") || sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void List_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PokemonsListView.SelectedItems.Count > 0)
            {
                _selectedPokemon = (Pokemon)PokemonsListView.SelectedItems[0];
            }
            else
            {
                _selectedPokemon = null;
            }
        }

        private void List_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (_selectedPokemon == null) return;

            Pokemon pokemon = _selectedPokemon;

            lock (_bot)
            {
                if (_bot.Game == null) return;
                if (!_bot.Game.IsConnected) return;

                ContextMenu contextMenu = new ContextMenu();
                if (!string.IsNullOrEmpty(pokemon.ItemHeld))
                {
                    MenuItem takeItem = new MenuItem();
                    takeItem.Header = "Take " + pokemon.ItemHeld;
                    takeItem.Click += MenuItemTakeItem_Click;
                    contextMenu.Items.Add(takeItem);
                }
                if (_bot.Game.Items.Count > 0)
                {
                    MenuItem giveItem = new MenuItem();
                    giveItem.Header = "Give item";

                    _bot.Game.Items
                        .Where(i => i.CanBeHeld)
                        .OrderBy(i => i.Name)
                        .ToList()
                        .ForEach(i => giveItem.Items.Add(i.Name));

                    giveItem.Click += MenuItemGiveItem_Click;
                    contextMenu.Items.Add(giveItem);
                }
                PokemonsListView.ContextMenu = contextMenu;
            }
        }

        private void MenuItemGiveItem_Click(object sender, RoutedEventArgs e)
        {
            if (PokemonsListView.SelectedItems.Count == 0) return;
            Pokemon pokemon = (Pokemon)PokemonsListView.SelectedItems[0];
            string itemName = ((MenuItem)e.OriginalSource).Header.ToString();
            lock (_bot)
            {
                InventoryItem item = _bot.Game.Items.Find(i => i.Name == itemName);
                if (item != null)
                {
                    _bot.Game.SendGiveItem(pokemon.DatabaseId, item.Id);
                }
            }
        }

        private void MenuItemTakeItem_Click(object sender, RoutedEventArgs e)
        {
            if (PokemonsListView.SelectedItems.Count == 0) return;
            Pokemon pokemon = (Pokemon)PokemonsListView.SelectedItems[0];
            lock (_bot)
            {
                _bot.Game.SendTakeItem(pokemon.DatabaseId);
            }
        }
    }
}
