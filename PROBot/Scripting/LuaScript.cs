using MoonSharp.Interpreter;
using PROBot.Utils;
using PROProtocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;

namespace PROBot.Scripting
{
    public class LuaScript : BaseScript
    {
        public BotClient Bot { get; private set; }

#if DEBUG
        public int TimeoutDelay = 60000;
#else
        public int TimeoutDelay = 3000;
#endif

        private Script _lua;
        private string _path;
        private string _content;
        private IList<string> _libsContent;
        private IDictionary<string, IList<DynValue>> _hookedFunctions;

        private bool _actionExecuted;

        public LuaScript(BotClient bot, string path, string content, IList<string> libsContent)
        {
            Bot = bot;
            _path = Path.GetDirectoryName(path);
            _content = content;
            _libsContent = libsContent;
        }

        public override void Initialize()
        {
            CreateLuaInstance();

            Name = _lua.Globals.Get("name").CastToString();
            Author = _lua.Globals.Get("author").CastToString();
            Description = _lua.Globals.Get("description").CastToString();
        }

        public override void Start()
        {
            CallFunctionSafe("onStart");
        }

        public override void Stop()
        {
            CallFunctionSafe("onStop");
        }

        public override void Pause()
        {
            CallFunctionSafe("onPause");
        }

        public override void Resume()
        {
            CallFunctionSafe("onResume");
        }

        public override void OnDialogMessage(string message)
        {
            CallFunctionSafe("onDialogMessage", message);
        }

        public override void OnBattleMessage(string message)
        {
            CallFunctionSafe("onBattleMessage", message);
        }

        public override void OnSystemMessage(string message)
        {
            CallFunctionSafe("onSystemMessage", message);
        }

        public override void OnLearningMove(string moveName, int pokemonIndex)
        {
            CallFunctionSafe("onLearningMove", moveName, pokemonIndex);
        }

        public override bool ExecuteNextAction()
        {
            if (Bot.Game.IsInBattle && Bot.AI.UseMandatoryAction())
            {
                return true;
            }

            string functionName = Bot.Game.IsInBattle ? "onBattleAction" : "onPathAction";

            _actionExecuted = false;
            try
            {
                CallFunction(functionName, true);
            }
            catch (ScriptRuntimeException ex)
            {
                throw new Exception(ex.DecoratedMessage, ex);
            }
            return _actionExecuted;
        }

        private void CreateLuaInstance()
        {
            _hookedFunctions = new Dictionary<string, IList<DynValue>>();

            _lua = new Script(CoreModules.Preset_SoftSandbox | CoreModules.LoadMethods);
            _lua.Options.ScriptLoader = new CustomScriptLoader(_path) { ModulePaths = new[] { "?.lua" } };
            _lua.Options.CheckThreadAccess = false;
            _lua.Globals["log"] = new Action<string>(Log);
            _lua.Globals["fatal"] = new Action<string>(Fatal);
            _lua.Globals["logout"] = new Action<string>(Logout);
            _lua.Globals["stringContains"] = new Func<string, string, bool>(StringContains);
            _lua.Globals["playSound"] = new Action<string>(PlaySound);
            _lua.Globals["registerHook"] = new Action<string, DynValue>(RegisterHook);

            // General conditions
            _lua.Globals["getPlayerX"] = new Func<int>(GetPlayerX);
            _lua.Globals["getPlayerY"] = new Func<int>(GetPlayerY);
            _lua.Globals["getAccountName"] = new Func<string>(GetAccountName);
            _lua.Globals["getMapName"] = new Func<string>(GetMapName);
            _lua.Globals["getPokedexOwned"] = new Func<int>(GetPokedexOwned);
            _lua.Globals["getPokedexSeen"] = new Func<int>(GetPokedexSeen);
            _lua.Globals["getPokedexEvolved"] = new Func<int>(GetPokedexEvolved);
            _lua.Globals["getTeamSize"] = new Func<int>(GetTeamSize);

            _lua.Globals["getPokemonId"] = new Func<int, int>(GetPokemonId);
            _lua.Globals["getPokemonName"] = new Func<int, string>(GetPokemonName);
            _lua.Globals["getPokemonHealth"] = new Func<int, int>(GetPokemonHealth);
            _lua.Globals["getPokemonHealthPercent"] = new Func<int, int>(GetPokemonHealthPercent);
            _lua.Globals["getPokemonMaxHealth"] = new Func<int, int>(GetPokemonMaxHealth);
            _lua.Globals["getPokemonLevel"] = new Func<int, int>(GetPokemonLevel);
            _lua.Globals["getPokemonTotalExperience"] = new Func<int, int>(GetPokemonTotalExperience);
            _lua.Globals["getPokemonRemainingExperience"] = new Func<int, int>(GetPokemonRemainingExperience);
            _lua.Globals["getPokemonStatus"] = new Func<int, string>(GetPokemonStatus);
            _lua.Globals["getPokemonForm"] = new Func<int, int>(GetPokemonForm);
            _lua.Globals["getPokemonHeldItem"] = new Func<int, string>(GetPokemonHeldItem);
            _lua.Globals["getPokemonUniqueId"] = new Func<int, int>(GetPokemonUniqueId);
            _lua.Globals["getRemainingPowerPoints"] = new Func<int, string, int>(GetRemainingPowerPoints);
            _lua.Globals["getPokemonMaxPowerPoints"] = new Func<int, int, int>(GetPokemonMaxPowerPoints);
            _lua.Globals["isPokemonShiny"] = new Func<int, bool>(IsPokemonShiny);
            _lua.Globals["getPokemonMoveName"] = new Func<int, int, string>(GetPokemonMoveName);
            _lua.Globals["getPokemonMoveAccuracy"] = new Func<int, int, int>(GetPokemonMoveAccuracy);
            _lua.Globals["getPokemonMovePower"] = new Func<int, int, int>(GetPokemonMovePower);
            _lua.Globals["getPokemonMoveType"] = new Func<int, int, string>(GetPokemonMoveType);
            _lua.Globals["getPokemonMoveDamageType"] = new Func<int, int, string>(GetPokemonMoveDamageType);
            _lua.Globals["getPokemonMoveStatus"] = new Func<int, int, bool>(GetPokemonMoveStatus);
            _lua.Globals["getPokemonNature"] = new Func<int, string>(GetPokemonNature);
            _lua.Globals["getPokemonAbility"] = new Func<int, string>(GetPokemonAbility);
            _lua.Globals["getPokemonEffortValue"] = new Func<int, string, int>(GetPokemonEffortValue);
            _lua.Globals["getPokemonIndividualValue"] = new Func<int, string, int>(GetPokemonIndividualValue);
            _lua.Globals["getPokemonHappiness"] = new Func<int, int>(GetPokemonHappiness);
            _lua.Globals["getPokemonRegion"] = new Func<int, string>(GetPokemonRegion);
            _lua.Globals["getPokemonOriginalTrainer"] = new Func<int, string>(GetPokemonOriginalTrainer);
            _lua.Globals["getPokemonGender"] = new Func<int, string>(GetPokemonGender);
            _lua.Globals["getPokemonType"] = new Func<int, string[]>(GetPokemonType);
            _lua.Globals["getDamageMultiplier"] = new Func<string, DynValue[], double>(GetDamageMultiplier);
            _lua.Globals["isPokemonUsable"] = new Func<int, bool>(IsPokemonUsable);
            _lua.Globals["getUsablePokemonCount"] = new Func<int>(GetUsablePokemonCount);
            _lua.Globals["hasMove"] = new Func<int, string, bool>(HasMove);
            _lua.Globals["getActiveBattlers"] = new Func<Dictionary<string, Dictionary<string, int>>>(GetActiveBattlers);
            _lua.Globals["getActiveDigSpots"] = new Func<List<Dictionary<string, int>>>(GetActiveDigSpots);
            _lua.Globals["getActiveHeadbuttTrees"] = new Func<List<Dictionary<string, int>>>(GetActiveHeadbuttTrees);
            _lua.Globals["getActiveBerryTrees"] = new Func<List<Dictionary<string, int>>>(GetActiveBerryTrees);
            _lua.Globals["getDiscoverableItems"] = new Func<List<Dictionary<string, int>>>(GetDiscoverableItems);
            _lua.Globals["getNpcData"] = new Func<List<Dictionary<string, string>>>(GetNpcData);

            _lua.Globals["hasItem"] = new Func<string, bool>(HasItem);
            _lua.Globals["getItemQuantity"] = new Func<string, int>(GetItemQuantity);
            _lua.Globals["hasItemId"] = new Func<int, bool>(HasItemId);
            _lua.Globals["getItemQuantityId"] = new Func<int, int>(GetItemQuantityID);

            _lua.Globals["hasPokemonInTeam"] = new Func<string, bool>(HasPokemonInTeam);
            _lua.Globals["isTeamSortedByLevelAscending"] = new Func<bool>(IsTeamSortedByLevelAscending);
            _lua.Globals["isTeamSortedByLevelDescending"] = new Func<bool>(IsTeamSortedByLevelDescending);
            _lua.Globals["isTeamRangeSortedByLevelAscending"] = new Func<int, int, bool>(IsTeamRangeSortedByLevelAscending);
            _lua.Globals["isTeamRangeSortedByLevelDescending"] = new Func<int, int, bool>(IsTeamRangeSortedByLevelDescending);
            _lua.Globals["isNpcVisible"] = new Func<string, bool>(IsNpcVisible);
            _lua.Globals["isNpcOnCell"] = new Func<int, int, bool>(IsNpcOnCell);
            _lua.Globals["isShopOpen"] = new Func<bool>(IsShopOpen);
            _lua.Globals["getMoney"] = new Func<int>(GetMoney);
            _lua.Globals["isMounted"] = new Func<bool>(IsMounted);
            _lua.Globals["isSurfing"] = new Func<bool>(IsSurfing);
            _lua.Globals["isPrivateMessageEnabled"] = new Func<bool>(IsPrivateMessageEnabled);
            _lua.Globals["getTime"] = new GetTimeDelegate(GetTime);
            _lua.Globals["isMorning"] = new Func<bool>(IsMorning);
            _lua.Globals["isNoon"] = new Func<bool>(IsNoon);
            _lua.Globals["isNight"] = new Func<bool>(IsNight);
            _lua.Globals["isOutside"] = new Func<bool>(IsOutside);
            _lua.Globals["isAutoEvolve"] = new Func<bool>(IsAutoEvolve);

            _lua.Globals["isCurrentPCBoxRefreshed"] = new Func<bool>(IsCurrentPCBoxRefreshed);
            _lua.Globals["getCurrentPCBoxId"] = new Func<int>(GetCurrentPCBoxId);
            _lua.Globals["isPCOpen"] = new Func<bool>(IsPCOpen);
            _lua.Globals["getCurrentPCBoxId"] = new Func<int>(GetCurrentPCBoxId);
            _lua.Globals["getCurrentPCBoxSize"] = new Func<int>(GetCurrentPCBoxSize);
            _lua.Globals["getPCBoxCount"] = new Func<int>(GetPCBoxCount);
            _lua.Globals["getPCPokemonCount"] = new Func<int>(GetPCPokemonCount);

            _lua.Globals["getPokemonIdFromPC"] = new Func<int, int, int>(GetPokemonIdFromPC);
            _lua.Globals["getPokemonNameFromPC"] = new Func<int, int, string>(GetPokemonNameFromPC);
            _lua.Globals["getPokemonHealthFromPC"] = new Func<int, int, int>(GetPokemonHealthFromPC);
            _lua.Globals["getPokemonHealthPercentFromPC"] = new Func<int, int, int>(GetPokemonHealthPercentFromPC);
            _lua.Globals["getPokemonMaxHealthFromPC"] = new Func<int, int, int>(GetPokemonMaxHealthFromPC);
            _lua.Globals["getPokemonLevelFromPC"] = new Func<int, int, int>(GetPokemonLevelFromPC);
            _lua.Globals["getPokemonTotalExperienceFromPC"] = new Func<int, int, int>(GetPokemonTotalExperienceFromPC);
            _lua.Globals["getPokemonRemainingExperienceFromPC"] = new Func<int, int, int>(GetPokemonRemainingExperienceFromPC);
            _lua.Globals["getPokemonStatusFromPC"] = new Func<int, int, string>(GetPokemonStatusFromPC);
            _lua.Globals["getPokemonTypeFromPC"] = new Func<int, int, string[]>(GetPokemonTypeFromPC);
            _lua.Globals["getPokemonHeldItemFromPC"] = new Func<int, int, string>(GetPokemonHeldItemFromPC);
            _lua.Globals["getPokemonUniqueIdFromPC"] = new Func<int, int, int>(GetPokemonUniqueIdFromPC);
            _lua.Globals["getPokemonRemainingPowerPointsFromPC"] = new Func<int, int, int, int>(GetPokemonRemainingPowerPointsFromPC);
            _lua.Globals["getPokemonMaxPowerPointsFromPC"] = new Func<int, int, int, int>(GetPokemonMaxPowerPointsFromPC);
            _lua.Globals["isPokemonFromPCShiny"] = new Func<int, int, bool>(IsPokemonFromPCShiny);
            _lua.Globals["getPokemonMoveNameFromPC"] = new Func<int, int, int, string>(GetPokemonMoveNameFromPC);
            _lua.Globals["getPokemonMoveAccuracyFromPC"] = new Func<int, int, int, int>(GetPokemonMoveAccuracyFromPC);
            _lua.Globals["getPokemonMovePowerFromPC"] = new Func<int, int, int, int>(GetPokemonMovePowerFromPC);
            _lua.Globals["getPokemonMoveTypeFromPC"] = new Func<int, int, int, string>(GetPokemonMoveTypeFromPC);
            _lua.Globals["getPokemonMoveDamageTypeFromPC"] = new Func<int, int, int, string>(GetPokemonMoveDamageTypeFromPC);
            _lua.Globals["getPokemonMoveStatusFromPC"] = new Func<int, int, int, bool>(GetPokemonMoveStatusFromPC);
            _lua.Globals["getPokemonNatureFromPC"] = new Func<int, int, string>(GetPokemonNatureFromPC);
            _lua.Globals["getPokemonAbilityFromPC"] = new Func<int, int, string>(GetPokemonAbilityFromPC);
            _lua.Globals["getPokemonEffortValueFromPC"] = new Func<int, int, string, int>(GetPokemonEffortValueFromPC);
            _lua.Globals["getPokemonIndividualValueFromPC"] = new Func<int, int, string, int>(GetPokemonIndividualValueFromPC);
            _lua.Globals["getPokemonHappinessFromPC"] = new Func<int, int, int>(GetPokemonHappinessFromPC);
            _lua.Globals["getPokemonRegionFromPC"] = new Func<int, int, string>(GetPokemonRegionFromPC);
            _lua.Globals["getPokemonOriginalTrainerFromPC"] = new Func<int, int, string>(GetPokemonOriginalTrainerFromPC);
            _lua.Globals["getPokemonGenderFromPC"] = new Func<int, int, string>(GetPokemonGenderFromPC);
            _lua.Globals["getPokemonFormFromPC"] = new Func<int, int, int>(GetPokemonFormFromPC);

            _lua.Globals["getServer"] = new Func<string>(GetServer);

            // Battle conditions
            _lua.Globals["isOpponentShiny"] = new Func<bool>(IsOpponentShiny);
            _lua.Globals["isAlreadyCaught"] = new Func<bool>(IsAlreadyCaught);
            _lua.Globals["isWildBattle"] = new Func<bool>(IsWildBattle);
            _lua.Globals["getActivePokemonNumber"] = new Func<int>(GetActivePokemonNumber);
            _lua.Globals["getOpponentId"] = new Func<int>(GetOpponentId);
            _lua.Globals["getOpponentName"] = new Func<string>(GetOpponentName);
            _lua.Globals["getOpponentHealth"] = new Func<int>(GetOpponentHealth);
            _lua.Globals["getOpponentHealthPercent"] = new Func<int>(GetOpponentHealthPercent);
            _lua.Globals["getOpponentLevel"] = new Func<int>(GetOpponentLevel);
            _lua.Globals["getOpponentStatus"] = new Func<string>(GetOpponentStatus);
            _lua.Globals["getOpponentForm"] = new Func<int>(GetOpponentForm);
            _lua.Globals["isOpponentEffortValue"] = new Func<string, bool>(IsOpponentEffortValue);
            _lua.Globals["getOpponentEffortValue"] = new Func<string, int>(GetOpponentEffortValue);
            _lua.Globals["getOpponentType"] = new Func<string[]>(GetOpponentType);

            // Path actions
            _lua.Globals["moveToCell"] = new Func<int, int, bool>(MoveToCell);
            _lua.Globals["moveToMap"] = new Func<string, bool>(MoveToMap);
            _lua.Globals["moveToRectangle"] = new Func<DynValue[], bool>(MoveToRectangle);

            _lua.Globals["moveToNormalGround"] = new Func<bool>(MoveToNormalGround);
            _lua.Globals["moveToGrass"] = new Func<bool>(MoveToGrass);
            _lua.Globals["moveToWater"] = new Func<bool>(MoveToWater);
            _lua.Globals["moveNearExit"] = new Func<string, bool>(MoveNearExit);
            _lua.Globals["talkToNpc"] = new Func<string, bool>(TalkToNpc);
            _lua.Globals["talkToNpcOnCell"] = new Func<int, int, bool>(TalkToNpcOnCell);
            _lua.Globals["usePokecenter"] = new Func<bool>(UsePokecenter);
            _lua.Globals["swapPokemon"] = new Func<int, int, bool>(SwapPokemon);
            _lua.Globals["swapPokemonWithLeader"] = new Func<string, bool>(SwapPokemonWithLeader);
            _lua.Globals["sortTeamByLevelAscending"] = new Func<bool>(SortTeamByLevelAscending);
            _lua.Globals["sortTeamByLevelDescending"] = new Func<bool>(SortTeamByLevelDescending);
            _lua.Globals["sortTeamRangeByLevelAscending"] = new Func<int, int, bool>(SortTeamRangeByLevelAscending);
            _lua.Globals["sortTeamRangeByLevelDescending"] = new Func<int, int, bool>(SortTeamRangeByLevelDescending);
            _lua.Globals["buyItem"] = new Func<string, int, bool>(BuyItem);
            _lua.Globals["usePC"] = new Func<bool>(UsePC);
            _lua.Globals["openPCBox"] = new Func<int, bool>(OpenPCBox);
            _lua.Globals["depositPokemonToPC"] = new Func<int, bool>(DepositPokemonToPC);
            _lua.Globals["withdrawPokemonFromPC"] = new Func<int, int, bool>(WithdrawPokemonFromPC);
            _lua.Globals["swapPokemonFromPC"] = new Func<int, int, int, bool>(SwapPokemonFromPC);
            _lua.Globals["giveItemToPokemon"] = new Func<string, int, bool>(GiveItemToPokemon);
            _lua.Globals["takeItemFromPokemon"] = new Func<int, bool>(TakeItemFromPokemon);
            _lua.Globals["releasePokemonFromTeam"] = new Func<int, bool>(ReleasePokemonFromTeam);
            _lua.Globals["releasePokemonFromPC"] = new Func<int, int, bool>(ReleasePokemonFromPC);
            _lua.Globals["enablePrivateMessage"] = new Func<bool>(EnablePrivateMessage);
            _lua.Globals["disablePrivateMessage"] = new Func<bool>(DisablePrivateMessage);
            _lua.Globals["enableAutoEvolve"] = new Func<bool>(EnableAutoEvolve);
            _lua.Globals["disableAutoEvolve"] = new Func<bool>(DisableAutoEvolve);

            // Path functions
            _lua.Globals["pushDialogAnswer"] = new Action<DynValue>(PushDialogAnswer);

            // General actions
            _lua.Globals["useItem"] = new Func<string, bool>(UseItem);
            _lua.Globals["useItemOnPokemon"] = new Func<string, int, bool>(UseItemOnPokemon);

            // Battle actions
            _lua.Globals["attack"] = new Func<bool>(Attack);
            _lua.Globals["weakAttack"] = new Func<bool>(WeakAttack);
            _lua.Globals["run"] = new Func<bool>(Run);
            _lua.Globals["sendUsablePokemon"] = new Func<bool>(SendUsablePokemon);
            _lua.Globals["sendAnyPokemon"] = new Func<bool>(SendAnyPokemon);
            _lua.Globals["sendPokemon"] = new Func<int, bool>(SendPokemon);
            _lua.Globals["useMove"] = new Func<string, bool>(UseMove);

            // Move learning actions
            _lua.Globals["forgetMove"] = new Func<string, bool>(ForgetMove);
            _lua.Globals["forgetAnyMoveExcept"] = new Func<DynValue[], bool>(ForgetAnyMoveExcept);
            
            // Custom option slider functions
            _lua.Globals["setOption"] = new Action<int, bool>(SetOption);
            _lua.Globals["getOption"] = new Func<int, bool>(GetOption);
            _lua.Globals["setOptionName"] = new Action<int, string>(SetOptionName);
            _lua.Globals["setOptionDescription"] = new Action<int, string>(SetOptionDescription);

            // Custom text option functions
            _lua.Globals["setTextOption"] = new Action<int, string>(SetTextOption);
            _lua.Globals["getTextOption"] = new Func<int, string>(GetTextOption);
            _lua.Globals["setTextOptionName"] = new Action<int, string>(SetTextOptionName);
            _lua.Globals["setTextOptionDescription"] = new Action<int, string>(SetTextOptionDescription);

            // File editing actions
            _lua.Globals["logToFile"] = new Action<string, DynValue, bool>(LogToFile);
            _lua.Globals["readLinesFromFile"] = new Func<string, string[]>(ReadLinesFromFile);
	    
            _lua.Globals["login"] = new Action<string, string, string, int, string, int, string, string>(Login);
            _lua.Globals["invoke"] = new Action<DynValue, float, DynValue[]>(Invoke);
            _lua.Globals["startScript"] = new Func<bool>(StartScript);

            foreach (string content in _libsContent)
            {
                CallContent(content);
            }
            CallContent(_content);
        }

        private void CallFunctionSafe(string functionName, params object[] args)
        {
            try
            {
                try
                {
                    CallFunction(functionName, false, args);
                }
                catch (ScriptRuntimeException ex)
                {
                    throw new Exception(ex.DecoratedMessage, ex);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Fatal("Error during the execution of '" + functionName + "': " + ex);
#else
                Fatal("Error during the execution of '" + functionName + "': " + ex.Message);
#endif
            }
        }

        private void CallContent(string content)
        {
            try
            {
                TaskUtils.CallActionWithTimeout(() => _lua.DoString(content), delegate
                {
                    throw new Exception("The execution of the script timed out.");
                }, TimeoutDelay);
            }
            catch (SyntaxErrorException ex)
            {
                throw new Exception(ex.DecoratedMessage, ex);
            }
        }

        private void CallFunction(string functionName, bool isPathAction, params object[] args)
        {
            if (_hookedFunctions.ContainsKey(functionName))
            {
                foreach (DynValue function in _hookedFunctions[functionName])
                {
                    CallDynValueFunction(function, "hook:" + functionName, args);
                    if (isPathAction && _actionExecuted) return;
                }
            }
            CallDynValueFunction(_lua.Globals.Get(functionName), functionName, args);
        }

        private void CallDynValueFunction(DynValue function, string functionName, params object[] args)
        {
            if (function.Type != DataType.Function) return;
            TaskUtils.CallActionWithTimeout(() => _lua.Call(function, args), delegate
            {
                Fatal("The execution of the script timed out (" + functionName + ").");
            }, TimeoutDelay);
        }

        private bool ValidateAction(string source, bool inBattle)
        {
            if (_actionExecuted)
            {
                Fatal("error: " + source + ": the script can only execute one action per frame.");
                return false;
            }
            if (Bot.Game.IsInBattle != inBattle)
            {
                if (inBattle)
                {
                    Fatal("error: " + source + " you cannot execute a battle action while not in a battle.");
                }
                else
                {
                    Fatal("error: " + source + " you cannot execute a path action while in a battle.");
                }
                return false;
            }
            return true;
        }

        private bool ExecuteAction(bool result)
        {
            if (result)
            {
                _actionExecuted = true;
            }
            return result;
        }

        // API: Displays the specified message to the message log.
        private void Log(string message)
        {
            LogMessage(message);
        }

        // API: Displays the specified message to the message log and stop the bot.
        private void Fatal(string message)
        {
            LogMessage(message);
            Bot.Stop();
        }

        // API: Displays the specified message to the message log and logs out.
        private void Logout(string message)
        {
            LogMessage(message);
            Bot.Stop();
            Bot.Logout(false);
        }

        // API return an array of all NPCs that can be challenged on the current map. format : {"npcName" = {"x" = x, "y" = y}}
        private Dictionary<string, Dictionary<string, int>> GetActiveBattlers()
        {
            var activeBattlers = new Dictionary<string, Dictionary<string, int>>();
            foreach (Npc npc in Bot.Game.Map.Npcs.Where(npc => npc.CanBattle))
            {
                var npcData = new Dictionary<string, int>();
                npcData["x"] = npc.PositionX;
                npcData["y"] = npc.PositionY;
                activeBattlers[npc.Name] = npcData;
            }
            return activeBattlers;
        }

        // API return an array of all usable Dig Spots on the currrent map. format : {index = {"x" = x, "y" = y}}
        private List<Dictionary<string, int>> GetActiveDigSpots()
        {
            var digSpots = new List<Dictionary<string, int>>();
            foreach (Npc npc in Bot.Game.Map.Npcs.Where(npc => npc.Type == 70 || npc.Type == 71))
            {
                var npcData = new Dictionary<string, int>();
                npcData["x"] = npc.PositionX;
                npcData["y"] = npc.PositionY;
                digSpots.Add(npcData);
            }
            return digSpots;
        }

        // API return an array of all usable Headbutt trees on the currrent map. format : {index = {"x" = x, "y" = y}}
        private List<Dictionary<string, int>> GetActiveHeadbuttTrees()
        {
            var trees = new List<Dictionary<string, int>>();
            foreach (Npc npc in Bot.Game.Map.Npcs.Where(npc => npc.Type == 101))
            {
                var npcData = new Dictionary<string, int>();
                npcData["x"] = npc.PositionX;
                npcData["y"] = npc.PositionY;
                trees.Add(npcData);
            }
            return trees;
        }

        // API return an array of all harvestable berry trees on the currrent map. format : {index = {"x" = x, "y" = y}}
        private List<Dictionary<string, int>> GetActiveBerryTrees()
        {
            int[] berries = { 42, 43, 44, 45, 49, 50, 51, 52, 55, 56, 57, 58, 59, 60, 61, 62 };
            var trees = new List<Dictionary<string, int>>();
            foreach (Npc npc in Bot.Game.Map.Npcs.Where(npc => Array.Exists(berries, e => e == npc.Type)))
            {
                var npcData = new Dictionary<string, int>();
                npcData["x"] = npc.PositionX;
                npcData["y"] = npc.PositionY;
                trees.Add(npcData);
            }
            return trees;
        }

        // API return an array of all discoverable items on the currrent map. format : {index = {"x" = x, "y" = y}}
        private List<Dictionary<string, int>> GetDiscoverableItems()
        {
            var items = new List<Dictionary<string, int>>();
            foreach (Npc npc in Bot.Game.Map.Npcs.Where(npc => npc.Type == 11 && npc.LosLength < 100))
            {
                var npcData = new Dictionary<string, int>();
                npcData["x"] = npc.PositionX;
                npcData["y"] = npc.PositionY;
                items.Add(npcData);
            }
            return items;
        }

        // API return npc data on current map, format : { { "x" = x , "y" = y, "type" = type }, {...}, ... }
        private List<Dictionary<string, string>> GetNpcData()
        {
            var lNpc = new List<Dictionary<string, string>>();
            foreach (Npc npc in Bot.Game.Map.Npcs)
            {
                var npcData = new Dictionary<string, string>();
                npcData["x"] = npc.PositionX.ToString();
                npcData["y"] = npc.PositionY.ToString();
                npcData["type"] = npc.Type.ToString();
                npcData["name"] = npc.Name;
                npcData["isBattler"] = npc.IsBattler.ToString();
                npcData["id"] = npc.Id.ToString();
                npcData["los"] = npc.LosLength.ToString();
                lNpc.Add(npcData);
            }
            return lNpc;
        }

        // API: Returns true if the string contains the specified part, ignoring the case.
        private bool StringContains(string haystack, string needle)
        {
            return haystack.ToUpperInvariant().Contains(needle.ToUpperInvariant());
        }

        // API: Returns playing a custom sound.
        private void PlaySound(string file)
        {
            if (File.Exists(file))
            {
                using (SoundPlayer player = new SoundPlayer(file))
                {
                    player.Play();
                }
            };
        }

        // API: Calls the specified function when the specified event occurs.
        private void RegisterHook(string eventName, DynValue callback)
        {
            if (callback.Type != DataType.Function)
            {
                Fatal("error: registerHook: the callback must be a function.");
                return;
            }
            if (!_hookedFunctions.ContainsKey(eventName))
            {
                _hookedFunctions.Add(eventName, new List<DynValue>());
            }
            _hookedFunctions[eventName].Add(callback);
        }

        // API: Returns the X-coordinate of the current cell.
        private int GetPlayerX()
        {
            return Bot.Game.PlayerX;
        }

        // API: Returns the Y-coordinate of the current cell.
        private int GetPlayerY()
        {
            return Bot.Game.PlayerY;
        }

        // API: Returns current account name.
        private string GetAccountName()
        {
            return Bot.Account.Name;
        }

        // API: Returns the name of the current map.
        private string GetMapName()
        {
            return Bot.Game.MapName;
        }

        // API: Returns Owned Entry of the pokedex
        private int GetPokedexOwned()
        {
            return Bot.Game.PokedexOwned;
        }

        // API: Returns Seen Entry of the pokedex
        private int GetPokedexSeen()
        {
            return Bot.Game.PokedexSeen;
        }

        // API: Returns Evolved Entry of the pokedex
        private int GetPokedexEvolved()
        {
            return Bot.Game.PokedexEvolved;
        }

        // API: Returns the amount of pokémon in the team.
        private int GetTeamSize()
        {
            return Bot.Game.Team.Count;
        }

        // API: Returns the ID of the specified pokémon in the team.
        private int GetPokemonId(int index)
        {
            if (index < 1 || index > Bot.Game.Team.Count)
            {
                Fatal("error: getPokemonId: tried to retrieve the non-existing pokemon " + index + ".");
                return 0;
            }
            return Bot.Game.Team[index - 1].Id;
        }

        // API: Returns the name of the specified pokémon in the team.
        private string GetPokemonName(int index)
        {
            if (index < 1 || index > Bot.Game.Team.Count)
            {
                Fatal("error: getPokemonName: tried to retrieve the non-existing pokemon " + index + ".");
                return null;
            }
            return Bot.Game.Team[index - 1].Name;
        }

        // API: PROShine unique ID of the pokemon of the current box matching the ID.
        private int GetPokemonUniqueId(int pokemonUid)
        {
            if (pokemonUid < 1 || pokemonUid > Bot.Game.Team.Count)
            {
                Fatal("error: getPokemonUniqueId: tried to retrieve the non-existing pokemon " + pokemonUid + ".");
                return -1;
            }
            PokemonStats iv = Bot.Game.Team[pokemonUid - 1].IV;

            // Converting a base 31 to 10
            // The odds of having twice the same pokemon unique ID being
            // 1 against 887,503,680
            int uniqueId = (iv.Attack - 1);
            uniqueId += (iv.Defence - 1) * (int)Math.Pow(31, 1);
            uniqueId += (iv.Speed - 1) * (int)Math.Pow(31, 2);
            uniqueId += (iv.SpAttack - 1) * (int)Math.Pow(31, 3);
            uniqueId += (iv.SpDefence - 1) * (int)Math.Pow(31, 4);
            uniqueId += (iv.Health - 1) * (int)Math.Pow(31, 5);
            return uniqueId;
        }

        // API: Returns the current health of the specified pokémon in the team.
        private int GetPokemonHealth(int index)
        {
            if (index < 1 || index > Bot.Game.Team.Count)
            {
                Fatal("error: getPokemonHealth: tried to retrieve the non-existing pokemon " + index + ".");
                return 0;
            }
            return Bot.Game.Team[index - 1].CurrentHealth;
        }

        // API: Returns the percentage of remaining health of the specified pokémon in the team.
        private int GetPokemonHealthPercent(int index)
        {
            if (index < 1 || index > Bot.Game.Team.Count)
            {
                Fatal("error: getPokemonHealthPercent: tried to retrieve the non-existing pokemon " + index + ".");
                return 0;
            }
            Pokemon pokemon = Bot.Game.Team[index - 1];
            return pokemon.CurrentHealth * 100 / pokemon.MaxHealth;
        }

        // API: Returns the maximum health of the specified pokémon in the team.
        private int GetPokemonMaxHealth(int index)
        {
            if (index < 1 || index > Bot.Game.Team.Count)
            {
                Fatal("error: getPokemonMaxHealth: tried to retrieve the non-existing pokemon " + index + ".");
                return 0;
            }
            Pokemon pokemon = Bot.Game.Team[index - 1];
            return pokemon.MaxHealth;
        }

        // API: Returns the shyniness of the specified pokémon in the team.
        private bool IsPokemonShiny(int index)
        {
            if (index < 1 || index > Bot.Game.Team.Count)
            {
                Fatal("error: isPokemonShiny: tried to retrieve the non-existing pokemon " + index + ".");
                return false;
            }
            Pokemon pokemon = Bot.Game.Team[index - 1];
            return pokemon.IsShiny;
        }

        // API: Returns the move of the specified pokémon in the team at the specified index.
        private string GetPokemonMoveName(int index, int moveId)
        {
            if (index < 1 || index > Bot.Game.Team.Count)
            {
                Fatal("error: getPokemonMove: tried to retrieve the non-existing pokemon " + index + ".");
                return null;
            }
            if (moveId < 1 || moveId > 4)
            {
                Fatal("error: getPokemonMove: tried to access an impossible move #" + moveId + ".");
                return null;
            }
            Pokemon pokemon = Bot.Game.Team[index - 1];
            return pokemon.Moves[moveId - 1].Name;
        }

        // API: Returns the move accuracy of the specified pokémon in the team at the specified index.
        private int GetPokemonMoveAccuracy(int index, int moveId)
        {
            if (index < 1 || index > Bot.Game.Team.Count)
            {
                Fatal("error: getPokemonMoveAccuracy: tried to retrieve the non-existing pokemon " + index + ".");
                return -1;
            }
            if (moveId < 1 || moveId > 4)
            {
                Fatal("error: getPokemonMoveAccuracy: tried to access an impossible move #" + moveId + ".");
                return -1;
            }
            Pokemon pokemon = Bot.Game.Team[index - 1];
            return pokemon.Moves[moveId - 1].Data.Accuracy;
        }

        // API: Returns the move power of the specified pokémon in the team at the specified index.
        private int GetPokemonMovePower(int index, int moveId)
        {
            if (index < 1 || index > Bot.Game.Team.Count)
            {
                Fatal("error: getPokemonMovePower: tried to retrieve the non-existing pokemon " + index + ".");
                return -1;
            }
            if (moveId < 1 || moveId > 4)
            {
                Fatal("error: getPokemonMovePower: tried to access an impossible move #" + moveId + ".");
                return -1;
            }
            Pokemon pokemon = Bot.Game.Team[index - 1];
            return pokemon.Moves[moveId - 1].Data.Power;
        }

        // API: Returns the move type of the specified pokémon in the team at the specified index.
        private string GetPokemonMoveType(int index, int moveId)
        {
            if (index < 1 || index > Bot.Game.Team.Count)
            {
                Fatal("error: getPokemonMoveType: tried to retrieve the non-existing pokemon " + index + ".");
                return null;
            }
            if (moveId < 1 || moveId > 4)
            {
                Fatal("error: getPokemonMoveType: tried to access an impossible move #" + moveId + ".");
                return null;
            }
            Pokemon pokemon = Bot.Game.Team[index - 1];
            return pokemon.Moves[moveId - 1].Data.Type.ToString();
        }

        // API: Returns the move damage type of the specified pokémon in the team at the specified index.
        private string GetPokemonMoveDamageType(int index, int moveId)
        {
            if (index < 1 || index > Bot.Game.Team.Count)
            {
                Fatal("error: getPokemonMoveDamageType: tried to retrieve the non-existing pokemon " + index + ".");
                return null;
            }
            if (moveId < 1 || moveId > 4)
            {
                Fatal("error: getPokemonMoveDamageType: tried to access an impossible move #" + moveId + ".");
                return null;
            }
            Pokemon pokemon = Bot.Game.Team[index - 1];
            return pokemon.Moves[moveId - 1].Data.DamageType.ToString();
        }

        // API: Returns true if the move of the specified pokémon in the team at the specified index can apply a status .
        private bool GetPokemonMoveStatus(int index, int moveId)
        {
            if (index < 1 || index > Bot.Game.Team.Count)
            {
                Fatal("error: getPokemonMoveStatus: tried to retrieve the non-existing pokemon " + index + ".");
                return false;
            }
            if (moveId < 1 || moveId > 4)
            {
                Fatal("error: getPokemonMoveStatus: tried to access an impossible move #" + moveId + ".");
                return false;
            }
            Pokemon pokemon = Bot.Game.Team[index - 1];
            return pokemon.Moves[moveId - 1].Data.Status;
        }

        // API: Max move PP of the pokemon of the current box matching the ID.
        private int GetPokemonMaxPowerPoints(int index, int moveId)
        {
            if (index < 1 || index > Bot.Game.Team.Count)
            {
                Fatal("error: getPokemonMove: tried to retrieve the non-existing pokemon " + index + ".");
                return -1;
            }
            if (moveId < 1 || moveId > 4)
            {
                Fatal("error: getPokemonMove: tried to access an impossible move #" + moveId + ".");
                return -1;
            }
            Pokemon pokemon = Bot.Game.Team[index - 1];
            return pokemon.Moves[moveId - 1].MaxPoints;
        }

        // API: Nature of the pokemon of the current box matching the ID.
        private string GetPokemonNature(int index)
        {
            if (index < 1 || index > Bot.Game.Team.Count)
            {
                Fatal("error: getPokemonNature: tried to retrieve the non-existing pokemon " + index + ".");
                return null;
            }
            Pokemon pokemon = Bot.Game.Team[index - 1];
            return pokemon.Nature.Name;
        }

        // API: Ability of the pokemon of the current box matching the ID.
        private string GetPokemonAbility(int index)
        {
            if (index < 1 || index > Bot.Game.Team.Count)
            {
                Fatal("error: getPokemonAbility: tried to retrieve the non-existing pokemon " + index + ".");
                return null;
            }
            Pokemon pokemon = Bot.Game.Team[index - 1];
            return pokemon.Ability.Name;
        }

        // API: Returns the experience total of a pokemon level.
        private int GetPokemonTotalExperience(int index)
        {
            if (index < 1 || index > Bot.Game.Team.Count)
            {
                Fatal("error: getPokemonTotalXP: tried to retrieve the non-existing pokemon " + index + ".");
                return 0;
            }
            Pokemon pokemon = Bot.Game.Team[index - 1];
            return pokemon.Experience.TotalLevelExperience;
        }

        // API: Returns the remaining experience of a pokemon before next level.
        private int GetPokemonRemainingExperience(int index)
        {
            if (index < 1 || index > Bot.Game.Team.Count)
            {
                Fatal("error: getPokemonRemainingXP: tried to retrieve the non-existing pokemon " + index + ".");
                return 0;
            }
            Pokemon pokemon = Bot.Game.Team[index - 1];
            return pokemon.Experience.RemainingExperience;
        }

        // API: Returns the level of the specified pokémon in the team.
        private int GetPokemonLevel(int index)
        {
            if (index < 1 || index > Bot.Game.Team.Count)
            {
                Fatal("error: getPokemonLevel: tried to retrieve the non-existing pokemon " + index + ".");
                return 0;
            }
            return Bot.Game.Team[index - 1].Level;
        }

        // API: Returns the happiness of the specified pokémon in the team.
        private int GetPokemonHappiness(int index)
        {
            if (index < 1 || index > Bot.Game.Team.Count)
            {
                Fatal("error: getPokemonHappiness: tried to retrieve the non-existing pokemon " + index + ".");
                return -1;
            }
            return Bot.Game.Team[index - 1].Happiness;
        }

        // API: Returns the region of capture of the specified pokémon in the team.
        private string GetPokemonRegion(int index)
        {
            if (index < 1 || index > Bot.Game.Team.Count)
            {
                Fatal("error: getPokemonRegion: tried to retrieve the non-existing pokemon " + index + ".");
                return null;
            }
            return Bot.Game.Team[index - 1].Region.ToString();
        }

        // API: Returns the original trainer of the specified pokémon in the team.
        private string GetPokemonOriginalTrainer(int index)
        {
            if (index < 1 || index > Bot.Game.Team.Count)
            {
                Fatal("error: getPokemonOriginalTrainer: tried to retrieve the non-existing pokemon " + index + ".");
                return null;
            }
            return Bot.Game.Team[index - 1].OriginalTrainer;
        }

        // API: Returns the gender of the specified pokémon in the team.
        private string GetPokemonGender(int index)
        {
            if (index < 1 || index > Bot.Game.Team.Count)
            {
                Fatal("error: getPokemonGender: tried to retrieve the non-existing pokemon " + index + ".");
                return null;
            }
            return Bot.Game.Team[index - 1].Gender;
        }

        // API: Returns the type of the specified pokémon in the team as an array of length 2.
        private string[] GetPokemonType(int index)
        {
            if (index < 1 || index > Bot.Game.Team.Count)
            {
                Fatal("error: getPokemonType: tried to retrieve the non-existing pokemon " + index + ".");
                return null;
            }

            int id = Bot.Game.Team[index - 1].Id;

            if (id <= 0 || id >= TypesManager.Instance.Type1.Count())
            {
                return new string[] { "Unknown", "Unknown" };
            }

            return new string[] { TypesManager.Instance.Type1[id].ToString(), TypesManager.Instance.Type2[id].ToString() };
        }

        private string[] _types = { "", "NORMAL", "FIGHTING", "FLYING", "POISON", "GROUND", "ROCK", "BUG", "GHOST", "STEEL", "FIRE", "WATER", "GRASS", "ELECTRIC", "PSYCHIC", "ICE", "DRAGON", "DARK", "FAIRY" };

        // API: Returns the multiplier of the damage type between an attacking type and one or two defending types.
        private double GetDamageMultiplier(string attacker, params DynValue[] defender)
        {
            if (defender[0].Type == DataType.Table)
            {
                if (defender[0].Table.Length == 1)
                {
                    defender = new DynValue[] { defender[0].Table.Values.ToArray()[0], DynValue.NewString("") };
                }
                else
                {
                    defender = defender[0].Table.Values.ToArray();
                }
            }            
            else if (defender.Length == 1)
            {
                defender = new DynValue[] { defender[0], DynValue.NewString("") };
            }

            if (attacker.ToUpperInvariant() == "NONE")
                attacker = "";

            if (defender[0].CastToString().ToUpperInvariant() == "NONE")
                defender[0] = DynValue.NewString("");

            if (defender[1].CastToString().ToUpperInvariant() == "NONE")
                defender[1] = DynValue.NewString("");

            if (!Array.Exists(_types, e => e == attacker.ToUpperInvariant()))
            {
                Fatal("error: getDamageMultiplier: the damage type '" + attacker + "' does not exist.");
                return -1;
            }
            
            if (!Array.Exists(_types, e => e == defender[0].CastToString().ToUpperInvariant()))
            {
                Fatal("error: getDamageMultiplier: the damage type '" + defender[0].CastToString() + "' does not exist.");
                return -1;
            }
            
            if (!Array.Exists(_types, e => e == defender[1].CastToString().ToUpperInvariant()))
            {
                Fatal("error: getDamageMultiplier: the damage type '" + defender[1].CastToString() + "' does not exist.");
                return -1;
            }
            
            double power = 1;
            
            power *= TypesManager.Instance.GetMultiplier(PokemonTypeExtensions.FromName(attacker), PokemonTypeExtensions.FromName(defender[0].CastToString()));
            power *= TypesManager.Instance.GetMultiplier(PokemonTypeExtensions.FromName(attacker), PokemonTypeExtensions.FromName(defender[1].CastToString()));

            return power;
        }

        // API: Returns the status of the specified pokémon in the team.
        private string GetPokemonStatus(int index)
        {
            if (index < 1 || index > Bot.Game.Team.Count)
            {
                Fatal("error: getPokemonStatus: tried to retrieve the non-existing pokemon " + index + ".");
                return null;
            }
            return Bot.Game.Team[index - 1].Status;
        }

        // API: Returns the form of the specified pokémon in the team (0 if no form).
        private int GetPokemonForm(int index)
        {
            if (index < 1 || index > Bot.Game.Team.Count)
            {
                Fatal("error: getPokemonForm: tried to retrieve the non-existing pokemon " + index + ".");
                return 0;
            }
            return Bot.Game.Team[index - 1].Form;
        }

        // API: Returns the item held by the specified pokemon in the team, null if empty.
        private string GetPokemonHeldItem(int index)
        {
            if (index < 1 || index > Bot.Game.Team.Count)
            {
                Fatal("error: getPokemonHeldItem: tried to retrieve the non-existing pokemon " + index + ".");
                return null;
            }
            string itemHeld = Bot.Game.Team[index - 1].ItemHeld;
            return itemHeld == string.Empty ? null : itemHeld;
        }

        // API: Returns true if the specified pokémon has is alive and has an offensive attack available.
        private bool IsPokemonUsable(int index)
        {
            if (index < 1 || index > Bot.Game.Team.Count)
            {
                Fatal("error: isPokemonUsable: tried to retrieve the non-existing pokemon " + index + ".");
                return false;
            }
            return Bot.AI.IsPokemonUsable(Bot.Game.Team[index - 1]);
        }

        // API: Returns the amount of usable pokémon in the team.
        private int GetUsablePokemonCount()
        {
            return Bot.AI.UsablePokemonsCount;
        }

        // API: Returns true if the specified pokémon has a move with the specified name.
        private bool HasMove(int pokemonIndex, string moveName)
        {
            if (pokemonIndex < 1 || pokemonIndex > Bot.Game.Team.Count)
            {
                Fatal("error: hasMove: tried to retrieve the non-existing pokemon " + pokemonIndex + ".");
                return false;
            }

            return Bot.Game.PokemonUidHasMove(pokemonIndex, moveName.ToUpperInvariant());
        }

        // API: Returns the remaining power points of the specified move of the specified pokémon in the team.
        private int GetRemainingPowerPoints(int pokemonIndex, string moveName)
        {
            if (pokemonIndex < 1 || pokemonIndex > Bot.Game.Team.Count)
            {
                Fatal("error: getRemainingPowerPoints: tried to retrieve the non-existing pokémon " + pokemonIndex + ".");
                return 0;
            }

            moveName = moveName.ToUpperInvariant();
            PokemonMove move = Bot.Game.Team[pokemonIndex - 1].Moves.FirstOrDefault(m => MovesManager.Instance.GetMoveData(m.Id)?.Name.ToUpperInvariant() == moveName);
            if (move == null)
            {
                Fatal("error: getRemainingPowerPoints: the pokémon " + pokemonIndex + " does not have a move called '" + moveName + "'.");
                return 0;
            }

            return move.CurrentPoints;
        }

        // API: Returns the effort value for the specified stat of the specified pokémon in the team.
        private int GetPokemonEffortValue(int pokemonIndex, string statType)
        {
            if (pokemonIndex < 1 || pokemonIndex > Bot.Game.Team.Count)
            {
                Fatal("error: getPokemonEffortValue: tried to retrieve the non-existing pokémon " + pokemonIndex + ".");
                return 0;
            }

            if (!_stats.ContainsKey(statType.ToUpperInvariant()))
            {
                Fatal("error: getPokemonEffortValue: the stat '" + statType + "' does not exist.");
                return 0;
            }

            return Bot.Game.Team[pokemonIndex - 1].EV.GetStat(_stats[statType.ToUpperInvariant()]);
        }

        // API: Returns the individual value for the specified stat of the specified pokémon in the team.
        private int GetPokemonIndividualValue(int pokemonIndex, string statType)
        {
            if (pokemonIndex < 1 || pokemonIndex > Bot.Game.Team.Count)
            {
                Fatal("error: getPokemonIndividualValue: tried to retrieve the non-existing pokémon " + pokemonIndex + ".");
                return 0;
            }

            if (!_stats.ContainsKey(statType.ToUpperInvariant()))
            {
                Fatal("error: getPokemonIndividualValue: the stat '" + statType + "' does not exist.");
                return 0;
            }

            return Bot.Game.Team[pokemonIndex - 1].IV.GetStat(_stats[statType.ToUpperInvariant()]);
        }

        // API: Returns true if the specified item is in the inventory.
        private bool HasItem(string itemName)
        {
            return Bot.Game.HasItemName(itemName.ToUpperInvariant());
        }

        // API: Returns the quantity of the specified item in the inventory.
        private int GetItemQuantity(string itemName)
        {
            return Bot.Game.GetItemFromName(itemName.ToUpperInvariant())?.Quantity ?? 0;
        }

        // API: Returns true if the specified item is in the inventory.
        private bool HasItemId(int itemid)
        {
            return Bot.Game.HasItemId(itemid);
        }
        // API: Returns the quantity of the specified item in the inventory.
        private int GetItemQuantityID(int itemid)
        {
            return Bot.Game.GetItemFromId(itemid)?.Quantity ?? 0;
        }

        // API: Returns true if the specified pokémon is present in the team.
        private bool HasPokemonInTeam(string pokemonName)
        {
            return Bot.Game.HasPokemonInTeam(pokemonName.ToUpperInvariant());
        }

        // API: Returns true if the team is sorted by level in ascending order.
        private bool IsTeamSortedByLevelAscending()
        {
            return IsTeamSortedByLevel(true, 1, 6);
        }

        // API: Returns true if the team is sorted by level in descending order.
        private bool IsTeamSortedByLevelDescending()
        {
            return IsTeamSortedByLevel(false, 1, 6);
        }

        // API: Returns true if the specified part of the team is sorted by level in ascending order.
        private bool IsTeamRangeSortedByLevelAscending(int fromIndex, int toIndex)
        {
            return IsTeamSortedByLevel(true, fromIndex, toIndex);
        }

        // API: Returns true if the specified part of the team the team is sorted by level in descending order.
        private bool IsTeamRangeSortedByLevelDescending(int fromIndex, int toIndex)
        {
            return IsTeamSortedByLevel(false, fromIndex, toIndex);
        }

        private bool IsTeamSortedByLevel(bool ascending, int from, int to)
        {
            from = Math.Max(from, 1);
            to = Math.Min(to, Bot.Game.Team.Count);

            int level = ascending ? 0 : int.MaxValue;
            for (int i = from - 1; i < to; ++i)
            {
                Pokemon pokemon = Bot.Game.Team[i];
                if (ascending && pokemon.Level < level) return false;
                if (!ascending && pokemon.Level > level) return false;
                level = pokemon.Level;
            }
            return true;
        }

        // API: Returns true if there is a visible NPC with the specified name on the map.
        private bool IsNpcVisible(string npcName)
        {
            npcName = npcName.ToUpperInvariant();
            return Bot.Game.Map.Npcs.Any(npc => npc.Name.ToUpperInvariant() == npcName);
        }

        // API: Returns true if there is a visible NPC the specified coordinates.
        private bool IsNpcOnCell(int cellX, int cellY)
        {
            return Bot.Game.Map.Npcs.Any(npc => npc.PositionX == cellX && npc.PositionY == cellY);
        }

        // API: Returns true if there is a shop opened.
        private bool IsShopOpen()
        {
            return Bot.Game.OpenedShop != null;
        }

        // API: Returns the amount of money in the inventory.
        private int GetMoney()
        {
            return Bot.Game.Money;
        }

        // API: Returns true if the player is riding a mount or the bicycle.
        private bool IsMounted()
        {
            return Bot.Game.IsBiking;
        }

        // API: Returns true if the player is surfing
        private bool IsSurfing()
        {
            return Bot.Game.IsSurfing;
        }

        // API: Returns true if the opponent pokémon is shiny.
        private bool IsOpponentShiny()
        {
            if (!Bot.Game.IsInBattle)
            {
                Fatal("error: isOpponentShiny is only usable in battle.");
                return false;
            }
            return Bot.Game.ActiveBattle.IsShiny;
        }

        // API: Returns true if the opponent pokémon has already been caught and has a pokédex entry.
        private bool IsAlreadyCaught()
        {
            if (!Bot.Game.IsInBattle)
            {
                Fatal("error: isAlreadyCaught is only usable in battle.");
                return false;
            }
            return Bot.Game.ActiveBattle.AlreadyCaught;
        }

        // API: Returns true if the current battle is against a wild pokémon.
        private bool IsWildBattle()
        {
            if (!Bot.Game.IsInBattle)
            {
                Fatal("error: isWildBattle is only usable in battle.");
                return false;
            }
            return Bot.Game.ActiveBattle.IsWild;
        }

        // API: Returns the index of the active team pokémon in the current battle.
        private int GetActivePokemonNumber()
        {
            if (!Bot.Game.IsInBattle)
            {
                Fatal("error: getActivePokemonNumber is only usable in battle.");
                return 0;
            }
            return Bot.Game.ActiveBattle.SelectedPokemonIndex + 1;
        }

        // API: Returns the id of the opponent pokémon in the current battle.
        private int GetOpponentId()
        {
            if (!Bot.Game.IsInBattle)
            {
                Fatal("error: getOpponentId can only be used in battle.");
                return 0;
            }
            return Bot.Game.ActiveBattle.OpponentId;
        }

        // API: Returns the name of the opponent pokémon in the current battle.
        private string GetOpponentName()
        {
            if (!Bot.Game.IsInBattle)
            {
                Fatal("error: getOpponentName can only be used in battle.");
                return null;
            }
            return PokemonNamesManager.Instance.Names[Bot.Game.ActiveBattle.OpponentId];
        }

        // API: Returns the current health of the opponent pokémon in the current battle.
        private int GetOpponentHealth()
        {
            if (!Bot.Game.IsInBattle)
            {
                Fatal("error: getOpponentHealth can only be used in battle.");
                return 0;
            }
            return Bot.Game.ActiveBattle.CurrentHealth;
        }

        // API: Returns the percentage of remaining health of the opponent pokémon in the current battle.
        private int GetOpponentHealthPercent()
        {
            if (!Bot.Game.IsInBattle)
            {
                Fatal("error: getOpponentHealthPercent can only be used in battle.");
                return 0;
            }
            return Bot.Game.ActiveBattle.CurrentHealth * 100 / Bot.Game.ActiveBattle.OpponentHealth;
        }

        // API: Returns the level of the opponent pokémon in the current battle.
        private int GetOpponentLevel()
        {
            if (!Bot.Game.IsInBattle)
            {
                Fatal("error: getOpponentLevel can only be used in battle.");
                return 0;
            }
            return Bot.Game.ActiveBattle.OpponentLevel;
        }

        // API: Returns the status of the opponent pokémon in the current battle.
        private string GetOpponentStatus()
        {
            if (!Bot.Game.IsInBattle)
            {
                Fatal("error: getOpponentStatus can only be used in battle.");
                return null;
            }
            return Bot.Game.ActiveBattle.OpponentStatus;
        }

        // API: Returns the form of the opponent pokémon in the current battle (0 if no form).
        private int GetOpponentForm()
        {
            if (!Bot.Game.IsInBattle)
            {
                Fatal("error: getOpponentForm can only be used in battle.");
                return 0;
            }
            return Bot.Game.ActiveBattle.AlternateForm;
        }

        private static Dictionary<string, StatType> _stats = new Dictionary<string, StatType>()
        {
            { "HP", StatType.Health },
            { "HEALTH", StatType.Health },
            { "ATK", StatType.Attack },
            { "ATTACK", StatType.Attack },
            { "DEF", StatType.Defence },
            { "DEFENCE", StatType.Defence },
            { "DEFENSE", StatType.Defence },
            { "SPATK", StatType.SpAttack },
            { "SPATTACK", StatType.SpAttack },
            { "SPDEF", StatType.SpDefence },
            { "SPDEFENCE", StatType.SpDefence },
            { "SPDEFENSE", StatType.SpDefence },
            { "SPD", StatType.Speed },
            { "SPEED", StatType.Speed }
        };

        // API: Returns true if the opponent is only giving the specified effort value.
        private bool IsOpponentEffortValue(string statType)
        {
            if (!Bot.Game.IsInBattle)
            {
                Fatal("error: isOpponentEffortValue can only be used in battle.");
                return false;
            }
            if (!_stats.ContainsKey(statType.ToUpperInvariant()))
            {
                Fatal("error: isOpponentEffortValue: the stat '" + statType + "' does not exist.");
                return false;
            }
            if (!EffortValuesManager.Instance.BattleValues.ContainsKey(Bot.Game.ActiveBattle.OpponentId))
            {
                return false;
            }

            PokemonStats stats = EffortValuesManager.Instance.BattleValues[Bot.Game.ActiveBattle.OpponentId];
            return stats.HasOnly(_stats[statType.ToUpperInvariant()]);
        }

        // API: Returns the amount of a particular EV given by the opponent.
        private int GetOpponentEffortValue(string statType)
        {
            if (!Bot.Game.IsInBattle)
            {
                Fatal("error: getOpponentEffortValue can only be used in battle.");
                return -1;
            }
            if (!_stats.ContainsKey(statType.ToUpperInvariant()))
            {
                Fatal("error: getOpponentEffortValue: the stat '" + statType + "' does not exist.");
                return -1;
            }
            if (!EffortValuesManager.Instance.BattleValues.ContainsKey(Bot.Game.ActiveBattle.OpponentId))
            {
                return -1;
            }

            PokemonStats stats = EffortValuesManager.Instance.BattleValues[Bot.Game.ActiveBattle.OpponentId];
            return stats.GetStat(_stats[statType.ToUpperInvariant()]);
        }
        
        // API: Returns the type of the opponent pokémon in the current battle as an array of length 2.
        private string[] GetOpponentType()
        {
            if (!Bot.Game.IsInBattle)
            {
                Fatal("error: getOpponentType can only be used in battle.");
                return null;
            }

            int id = Bot.Game.ActiveBattle.OpponentId;	    

            if (id <= 0 || id >= TypesManager.Instance.Type1.Count())
            {
                return new string[] { "Unknown", "Unknown" };
            }
	    
            return new string[] { TypesManager.Instance.Type1[id].ToString(), TypesManager.Instance.Type2[id].ToString() };
        }

        // API: Moves to the specified coordinates.
        private bool MoveToCell(int x, int y)
        {
            if (!ValidateAction("moveToCell", false)) return false;

            return ExecuteAction(Bot.MoveToCell(x, y));
        }

        // API: Moves to the nearest cell teleporting to the specified map.
        private bool MoveToMap(string mapName)
        {
            if (!ValidateAction("moveToMap", false)) return false;

            return ExecuteAction(Bot.MoveToLink(mapName.ToUpperInvariant()));
        }

        // API: Moves to a random accessible cell of the specified rectangle.
        private bool MoveToRectangle(params DynValue[] values)
        {
            if (values.Length != 1 && values.Length != 4 ||
                (values.Length == 1 && values[0].Type != DataType.Table) ||
                (values.Length == 4
                    && (values[0].Type != DataType.Number || values[1].Type != DataType.Number
                    || values[2].Type != DataType.Number || values[3].Type != DataType.Number)))
            {
                Fatal("error: moveToRectangle: must receive either a table or four numbers.");
                return false;
            }
            if (values.Length == 1)
            {
                values = values[0].Table.Values.ToArray();
            }
            return MoveToRectangle((int)values[0].Number, (int)values[1].Number, (int)values[2].Number, (int)values[3].Number);
        }

        // API: Moves to a random accessible cell of the specified rectangle.
        private bool MoveToRectangle(int minX, int minY, int maxX, int maxY)
        {
            if (!ValidateAction("moveToRectangle", false)) return false;

            if (minX > maxX || minY > maxY)
            {
                Fatal("error: moveToRectangle: the maximum cell cannot be less than the minimum cell.");
                return false;
            }

            int x;
            int y;
            int tries = 0;
            do
            {
                if (++tries > 100) return false;
                x = Bot.Game.Rand.Next(minX, maxX + 1);
                y = Bot.Game.Rand.Next(minY, maxY + 1);
            } while (x == Bot.Game.PlayerX && y == Bot.Game.PlayerY);

            return ExecuteAction(Bot.MoveToCell(x, y));
        }

        // API: Move randomly avoiding water and links.
        private bool MoveToNormalGround()
        {
            if (!ValidateAction("moveToNormalGround", false)) return false;

            return ExecuteAction(MoveToCellType((x, y) => Bot.Game.Map.IsNormalGround(x, y)));
        }

        // API: Moves to the nearest grass patch then move randomly inside it.
        private bool MoveToGrass()
        {
            if (!ValidateAction("moveToGrass", false)) return false;

            return ExecuteAction(MoveToCellType((x, y) => Bot.Game.Map.IsGrass(x, y)));
        }

        // API: Moves to the nearest water area then move randomly inside it.
        private bool MoveToWater()
        {
            if (!ValidateAction("moveToWater", false)) return false;

            return ExecuteAction(MoveToCellType((x, y) => Bot.Game.Map.IsWater(x, y)));
        }

        private bool MoveToCellType(Func<int, int, bool> cellTypePredicate)
        {
            bool alreadyInCell = cellTypePredicate(Bot.Game.PlayerX, Bot.Game.PlayerY);

            List<Tuple<int, int, int>> cells = new List<Tuple<int, int, int>>();

            for (int x = 0; x < Bot.Game.Map.Width; ++x)
            {
                for (int y = 0; y < Bot.Game.Map.Height; ++y)
                {
                    if (cellTypePredicate(x, y) && (x != Bot.Game.PlayerX || y != Bot.Game.PlayerY))
                    {
                        int distance = Bot.Game.DistanceTo(x, y);
                        cells.Add(new Tuple<int, int, int>(x, y, distance));
                    }
                }
            }

            List<Tuple<int, int, int>> trash = new List<Tuple<int, int, int>>();
            if (alreadyInCell)
            {
                foreach (var cell in cells)
                {
                    if (cell.Item3 >= 10)
                    {
                        trash.Add(cell);
                    }
                }
            }
            else
            {
                int minDistance = -1;
                foreach (var cell in cells)
                {
                    if (minDistance == -1 || cell.Item3 < minDistance)
                    {
                        minDistance = cell.Item3;
                    }
                }
                foreach (var cell in cells)
                {
                    if (cell.Item3 > minDistance + 5)
                    {
                        trash.Add(cell);
                    }
                }
            }
            while (trash.Count > 0)
            {
                cells.Remove(trash[0]);
                trash.RemoveAt(0);
            }

            if (cells.Count > 0)
            {
                var randomCell = cells[Bot.Game.Rand.Next(cells.Count)];
                return Bot.MoveToCell(randomCell.Item1, randomCell.Item2);
            }
            return false;
        }

        // API: Moves near the cell teleporting to the specified map.
        private bool MoveNearExit(string mapName)
        {
            if (!ValidateAction("moveNearExit", false)) return false;

            Tuple<int, int> nearest = Bot.Game.Map.GetNearestLinks(mapName.ToUpperInvariant(), Bot.Game.PlayerX, Bot.Game.PlayerY).First();
            if (nearest == null)
            {
                Fatal("error: moveNearExit: could not find the exit '" + mapName + "'.");
                return false;
            }

            int x, y;
            int tries = 0;
            do
            {
                x = Bot.Game.Rand.Next(-10, 10) + nearest.Item1;
                y = Bot.Game.Rand.Next(-10, 10) + nearest.Item2;
                if (++tries > 100) return false;
            } while (x <= 0 || y <= 0 || !Bot.Game.Map.IsNormalGround(x, y) || (x == Bot.Game.PlayerX && y == Bot.Game.PlayerY));

            return ExecuteAction(Bot.MoveToCell(x, y));
        }

        // API: Moves then talk to NPC specified by its name.
        private bool TalkToNpc(string npcName)
        {
            if (!ValidateAction("talkToNpc", false)) return false;

            npcName = npcName.ToUpperInvariant();
            Npc target = Bot.Game.Map.Npcs.FirstOrDefault(npc => npc.Name.ToUpperInvariant() == npcName);
            if (target == null)
            {
                Fatal("error: talkToNpc: could not find the NPC '" + npcName + "'.");
                return false;
            }

            return ExecuteAction(Bot.TalkToNpc(target));
        }

        // API: Moves then talk to NPC located on the specified cell.
        private bool TalkToNpcOnCell(int cellX, int cellY)
        {
            if (!ValidateAction("talkToNpcOnCell", false)) return false;

            Npc target = Bot.Game.Map.Npcs.FirstOrDefault(npc => npc.PositionX == cellX && npc.PositionY == cellY);
            if (target == null)
            {
                Fatal("error: talkToNpcOnCell: could not find any NPC on the cell [" + cellX + "," + cellY + "].");
                return false;
            }

            return ExecuteAction(Bot.TalkToNpc(target));
        }

        // API: Moves to the Nurse Joy then talk to the cell below her.
        private bool UsePokecenter()
        {
            if (!ValidateAction("usePokecenter", false)) return false;

            Npc nurse = Bot.Game.Map.Npcs.FirstOrDefault(npc => npc.Name.StartsWith("Nurse"));
            if (nurse == null)
            {
                Fatal("error: usePokecenter: could not find the Nurse Joy.");
                return false;
            }
            Npc target = Bot.Game.Map.Npcs.FirstOrDefault(npc => npc.PositionX == nurse.PositionX && npc.PositionY == nurse.PositionY + 1);
            if (target == null)
            {
                Fatal("error: usePokecenter: could not find the entity below the Nurse Joy.");
                return false;
            }

            return ExecuteAction(Bot.TalkToNpc(target));
        }

        // API: Swaps the two pokémon specified by their position in the team.
        private bool SwapPokemon(int index1, int index2)
        {
            if (!ValidateAction("swapPokemon", false)) return false;

            return ExecuteAction(Bot.Game.SwapPokemon(index1, index2));
        }

        // API: Swaps the first pokémon with the specified name with the leader of the team.
        private bool SwapPokemonWithLeader(string pokemonName)
        {
            if (!ValidateAction("swapPokemonWithLeader", false)) return false;

            Pokemon pokemon = Bot.Game.FindFirstPokemonInTeam(pokemonName.ToUpperInvariant());
            if (pokemon == null)
            {
                Fatal("error: swapPokemonWithLeader: there is no pokémon '" + pokemonName + "' in the team.");
                return false;
            }
            if (pokemon.Uid == 1)
            {
                Fatal("error: swapPokemonWithLeader: '" + pokemonName + "' is already the leader of the team.");
                return false;
            }

            return ExecuteAction(Bot.Game.SwapPokemon(1, pokemon.Uid));
        }

        // API: Sorts the pokémon in the team by level in ascending order, one pokémon at a time.
        private bool SortTeamByLevelAscending()
        {
            if (!ValidateAction("sortTeamByLevelAscending", false)) return false;

            return ExecuteAction(SortTeamByLevel(true, 1, 6));
        }

        // API: Sorts the pokémon in the team by level in descending order, one pokémon at a time.
        private bool SortTeamByLevelDescending()
        {
            if (!ValidateAction("sortTeamByLevelDescending", false)) return false;

            return ExecuteAction(SortTeamByLevel(false, 1, 6));
        }

        // API: Sorts the specified part of the team by level in ascending order, one pokémon at a time.
        private bool SortTeamRangeByLevelAscending(int fromIndex, int toIndex)
        {
            if (!ValidateAction("sortTeamRangeByLevelAscending", false)) return false;

            return ExecuteAction(SortTeamByLevel(true, fromIndex, toIndex));
        }

        // API: Sorts the specified part of the team by level in descending order, one pokémon at a time.
        private bool SortTeamRangeByLevelDescending(int fromIndex, int toIndex)
        {
            if (!ValidateAction("sortTeamRangeByLevelDescending", false)) return false;

            return ExecuteAction(SortTeamByLevel(false, fromIndex, toIndex));
        }

        private bool SortTeamByLevel(bool ascending, int from, int to)
        {
            from = Math.Max(from, 1);
            to = Math.Min(to, Bot.Game.Team.Count);

            for (int i = from - 1; i < to - 1; ++i)
            {
                int currentIndex = i;
                int currentLevel = Bot.Game.Team[i].Level;
                for (int j = i + 1; j < to; ++j)
                {
                    if ((ascending && Bot.Game.Team[j].Level < currentLevel) ||
                        (!ascending && Bot.Game.Team[j].Level > currentLevel))
                    {
                        currentIndex = j;
                        currentLevel = Bot.Game.Team[j].Level;
                    }
                }

                if (currentIndex != i)
                {
                    Bot.Game.SwapPokemon(i + 1, currentIndex + 1);
                    return true;
                }
            }
            return false;
        }

        // API: Return the state Auto Evolve
        private bool IsAutoEvolve()
        {
            return Bot.PokemonEvolver.IsEnabled;
        }

        // API: Enable auto evolve on PrO Shine client.
        private bool EnableAutoEvolve()
        {
            Bot.PokemonEvolver.IsEnabled = true;
            return Bot.PokemonEvolver.IsEnabled;
        }

        // API: Disable auto evolve on PrO Shine client.
        private bool DisableAutoEvolve()
        {
            Bot.PokemonEvolver.IsEnabled = false;
            return !Bot.PokemonEvolver.IsEnabled;
        }

        // API: Check if the private message from normal users are blocked.
        private bool IsPrivateMessageEnabled()
        {
            return Bot.Game.IsPrivateMessageOn;
        }

        // API: Enable private messages from users.
        private bool EnablePrivateMessage()
        {
            return ExecuteAction(Bot.Game.PrivateMessageOn());
        }

        // API: Disable private messages from users.
        private bool DisablePrivateMessage()
        {
            return ExecuteAction(Bot.Game.PrivateMessageOff());
        }

        private delegate int GetTimeDelegate(out int minute);

        // API: Return the current in game hour and minute.
        private int GetTime(out int minute)
        {
            DateTime dt = Convert.ToDateTime(Bot.Game.PokemonTime);
            minute = dt.Minute;
            return dt.Hour;
        }

        // API: Return true if morning time.
        private bool IsMorning()
        {
            DateTime dt = Convert.ToDateTime(Bot.Game.PokemonTime);
            if (dt.Hour >= 4 && dt.Hour < 10)
            {
                return true;
            }
            return false;
        }

        // API: Return true if noon time.
        private bool IsNoon()
        {
            DateTime dt = Convert.ToDateTime(Bot.Game.PokemonTime);
            if (dt.Hour >= 10 && dt.Hour < 20)
            {
                return true;
            }
            return false;
        }

        // API: Return true if night time.
        private bool IsNight()
        {
            DateTime dt = Convert.ToDateTime(Bot.Game.PokemonTime);
            if (dt.Hour >= 20 || dt.Hour < 4)
            {
                return true;
            }
            return false;
        }

        // API: Return true if the character is outside.
        private bool IsOutside()
        {
            return Bot.Game.Map.IsOutside;
        }

        // API: Check if the PC is open. Moving close the PC, usePC() opens it.
        private bool IsPCOpen()
        {
            return Bot.Game.IsPCOpen;
        }

        // API: Move to the PC and opens it, refreshing the first box.
        private bool UsePC()
        {
            if (!ValidateAction("usePc", false)) return false;

            return ExecuteAction(Bot.OpenPC());
        }

        // API: Open box from the PC.
        private bool OpenPCBox(int boxId)
        {
            if (!ValidateAction("openPCBox", false)) return false;

            if (!Bot.Game.IsPCOpen)
            {
                Fatal("error: openPCBox: tried to open box #" + boxId + " while the PC is closed.");
            }
            return ExecuteAction(Bot.Game.RefreshPCBox(boxId));
        }

        // API: Withdraw a pokemon from a known box.
        private bool WithdrawPokemonFromPC(int boxId, int boxPokemonId)
        {
            if (!ValidateAction("withdrawPokemonFromPC", false)) return false;

            if (!IsPCAccessValid("withdrawPokemonFromPC", boxId, boxPokemonId))
            {
                return false;
            }

            if (Bot.Game.WithdrawPokemonFromPC(boxId, boxPokemonId))
            {
                return ExecuteAction(Bot.Game.RefreshPCBox(boxId));
            }
            return false;
        }

        // API: Deposit a pokemon to the pc.
        private bool DepositPokemonToPC(int pokemonUid)
        {
            if (!ValidateAction("depositPokemonToPC", false)) return false;

            if (Bot.Game.DepositPokemonToPC(pokemonUid))
            {
                return ExecuteAction(Bot.Game.RefreshCurrentPCBox());
            }
            return false;
        }

        // API: Swap a pokemon from the team with a pokemon from the pc.
        private bool SwapPokemonFromPC(int boxId, int boxPokemonId, int pokemonUid)
        {
            if (!ValidateAction("swapPokemonFromPC", false)) return false;

            if (!IsPCAccessValid("swapPokemonFromPC", boxId, boxPokemonId))
            {
                return false;
            }

            if (Bot.Game.SwapPokemonFromPC(boxId, boxPokemonId, pokemonUid))
            {
                return ExecuteAction(Bot.Game.RefreshCurrentPCBox());
            }
            return false;
        }

        // API: Get the active PC Box.
        private int GetCurrentPCBoxId()
        {
            if (!Bot.Game.IsPCOpen)
            {
                return -1;
            }
            return Bot.Game.CurrentPCBoxId;
        }

        // API: Return the number of non-empty boxes in the PC
        private int GetPCBoxCount()
        {
            // The PCGreatestUid is only known after the first box refresh
            if (!Bot.Game.IsPCOpen || Bot.Game.PCGreatestUid == -1 || Bot.Game.IsPCBoxRefreshing)
            {
                return -1;
            }
            return Bot.Game.GetBoxIdFromPokemonUid(Bot.Game.PCGreatestUid);
        }

        // API: Return the number of pokemon in the PC
        private int GetPCPokemonCount()
        {
            // The PCGreatestUid is only known after the first box refresh
            if (!Bot.Game.IsPCOpen || Bot.Game.PCGreatestUid == -1 || Bot.Game.IsPCBoxRefreshing)
            {
                return -1;
            }
            return Bot.Game.PCGreatestUid - 7;
        }

        // API: Is the currentPcBox refreshed yet?
        private bool IsCurrentPCBoxRefreshed()
        {
            if (!Bot.Game.IsPCOpen || Bot.Game.IsPCBoxRefreshing)
            {
                return false;
            }
            return true;
        }

        // API: Current box size.
        private int GetCurrentPCBoxSize()
        {
            if (!Bot.Game.IsPCOpen || Bot.Game.IsPCBoxRefreshing)
            {
                return -1;
            }
            return Bot.Game.CurrentPCBox.Count;
        }

        private bool IsPCAccessValid(string functionName, int boxId, int boxPokemonId)
        {
            if (!Bot.Game.IsPCOpen)
            {
                Fatal("error: " + functionName + ": tried to access box #" + boxId + " while the PC is closed.");
                return false;
            }
            if (Bot.Game.IsPCBoxRefreshing)
            {
                Fatal("error: " + functionName + ": tried to access box #" + boxId + " while the box is refreshing.");
                return false;
            }
            if (boxId != Bot.Game.CurrentPCBoxId)
            {
                Fatal("error: " + functionName + ": tried to access box #" + boxId + " different from the currently loaded box.");
                return false;
            }
            if (boxPokemonId < 1 || boxPokemonId > Bot.Game.CurrentPCBox.Count)
            {
                Fatal("error: " + functionName + ": tried to access the unknown pokemon #" + boxPokemonId + " of the box #" + boxId + ".");
                return false;
            }
            return true;
        }

        // API: Name of the pokemon of the current box matching the ID.
        private string GetPokemonNameFromPC(int boxId, int boxPokemonId)
        {
            if (!IsPCAccessValid("getPokemonNameFromPC", boxId, boxPokemonId))
            {
                return null;
            }
            return Bot.Game.CurrentPCBox[boxPokemonId - 1].Name;
        }

        // API: Pokedex ID of the pokemon of the current box matching the ID.
        private int GetPokemonIdFromPC(int boxId, int boxPokemonId)
        {
            if (!IsPCAccessValid("getPokemonNationalIdFromPC", boxId, boxPokemonId))
            {
                return -1;
            }
            return Bot.Game.CurrentPCBox[boxPokemonId - 1].Id;
        }

        // API: PROShine custom unique ID of the pokemon of the current box matching the ID.
        private int GetPokemonUniqueIdFromPC(int boxId, int boxPokemonId)
        {
            if (!IsPCAccessValid("getPokemonUniqueIdFromPC", boxId, boxPokemonId))
            {
                return -1;
            }
            PokemonStats iv = Bot.Game.CurrentPCBox[boxPokemonId - 1].IV;

            // Converting a base 31 to 10
            // The odds of having twice the same pokemon unique ID being
            // 1 against 887,503,680
            int uniqueId = (iv.Attack - 1);
            uniqueId += (iv.Defence - 1) * (int)Math.Pow(31, 1);
            uniqueId += (iv.Speed - 1) * (int)Math.Pow(31, 2);
            uniqueId += (iv.SpAttack - 1) * (int)Math.Pow(31, 3);
            uniqueId += (iv.SpDefence - 1) * (int)Math.Pow(31, 4);
            uniqueId += (iv.Health - 1) * (int)Math.Pow(31, 5);
            return uniqueId;
        }

        // API: Current HP of the pokemon of the current box matching the ID.
        private int GetPokemonHealthFromPC(int boxId, int boxPokemonId)
        {
            if (!IsPCAccessValid("getPokemonCurrentHealthFromPC", boxId, boxPokemonId))
            {
                return -1;
            }
            return Bot.Game.CurrentPCBox[boxPokemonId - 1].CurrentHealth;
        }

        // API: Returns the percentage of remaining health of the specified pokémon in the team.
        private int GetPokemonHealthPercentFromPC(int boxId, int boxPokemonId)
        {
            if (!IsPCAccessValid("getPokemonCurrentHealthPercentFromPC", boxId, boxPokemonId))
            {
                return -1;
            }
            Pokemon pokemon = Bot.Game.CurrentPCBox[boxPokemonId - 1];
            return pokemon.CurrentHealth * 100 / pokemon.MaxHealth;
        }

        // API: Max HP of the pokemon of the current box matching the ID.
        private int GetPokemonMaxHealthFromPC(int boxId, int boxPokemonId)
        {
            if (!IsPCAccessValid("getPokemonMaxHealthFromPC", boxId, boxPokemonId))
            {
                return -1;
            }
            return Bot.Game.CurrentPCBox[boxPokemonId - 1].MaxHealth;
        }

        // API: Level of the pokemon of the current box matching the ID.
        private int GetPokemonLevelFromPC(int boxId, int boxPokemonId)
        {
            if (!IsPCAccessValid("getPokemonMaxHealthFromPC", boxId, boxPokemonId))
            {
                return -1;
            }
            return Bot.Game.CurrentPCBox[boxPokemonId - 1].Level;
        }

        // API: Total of experience cost of a level for the pokemon of the current box matching the ID.
        private int GetPokemonTotalExperienceFromPC(int boxId, int boxPokemonId)
        {
            if (!IsPCAccessValid("getPokemonTotalXPFromPC", boxId, boxPokemonId))
            {
                return -1;
            }
            return Bot.Game.CurrentPCBox[boxPokemonId - 1].Experience.TotalLevelExperience;
        }

        // API: Remaining experience before the next level of the pokemon of the current box matching the ID.
        private int GetPokemonRemainingExperienceFromPC(int boxId, int boxPokemonId)
        {
            if (!IsPCAccessValid("getPokemonRemainingXPFromPC", boxId, boxPokemonId))
            {
                return -1;
            }
            return Bot.Game.CurrentPCBox[boxPokemonId - 1].Experience.RemainingExperience;
        }

        // API: Shyniness of the pokemon of the current box matching the ID.
        private bool IsPokemonFromPCShiny(int boxId, int boxPokemonId)
        {
            if (!IsPCAccessValid("isPokemonFromPCShiny", boxId, boxPokemonId))
            {
                return false;
            }
            return Bot.Game.CurrentPCBox[boxPokemonId - 1].IsShiny;
        }

        // API: Move of the pokemon of the current box matching the ID.
        private string GetPokemonMoveNameFromPC(int boxId, int boxPokemonId, int moveId)
        {
            if (!IsPCAccessValid("getPokemonMoveNameFromPC", boxId, boxPokemonId))
            {
                return null;
            }
            if (moveId < 1 || moveId > 4)
            {
                Fatal("error: getPokemonMoveNameFromPC: tried to access an impossible move #" + moveId + ".");
                return null;
            }
            return Bot.Game.CurrentPCBox[boxPokemonId - 1].Moves[moveId - 1].Name;
        }

        // API: Returns the move accuracy of the specified pokémon in the box at the specified index.
        private int GetPokemonMoveAccuracyFromPC(int boxId, int boxPokemonId, int moveId)
        {
            if (!IsPCAccessValid("getPokemonMoveAccuracyFromPC", boxId, boxPokemonId))
            {
                return -1;
            }
            if (moveId < 1 || moveId > 4)
            {
                Fatal("error: getPokemonMoveAccuracyFromPC: tried to access an impossible move #" + moveId + ".");
                return -1;
            }
            return Bot.Game.CurrentPCBox[boxPokemonId - 1].Moves[moveId - 1].Data.Accuracy;
        }

        // API: Returns the move power of the specified pokémon in the box at the specified index.
        private int GetPokemonMovePowerFromPC(int boxId, int boxPokemonId, int moveId)
        {
            if (!IsPCAccessValid("getPokemonMovePowerFromPC", boxId, boxPokemonId))
            {
                return -1;
            }
            if (moveId < 1 || moveId > 4)
            {
                Fatal("error: getPokemonMovePowerFromPC: tried to access an impossible move #" + moveId + ".");
                return -1;
            }
            return Bot.Game.CurrentPCBox[boxPokemonId - 1].Moves[moveId - 1].Data.Power;
        }

        // API: Returns the move type of the specified pokémon in the box at the specified index.
        private string GetPokemonMoveTypeFromPC(int boxId, int boxPokemonId, int moveId)
        {
            if (!IsPCAccessValid("getPokemonMoveTypeFromPC", boxId, boxPokemonId))
            {
                return null;
            }
            if (moveId < 1 || moveId > 4)
            {
                Fatal("error: getPokemonMoveTypeFromPC: tried to access an impossible move #" + moveId + ".");
                return null;
            }
            return Bot.Game.CurrentPCBox[boxPokemonId - 1].Moves[moveId - 1].Data.Type.ToString();
        }

        // API: Returns the move damage type of the specified pokémon in the box at the specified index.
        private string GetPokemonMoveDamageTypeFromPC(int boxId, int boxPokemonId, int moveId)
        {
            if (!IsPCAccessValid("getPokemonMoveDamageTypeFromPC", boxId, boxPokemonId))
            {
                return null;
            }
            if (moveId < 1 || moveId > 4)
            {
                Fatal("error: getPokemonMoveDamageTypeFromPC: tried to access an impossible move #" + moveId + ".");
                return null;
            }
            return Bot.Game.CurrentPCBox[boxPokemonId - 1].Moves[moveId - 1].Data.DamageType.ToString();
        }

        // API: Returns true if the move of the specified pokémon in the box at the specified index can apply a status .
        private bool GetPokemonMoveStatusFromPC(int boxId, int boxPokemonId, int moveId)
        {
            if (!IsPCAccessValid("getPokemonMoveStatusTypeFromPC", boxId, boxPokemonId))
            {
                return false;
            }
            if (moveId < 1 || moveId > 4)
            {
                Fatal("error: getPokemonMoveStatusTypeFromPC: tried to access an impossible move #" + moveId + ".");
                return false;
            }
            return Bot.Game.CurrentPCBox[boxPokemonId - 1].Moves[moveId - 1].Data.Status;
        }

        // API: Current move PP of the pokemon of the current box matching the ID.
        private int GetPokemonRemainingPowerPointsFromPC(int boxId, int boxPokemonId, int moveId)
        {
            if (!IsPCAccessValid("getPokemonMoveCurrentPPFromPC", boxId, boxPokemonId))
            {
                return -1;
            }
            if (moveId < 1 || moveId > 4)
            {
                Fatal("error: getPokemonMoveCurrentPPFromPC: tried to access an impossible move #" + moveId + ".");
                return -1;
            }
            return Bot.Game.CurrentPCBox[boxPokemonId - 1].Moves[moveId - 1].CurrentPoints;
        }

        // API: Max move PP of the pokemon of the current box matching the ID.
        private int GetPokemonMaxPowerPointsFromPC(int boxId, int boxPokemonId, int moveId)
        {
            if (!IsPCAccessValid("getPokemonMoveMaxPPFromPC", boxId, boxPokemonId))
            {
                return -1;
            }
            if (moveId < 1 || moveId > 4)
            {
                Fatal("error: getPokemonMoveMaxPPFromPC: tried to access an impossible move #" + moveId + ".");
                return -1;
            }
            return Bot.Game.CurrentPCBox[boxPokemonId - 1].Moves[moveId - 1].MaxPoints;
        }

        // API: Nature of the pokemon of the current box matching the ID.
        private string GetPokemonNatureFromPC(int boxId, int boxPokemonId)
        {
            if (!IsPCAccessValid("getPokemonNatureFromPC", boxId, boxPokemonId))
            {
                return null;
            }
            return Bot.Game.CurrentPCBox[boxPokemonId - 1].Nature.Name;
        }

        // API: Ability of the pokemon of the current box matching the ID.
        private string GetPokemonAbilityFromPC(int boxId, int boxPokemonId)
        {
            if (!IsPCAccessValid("getPokemonAbilityFromPC", boxId, boxPokemonId))
            {
                return null;
            }
            return Bot.Game.CurrentPCBox[boxPokemonId - 1].Ability.Name;
        }

        // API: Returns the effort value for the specified stat of the specified pokémon in the team.
        private int GetPokemonEffortValueFromPC(int boxId, int boxPokemonId, string statType)
        {
            if (!IsPCAccessValid("getPokemonEffortValueFromPC", boxId, boxPokemonId))
            {
                return -1;
            }

            if (!_stats.ContainsKey(statType.ToUpperInvariant()))
            {
                Fatal("error: getPokemonEffortValueFromPC: the stat '" + statType + "' does not exist.");
                return 0;
            }

            return Bot.Game.CurrentPCBox[boxPokemonId - 1].EV.GetStat(_stats[statType.ToUpperInvariant()]);
        }

        // API: Returns the individual value for the specified stat of the specified pokémon in the PC.
        private int GetPokemonIndividualValueFromPC(int boxId, int boxPokemonId, string statType)
        {
            if (!IsPCAccessValid("getPokemonIndividualValueFromPC", boxId, boxPokemonId))
            {
                return -1;
            }

            if (!_stats.ContainsKey(statType.ToUpperInvariant()))
            {
                Fatal("error: getPokemonIndividualValueFromPC: the stat '" + statType + "' does not exist.");
                return 0;
            }

            return Bot.Game.CurrentPCBox[boxPokemonId - 1].IV.GetStat(_stats[statType.ToUpperInvariant()]);
        }

        // API: Happiness of the pokemon of the current box matching the ID.
        private int GetPokemonHappinessFromPC(int boxId, int boxPokemonId)
        {
            if (!IsPCAccessValid("getPokemonHappinessFromPC", boxId, boxPokemonId))
            {
                return -1;
            }
            return Bot.Game.CurrentPCBox[boxPokemonId - 1].Happiness;
        }

        // API: Region of capture of the pokemon of the current box matching the ID.
        private string GetPokemonRegionFromPC(int boxId, int boxPokemonId)
        {
            if (!IsPCAccessValid("getPokemonRegionFromPC", boxId, boxPokemonId))
            {
                return null;
            }
            return Bot.Game.CurrentPCBox[boxPokemonId - 1].Region.ToString();
        }

        // API: Original trainer of the pokemon of the current box matching the ID.
        private string GetPokemonOriginalTrainerFromPC(int boxId, int boxPokemonId)
        {
            if (!IsPCAccessValid("getPokemonOriginalTrainerFromPC", boxId, boxPokemonId))
            {
                return null;
            }
            return Bot.Game.CurrentPCBox[boxPokemonId - 1].OriginalTrainer;
        }

        // API: Gender of the pokemon of the current box matching the ID.
        private string GetPokemonGenderFromPC(int boxId, int boxPokemonId)
        {
            if (!IsPCAccessValid("getPokemonHappinessFromPC", boxId, boxPokemonId))
            {
                return null;
            }
            return Bot.Game.CurrentPCBox[boxPokemonId - 1].Gender;
        }

        // API: Form of the pokémon in the current box matching the ID. (0 if no form)
        private int GetPokemonFormFromPC(int boxId, int boxPokemonId)
        {
            if (!IsPCAccessValid("getPokemonFormFromPC", boxId, boxPokemonId))
            {
                return -1;
            }
            return Bot.Game.CurrentPCBox[boxPokemonId - 1].Form;
        }

        // API: Status of the pokemon of the current box matching the ID.
        private string GetPokemonStatusFromPC(int boxId, int boxPokemonId)
        {
            if (!IsPCAccessValid("getPokemonStatusFromPC", boxId, boxPokemonId))
            {
                return null;
            }
            return Bot.Game.CurrentPCBox[boxPokemonId - 1].Status;
        }

	// API: Type of the pokemon of the current box matching the ID as an array of length 2.
	private string[] GetPokemonTypeFromPC(int boxId, int boxPokemonId)
	{
		if (!IsPCAccessValid("getPokemonTypeFromPC", boxId, boxPokemonId))
		{
			return null;
		}
		
		int id = Bot.Game.CurrentPCBox[boxPokemonId - 1].Id;
		
		if (id <= 0 || id >= TypesManager.Instance.Type1.Count())
		{
			return new string[] { "Unknown", "Unknown" };
		}

		return new string[] { TypesManager.Instance.Type1[id].ToString(), TypesManager.Instance.Type2[id].ToString() };
	}

        // API: Returns the item held by the specified pokemon in the PC, null if empty.
        private string GetPokemonHeldItemFromPC(int boxId, int boxPokemonId)
        {
            if (!IsPCAccessValid("getPokemonHeldItemFromPC", boxId, boxPokemonId))
            {
                return null;
            }
            string itemHeld = Bot.Game.CurrentPCBox[boxPokemonId - 1].ItemHeld;
            return itemHeld == string.Empty ? null : itemHeld;
        }

        // API: Releases the specified pokemon in the team.
        private bool ReleasePokemonFromTeam(int pokemonUid)
        {
            if (pokemonUid < 1 || pokemonUid > 6 || pokemonUid > Bot.Game.Team.Count)
            {
                Fatal("error: releasePokemonFromTeam: pokemonUid is out of range: " + pokemonUid
                    + " (team size: " + Bot.Game.Team.Count.ToString() + ").");
                return false;
            }
            if (!Bot.Game.IsPCOpen)
            {
                Fatal("error: releasePokemonFromTeam: cannot release a pokemon while the PC is closed: #" + pokemonUid + " (" + Bot.Game.Team[pokemonUid].Name + ").");
                return false;
            }
            if (Bot.Game.IsPCBoxRefreshing)
            {
                Fatal("error: releasePokemonFromTeam: cannot release a pokemon while the PC box is refreshing: #" + pokemonUid + " (" + Bot.Game.Team[pokemonUid].Name + ").");
                return false;
            }
            return ExecuteAction(Bot.Game.ReleasePokemonFromTeam(pokemonUid));
        }

        // API: Releases the specified pokemon in the PC.
        private bool ReleasePokemonFromPC(int boxId, int boxPokemonId)
        {
            if (!IsPCAccessValid("releasePokemonFromPC", boxId, boxPokemonId))
            {
                return false;
            }
            return ExecuteAction(Bot.Game.ReleasePokemonFromPC(boxId, boxPokemonId));
        }

        // API: Buys the specified item from the opened shop.
        private bool BuyItem(string itemName, int quantity)
        {
            if (!ValidateAction("buyItem", false)) return false;

            if (Bot.Game.OpenedShop == null)
            {
                Fatal("error: buyItem can only be used when a shop is open.");
                return false;
            }

            ShopItem item = Bot.Game.OpenedShop.Items.FirstOrDefault(i => i.Name.Equals(itemName, StringComparison.InvariantCultureIgnoreCase));

            if (item == null)
            {
                Fatal("error: buyItem: the item '" + itemName + "' does not exist in the opened shop.");
                return false;
            }

            return ExecuteAction(Bot.Game.BuyItem(item.Id, quantity));
        }

        // API: Give the specified item on the specified pokemon.
        private bool GiveItemToPokemon(string itemName, int pokemonIndex)
        {
            if (!ValidateAction("giveItemToPokemon", false)) return false;

            if (pokemonIndex < 1 || pokemonIndex > Bot.Game.Team.Count)
            {
                Fatal("error: giveItemToPokemon: tried to retrieve the non-existing pokémon " + pokemonIndex + ".");
                return false;
            }

            InventoryItem item = Bot.Game.GetItemFromName(itemName);
            if (item == null || item.Quantity == 0)
            {
                Fatal("error: giveItemToPokemon: tried to give the non-existing item '" + itemName + "'.");
                return false;
            }

            return ExecuteAction(Bot.Game.GiveItemToPokemon(pokemonIndex, item.Id));
        }

        // API: Take the held item from the specified pokemon.
        private bool TakeItemFromPokemon(int index)
        {
            if (!ValidateAction("takeItemFromPokemon", false)) return false;

            if (index < 1 || index > Bot.Game.Team.Count)
            {
                Fatal("error: takeItemFromPokemon: tried to retrieve the non-existing pokemon " + index + ".");
                return false;
            }

            if (Bot.Game.Team[index - 1].ItemHeld == string.Empty)
            {
                Fatal("error: takeItemFromPokemon: tried to take the non-existing held item from pokémon '" + index + "'.");
                return false;
            }

            return ExecuteAction(Bot.Game.TakeItemFromPokemon(index));
        }

        // API: Adds the specified answer to the answer queue. It will be used in the next dialog.
        private void PushDialogAnswer(DynValue answerValue)
        {
            if (answerValue.Type == DataType.String)
            {
                Bot.Game.PushDialogAnswer(answerValue.CastToString());
            }
            else if (answerValue.Type == DataType.Number)
            {
                Bot.Game.PushDialogAnswer((int)answerValue.CastToNumber());
            }
            else
            {
                Fatal("error: pushDialogAnswer: the argument must be a number (index) or a string (search text).");
            }
        }

        private static HashSet<int> _outOfCombatItemScopes = new HashSet<int> { 8, 10, 15 };
        private static HashSet<int> _inCombatItemScopes = new HashSet<int> { 5 };
        private static HashSet<int> _outOfCombatOnPokemonItemScopes = new HashSet<int> { 2, 3, 9, 13, 14 };
        private static HashSet<int> _inCombatOnPokemonItemScopes = new HashSet<int> { 2 };

        // API: Uses the specified item.
        private bool UseItem(string itemName)
        {
            InventoryItem item = Bot.Game.GetItemFromName(itemName.ToUpperInvariant());
            if (item != null && item.Quantity > 0)
            {
                if (Bot.Game.IsInBattle && _inCombatItemScopes.Contains(item.Scope))
                {
                    if (!ValidateAction("useItem", true)) return false;
                    return ExecuteAction(Bot.AI.UseItem(item.Id));
                }
                else if (!Bot.Game.IsInBattle && _outOfCombatItemScopes.Contains(item.Scope))
                {
                    if (!ValidateAction("useItem", false)) return false;
                    Bot.Game.UseItem(item.Id);
                    return ExecuteAction(true);
                }
            }
            return false;
        }

        // API: Uses the specified item on the specified pokémon.
        private bool UseItemOnPokemon(string itemName, int pokemonIndex)
        {
            itemName = itemName.ToUpperInvariant();
            InventoryItem item = Bot.Game.GetItemFromName(itemName.ToUpperInvariant());

            if (item != null && item.Quantity > 0)
            {
                if (Bot.Game.IsInBattle && _inCombatOnPokemonItemScopes.Contains(item.Scope))
                {
                    if (!ValidateAction("useItemOnPokemon", true)) return false;
                    return ExecuteAction(Bot.AI.UseItem(item.Id, pokemonIndex));
                }
                else if (!Bot.Game.IsInBattle && _outOfCombatOnPokemonItemScopes.Contains(item.Scope))
                {
                    if (!ValidateAction("useItemOnPokemon", false)) return false;
                    Bot.Game.UseItem(item.Id, pokemonIndex);
                    return ExecuteAction(true);
                }
            }
            return false;
        }

        // API: Uses the most effective offensive move available.
        private bool Attack()
        {
            if (!ValidateAction("attack", true)) return false;

            return ExecuteAction(Bot.AI.Attack());
        }

        // API: Uses the least effective offensive move available.
        private bool WeakAttack()
        {
            if (!ValidateAction("weakAttack", true)) return false;

            return ExecuteAction(Bot.AI.WeakAttack());
        }

        // API: Tries to escape from the current wild battle.
        private bool Run()
        {
            if (!ValidateAction("run", true)) return false;

            return ExecuteAction(Bot.AI.Run());
        }

        // API: Sends the first usable pokemon different from the active one.
        private bool SendUsablePokemon()
        {
            if (!ValidateAction("sendUsablePokemon", true)) return false;

            return ExecuteAction(Bot.AI.SendUsablePokemon());
        }

        // API: Sends the first available pokemon different from the active one.
        private bool SendAnyPokemon()
        {
            if (!ValidateAction("sendAnyPokemon", true)) return false;

            return ExecuteAction(Bot.AI.SendAnyPokemon());
        }

        // API: Sends the specified pokemon to battle.
        private bool SendPokemon(int index)
        {
            if (!ValidateAction("sendPokemon", true)) return false;

            if (index < 1 || index > Bot.Game.Team.Count)
            {
                Fatal("error: sendPokemon: tried to send the non-existing pokemon " + index + ".");
                return false;
            }

            return ExecuteAction(Bot.AI.SendPokemon(index));
        }

        // API: Uses the specified move in the current battle if available.
        private bool UseMove(string moveName)
        {
            if (!ValidateAction("useMove", true)) return false;

            return ExecuteAction(Bot.AI.UseMove(moveName));
        }

        // API: Forgets the specified move, if existing, in order to learn a new one.
        private bool ForgetMove(string moveName)
        {
            if (!Bot.MoveTeacher.IsLearning)
            {
                Fatal("error: ‘forgetMove’ can only be used when a pokémon is learning a new move.");
                return false;
            }

            moveName = moveName.ToUpperInvariant();
            Pokemon pokemon = Bot.Game.Team[Bot.MoveTeacher.PokemonUid - 1];
            PokemonMove move = pokemon.Moves.FirstOrDefault(m => MovesManager.Instance.GetMoveData(m.Id)?.Name.ToUpperInvariant() == moveName);

            if (move != null)
            {
                Bot.MoveTeacher.MoveToForget = move.Position;
                return true;
            }
            return false;
        }

        // API: Forgets the first move that is not one of the specified moves.
        private bool ForgetAnyMoveExcept(DynValue[] moveNames)
        {
            if (!Bot.MoveTeacher.IsLearning)
            {
                Fatal("error: ‘forgetAnyMoveExcept’ can only be used when a pokémon is learning a new move.");
                return false;
            }

            HashSet<string> movesInvariantNames = new HashSet<string>();
            foreach (DynValue value in moveNames)
            {
                movesInvariantNames.Add(value.CastToString().ToUpperInvariant());
            }

            Pokemon pokemon = Bot.Game.Team[Bot.MoveTeacher.PokemonUid - 1];
            PokemonMove move = pokemon.Moves.FirstOrDefault(m => !movesInvariantNames.Contains(MovesManager.Instance.GetMoveData(m.Id)?.Name.ToUpperInvariant()));

            if (move != null)
            {
                Bot.MoveTeacher.MoveToForget = move.Position;
                return true;
            }
            return false;
        }
        
        // API: Writes a string, a number, or a table of strings and/or numbers to file
        // overwrite is an optional parameter, and will append the line(s) if absent
        private void LogToFile(string file, DynValue text, bool overwrite = false)
        {
            DirectoryInfo directory = new DirectoryInfo("Logs/");
            FileInfo info = new FileInfo("Logs/" + file);

            // Restricting access to Logs folder
            if (!info.FullName.StartsWith(directory.FullName))
            {
                Fatal("Error: Invalid file write access");
                return;
            }

            // Creating all necessary folders
            Directory.CreateDirectory(Path.GetDirectoryName(info.FullName));

            StringBuilder sb = new StringBuilder();
            
            if (text.Type == DataType.Table)
            {
                DynValue[] lines = text.Table.Values.ToArray();
                for (int i = 0; i < lines.Length; i++)
                {
                    sb.AppendLine(lines[i].CastToString());
                }
            }
            else
            {
                sb.AppendLine(text.CastToString());
            }

            if (overwrite)
                File.WriteAllText(info.FullName, sb.ToString());
            else
                File.AppendAllText(info.FullName, sb.ToString());
        }

        // API: Returns a table of every line in file
        private string[] ReadLinesFromFile(string file)
        {
            DirectoryInfo directory = new DirectoryInfo("Logs/");
            FileInfo info = new FileInfo("Logs/" + file);

            if (!info.FullName.StartsWith(directory.FullName))
            {
                Fatal("Error: Invalid File read access");
                return new string[] { };
            }

            file = info.FullName;

            if (!File.Exists(file)) return new string[] { };
            return File.ReadAllLines(file);
        }

        // API: Returns the connected server
        private string GetServer()
        {
            return Bot.Game != null ? Bot.Game.Server.ToString() : "None";
        }

        // API: Sets the option at a particular index, or creates it if it doesn't exist
        private void SetOption(int index, bool value)
        {
            if (!Bot.SliderOptions.ContainsKey(index))
            {
                Bot.CreateSlider(index, value);
                return;
            }

            Bot.SliderOptions[index].IsEnabled = value;
        }

        // API: Gets the option at a particular index, or creates it if it doesn't exist
        private bool GetOption(int index)
        {
            if (!Bot.SliderOptions.ContainsKey(index))
                return false;

            return Bot.SliderOptions[index].IsEnabled;
        }

        // API: Sets the name of the option at a particular index, or creates it if it doesn't exist
        private void SetOptionName(int index, string content)
        {
            if (!Bot.SliderOptions.ContainsKey(index))
            {
                Bot.CreateSlider(index, content + ": ", true);
                return;
            }

            Bot.SliderOptions[index].Name = content + ": ";
        }

        // API: Sets the tooltip description of the option at a particular index, or creates it if it doesn't exist
        private void SetOptionDescription(int index, string content)
        {
            if (!Bot.SliderOptions.ContainsKey(index))
            {
                Bot.CreateSlider(index, content, false);
                return;
            }

            Bot.SliderOptions[index].Description = content;
        }

        // API: Sets the text of the TextOption at a particular index, or creates it if it doesn't exist
        private void SetTextOption(int index, string content)
        {
            if (!Bot.TextOptions.ContainsKey(index))
            {
                Bot.CreateText(index, content);
                return;
            }

            Bot.TextOptions[index].Content = content;
        }

        // API: Returns the text content of the TextOption at a particular index, or an empty string if it doesn't exist
        private string GetTextOption(int index)
        {
            if (!Bot.TextOptions.ContainsKey(index))
                return "";

            return Bot.TextOptions[index].Content;
        }

        // API: Sets the name of the TextOption at a particular index, or creates it if it doesn't exist
        private void SetTextOptionName(int index, string content)
        {
            if (!Bot.TextOptions.ContainsKey(index))
            {
                Bot.CreateText(index, content + ": ", true);
                return;
            }

            Bot.TextOptions[index].Name = content + ": ";
        }

        // API: Sets the tooltip description of the TextOption at a particular index, or creates it if it doesn't exist
        private void SetTextOptionDescription(int index, string content)
        {
            if (!Bot.TextOptions.ContainsKey(index))
            {
                Bot.CreateText(index, content, false);
                return;
            }

            Bot.TextOptions[index].Description = content;
        }
        
        // API: Logs in using specified credentials
        private void Login(string accountName, string password, string server, int socks = 0, string host = "", int port = 0, string socksUser = "", string socksPass = "")
        {
            server = server.ToUpperInvariant();

            if (Bot.Game != null)
            {
                Fatal("error: login: tried to login while already logged in");
                return;
            }

            if (server != "BLUE" && server != "RED" && server != "YELLOW")
            {
                Fatal("error: login: tried to connect to an invalid server: \"" + server + "\"");
                return;
            }

            LogMessage("Connecting to the server...");
            Account account = new Account(accountName);
            account.Password = password;
            account.Server = server;

            if (socks == 4 || socks == 5)
            {
                account.Socks.Version = (SocksVersion)socks;
                account.Socks.Host = host;
                account.Socks.Port = port;
                account.Socks.Username = socksUser;
                account.Socks.Password = socksPass;
            }

            Bot.Login(account);
        }
        
        // API: Calls the specified function after the specified number of seconds
        public void Invoke(DynValue function, float time, params DynValue[] args)
        {
            if (function.Type != DataType.Function && function.Type != DataType.ClrFunction)
            {
                Fatal("error: invoke: tried to call an invalid function");
                return;
            }

            if (time == 0)
            {
                _lua.Call(function, args);
                return;
            }

            Invoker invoker = new Invoker()
            {
                Function = function,
                Time = DateTime.UtcNow.AddSeconds(time),
                Script = this,
                Args = args
            };

            Invokes.Add(invoker);
        }
        
        // API: Starts the loaded script (usable in the outer scope or with invoke)
        private bool StartScript()
        {
            if (Bot.Game != null && (Bot.Running == BotClient.State.Stopped || Bot.Running == BotClient.State.Paused))
            {
                Bot.Start();
                return true;
            }

            return false;
        }
    }

    public class Invoker
    {
        public DynValue Function;
        public DateTime Time;
        public LuaScript Script;
        public DynValue[] Args;

        public void Call()
        {
            Script.Invoke(Function, 0, Args);
        }
    }
}
