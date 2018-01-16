using System;
using System.Linq;
using System.Collections.Generic;

using Rocket.Unturned.Player;

using UnityEngine;

using SDG.Unturned;

namespace GunGame.Managers
{
    public static class GameManager
    {
        static int lastSpawn;
        public static int timer;

        public static bool isRunning;
        public static bool isStopped;

        public static List<ulong> OnlinePlayers;
        public static List<ulong> InGamePlayers;

        public static void Initialize()
        {
            isStopped = false;

            OnlinePlayers = new List<ulong>();
            InGamePlayers = new List<ulong>();

            timer = 0;
            lastSpawn = 0;
            isRunning = false;
        }

        public static int GetTime()
        {
            float t = timer;

            t /= 60f;

            return Mathf.RoundToInt(timer);
        }

        public static void Update()
        {
            if (isRunning)
            {
                if (timer <= 0)
                    RequestFinish();
                else
                    timer--;

            }
            else if (!isStopped)
            {
                if (OnlinePlayers.Count >= GunGameConfig.instance.minPlayers)
                {
                    if (timer > 0)
                    {
                        if (timer == 300)
                            GunGame.Say("next", Color.green, "5");
                        else if (timer == 1800)
                            GunGame.Say("next", Color.green, "30");
                        timer--;
                    }
                    else
                        RequestBegin();
                }
                else
                    timer = 1800;
            }
        }

        public static void RequestFinish()
        {
            isRunning = false;

            timer = 1800;

            foreach (ulong player in InGamePlayers)
            {
                player.GetPlayer().GunGamePlayer().ClearItems();
                player.GetPlayer().Teleport(GunGameConfig.instance.safezone.Vector3, GunGameConfig.instance.safezone.rot);
                player.GetPlayer().Heal(100);
            }

            IEnumerable<ulong> winners = from player in InGamePlayers
                                         orderby player.GetPlayer().GunGamePlayer().currentWeapon descending
                                         select player;

            UnturnedPlayer first = winners.ElementAt(0).GetPlayer();

            if (GunGame.IsMySqlEnabled)
                first.GunGamePlayer().data.first++;

            GunGame.Say("first", Color.cyan, first.DisplayName, first.GunGamePlayer().kills, first.GunGamePlayer().deaths);

            if (InGamePlayers.Count > 1)
            {
                UnturnedPlayer second = winners.ElementAt(1).GetPlayer();

                if (GunGame.IsMySqlEnabled)
                    second.GunGamePlayer().data.second++;

                GunGame.Say("second", Color.cyan, second.DisplayName, second.GunGamePlayer().kills, second.GunGamePlayer().deaths);
            }

            if (InGamePlayers.Count > 2)
            {
                UnturnedPlayer third = winners.ElementAt(2).GetPlayer();

                if (GunGame.IsMySqlEnabled)
                    third.GunGamePlayer().data.third++;

                GunGame.Say("third", Color.cyan, third.DisplayName, third.GunGamePlayer().kills, third.GunGamePlayer().deaths);
            }

            InGamePlayers.Clear();
        }

        public static void RequestBegin()
        {
            LightingManager.time = (uint)(LightingManager.cycle * LevelLighting.transition);

            isRunning = true;
            timer = GunGameConfig.instance.maxRoundTime * 60;

            foreach (ulong player in OnlinePlayers)
            {
                GunGameConfig.SpawnPosition sp = GetSpawnPositionRR();
                InGamePlayers.Add(player);
                player.GetPlayer().Teleport(sp.Vector3, sp.rot);
                player.GetPlayer().GunGamePlayer().EnterGame();
            }

            Rocket.Core.Logging.Logger.Log(String.Format("The game has started with {0} players!", InGamePlayers.Count), ConsoleColor.Yellow);

            if (InGamePlayers.Count < 3)
                Rocket.Core.Logging.Logger.Log("Starting game with less then 3 players can cause bugs!", ConsoleColor.Yellow);
        }

        public static bool IsPlayerInGame(ulong player)
        {
            return InGamePlayers.Contains(player);
        }

        public static GunGameConfig.SpawnPosition GetSpawnPositionRR()
        {
            GunGameConfig.SpawnPosition vect = GunGameConfig.instance.positions[lastSpawn];

            if (lastSpawn == GunGameConfig.instance.positions.Length - 1)
                lastSpawn = 0;
            else
                lastSpawn++;

            return vect;
        }
    }
}