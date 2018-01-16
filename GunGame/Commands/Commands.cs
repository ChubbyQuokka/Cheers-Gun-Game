using System;
using GunGame.API;
using GunGame.API.Exceptions;
using GunGame.Managers;

using Rocket.API;
using Rocket.Unturned.Player;

using UnityEngine;

namespace GunGame.Commands
{
    public class CommandSetLobby : IGunGameCommand
    {
        public string Help => "Sets the lobby to your current location.";
        public EPermissionLevel PermissionLevel => EPermissionLevel.HIGH;
        public ECommandTiming CommandTiming => ECommandTiming.STOPPED | ECommandTiming.WAITING;
        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer p = (UnturnedPlayer)caller;
            GunGameConfig.RegisterSafezone(p.Position, p.Rotation);
            GunGame.Say(caller, "register_lobby", Color.green);
        }
    }

    public class CommandSetSpawn : IGunGameCommand
    {
        public string Help => "Sets a spawn position to your current location.";
        public EPermissionLevel PermissionLevel => EPermissionLevel.HIGH;
        public ECommandTiming CommandTiming => ECommandTiming.STOPPED | ECommandTiming.WAITING;
        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer p = (UnturnedPlayer)caller;
            GunGameConfig.RegisterSpawnPosition(p.Position, p.Rotation);
            GunGame.Say(caller, "register_spawn", Color.green);
        }
    }

    public class CommandStatus : IGunGameCommand
    {
        public string Help => "Checks the status of the game.";
        public EPermissionLevel PermissionLevel => EPermissionLevel.LOW;
        public ECommandTiming CommandTiming => ECommandTiming.STOPPED | ECommandTiming.WAITING | ECommandTiming.RUNNING;
        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (GameManager.isRunning)
                GunGame.Say(caller, "inprogress", Color.green, GameManager.GetTime());
            else if (GameManager.isStopped)
                GunGame.Say(caller, "stopped", Color.green);
            else if (GameManager.OnlinePlayers.Count < GunGameConfig.instance.minPlayers)
                GunGame.Say(caller, "notenoughplayers", Color.green, GunGameConfig.instance.minPlayers - GameManager.OnlinePlayers.Count);
            else
                GunGame.Say(caller, "next", Color.green, GameManager.GetTime());
        }
    }

    public class CommandForceStart : IGunGameCommand
    {
        public string Help => "Forces the game to start.";
        public EPermissionLevel PermissionLevel => EPermissionLevel.HIGH;
        public ECommandTiming CommandTiming => ECommandTiming.STOPPED | ECommandTiming.WAITING;
        public void Execute(IRocketPlayer caller, string[] args)
        {
            GameManager.isStopped = false;
            GameManager.RequestBegin();
            GunGame.Say(caller, "forcestart", Color.green);
        }
    }

    public class CommandForceStop : IGunGameCommand
    {
        public string Help => "Forces the game to stop.";
        public EPermissionLevel PermissionLevel => EPermissionLevel.HIGH;
        public ECommandTiming CommandTiming => ECommandTiming.RUNNING;
        public void Execute(IRocketPlayer caller, string[] args)
        {
            GameManager.isStopped = true;
            GameManager.RequestFinish();
            GunGame.Say(caller, "forcestop", Color.green);
        }
    }

    public class CommandStart : IGunGameCommand
    {
        public string Help => "Starts the lobby timer.";
        public EPermissionLevel PermissionLevel => EPermissionLevel.HIGH;
        public ECommandTiming CommandTiming => ECommandTiming.STOPPED | ECommandTiming.RUNNING | ECommandTiming.WAITING;
        public void Execute(IRocketPlayer caller, string[] args)
        {
            if (!GameManager.isStopped)
                GunGame.Say(caller, "invalid_start", Color.red);
            else
            {
                GameManager.isStopped = false;
                GunGame.Say(caller, "start", Color.green);
            }
        }
    }

    public class CommandStop : IGunGameCommand
    {
        public string Help => "Stops the lobby timer.";
        public EPermissionLevel PermissionLevel => EPermissionLevel.HIGH;
        public ECommandTiming CommandTiming => ECommandTiming.RUNNING | ECommandTiming.WAITING;
        public void Execute(IRocketPlayer caller, string[] args)
        {
            if (!GameManager.isStopped)
            {
                GameManager.isStopped = true;
                GunGame.Say(caller, "stop", Color.green);
            }
            else
                GunGame.Say(caller, "invalid_stop", Color.red);
        }
    }

    public class CommandHelp : IGunGameCommand
    {
        public string Help => "Shows info on a requested command.";
        public EPermissionLevel PermissionLevel => EPermissionLevel.LOW;
        public ECommandTiming CommandTiming => ECommandTiming.STOPPED | ECommandTiming.RUNNING | ECommandTiming.WAITING;
        public void Execute(IRocketPlayer caller, string[] args)
        {
            if (args.Length != 1)
                throw new GunGameException(EExceptionType.INVALID_ARGS);

            if (CommandManager.TryGetCommand(args[0], out IGunGameCommand cmd))
                if (caller.HasPermissionFor(cmd))
                    GunGame.Say(caller, "help", Color.green, args[0], cmd.Help);
                else
                    GunGame.Say(caller, "invalid_perms_help", Color.red);
            else
                GunGame.Say(caller, "invalid_cmd_help", Color.red);
        }
    }

    public class CommandGiveKit : IGunGameCommand
    {
        public string Help => "Gives you the specified kit.";
        public EPermissionLevel PermissionLevel => EPermissionLevel.HIGH;
        public ECommandTiming CommandTiming => ECommandTiming.RUNNING;
        public void Execute(IRocketPlayer caller, string[] args)
        {
            if (args.Length != 1 || !byte.TryParse(args[0], out byte kit) || kit >= GunGameConfig.instance.weapons.weapons.Length)
                throw new GunGameException(EExceptionType.INVALID_ARGS);

            GunGamePlayerComponent p = ((UnturnedPlayer)caller).GunGamePlayer();

            p.ClearItems();
            p.GiveKit(kit);
            p.currentWeapon = kit;
            GunGame.Say(caller, "kit", Color.green, kit);
        }
    }
}