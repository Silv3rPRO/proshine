//#define DEBUG_TRAINER_BATTLES

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace PROProtocol
{
    public class GameClient
    {
        private const string Version = "Sinnoh";
        private readonly Timeout _battleTimeout = new Timeout();

        private readonly GameConnection _connection;
        private readonly Queue<object> _dialogResponses = new Queue<object>();
        private readonly Timeout _dialogTimeout = new Timeout();
        private readonly Timeout _fishingTimeout = new Timeout();

        /// <summary>
        ///     A dictionary containing a point as key and it's guarding battler as
        ///     value. Building the dictionary like that is a little less readable
        ///     and takes a bit more memory, but allows an access speed of O(1)
        ///     instead of O(n). Which is the case, when stored in a list. Since
        ///     fields need to be tested after every movement, access speed was
        ///     prioritized.
        /// </summary>
        /// <attention>
        ///     Access via getGuardedFields(). Never call it directly.
        /// </attention>
        private Dictionary<Point, Npc> _guardedFields;

        private readonly Timeout _itemUseTimeout = new Timeout();
        private DateTime _lastMovement;
        private readonly Timeout _loadingTimeout = new Timeout();

        private readonly MapClient _mapClient;
        private readonly Timeout _mountingTimeout = new Timeout();
        private readonly List<Direction> _movements = new List<Direction>();

        private readonly Timeout _movementTimeout = new Timeout();
        private DateTime _refreshBoxTimeout;
        private readonly Timeout _refreshingPcBox = new Timeout();
        private Direction? _slidingDirection;
        private bool _surfAfterMovement;
        private readonly Timeout _swapTimeout = new Timeout();
        private readonly Timeout _teleportationTimeout = new Timeout();
        private DateTime _updatePlayers;

        /// <summary>
        ///     A list of valid movement operations that don't
        ///     <see langword="break" /> line of vision.
        /// </summary>
        /// <attention>
        ///     Only accessed by getGuardedFields() and shouldn't otherwise.
        /// </attention>
        private HashSet<Map.MoveResult> _validMovementResults;

        public List<string> GetAreaName = new List<string>();

        public List<string> IsMs = new List<string>();

        public List<string> TimeZone = new List<string>();

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
            I18N = new Language();
            Team = new List<Pokemon>();
            CurrentPcBox = new List<Pokemon>();
            Items = new List<InventoryItem>();
            Channels = new List<ChatChannel>();
            Conversations = new List<string>();
            Players = new Dictionary<string, PlayerInfos>();
            PcGreatestUid = -1;
            IsPrivateMessageOn = true;

            IsTeamInspectionEnabled = true;

            IsTrainerBattlesActive = true;
            FirstTrade = new List<TradePokemon>();
            SecondTrade = new List<TradePokemon>();
            SpawnList = new List<PokemonSpawn>();
            AnotherMapSpawnList = new List<PokemonSpawn>();
            CreatingCharacter = false;
            CreatingCharacterFemale = false;
            CreatingCharacterMale = false;
            IsCaughtOn = false;
            IsSpawnListThingStarted = false;
            ScriptStarted = false;
            IsCrrentMap = true;
            IsRequestingForCurrentSpawnCheck = false;
            PokedexList = new List<PokedexPokemon>();
        }

        public Random Rand { get; }
        public Language I18N { get; }

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
        public List<PokemonSpawn> SpawnList { get; private set; }
        public List<PokemonSpawn> AnotherMapSpawnList { get; }
        public List<PokedexPokemon> PokedexList { get; }

        public bool IsInBattle { get; private set; }
        public bool IsSurfing { get; private set; }
        public bool IsBiking { get; private set; }
        public bool IsOnGround { get; private set; }
        public bool IsPcOpen { get; private set; }
        public bool CanUseCut { get; private set; }
        public bool CanUseSmashRock { get; private set; }
        public bool IsPrivateMessageOn { get; private set; }
        public bool IsTrainerBattlesActive { get; set; }

        public bool IsTeamInspectionEnabled { get; private set; }

        public int Money { get; private set; }
        public int Coins { get; private set; }
        public bool IsMember { get; private set; }
        public List<Pokemon> Team { get; }
        public List<Pokemon> CurrentPcBox { get; private set; }
        public List<InventoryItem> Items { get; }
        public string PokemonTime { get; private set; }
        public string Weather { get; private set; }
        public int PcGreatestUid { get; private set; }

        public bool IsScriptActive { get; private set; }
        public bool IsCrrentMap { get; set; }
        public string ScriptId { get; private set; }
        public bool CreatingCharacterMale { get; set; }
        public bool CreatingCharacterFemale { get; set; }
        public bool CreatingCharacter { get; set; }
        public int ScriptStatus { get; private set; }
        public string[] DialogContent { get; private set; }

        public Battle ActiveBattle { get; private set; }
        public Shop OpenedShop { get; private set; }

        public List<ChatChannel> Channels { get; }
        public List<string> Conversations { get; }
        public Dictionary<string, PlayerInfos> Players { get; }
        public bool IsPcBoxRefreshing { get; private set; }
        public bool IsInNpcBattle { get; set; }
        public int CurrentPcBoxId { get; private set; }

        public bool IsCaughtOn { get; set; }
        public bool IsSpawnListThingStarted { get; set; }
        public string SpawnMapName { get; set; }
        public bool ScriptStarted { get; set; }
        public bool IsRequestingForSpawnCheck { get; set; }
        public bool IsRequestingForCurrentSpawnCheck { get; set; }

        public bool IsInactive => _movements.Count == 0
                                  && !_movementTimeout.IsActive
                                  && !_battleTimeout.IsActive
                                  && !_loadingTimeout.IsActive
                                  && !_mountingTimeout.IsActive
                                  && !_teleportationTimeout.IsActive
                                  && !_dialogTimeout.IsActive
                                  && !_swapTimeout.IsActive
                                  && !_itemUseTimeout.IsActive
                                  && !_fishingTimeout.IsActive
                                  && !_refreshingPcBox.IsActive;

        public bool IsTeleporting => _teleportationTimeout.IsActive;

        public GameServer Server => _connection.Server;

        public bool IsMapLoaded => Map != null;

        public bool AreNpcReceived { get; private set; }

        public event Action ConnectionOpened;

        public event Action<Exception> ConnectionFailed;

        public event Action<Exception> ConnectionClosed;

        public event Action LoggedIn;

        public event Action<AuthenticationResult> AuthenticationFailed;

        public event Action<int> QueueUpdated;

        public event Action<string, int, int> PositionUpdated;

        public event Action<string, int, int> TeleportationOccuring;

        public event Action<string> MapLoaded;

        public event Action<List<Npc>> NpcReceived;

        public event Action PokemonsUpdated;

        public event Action InventoryUpdated;

        public event Action<List<PokemonSpawn>> SpawnListUpdated;

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

        public event Action<List<Pokemon>> PcBoxUpdated;

        public event Action CreatingCharacterAction;

        public event Action PokedexDataUpdated;

        public void ClearPath()
        {
            _movements.Clear();
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
            _refreshingPcBox.Update();

            SendRegularPing();
            UpdateMovement();
            UpdateScript();
            UpdatePlayers();
            UpdatePcBox();
        }

        public void CloseChannel(string channelName)
        {
            if (Channels.Any(e => e.Name == channelName))
                SendMessage("/cgleave " + channelName);
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
                var direction = _movements[0];
                _movements.RemoveAt(0);

                if (ApplyMovement(direction))
                {
                    SendMovement(direction.AsChar());
                    _movementTimeout.Set(IsBiking ? 125 : 250);
                    if (Map.HasLink(PlayerX, PlayerY))
                        _teleportationTimeout.Set();
                }

                if (_movements.Count == 0 && _surfAfterMovement)
                    _movementTimeout.Set(Rand.Next(750, 2000));
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
                foreach (var playerName in Players.Keys.ToArray())
                    if (Players[playerName].IsExpired())
                    {
                        PlayerRemoved?.Invoke(Players[playerName]);
                        Players.Remove(playerName);
                    }
                _updatePlayers = DateTime.UtcNow.AddSeconds(5);
            }
        }

        private void UpdatePcBox()
        {
            // if we did not receive an answer, then the box is empty
            if (IsPcBoxRefreshing && _refreshBoxTimeout > DateTime.UtcNow)
            {
                IsPcBoxRefreshing = false;
                if (Map.IsPc(PlayerX, PlayerY - 1))
                    IsPcOpen = true;
                CurrentPcBox = new List<Pokemon>();
                PcBoxUpdated?.Invoke(CurrentPcBox);
            }
        }

        private Dictionary<Point, Npc> GetGuardedFields()
        {
#if DEBUG && DEBUG_TRAINER_BATTLES
            Console.WriteLine("Trainer Battles | guarded fields | access");
#endif

            //if initiated, then return it
            if (_guardedFields != null)
                return _guardedFields;

#if DEBUG && DEBUG_TRAINER_BATTLES
            Console.WriteLine("Trainer Battles | guarded fields | init");
            Console.WriteLine("Trainer Battles | map ncp | count: " + Map.Npcs.Count);
#endif

            //initiate guardedFields
            _guardedFields = new Dictionary<Point, Npc>();

            //iterating all battlers
            foreach (var battler in Map.Npcs.Where(npc => npc.CanBattle))
            {
#if DEBUG && DEBUG_TRAINER_BATTLES
                Console.WriteLine("Trainer Battles | map ncp | "+battler.Id+" ("+battler.Name+") | guarded field count: " + getGuardedFields(battler).Count);
#endif
                //iterate all points those battlers have in vision
                foreach (var guardedField in GetGuardedFields(battler))
                {
#if DEBUG && DEBUG_TRAINER_BATTLES
                    if (_guardedFields.ContainsKey(guardedField))
                        Console.WriteLine("Trainer Battles | guarded fields | conflict: " + guardedField.ToString());
#endif
                    //fill dictionary
                    _guardedFields.Add(guardedField, battler);
                }
            }

            return _guardedFields;
        }

        private List<Point> GetGuardedFields(Npc battler)
        {
            //init on first usage
            if (_validMovementResults == null)
            {
                _validMovementResults = new HashSet<Map.MoveResult>();
                _validMovementResults.Add(Map.MoveResult.Success); //standard behaviour, free vision
                _validMovementResults.Add(Map.MoveResult
                    .Sliding); //when sliding on ice, you can still be stopped for battle
            }

            //access declaration for future easy manipulation
            var viewDirection = battler.ViewDirection;
            var viewRange = battler.LosLength;

            //the list of guarded fields in one direction
            var guardedFields = new List<Point>();

            //making copy, because reference would modify battlers position
            //all elements in list would probably point to same Point obj at that time
            var battlerPos = new Point(battler.PositionX, battler.PositionY);
            var checkingPos = new Point(battler.PositionX, battler.PositionY);

            //references, will be modified by Map.CanMove()
            var isOnGround = IsOnGround;
            var isSurfing = IsSurfing;

            //algorithm vars, initiated to fail if not changed
            var result = Map.MoveResult.Fail;
            var isBlocked = true;
            var isInViewRange = false;

            while (true)
            {
                //move into direction
                checkingPos = viewDirection.ApplyToCoordinates(checkingPos);

                //calculate move results
                result = Map.CanMove(viewDirection, checkingPos.X, checkingPos.Y, isOnGround, isSurfing, CanUseCut,
                    CanUseSmashRock);
                isBlocked = !_validMovementResults.Contains(result);
                isInViewRange = GetManhattanDistance(battlerPos, checkingPos) <= viewRange;

                //adding to guarded fields, if conditions are set
                if (!isBlocked && isInViewRange)
                    guardedFields.Add(checkingPos);

                //leave loop otherwise
                else
                    break;
            }

            return guardedFields;
        }

        /// <summary>
        ///     In short: the method summarizes the axial differences. Google for
        ///     Manhatten Distance or see
        ///     https://en.wikipedia.org/wiki/Taxicab_geometry for more information.
        /// </summary>
        /// <remarks>
        ///     Couldn't find available libs for this standard function, so I still
        ///     needed to implement it.
        /// </remarks>
        /// <param name="p1">Start point of the distance calculation.</param>
        /// <param name="p2">End point of the distance calculation.</param>
        /// <returns>
        ///     The manhatten distance between two points.
        /// </returns>
        private static int GetManhattanDistance(Point p1, Point p2)
        {
            return Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y);
        }

        private bool ApplyMovement(Direction direction)
        {
            //init vars
            var destinationX = PlayerX;
            var destinationY = PlayerY;
            var isOnGround = IsOnGround;
            var isSurfing = IsSurfing;

            direction.ApplyToCoordinates(ref destinationX, ref destinationY);
            var playerPos = new Point(PlayerX, PlayerY);

#if DEBUG && DEBUG_TRAINER_BATTLES
            Console.WriteLine("Trainer Battles | IsTrainerBattlesActive | " + IsTrainerBattlesActive);
            Console.WriteLine("Trainer Battles | guarded fields | count: " + getGuardedFields().Count);
            Console.WriteLine("Trainer Battles | guarded fields | contains playerpos: " + getGuardedFields().ContainsKey(playerPos));
#endif
            //--------analyze current position
            if (IsTrainerBattlesActive && GetGuardedFields().TryGetValue(playerPos, out var battler))
            {
                //stop bot movement
                _movements.Clear();

                //TODO: if wanted battler movement could be triggered here

                //start battle
                TalkToNpc(battler.Id);

                //while sending, remove listed guarding fields
                //ATTENTION: don't know how to handle interrupts here
                foreach (var guarded in GetGuardedFields(battler))
                    GetGuardedFields().Remove(guarded);

                //no further movement therefore return
                return false;
            }

            //--------analyze next movement
            var result = Map.CanMove(direction, destinationX, destinationY, isOnGround, isSurfing, CanUseCut,
                CanUseSmashRock);
            if (Map.ApplyMovement(direction, result, ref destinationX, ref destinationY, ref isOnGround, ref isSurfing))
            {
                PlayerX = destinationX;
                PlayerY = destinationY;
                IsOnGround = isOnGround;
                IsSurfing = isSurfing;
                PositionUpdated?.Invoke(MapName, PlayerX, PlayerY);

                if (result == Map.MoveResult.Icing)
                    _movements.Insert(0, direction);

                if (result == Map.MoveResult.Sliding)
                {
                    var slider = Map.GetSlider(destinationX, destinationY);
                    if (slider != -1)
                        _slidingDirection = Map.SliderToDirection(slider);
                }

                if (_slidingDirection != null)
                    _movements.Insert(0, _slidingDirection.Value);

                return true;
            }

            _slidingDirection = null;
            return false;
        }

        private void UpdateScript()
        {
            if (IsScriptActive && !_dialogTimeout.IsActive)
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

        private int GetNextDialogResponse()
        {
            if (_dialogResponses.Count > 0)
            {
                var response = _dialogResponses.Dequeue();
                if (response is int)
                    return (int)response;
                if (response is string)
                {
                    var text = ((string)response).ToUpperInvariant();
                    for (var i = 1; i < DialogContent.Length; ++i)
                        if (DialogContent[i].ToUpperInvariant().Equals(text))
                            return i;
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
            var pmHeader = "/pm " + PlayerName + "-=-" + nickname;
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
            var toSend = "*|.|" + id;
            if (pokemon != 0)
                toSend += "|.|" + pokemon;
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

        private void SendMovePokemonToPc(int pokemonUid)
        {
            SendPacket("?|.|" + pokemonUid + "|.|-1");
        }

        // if there is a pokemon in teamSlot, it will be swapped
        private void SendMovePokemonFromPc(int pokemonUid, int teamSlot)
        {
            SendPacket("?|.|" + pokemonUid + "|.|" + teamSlot);
        }

        private void SendRefreshPcBox(int box, string search)
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

        public void SendSpawnPacket()
        {
            SendPacket("k|.|" + SpawnMapName.ToLowerInvariant());
        }

        public void SendCurrentMapSpawnPacket()
        {
            SendPacket("k|.|" + MapName.ToLowerInvariant());
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

        public bool ReleasePokemonFromPc(int boxId, int boxPokemonId)
        {
            if (!IsPcOpen || IsPcBoxRefreshing || boxId < 1 || boxId > 67
                || boxPokemonId < 1 || boxPokemonId > 15 || boxPokemonId > CurrentPcBox.Count)
                return false;
            var pokemonUid = GetPokemonPcUid(boxId, boxPokemonId);
            if (pokemonUid == -1 || pokemonUid != CurrentPcBox[boxPokemonId - 1].Uid)
                return false;
            _refreshingPcBox.Set(Rand.Next(1500, 2000));
            SendReleasePokemon(pokemonUid);
            return true;
        }

        public bool ReleasePokemonFromTeam(int pokemonUid)
        {
            if (!IsPcOpen || IsPcBoxRefreshing
                || pokemonUid < 1 || pokemonUid > 6 || pokemonUid > Team.Count)
                return false;
            _refreshingPcBox.Set(Rand.Next(1500, 2000));
            SendReleasePokemon(pokemonUid);
            return true;
        }

        public bool RefreshPcBox(int boxId)
        {
            if (!IsPcOpen || boxId < 1 || boxId > 67 || _refreshingPcBox.IsActive || IsPcBoxRefreshing)
                return false;
            _refreshingPcBox.Set(Rand.Next(1500, 2000)); // this is the amount of time we wait for an answer
            CurrentPcBoxId = boxId;
            IsPcBoxRefreshing = true;
            CurrentPcBox = null;
            _refreshBoxTimeout = DateTime.UtcNow.AddSeconds(5); // this is to avoid a flood of the function
            SendRefreshPcBox(boxId - 1, "ID");
            return true;
        }

        public bool RefreshCurrentPcBox()
        {
            return RefreshPcBox(CurrentPcBoxId);
        }

        private int GetPokemonPcUid(int box, int id)
        {
            if (box < 1 || box > 67 || id < 1 || id > 15)
                return -1;
            var result = (box - 1) * 15 + 6 + id;
            // ensures we cannot access a pokemon we do not have or know
            if (result > PcGreatestUid || CurrentPcBox == null || box != CurrentPcBoxId)
                return -1;
            return result;
        }

        public bool DepositPokemonToPc(int pokemonUid)
        {
            if (!IsPcOpen || pokemonUid < 1 || pokemonUid > 6 || Team.Count < pokemonUid)
                return false;
            SendMovePokemonToPc(pokemonUid);
            return true;
        }

        public bool WithdrawPokemonFromPc(int boxId, int boxPokemonId)
        {
            var pcPokemonUid = GetPokemonPcUid(boxId, boxPokemonId);
            if (pcPokemonUid == -1)
                return false;
            if (!IsPcOpen || pcPokemonUid < 7 || pcPokemonUid > PcGreatestUid || Team.Count >= 6)
                return false;
            SendMovePokemonFromPc(pcPokemonUid, Team.Count + 1);
            return true;
        }

        public bool SwapPokemonFromPc(int boxId, int boxPokemonId, int teamPokemonUid)
        {
            var pcPokemonUid = GetPokemonPcUid(boxId, boxPokemonId);
            if (pcPokemonUid == -1)
                return false;
            if (!IsPcOpen || pcPokemonUid < 7 || pcPokemonUid > PcGreatestUid
                || teamPokemonUid < 1 || teamPokemonUid > 6 || Team.Count < teamPokemonUid)
                return false;
            SendMovePokemonFromPc(pcPokemonUid, teamPokemonUid);
            return true;
        }

        public bool SwapPokemon(int pokemon1, int pokemon2)
        {
            if (IsInBattle || pokemon1 < 1 || pokemon2 < 1 || Team.Count < pokemon1 || Team.Count < pokemon2 ||
                pokemon1 == pokemon2)
                return false;
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
                return;
            var item = GetItemFromId(id);
            if (item == null || item.Quantity == 0)
                return;
            if (pokemonUid == 0) // simple use
            {
                if (!_itemUseTimeout.IsActive && !IsInBattle &&
                    (item.Scope == 8 || item.Scope == 10 || item.Scope == 15))
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
                return false;
            var item = GetItemFromId(itemId);
            if (item == null || item.Quantity == 0)
                return false;
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
                return false;
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
                    Map.Region == "3" && HasItemName("Balance Badge") ||
                    Map.Region == "4" && HasItemName("Relic Badge"));
        }

        public bool HasCutAbility()
        {
            return (HasMove("Cut") || HasTreeaxe()) &&
                   (Map.Region == "1" && HasItemName("Cascade Badge") ||
                    Map.Region == "2" && HasItemName("Hive Badge") ||
                    Map.Region == "3" && HasItemName("Stone Badge") ||
                    Map.Region == "4" && HasItemName("Coal Badge"));
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
            return Team.FirstOrDefault(p => p.Uid == pokemonUid)?.Moves.Any(m =>
                       m.Name?.Equals(moveName, StringComparison.InvariantCultureIgnoreCase) ?? false) ?? false;
        }

        public bool HasMove(string moveName)
        {
            return Team.Any(p =>
                p.Moves.Any(m => m.Name?.Equals(moveName, StringComparison.InvariantCultureIgnoreCase) ?? false));
        }

        public int GetMovePosition(int pokemonUid, string moveName)
        {
            return Team[pokemonUid].Moves
                       .FirstOrDefault(m =>
                           m.Name?.Equals(moveName, StringComparison.InvariantCultureIgnoreCase) ?? false)?.Position ??
                   -1;
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
            return Items.FirstOrDefault(i =>
                i.Name.Equals(itemName, StringComparison.InvariantCultureIgnoreCase) && i.Quantity > 0);
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

        public bool OpenPc()
        {
            if (!Map.IsPc(PlayerX, PlayerY - 1))
                return false;
            IsPcOpen = true;
            return RefreshPcBox(1);
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

                MapLoaded?.Invoke(MapName);
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
            IsPcOpen = false;
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
                packet = "U|.|" + packet.Substring(1);

            var data = packet.Split(new[] { "|.|" }, StringSplitOptions.None);
            var type = data[0].ToLowerInvariant();
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
                    OnPcBox(data);
                    break;

                case "k":
                    IsSpawnListThingStarted = false;
                    if (!IsCrrentMap && IsRequestingForSpawnCheck && !IsRequestingForCurrentSpawnCheck)
                        AnotherMapLoadPokemons(data);
                    LoadSpawnMenu(data);
                    break;

                case "p":
                    HandlePokedexMsg(data);
                    break;

                #region Trade actions

                case "mb":
                    // Actions : Trades requests, Friends requests..
                    HandleActions(data);
                    break;

                case "t":
                    // Trade Start
                    HandleTrade(data);
                    break;

                case "tu":
                    OnTradeUpdate(data);
                    break;

                case "ta":
                    UselessTradeFeature(); // Send a "change to final screen" order to client. Useless.
                    break;

                case "tb":
                    OnTradeStatusChange(data);
                    break;

                case "tc":
                    OnTradeStatusReset();
                    break;

                #endregion Trade actions

                default:
#if DEBUG
                    Console.WriteLine(" ^ unhandled /!\\");
#endif
                    break;
            }
        }

        public bool DisableTeamInspection()
        {
            IsTeamInspectionEnabled = false;
            SendMessage("/in0");
            return true;
        }

        public bool EnableTeamInspection()
        {
            IsTeamInspectionEnabled = true;
            SendMessage("/in1");
            return true;
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
            AskForPokedex();
            Console.WriteLine("[Login] Authenticated successfully");
            LoggedIn?.Invoke();
        }

        private void OnAuthenticationResult(string[] data)
        {
            var result = (AuthenticationResult)Convert.ToInt32(data[1]);

            if (result != AuthenticationResult.ServerFull)
            {
                AuthenticationFailed?.Invoke(result);
                Close();
            }
        }

        private void OnQueueUpdated(string[] data)
        {
            var queueData = data[1].Split('|');

            var position = Convert.ToInt32(queueData[0]);
            QueueUpdated?.Invoke(position);
        }

        private void OnPlayerPosition(string[] data)
        {
            var mapData = data[1].Split(new[] { "|" }, StringSplitOptions.None);
            var map = mapData[0];
            var playerX = Convert.ToInt32(mapData[1]);
            var playerY = Convert.ToInt32(mapData[2]);
            if (playerX != PlayerX || playerY != PlayerY || map != MapName)
            {
                TeleportationOccuring?.Invoke(map, playerX, playerY);

                PlayerX = playerX;
                PlayerY = playerY;
                LoadMap(map);
                IsOnGround = mapData[3] == "1";
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
            var mapData = data[1].Split(new[] { "|" }, StringSplitOptions.None);

            if (mapData.Length < 2)
                return;

            var map = mapData[0];
            var playerX = Convert.ToInt32(mapData[1]);
            var playerY = Convert.ToInt32(mapData[2]);
            if (map.Length > 1)
            {
                PlayerX = playerX;
                PlayerY = playerY;
                LoadMap(map);
            }
            IsOnGround = mapData[3] == "1";

            PositionUpdated?.Invoke(MapName, PlayerX, playerY);
        }

        private void OnPlayerInfos(string[] data)
        {
            var playerData = data[1].Split('|');
            PlayerName = playerData[0];
            PokedexOwned = Convert.ToInt32(playerData[4]);
            PokedexSeen = Convert.ToInt32(playerData[5]);
            PokedexEvolved = Convert.ToInt32(playerData[6]);
            IsMember = playerData[10] == "1";
        }

        private void OnUpdateTime(string[] data)
        {
            var timeData = data[1].Split('|');

            PokemonTime = timeData[0];
            var dt = Convert.ToDateTime(PokemonTime);

            Weather = timeData[1];

            PokeTimeUpdated?.Invoke(PokemonTime, Weather);
        }

        private void OnNpcBattlers(string[] data)
        {
            if (!IsMapLoaded) return;

            var defeatedBattlers = data[1].Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(id => int.Parse(id));

            Map.Npcs.Clear();
            foreach (var npc in Map.OriginalNpcs)
            {
                var clone = npc.Clone();
                if (defeatedBattlers.Contains(npc.Id))
                    clone.CanBattle = false;
                Map.Npcs.Add(clone);
            }
        }

        private void OnNpcDestroy(string[] data)
        {
            if (!IsMapLoaded) return;

            var npcData = data[1].Split('|');

            foreach (var npcText in npcData)
            {
                var npcId = int.Parse(npcText);
                foreach (var npc in Map.Npcs)
                    if (npc.Id == npcId)
                    {
                        Map.Npcs.Remove(npc);
                        break;
                    }
            }

            AreNpcReceived = true;
            NpcReceived?.Invoke(Map.Npcs);
        }

        private void OnTeamUpdate(string[] data)
        {
            var teamData = data[1].Split(new[] { "\r\n" }, StringSplitOptions.None);

            Team.Clear();
            foreach (var pokemon in teamData)
            {
                if (pokemon == string.Empty)
                    continue;

                var pokemonData = pokemon.Split('|');

                Team.Add(new Pokemon(pokemonData));
            }

            if (IsMapLoaded)
            {
                CanUseCut = HasCutAbility();
                CanUseSmashRock = HasRockSmashAbility();
            }

            if (_swapTimeout.IsActive)
                _swapTimeout.Set(Rand.Next(500, 1000));
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

            var itemsData = content.Split(new[] { "\r\n" }, StringSplitOptions.None);
            foreach (var item in itemsData)
            {
                if (item == string.Empty)
                    continue;
                var itemData = item.Split(new[] { "|" }, StringSplitOptions.None);
                Items.Add(new InventoryItem(itemData[0], Convert.ToInt32(itemData[1]), Convert.ToInt32(itemData[2]),
                    Convert.ToInt32(itemData[3])));
            }

            if (_itemUseTimeout.IsActive)
                _itemUseTimeout.Set(Rand.Next(500, 1000));
            InventoryUpdated?.Invoke();
        }

        private void OnBattleJoin(string packet)
        {
            var data = packet.Substring(4).Split('|');

            IsScriptActive = false;

            IsInBattle = true;
            ActiveBattle = new Battle(PlayerName, data);

            _movements.Clear();
            _slidingDirection = null;

            _battleTimeout.Set(Rand.Next(4000, 6000));
            _fishingTimeout.Cancel();

            BattleStarted?.Invoke();

            var battleMessages = ActiveBattle.BattleText.Split(new[] { "\r\n" }, StringSplitOptions.None);

            foreach (var message in battleMessages)
                if (!ActiveBattle.ProcessMessage(Team, message))
                    BattleMessage?.Invoke(I18N.Replace(message));
        }

        private void OnBattleMessage(string[] data)
        {
            if (!IsInBattle)
                return;
            SendCurrentMapSpawnPacket();
            AskForPokedex();

            var battleData = data[1].Split(new[] { "|" }, StringSplitOptions.None);
            var battleMessages = battleData[4].Split(new[] { "\r\n" }, StringSplitOptions.None);

            foreach (var message in battleMessages)
                if (!ActiveBattle.ProcessMessage(Team, message))
                    BattleMessage?.Invoke(I18N.Replace(message));

            PokemonsUpdated?.Invoke();

            if (ActiveBattle.IsFinished)
                _battleTimeout.Set(Rand.Next(1500, 5000));
            else
                _battleTimeout.Set(Rand.Next(2000, 4000));

            if (ActiveBattle.IsFinished)
            {
                IsInBattle = false;
                ActiveBattle = null;
                BattleEnded?.Invoke();
            }
        }

        private void OnScript(string[] data)
        {
            var id = data[2];
            var status = Convert.ToInt32(data[1]);
            var script = data[3];

            DialogContent = script.Split(new[] { "-#-" }, StringSplitOptions.None);
            if (script.Contains("-#-") && status > 1)
                script = DialogContent[0];
            var messages = script.Split(new[] { "-=-" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var message in messages)
            {
                if (message.StartsWith("emote") || message.StartsWith("playsound") || message.StartsWith("playmusic") ||
                    message.StartsWith("playcry"))
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
            var moveId = Convert.ToInt32(data[1]);
            var moveName = Convert.ToString(data[2]);
            var pokemonUid = Convert.ToInt32(data[3]);
            var movePp = Convert.ToInt32(data[4]);
            LearningMove?.Invoke(moveId, moveName, pokemonUid);
            _itemUseTimeout.Cancel();
            // ^|.|348|.|Cut|.|3|.|30|.\
        }

        private void OnEvolving(string[] data)
        {
            var evolvingPokemonUid = Convert.ToInt32(data[1]);
            var evolvingItem = Convert.ToInt32(data[3]);

            Evolving.Invoke(evolvingPokemonUid, evolvingItem);
        }

        private void OnUpdatePlayer(string[] data)
        {
            var updateData = data[1].Split('|');

            var isNewPlayer = false;
            PlayerInfos player;
            var expiration = DateTime.UtcNow.AddSeconds(20);
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
                PlayerAdded?.Invoke(player);
            else
                PlayerUpdated?.Invoke(player);
        }

        private void OnChannels(string[] data)
        {
            Channels.Clear();
            var channelsData = data[1].Split('|');
            for (var i = 1; i < channelsData.Length; i += 2)
            {
                var channelId = channelsData[i];
                var channelName = channelsData[i + 1];
                Channels.Add(new ChatChannel(channelId, channelName));
            }
            RefreshChannelList?.Invoke();
        }

        private void OnChatMessage(string[] data)
        {
            var fullMessage = data[1];
            var chatData = fullMessage.Split(':');

            if (fullMessage[0] == '*' && fullMessage[2] == '*')
                fullMessage = fullMessage.Substring(3);

            string message;
            if (chatData.Length <= 1) // we are not really sure what this stands for
            {
                string channelName;

                var start = fullMessage.IndexOf('(') + 1;
                var end = fullMessage.IndexOf(')');
                if (fullMessage.Length <= end || start == 0 || end == -1)
                {
                    var packet = string.Join("|.|", data);
                    InvalidPacket?.Invoke(packet, "Channel System Message with invalid channel");
                    channelName = "";
                }
                else
                {
                    channelName = fullMessage.Substring(start, end - start);
                }

                if (fullMessage.Length <= end + 2 || start == 0 || end == -1)
                {
                    var packet = string.Join("|.|", data);
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

                var start = fullMessage[0] == '(' ? 1 : 0;
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
                        var packet = string.Join("|.|", data);
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
                        var packet = string.Join("|.|", data);
                        InvalidPacket?.Invoke(packet, "Message with invalid mode");
                        message = "";
                    }
                    mode = fullMessage.Substring(start, end - start);
                }
                string conversation = null;
                if (channelName == "PM")
                {
                    end = fullMessage.IndexOf(':');
                    var header = "";
                    if (end == -1)
                    {
                        var packet = string.Join("|.|", data);
                        InvalidPacket?.Invoke(packet, "Channel Private Message with invalid author");
                        conversation = "";
                    }
                    else
                    {
                        header = fullMessage.Substring(0, end);
                        start = header.LastIndexOf(' ') + 1;
                        if (end == -1)
                        {
                            var packet = string.Join("|.|", data);
                            InvalidPacket?.Invoke(packet, "Channel Private Message with invalid author");
                            conversation = "";
                        }
                        else
                        {
                            conversation = header.Substring(start);
                        }
                    }
                    if (header.Contains(" to "))
                        author = PlayerName;
                    else
                        author = conversation;
                }
                else
                {
                    start = fullMessage.IndexOf("[n=") + 3;
                    end = fullMessage.IndexOf("][/n]:");
                    if (end == -1)
                    {
                        var packet = string.Join("|.|", data);
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
                    var packet = string.Join("|.|", data);
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
                        ChannelPrivateMessage?.Invoke(conversation, mode, author, message);
                    else
                        ChannelMessage?.Invoke(channelName, mode, author, message);
                }
                else
                {
                    if (message.IndexOf("em(") == 0)
                    {
                        end = message.IndexOf(")");
                        int emoteId;
                        if (end != -1 && end - 3 > 0)
                        {
                            var emoteIdString = message.Substring(3, end - 3);
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

            var offset = fullMessage.IndexOf(':') + 2;
            if (offset == -1 + 2) // for clarity... I prefectly know it's -3
            {
                var packet = string.Join("|.|", data);
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

            SystemMessage?.Invoke(I18N.Replace(message));
        }

        private void OnPrivateMessage(string[] data)
        {
            if (data.Length < 2)
            {
                var packet = string.Join("|.|", data);
                InvalidPacket?.Invoke(packet, "PM with no parameter");
            }
            var nicknames = data[1].Split(new[] { "-=-" }, StringSplitOptions.None);
            if (nicknames.Length < 2)
            {
                var packet = string.Join("|.|", data);
                InvalidPacket?.Invoke(packet, "PM with invalid header");
                return;
            }

            string conversation;
            if (nicknames[0] != PlayerName)
                conversation = nicknames[0];
            else
                conversation = nicknames[1];

            if (data.Length < 3)
            {
                var packet = string.Join("|.|", data);
                InvalidPacket?.Invoke(packet, "PM without a message");
                /*
                 * the PM is sent since the packet is still understandable
                 * however, PRO client does not allow it
                 */
                PrivateMessage?.Invoke(conversation, null, conversation + " (deduced)", "");
                return;
            }

            string mode = null;
            var offset = data[2].IndexOf('[') + 1;
            var end = 0;
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
            if (!Conversations.Contains(conversation))
                Conversations.Add(conversation);

            var modeRemoved = data[2];
            if (end != 0)
                modeRemoved = data[2].Substring(end + 2);
            offset = modeRemoved.IndexOf(' ');
            var speaker = modeRemoved.Substring(0, offset);

            offset = data[2].IndexOf(':') + 2;
            var message = data[2].Substring(offset);

            PrivateMessage?.Invoke(conversation, mode, speaker, message);
        }

        public int GetBoxIdFromPokemonUid(int lastUid)
        {
            return (lastUid - 7) / 15 + 1;
        }

        private void OnPcBox(string[] data)
        {
            _refreshingPcBox.Cancel();
            IsPcBoxRefreshing = false;
            if (Map.IsPc(PlayerX, PlayerY - 1))
                IsPcOpen = true;
            var body = data[1].Split('=');
            if (body.Length < 3)
            {
                InvalidPacket?.Invoke(data[0] + "|.|" + data[1], "Received an invalid PC Box packet");
                return;
            }
            PcGreatestUid = Convert.ToInt32(body[0]);

            var pokemonCount = Convert.ToInt32(body[1]);
            if (pokemonCount <= 0 || pokemonCount > 15)
            {
                InvalidPacket?.Invoke(data[0] + "|.|" + data[1], "Received an invalid PC Box size");
                return;
            }
            var pokemonListDatas = body[2].Split(new[] { "\r\n" }, StringSplitOptions.None);
            if (body.Length < 1)
            {
                InvalidPacket?.Invoke(data[0] + "|.|" + data[1], "Received an empty box");
                return;
            }
            var pokemonBox = new List<Pokemon>();
            foreach (var pokemonDatas in pokemonListDatas)
            {
                if (pokemonDatas == string.Empty) continue;
                var pokemonDatasArray = pokemonDatas.Split('|');
                var pokemon = new Pokemon(pokemonDatasArray);
                if (CurrentPcBoxId != GetBoxIdFromPokemonUid(pokemon.Uid))
                {
                    InvalidPacket?.Invoke(data[0] + "|.|" + data[1],
                        "Received a box packet for an unexpected box: expected #"
                        + CurrentPcBox + ", received #" + GetBoxIdFromPokemonUid(pokemon.Uid));
                    return;
                }
                pokemonBox.Add(pokemon);
            }
            if (pokemonBox.Count != pokemonCount)
            {
                InvalidPacket?.Invoke(data[0] + "|.|" + data[1],
                    "Received a PC Box size that does not match the content");
                return;
            }
            CurrentPcBox = pokemonBox;
            PcBoxUpdated?.Invoke(CurrentPcBox);
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
                DownloadMap(mapName);
        }

        private void DownloadMap(string mapName)
        {
            Console.WriteLine("[Map] Requesting: " + MapName);

            Map = null;
            AreNpcReceived = false;
            MapName = mapName;
            _mapClient.DownloadMap(MapName);
            Players.Clear();
            //when new map is requested, reset guarded fields from npc traineres
            _guardedFields = null;
#if DEBUG && DEBUG_TRAINER_BATTLES
            Console.WriteLine("Trainer Battles | guarded fields | reset");
#endif
        }

        private void LoadSpawnMenu(string[] data)
        {
            var sdata = data[1].Split(',');
            SpawnList = new List<PokemonSpawn>();
            for (var i = 1; i < sdata.Length - 1; i++)
            {
                var fish = false;
                var surf = false;
                var hitem = false;
                var msonly = false;
                var captured = false;

                if (sdata[i].Contains("f"))
                {
                    fish = true;
                    sdata[i] = sdata[i].Replace("f", string.Empty);
                }

                if (sdata[i].Contains("i"))
                {
                    hitem = true;
                    sdata[i] = sdata[i].Replace("i", string.Empty);
                }

                if (sdata[i].Contains("s"))
                {
                    surf = true;
                    sdata[i] = sdata[i].Replace("s", string.Empty);
                }

                if (sdata[i].Contains("m"))
                {
                    msonly = true;
                    sdata[i] = sdata[i].Replace("m", string.Empty);
                }

                if (sdata[i].Contains("c"))
                {
                    captured = true;
                    sdata[i] = sdata[i].Replace("c", string.Empty);
                }

                // Shit for ms color lol
                if (sdata[i].Contains("x"))
                    sdata[i] = sdata[i].Replace("x", string.Empty);
                //Adding to pokemon Spawn class each pokemon's each data
                var pokeaadd = new PokemonSpawn(Convert.ToInt32(sdata[i]), captured, surf, fish, hitem, msonly);
                //Adding to the list
                SpawnList.Add(pokeaadd);
            }
            /*This LoadPokemons is used only if the player map is the same map as the data map
             *I mean these information are used for the same map as the player map so if these information aren't of the same map of the player
             * we not going to add to our bot spawn list I mean if we hover the map that spawn list.
             */
            if (sdata[0].Contains(MapName) || IsRequestingForCurrentSpawnCheck)
                SpawnListUpdated?.Invoke(SpawnList.ToList());
            //Checking is all caught on the player map
            if (IsCrrentMap && IsRequestingForSpawnCheck && ScriptStarted && IsRequestingForCurrentSpawnCheck)
                IsAllcaughtOn(SpawnList.ToList());
        }

        private void AnotherMapLoadPokemons(string[] data)
        {
            /* This is used for if the data isn't contains the map of the player current map
            * This is only used for the "isAllCaughtOn(map name)"
            */
            AnotherMapSpawnList.Clear();
            var sdata = data[1].Split(',');

            for (var i = 1; i < sdata.Length - 1; i++)
            {
                /*
                 * These All are used for the same use as the LoadPokemons
                 */
                var fish = false;
                var surf = false;
                var hitem = false;
                var msonly = false;
                var captured = false;

                if (sdata[i].Contains("f"))
                {
                    fish = true;
                    sdata[i] = sdata[i].Replace("f", string.Empty);
                }

                if (sdata[i].Contains("i"))
                {
                    hitem = true;
                    sdata[i] = sdata[i].Replace("i", string.Empty);
                }

                if (sdata[i].Contains("s"))
                {
                    surf = true;
                    sdata[i] = sdata[i].Replace("s", string.Empty);
                }

                if (sdata[i].Contains("m"))
                {
                    msonly = true;
                    sdata[i] = sdata[i].Replace("m", string.Empty);
                }

                if (sdata[i].Contains("c"))
                {
                    captured = true;
                    sdata[i] = sdata[i].Replace("c", string.Empty);
                }

                // Shit for ms color lol
                if (sdata[i].Contains("x"))
                    sdata[i] = sdata[i].Replace("x", string.Empty);
                var pokeaadd = new PokemonSpawn(Convert.ToInt32(sdata[i]), captured, surf, fish, hitem, msonly);

                AnotherMapSpawnList.Add(pokeaadd);
            }
            // If the script requested for the spawn check we going to check
            if (IsRequestingForSpawnCheck && !IsRequestingForCurrentSpawnCheck)
                IsAllcaughtOn(AnotherMapSpawnList.ToList());
        }

        /*
         * This is the main guy or function who going to check is all pokemon caught on a map
         */

        public void IsAllcaughtOn(List<PokemonSpawn> pkmns)
        {
            lock (pkmns)
            {
                if (!ScriptStarted && !IsScriptActive && !IsRequestingForSpawnCheck)
                    pkmns.Clear();
                else
                    for (var j = 0; j <= pkmns.Count - 1; j++)
                    {
                        var trash = new List<PokemonSpawn>();
                        var caught = new List<PokemonSpawn>();
                        var allSpawns = new List<PokemonSpawn>();
                        var notCaughtMsPoke = new List<PokemonSpawn>();
                        allSpawns.Add(pkmns[j]);
                        if (pkmns[j].Msonly && IsMember)
                            trash.Add(pkmns[j]);
                        while (trash.Count > 0)
                            for (var o = 0; o <= trash.Count - 1; o++)
                            {
                                allSpawns.Remove(trash[o]);
                                trash.RemoveAt(o);
                            }
                        if (pkmns[j].Msonly && IsMember && !pkmns[j].Captured)
                            notCaughtMsPoke.Add(pkmns[j]);
                        else if (notCaughtMsPoke.Count > 0 && IsMember)
                            for (var l = 0; l <= notCaughtMsPoke.Count - 1; l++)
                                allSpawns.Add(notCaughtMsPoke[l]);
                        if (pkmns[j].Captured)
                            caught.Add(pkmns[j]);
                        if (allSpawns.Count - 1 == caught.Count - 1)
                            IsCaughtOn = true;
                        else
                            IsCaughtOn = false;
                    }
            }
        }

        /*
         * This Function help us to check is all pokemon caught on a map
         */

        public bool IsAllPokemonCaughtOn(string map)
        {
            if (IsRequestingForSpawnCheck)
            {
                SpawnMapName = map;
                if (map == MapName)
                {
                    IsCrrentMap = true;
                    IsRequestingForCurrentSpawnCheck = true;
                    SendPacket("k|.|" + map.ToLowerInvariant());
                }
                else
                {
                    IsCrrentMap = false;
                    IsRequestingForCurrentSpawnCheck = false;
                    SendPacket("k|.|" + map.ToLowerInvariant());
                }
                IsSpawnListThingStarted = true;
            }
            return IsCaughtOn;
        }

        #region Trading Variables

        public event Action<string> TradeRequested;

        public event Action TradeCanceled;

        public event Action TradeAccepted;

        public event Action<string[]> TradeMoneyUpdated;

        public event Action TradePokemonUpdated;

        public event Action<string[]> TradeStatusUpdated;

        public event Action TradeStatusReset;

        public List<TradePokemon> FirstTrade;
        public List<TradePokemon> SecondTrade;

        #endregion Trading Variables

        #region Pokedex

        private void AskForPokedex()
        {
            SendPacket("p|.|l|0");
        }

        private void HandlePokedexMsg(string[] data)
        {
            var tdata = data[1].Split('|');
            var type = tdata[0];
            if (type == "l")
            {
                var pdata = tdata[2].Split('<'); // Format now : PokemonID>StatusID
                var num2 = Convert.ToInt32(tdata[1]);
                for (var i = 0; i < pdata.Length; i++)
                    if (pdata[i].Length > 0)
                    {
                        var pokedata = pdata[i].Split('>');
                        var poke = new PokedexPokemon(Convert.ToInt32(pokedata[1]), Convert.ToInt32(pokedata[0]));
                        //Console.WriteLine(poke.ToString() + pokedata[1]);
                        PokedexList.Add(poke);
                    }
                //if (num2 != -1)
                //{
                //    if (PokedexList[num2].id > 0)
                //    {
                //        SendPacket("p|.|a|" + PokedexList[num2].pokeid2.ToString());
                //    }
                //}
            }
            else if (type == "a")
            {
                var ddata = tdata[2].Split('|');
                var num = FindPokedexEntry(Convert.ToInt32(tdata[1]));
                var selectedEntry = num;
                var areadata = tdata[20].Split(new[]
                {
                    "<"
                }, StringSplitOptions.None);
                for (var j = 0; j < areadata.Length; j++)
                    PokedexList[num].Area.Add(areadata[j]);
                for (var i = 0; i < PokedexList[num].Area.Count; i++)
                    if (PokedexList[num].Area[i].Length > 2)
                    {
                        var text = TimeZoneIcon(PokedexList[num].Area[i].Substring(0, 1));
                        if (PokedexList[num].Area[i].Substring(1, 1) == "1")
                        {
                            GetAreaName.Add(string.Concat(PokedexList[num].Area[i].Substring(2)));
                            TimeZone.Add(string.Concat(new[]
                            {
                                "Land,",
                                text
                            }));
                            IsMs.Add(string.Concat("MS No"));
                        }
                        else if (PokedexList[num].Area[i].Substring(1, 1) == "2")
                        {
                            GetAreaName.Add(string.Concat(PokedexList[num].Area[i].Substring(2)));
                            TimeZone.Add(string.Concat(new[]
                            {
                                "Water,",
                                text
                            }));
                            IsMs.Add(string.Concat("MS No"));
                        }
                        else if (PokedexList[num].Area[i].Substring(1, 1) == "3")
                        {
                            GetAreaName.Add(string.Concat(PokedexList[num].Area[i].Substring(2)));
                            TimeZone.Add(string.Concat(new[]
                            {
                                "Land,Water,",
                                text
                            }));
                            IsMs.Add(string.Concat("MS No"));
                        }
                        else if (PokedexList[num].Area[i].Substring(1, 1) == "4")
                        {
                            GetAreaName.Add(string.Concat(PokedexList[num].Area[i].Substring(2)));
                            TimeZone.Add(string.Concat(new[]
                            {
                                "Land,",
                                text
                            }));
                            IsMs.Add(string.Concat("MS Yes"));
                        }
                        else if (PokedexList[num].Area[i].Substring(1, 1) == "6")
                        {
                            GetAreaName.Add(string.Concat(PokedexList[num].Area[i].Substring(2)));
                            TimeZone.Add(string.Concat(new[]
                            {
                                "Land,Water,",
                                text
                            }));
                            IsMs.Add(string.Concat("MS Yes"));
                        }
                        else if (PokedexList[num].Area[i].Substring(1, 1) == "7")
                        {
                            GetAreaName.Add(string.Concat(PokedexList[num].Area[i].Substring(2)));
                            TimeZone.Add(string.Concat(new[]
                            {
                                "Water,",
                                text
                            }));
                            IsMs.Add(string.Concat("MS Yes"));
                        }
                        else if (PokedexList[num].Area[i].Substring(1, 1) == "8")
                        {
                            GetAreaName.Add(string.Concat(PokedexList[num].Area[i].Substring(2)));
                            TimeZone.Add(string.Concat(new[]
                            {
                                "Land,Water,",
                                text
                            }));
                            IsMs.Add(string.Concat("MS Yes"));
                        }
                        else if (PokedexList[num].Area[i].Substring(1, 1) == "9")
                        {
                            GetAreaName.Add(string.Concat(PokedexList[num].Area[i].Substring(2)));
                            TimeZone.Add(string.Concat(new[]
                            {
                                "Land,Water,",
                                text
                            }));
                            IsMs.Add(string.Concat("MS Yes"));
                        }
                        else
                        {
                            GetAreaName.Add(string.Concat(PokedexList[num].Area[i].Substring(2)));
                            TimeZone.Add(string.Concat(new[]
                            {
                                " ",
                                text
                            }));
                            IsMs.Add(string.Concat("MS No"));
                        }
                    }
                PokedexDataUpdated?.Invoke();
                // Pokedex Informations, not done bcoz i'm lazy.
                /* If you wanna do it :
                 * [0] : ID
                 * [1] : Animal type
                 * [2] : Unknown (not important), maybe gender percentage
                 * [3] : Height
                 * [4] : Weight
                 * [5] : Description
                 * [6] : Type
                 * [7] : Base stats, format : HP|ATK|DEF|SPD|SP. DEF|SP. ATK
                 * [8] : Abilities , Format : Same as list above, LVL>Name
                 * [9] : Places where the pokémon can be caught, Format IDName<IDName
                     Some ids :
                       41 : Land, Day, Morning.
                       01 : Land, Day, Morning, Night.
                       14 : Land, Day, MS.
                       04 : Land, Day, Morning, Night, MS.
                       11 : Land, Morning.
                       17 : Water, Morning, MS.
                       02 : Water, Day, Morning, Night.
                       42 : Day, Water, Morning.
                       12 : Morning, Water.
                       03 : Land, Day, Morning, Water, Night.
                       23 : Day, Water, Land.
                       32 : Night, Water.
                       07 : Water, MS, Day, Night, Morning.
                       11 : Morning, Land.
                       22 : Day, Water.
                       13 : Land, Water, Day, Night, Morning.
                       31 : Land, Night.
                       33 :
                       37 :
                */
            }
        }

        public bool HasSeen(string pokeName)
        {
            var seen = false;
            if (PokedexList != null)
            {
                PokedexList.ForEach(delegate (PokedexPokemon pkmn)
                {
                    if (pkmn.ToString() == pokeName && pkmn.IsSeen())
                        seen = true;
                });
                return seen;
            }
            return false;
        }

        public bool IsAlreadyCaught(string pokeName)
        {
            var caught = false;
            if (PokedexList != null)
            {
                PokedexList.ForEach(delegate (PokedexPokemon pkmn)
                {
                    if (pkmn.ToString() == pokeName && pkmn.IsCaught())
                        caught = true;
                });
                return caught;
            }
            return false;
        }

        public int FindPokedexEntry(int id)
        {
            if (PokedexList.Count <= 0)
                return -1;
            for (var i = 0; i < PokedexList.Count; i++)
                if (PokedexList[i] != null && PokedexList[i].Pokeid2 == id)
                    return i;
            return -1;
        }

        private string TimeZoneIcon(string ti)
        {
            if (ti == "0")
                return "Morning,Day,Night";
            if (ti == "1")
                return "Morning";
            if (ti == "2")
                return "Day";
            if (ti == "3")
                return "Night";
            if (ti == "4")
                return "Morning,Day";
            if (ti == "5")
                return "Morning,Night";
            return string.Empty;
        }

        #endregion Pokedex

        #region Trade functions

        private void HandleActions(string[] data)
        {
            var action = data[1].Split('|')[3];
            if (action.Contains("/trade"))
            {
                OnTradeRequest(data);
            }
        }

        private void OnTradeRequest(string[] data)
        {
            var applicant = data[1].Split('|')[3].Replace("/trade ", ""); // Basically getting exchange applicant.
            TradeRequested?.Invoke(applicant);
        }

        private void HandleTrade(string[] data)
        {
            var tdata = data[1].Split('|');
            var type = tdata[0];
            if (type == "c")
            {
                TradeCanceled?.Invoke();
            }
            else
            {
                TradeMoneyUpdated?.Invoke(tdata);
                Console.WriteLine(tdata[0] + '|' + tdata[1] + '|' // First exhcnager
                                  + tdata[2] + '|' // Second
                                  + tdata[3] + '|' // First money on exhcnage
                                  + tdata[4] + '|'); // Second money on exchange0
            }
        }

        private void OnTradeUpdate(string[] data)
        {
            var teamData = data[1].Split('|');
            var uid = 0;
            FirstTrade.Clear();
            SecondTrade.Clear();
            var tradeList = new List<TradePokemon>();
            foreach (var pokemon in teamData)
            {
                uid++;
                if (pokemon == string.Empty)
                    continue;

                var pokemonData = pokemon.Split(',');
                if (uid <= 6)
                    FirstTrade.Add(new TradePokemon(pokemonData));
                else
                    SecondTrade.Add(new TradePokemon(pokemonData));
            }
            TradePokemonUpdated?.Invoke();
        }

        private void OnTradeStatusChange(string[] data)
        {
            TradeStatusUpdated?.Invoke(data);
        }

        // Send a "change to final screen" order to client. Useless.
        private void UselessTradeFeature()
        {
            TradeAccepted?.Invoke();
        }

        private void OnTradeStatusReset()
        {
            TradeStatusReset?.Invoke();
        }

        #endregion Trade functions
    }
}
