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
using System.Linq;
using System.Text;

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
        private bool _areNpcsDirty;
        private Shape[] _npcs;
        private bool _arePlayersDirty;
        private Shape[] _otherPlayers;

        private Point _lastDisplayedCell = new Point(-1, -1);
        private Point _playerPosition = new Point();

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
                { 12, Brushes.LightSkyBlue }
            };

            IsVisibleChanged += MapView_IsVisibleChanged;
            MouseDown += MapView_MouseDown;
            SizeChanged += MapView_SizeChanged;
        }

        private void MapCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Tuple<double, double> drawingOffset = GetDrawingOffset();
            double deltaX = drawingOffset.Item1;
            double deltaY = drawingOffset.Item2;
            int ingameX = (int)((e.GetPosition(this).X / _cellWidth - deltaX));
            int ingameY = (int)((e.GetPosition(this).Y / _cellWidth) - deltaY);

            lock (_bot)
            {
                if (_bot.Game != null &&
                    _bot.Game.IsMapLoaded &&
                    _bot.Game.AreNpcReceived &&
                    _bot.Game.IsInactive &&
                    !_bot.Game.IsInBattle &&
                    _bot.Running != BotClient.State.Started)
                {
                    Npc npcOnCell = _bot.Game.Map.Npcs.FirstOrDefault(npc => npc.PositionX == ingameX && npc.PositionY == ingameY);
                    if (npcOnCell == null)
                    {
                        _bot.MoveToCell(ingameX, ingameY);
                    }
                    else
                    {
                        _bot.TalkToNpc(npcOnCell);
                    }
                }
            }
        }

        private void MapCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            FloatingTip.IsOpen = false;
        }

        private void MapCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            Tuple<double, double> drawingOffset = GetDrawingOffset();
            double deltaX = drawingOffset.Item1;
            double deltaY = drawingOffset.Item2;
            int ingameX = (int)((e.GetPosition(this).X / _cellWidth - deltaX));
            int ingameY = (int)((e.GetPosition(this).Y / _cellWidth) - deltaY);

            if (_lastDisplayedCell.X != ingameX || _lastDisplayedCell.Y != ingameY)
            {
                lock (_bot)
                {
                    if (_bot.Game != null && _bot.Game.IsMapLoaded)
                    {
                        RetrieveCellInfo(ingameX, ingameY);
                        FloatingTip.IsOpen = true;
                    }
                }
            }

            Point currentPos = e.GetPosition(MapCanvas);
            FloatingTip.HorizontalOffset = currentPos.X + 20;
            FloatingTip.VerticalOffset = currentPos.Y;
        }

        private void MapView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.Focus(this);
        }

        private void RetrieveCellInfo(int x, int y)
        {
            _lastDisplayedCell = new Point(x, y);

            StringBuilder logBuilder = new StringBuilder();
            logBuilder.AppendLine(string.Format("Cell: ({0},{1})", x, y));

            if (_bot.Game.Map.HasLink(x, y))
            {
                logBuilder.AppendLine("Link: " + _bot.Game.Map.Links[x, y].DestinationMap);
            }

            PlayerInfos[] playersOnCell = _bot.Game.Players.Values.Where(player => player.PosX == x && player.PosY == y).ToArray();
            if (playersOnCell.Length > 0)
            {
                logBuilder.AppendLine(string.Format("{0} player{1}:", playersOnCell.Length, playersOnCell.Length != 1 ? "s" : ""));
                foreach (PlayerInfos player in playersOnCell)
                {
                    logBuilder.Append("  " + player.Name);
                    if (player.IsInBattle) logBuilder.Append(" [in battle]");
                    if (player.IsMember) logBuilder.Append(" [member]");
                    if (player.IsAfk) logBuilder.Append(" [afk]");
                    logBuilder.AppendLine();
                }
            }

            Npc[] npcsOnCell = _bot.Game.Map.Npcs.Where(npc => npc.PositionX == x && npc.PositionY == y).ToArray();
            if (npcsOnCell.Length > 0)
            {
                logBuilder.AppendLine(string.Format("{0} npc{1}:", npcsOnCell.Length, npcsOnCell.Length != 1 ? "s" : ""));
                foreach (Npc npc in npcsOnCell)
                {
                    logBuilder.AppendLine("  ID: " + npc.Id);
                    if (npc.Name != string.Empty) logBuilder.AppendLine("    name: " + npc.Name);
                    logBuilder.AppendLine("    type: " + npc.TypeDescription);
                    logBuilder.AppendLine("    battler: " + npc.IsBattler.ToString());
                }
            }

            logBuilder.Length -= Environment.NewLine.Length;
            TipText.Text = logBuilder.ToString();
        }

        private void MapView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Window parent = Window.GetWindow(this);
            if (IsVisible)
            {
                if (_isMapDirty) RefreshMap();
                if (_isPlayerDirty) RefreshPlayer(!_areNpcsDirty || !_arePlayersDirty);
                if (_areNpcsDirty) RefreshNpcs();
                if (_arePlayersDirty) RefreshOtherPlayers();
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
                if (_cellWidth < 4) _cellWidth = 4;
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

                grid.Background = Brushes.White;

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
                        if (collider == 2)
                        {
                            rect.Height = _cellWidth / 4;
                        }
                        if (collider == 3 || collider == 4)
                        {
                            rect.Width = _cellWidth / 4;
                        }
                        if (collider == 14)
                        {
                            rect.Height = _cellWidth / 4;
                            rect.VerticalAlignment = VerticalAlignment.Top;
                        }
                        grid.Children.Add(rect);
                    }
                }

                _mapGrid = grid;
                MapCanvas.Children.Add(grid);

                _player = new Ellipse() { Fill = Brushes.Red, Width = _cellWidth, Height = _cellWidth };
                MapCanvas.Children.Add(_player);
                Panel.SetZIndex(_player, 100);

                RefreshPlayer(false);
                RefreshNpcs();
                RefreshOtherPlayers();
            }
        }

        public void RefreshPlayer(bool refreshEntities)
        {
            if (!IsVisible)
            {
                _isPlayerDirty = true;
                return;
            }
            _isPlayerDirty = false;

            lock (_bot)
            {
                if (_bot.Game == null || _bot.Game.Map == null || _player == null) return;
                UpdatePlayerPosition();
                if (refreshEntities)
                {
                    UpdateNpcPositions();
                    UpdateOtherPlayerPositions();
                }
            }
        }

        private void UpdatePlayerPosition()
        {
            _playerPosition = new Point(_bot.Game.PlayerX, _bot.Game.PlayerY);

            Tuple<double, double> drawingOffset = GetDrawingOffset();
            double deltaX = drawingOffset.Item1;
            double deltaY = drawingOffset.Item2;

            Canvas.SetLeft(_mapGrid, deltaX * _cellWidth);
            Canvas.SetTop(_mapGrid, deltaY * _cellWidth);
            Canvas.SetLeft(_player, (_bot.Game.PlayerX + deltaX) * _cellWidth);
            Canvas.SetTop(_player, (_bot.Game.PlayerY + deltaY) * _cellWidth);
        }

        public void RefreshNpcs()
        {
            if (!IsVisible)
            {
                _areNpcsDirty = true;
                return;
            }
            _areNpcsDirty = false;

            lock (_bot)
            {
                if (_bot.Game == null || _bot.Game.Map == null || _mapGrid == null) return;

                if (_npcs != null)
                    foreach (Shape npc in _npcs)
                        MapCanvas.Children.Remove(npc);

                _npcs = new Shape[_bot.Game.Map.Npcs.Count];
                for (int i = 0; i < _npcs.Length; i++)
                {
                    _npcs[i] = new Ellipse() { Fill = Brushes.DarkOrange, Width = _cellWidth, Height = _cellWidth };
                    MapCanvas.Children.Add(_npcs[i]);
                }

                UpdateNpcPositions();
            }
        }

        private void UpdateNpcPositions()
        {
            if (_bot.Game.Map.Npcs.Count != _npcs.Length) return;

            Tuple<double, double> drawingOffset = GetDrawingOffset();
            double deltaX = drawingOffset.Item1;
            double deltaY = drawingOffset.Item2;

            for (int i = 0; i < _npcs.Length; i++)
            {
                Canvas.SetLeft(_npcs[i], (_bot.Game.Map.Npcs[i].PositionX + deltaX) * _cellWidth);
                Canvas.SetTop(_npcs[i], (_bot.Game.Map.Npcs[i].PositionY + deltaY) * _cellWidth);
            }
        }

        public void RefreshOtherPlayers()
        {
            if (!IsVisible)
            {
                _arePlayersDirty = true;
                return;
            }
            _arePlayersDirty = false;

            lock (_bot)
            {
                if (_bot.Game == null || _bot.Game.Map == null || _mapGrid == null) return;

                if (_otherPlayers != null)
                    foreach (Shape player in _otherPlayers)
                        MapCanvas.Children.Remove(player);

                _otherPlayers = new Shape[_bot.Game.Players.Count];
                for (int i = 0; i < _otherPlayers.Length; i++)
                {
                    _otherPlayers[i] = new Ellipse() { Fill = Brushes.Green, Width = _cellWidth, Height = _cellWidth };
                    MapCanvas.Children.Add(_otherPlayers[i]);
                }
                UpdateOtherPlayerPositions();
            }
        }

        private void UpdateOtherPlayerPositions()
        {
            if (_bot.Game.Players.Count != _otherPlayers.Length) return;

            Tuple<double, double> drawingOffset = GetDrawingOffset();
            double deltaX = drawingOffset.Item1;
            double deltaY = drawingOffset.Item2;

            int playerIndex = 0;
            foreach (PlayerInfos player in _bot.Game.Players.Values)
            {
                Canvas.SetLeft(_otherPlayers[playerIndex], (player.PosX + deltaX) * _cellWidth);
                Canvas.SetTop(_otherPlayers[playerIndex], (player.PosY + deltaY) * _cellWidth);
                playerIndex++;
            }
        }

        private Tuple<double, double> GetDrawingOffset()
        {
            double canFillX = Math.Floor(MapCanvas.ActualWidth / _cellWidth);
            double canFillY = Math.Floor(MapCanvas.ActualHeight / _cellWidth);

            double deltaX = -_playerPosition.X + canFillX / 2;
            double deltaY = -_playerPosition.Y + canFillY / 2;

            if (_mapGrid.Columns <= canFillX) deltaX = 0;
            if (_mapGrid.Rows <= canFillY) deltaY = 0;

            if (deltaX < -_mapGrid.Columns + canFillX) deltaX = -_mapGrid.Columns + canFillX;
            if (deltaY < -_mapGrid.Rows + canFillY) deltaY = -_mapGrid.Rows + canFillY;

            if (deltaX > 0) deltaX = 0;
            if (deltaY > 0) deltaY = 0;

            return new Tuple<double, double>(deltaX, deltaY);
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
                RefreshPlayer(true);
            });
        }

        public void Client_NpcReceived(List<Npc> npcs)
        {
            Dispatcher.InvokeAsync(delegate
            {
                RefreshNpcs();
            });
        }

        public void Client_PlayerEnteredMap(PlayerInfos player)
        {
            Dispatcher.InvokeAsync(delegate
            {
                RefreshOtherPlayers();
            });
        }

        public void Client_PlayerLeftMap(PlayerInfos player)
        {
            Dispatcher.InvokeAsync(delegate
            {
                RefreshOtherPlayers();
            });
        }

        public void Client_PlayerMoved(PlayerInfos player)
        {
            Dispatcher.InvokeAsync(delegate
            {
                RefreshOtherPlayers();
            });
        }
    }
}
