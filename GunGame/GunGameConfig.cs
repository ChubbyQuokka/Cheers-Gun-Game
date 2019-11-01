using System;
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

using Rocket.Unturned.Items;
using RocketLogger = Rocket.Core.Logging.Logger;

using SDG.Unturned;

using UnityEngine;

namespace GunGame
{
    public class GunGamePlayerConfig
    {
        static GunGamePlayerConfig instance;

        public List<ulong> JoinedPlayers;

        public static string Directory = Rocket.Core.Environment.PluginsDirectory + "/GunGame/Players.json";

        public static void Initialize()
        {
            if (File.Exists(Directory)) {
                string p = File.ReadAllText(Directory);

                try {
                    instance = JsonConvert.DeserializeObject<GunGamePlayerConfig>(p);
                } catch {
                    instance = new GunGamePlayerConfig
                    {
                        JoinedPlayers = new List<ulong>()
                    };
                }
            }
        }

        public static void Save()
        {
            string file = JsonConvert.SerializeObject(instance);

            File.WriteAllText(Directory, file);
        }

        public static void AddPlayer(ulong p)
        {
            if (!Contains(p)) {
                instance.JoinedPlayers.Add(p);
                Save();
            }
        }

        public static bool Contains(ulong p)
        {
            return instance.JoinedPlayers.Contains(p);
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class GunGameConfig
    {
        public static GunGameConfig instance;

        public static readonly GunGameConfig Default = new GunGameConfig
        {
            maxRoundTime = 600,
            minPlayers = 3,
            broadcastKills = true,
            maxSkills = true,
            forceNoGroup = true,
            mutePlayers = true,
            disableCosmetics = true,
            positions = new SpawnPosition[] { new SpawnPosition(new Vector3(0, 0, 0), 0) },
            safezone = new SpawnPosition(new Vector3(0, 0, 0), 0),
            sqlSettings = new MySqlSettings(false, "unturned", "gungame", "localhost", 3306, "root", "toor"),
            weapons = new WeaponSettings
            {
                secondary = 121,
                weapons = new Weapon[]
                {
                    new Weapon { magAmt = 2, ammo = 30, barrel = 0, grip = 8, id = 363, mag = 6, sight = 364, tactical = 0, mode = EFiremode.AUTO},
                    new Weapon { magAmt = 2, ammo = 5, barrel = 0, grip = 143, id = 297, mag = 298, sight = 21, tactical = 0, mode = EFiremode.SEMI }
                }
            },

            advSettings = new AdvanceSettings
            {
                tpTime = 3f,
                kitTime = 0.05f,
                equipTime = 0.05f
            },

            econSettings = new EconomySettings
            {
                enabled = false,
                rewards = new Reward[]
                {
                    new Reward(1, "st", 500),
                    new Reward(2, "nd", 100),
                    new Reward(3, "rd", 50)
                }
            }
        };

        public static string Directory = Rocket.Core.Environment.PluginsDirectory + "/GunGame/Config.json";
        public static string DirectoryFail = Rocket.Core.Environment.PluginsDirectory + "/GunGame/Config_";

#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
        public static void RegisterSpawnPosition(Vector3 vector, float rot)
        {
            if (instance.positions[0].x == 0 && instance.positions[0].y == 0 && instance.positions[0].z == 0 && rot == 0)
                instance.positions = new SpawnPosition[0];

            List<SpawnPosition> vectors = new List<SpawnPosition>();

            vectors.AddRange(instance.positions);
            vectors.Add(new SpawnPosition(vector, rot));

            instance.positions = vectors.ToArray();

            SaveConfigFile();
        }
#pragma warning restore RECS0018

        public static void RegisterSafezone(Vector3 vect, float rot)
        {
            instance.safezone = new SpawnPosition(vect, rot);

            SaveConfigFile();
        }

        public static void Initialize()
        {
            if (File.Exists(Directory)) {
                string file = File.ReadAllText(Directory);

                try {
                    JObject jObj = JObject.Parse(file);
                    instance = new GunGameConfig();

                    bool hasDefaulted = false;

                    //All basic assignments
                    Assign(jObj, "MinimumPlayers", ref instance.minPlayers, Default.minPlayers, ref hasDefaulted);
                    Assign(jObj, "RoundTime", ref instance.maxRoundTime, Default.maxRoundTime, ref hasDefaulted);
                    Assign(jObj, "BroadcastKills", ref instance.broadcastKills, Default.broadcastKills, ref hasDefaulted);
                    Assign(jObj, "MaxSkills", ref instance.maxSkills, Default.maxSkills, ref hasDefaulted);
                    Assign(jObj, "MuteGlobalChat", ref instance.mutePlayers, Default.mutePlayers, ref hasDefaulted);
                    Assign(jObj, "ForceNoGroups", ref instance.forceNoGroup, Default.forceNoGroup, ref hasDefaulted);
                    Assign(jObj, "DisableCosmetics", ref instance.disableCosmetics, Default.disableCosmetics, ref hasDefaulted);

                    //Assign(jObj, "Lobby", ref instance.safezone, Default.safezone, ref hasDefaulted);
                    //Assign(jObj, "SpawnPositions", ref instance.positions, Default.positions);

                    JObject lobby = (JObject)jObj["Safezone"];

                    if (lobby != null) {
                        instance.safezone = new SpawnPosition();
                        ref SpawnPosition lobbySettings = ref instance.safezone;

                        Assign(lobby, "x", ref lobbySettings.x, 0f, ref hasDefaulted);
                        Assign(lobby, "y", ref lobbySettings.y, 0f, ref hasDefaulted);
                        Assign(lobby, "z", ref lobbySettings.z, 0f, ref hasDefaulted);
                        Assign(lobby, "rot", ref lobbySettings.rot, 0f, ref hasDefaulted);
                    } else {
                        instance.safezone = Default.safezone;
                        hasDefaulted = true;
                    }

                    JObject econ = (JObject)jObj["EconomySettings"];

                    if (econ != null) {
                        instance.econSettings = new EconomySettings();
                        ref EconomySettings econSettings = ref instance.econSettings;

                        Assign(econ, "Enabled", ref econSettings.enabled, false, ref hasDefaulted);

                        JArray rewards = (JArray)econ["Rewards"];

                        if (rewards != null) {
                            ref Reward[] rewardsSettings = ref econSettings.rewards;
                            rewardsSettings = new Reward[rewards.Count];

                            for (int i = 0; i < rewards.Count; i++) {
                                Reward temp = new Reward();
                                JObject tempObj = (JObject)rewards[i];

                                Assign(tempObj, "Place", ref temp.place, (byte)0, ref hasDefaulted);
                                Assign(tempObj, "OrdinalIndicator", ref temp.ordinal, string.Empty, ref hasDefaulted);
                                Assign(tempObj, "Reward", ref temp.reward, 0, ref hasDefaulted);

                                rewardsSettings[i] = temp;
                            }
                        } else {
                            econSettings.rewards = Default.econSettings.rewards;
                            hasDefaulted = true;
                        }
                    } else {
                        instance.econSettings = Default.econSettings;
                        hasDefaulted = true;
                    }

                    JArray spawns = (JArray)jObj["SpawnPositions"];

                    if (spawns != null) {
                        ref SpawnPosition[] spawnSettings = ref instance.positions;
                        spawnSettings = new SpawnPosition[spawns.Count];

                        for (int i = 0; i < spawns.Count; i++) {
                            SpawnPosition temp = new SpawnPosition();
                            JObject tempObj = (JObject)spawns[i];

                            Assign(tempObj, "x", ref temp.x, 0f, ref hasDefaulted);
                            Assign(tempObj, "y", ref temp.y, 0f, ref hasDefaulted);
                            Assign(tempObj, "z", ref temp.z, 0f, ref hasDefaulted);
                            Assign(tempObj, "rot", ref temp.rot, 0f, ref hasDefaulted);

                            spawnSettings[i] = temp;
                        }
                    } else {
                        instance.positions = Default.positions;
                        hasDefaulted = true;
                    }

                    JObject sql = (JObject)jObj["MySqlSettings"];

                    if (sql != null) {
                        instance.sqlSettings = new MySqlSettings();
                        ref MySqlSettings sqlSettings = ref instance.sqlSettings;

                        //MySQL assignments
                        Assign(sql, "Enabled", ref sqlSettings.enabled, Default.sqlSettings.enabled, ref hasDefaulted);
                        Assign(sql, "Database", ref sqlSettings.database, Default.sqlSettings.database, ref hasDefaulted);
                        Assign(sql, "Table", ref sqlSettings.table, Default.sqlSettings.table, ref hasDefaulted);
                        Assign(sql, "IP", ref sqlSettings.address, Default.sqlSettings.address, ref hasDefaulted);
                        Assign(sql, "Port", ref sqlSettings.port, Default.sqlSettings.port, ref hasDefaulted);
                        Assign(sql, "Username", ref sqlSettings.user, Default.sqlSettings.user, ref hasDefaulted);
                        Assign(sql, "Password", ref sqlSettings.pass, Default.sqlSettings.pass, ref hasDefaulted);
                    } else {
                        instance.sqlSettings = Default.sqlSettings;
                        hasDefaulted = true;
                    }

                    JObject weapon = (JObject)jObj["WeaponSettings"];
                    if (weapon != null) {
                        instance.weapons = new WeaponSettings();
                        ref WeaponSettings weaponSettings = ref instance.weapons;

                        //Weapon assignments
                        Assign(weapon, "Secondary", ref weaponSettings.secondary, Default.weapons.secondary, ref hasDefaulted);
                        Assign(weapon, "Helmet", ref weaponSettings.hat, Default.weapons.hat, ref hasDefaulted);
                        Assign(weapon, "Mask", ref weaponSettings.mask, Default.weapons.mask, ref hasDefaulted);
                        Assign(weapon, "Vest", ref weaponSettings.vest, Default.weapons.vest, ref hasDefaulted);
                        Assign(weapon, "Shirt", ref weaponSettings.shirt, Default.weapons.shirt, ref hasDefaulted);
                        Assign(weapon, "Pants", ref weaponSettings.pants, Default.weapons.pants, ref hasDefaulted);

                        JArray ladder = (JArray)weapon["PrimaryLadder"];

                        if (ladder != null) {
                            ref Weapon[] ladderSettings = ref instance.weapons.weapons;
                            ladderSettings = new Weapon[ladder.Count];

                            for (int i = 0; i < ladderSettings.Length; i++) {
                                Weapon temp = new Weapon();
                                JObject tempObj = (JObject)ladder[i];

                                Assign(tempObj, "ID", ref temp.id, (ushort)0, ref hasDefaulted);
                                Assign(tempObj, "Ammo", ref temp.ammo, (byte)0, ref hasDefaulted);
                                Assign(tempObj, "Magazine", ref temp.mag, (ushort)0, ref hasDefaulted);
                                Assign(tempObj, "MagazineAmount", ref temp.magAmt, (byte)2, ref hasDefaulted);
                                Assign(tempObj, "Sight", ref temp.sight, (ushort)0, ref hasDefaulted);
                                Assign(tempObj, "Tactical", ref temp.tactical, (ushort)0, ref hasDefaulted);
                                Assign(tempObj, "Grip", ref temp.grip, (ushort)0, ref hasDefaulted);
                                Assign(tempObj, "Barrel", ref temp.barrel, (ushort)0, ref hasDefaulted);

                                string firemode = "";
                                Assign(tempObj, "Firemode", ref firemode, "SAFETY", ref hasDefaulted);

                                switch (firemode.ToLowerInvariant()) {
                                    case "safety":
                                        temp.mode = EFiremode.SAFETY;
                                        break;
                                    case "semi":
                                        temp.mode = EFiremode.SEMI;
                                        break;
                                    case "auto":
                                        temp.mode = EFiremode.AUTO;
                                        break;
                                    case "burst":
                                        temp.mode = EFiremode.BURST;
                                        break;
                                    default:
                                        temp.mode = EFiremode.SAFETY;
                                        hasDefaulted = true;
                                        break;
                                }

                                ladderSettings[i] = temp;
                            }
                        } else {
                            instance.weapons.weapons = Default.weapons.weapons;
                            hasDefaulted = true;
                        }
                    } else {
                        instance.weapons = Default.weapons;
                        hasDefaulted = true;
                    }

                    //Advanced assignments
                    JObject adv = (JObject)jObj["AdvancedSettings"];

                    if (adv != null) {
                        instance.advSettings = new AdvanceSettings();
                        ref AdvanceSettings advSettings = ref instance.advSettings;

                        Assign(adv, "RespawnTeleportTime", ref advSettings.tpTime, Default.advSettings.tpTime, ref hasDefaulted);
                        Assign(adv, "RespawnKitTime", ref advSettings.kitTime, Default.advSettings.kitTime, ref hasDefaulted);
                        Assign(adv, "KitEquipTime", ref advSettings.equipTime, Default.advSettings.equipTime, ref hasDefaulted);
                    } else {
                        instance.advSettings = Default.advSettings;
                        hasDefaulted = true;
                    }

                    if (hasDefaulted) {
                        File.WriteAllText($"{DirectoryFail}{GunGame.UnixTimestamp}.json", file);
                        SaveConfigFile();
                        RocketLogger.Log($"ERROR: Some keys inside the config were reverted to their default value. A copy of your old config has been saved as Config_{GunGame.UnixTimestamp}.json!", ConsoleColor.Red);
                    }

                } catch (Exception e) {
                    RocketLogger.LogException(e, null);
                    RocketLogger.Log($"ERROR: Config failed to load, everything will revert to default. A copy of your old config has been saved as Config_{GunGame.UnixTimestamp}.json!", ConsoleColor.Red);
                    File.WriteAllText($"{DirectoryFail}{GunGame.UnixTimestamp}.json", file);
                    LoadDefaultConfig();
                    SaveConfigFile();
                }
            } else {
                LoadDefaultConfig();
                SaveConfigFile();
            }

            //Ensure that no player can change their group
            if (instance.forceNoGroup) {
                switch (Provider.mode) {
                    case EGameMode.EASY:
                        Provider.configData.Easy.Gameplay.Allow_Static_Groups = false;
                        Provider.configData.Easy.Gameplay.Allow_Dynamic_Groups = false;
                        break;
                    case EGameMode.NORMAL:
                        Provider.configData.Normal.Gameplay.Allow_Static_Groups = false;
                        Provider.configData.Normal.Gameplay.Allow_Dynamic_Groups = false;
                        break;
                    case EGameMode.HARD:
                        Provider.configData.Hard.Gameplay.Allow_Static_Groups = false;
                        Provider.configData.Hard.Gameplay.Allow_Dynamic_Groups = false;
                        break;
                }
            }
        }

        static void Assign<T>(JObject jObj, string key, ref T assignee, T def, ref bool wasDefaulted)
        {
            try {
                if (jObj[key].Value<object>() != null) {
                    assignee = jObj[key].Value<T>();
                } else {
                    assignee = def;
                    wasDefaulted = true;
                }
            } catch {
                assignee = def;
                wasDefaulted = true;
            }
        }

        static void LoadDefaultConfig()
        {
            instance = Default;
        }

        static void SaveConfigFile()
        {
            string json = JsonConvert.SerializeObject(instance, Formatting.Indented);

            File.WriteAllText(Directory, json);
        }

        [JsonProperty(PropertyName = "MinimumPlayers")]
        public int minPlayers;

        [JsonProperty(PropertyName = "RoundTime")]
        public int maxRoundTime;

        [JsonProperty(PropertyName = "BroadcastKills")]
        public bool broadcastKills;

        [JsonProperty(PropertyName = "MaxSkills")]
        public bool maxSkills;

        [JsonProperty(PropertyName = "MuteGlobalChat")]
        public bool mutePlayers;

        [JsonProperty(PropertyName = "ForceNoGroups")]
        public bool forceNoGroup;

        [JsonProperty(PropertyName = "DisableCosmetics")]
        public bool disableCosmetics;

        [JsonProperty(PropertyName = "Safezone")]
        public SpawnPosition safezone;

        [JsonProperty(PropertyName = "EconomySettings")]
        public EconomySettings econSettings;

        [JsonProperty(PropertyName = "MySqlSettings")]
        public MySqlSettings sqlSettings;

        [JsonProperty(PropertyName = "SpawnPositions")]
        public SpawnPosition[] positions;

        [JsonProperty(PropertyName = "WeaponSettings")]
        public WeaponSettings weapons;

        [JsonProperty(PropertyName = "AdvancedSettings")]
        public AdvanceSettings advSettings;

        public struct WeaponSettings
        {
            [JsonProperty(PropertyName = "Secondary")]
            public ushort secondary;

            [JsonProperty(PropertyName = "Helmet")]
            public ushort hat;

            [JsonProperty(PropertyName = "Mask")]
            public ushort mask;

            [JsonProperty(PropertyName = "Vest")]
            public ushort vest;

            [JsonProperty(PropertyName = "Shirt")]
            public ushort shirt;

            [JsonProperty(PropertyName = "Pants")]
            public ushort pants;

            [JsonProperty(PropertyName = "PrimaryLadder")]
            public Weapon[] weapons;

        }

        public struct AdvanceSettings
        {
            [JsonProperty(PropertyName = "RespawnTeleportTime")]
            public float tpTime;

            [JsonProperty(PropertyName = "RespawnKitTime")]
            public float kitTime;

            [JsonProperty(PropertyName = "KitEquipTime")]
            public float equipTime;
        }

        public struct Weapon
        {
            [JsonProperty(PropertyName = "ID")]
            public ushort id;

            [JsonProperty(PropertyName = "Ammo")]
            public byte ammo;

            [JsonProperty(PropertyName = "MagazineAmount")]
            public byte magAmt;

            [JsonProperty(PropertyName = "Magazine")]
            public ushort mag;

            [JsonProperty(PropertyName = "Sight")]
            public ushort sight;

            [JsonProperty(PropertyName = "Tactical")]
            public ushort tactical;

            [JsonProperty(PropertyName = "Grip")]
            public ushort grip;

            [JsonProperty(PropertyName = "Barrel")]
            public ushort barrel;

            [JsonConverter(typeof(StringEnumConverter))]
            [JsonProperty(PropertyName = "Firemode")]
            public EFiremode mode;

            public Weapon(ushort id, byte magAmt, byte ammo, ushort mag, ushort sight, ushort tactical, ushort grip, ushort barrel, EFiremode mode)
            {
                this.id = id;
                this.magAmt = magAmt;
                this.ammo = ammo;
                this.mag = mag;
                this.sight = sight;
                this.tactical = tactical;
                this.grip = grip;
                this.barrel = barrel;
                this.mode = mode;
            }

            public Item GetUnturnedItem()
            {
                return UnturnedItems.AssembleItem(id, ammo, new Attachment(sight, 100), new Attachment(tactical, 100), new Attachment(grip, 100), new Attachment(barrel, 100), new Attachment(mag, 100), mode, 1, 100);
            }
        }

        public struct EconomySettings
        {
            [JsonProperty(PropertyName = "Enabled")]
            public bool enabled;

            [JsonProperty(PropertyName = "Rewards")]
            public Reward[] rewards;
        }

        public struct Reward
        {
            [JsonProperty(PropertyName = "Place")]
            public byte place;

            [JsonProperty(PropertyName = "OrdinalIndicator")]
            public string ordinal;

            [JsonProperty(PropertyName = "Reward")]
            public decimal reward;

            public Reward(Byte place, string ordinal, decimal reward) {
                this.place = place;
                this.ordinal = ordinal;
                this.reward = reward;
            }
        }

        public struct MySqlSettings
        {
            [JsonProperty(PropertyName = "Enabled")]
            public bool enabled;

            [JsonProperty(PropertyName = "Database")]
            public string database;

            [JsonProperty(PropertyName = "Table")]
            public string table;

            [JsonProperty(PropertyName = "IP")]
            public string address;

            [JsonProperty(PropertyName = "Port")]
            public ushort port;

            [JsonProperty(PropertyName = "Username")]
            public string user;

            [JsonProperty(PropertyName = "Password")]
            public string pass;

            public MySqlSettings(bool enabled, string database, string table, string address, ushort port, string user, string pass)
            {
                this.enabled = enabled;
                this.database = database;
                this.table = table;
                this.address = address;
                this.port = port;
                this.user = user;
                this.pass = pass;
            }
        }

        public struct SpawnPosition
        {
            public float x;

            public float y;

            public float z;

            public float rot;

            [JsonIgnore]
            public Vector3 Vector3
            {
                get { return new Vector3(x, y, z); }
            }

            public SpawnPosition(Vector3 vector, float rot)
            {
                x = vector.x;
                y = vector.y;
                z = vector.z;
                this.rot = rot;
            }
        }
    }
}