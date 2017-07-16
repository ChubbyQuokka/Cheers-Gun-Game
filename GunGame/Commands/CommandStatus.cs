using System.Collections.Generic;

using Rocket.API;
using Rocket.Unturned.Player;

using UnityEngine;

using GunGame.Managers;

namespace GunGame.Commands
{
	public class CommandStatus : IRocketCommand
	{
		public List<string> Aliases
		{
			get {
				return new List<string> ();
			}
		}

		public AllowedCaller AllowedCaller
		{
			get {
				return AllowedCaller.Player;
			}
		}

		public string Help
		{
			get {
				return "Checks the status of the game.";
			}
		}

		public string Name
		{
			get {
				return "status";
			}
		}

		public List<string> Permissions
		{
			get {
				return new List<string> { "gungame.status" };
			}
		}

		public string Syntax
		{
			get {
				return "";
			}
		}

		public void Execute(IRocketPlayer caller, string [] command)
		{
			if (GameManager.isRunning) {
				GunGame.Say (caller, "inprogress", Color.green, GameManager.timer);
			} else if (GameManager.isWaiting) {
				GunGame.Say (caller, "next", Color.green, GameManager.timer);
			} else {
				GunGame.Say (caller, "notenoughplayers", Color.green, GunGameConfig.instance.minPlayers - GameManager.OnlinePlayers.Count);
			}
		}
	}
}
