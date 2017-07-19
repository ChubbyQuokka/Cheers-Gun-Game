using Rocket.Unturned.Items;
using Rocket.Unturned.Player;

using SDG.Unturned;

using GunGame;
using GunGame.Managers;

namespace GunGame
{
	public class GunGamePlayerComponent : UnturnedPlayerComponent
	{
		public SQLManager.PlayerQuery data;

		public byte currentWeapon;

		public byte kills;
		public byte deaths;

		void Start()
		{
			data = SQLManager.LoadPlayer (Player.CSteamID.m_SteamID);

			if (data.isFirstQuery) {
				Player.GiveItem (GunGameConfig.instance.weapons.hat, 1);
				Player.GiveItem (GunGameConfig.instance.weapons.mask, 1);
				Player.GiveItem (GunGameConfig.instance.weapons.vest, 1);
				Player.GiveItem (GunGameConfig.instance.weapons.pants, 1);
				Player.GiveItem (GunGameConfig.instance.weapons.shirt, 1);
			} else {
				ClearInv ();
			}
		}

		public void EnterGame()
		{
			currentWeapon = 0;
			GiveKit (currentWeapon);
			data.rounds++;

			kills = 0;
			deaths = 0;
		}

		public void KickPlayer()
		{
			Invoke ("Kick", 3);
		}

		void Kick()
		{
			Player.Kick ("Please you leave your group before joining.");
		}

		public void DeathCallback(bool wasByKnife)
		{
			ClearInv ();

			if (wasByKnife && currentWeapon != 0) {
				currentWeapon--;
			}
			data.deaths++;
			deaths++;
		}

		public void RespawnCallback()
		{
			if (GameManager.isRunning) {
				GiveKit (currentWeapon);
				Invoke ("TeleportAfterRespawn", 3);
			}
		}

		void TeleportAfterRespawn()
		{
			Player.Teleport (GameManager.GetSpawnPositionRR (), 0);
		}

		public void KillCallback(bool wasWithKnife)
		{
			if (wasWithKnife) {
				if (currentWeapon == GunGameConfig.instance.weapons.weapons.Length - 1) {
					currentWeapon++;
					GameManager.RequestFinish ();
				} else {
					ClearInv ();
					currentWeapon++;
					GiveKit (currentWeapon);
				}
			}
			data.kills++;
			kills++;
		}

		public void ClearInv()
		{
			for (byte p = 0; p <= 7; p++) {
				byte amt = Player.Inventory.getItemCount (p);
				for (byte index = 0; index < amt; index++) {
					Player.Inventory.removeItem (p, 0);
				}
			}
		}

		void GiveKit(byte kit)
		{
			Item primary = GunGameConfig.instance.weapons.weapons [kit].GetUnturnedItem ();
			Item secondary = UnturnedItems.AssembleItem (GunGameConfig.instance.weapons.secondary, 1, 100, null);
			Item mag = UnturnedItems.AssembleItem (GunGameConfig.instance.weapons.weapons [kit].mag, GunGameConfig.instance.weapons.weapons [kit].ammo, 100, null);

			Player.Inventory.items [0].tryAddItem (primary);
			Player.Inventory.items [1].tryAddItem (secondary);
			Player.Inventory.items [2].tryAddItem (mag);
			Player.Inventory.items [2].tryAddItem (mag);

			Player.Player.equipment.tryEquip (0, 0, 0);
		}
	}
}
namespace Rocket.Unturned.Player
{
	public static class UnturnedPlayerExtensions
	{
		public static GunGamePlayerComponent GunGamePlayer(this UnturnedPlayer player)
		{
			return player.GetComponent<GunGamePlayerComponent> ();
		}
	}
}

public static class UInt64Extensions
{
	public static UnturnedPlayer GetPlayer(this ulong id)
	{
		return UnturnedPlayer.FromCSteamID (new Steamworks.CSteamID (id));
	}
}