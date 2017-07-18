using System.Collections.Generic;

using Rocket.API;
using Rocket.Unturned.Player;

using UnityEngine;

namespace GunGame.Commands
{
	public class CommandSafezone : IRocketCommand
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
				return "Sets the safezone to your current location.";
			}
		}

		public string Name
		{
			get {
				return "safezone";
			}
		}

		public List<string> Permissions
		{
			get {
				return new List<string> { "gungame.safezone" };
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
			GunGameConfig.RegisterSafezone (p.Position);
			GunGame.Say (caller, "register_safezone", Color.green);
		}
	}
}
