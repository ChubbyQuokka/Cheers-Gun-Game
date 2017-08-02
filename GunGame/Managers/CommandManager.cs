using System;
using System.Collections.Generic;

using GunGame.API;
using GunGame.API.Exceptions;
using GunGame.Commands;
using GunGame.Managers;

using Rocket.API;
using Rocket.Unturned.Player;
using UnityEngine;

namespace GunGame.Managers
{
    public static class CommandManager
    {
        static Dictionary<string, IGunGameCommand> commands;

        public static void Initialize()
        {
            commands = new Dictionary<string, IGunGameCommand>();

            RegisterCommand("setlobby", new CommandSetLobby());
            RegisterCommand("setspawn", new CommandSetSpawn());
            RegisterCommand("status", new CommandStatus());
            RegisterCommand("start", new CommandStart());
            RegisterCommand("stop", new CommandStop());
            RegisterCommand("forcestart", new CommandForceStart());
            RegisterCommand("forcestop", new CommandForceStop());
            RegisterCommand("help", new CommandHelp());
            RegisterCommand("kit", new CommandGiveKit());
        }

        public static void ExcecuteCommand(IRocketPlayer player, string[] args)
        {
            if (TryGetCommand(args[0], out IGunGameCommand cmd)) {

                if (!player.HasPermissionFor(cmd) && !player.IsAdmin)
                    throw new GunGameException(cmd.PermissionLevel);

                ECommandTiming current;

                if (GameManager.isRunning)
                    current = ECommandTiming.RUNNING;
                else if (GameManager.isStopped)
                    current = ECommandTiming.STOPPED;
                else
                    current = ECommandTiming.WAITING;

                if (!cmd.CommandTiming.HasFlags(current))
                    throw new GunGameException(cmd.CommandTiming);

                string[] _args = new string[args.Length - 1];

                for (int i = 1; i < args.Length; i++)
                    _args[i - 1] = args[i];

                cmd.Execute(player, _args);

            } else
                throw new GunGameException(EExceptionType.INVALID_CMD);
        }

        public static bool TryGetCommand(string key, out IGunGameCommand cmd)
        {
            return commands.TryGetValue(key, out cmd);
        }

        public static bool RegisterCommand(string name, IGunGameCommand command)
        {
            if (!commands.ContainsKey(name)) {
                commands.Add(name, command);
                return true;
            }

            return false;
        }

        public static bool UnregisterCommand(string name)
        {
            if (commands.ContainsKey(name)) {
                commands.Remove(name);
                return true;
            }

            return false;
        }
    }
}
namespace GunGame
{
    public class GunGameCommand : IRocketCommand
    {
        public List<string> Aliases => new List<string> { "gg" };

        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Help => "The universal Gun Game command!";

        public string Name => "gungame";

        public List<string> Permissions => new List<string> { "gungame" };

        public string Syntax => "";

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0) {
                ((UnturnedPlayer)caller).Player.sendBrowserRequest("For any information on this plugin, visit our Wiki!", "https://github.com/Graybad1/Gun-Game/wiki");
                return;
            }
            try {
                CommandManager.ExcecuteCommand(caller, command);
            } catch (GunGameException e) {
                switch (e.type) {
                    case EExceptionType.INVALID_ARGS:
                        GunGame.Say(caller, "invalid_args", Color.red);
                        break;
                    case EExceptionType.INVALID_CMD:
                        GunGame.Say(caller, "invalid_cmd", Color.red);
                        break;
                    case EExceptionType.INVALID_PERMS:
                        string permLevel = "";
                        switch (e.level) {
                            case EPermissionLevel.HIGH:
                                permLevel = "High";
                                break;
                            case EPermissionLevel.MEDIUM:
                                permLevel = "Medium";
                                break;
                            case EPermissionLevel.LOW:
                                permLevel = "Low";
                                break;
                        }
                        GunGame.Say(caller, "invalid_perms", Color.red, permLevel);
                        break;
                    case EExceptionType.INVALID_TIME:
                        bool hasMultiple = false;
                        string str = "";
                        if (e.timing.HasFlags(ECommandTiming.RUNNING)) {
                            str += "Running";
                            hasMultiple = true;
                        }
                        if (e.timing.HasFlags(ECommandTiming.STOPPED)) {
                            if (hasMultiple)
                                str += " or Stopped";
                            else {
                                str += "Stopped";
                                hasMultiple = true;
                            }
                        }
                        if (e.timing.HasFlags(ECommandTiming.WAITING)) {
                            if (hasMultiple)
                                str += " or Waiting";
                            else
                                str += "Waiting";
                        }
                        GunGame.Say(caller, "invalid_time", Color.red, str);
                        break;
                }
            }
        }
    }
}