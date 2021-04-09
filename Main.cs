
using HarmonyLib;
using QModManager.API.ModLoading;
using System;
using System.Reflection;
using SMLHelper.V2.Handlers;
using System.Collections.Generic;

namespace Radical_Radiation
{
    [QModCore]
    public class Main
    {
        internal static Config config { get; } = OptionsPanelHandler.RegisterModOptions<Config>();
        //public static bool modLoaded = false;
        public static void Log(string str, QModManager.Utility.Logger.Level lvl = QModManager.Utility.Logger.Level.Info)
        {
            QModManager.Utility.Logger.Log(lvl, str);
        }

        [HarmonyPatch(typeof(IngameMenu), "QuitGameAsync")]
        internal class IngameMenu_QuitGameAsync_Patch
        {
            public static void Postfix(IngameMenu __instance, bool quitToDesktop)
            {
                if (!quitToDesktop)
                    RadiationPatches.radPlayerInRange = new Dictionary<RadiatePlayerInRange, float>();
            }
        }

        [QModPatch]
        public static void Load()
        {
            config.Load();
            Assembly assembly = Assembly.GetExecutingAssembly();
            new Harmony($"qqqbbb_{assembly.GetName().Name}").PatchAll(assembly);
            //modLoaded = true;
            RadiationPatches.UpdateRadiusDict();
        }

        //[QModPostPatch] // runs before SMLhelper adds new techtypes
        public static void ModCompat()
        {
            foreach (TechType tt in (TechType[])Enum.GetValues(typeof(TechType)))
            {
                Log("TechType " + tt);
                //if (tt.ToString() == "MIUraninite") 
            }
        }


    }
}