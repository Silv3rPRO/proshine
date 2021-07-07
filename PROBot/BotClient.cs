﻿using PROBot.Modules;
using PROBot.Scripting;
using PROProtocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PROBot
{
    public class BotClient
    {
        public enum State
        {
            Stopped,
            Started,
            Paused
        };

        public GameClient Game { get; private set; }
        public BattleAI AI { get; private set; }
        public BaseScript Script { get; private set; }
        public AccountManager AccountManager { get; private set; }
        public Random Rand { get; private set; }
        public Account Account { get; set; }
        public UserSettings Settings { get; private set;}

        public State Running { get; private set; }
        public bool IsPaused { get; private set; }

        public event Action<State> StateChanged;
        public event Action<string> MessageLogged;
        public event Action ClientChanged;
        public event Action ConnectionOpened;
        public event Action ConnectionClosed;
        public event Action<OptionSlider> SliderCreated;
        public event Action<OptionSlider> SliderRemoved;
        public event Action<TextOption> TextboxCreated;
        public event Action<TextOption> TextboxRemoved;

        public PokemonEvolver PokemonEvolver { get; private set; }
        public MoveTeacher MoveTeacher { get; private set; }
        public StaffAvoider StaffAvoider { get; private set; }
        public AutoReconnector AutoReconnector { get; private set; }
        public MovementResynchronizer MovementResynchronizer { get; private set; }
        public Dictionary<int, OptionSlider> SliderOptions { get; set; }
        public Dictionary<int, TextOption> TextOptions { get; set; }
        
        private bool _loginRequested;
        private bool _authenticationRequired;

        private Timeout _actionTimeout = new Timeout();

        public BotClient()
        {
            AccountManager = new AccountManager("Accounts");
            PokemonEvolver = new PokemonEvolver(this);
            MoveTeacher = new MoveTeacher(this);
            StaffAvoider = new StaffAvoider(this);
            AutoReconnector = new AutoReconnector(this);
            MovementResynchronizer = new MovementResynchronizer(this);
            Rand = new Random();
            SliderOptions = new Dictionary<int, OptionSlider>();
            TextOptions = new Dictionary<int, TextOption>();
            Settings = new UserSettings();
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
            TextOptions[index] = new TextOption("Text " + index + ": ", "Custom text option " + index + " for use in scripts.", content);
            TextboxCreated?.Invoke(TextOptions[index]);
        }

        public void CreateText(int index, string content, bool isName)
        {
            if (isName)
                TextOptions[index] = new TextOption(content, "Custom text option " + index + " for use in scripts.", "");
            else
                TextOptions[index] = new TextOption("Text " + index + ": ", content, "");

            TextboxCreated?.Invoke(TextOptions[index]);
        }

        public void CreateSlider(int index, bool enable)
        {
            SliderOptions[index] = new OptionSlider("Option " + index + ": ", "Custom option " + index + " for use in scripts.");
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
            AI = null;
            Stop();

            if (client != null)
            {
                AI = new BattleAI(client);
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

        public void Relog(double delay)
        {
            Stop();
            AutoReconnector.Relog(delay);
            Game.Close();
        }

        private void LoginUpdate()
        {
            GameClient client;
            GameServer server = GameServerExtensions.FromName(Account.Server);
            if (Account.Socks.Version != SocksVersion.None)
            {
                // TODO: Clean this code.
                client = new GameClient(new GameConnection(server, (int)Account.Socks.Version, Account.Socks.Host, Account.Socks.Port, Account.Socks.Username, Account.Socks.Password),
                    new MapConnection((int)Account.Socks.Version, Account.Socks.Host, Account.Socks.Port, Account.Socks.Username, Account.Socks.Password));
            }
            else
            {
                client = new GameClient(new GameConnection(server), new MapConnection());
            }

            Encryption.Reset();
            SetClient(client);
            client.Open();
        }

        public void Logout(bool allowAutoReconnect)
        {
            if (!allowAutoReconnect)
            {
                AutoReconnector.IsEnabled = false;
            }
            Game.Close();
        }

        public void Update()
        {
            if (_authenticationRequired)
            {
                if (Encryption.StateReady)
                {
                    Game.SendAuthentication(Account.Name, Account.Password, Account.DeviceId ?? HardwareHash.GenerateRandom());
                    _authenticationRequired = false;
                }
                return;
            }
            AutoReconnector.Update();

            if (_loginRequested)
            {
                LoginUpdate();
                _loginRequested = false;
                return;
            }

            if (Game != null && Game.IsInBattle && Game.IsInactive && AI != null && AI.UseMandatoryAction())
            {
                return;
            }

            if (Running != State.Started)
            {
                return;
            }
            
            if (Game.IsCreatingNewCharacter)
            {
                LogMessage("Creating a new character with a random skin...");
                Game.CreateCharacter(Rand.Next(14), Rand.Next(28), Rand.Next(8), Rand.Next(6), Rand.Next(5));
                return;
            }

            if (PokemonEvolver.Update()) return;
            if (MoveTeacher.Update()) return;

            if (Game.IsMapLoaded && Game.AreNpcReceived && Game.IsInactive)
            {
                ExecuteNextAction();
            }
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
            {
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
                {
                    Script.Stop();
                }
            }
        }

        public void LoadScript(string filename)
        {
            string input = File.ReadAllText(filename);

            var libs = new List<string>();
            if (Directory.Exists("Libs"))
            {
                libs = Directory.GetFiles("Libs")
                    .Where(f => f.EndsWith(".lua", StringComparison.InvariantCultureIgnoreCase))
                    .Select(File.ReadAllText)
                    .ToList();
            }

            BaseScript script = new LuaScript(this, Path.GetFullPath(filename), input, libs);

            Stop();
            Script = script;
            try
            {
                Script.ScriptMessage += Script_ScriptMessage;
                Script.Initialize();
            }
            catch (Exception)
            {
                Script = null;
                throw;
            }
        }

        public bool MoveToLink(string destinationMap)
        {
            IEnumerable<Tuple<int, int>> nearest = Game.Map.GetNearestLinks(destinationMap, Game.PlayerX, Game.PlayerY);
            if (nearest != null)
            {
                foreach (Tuple<int, int> link in nearest)
                {
                    if (MoveToCell(link.Item1, link.Item2)) return true;
                }
            }
            return false;
        }

        public bool MoveToCell(int x, int y, int requiredDistance = 0)
        {
            MovementResynchronizer.CheckMovement(x, y);

            Pathfinding path = new Pathfinding(Game);
            bool result;

            if (Game.PlayerX == x && Game.PlayerY == y)
            {
                result = path.MoveToSameCell();
            }
            else
            {
                result = path.MoveTo(x, y, requiredDistance);
            }

            if (result)
            {
                MovementResynchronizer.ApplyMovement(x, y);
            }

            return result;
        }

        public bool TalkToNpc(Npc target)
        {
            bool canInteract = Game.Map.CanInteract(Game.PlayerX, Game.PlayerY, target.PositionX, target.PositionY);
            if (canInteract)
            {
                Game.TalkToNpc(target);
                return true;
            }
            else
            {
                return MoveToCell(target.PositionX, target.PositionY, 1);
            }
        }

        public bool OpenPC()
        {
            Tuple<int, int> pcPosition = Game.Map.GetPC();
            if (pcPosition == null || Game.IsPCOpen)
            {
                return false;
            }
            int distance = Game.DistanceTo(pcPosition.Item1, pcPosition.Item2);
            if (distance == 1)
            {
                return Game.OpenPC();
            }
            else
            {
                return MoveToCell(pcPosition.Item1, pcPosition.Item2 + 1);
            }
        }

        public bool RefreshPCBox(int boxId)
        {
            if (!Game.IsPCOpen)
            {
                return false;
            }
            if (!Game.RefreshPCBox(boxId))
            {
                return false;
            }
            _actionTimeout.Set();
            return true;
        }

        private void ExecuteNextAction()
        {
            try
            {
                bool executed = Script.ExecuteNextAction();
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
            _authenticationRequired = true;
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

        private void Client_DialogOpened(string message, string[] options)
        {
            if (Running == State.Started)
            {
                Script.OnDialogMessage(message);
            }
        }

        private void Client_SystemMessage(string message)
        {
            if (Running == State.Started)
            {
                Script.OnSystemMessage(message);
            }
        }

        private void Client_BattleMessage(string message)
        {
            if (Running == State.Started)
            {
                Script.OnBattleMessage(message);
            }
        }

        private void Client_TeleportationOccuring(string map, int x, int y)
        {
            string message = "Position updated: " + map + " (" + x + ", " + y + ")";
            if (Game.Map == null || Game.IsTeleporting)
            {
                message += " [OK]";
            }
            else if (Game.MapName != map)
            {
                message += " [WARNING, different map] /!\\";
                Script?.OnWarningMessage(true);
            }
            else
            {
                int distance = GameClient.DistanceBetween(x, y, Game.PlayerX, Game.PlayerY);
                if (distance < 8)
                {
                    message += " [OK, lag, distance=" + distance + "]";
                }
                else
                {
                    message += " [WARNING, distance=" + distance + "] /!\\";
                    Script?.OnWarningMessage(false, distance);
                }
            }
            LogMessage(message);
        }

        private void Script_ScriptMessage(string message)
        {
            LogMessage(message);
        }
    }
}
