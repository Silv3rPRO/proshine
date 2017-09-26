using PROBot.Modules;
using PROBot.Scripting;
using PROProtocol;
using System;
using System.Collections.Generic;
using System.IO;

namespace PROBot
{
    public class BotClient
    {
        public enum State
        {
            Stopped,
            Started,
            Paused
        }

        private readonly Timeout _actionTimeout = new Timeout();

        private bool _loginRequested;

        public BotClient()
        {
            AccountManager = new AccountManager("Accounts");
            PokemonEvolver = new PokemonEvolver(this);
            MoveTeacher = new MoveTeacher(this);
            StaffAvoider = new StaffAvoider(this);
            AutoReconnector = new AutoReconnector(this);
            IsTrainerBattlesActive = new IsTrainerBattlesActive(this);
            MovementResynchronizer = new MovementResynchronizer(this);
            Rand = new Random();
            SliderOptions = new Dictionary<int, OptionSlider>();
            TextOptions = new Dictionary<int, TextOption>();
        }

        public GameClient Game { get; private set; }
        public BattleAi Ai { get; private set; }
        public BaseScript Script { get; private set; }
        public AccountManager AccountManager { get; }
        public Random Rand { get; }
        public Account Account { get; set; }

        public State Running { get; private set; }
        public bool IsPaused { get; private set; }

        public PokemonEvolver PokemonEvolver { get; }
        public MoveTeacher MoveTeacher { get; }
        public StaffAvoider StaffAvoider { get; }
        public AutoReconnector AutoReconnector { get; }
        public IsTrainerBattlesActive IsTrainerBattlesActive { get; }
        public MovementResynchronizer MovementResynchronizer { get; }
        public Dictionary<int, OptionSlider> SliderOptions { get; set; }
        public Dictionary<int, TextOption> TextOptions { get; set; }

        public event Action<State> StateChanged;

        public event Action<string> MessageLogged;

        public event Action ClientChanged;

        public event Action ConnectionOpened;

        public event Action ConnectionClosed;

        public event Action<OptionSlider> SliderCreated;

        public event Action<OptionSlider> SliderRemoved;

        public event Action<TextOption> TextboxCreated;

        public event Action<TextOption> TextboxRemoved;

        public void CancelInvokes()
        {
            if (Script != null)
                foreach (var invoker in Script.Invokes)
                    invoker.Called = true;
        }

        public void CallInvokes()
        {
            if (Script != null)
                for (var i = Script.Invokes.Count - 1; i >= 0; i--)
                    if (Script.Invokes[i].Time < DateTime.UtcNow)
                        if (Script.Invokes[i].Called)
                            Script.Invokes.RemoveAt(i);
                        else
                            Script.Invokes[i].Call();
        }

        public void RemoveText(int index)
        {
            TextboxRemoved?.Invoke(TextOptions[index]);
            TextOptions.Remove(index);
        }

        public void RemoveSlider(int index)
        {
            SliderRemoved?.Invoke(SliderOptions[index]);
            SliderOptions.Remove(index);
        }

        public void CreateText(int index, string content)
        {
            TextOptions[index] = new TextOption("Text " + index + ": ",
                "Custom text option " + index + " for use in scripts.", content);
            TextboxCreated?.Invoke(TextOptions[index]);
        }

        public void CreateText(int index, string content, bool isName)
        {
            if (isName)
                TextOptions[index] =
                    new TextOption(content, "Custom text option " + index + " for use in scripts.", "");
            else
                TextOptions[index] = new TextOption("Text " + index + ": ", content, "");

            TextboxCreated?.Invoke(TextOptions[index]);
        }

        public void CreateSlider(int index, bool enable)
        {
            SliderOptions[index] =
                new OptionSlider("Option " + index + ": ", "Custom option " + index + " for use in scripts.");
            SliderOptions[index].IsEnabled = enable;
            SliderCreated?.Invoke(SliderOptions[index]);
        }

        public void CreateSlider(int index, string content, bool isName)
        {
            if (isName)
                SliderOptions[index] = new OptionSlider(content, "Custom option " + index + " for use in scripts.");
            else
                SliderOptions[index] = new OptionSlider("Option " + index + ": ", content);

            SliderCreated?.Invoke(SliderOptions[index]);
        }

        public void LogMessage(string message)
        {
            MessageLogged?.Invoke(message);
        }

        public void SetClient(GameClient client)
        {
            Game = client;
            Ai = null;
            Stop();

            if (client != null)
            {
                Ai = new BattleAi(client);
                client.ConnectionOpened += Client_ConnectionOpened;
                client.ConnectionFailed += Client_ConnectionFailed;
                client.ConnectionClosed += Client_ConnectionClosed;
                client.BattleMessage += Client_BattleMessage;
                client.SystemMessage += Client_SystemMessage;
                client.DialogOpened += Client_DialogOpened;
                client.TeleportationOccuring += Client_TeleportationOccuring;
                client.LogMessage += LogMessage;
            }
            ClientChanged?.Invoke();
        }

        public void Login(Account account)
        {
            Account = account;
            _loginRequested = true;
        }

        private void LoginUpdate()
        {
            GameClient client;
            var server = GameServerExtensions.FromName(Account.Server);
            if (Account.Socks.Version != SocksVersion.None)
                client = new GameClient(
                    new GameConnection(server, (int)Account.Socks.Version, Account.Socks.Host, Account.Socks.Port,
                        Account.Socks.Username, Account.Socks.Password),
                    new MapConnection((int)Account.Socks.Version, Account.Socks.Host, Account.Socks.Port,
                        Account.Socks.Username, Account.Socks.Password));
            else
                client = new GameClient(new GameConnection(server), new MapConnection());
            SetClient(client);
            client.Open();
        }

        public void Logout(bool allowAutoReconnect)
        {
            if (!allowAutoReconnect)
                AutoReconnector.IsEnabled = true;
            Game.Close();
        }

        public void Update()
        {
            if (Script != null)
                Script.Update();
            CallInvokes();
            AutoReconnector.Update();
            if (_loginRequested)
            {
                LoginUpdate();
                _loginRequested = false;
                return;
            }

            if (Running != State.Started)
                return;
            if (PokemonEvolver.Update()) return;
            if (MoveTeacher.Update()) return;

            if (Game.IsMapLoaded && Game.AreNpcReceived && Game.IsInactive)
                ExecuteNextAction();
        }

        public void Start()
        {
            if (Game != null && Script != null && Running == State.Stopped)
            {
                _actionTimeout.Set();
                Running = State.Started;
                StateChanged?.Invoke(Running);
                Script.Start();
            }
        }

        public void Pause()
        {
            if (Game != null && Script != null && Running != State.Stopped)
                if (Running == State.Started)
                {
                    Running = State.Paused;
                    StateChanged?.Invoke(Running);
                    Script.Pause();
                }
                else
                {
                    Running = State.Started;
                    StateChanged?.Invoke(Running);
                    Script.Resume();
                }
        }

        public void Stop()
        {
            if (Game != null)
                Game.ClearPath();

            if (Running != State.Stopped)
            {
                Running = State.Stopped;
                StateChanged?.Invoke(Running);
                if (Script != null)
                    Script.Stop();
            }
        }

        public void LoadScript(string filename)
        {
            var input = File.ReadAllText(filename);

            var libs = new List<string>();
            if (Directory.Exists("Libs"))
            {
                var files = Directory.GetFiles("Libs");
                foreach (var file in files)
                    if (file.ToUpperInvariant().EndsWith(".LUA"))
                        libs.Add(File.ReadAllText(file));
            }

            BaseScript script = new LuaScript(this, Path.GetFullPath(filename), input, libs);

            Stop();
            Script = script;
            try
            {
                Script.ScriptMessage += Script_ScriptMessage;
                Script.Initialize();
            }
            catch (Exception ex)
            {
                Script = null;
                throw ex;
            }
        }

        public bool MoveToLink(string destinationMap)
        {
            var nearest = Game.Map.GetNearestLinks(destinationMap, Game.PlayerX, Game.PlayerY);
            if (nearest != null)
                foreach (var link in nearest)
                    if (MoveToCell(link.Item1, link.Item2)) return true;
            return false;
        }

        public bool MoveToCell(int x, int y, int requiredDistance = 0)
        {
            MovementResynchronizer.CheckMovement(x, y);

            var path = new Pathfinding(Game);
            bool result;

            if (Game.PlayerX == x && Game.PlayerY == y)
                result = path.MoveToSameCell();
            else
                result = path.MoveTo(x, y, requiredDistance);

            if (result)
                MovementResynchronizer.ApplyMovement(x, y);

            return result;
        }

        public bool TalkToNpc(Npc target)
        {
            var canInteract = Game.Map.CanInteract(Game.PlayerX, Game.PlayerY, target.PositionX, target.PositionY);
            if (canInteract)
            {
                Game.TalkToNpc(target.Id);
                return true;
            }
            return MoveToCell(target.PositionX, target.PositionY, 1);
        }

        public bool OpenPc()
        {
            var pcPosition = Game.Map.GetPc();
            if (pcPosition == null || Game.IsPcOpen)
                return false;
            var distance = Game.DistanceTo(pcPosition.Item1, pcPosition.Item2);
            if (distance == 1)
                return Game.OpenPc();
            return MoveToCell(pcPosition.Item1, pcPosition.Item2 + 1);
        }

        public bool RefreshPcBox(int boxId)
        {
            if (!Game.IsPcOpen)
                return false;
            if (!Game.RefreshPcBox(boxId))
                return false;
            _actionTimeout.Set();
            return true;
        }

        private void ExecuteNextAction()
        {
            try
            {
                var executed = Script.ExecuteNextAction();
                if (!executed && Running != State.Stopped && !_actionTimeout.Update())
                {
                    LogMessage("No action executed: stopping the bot.");
                    Stop();
                }
                else if (executed)
                {
                    _actionTimeout.Set();
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                LogMessage("Error during the script execution: " + ex);
#else
                LogMessage("Error during the script execution: " + ex.Message);
#endif
                Stop();
            }
        }

        private void Client_ConnectionOpened()
        {
            ConnectionOpened?.Invoke();
            Game.SendAuthentication(Account.Name, Account.Password,
                Account.MacAddress ?? HardwareHash.GenerateRandom());
        }

        private void Client_ConnectionClosed(Exception ex)
        {
            if (ex != null)
            {
#if DEBUG
                LogMessage("Disconnected from the server: " + ex);
#else
                LogMessage("Disconnected from the server: " + ex.Message);
#endif
            }
            else
            {
                LogMessage("Disconnected from the server.");
            }
            ConnectionClosed?.Invoke();
            SetClient(null);
        }

        private void Client_ConnectionFailed(Exception ex)
        {
            if (ex != null)
            {
#if DEBUG
                LogMessage("Could not connect to the server: " + ex);
#else
                LogMessage("Could not connect to the server: " + ex.Message);
#endif
            }
            else
            {
                LogMessage("Could not connect to the server.");
            }
            ConnectionClosed?.Invoke();
            SetClient(null);
        }

        private void Client_DialogOpened(string message)
        {
            if (Running == State.Started)
                Script.OnDialogMessage(message);
        }

        private void Client_SystemMessage(string message)
        {
            if (Running == State.Started)
                Script.OnSystemMessage(message);
        }

        private void Client_BattleMessage(string message)
        {
            if (Running == State.Started)
                Script.OnBattleMessage(message);
        }

        private void Client_TeleportationOccuring(string map, int x, int y)
        {
            var message = "Position updated: " + map + " (" + x + ", " + y + ")";
            if (Game.Map == null || Game.IsTeleporting)
            {
                message += " [OK]";
            }
            else if (Game.MapName != map)
            {
                message += " [WARNING, different map] /!\\";
            }
            else
            {
                var distance = GameClient.DistanceBetween(x, y, Game.PlayerX, Game.PlayerY);
                if (distance < 8)
                    message += " [OK, lag, distance=" + distance + "]";
                else
                    message += " [WARNING, distance=" + distance + "] /!\\";
            }
            LogMessage(message);
        }

        private void Script_ScriptMessage(string message)
        {
            LogMessage(message);
        }
    }
}
