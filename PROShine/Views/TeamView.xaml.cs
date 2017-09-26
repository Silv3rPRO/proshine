﻿using PROBot;
using PROProtocol;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PROShine.Views
{
    public partial class TeamView : UserControl
    {
        private readonly BotClient _bot;
        private Point _startPoint;

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
            var mousePos = e.GetPosition(null);
            var diff = _startPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                 Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                // Get the dragged ListViewItem
                var listView = sender as ListView;
                var listViewItem =
                    FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);

                if (listViewItem != null)
                {
                    // Find the data behind the ListViewItem
                    var pokemon = (Pokemon)listView.ItemContainerGenerator.ItemFromContainer(listViewItem);

                    // Initialize the drag & drop operation
                    var dragData = new DataObject("PROShinePokemon", pokemon);
                    DragDrop.DoDragDrop(listViewItem, dragData, DragDropEffects.Move);
                }
            }
        }

        private static T FindAnchestor<T>(DependencyObject current) where T : DependencyObject
        {
            do
            {
                if (current is T)
                    return (T)current;
                current = VisualTreeHelper.GetParent(current);
            } while (current != null);
            return null;
        }

        private void List_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("PROShinePokemon"))
            {
                var sourcePokemon = e.Data.GetData("PROShinePokemon") as Pokemon;

                // Get the dragged ListViewItem
                var listView = sender as ListView;
                var listViewItem =
                    FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);

                if (listViewItem != null)
                {
                    // Find the data behind the ListViewItem
                    var destinationPokemon =
                        (Pokemon)listView.ItemContainerGenerator.ItemFromContainer(listViewItem);

                    lock (_bot)
                    {
                        if (_bot.Game != null)
                            _bot.Game.SwapPokemon(sourcePokemon.Uid, destinationPokemon.Uid);
                    }
                }
            }
        }

        private void List_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("PROShinePokemon") || sender == e.Source)
                e.Effects = DragDropEffects.None;
        }
    }
}