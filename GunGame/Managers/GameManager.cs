using System.Linq;
using System.Collections.Generic;

using Rocket.Unturned.Player;

using UnityEngine;

namespace GunGame.Managers
{
	public static class GameManager
	{

		public static int timer;
		public static bool isRunning;
		public static bool isWaiting;

		public static List<UnturnedPlayer> OnlinePlayers;
		public static List<UnturnedPlayer> InGamePlayers;

		public static void Initialize()
		{
			OnlinePlayers = new List<UnturnedPlayer> ();
			InGamePlayers = new List<UnturnedPlayer> ();

			timer = 0;
			isWaiting = false;
			isRunning = false;
		}

		public static void Update()
		{
			if (!isRunning && !isWaiting) {
				if (OnlinePlayers.Count >= GunGameConfig.instance.minPlayers) {
					RequestBegin ();
				}
			} else if (isRunning) {
				if (timer <= 0) {
					RequestFinish ();
				}
			} else if (isWaiting) {
				if (timer > 0) {

					if (timer == 5)
						GunGame.Say ("next", Color.green, "5");

					timer--;
				} else {
					isWaiting = false;

					if (OnlinePlayers.Count < GunGameConfig.instance.minPlayers)
						GunGame.Say ("notenoughplayers", Color.green, GunGameConfig.instance.minPlayers - OnlinePlayers.Count);

				}
			}
		}

		public static void RequestFinish()
		{
			isRunning = false;
			isWaiting = true;

			timer = 30;

			foreach (UnturnedPlayer player in InGamePlayers) {
				player.GunGamePlayer ().ClearInv ();
				player.Teleport (GunGameConfig.instance.safezone.Vector3, 0);
			}

			IEnumerable<UnturnedPlayer> winners = from player in InGamePlayers
												  orderby player.GunGamePlayer ().currentWeapon descending
												  select player;

			GunGame.Say ("first", Color.cyan, winners.ElementAt (0).DisplayName);
			GunGame.Say ("second", Color.cyan, winners.ElementAt (1).DisplayName);
			GunGame.Say ("third", Color.cyan, winners.ElementAt (2).DisplayName);
			GunGame.Say ("next", Color.green, "30");

			InGamePlayers.Clear ();
		}

		public static void RequestBegin()
		{
			isRunning = true;
			timer = GunGameConfig.instance.maxRoundTime;
		}

		public static bool IsPlayerInGame(UnturnedPlayer player)
		{
			return InGamePlayers.Contains (player);
		}
	}
}
