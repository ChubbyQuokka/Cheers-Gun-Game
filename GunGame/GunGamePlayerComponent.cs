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

        public void Start()
        {
            if (GunGame.IsMySqlEnabled) {
                data = SQLManager.LoadPlayer(Player.CSteamID.m_SteamID);
            } else {
                _ = !GunGamePlayerConfig.Contains(Player.CSteamID.m_SteamID);
            }

            if (R.Permissions.HasPermission(Player, new List<string> { "gungame.high" })) {
                pLevel = EPermissionLevel.HIGH;
            } else if (R.Permissions.HasPermission(Player, new List<string> { "gungame.medium" })) {
                pLevel = EPermissionLevel.MEDIUM;
            } else if (R.Permissions.HasPermission(Player, new List<string> { "gungame.low" })) {
                pLevel = EPermissionLevel.LOW;
            } else {
                pLevel = EPermissionLevel.NONE;
            }

            ClearItems();
        }

        public void EnterGame()
        {
            currentWeapon = 0;
            GiveKit(currentWeapon);

            if (GunGame.IsMySqlEnabled)
                data.rounds++;

            kills = 0;
            deaths = 0;
        }

        public void DeathCallback(bool wasByKnife)
        {
            if (GunGame.IsMySqlEnabled)
                data.deaths++;

            deaths++;

            ClearItems();

            if (wasByKnife && currentWeapon != 0) {
                currentWeapon--;
            }
        }

        public void RespawnCallback()
        {
            if (GunGameConfig.instance.maxSkills)
                Player.GunGamePlayer().MaxSkills();

            if (GameManager.isRunning) {
                GiveKit(currentWeapon);
                Invoke("TeleportAfterRespawn", GunGameConfig.instance.advSettings.tpTime);
            }
        }

        public void TeleportAfterRespawn()
        {
            if (GameManager.isRunning) {
                GunGameConfig.SpawnPosition sp = GameManager.GetSpawnPositionRR();
                Player.Teleport(sp.Vector3, sp.rot);
            }
        }

        public void KillCallback(bool wasWithKnife)
        {
            if (GunGame.IsMySqlEnabled)
                data.kills++;

            kills++;

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
        }

        public void ClearItems()
        {
            for (byte p = 0; p <= 7; p++) {
                byte amt = Player.Inventory.getItemCount(p);
                for (byte index = 0; index < amt; index++) {
                    Player.Inventory.removeItem(p, 0);
                }
            }

            Player.Player.clothing.askWearHat(0, 0, new byte[0], false);
            for (byte i = 0; i < Player.Inventory.getItemCount(2); i++)
                Player.Inventory.removeItem(2, 0);

            Player.Player.clothing.askWearShirt(0, 0, new byte[0], false);
            for (byte i = 0; i < Player.Inventory.getItemCount(2); i++)
                Player.Inventory.removeItem(2, 0);

            Player.Player.clothing.askWearPants(0, 0, new byte[0], false);
            for (byte i = 0; i < Player.Inventory.getItemCount(2); i++)
                Player.Inventory.removeItem(2, 0);

            Player.Player.clothing.askWearVest(0, 0, new byte[0], false);
            for (byte i = 0; i < Player.Inventory.getItemCount(2); i++)
                Player.Inventory.removeItem(2, 0);

            Player.Player.clothing.askWearMask(0, 0, new byte[0], false);
            for (byte i = 0; i < Player.Inventory.getItemCount(2); i++)
                Player.Inventory.removeItem(2, 0);

            Player.Player.clothing.askWearBackpack(0, 0, new byte[0], false);
            for (byte i = 0; i < Player.Inventory.getItemCount(2); i++)
                Player.Inventory.removeItem(2, 0);

            Player.Player.clothing.askWearGlasses(0, 0, new byte[0], false);
            for (byte i = 0; i < Player.Inventory.getItemCount(2); i++)
                Player.Inventory.removeItem(2, 0);
        }

        byte kitRequest;
        public void GiveKit(byte kit)
        {
            kitRequest = kit;
            Invoke("GiveKit", GunGameConfig.instance.advSettings.kitTime);
        }

        public void GiveKit()
        {
            Player.GiveItem(GunGameConfig.instance.weapons.hat, 1);
            Player.GiveItem(GunGameConfig.instance.weapons.mask, 1);
            Player.GiveItem(GunGameConfig.instance.weapons.vest, 1);
            Player.GiveItem(GunGameConfig.instance.weapons.pants, 1);
            Player.GiveItem(GunGameConfig.instance.weapons.shirt, 1);

            GunGameConfig.Weapon weapon = GunGameConfig.instance.weapons.weapons[kitRequest];

            Item primary = weapon.GetUnturnedItem();
            Item secondary = UnturnedItems.AssembleItem(GunGameConfig.instance.weapons.secondary, 1, 100, null);
            Item mag = UnturnedItems.AssembleItem(weapon.mag, weapon.ammo, 100, null);

            Player.Inventory.items[0].tryAddItem(primary);
            Player.Inventory.items[1].tryAddItem(secondary);

            for (int i = 0; i < weapon.magAmt; i++) {
                Player.Inventory.items[2].tryAddItem(mag);
            }

            Invoke("Equip", GunGameConfig.instance.advSettings.equipTime);
        }

        public void Equip()
        {
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