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
        private Shape[] _otherPlayers;
        private Shape[] _npcs;
        private Point _lastDisplayedCell = new Point(-1, -1);
        
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

        private void MapCanvas_MouseEnter(object sender, MouseEventArgs e)
        {
            if (_bot.Game != null)
                floatingTip.IsOpen = true;
        }

        private void MapCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            floatingTip.IsOpen = false;
        }

        private void MapCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_bot.Game != null)
            {
                Tuple<double, double> drawingOffset = GetDrawingOffset();
                double deltaX = drawingOffset.Item1;
                double deltaY = drawingOffset.Item2;
                int ingameX = (int)((e.GetPosition(this).X / _cellWidth - deltaX));
                int ingameY = (int)((e.GetPosition(this).Y / _cellWidth) - deltaY);

                if(this._lastDisplayedCell.X!=ingameX || this._lastDisplayedCell.Y != ingameY)
                    LogCellInfo(ingameX, ingameY);

                Point currentPos = e.GetPosition(MapCanvas);

                floatingTip.HorizontalOffset = currentPos.X + 20;
                floatingTip.VerticalOffset = currentPos.Y;
            }
        }   

        private void MapView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.Focus(this);
        }

        private void LogCellInfo(int x, int y)
        {
            _lastDisplayedCell = new Point(x, y);

            string log = "";
            log += string.Format("Clicked Cell: ({0},{1})\r\n", x, y);
            if (_bot.Game.Map.HasLink(x, y))
            {
                log += "Link:\r\n";
                log += "    destination map: " + _bot.Game.Map.Links[x, y].DestinationMap + "\r\n";
            }

            PlayerInfos[] playersOnCell = _bot.Game.Players.Values.Where(player => player.PosX == x && player.PosY == y).ToArray();
            if (playersOnCell.Length > 0)
            {
                log += string.Format("{0} player(s): \r\n", playersOnCell.Length);
                foreach(PlayerInfos player in playersOnCell)
                {
                    log += "    " + player.Name + "\r\n";
                    log += "        in Battle: " + player.IsInBattle.ToString() + "\r\n";
                    log += "        membership: " + player.IsMember.ToString() + "\r\n";
                    log += "        afk: " + player.IsAfk.ToString() + "\r\n";
                }
            }

            Npc[] npcsOnCell = _bot.Game.Map.Npcs.Where(npc => npc.PositionX == x && npc.PositionY == y).ToArray();
            if (npcsOnCell.Length > 0)
            {
                log += string.Format("{0} npc(s): \r\n", npcsOnCell.Length);
                foreach (Npc npc in npcsOnCell)
                {
                    log += "    ID: " + npc.Id + "\r\n";
                    log += "        name: " + (npc.Name==""?"[unnamed]":npc.Name) + "\r\n";
                    log += "        type: " + npc.TypeDescription + "\r\n";
                    log += "        battler: " + npc.IsBattler.ToString() + "\r\n";
                }
            }

            if (log.EndsWith("\r\n"))
                log = log.Remove(log.LastIndexOf("\r\n"));
            tipText.Text = log;
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

                RefreshPlayers();
                RefreshNpcs();

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
            
            Tuple<double, double> drawingOffset = GetDrawingOffset();
            double deltaX = drawingOffset.Item1;
            double deltaY = drawingOffset.Item2;

            Canvas.SetLeft(_mapGrid, deltaX * _cellWidth);
            Canvas.SetTop(_mapGrid, deltaY * _cellWidth);
            Canvas.SetLeft(_player, (_bot.Game.PlayerX + deltaX) * _cellWidth);
            Canvas.SetTop(_player, (_bot.Game.PlayerY + deltaY) * _cellWidth);

            UpdatePlayerPositions();
            UpdateNpcPositions();
        }

        public void RefreshNpcs()
        {
            if (_npcs!=null)
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

        private void UpdateNpcPositions()
        {
            if (_mapGrid == null) //prevent Null Pointer Exception in GetDrawingOffset() when _mapGrid is not initialized
                RefreshMap();

            if (_mapGrid != null) //RefreshMap() does not initialize _mapGrid if Map is null
            {
                Tuple<double, double> drawingOffset = GetDrawingOffset();
                double deltaX = drawingOffset.Item1;
                double deltaY = drawingOffset.Item2;

                for (int i = 0; i < _npcs.Length; i++)
                {
                    Canvas.SetLeft(_npcs[i], (_bot.Game.Map.Npcs[i].PositionX + deltaX) * _cellWidth);
                    Canvas.SetTop(_npcs[i], (_bot.Game.Map.Npcs[i].PositionY + deltaY) * _cellWidth);
                }
            }
        }

        public void RefreshPlayers()
        {
            if (_otherPlayers!=null)
                foreach (Shape player in _otherPlayers)
                    MapCanvas.Children.Remove(player);

            _otherPlayers = new Shape[_bot.Game.Players.Count];
            for (int i = 0; i < _otherPlayers.Length; i++)
            {
                _otherPlayers[i] = new Ellipse() { Fill = Brushes.Green, Width = _cellWidth, Height = _cellWidth };
                MapCanvas.Children.Add(_otherPlayers[i]);
            }
            UpdatePlayerPositions();
        }

        private void UpdatePlayerPositions()
        {
            if (_mapGrid == null) //prevent Null Pointer Exception in GetDrawingOffset() when _mapGrid is not initialized
                RefreshMap();

            if (_mapGrid != null)
            {
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
        }

        private Tuple<double, double> GetDrawingOffset()
        {
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

            return new Tuple<double, double>(deltaX, deltaY);
        }

        public void Client_MapLoaded(string mapName)
        {
            Dispatcher.InvokeAsync(delegate
            {
                RefreshMap();
            });
        }

        public void Client_PlayerEnteredMap(PlayerInfos player)
        {
            Dispatcher.InvokeAsync(delegate
            {
                RefreshPlayers();
            });
        }

        public void Client_PlayerLeftMap(PlayerInfos player)
        {
            Dispatcher.InvokeAsync(delegate
            {
                RefreshPlayers();
            });
        }

        public void Client_PlayerMoved(PlayerInfos player)
        {
            Dispatcher.InvokeAsync(delegate
            {
                RefreshPlayers();
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
