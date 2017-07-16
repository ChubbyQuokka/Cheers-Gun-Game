using System.IO;
using System.Collections.Generic;

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

using GunGame.Managers;

namespace GunGame
{
	public class GunGame : RocketPlugin
	{
		static GunGame instance;

		const string SteamApiKey = "D57A6B0437CB735FFEE9317A9D42CCAA";

		protected override void Load()
		{
			instance = this;

			GunGameConfig.Initialize ();
			GameManager.Initialize ();
			SQLManager.Initialize ();

			EventManager.Register ();
		}

		protected override void Unload()
		{
			EventManager.Unregister ();
		}

		void FixedUpdate()
		{
			GameManager.Update ();
		}

		public override TranslationList DefaultTranslations
		{
			get {
				return new TranslationList
				{
					{"kill", "{0} [{1}] {2}"},
					{"first", "{0} has placed first!"},
					{"second", "{0} has placed second!"},
					{"third", "{0} has placed third!"},
					{"notenoughplayers", "The next match will start once {0} more players have joined."},
					{"inprogress", "The game is currently in progress, it will end in {0} seconds."},
					{"next", "Next round will start in {0} seconds!"},
					{"mute", "Global chat has been disabled! Please use area."},
					{"register_safezone", "You have set the safezone to your current position!"},
					{"register_spawn", "You have created a spawnpoint at your current position!"},
					{"begin", "The game has begun!"}
				};
			}
		}

		public static void Say(UnturnedPlayer player, string message, Color color, params object [] objs)
		{
			UnturnedChat.Say (player, instance.Translate (message, objs), color);
		}

		public static void Say(IRocketPlayer caller, string message, Color color, params object [] objs)
		{
			UnturnedChat.Say (caller, instance.Translate (message, objs), color);
		}

		public static void Say(string message, Color color, params object [] objs)
		{
			UnturnedChat.Say (instance.Translate (message, objs), color);
		}
	}

	[JsonObject (MemberSerialization.OptIn)]
	public class GunGameConfig
	{
		public static GunGameConfig instance;
		public static string Directory = Rocket.Core.Environment.PluginsDirectory + "/GunGame/Config.json";
		static string DirectoryFail = Rocket.Core.Environment.PluginsDirectory + "/GunGame/Config_errored.json";

		public static void RegisterSpawnPosition(Vector3 vector)
		{
			List<Vec3> vectors = new List<Vec3> ();

			vectors.AddRange (instance.positions);
			vectors.Add (new Vec3 (vector));

			instance.positions = vectors.ToArray ();

			SaveConfigFile ();
		}

		public static void RegisterSafezone(Vector3 vect)
		{
			instance.safezone = new Vec3 (vect);

			SaveConfigFile ();
		}

		public static void Initialize()
		{
			if (File.Exists (Directory)) {

				string file = File.ReadAllText (Directory);

				try {
					instance = (GunGameConfig)JsonConvert.DeserializeObject (file);
				} catch {
					Rocket.Core.Logging.Logger.LogWarning ("Config failed to load, reverting to default settings...");
					File.WriteAllText (DirectoryFail, file);
					LoadDefaultConfig ();
					SaveConfigFile ();
				}
			} else {
				LoadDefaultConfig ();
				SaveConfigFile ();
			}
		}

		public static void LoadDefaultConfig()
		{
			GunGameConfig config = new GunGameConfig ();

			config.maxRoundTime = 300;
			config.minPlayers = 8;
			config.broadcastKills = true;
			config.mutePlayers = true;
			config.positions = new Vec3 [] { new Vec3 (new Vector3 (0, 0, 0)) };
			config.safezone = new Vec3 (new Vector3 (0, 0, 0));

			config.weapons = new WeaponSettings ();
			config.weapons.secondary = 121;

			config.weapons.weapons = new Weapon [] { new Weapon { ammo = 30, barrel = 0, grip = 0, id = 363, mag = 6, sight = 364, tactical = 0 } };
			config.sqlSettings = new MySqlSettings ("unturned", "localhost", 3306, "root", "toor");

			instance = config;
		}

		public static void SaveConfigFile()
		{
			string json = JsonConvert.SerializeObject (instance, Formatting.Indented);

			File.WriteAllText (Directory, json);
		}

		[JsonProperty (PropertyName = "MinimumPlayers")]
		public int minPlayers;

		[JsonProperty (PropertyName = "RoundTime")]
		public int maxRoundTime;

		[JsonProperty (PropertyName = "BroadcastKills")]
		public bool broadcastKills;

		[JsonProperty (PropertyName = "MutePlayers")]
		public bool mutePlayers;

		[JsonProperty (PropertyName = "Safezone")]
		public Vec3 safezone;

		[JsonProperty (PropertyName = "SpawnPositions")]
		public Vec3 [] positions;

		[JsonProperty (PropertyName = "MySqlSettings")]
		public MySqlSettings sqlSettings;

		[JsonProperty (PropertyName = "WeaponSettings")]
		public WeaponSettings weapons;

		public struct WeaponSettings
		{
			[JsonProperty (PropertyName = "Secondary")]
			public ushort secondary;

			[JsonProperty (PropertyName = "Helmet")]
			public ushort hat;

			[JsonProperty (PropertyName = "Mask")]
			public ushort mask;

			[JsonProperty (PropertyName = "Vest")]
			public ushort vest;

			[JsonProperty (PropertyName = "Shirt")]
			public ushort shirt;

			[JsonProperty (PropertyName = "Pants")]
			public ushort pants;

			[JsonProperty (PropertyName = "PrimaryLadder")]
			public Weapon [] weapons;

		}

		public struct Weapon
		{
			[JsonProperty (PropertyName = "ID")]
			public ushort id;

			[JsonProperty (PropertyName = "Ammo")]
			public byte ammo;

			[JsonProperty (PropertyName = "Magazine")]
			public ushort mag;

			[JsonProperty (PropertyName = "Sight")]
			public ushort sight;

			[JsonProperty (PropertyName = "Tactical")]
			public ushort tactical;

			[JsonProperty (PropertyName = "Grip")]
			public ushort grip;

			[JsonProperty (PropertyName = "Barrel")]
			public ushort barrel;

			[JsonConverter (typeof (StringEnumConverter))]
			[JsonProperty (PropertyName = "Firemode")]
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
				return UnturnedItems.AssembleItem (id, ammo, new Attachment (sight, 100), new Attachment (tactical, 100), new Attachment (grip, 100), new Attachment (barrel, 100), new Attachment (mag, 100), mode, 1, 100);
			}
		}

		public struct MySqlSettings
		{
			[JsonProperty (PropertyName = "Database")]
			public string database;

			[JsonProperty (PropertyName = "IP")]
			public string address;

			[JsonProperty (PropertyName = "Port")]
			public ushort port;

			[JsonProperty (PropertyName = "Username")]
			public string user;

			[JsonProperty (PropertyName = "Password")]
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
				get { return new Vector3 (x, y, z); }
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