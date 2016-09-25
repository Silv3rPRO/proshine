using System;
using System.Collections.Generic;
using System.Linq;

namespace PROProtocol
{
    public class GameClient
    {
        public Random Rand { get; private set; }
        public Language I18n { get; private set; }

        public bool IsConnected { get; private set; }
        public bool IsAuthenticated { get; private set; }
        public string PlayerName { get; private set; }

        public int PlayerX { get; private set; }
        public int PlayerY { get; private set; }
        public string MapName { get; private set; }
        public Map Map { get; private set; }

        public int PokedexOwned { get; private set; }
        public int PokedexSeen { get; private set; }
        public int PokedexEvolved { get; private set; }

        public bool IsInBattle { get; private set; }
        public bool IsSurfing { get; private set; }
        public bool IsBiking { get; private set; }
        public bool IsOnGround { get; private set; }
        public bool IsPCOpen { get; private set; }
        public bool CanUseCut { get; private set; }
        public bool CanUseSmashRock { get; private set; }
        public bool IsPrivateMessageOn { get; private set; }

        public int Money { get; private set; }
        public int Coins { get; private set; }
        public List<Pokemon> Team { get; private set; }
        public List<Pokemon> CurrentPCBox { get; private set; }
        public List<InventoryItem> Items { get; private set; }
        public string PokemonTime { get; private set; }
        public string Weather { get; private set; }
        public int PCGreatestUid { get; private set; }

        public bool IsScriptActive { get; private set; }
        public string ScriptId { get; private set; }
        public int ScriptStatus { get; private set; }
        public string[] DialogContent { get; private set; }

        public Battle ActiveBattle { get; private set; }
        public Shop OpenedShop { get; private set; }

        public List<ChatChannel> Channels { get; private set; }
        public List<string> Conversations { get; private set; }
        public Dictionary<string, PlayerInfos> Players { get; private set; }
        private DateTime _updatePlayers;
        private DateTime _refreshBoxTimeout;
        public bool IsPCBoxRefreshing { get; private set; }
        public int CurrentPCBoxId { get; private set; }

        public event Action ConnectionOpened;
        public event Action<Exception> ConnectionFailed;
        public event Action<Exception> ConnectionClosed;
        public event Action LoggedIn;
        public event Action<AuthenticationResult> AuthenticationFailed;
        public event Action<int> QueueUpdated;

        public event Action<string, int, int> PositionUpdated;
        public event Action PokemonsUpdated;
        public event Action InventoryUpdated;
        public event Action BattleStarted;
        public event Action<string> BattleMessage;
        public event Action BattleEnded;
        public event Action<string> DialogOpened;
        public event Action<string, string, int> EmoteMessage;
        public event Action<string, string, string> ChatMessage;
        public event Action RefreshChannelList;
        public event Action<string, string, string, string> ChannelMessage;
        public event Action<string, string> ChannelSystemMessage;
        public event Action<string, string, string, string> ChannelPrivateMessage;
        public event Action<string> SystemMessage;
        public event Action<string, string, string, string> PrivateMessage;
        public event Action<string, string, string> LeavePrivateMessage;
        public event Action<PlayerInfos> PlayerUpdated;
        public event Action<PlayerInfos> PlayerAdded;
        public event Action<PlayerInfos> PlayerRemoved;
        public event Action<string, string> InvalidPacket;
        public event Action<int, string, int> LearningMove;
        public event Action<int, int> Evolving;
        public event Action<string, string> PokeTimeUpdated;
        public event Action<Shop> ShopOpened;
        public event Action<List<Pokemon>> PCBoxUpdated;

        private const string Version = "0.957";

        private GameConnection _connection;
        private DateTime _lastMovement;
        private List<Direction> _movements = new List<Direction>();
        private Direction? _slidingDirection;
        private bool _surfAfterMovement;
        private Queue<object> _dialogResponses = new Queue<object>();

        private Timeout _movementTimeout = new Timeout();
        private Timeout _battleTimeout = new Timeout();
        private Timeout _loadingTimeout = new Timeout();
        private Timeout _mountingTimeout = new Timeout();
        private Timeout _teleportationTimeout = new Timeout();
        private Timeout _dialogTimeout = new Timeout();
        private Timeout _swapTimeout = new Timeout();
        private Timeout _itemUseTimeout = new Timeout();
        private Timeout _fishingTimeout = new Timeout();
        private Timeout _refreshingPCBox = new Timeout();

        private MapClient _mapClient;

        public bool IsInactive
        {
            get
            {
                return _movements.Count == 0
                    && !_movementTimeout.IsActive
                    && !_battleTimeout.IsActive
                    && !_loadingTimeout.IsActive
                    && !_mountingTimeout.IsActive
                    && !_teleportationTimeout.IsActive
                    && !_dialogTimeout.IsActive
                    && !_swapTimeout.IsActive
                    && !_itemUseTimeout.IsActive
                    && !_fishingTimeout.IsActive
                    && !_refreshingPCBox.IsActive;
            }
        }

        public bool IsMapLoaded
        {
            get { return Map != null; }
        }
        public bool AreNpcReceived { get; private set; }

        public GameClient(GameConnection connection, MapConnection mapConnection)
        {
            _mapClient = new MapClient(mapConnection);
            _mapClient.ConnectionOpened += MapClient_ConnectionOpened;
            _mapClient.ConnectionClosed += MapClient_ConnectionClosed;
            _mapClient.ConnectionFailed += MapClient_ConnectionFailed;
            _mapClient.MapLoaded += MapClient_MapLoaded;

            _connection = connection;
            _connection.PacketReceived += OnPacketReceived;
            _connection.Connected += OnConnectionOpened;
            _connection.Disconnected += OnConnectionClosed;

            Rand = new Random();
            I18n = new Language();
            Team = new List<Pokemon>();
            CurrentPCBox = new List<Pokemon>();
            Items = new List<InventoryItem>();
            Channels = new List<ChatChannel>();
            Conversations = new List<string>();
            Players = new Dictionary<string, PlayerInfos>();
            PCGreatestUid = -1;
            IsPrivateMessageOn = true;
        }

        public void Open()
        {
            _mapClient.Open();
        }

        public void Close(Exception error = null)
        {
            _connection.Close(error);
        }

        public void Update()
        {
            _mapClient.Update();
            _connection.Update();
            if (!IsAuthenticated)
                return;

            _movementTimeout.Update();
            _battleTimeout.Update();
            _loadingTimeout.Update();
            _mountingTimeout.Update();
            _teleportationTimeout.Update();
            _dialogTimeout.Update();
            _swapTimeout.Update();
            _itemUseTimeout.Update();
            _fishingTimeout.Update();
            _refreshingPCBox.Update();

            SendRegularPing();
            UpdateMovement();
            UpdateScript();
            UpdatePlayers();
            UpdatePCBox();
        }

        public void CloseChannel(string channelName)
        {
            if (Channels.Any(e => e.Name == channelName))
            {
                SendMessage("/cgleave " + channelName);
            }
        }

        public void CloseConversation(string pmName)
        {
            if (Conversations.Contains(pmName))
            {
                SendMessage("/pm rem " + pmName + "-=-" + PlayerName + '|' + PlayerName);
                Conversations.Remove(pmName);
            }
        }

        private void SendRegularPing()
        {
            if ((DateTime.UtcNow - _lastMovement).TotalSeconds >= 10)
            {
                _lastMovement = DateTime.UtcNow;
                // DSSock.Update
                SendPacket("2");
            }
        }

        private void UpdateMovement()
        {
            if (!IsMapLoaded) return;

            if (!_movementTimeout.IsActive && _movements.Count > 0)
            {
                Direction direction = _movements[0];
                _movements.RemoveAt(0);

                if (ApplyMovement(direction))
                {
                    SendMovement(direction.AsChar());
                    _movementTimeout.Set(IsBiking ? 125 : 250);
                    if (Map.HasLink(PlayerX, PlayerY))
                    {
                        _teleportationTimeout.Set();
                    }
                }

                if (_movements.Count == 0 && _surfAfterMovement)
                {
                    _movementTimeout.Set(Rand.Next(750, 2000));
                }
            }
            if (!_movementTimeout.IsActive && _movements.Count == 0 && _surfAfterMovement)
            {
                _surfAfterMovement = false;
                UseSurf();
            }
        }

        private void UpdatePlayers()
        {
            if (_updatePlayers < DateTime.UtcNow)
            {
                foreach (string playerName in Players.Keys.ToArray())
                {
                    if (Players[playerName].IsExpired())
                    {
                        PlayerRemoved?.Invoke(Players[playerName]);
                        Players.Remove(playerName);
                    }
                }
                _updatePlayers = DateTime.UtcNow.AddSeconds(5);
            }
        }

        private void UpdatePCBox()
        {
            // if we did not receive an answer, then the box is empty
            if (IsPCBoxRefreshing && _refreshBoxTimeout > DateTime.UtcNow)
            {
                IsPCBoxRefreshing = false;
                if (Map.IsPC(PlayerX, PlayerY - 1))
                {
                    IsPCOpen = true;
                }
                CurrentPCBox = new List<Pokemon>();
                PCBoxUpdated?.Invoke(CurrentPCBox);
            }
        }

        private bool ApplyMovement(Direction direction)
        {
            int destinationX = PlayerX;
            int destinationY = PlayerY;
            bool isOnGround = IsOnGround;
            bool isSurfing = IsSurfing;

            direction.ApplyToCoordinates(ref destinationX, ref destinationY);

            Map.MoveResult result = Map.CanMove(direction, destinationX, destinationY, isOnGround, isSurfing, CanUseCut, CanUseSmashRock);
            if (Map.ApplyMovement(direction, result, ref destinationX, ref destinationY, ref isOnGround, ref isSurfing))
            {
                PlayerX = destinationX;
                PlayerY = destinationY;
                IsOnGround = isOnGround;
                IsSurfing = isSurfing;
                PositionUpdated?.Invoke(MapName, PlayerX, PlayerY);

                if (result == Map.MoveResult.Icing)
                {
                    _movements.Insert(0, direction);
                }

                if (result == Map.MoveResult.Sliding)
                {
                    int slider = Map.GetSlider(destinationX, destinationY);
                    if (slider != -1)
                    {
                        _slidingDirection = Map.SliderToDirection(slider);
                    }
                }

                if (_slidingDirection != null)
                {
                    _movements.Insert(0, _slidingDirection.Value);
                }

                return true;
            }

            _slidingDirection = null;
            return false;
        }

        private void UpdateScript()
        {
            if (IsScriptActive && !_dialogTimeout.IsActive)
            {
                if (ScriptStatus == 0)
                {
                    _dialogResponses.Clear();
                    IsScriptActive = false;
                }
                else if (ScriptStatus == 1)
                {
                    SendDialogResponse(0);
                    _dialogTimeout.Set();
                }
                else if (ScriptStatus == 3 || ScriptStatus == 4)
                {
                    SendDialogResponse(GetNextDialogResponse());
                    _dialogTimeout.Set();
                }
                else if (ScriptStatus == 1234) // Yes, this is a magic value. I don't care.
                {
                    SendCreateCharacter(Rand.Next(14), Rand.Next(28), Rand.Next(8), Rand.Next(6), Rand.Next(5));
                    IsScriptActive = false;
                    _dialogTimeout.Set();
                }
            }
        }

        private int GetNextDialogResponse()
        {
            if (_dialogResponses.Count > 0)
            {
                object response = _dialogResponses.Dequeue();
                if (response is int)
                {
                    return (int)response;
                }
                else if (response is string)
                {
                    string text = ((string)response).ToUpperInvariant();
                    for (int i = 1; i < DialogContent.Length; ++i)
                    {
                        if (DialogContent[i].ToUpperInvariant().Equals(text))
                        {
                            return i;
                        }
                    }
                }
            }
            return 1;
        }

        public int DistanceTo(int cellX, int cellY)
        {
            return Math.Abs(PlayerX - cellX) + Math.Abs(PlayerY - cellY);
        }

        public static int DistanceBetween(int fromX, int fromY, int toX, int toY)
        {
            return Math.Abs(fromX - toX) + Math.Abs(fromY - toY);
        }

        public void SendPacket(string packet)
        {
#if DEBUG
            Console.WriteLine("[>] " + packet);
#endif
            _connection.Send(packet);
        }

        public void SendMessage(string text)
        {
            // DSSock.sendMSG
            SendPacket("{|.|" + text);
        }

        public void SendPrivateMessage(string nickname, string text)
        {
            // DSSock.sendMSG
            string pmHeader = "/pm " + PlayerName + "-=-" + nickname;
            SendPacket("{|.|" + pmHeader + '|' + text);
        }

        public void SendCreateCharacter(int hair, int colour, int tone, int clothe, int eyes)
        {
            SendMessage("/setchar " + hair + "," + colour + "," + tone + "," + clothe + "," + eyes);
        }

        public void SendAuthentication(string username, string password, string hash)
        {
            // DSSock.AttemptLogin
            SendPacket("+|.|" + username + "|.|" + password + "|.|" + Version + "|.|" + hash);
        }

        public void SendUseItem(int id, int pokemon = 0)
        {
            string toSend = "*|.|" + id;
            if (pokemon != 0)
            {
                toSend += "|.|" + pokemon;
            }
            SendPacket(toSend);
        }
        
        public void SendGiveItem(int pokemonUid, int itemId)
        {
            SendMessage("/giveitem " + pokemonUid + "," + itemId);
        }

        public void SendTakeItem(int pokemonUid)
        {
            SendMessage("/takeitem " + pokemonUid);
        }

        public void LearnMove(int pokemonUid, int moveToForgetUid)
        {
            _swapTimeout.Set();
            SendLearnMove(pokemonUid, moveToForgetUid);
        }

        private void SendLearnMove(int pokemonUid, int moveToForgetUid)
        {
            SendPacket("^|.|" + pokemonUid + "|.|" + moveToForgetUid);
        }

        private void SendMovePokemonToPC(int pokemonUid)
        {
            SendPacket("?|.|" + pokemonUid + "|.|-1");
        }

        // if there is a pokemon in teamSlot, it will be swapped
        private void SendMovePokemonFromPC(int pokemonUid, int teamSlot)
        {
            SendPacket("?|.|" + pokemonUid + "|.|" + teamSlot);
        }

        private void SendRefreshPCBox(int box, string search)
        {
            SendPacket("M|.|" + box + "|.|" + search);
        }

        private void SendReleasePokemon(int pokemonUid)
        {
            SendMessage("/release " + pokemonUid);
        }

        private void SendPrivateMessageOn()
        {
            SendMessage("/pmon");
        }

        private void SendPrivateMessageOff()
        {
            SendMessage("/pmoff");
        }

        private void SendPrivateMessageAway()
        {
            SendMessage("/pmaway");
        }

        public bool PrivateMessageOn()
        {
            IsPrivateMessageOn = true;
            SendPrivateMessageOn();
            return true;
        }

        public bool PrivateMessageOff()
        {
            IsPrivateMessageOn = false;
            SendPrivateMessageOff();
            return true;
        }

        // /pmaway does not seem to do anything
        public bool PrivateMessageAway()
        {
            SendPrivateMessageAway();
            return true;
        }

        public bool ReleasePokemonFromPC(int boxId, int boxPokemonId)
        {
            if (!IsPCOpen || IsPCBoxRefreshing || boxId < 1 || boxId > 67
                || boxPokemonId < 1 || boxPokemonId > 15 || boxPokemonId > CurrentPCBox.Count)
            {
                return false;
            }
            int pokemonUid = GetPokemonPCUid(boxId, boxPokemonId);
            if (pokemonUid == -1 || pokemonUid != CurrentPCBox[boxPokemonId].Uid)
            {
                return false;
            }
            SendReleasePokemon(pokemonUid);
            return true;
        }

        public bool ReleasePokemonFromTeam(int pokemonUid)
        {
            if (!IsPCOpen || IsPCBoxRefreshing
                || pokemonUid < 1 || pokemonUid > 6 || pokemonUid > Team.Count)
            {
                return false;
            }
            SendReleasePokemon(pokemonUid);
            return true;
        }

        public bool RefreshPCBox(int boxId)
        {
            if (!IsPCOpen || boxId < 1 || boxId > 67 || _refreshingPCBox.IsActive || IsPCBoxRefreshing)
            {
                return false;
            }
            _refreshingPCBox.Set(Rand.Next(1500, 2000)); // this is the amount of time we wait for an answer
            CurrentPCBoxId = boxId;
            IsPCBoxRefreshing = true;
            CurrentPCBox = null;
            _refreshBoxTimeout = DateTime.UtcNow.AddSeconds(5); // this is to avoid a flood of the function
            SendRefreshPCBox(boxId - 1, "ID");
            return true;
        }

        public bool RefreshCurrentPCBox()
        {
            return RefreshPCBox(CurrentPCBoxId);
        }

        private int GetPokemonPCUid(int box, int id)
        {
            if (box < 1 || box > 67 || id < 1 || id > 15)
            {
                return -1;
            }
            int result = (box - 1) * 15 + 6 + id;
            // ensures we cannot access a pokemon we do not have or know
            if (result > PCGreatestUid || CurrentPCBox == null || box != CurrentPCBoxId)
            {
                return -1;
            }
            return result;
        }

        public bool DepositPokemonToPC(int pokemonUid)
        {
            if (!IsPCOpen || pokemonUid < 1 || pokemonUid > 6 || Team.Count < pokemonUid)
            {
                return false;
            }
            SendMovePokemonToPC(pokemonUid);
            return true;
        }

        public bool WithdrawPokemonFromPC(int boxId, int boxPokemonId)
        {
            int pcPokemonUid = GetPokemonPCUid(boxId, boxPokemonId);
            if (pcPokemonUid == -1)
            {
                return false;
            }
            if (!IsPCOpen || pcPokemonUid < 7 || pcPokemonUid > PCGreatestUid || Team.Count >= 6)
            {
                return false;
            }
            SendMovePokemonFromPC(pcPokemonUid, Team.Count + 1);
            return true;
        }

        public bool SwapPokemonFromPC(int boxId, int boxPokemonId, int teamPokemonUid)
        {
            int pcPokemonUid = GetPokemonPCUid(boxId, boxPokemonId);
            if (pcPokemonUid == -1)
            {
                return false;
            }
            if (!IsPCOpen || pcPokemonUid < 7 || pcPokemonUid > PCGreatestUid
                || teamPokemonUid < 1 || teamPokemonUid > 6 || Team.Count < teamPokemonUid)
            {
                return false;
            }
            SendMovePokemonFromPC(pcPokemonUid, teamPokemonUid);
            return true;
        }

        public bool SwapPokemon(int pokemon1, int pokemon2)
        {
            if (IsInBattle || pokemon1 < 1 || pokemon2 < 1 || Team.Count < pokemon1 || Team.Count < pokemon2 || pokemon1 == pokemon2)
            {
                return false;
            }
            if (!_swapTimeout.IsActive)
            {
                SendSwapPokemons(pokemon1, pokemon2);
                _swapTimeout.Set();
                return true;
            }
            return false;
        }

        public void Move(Direction direction)
        {
            _movements.Add(direction);
        }

        public void RequestResync()
        {
            SendMessage("/syn");
            _teleportationTimeout.Set();
        }

        public void UseAttack(int number)
        {
            SendAttack(number.ToString());
            _battleTimeout.Set();
        }

        public void UseItem(int id, int pokemonUid = 0)
        {
            if (!(pokemonUid >= 0 && pokemonUid <= 6) || !HasItemId(id))
            {
                return;
            }
            InventoryItem item = GetItemFromId(id);
            if (item == null || item.Quantity == 0)
            {
                return;
            }
            if (pokemonUid == 0) // simple use
            {
                if (!_itemUseTimeout.IsActive && !IsInBattle && (item.Scope == 8 || item.Scope == 10 || item.Scope == 15))
                {
                    SendUseItem(id);
                    _itemUseTimeout.Set();
                }
                else if (!_battleTimeout.IsActive && IsInBattle && item.Scope == 5)
                {
                    SendAttack("item" + id);
                    _battleTimeout.Set();
                }
            }
            else // use item on pokemon
            {
                if (!_itemUseTimeout.IsActive && !IsInBattle
                    && (item.Scope == 2 || item.Scope == 3 || item.Scope == 9
                        || item.Scope == 13 || item.Scope == 14))
                {
                    SendUseItem(id, pokemonUid);
                    _itemUseTimeout.Set();
                }
                else if (!_battleTimeout.IsActive && IsInBattle && item.Scope == 2)
                {
                    SendAttack("item" + id + ":" + pokemonUid);
                    _battleTimeout.Set();
                }
            }
        }
        
        public bool GiveItemToPokemon(int pokemonUid, int itemId)
        {
            if (!(pokemonUid >= 1 && pokemonUid <= Team.Count))
            {
                return false;
            }
            InventoryItem item = GetItemFromId(itemId);
            if (item == null || item.Quantity == 0)
            {
                return false;
            }
            if (!_itemUseTimeout.IsActive && !IsInBattle
                && (item.Scope == 2 || item.Scope == 3 || item.Scope == 9 || item.Scope == 13
                || item.Scope == 14 || item.Scope == 5 || item.Scope == 12 || item.Scope == 6))
            {
                SendGiveItem(pokemonUid, itemId);
                _itemUseTimeout.Set();
                return true;
            }
            return false;
        }

        public bool TakeItemFromPokemon(int pokemonUid)
        {
            if (!(pokemonUid >= 1 && pokemonUid <= Team.Count))
            {
                return false;
            }
            if (!_itemUseTimeout.IsActive && Team[pokemonUid - 1].ItemHeld != "")
            {
                SendTakeItem(pokemonUid);
                _itemUseTimeout.Set();
                return true;
            }
            return false;
        }

        public bool HasSurfAbility()
        {
            return HasMove("Surf") &&
                (Map.Region == "1" && HasItemName("Soul Badge") ||
                Map.Region == "2" && HasItemName("Fog Badge") ||
                Map.Region == "3" && HasItemName("Balance Badge"));
        }

        public bool HasCutAbility()
        {
            return (HasMove("Cut") || HasTreeaxe()) &&
                (Map.Region == "1" && HasItemName("Cascade Badge") ||
                Map.Region == "2" && HasItemName("Hive Badge") ||
                Map.Region == "3" && HasItemName("Stone Badge"));
        }

        public bool HasRockSmashAbility()
        {
            return HasMove("Rock Smash") || HasPickaxe();
        }

        public bool HasTreeaxe()
        {
            return HasItemId(838) && HasItemId(317);
        }

        public bool HasPickaxe()
        {
            return HasItemId(839);
        }

        public bool PokemonUidHasMove(int pokemonUid, string moveName)
        {
            return Team.FirstOrDefault(p => p.Uid == pokemonUid)?.Moves.Any(m => m.Name?.Equals(moveName, StringComparison.InvariantCultureIgnoreCase) ?? false) ?? false;
        }

        public bool HasMove(string moveName)
        {
            return Team.Any(p => p.Moves.Any(m => m.Name?.Equals(moveName, StringComparison.InvariantCultureIgnoreCase) ?? false));
        }

        public int GetMovePosition(int pokemonUid, string moveName)
        {
            return Team[pokemonUid].Moves.FirstOrDefault(m => m.Name?.Equals(moveName, StringComparison.InvariantCultureIgnoreCase) ?? false)?.Position ?? -1;
        }

        public InventoryItem GetItemFromId(int id)
        {
            return Items.FirstOrDefault(i => i.Id == id && i.Quantity > 0);
        }

        public bool HasItemId(int id)
        {
            return GetItemFromId(id) != null;
        }

        public InventoryItem GetItemFromName(string itemName)
        {
            return Items.FirstOrDefault(i => i.Name.Equals(itemName, StringComparison.InvariantCultureIgnoreCase) && i.Quantity > 0);
        }

        public bool HasItemName(string itemName)
        {
            return GetItemFromName(itemName) != null;
        }

        public bool HasPokemonInTeam(string pokemonName)
        {
            return FindFirstPokemonInTeam(pokemonName) != null;
        }

        public Pokemon FindFirstPokemonInTeam(string pokemonName)
        {
            return Team.FirstOrDefault(p => p.Name.Equals(pokemonName, StringComparison.InvariantCultureIgnoreCase));
        }

        public void UseSurf()
        {
            SendMessage("/surf");
            _mountingTimeout.Set();
        }

        public void UseSurfAfterMovement()
        {
            _surfAfterMovement = true;
        }

        public void RunFromBattle()
        {
            UseAttack(5);
        }

        public void ChangePokemon(int number)
        {
            UseAttack(number + 5);
        }

        public void TalkToNpc(int id)
        {
            SendTalkToNpc(id);
            _dialogTimeout.Set();
        }

        public bool OpenPC()
        {
            if (!Map.IsPC(PlayerX, PlayerY - 1))
            {
                return false;
            }
            IsPCOpen = true;
            return RefreshPCBox(1);
        }

        public void PushDialogAnswer(int index)
        {
            _dialogResponses.Enqueue(index);
        }

        public void PushDialogAnswer(string text)
        {
            _dialogResponses.Enqueue(text);
        }

        public bool BuyItem(int itemId, int quantity)
        {
            if (OpenedShop != null && OpenedShop.Items.Any(item => item.Id == itemId))
            {
                _itemUseTimeout.Set();
                SendShopPokemart(OpenedShop.Id, itemId, quantity);
                return true;
            }
            return false;
        }

        private void MapClient_ConnectionOpened()
        {
#if DEBUG
            Console.WriteLine("[+++] Connecting to the game server");
#endif
            _connection.Connect();
        }

        private void MapClient_ConnectionFailed(Exception ex)
        {
            ConnectionFailed?.Invoke(ex);
        }

        private void MapClient_ConnectionClosed(Exception ex)
        {
            Close(ex);
        }

        private void MapClient_MapLoaded(string mapName, Map map)
        {
            if (mapName == MapName)
            {
                Players.Clear();

                Map = map;
                // DSSock.loadMap
                SendPacket("-");
                SendPacket("k|.|" + MapName.ToLowerInvariant());

                CanUseCut = HasCutAbility();
                CanUseSmashRock = HasRockSmashAbility();
            }
            else
            {
                InvalidPacket?.Invoke(mapName, "Received a map that is not the current map");
            }
        }

        private void OnPacketReceived(string packet)
        {
            ProcessPacket(packet);
        }

        private void OnConnectionOpened()
        {
            IsConnected = true;
#if DEBUG
            Console.WriteLine("[+++] Connection opened");
#endif
            ConnectionOpened?.Invoke();
        }

        private void OnConnectionClosed(Exception ex)
        {
            _mapClient.Close();
            if (!IsConnected)
            {
#if DEBUG
                Console.WriteLine("[---] Connection failed");
#endif
                ConnectionFailed?.Invoke(ex);
            }
            else
            {
                IsConnected = false;
#if DEBUG
                Console.WriteLine("[---] Connection closed");
#endif
                ConnectionClosed?.Invoke(ex);
            }
        }

        private void SendMovement(string direction)
        {
            _lastMovement = DateTime.UtcNow;
            // Consider the pokemart closed after the first movement.
            OpenedShop = null;
            IsPCOpen = false;
            // DSSock.sendMove
            SendPacket("/|.|" + direction);
        }

        private void SendAttack(string number)
        {
            // DSSock.sendAttack
            // DSSock.RunButton
            SendPacket("(|.|" + number);
        }

        private void SendTalkToNpc(int npcId)
        {
            // DSSock.Interact
            SendPacket("N|.|" + npcId);
        }

        private void SendDialogResponse(int number)
        {
            // DSSock.ClickButton
            SendPacket("R|.|" + ScriptId + "|.|" + number);
        }

        public void SendAcceptEvolution(int evolvingPokemonUid, int evolvingItem)
        {
            // DSSock.AcceptEvo
            SendPacket("h|.|" + evolvingPokemonUid + "|.|" + evolvingItem);
        }

        public void SendCancelEvolution(int evolvingPokemonUid, int evolvingItem)
        {
            // DSSock.CancelEvo
            SendPacket("j|.|" + evolvingPokemonUid + "|.|" + evolvingItem);
        }

        private void SendSwapPokemons(int pokemon1, int pokemon2)
        {
            SendPacket("?|.|" + pokemon2 + "|.|" + pokemon1);
        }

        private void SendShopPokemart(int shopId, int itemId, int quantity)
        {
            SendPacket("c|.|" + shopId + "|.|" + itemId + "|.|" + quantity);
        }

        private void ProcessPacket(string packet)
        {
#if DEBUG
            Console.WriteLine(packet);
#endif

            if (packet.Substring(0, 1) == "U")
            {
                packet = "U|.|" + packet.Substring(1);
            }

            string[] data = packet.Split(new string[] { "|.|" }, StringSplitOptions.None);
            string type = data[0].ToLowerInvariant();
            switch (type)
            {
                case "5":
                    OnLoggedIn(data);
                    break;
                case "6":
                    OnAuthenticationResult(data);
                    break;
                case ")":
                    OnQueueUpdated(data);
                    break;
                case "q":
                    OnPlayerPosition(data);
                    break;
                case "s":
                    OnPlayerSync(data);
                    break;
                case "i":
                    OnPlayerInfos(data);
                    break;
                case "(":
                    // CDs ?
                    break;
                case "e":
                    OnUpdateTime(data);
                    break;
                case "@":
                    OnNpcBattlers(data);
                    break;
                case "*":
                    OnNpcDestroy(data);
                    break;
                case "#":
                    OnTeamUpdate(data);
                    break;
                case "d":
                    OnInventoryUpdate(data);
                    break;
                case "&":
                    OnItemsUpdate(data);
                    break;
                case "!":
                    OnBattleJoin(packet);
                    break;
                case "a":
                    OnBattleMessage(data);
                    break;
                case "r":
                    OnScript(data);
                    break;
                case "$":
                    OnBikingUpdate(data);
                    break;
                case "%":
                    OnSurfingUpdate(data);
                    break;
                case "^":
                    OnLearningMove(data);
                    break;
                case "h":
                    OnEvolving(data);
                    break;
                case "u":
                    OnUpdatePlayer(data);
                    break;
                case "c":
                    OnChannels(data);
                    break;
                case "w":
                    OnChatMessage(data);
                    break;
                case "o":
                    // Shop content
                    break;
                case "pm":
                    OnPrivateMessage(data);
                    break;
                case ".":
                    // DSSock.ProcessCommands
                    SendPacket("_");
                    break;
                case "'":
                    // DSSock.ProcessCommands
                    SendPacket("'");
                    break;
                case "m":
                    OnPCBox(data);
                    break;
                default:
#if DEBUG
                    Console.WriteLine(" ^ unhandled /!\\");
#endif
                    break;
            }
        }


        private void OnLoggedIn(string[] data)
        {
            // DSSock.ProcessCommands
            SendPacket(")");
            SendPacket("_");
            SendPacket("g");
            IsAuthenticated = true;

            if (data[1] == "1")
            {
                IsScriptActive = true;
                ScriptStatus = 1234;
                _dialogTimeout.Set(Rand.Next(4000, 8000));
            }

            Console.WriteLine("[Login] Authenticated successfully");
            LoggedIn?.Invoke();
        }

        private void OnAuthenticationResult(string[] data)
        {
            AuthenticationResult result = (AuthenticationResult)Convert.ToInt32(data[1]);

            if (result != AuthenticationResult.ServerFull)
            {
                AuthenticationFailed?.Invoke(result);
                Close();
            }
        }

        private void OnQueueUpdated(string[] data)
        {
            string[] queueData = data[1].Split('|');

            int position = Convert.ToInt32(queueData[0]);
            QueueUpdated?.Invoke(position);
        }

        private void OnPlayerPosition(string[] data)
        {
            string[] mapData = data[1].Split(new string[] { "|" }, StringSplitOptions.None);
            string map = mapData[0];
            int playerX = Convert.ToInt32(mapData[1]);
            int playerY = Convert.ToInt32(mapData[2]);
            if (playerX != PlayerX || playerY != PlayerY || map != MapName)
            {
                PlayerX = playerX;
                PlayerY = playerY;
                LoadMap(map);
                IsOnGround = (mapData[3] == "1");
                if (Convert.ToInt32(mapData[4]) == 1)
                {
                    IsSurfing = true;
                    IsBiking = false;
                }
                // DSSock.sendSync
                SendPacket("S");
            }

            PositionUpdated?.Invoke(MapName, PlayerX, playerY);

            _teleportationTimeout.Cancel();
        }

        private void OnPlayerSync(string[] data)
        {
            string[] mapData = data[1].Split(new string[] { "|" }, StringSplitOptions.None);

            if (mapData.Length < 2)
                return;

            string map = mapData[0];
            int playerX = Convert.ToInt32(mapData[1]);
            int playerY = Convert.ToInt32(mapData[2]);
            if (map.Length > 1)
            {
                PlayerX = playerX;
                PlayerY = playerY;
                LoadMap(map);
            }
            IsOnGround = (mapData[3] == "1");

            PositionUpdated?.Invoke(MapName, PlayerX, playerY);
        }

         private void OnPlayerInfos(string[] data)
        {
            string[] playerData = data[1].Split('|');
            PlayerName = playerData[0];
            PokedexOwned = Convert.ToInt32(playerData[4]);
            PokedexSeen = Convert.ToInt32(playerData[5]);
            PokedexEvolved = Convert.ToInt32(playerData[6]);
        }

        private void OnUpdateTime(string[] data)
        {
            string[] timeData = data[1].Split('|');

            PokemonTime = timeData[0];
            DateTime dt = Convert.ToDateTime(PokemonTime);

            Weather = timeData[1];

            PokeTimeUpdated?.Invoke(PokemonTime, Weather);
        }

        private void OnNpcBattlers(string[] data)
        {
            if (!IsMapLoaded) return;

            Map.Npcs.Clear();
            foreach (Npc npc in Map.OriginalNpcs)
            {
                Map.Npcs.Add(npc.Clone());
            }

            AreNpcReceived = true;
        }

        private void OnNpcDestroy(string[] data)
        {
            if (!IsMapLoaded) return;

            string[] npcData = data[1].Split('|');

            Map.Npcs.Clear();
            foreach (Npc npc in Map.OriginalNpcs)
            {
                Map.Npcs.Add(npc.Clone());
            }

            foreach (string npcText in npcData)
            {
                int npcId = int.Parse(npcText);
                foreach (Npc npc in Map.Npcs)
                {
                    if (npc.Id == npcId)
                    {
                        Map.Npcs.Remove(npc);
                        break;
                    }
                }
            }
        }

        private void OnTeamUpdate(string[] data)
        {
            string[] teamData = data[1].Split(new[] { "\r\n" }, StringSplitOptions.None);

            Team.Clear();
            foreach (string pokemon in teamData)
            {
                if (pokemon == string.Empty)
                    continue;

                string[] pokemonData = pokemon.Split('|');

                Team.Add(new Pokemon(pokemonData));
            }

            if (IsMapLoaded)
            {
                CanUseCut = HasCutAbility();
                CanUseSmashRock = HasRockSmashAbility();
            }

            if (_swapTimeout.IsActive)
            {
                _swapTimeout.Set(Rand.Next(500, 1000));
            }
            PokemonsUpdated?.Invoke();
        }

        private void OnInventoryUpdate(string[] data)
        {
            Money = Convert.ToInt32(data[1]);
            Coins = Convert.ToInt32(data[2]);
            UpdateItems(data[3]);
        }

        private void OnItemsUpdate(string[] data)
        {
            UpdateItems(data[1]);
        }

        private void UpdateItems(string content)
        {
            Items.Clear();

            string[] itemsData = content.Split(new[] { "\r\n" }, StringSplitOptions.None);
            foreach (string item in itemsData)
            {
                if (item == string.Empty)
                    continue;
                string[] itemData = item.Split(new[] { "|" }, StringSplitOptions.None);
                Items.Add(new InventoryItem(itemData[0], Convert.ToInt32(itemData[1]), Convert.ToInt32(itemData[2]), Convert.ToInt32(itemData[3])));
            }

            if (_itemUseTimeout.IsActive)
            {
                _itemUseTimeout.Set(Rand.Next(500, 1000));
            }
            InventoryUpdated?.Invoke();
        }

        private void OnBattleJoin(string packet)
        {
            string[] data = packet.Substring(4).Split('|');

            IsScriptActive = false;

            IsInBattle = true;
            ActiveBattle = new Battle(PlayerName, data);

            _movements.Clear();
            _slidingDirection = null;

            _battleTimeout.Set(Rand.Next(4000, 6000));
            _fishingTimeout.Cancel();

            BattleStarted?.Invoke();

            string[] battleMessages = ActiveBattle.BattleText.Split(new string[] { "\r\n" }, StringSplitOptions.None);

            foreach (string message in battleMessages)
            {
                if (!ActiveBattle.ProcessMessage(Team, message))
                {
                    BattleMessage?.Invoke(I18n.Replace(message));
                }
            }
        }

        private void OnBattleMessage(string[] data)
        {
            if (!IsInBattle)
            {
                return;
            }

            string[] battleData = data[1].Split(new string[] { "|" }, StringSplitOptions.None);
            string[] battleMessages = battleData[4].Split(new string[] { "\r\n" }, StringSplitOptions.None);

            foreach (string message in battleMessages)
            {
                if (!ActiveBattle.ProcessMessage(Team, message))
                {
                    BattleMessage?.Invoke(I18n.Replace(message));
                }
            }

            PokemonsUpdated?.Invoke();

            if (ActiveBattle.IsFinished)
            {
                _battleTimeout.Set(Rand.Next(1000, 3000));
            }
            else
            {
                _battleTimeout.Set(Rand.Next(2000, 4000));
            }

            if (ActiveBattle.IsFinished)
            {
                IsInBattle = false;
                ActiveBattle = null;
                BattleEnded?.Invoke();
            }
        }

        private void OnScript(string[] data)
        {
            string id = data[2];
            int status = Convert.ToInt32(data[1]);
            string script = data[3];

            DialogContent = script.Split(new string[] { "-#-" }, StringSplitOptions.None);
            if (script.Contains("-#-") && status > 1)
            {
                script = DialogContent[0];
            }
            string[] messages = script.Split(new string[] { "-=-" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string message in messages)
            {
                if (message.StartsWith("emote") || message.StartsWith("playsound") || message.StartsWith("playmusic") || message.StartsWith("playcry"))
                    continue;
                if (message.StartsWith("shop"))
                {
                    OpenedShop = new Shop(message.Substring(4));
                    ShopOpened?.Invoke(OpenedShop);
                    continue;
                }
                DialogOpened?.Invoke(message);
            }

            IsScriptActive = true;
            _dialogTimeout.Set(Rand.Next(1500, 4000));
            ScriptId = id;
            ScriptStatus = status;
        }

        private void OnBikingUpdate(string[] data)
        {
            if (data[1] == "1")
            {
                IsBiking = true;
                IsSurfing = false;
            }
            else
            {
                IsBiking = false;
            }
            _mountingTimeout.Set(Rand.Next(500, 1000));
            _itemUseTimeout.Cancel();
        }

        private void OnSurfingUpdate(string[] data)
        {
            if (data[1] == "1")
            {
                IsSurfing = true;
                IsBiking = false;
            }
            else
            {
                IsSurfing = false;
            }
            _mountingTimeout.Set(Rand.Next(500, 1000));
            _itemUseTimeout.Cancel();
        }

        private void OnLearningMove(string[] data)
        {
            int moveId = Convert.ToInt32(data[1]);
            string moveName = Convert.ToString(data[2]);
            int pokemonUid = Convert.ToInt32(data[3]);
            int movePp = Convert.ToInt32(data[4]);
            LearningMove?.Invoke(moveId, moveName, pokemonUid);
            _itemUseTimeout.Cancel();
            // ^|.|348|.|Cut|.|3|.|30|.\
        }

        private void OnEvolving(string[] data)
        {
            int evolvingPokemonUid = Convert.ToInt32(data[1]);
            int evolvingItem = Convert.ToInt32(data[3]);


            Evolving.Invoke(evolvingPokemonUid, evolvingItem);
        }

        private void OnUpdatePlayer(string[] data)
        {
            string[] updateData = data[1].Split('|');

            bool isNewPlayer = false;
            PlayerInfos player;
            DateTime expiration = DateTime.UtcNow.AddSeconds(20);
            if (Players.ContainsKey(updateData[0]))
            {
                player = Players[updateData[0]];
                player.Expiration = expiration;
            }
            else
            {
                isNewPlayer = true;
                player = new PlayerInfos(expiration);
                player.Name = updateData[0];
            }

            player.Updated = DateTime.UtcNow;
            player.PosX = Convert.ToInt32(updateData[1]);
            player.PosY = Convert.ToInt32(updateData[2]);
            player.Direction = updateData[3][0];
            player.Skin = updateData[3].Substring(1);
            player.IsAfk = updateData[4][0] != '0';
            player.IsInBattle = updateData[4][1] != '0';
            player.PokemonPetId = Convert.ToInt32(updateData[4].Substring(2));
            player.IsPokemonPetShiny = updateData[5][0] != '0';
            player.IsMember = updateData[5][1] != '0';
            player.IsOnground = updateData[5][2] != '0';
            player.GuildId = Convert.ToInt32(updateData[5].Substring(3));
            player.PetForm = Convert.ToInt32(updateData[6]); // ???

            Players[player.Name] = player;

            if (isNewPlayer)
            {
                PlayerAdded?.Invoke(player);
            }
            else
            {
                PlayerUpdated?.Invoke(player);
            }
        }

        private void OnChannels(string[] data)
        {
            Channels.Clear();
            string[] channelsData = data[1].Split('|');
            for (int i = 1; i < channelsData.Length; i += 2)
            {
                string channelId = channelsData[i];
                string channelName = channelsData[i + 1];
                Channels.Add(new ChatChannel(channelId, channelName));
            }
            RefreshChannelList?.Invoke();
        }

        private void OnChatMessage(string[] data)
        {
            string fullMessage = data[1];
            string[] chatData = fullMessage.Split(':');

            if (fullMessage[0] == '*' && fullMessage[2] == '*')
            {
                fullMessage = fullMessage.Substring(3);
            }

            string message;
            if (chatData.Length <= 1) // we are not really sure what this stands for
            {
                string channelName;

                int start = fullMessage.IndexOf('(') + 1;
                int end = fullMessage.IndexOf(')');
                if (fullMessage.Length <= end || start == 0 || end == -1)
                {
                    string packet = string.Join("|.|", data);
                    InvalidPacket?.Invoke(packet, "Channel System Message with invalid channel");
                    channelName = "";
                }
                else
                {
                    channelName = fullMessage.Substring(start, end - start);
                }

                if (fullMessage.Length <= end + 2 || start == 0 || end == -1)
                {
                    string packet = string.Join("|.|", data);
                    InvalidPacket?.Invoke(packet, "Channel System Message with invalid message");
                    message = "";
                }
                else
                {
                    message = fullMessage.Substring(end + 2);
                }

                ChannelSystemMessage?.Invoke(channelName, message);
                return;
            }
            if (chatData[0] != "*G*System")
            {
                string channelName = null;
                string mode = null;
                string author;

                int start = (fullMessage[0] == '(' ? 1 : 0);
                int end;
                if (start != 0)
                {
                    end = fullMessage.IndexOf(')');
                    if (end != -1 && end - start > 0)
                    {
                        channelName = fullMessage.Substring(start, end - start);
                    }
                    else
                    {
                        string packet = string.Join("|.|", data);
                        InvalidPacket?.Invoke(packet, "Channel Message with invalid channel name");
                        channelName = "";
                    }
                }
                start = fullMessage.IndexOf('[') + 1;
                if (start != 0 && fullMessage[start] != 'n')
                {
                    end = fullMessage.IndexOf(']');
                    if (end == -1)
                    {
                        string packet = string.Join("|.|", data);
                        InvalidPacket?.Invoke(packet, "Message with invalid mode");
                        message = "";
                    }
                    mode = fullMessage.Substring(start, end - start);
                }
                string conversation = null;
                if (channelName == "PM")
                {
                    end = fullMessage.IndexOf(':');
                    string header = "";
                    if (end == -1)
                    {
                        string packet = string.Join("|.|", data);
                        InvalidPacket?.Invoke(packet, "Channel Private Message with invalid author");
                        conversation = "";
                    }
                    else
                    {
                        header = fullMessage.Substring(0, end);
                        start = header.LastIndexOf(' ') + 1;
                        if (end == -1)
                        {
                            string packet = string.Join("|.|", data);
                            InvalidPacket?.Invoke(packet, "Channel Private Message with invalid author");
                            conversation = "";
                        }
                        else
                        {
                            conversation = header.Substring(start);
                        }
                    }
                    if (header.Contains(" to "))
                    {
                        author = PlayerName;
                    }
                    else
                    {
                        author = conversation;
                    }
                }
                else
                {
                    start = fullMessage.IndexOf("[n=") + 3;
                    end = fullMessage.IndexOf("][/n]:");
                    if (end == -1)
                    {
                        string packet = string.Join("|.|", data);
                        InvalidPacket?.Invoke(packet, "Message with invalid author");
                        author = "";
                    }
                    else
                    {
                        author = fullMessage.Substring(start, end - start);
                    }
                }
                start = fullMessage.IndexOf(':') + 2;
                if (end == -1)
                {
                    string packet = string.Join("|.|", data);
                    InvalidPacket?.Invoke(packet, "Channel Private Message with invalid message");
                    message = "";
                }
                else
                {
                    message = fullMessage.Substring(start == 1 ? 0 : start);
                }
                if (channelName != null)
                {
                    if (channelName == "PM")
                    {
                        ChannelPrivateMessage?.Invoke(conversation, mode, author, message);
                    }
                    else
                    {
                        ChannelMessage?.Invoke(channelName, mode, author, message);
                    }
                }
                else
                {
                    if (message.IndexOf("em(") == 0)
                    {
                        end = message.IndexOf(")");
                        int emoteId;
                        if (end != -1 && end - 3 > 0)
                        {
                            string emoteIdString = message.Substring(3, end - 3);
                            if (int.TryParse(emoteIdString, out emoteId) && emoteId > 0)
                            {
                                EmoteMessage?.Invoke(mode, author, emoteId);
                                return;
                            }
                        }
                    }
                    ChatMessage?.Invoke(mode, author, message);
                }
                return;
            }

            int offset = fullMessage.IndexOf(':') + 2;
            if (offset == -1 + 2) // for clarity... I prefectly know it's -3
            {
                string packet = string.Join("|.|", data);
                InvalidPacket?.Invoke(packet, "Channel Private Message with invalid author");
                message = "";
            }
            else
            {
                message = fullMessage.Substring(offset == 1 ? 0 : offset);
            }

            if (message.Contains("$YouUse the ") && message.Contains("Rod!"))
            {
                _itemUseTimeout.Cancel();
                _fishingTimeout.Set(2500 + Rand.Next(500, 1500));
            }

            SystemMessage?.Invoke(I18n.Replace(message));
        }

        private void OnPrivateMessage(string[] data)
        {
            if (data.Length < 2)
            {
                string packet = string.Join("|.|", data);
                InvalidPacket?.Invoke(packet, "PM with no parameter");
            }
            string[] nicknames = data[1].Split(new[] { "-=-" }, StringSplitOptions.None);
            if (nicknames.Length < 2)
            {
                string packet = string.Join("|.|", data);
                InvalidPacket?.Invoke(packet, "PM with invalid header");
                return;
            }

            string conversation;
            if (nicknames[0] != PlayerName)
            {
                conversation = nicknames[0];
            }
            else
            {
                conversation = nicknames[1];
            }

            if (data.Length < 3)
            {
                string packet = string.Join("|.|", data);
                InvalidPacket?.Invoke(packet, "PM without a message");
                /*
                 * the PM is sent since the packet is still understandable
                 * however, PRO client does not allow it
                 */
                PrivateMessage?.Invoke(conversation, null, conversation + " (deduced)", "");
                return;
            }

            string mode = null;
            int offset = data[2].IndexOf('[') + 1;
            int end = 0;
            if (offset != 0 && offset < data[2].IndexOf(':'))
            {
                end = data[2].IndexOf(']');
                mode = data[2].Substring(offset, end - offset);
            }

            if (data[2].Substring(0, 4) == "rem:")
            {
                LeavePrivateMessage?.Invoke(conversation, mode, data[2].Substring(4 + end));
                return;
            }
            else if (!Conversations.Contains(conversation))
            {
                Conversations.Add(conversation);
            }

            string modeRemoved = data[2];
            if (end != 0)
            {
                modeRemoved = data[2].Substring(end + 2);
            }
            offset = modeRemoved.IndexOf(' ');
            string speaker = modeRemoved.Substring(0, offset);

            offset = data[2].IndexOf(':') + 2;
            string message = data[2].Substring(offset);

            PrivateMessage?.Invoke(conversation, mode, speaker, message);
        }

        public int GetBoxIdFromPokemonUid(int lastUid)
        {
            return (lastUid - 7) / 15 + 1;
        }

        private void OnPCBox(string[] data)
        {
            _refreshingPCBox.Cancel();
            IsPCBoxRefreshing = false;
            if (Map.IsPC(PlayerX, PlayerY - 1))
            {
                IsPCOpen = true;
            }
            string[] body = data[1].Split('=');
            if (body.Length < 3)
            {
                InvalidPacket?.Invoke(data[0] + "|.|" + data[1], "Received an invalid PC Box packet");
                return;
            }
            PCGreatestUid = Convert.ToInt32(body[0]);

            int pokemonCount = Convert.ToInt32(body[1]);
            if (pokemonCount <= 0 || pokemonCount > 15)
            {
                InvalidPacket?.Invoke(data[0] + "|.|" + data[1], "Received an invalid PC Box size");
                return;
            }
            string[] pokemonListDatas = body[2].Split(new[] { "\r\n" }, StringSplitOptions.None);
            if (body.Length < 1)
            {
                InvalidPacket?.Invoke(data[0] + "|.|" + data[1], "Received an empty box");
                return;
            }
            List<Pokemon> pokemonBox = new List<Pokemon>();
            foreach (var pokemonDatas in pokemonListDatas)
            {
                if (pokemonDatas == string.Empty) continue;
                string[] pokemonDatasArray = pokemonDatas.Split('|');
                Pokemon pokemon = new Pokemon(pokemonDatasArray);
                if (CurrentPCBoxId != GetBoxIdFromPokemonUid(pokemon.Uid))
                {
                    InvalidPacket?.Invoke(data[0] + "|.|" + data[1], "Received a box packet for an unexpected box: expected #"
                        + CurrentPCBox + ", received #" + GetBoxIdFromPokemonUid(pokemon.Uid));
                    return;
                }
                pokemonBox.Add(pokemon);
            }
            if (pokemonBox.Count != pokemonCount)
            {
                InvalidPacket?.Invoke(data[0] + "|.|" + data[1], "Received a PC Box size that does not match the content");
                return;
            }
            CurrentPCBox = pokemonBox;
            PCBoxUpdated?.Invoke(CurrentPCBox);
        }

        private void LoadMap(string mapName)
        {
            mapName = MapClient.RemoveExtension(mapName);

            _loadingTimeout.Set(Rand.Next(1500, 4000));

            OpenedShop = null;
            _movements.Clear();
            _surfAfterMovement = false;
            _slidingDirection = null;
            _dialogResponses.Clear();
            _movementTimeout.Cancel();
            _mountingTimeout.Cancel();
            _itemUseTimeout.Cancel();

            if (Map == null || MapName != mapName)
            {
                DownloadMap(mapName);
            }
        }

        private void DownloadMap(string mapName)
        {
            Console.WriteLine("[Map] Requesting: " + MapName);

            Map = null;
            AreNpcReceived = false;
            MapName = mapName;
            _mapClient.DownloadMap(MapName);
            Players.Clear();
        }
    }
}
