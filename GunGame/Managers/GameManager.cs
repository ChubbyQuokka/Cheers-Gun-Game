using System.Linq;
using System.Collections.Generic;

using Rocket.Unturned.Player;

using UnityEngine;

using SDG.Unturned;

namespace GunGame.Managers
{
	public static class GameManager
	{

		public static int timer;
		public static bool isRunning;
		public static bool isWaiting;

		public static List<ulong> OnlinePlayers;
		public static List<ulong> InGamePlayers;

		static int lastSpawn = 0;

		public static void Initialize()
		{
			OnlinePlayers = new List<ulong> ();
			InGamePlayers = new List<ulong> ();

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

					if (timer == 300)
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

			timer = 1800;

			foreach (ulong player in InGamePlayers) {
				player.GetPlayer ().GunGamePlayer ().ClearInv ();
				player.GetPlayer ().Teleport (GunGameConfig.instance.safezone.Vector3, 0);
			}

			IEnumerable<ulong> winners = from player in InGamePlayers
										 orderby player.GetPlayer ().GunGamePlayer ().currentWeapon descending
										 select player;

			UnturnedPlayer first = winners.ElementAt (0).GetPlayer ();
			first.GunGamePlayer ().data.first++;
			UnturnedPlayer second = winners.ElementAt (1).GetPlayer ();
			second.GunGamePlayer ().data.second++;
			UnturnedPlayer third = winners.ElementAt (2).GetPlayer ();
			third.GunGamePlayer ().data.third++;

			GunGame.Say ("first", Color.cyan, first.DisplayName, first.GunGamePlayer ().kills, first.GunGamePlayer ().deaths);
			GunGame.Say ("second", Color.cyan, second.DisplayName, second.GunGamePlayer ().kills, second.GunGamePlayer ().deaths);
			GunGame.Say ("third", Color.cyan, third.DisplayName, third.GunGamePlayer ().kills, third.GunGamePlayer ().deaths);

			InGamePlayers.Clear ();
		}

		public static void RequestBegin()
		{
			LightingManager.time = (uint)(LightingManager.cycle * LevelLighting.transition);

			isRunning = true;
			timer = GunGameConfig.instance.maxRoundTime;

			foreach (ulong player in OnlinePlayers) {
				InGamePlayers.Add (player);
				player.GetPlayer ().Teleport (GetSpawnPositionRR (), 0);
				player.GetPlayer ().GunGamePlayer ().EnterGame ();
			}

		}

		public static bool IsPlayerInGame(ulong player)
		{
			return InGamePlayers.Contains (player);
		}

		public static Vector3 GetSpawnPositionRR()
		{

			Vector3 vect = GunGameConfig.instance.positions [lastSpawn].Vector3;

			if (lastSpawn == GunGameConfig.instance.positions.Length - 1) {
				lastSpawn = 0;
			} else {
				lastSpawn++;
			}
			return vect;
		}
	}
}
