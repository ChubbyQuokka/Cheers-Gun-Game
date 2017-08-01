using System;

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
            U.Events.OnShutdown += OnShutdown;
        }

        public static void Unregister()
        {
            UnturnedPlayerEvents.OnPlayerDeath -= OnPlayerDeath;
            UnturnedPlayerEvents.OnPlayerRevive -= OnPlayerRevive;
            UnturnedPlayerEvents.OnPlayerChatted -= OnPlayerChatted;

            U.Events.OnPlayerConnected -= OnPlayerJoin;
            U.Events.OnPlayerDisconnected -= OnPlayerLeave;
            U.Events.OnShutdown -= OnShutdown;
        }

        static void OnPlayerDeath(UnturnedPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer)
        {
            bool isGun = cause == EDeathCause.GUN;
            bool isMelee = cause == EDeathCause.MELEE;
            bool isPunch = cause == EDeathCause.PUNCH;

            if (isGun || isMelee || isPunch) {
                UnturnedPlayer m = UnturnedPlayer.FromCSteamID(murderer);
                if (GunGameConfig.instance.broadcastKills) {
                    string itemName;

                    if (isMelee)
                        itemName = ((ItemAsset)Assets.find(EAssetType.ITEM, GunGameConfig.instance.weapons.secondary)).itemName;
                    else if (isPunch)
                        itemName = "Fists";
                    else
                        itemName = ((ItemAsset)Assets.find(EAssetType.ITEM, GunGameConfig.instance.weapons.weapons[m.GunGamePlayer().currentWeapon].id)).itemName;

                    foreach (ulong id in GameManager.InGamePlayers) {
                        if (id == player.CSteamID.m_SteamID) {
                            GunGame.Say(id, "kill", Color.red, m.DisplayName, itemName, player.DisplayName);
                        } else if (id == murderer.m_SteamID) {
                            GunGame.Say(id, "kill", Color.green, m.DisplayName, itemName, player.DisplayName);
                        } else {
                            GunGame.Say(id, "kill", Color.magenta, m.DisplayName, itemName, player.DisplayName);
                        }
                    }
                }
                player.GunGamePlayer().DeathCallback(isMelee || isPunch);
                m.GunGamePlayer().KillCallback(isMelee || isPunch);
            }
        }

        static void OnPlayerRevive(UnturnedPlayer player, Vector3 pos, byte angle)
        {
            player.GunGamePlayer().RespawnCallback();
        }

        static void OnPlayerChatted(UnturnedPlayer player, ref Color color, string message, EChatMode mode, ref bool cancel)
        {
            if (mode == EChatMode.GLOBAL && !message.StartsWith("/", StringComparison.Ordinal) && GunGameConfig.instance.mutePlayers) {
                GunGame.Say(player, "mute", Color.red);
                cancel = true;
            }
        }

#pragma warning disable RECS0018
        static void OnPlayerJoin(UnturnedPlayer player)
        {
            if (GunGameConfig.instance.kickGroup && player.SteamGroupID != CSteamID.Nil)
                player.GunGamePlayer().Kick();

            if (GunGameConfig.instance.safezone.x != 0 && GunGameConfig.instance.safezone.y != 0 && GunGameConfig.instance.safezone.z != 0)
                player.Teleport(GunGameConfig.instance.safezone.Vector3, 0);

            GameManager.OnlinePlayers.Add(player.CSteamID.m_SteamID);

            if (GunGameConfig.instance.maxSkills)
                player.GunGamePlayer().MaxSkills();

        }
#pragma warning restore RECS0018

        static void OnPlayerLeave(UnturnedPlayer player)
        {
            SQLManager.SavePlayer(player.CSteamID.m_SteamID, player.GunGamePlayer().data);
            GameManager.OnlinePlayers.Remove(player.CSteamID.m_SteamID);

            if (GameManager.IsPlayerInGame(player.CSteamID.m_SteamID))
                GameManager.InGamePlayers.Remove(player.CSteamID.m_SteamID);
        }

        static void OnShutdown()
        {
            foreach (ulong player in GameManager.OnlinePlayers)
                SQLManager.SavePlayer(player, player.GetPlayer().GunGamePlayer().data);
        }
    }
}
