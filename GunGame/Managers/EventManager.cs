#pragma warning disable RECS0018
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
            } else
                player.GunGamePlayer().ClearItems();

        }

        static void OnPlayerRevive(UnturnedPlayer player, Vector3 pos, byte angle)
        {
            if (GameManager.IsPlayerInGame(player.CSteamID.m_SteamID))
                player.GunGamePlayer().RespawnCallback();
        }

        static void OnPlayerChatted(UnturnedPlayer player, ref Color color, string message, EChatMode mode, ref bool cancel)
        {
            if (mode == EChatMode.GLOBAL && !message.StartsWith("/", StringComparison.Ordinal) && GunGameConfig.instance.mutePlayers) {
                GunGame.Say(player, "mute", Color.red);
                cancel = true;
            }
        }

        static void OnPlayerJoin(UnturnedPlayer player)
        {
            SteamPlayer steam = player.SteamPlayer();

            if (GunGameConfig.instance.forceNoGroup && player.SteamGroupID != CSteamID.Nil)
                steam.playerID.group = CSteamID.Nil;

            if (GunGameConfig.instance.disableCosmetics) {
                steam.maskItem = 0;
                steam.hatItem = 0;
                steam.vestItem = 0;
                steam.shirtItem = 0;
                steam.pantsItem = 0;
                steam.glassesItem = 0;
            }

            if (GunGameConfig.instance.safezone.x != 0 && GunGameConfig.instance.safezone.y != 0 && GunGameConfig.instance.safezone.z != 0)
                player.Teleport(GunGameConfig.instance.safezone.Vector3, GunGameConfig.instance.safezone.rot);

            if (GunGameConfig.instance.maxSkills)
                player.GunGamePlayer().MaxSkills();

        }

        static void OnPlayerLeave(UnturnedPlayer player)
        {
            if (GunGame.IsMySqlEnabled)
                SQLManager.SavePlayer(player.CSteamID.m_SteamID, player.GunGamePlayer().data);

            if (GameManager.IsPlayerInGame(player.CSteamID.m_SteamID))
                GameManager.InGamePlayers.Remove(player.CSteamID.m_SteamID);
        }

        public static void OnShutdown()
        {
            foreach (SteamPlayer player in Provider.clients) {
                ulong id = player.playerID.steamID.m_SteamID;
                GunGamePlayerComponent p = id.GetPlayer().GunGamePlayer();

                if (GunGame.IsMySqlEnabled) {
                    SQLManager.SavePlayer(id, p.data);
                }
            }
        }
    }
}
#pragma warning restore RECS0018