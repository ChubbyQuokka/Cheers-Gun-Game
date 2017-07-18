using System.Collections.Generic;

using Rocket.API;
using Rocket.Unturned.Player;

using UnityEngine;

namespace GunGame.Commands
{
	public class CommandSpawnpoint : IRocketCommand
	{
		public List<string> Aliases
		{
			get {
				return new List<string> { "spawn" };
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
				return "Sets a spawn position to your current location.";
			}
		}

		public string Name
		{
			get {
				return "spawnposition";
			}
		}

		public List<string> Permissions
		{
			get {
				return new List<string> { "gungame.spawn" };
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
			UnturnedPlayer p = caller as UnturnedPlayer;
			GunGameConfig.RegisterSpawnPosition (p.Position);
			GunGame.Say (caller, "register_spawn", Color.green);
		}
	}
}
