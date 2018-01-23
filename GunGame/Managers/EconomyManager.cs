using System;
using System.Collections.Generic;
using System.Reflection;

using GunGame.API;
using GunGame.Economy;

using Rocket.API;
using RocketLogger = Rocket.Core.Logging.Logger;

using UnityEngine;

namespace GunGame.Managers
{
    public static class EconomyManager
    {
        public static Assembly EconomyAssembly;
        public static IEconomyHook ActiveHook;
        public static bool Enabled = false;

        static List<IEconomyHook> hooks = new List<IEconomyHook>();

        public static void Initialize()
        {
            hooks.Add(new AviEconomyHook());
            hooks.Add(new UconomyHook());

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (IEconomyHook hook in hooks) {
                    if (assembly.GetName().Name == hook.DeterminingAssembly) {

                        EconomyAssembly = assembly;
                        ActiveHook = hook;

                        Enabled = true;

                        RocketLogger.Log($"Initialized with economy support for {hook.DeterminingAssembly} enabled.", ConsoleColor.Yellow);

                        return;
                    }
                }
            }
            
            RocketLogger.Log("Initialized with economy support disabled.", ConsoleColor.Yellow);
        }

        public static void IncreaseBalance(IRocketPlayer p, byte place)
        {
            if (Enabled) {
                GunGameConfig.Reward reward = new GunGameConfig.Reward();
                GunGameConfig.EconomySettings settings = GunGameConfig.instance.econSettings;

                for (int i = 0; i < settings.rewards.Length; i++) {
                    if (settings.rewards[i].place == place) {
                        reward = settings.rewards[place];
                        break;
                    }
                }

                if (reward.place != 0) {
                    ActiveHook.IncreaseBalance(p, reward.reward);
                    GunGame.Say(p, "reward", Color.green, reward.reward.ToString() + reward.ordinal, place.ToString());
                }
            }
        }
    }
}
