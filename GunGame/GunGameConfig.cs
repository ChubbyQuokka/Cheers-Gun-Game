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

        static string Directory = Rocket.Core.Environment.PluginsDirectory + "/GunGame/Players.json";

        public static void Initialize()
        {
            if (File.Exists(Directory))
            {
                string p = File.ReadAllText(Directory);

                try
                {
                    instance = JsonConvert.DeserializeObject<GunGamePlayerConfig>(p);
                }
                catch
                {
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
            if (!Contains(p))
            {
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
            }
        };

        static string Directory = Rocket.Core.Environment.PluginsDirectory + "/GunGame/Config.json";
        static string DirectoryFail = Rocket.Core.Environment.PluginsDirectory + "/GunGame/Config_";

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
            if (File.Exists(Directory))
            {
                string file = File.ReadAllText(Directory);

                try
                {
                    instance = JsonConvert.DeserializeObject<GunGameConfig>(file);
                }
                catch
                {
                    try
                    {
                        JObject jObj = JObject.Parse(file);
                        instance = new GunGameConfig();

                        //All basic assignments
                        Assign(jObj, "MinimumPlayers", ref instance.minPlayers, Default.minPlayers);
                        Assign(jObj, "RoundTime", ref instance.maxRoundTime, Default.maxRoundTime);
                        Assign(jObj, "BroadcastKills", ref instance.broadcastKills, Default.broadcastKills);
                        Assign(jObj, "MaxSkills", ref instance.maxSkills, Default.maxSkills);
                        Assign(jObj, "MuteGlobalChat", ref instance.mutePlayers, Default.mutePlayers);
                        Assign(jObj, "ForceNoGroups", ref instance.forceNoGroup, Default.forceNoGroup);
                        Assign(jObj, "Lobby", ref instance.safezone, Default.safezone);
                        Assign(jObj, "SpawnPositions", ref instance.positions, Default.positions);
                        Assign(jObj, "DisableCosmetics", ref instance.disableCosmetics, Default.disableCosmetics);

                        instance.sqlSettings = new MySqlSettings();
                        JObject sql = (JObject)jObj["MySqlSettings"];
                        ref MySqlSettings sqlSettings = ref instance.sqlSettings;

                        //MySQL assignments
                        Assign(sql, "MySqlEnabled", ref sqlSettings.enabled, Default.sqlSettings.enabled);
                        Assign(sql, "Database", ref sqlSettings.database, Default.sqlSettings.database);
                        Assign(sql, "Table", ref sqlSettings.table, Default.sqlSettings.table);
                        Assign(sql, "IP", ref sqlSettings.address, Default.sqlSettings.address);
                        Assign(sql, "Port", ref sqlSettings.port, Default.sqlSettings.port);
                        Assign(sql, "Username", ref sqlSettings.user, Default.sqlSettings.user);
                        Assign(sql, "Password", ref sqlSettings.pass, Default.sqlSettings.pass);

                        instance.weapons = new WeaponSettings();
                        JObject weapon = (JObject)jObj["WeaponSettings"];
                        ref WeaponSettings weaponSettings = ref instance.weapons;

                        //Weapon assignments
                        Assign(weapon, "Secondary", ref weaponSettings.secondary, Default.weapons.secondary);
                        Assign(weapon, "Helmet", ref weaponSettings.hat, Default.weapons.hat);
                        Assign(weapon, "Mask", ref weaponSettings.mask, Default.weapons.mask);
                        Assign(weapon, "Vest", ref weaponSettings.vest, Default.weapons.vest);
                        Assign(weapon, "Shirt", ref weaponSettings.shirt, Default.weapons.shirt);
                        Assign(weapon, "Pants", ref weaponSettings.pants, Default.weapons.pants);
                        Assign(weapon, "PrimaryLadder", ref weaponSettings.weapons, Default.weapons.weapons);

                        //Advanced assignments
                        instance.advSettings = new AdvanceSettings();
                        JObject adv = (JObject)jObj["AdvancedSettings"];
                        ref AdvanceSettings advSettings = ref instance.advSettings;

                        Assign(adv, "RespawnTeleportTime", ref advSettings.tpTime, Default.advSettings.tpTime);
                        Assign(adv, "RespawnKitTime", ref advSettings.kitTime, Default.advSettings.kitTime);
                        Assign(adv, "KitEquipTime", ref advSettings.equipTime, Default.advSettings.equipTime);

                        SaveConfigFile();

                    }
                    catch (Exception e)
                    {
                        RocketLogger.LogException(e, null);
                        RocketLogger.LogWarning("Config failed to load, reverting to default settings...");
                        File.WriteAllText($"{DirectoryFail}{DateTime.UtcNow.ToShortTimeString()}.json", file);
                        LoadDefaultConfig();
                        SaveConfigFile();
                    }
                }
            }
            else
            {
                LoadDefaultConfig();
                SaveConfigFile();
            }

            //Ensure that no player can change their group
            if (instance.forceNoGroup)
            {
                switch (Provider.mode)
                {
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

        static void Assign<T>(JObject jObj, string key, ref T assignee, T def)
        {
            assignee = jObj[key].HasValues ? jObj[key].Value<T>() : def;
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

        [JsonProperty(PropertyName = "Lobby")]
        public SpawnPosition safezone;

        [JsonProperty(PropertyName = "SpawnPositions")]
        public SpawnPosition[] positions;

        [JsonProperty(PropertyName = "MySqlSettings")]
        public MySqlSettings sqlSettings;

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

        public struct MySqlSettings
        {
            [JsonProperty(PropertyName = "MySqlEnabled")]
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