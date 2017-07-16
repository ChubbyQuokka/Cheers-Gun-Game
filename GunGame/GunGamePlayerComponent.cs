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

		void Awake()
		{
			bool isFirstTime;

			data = SQLManager.LoadPlayer (Player.CSteamID.m_SteamID, out isFirstTime);

			ClearInv ();

			if (isFirstTime) {
				Player.GiveItem (GunGameConfig.instance.weapons.hat, 1);
				Player.GiveItem (GunGameConfig.instance.weapons.mask, 1);
				Player.GiveItem (GunGameConfig.instance.weapons.vest, 1);
				Player.GiveItem (GunGameConfig.instance.weapons.pants, 1);
				Player.GiveItem (GunGameConfig.instance.weapons.shirt, 1);
			}
		}

		public void EnterGame()
		{
			currentWeapon = 0;
			GiveKit (currentWeapon);
		}

		public void DeathCallback(bool wasByKnife)
		{
			ClearInv ();

			if (wasByKnife && currentWeapon != 0) {
				currentWeapon--;
			}
		}

		public void RespawnCallback()
		{
			GiveKit (currentWeapon);
		}

		public void KillCallback()
		{
			if (currentWeapon == GunGameConfig.instance.weapons.weapons.Length - 1) {
				GameManager.RequestFinish ();
			} else {
				ClearInv ();
				currentWeapon++;
				GiveKit (currentWeapon);
			}
		}

		public void ClearInv()
		{
			foreach (Items i in Player.Inventory.items) {
				for (byte x = 0; x < i.width; x++) {
					for (byte y = 0; y < i.height; y++) {
						byte index = (byte)((x * i.width) + y);

						if (i.getItem (index) != null) {
							i.removeItem (index);
						}
					}
				}
			}
		}

		void GiveKit(byte kit)
		{
			Item primary = GunGameConfig.instance.weapons.weapons [kit].GetUnturnedItem ();
			Item secondary = UnturnedItems.AssembleItem (GunGameConfig.instance.weapons.secondary, 1, 100, null);
			Item mag = UnturnedItems.AssembleItem (GunGameConfig.instance.weapons.weapons [kit].mag, 2, 100, null);

			Player.Inventory.items [0].tryAddItem (primary);
			Player.Inventory.items [1].tryAddItem (secondary);
			Player.Inventory.items [2].tryAddItem (mag);
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