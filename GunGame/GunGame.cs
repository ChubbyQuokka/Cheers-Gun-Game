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

                RocketLogger.Log(string.Format("Welcome to Gun Game v{0}!", Assembly.GetName().Version), ConsoleColor.Yellow);

                if (GunGameConfig.instance.sqlSettings.enabled)
                {
                    if (!SQLManager.Initialize())
                    {
                        GunGamePlayerConfig.Initialize();
                        IsMySqlEnabled = false;
                        RocketLogger.Log("NOTE: Connection to MySQL database failed!", ConsoleColor.Yellow);
                        RocketLogger.Log("Initialized with MySQL support disabled.", ConsoleColor.Yellow);
                    }
                    else
                    {
                        RocketLogger.Log("Initialized with MySQL support enabled.", ConsoleColor.Yellow);
                    }
                }
                else
                {
                    GunGamePlayerConfig.Initialize();
                    IsMySqlEnabled = false;
                    RocketLogger.Log("Initialized with MySQL support disabled.", ConsoleColor.Yellow);
                }

                EventManager.Register();

#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                if (GunGameConfig.instance.positions[0].x == 0 && GunGameConfig.instance.positions[0].y == 0 && GunGameConfig.instance.positions[0].z == 0)
                    RocketLogger.Log("NOTE: You have not set any spawn positions yet!", ConsoleColor.Yellow);

                if (GunGameConfig.instance.safezone.x == 0 && GunGameConfig.instance.safezone.y == 0 && GunGameConfig.instance.safezone.z == 0)
                    RocketLogger.Log("NOTE: You have not set the lobby yet!", ConsoleColor.Yellow);
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                isLoaded = true;
            }
        }

        protected override void Unload()
        {
            if (!wasUnloaded)
                RocketLogger.LogError("Unloading plugin is unsupported! The olugin will not reload until server is restarted.");

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
}