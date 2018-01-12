using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Items;
using Rocket.Unturned.Player;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using SDG.Unturned;

using GunGame;
using GunGame.API;
using GunGame.Managers;

using Steamworks;

using RocketLogger = Rocket.Core.Logging.Logger;

//Random comment
namespace GunGame
{
    public class GunGame : RocketPlugin
    {
        static GunGame instance;

        static bool isLoaded;
        static bool wasUnloaded;

        public static bool IsMySqlEnabled = true;

        //This is for a future update :D
        //const string SteamApiKey = "D57A6B0437CB735FFEE9317A9D42CCAA";

        protected override void Load()
        {
            if (wasUnloaded)
                UnloadPlugin();
            else
            {
                instance = this;

                GunGameConfig.Initialize();
                GameManager.Initialize();
                CommandManager.Initialize();

                if (!SQLManager.Initialize())
                {
                    GunGamePlayerConfig.Initialize();
                    IsMySqlEnabled = false;
                }

                EventManager.Register();

                RocketLogger.Log(string.Format("Welcome to Gun Game v{0}!", Assembly.GetName().Version.ToString()), ConsoleColor.Yellow);
                //RocketLogger.Log("To start the timer, use /gg start!", ConsoleColor.Yellow);

#pragma warning disable RECS0018
                if (GunGameConfig.instance.positions[0].x == 0 && GunGameConfig.instance.positions[0].y == 0 && GunGameConfig.instance.positions[0].z == 0)
                    RocketLogger.Log("NOTE: You have not set the spawn positions yet!", ConsoleColor.Yellow);

                if (GunGameConfig.instance.safezone.x == 0 && GunGameConfig.instance.safezone.y == 0 && GunGameConfig.instance.safezone.z == 0)
                    RocketLogger.Log("NOTE: You have not set the lobby yet!", ConsoleColor.Yellow);

                isLoaded = true;
            }
        }

        protected override void Unload()
        {
            if (!wasUnloaded)
                RocketLogger.LogError("Reloading plugin is unsupported! Plugin will not load until server is restarted.");

            EventManager.Unregister();

            if (IsMySqlEnabled)
                foreach (ulong player in GameManager.OnlinePlayers)
                    SQLManager.SavePlayer(player, player.GetPlayer().GunGamePlayer().data);

            wasUnloaded = true;
            isLoaded = false;
        }

        void FixedUpdate()
        {
            if (isLoaded)
                GameManager.Update();
        }

        public override TranslationList DefaultTranslations
        {
            get
            {
                return new TranslationList
                {
                    {"kill", "{0} [{1}] {2}"},
                    {"first", "{0} placed first with {1} kills and {2} deaths!"},
                    {"second", "{0} placed second with {1} kills and {2} deaths!"},
                    {"third", "{0} placed third with {1} kills and {2} deaths!"},
                    {"notenoughplayers", "The next match will start once {0} more players have joined."},
                    {"inprogress", "The game is currently in progress, it will end in {0} seconds."},
                    {"next", "Next round will start in {0} seconds!"},
                    {"mute", "Global chat has been disabled! Please use area."},
                    {"register_lobby", "You have set the lobby to your current position!"},
                    {"register_spawn", "You have created a spawnpoint at your current position!"},
                    {"begin", "The game has begun!"},
                    {"invalid_perms", "This command requires a permission level of {0}!"},
                    {"invalid_args", "You have specified an invalid argument!"},
                    {"invalid_cmd", "You have specified an invalid command!"},
                    {"invalid_time", "This command can only be called when the game is {0}!"},
                    {"forcestop", "You have forced the game to stop! Use '/gg start' to resume the timer."},
                    {"forcestart", "You have forced the game to start!"},
                    {"start", "You have resumed the timer."},
                    {"stop", "You have paused the timer."},
                    {"stopped", "The game is currently stopped!"},
                    {"invalid_start", "The timer is already running!"},
                    {"invalid_stop", "The timer is already paused!"},
                    {"help", "{0} : {1}"},
                    {"invalid_cmd_help", "We couldn't find any info on your requested command!"},
                    {"invalid_perms_help", "You don't have permission to view info on your requested command!"},
                    {"kit", "You have been givin kit {0}!"}
                };
            }
        }

        public static void Say(ulong player, string message, Color color, params object[] objs)
        {
            UnturnedChat.Say(new CSteamID(player), instance.Translate(message, objs), color);
        }

        public static void Say(IRocketPlayer player, string message, Color color, params object[] objs)
        {
            UnturnedChat.Say(player, instance.Translate(message, objs), color);
        }

        public static void Say(string message, Color color, params object[] objs)
        {
            UnturnedChat.Say(instance.Translate(message, objs), color);
        }
    }

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
                    instance = new GunGamePlayerConfig();
                    instance.JoinedPlayers = new List<ulong>();
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
        static string Directory = Rocket.Core.Environment.PluginsDirectory + "/GunGame/Config.json";
        static string DirectoryFail = Rocket.Core.Environment.PluginsDirectory + "/GunGame/Config_errored.json";

        public static void RegisterSpawnPosition(Vector3 vector)
        {
            if (instance.positions[0].x == 0 && instance.positions[0].y == 0 && instance.positions[0].z == 0)
                instance.positions = new Vec3[0];

            List<Vec3> vectors = new List<Vec3>();

            vectors.AddRange(instance.positions);
            vectors.Add(new Vec3(vector));

            instance.positions = vectors.ToArray();

            SaveConfigFile();
        }
#pragma warning restore RECS0018

        public static void RegisterSafezone(Vector3 vect)
        {
            instance.safezone = new Vec3(vect);

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
                catch (Exception e)
                {
                    RocketLogger.LogException(e, null);
                    RocketLogger.LogWarning("Config failed to load, reverting to default settings...");
                    File.WriteAllText(DirectoryFail, file);
                    LoadDefaultConfig();
                    SaveConfigFile();
                }
            }
            else
            {
                LoadDefaultConfig();
                SaveConfigFile();
            }
        }

        public static void LoadDefaultConfig()
        {
            GunGameConfig config = new GunGameConfig
            {
                maxRoundTime = 600,
                minPlayers = 8,
                broadcastKills = true,
                maxSkills = true,
                kickGroup = true,
                mutePlayers = true,
                positions = new Vec3[] { new Vec3(new Vector3(0, 0, 0)) },
                safezone = new Vec3(new Vector3(0, 0, 0)),
                weapons = new WeaponSettings()
            };

            Weapon maplestrike = new Weapon { ammo = 30, barrel = 0, grip = 8, id = 363, mag = 6, sight = 364, tactical = 0, mode = EFiremode.AUTO };
            Weapon grizzly = new Weapon { ammo = 5, barrel = 0, grip = 143, id = 297, mag = 298, sight = 21, tactical = 0, mode = EFiremode.SEMI };

            config.weapons.secondary = 121;
            config.weapons.weapons = new Weapon[] { maplestrike, grizzly };

            config.sqlSettings = new MySqlSettings("unturned", "localhost", 3306, "root", "toor");

            instance = config;
        }

        public static void SaveConfigFile()
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

        [JsonProperty(PropertyName = "MutePlayers")]
        public bool mutePlayers;

        [JsonProperty(PropertyName = "KickGroupedPlayers")]
        public bool kickGroup;

        [JsonProperty(PropertyName = "Safezone")]
        public Vec3 safezone;

        [JsonProperty(PropertyName = "SpawnPositions")]
        public Vec3[] positions;

        [JsonProperty(PropertyName = "MySqlSettings")]
        public MySqlSettings sqlSettings;

        [JsonProperty(PropertyName = "WeaponSettings")]
        public WeaponSettings weapons;

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

        public struct Weapon
        {
            [JsonProperty(PropertyName = "ID")]
            public ushort id;

            [JsonProperty(PropertyName = "Ammo")]
            public byte ammo;

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

            public Weapon(ushort id, byte ammo, ushort mag, ushort sight, ushort tactical, ushort grip, ushort barrel, EFiremode mode)
            {
                this.id = id;
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
            [JsonProperty(PropertyName = "Database")]
            public string database;

            [JsonProperty(PropertyName = "IP")]
            public string address;

            [JsonProperty(PropertyName = "Port")]
            public ushort port;

            [JsonProperty(PropertyName = "Username")]
            public string user;

            [JsonProperty(PropertyName = "Password")]
            public string pass;

            public MySqlSettings(string database, string address, ushort port, string user, string pass)
            {
                this.database = database;
                this.address = address;
                this.port = port;
                this.user = user;
                this.pass = pass;
            }
        }

        public struct Vec3
        {
            public float x;

            public float y;

            public float z;

            [JsonIgnore]
            public Vector3 Vector3
            {
                get { return new Vector3(x, y, z); }
            }

            public Vec3(Vector3 vector)
            {
                x = vector.x;
                y = vector.y;
                z = vector.z;
            }
        }
    }
}

namespace Rocket.API
{
    public static class IRocketPlayerExtensions
    {
        public static bool HasPermissionFor(this IRocketPlayer player, IGunGameCommand command)
        {
            return (byte)command.PermissionLevel <= (byte)((UnturnedPlayer)player).GunGamePlayer().pLevel;
        }
    }
}

namespace Rocket.Unturned.Player
{
    public static class UnturnedPlayerExtensions
    {
        public static GunGamePlayerComponent GunGamePlayer(this UnturnedPlayer player)
        {
            return player.GetComponent<GunGamePlayerComponent>();
        }
    }
}

namespace System
{
    public static class UInt64Extensions
    {
        public static UnturnedPlayer GetPlayer(this ulong id)
        {
            return UnturnedPlayer.FromCSteamID(new CSteamID(id));
        }
    }

    public static class EnumExtensions
    {
        public static bool HasFlags(this Enum x, Enum y)
        {
            byte _x = Convert.ToByte(x);
            byte _y = Convert.ToByte(y);

            return (_x & _y) == _y;
        }
    }
}