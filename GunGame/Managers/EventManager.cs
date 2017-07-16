using Rocket.Unturned;
using Rocket.Unturned.Player;
using Rocket.Unturned.Events;

using SDG.Unturned;

using Steamworks;

using UnityEngine;

namespace GunGame.Managers
{
	public static class EventManager
	{
		public static void Register()
		{
			UnturnedPlayerEvents.OnPlayerDeath += OnPlayerDeath;
			UnturnedPlayerEvents.OnPlayerRevive += OnPlayerRevive;
			UnturnedPlayerEvents.OnPlayerChatted += OnPlayerChatted;

			U.Events.OnPlayerConnected += OnPlayerJoin;
			U.Events.OnPlayerDisconnected += OnPlayerLeave;
		}

		public static void Unregister()
		{
			UnturnedPlayerEvents.OnPlayerDeath -= OnPlayerDeath;
			UnturnedPlayerEvents.OnPlayerRevive -= OnPlayerRevive;
			UnturnedPlayerEvents.OnPlayerChatted -= OnPlayerChatted;

			U.Events.OnPlayerConnected -= OnPlayerJoin;
			U.Events.OnPlayerDisconnected -= OnPlayerLeave;
		}

		static void OnPlayerDeath(UnturnedPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer)
		{
			if (cause == EDeathCause.GUN || cause == EDeathCause.MELEE || cause == EDeathCause.PUNCH) {

				UnturnedPlayer m = UnturnedPlayer.FromCSteamID (murderer);

				player.GunGamePlayer ().data.deaths++;
				m.GunGamePlayer ().data.kills++;

				player.GunGamePlayer ().DeathCallback (cause == EDeathCause.MELEE || cause == EDeathCause.PUNCH);
				m.GunGamePlayer ().KillCallback ();

				ushort id = m.Player.equipment.itemID;
				string itemName;

				if (id == 0) {
					itemName = "Fists";
				} else {
					ItemAsset asset = (ItemAsset)Assets.find (EAssetType.ITEM, id);
					itemName = asset.itemName;
				}
				GunGame.Say ("kill", Color.magenta, m.DisplayName, itemName, player.DisplayName);
			}
		}

		static void OnPlayerRevive(UnturnedPlayer player, Vector3 pos, byte angle)
		{
			player.GunGamePlayer ().RespawnCallback ();
		}

		static void OnPlayerChatted(UnturnedPlayer player, ref Color color, string message, EChatMode mode, ref bool cancel)
		{
			GunGame.Say (player, "mute", Color.red);
			cancel = GunGameConfig.instance.mutePlayers && mode == EChatMode.GLOBAL;
		}

		static void OnPlayerJoin(UnturnedPlayer player)
		{
			player.Teleport (GunGameConfig.instance.safezone.Vector3, 0);
			GameManager.OnlinePlayers.Add (player);
		}

		static void OnPlayerLeave(UnturnedPlayer player)
		{
			SQLManager.SavePlayer (player.CSteamID.m_SteamID, player.GunGamePlayer ().data);
			GameManager.OnlinePlayers.Remove (player);

			if (GameManager.IsPlayerInGame (player)) {
				GameManager.InGamePlayers.Remove (player);
			}

		}

	}
}
