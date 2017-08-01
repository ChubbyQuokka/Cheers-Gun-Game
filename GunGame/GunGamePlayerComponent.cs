using System.Collections.Generic;

using Rocket.Unturned.Items;
using Rocket.Unturned.Player;
using Rocket.Unturned.Skills;

using Rocket.Core;

using SDG.Unturned;

using GunGame.API;
using GunGame.Managers;

namespace GunGame
{
    public class GunGamePlayerComponent : UnturnedPlayerComponent
    {
        public SQLManager.PlayerQuery data;

        public byte currentWeapon;

        public byte kills;
        public byte deaths;

        public EPermissionLevel pLevel;

        void Start()
        {
            data = SQLManager.LoadPlayer(Player.CSteamID.m_SteamID);

            if (R.Permissions.HasPermission(Player, new List<string> { "gungame.high" })) {
                pLevel = EPermissionLevel.HIGH;
            } else if (R.Permissions.HasPermission(Player, new List<string> { "gungame.medium" })) {
                pLevel = EPermissionLevel.MEDIUM;
            } else if (R.Permissions.HasPermission(Player, new List<string> { "gungame.low" })) {
                pLevel = EPermissionLevel.LOW;
            } else {
                pLevel = EPermissionLevel.NONE;
            }

            if (data.isFirstQuery) {
                Player.GiveItem(GunGameConfig.instance.weapons.hat, 1);
                Player.GiveItem(GunGameConfig.instance.weapons.mask, 1);
                Player.GiveItem(GunGameConfig.instance.weapons.vest, 1);
                Player.GiveItem(GunGameConfig.instance.weapons.pants, 1);
                Player.GiveItem(GunGameConfig.instance.weapons.shirt, 1);
            } else {
                ClearItems();
            }
        }

        public void EnterGame()
        {
            currentWeapon = 0;
            GiveKit(currentWeapon);
            data.rounds++;

            kills = 0;
            deaths = 0;
        }

        public void Kick()
        {
            Invoke("_Kick", 3);
        }

        void _Kick()
        {
            Player.Kick("Please you leave your group before joining.");
        }

        public void DeathCallback(bool wasByKnife)
        {
            ClearItems();

            if (wasByKnife && currentWeapon != 0) {
                currentWeapon--;
            }
            data.deaths++;
            deaths++;
        }

        public void RespawnCallback()
        {
            if (GunGameConfig.instance.maxSkills)
                Player.GunGamePlayer().MaxSkills();

            if (GameManager.isRunning) {
                GiveKit(currentWeapon);
                Invoke("TeleportAfterRespawn", 3);
            }
        }

        void TeleportAfterRespawn()
        {
            Player.Teleport(GameManager.GetSpawnPositionRR(), 0);
        }

        public void KillCallback(bool wasWithKnife)
        {
            if (!wasWithKnife) {
                if (currentWeapon == GunGameConfig.instance.weapons.weapons.Length - 1) {
                    currentWeapon++;
                    GameManager.RequestFinish();
                } else {
                    ClearItems();
                    currentWeapon++;
                    GiveKit(currentWeapon);
                }
            }
            data.kills++;
            kills++;
        }

        public void ClearItems()
        {
            for (byte p = 0; p <= 7; p++) {
                byte amt = Player.Inventory.getItemCount(p);
                for (byte index = 0; index < amt; index++) {
                    Player.Inventory.removeItem(p, 0);
                }
            }
        }

        void GiveKit(byte kit)
        {
            Item primary = GunGameConfig.instance.weapons.weapons[kit].GetUnturnedItem();
            Item secondary = UnturnedItems.AssembleItem(GunGameConfig.instance.weapons.secondary, 1, 100, null);
            Item mag = UnturnedItems.AssembleItem(GunGameConfig.instance.weapons.weapons[kit].mag, GunGameConfig.instance.weapons.weapons[kit].ammo, 100, null);

            Player.Inventory.items[0].tryAddItem(primary);
            Player.Inventory.items[1].tryAddItem(secondary);
            Player.Inventory.items[2].tryAddItem(mag);
            Player.Inventory.items[2].tryAddItem(mag);

            Player.Player.equipment.tryEquip(0, 0, 0);
        }

        public void MaxSkills()
        {
            Player.SetSkillLevel(UnturnedSkill.Agriculture, 255);
            Player.SetSkillLevel(UnturnedSkill.Cardio, 255);
            Player.SetSkillLevel(UnturnedSkill.Cooking, 255);
            Player.SetSkillLevel(UnturnedSkill.Crafting, 255);
            Player.SetSkillLevel(UnturnedSkill.Dexerity, 255);
            Player.SetSkillLevel(UnturnedSkill.Diving, 255);
            Player.SetSkillLevel(UnturnedSkill.Engineer, 255);
            Player.SetSkillLevel(UnturnedSkill.Exercise, 255);
            Player.SetSkillLevel(UnturnedSkill.Fishing, 255);
            Player.SetSkillLevel(UnturnedSkill.Healing, 255);
            Player.SetSkillLevel(UnturnedSkill.Immunity, 255);
            Player.SetSkillLevel(UnturnedSkill.Mechanic, 255);
            Player.SetSkillLevel(UnturnedSkill.Outdoors, 255);
            Player.SetSkillLevel(UnturnedSkill.Overkill, 255);
            Player.SetSkillLevel(UnturnedSkill.Parkour, 255);
            Player.SetSkillLevel(UnturnedSkill.Sharpshooter, 255);
            Player.SetSkillLevel(UnturnedSkill.Sneakybeaky, 255);
            Player.SetSkillLevel(UnturnedSkill.Strength, 255);
            Player.SetSkillLevel(UnturnedSkill.Survival, 255);
            Player.SetSkillLevel(UnturnedSkill.Toughness, 255);
            Player.SetSkillLevel(UnturnedSkill.Vitality, 255);
            Player.SetSkillLevel(UnturnedSkill.Warmblooded, 255);
        }
    }
}