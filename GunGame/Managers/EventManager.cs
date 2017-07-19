using Rocket.Unturned;
using Rocket.Unturned.Player;
using Rocket.Unturned.Events;
using Rocket.Unturned.Skills;

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

				ushort id = GunGameConfig.instance.weapons.weapons [m.GunGamePlayer ().currentWeapon].id;
				string itemName;

				if (id == 0) {
					itemName = "Fists";
				} else {
					ItemAsset asset = (ItemAsset)Assets.find (EAssetType.ITEM, id);
					itemName = asset.itemName;
				}

				GunGame.Say ("kill", Color.magenta, m.DisplayName, itemName, player.DisplayName);

				player.GunGamePlayer ().DeathCallback (cause == EDeathCause.MELEE || cause == EDeathCause.PUNCH);
				m.GunGamePlayer ().KillCallback (cause == EDeathCause.MELEE || cause == EDeathCause.PUNCH);

			}
		}

		static void OnPlayerRevive(UnturnedPlayer player, Vector3 pos, byte angle)
		{
			player.GunGamePlayer ().RespawnCallback ();
		}

		static void OnPlayerChatted(UnturnedPlayer player, ref Color color, string message, EChatMode mode, ref bool cancel)
		{
			if (mode == EChatMode.GLOBAL && !message.StartsWith ("/") && GunGameConfig.instance.mutePlayers) {
				GunGame.Say (player, "mute", Color.red);
				cancel = true;
			}
		}

		static void OnPlayerJoin(UnturnedPlayer player)
		{

			if (GunGameConfig.instance.kickGroup && player.SteamGroupID != CSteamID.Nil)
				player.GunGamePlayer ().KickPlayer ();


#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
			if (GunGameConfig.instance.safezone.x != 0 && GunGameConfig.instance.safezone.y != 0 && GunGameConfig.instance.safezone.z != 0)
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator
				player.Teleport (GunGameConfig.instance.safezone.Vector3, 0);

			GameManager.OnlinePlayers.Add (player.CSteamID.m_SteamID);

			if (GunGameConfig.instance.maxSkills) {
				player.SetSkillLevel (UnturnedSkill.Agriculture, 255);
				player.SetSkillLevel (UnturnedSkill.Cardio, 255);
				player.SetSkillLevel (UnturnedSkill.Cooking, 255);
				player.SetSkillLevel (UnturnedSkill.Crafting, 255);
				player.SetSkillLevel (UnturnedSkill.Dexerity, 255);
				player.SetSkillLevel (UnturnedSkill.Diving, 255);
				player.SetSkillLevel (UnturnedSkill.Engineer, 255);
				player.SetSkillLevel (UnturnedSkill.Exercise, 255);
				player.SetSkillLevel (UnturnedSkill.Fishing, 255);
				player.SetSkillLevel (UnturnedSkill.Healing, 255);
				player.SetSkillLevel (UnturnedSkill.Immunity, 255);
				player.SetSkillLevel (UnturnedSkill.Mechanic, 255);
				player.SetSkillLevel (UnturnedSkill.Outdoors, 255);
				player.SetSkillLevel (UnturnedSkill.Overkill, 255);
				player.SetSkillLevel (UnturnedSkill.Parkour, 255);
				player.SetSkillLevel (UnturnedSkill.Sharpshooter, 255);
				player.SetSkillLevel (UnturnedSkill.Sneakybeaky, 255);
				player.SetSkillLevel (UnturnedSkill.Strength, 255);
				player.SetSkillLevel (UnturnedSkill.Survival, 255);
				player.SetSkillLevel (UnturnedSkill.Toughness, 255);
				player.SetSkillLevel (UnturnedSkill.Vitality, 255);
				player.SetSkillLevel (UnturnedSkill.Warmblooded, 255);
			}
		}

		static void OnPlayerLeave(UnturnedPlayer player)
		{
			SQLManager.SavePlayer (player.CSteamID.m_SteamID, player.GunGamePlayer ().data);
			GameManager.OnlinePlayers.Remove (player.CSteamID.m_SteamID);

			if (GameManager.IsPlayerInGame (player.CSteamID.m_SteamID)) {
				GameManager.InGamePlayers.Remove (player.CSteamID.m_SteamID);
			}
		}
	}
}
