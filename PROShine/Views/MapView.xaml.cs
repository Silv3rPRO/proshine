using PROBot;
using PROProtocol;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PROShine.Views
{
    public partial class MapView : UserControl
    {
        private BotClient _bot;
        private Dictionary<int, Brush> _colliderColors;

        private int _cellWidth = 16;

        private bool _isMapDirty;
        private UniformGrid _mapGrid;
        private bool _isPlayerDirty;
        private Shape _player;

        public MapView(BotClient bot)
        {
            InitializeComponent();

            _bot = bot;

            _colliderColors = new Dictionary<int, Brush>
            {
                { 0, Brushes.White },
                { 2, Brushes.Gray },
                { 3, Brushes.Gray },
                { 4, Brushes.Gray },
                { 5, Brushes.LightSkyBlue },
                { 6, Brushes.LightGreen },
                { 7, Brushes.White },
                { 8, Brushes.White },
                { 9, Brushes.White },
                { 10, Brushes.LightGray },
                { 12, Brushes.LightSkyBlue },
                { 14, Brushes.Gray }
            };

            IsVisibleChanged += MapView_IsVisibleChanged;
            MouseDown += MapView_MouseDown;
            SizeChanged += MapView_SizeChanged;
        }

        private void MapView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.Focus(this);
        }

        private void MapView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Window parent = Window.GetWindow(this);
            if (IsVisible)
            {
                if (_isMapDirty)
                {
                    RefreshMap();
                }
                if (_isPlayerDirty)
                {
                    RefreshPlayer();
                }
                parent.KeyDown += Parent_KeyDown;
            }
            else
            {
                parent.KeyDown -= Parent_KeyDown;
            }
        }

        private void MapView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (IsVisible)
            {
                RefreshMap();
            }
        }

        private void Parent_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if (e.Key == Key.Add)
            {
                _cellWidth += 2;
                if (_cellWidth > 64) _cellWidth = 64;
                RefreshMap();
            }
            else if (e.Key == Key.Subtract)
            {
                _cellWidth -= 2;
                if (_cellWidth < 2) _cellWidth = 2;
                RefreshMap();
            }
            else if (e.Key == Key.Up)
            {
                MovePlayer(Direction.Up);
            }
            else if (e.Key == Key.Down)
            {
                MovePlayer(Direction.Down);
            }
            else if (e.Key == Key.Left)
            {
                MovePlayer(Direction.Left);
            }
            else if (e.Key == Key.Right)
            {
                MovePlayer(Direction.Right);
            }
            else
            {
                e.Handled = false;
            }
        }

        private void MovePlayer(Direction direction)
        {
            lock (_bot)
            {
                if (_bot.Game != null &&
                    _bot.Game.IsMapLoaded &&
                    _bot.Game.AreNpcReceived &&
                    _bot.Game.IsInactive &&
                    !_bot.Game.IsInBattle &&
                    _bot.Running != BotClient.State.Started)
                {
                    _bot.Game.Move(direction);
                }
            }
        }

        public void RefreshMap()
        {
            if (!IsVisible)
            {
                _isMapDirty = true;
                return;
            }
            _isMapDirty = false;

            MapCanvas.Children.Clear();

            lock (_bot)
            {
                if (_bot.Game == null || _bot.Game.Map == null) return;

                UniformGrid grid = new UniformGrid();

                grid.Columns = _bot.Game.Map.DimensionX;
                grid.Rows = _bot.Game.Map.DimensionY;
                grid.Width = grid.Columns * _cellWidth;
                grid.Height = grid.Rows * _cellWidth;

                for (int y = 0; y < grid.Rows; ++y)
                {
                    for (int x = 0; x < grid.Columns; ++x)
                    {
                        Rectangle rect = new Rectangle();
                        int collider = _bot.Game.Map.GetCollider(x, y);
                        if (_bot.Game.Map.HasLink(x, y))
                        {
                            rect.Fill = Brushes.Gold;
                        }
                        else if (_colliderColors.ContainsKey(collider))
                        {
                            rect.Fill = _colliderColors[collider];
                        }
                        else
                        {
                            rect.Fill = Brushes.Black;
                        }
                        grid.Children.Add(rect);
                    }
                }

                _mapGrid = grid;
                MapCanvas.Children.Add(grid);

                _player = new Ellipse() { Fill = Brushes.Red, Width = _cellWidth, Height = _cellWidth };
                MapCanvas.Children.Add(_player);

                UpdatePlayerPosition();
            }
        }

        public void RefreshPlayer()
        {
            if (!IsVisible)
            {
                _isPlayerDirty = true;
            }

            lock (_bot)
            {
                if (_bot.Game == null || _bot.Game.Map == null || _player == null) return;
                UpdatePlayerPosition();
            }
        }

        private void UpdatePlayerPosition()
        {
            _isPlayerDirty = false;

            double canFillX = Math.Floor(MapCanvas.ActualWidth / _cellWidth);
            double canFillY = Math.Floor(MapCanvas.ActualHeight / _cellWidth);

            double deltaX = -_bot.Game.PlayerX + canFillX / 2;
            double deltaY = -_bot.Game.PlayerY + canFillY / 2;

            if (_mapGrid.Columns <= canFillX) deltaX = 0;
            if (_mapGrid.Rows <= canFillY) deltaY = 0;

            if (deltaX < -_mapGrid.Columns + canFillX) deltaX = -_mapGrid.Columns + canFillX;
            if (deltaY < -_mapGrid.Rows + canFillY) deltaY = -_mapGrid.Rows + canFillY;

            if (deltaX > 0) deltaX = 0;
            if (deltaY > 0) deltaY = 0;

            Canvas.SetLeft(_mapGrid, deltaX * _cellWidth);
            Canvas.SetTop(_mapGrid, deltaY * _cellWidth);
            Canvas.SetLeft(_player, (_bot.Game.PlayerX + deltaX) * _cellWidth);
            Canvas.SetTop(_player, (_bot.Game.PlayerY + deltaY) * _cellWidth);
        }

        public void Client_MapLoaded(string mapName)
        {
            Dispatcher.InvokeAsync(delegate
            {
                RefreshMap();
            });
        }

        public void Client_PositionUpdated(string map, int x, int y)
        {
            Dispatcher.InvokeAsync(delegate
            {
                RefreshPlayer();
            });
        }
    }
}
