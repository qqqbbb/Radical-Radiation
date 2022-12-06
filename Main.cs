
using HarmonyLib;
using QModManager.API.ModLoading;
using System;
using System.Reflection;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;
using System.Collections.Generic;

namespace Radical_Radiation
{
    [QModCore]
    public class Main
    {
        public static Config config { get; } = OptionsPanelHandler.RegisterModOptions<Config>();

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
                {
                    RadPatches.radLockerOpen = false;
                    RadPatches.subLocker = null;
                    RadPatches.seamothStorage = null;
                    RadPatches.radPlayerInRange = new Dictionary<RadiatePlayerInRange, float>();
                }
   
            }
        }

        [QModPatch]
        public static void Load()
        {
            config.Load();
            Assembly assembly = Assembly.GetExecutingAssembly();
            new Harmony($"qqqbbb_{assembly.GetName().Name}").PatchAll(assembly);
            //modLoaded = true;
            RadPatches.UpdateRadiusDict();
            new RadLocker().Patch();
            new SeamothRadModule().Patch();
            new CyclopsRadModule().Patch();
            TechTypeExtensions.FromString("RadRadLocker", out RadPatches.radLocker, false);
            TechTypeExtensions.FromString("SeamothRadModule", out RadPatches.radModuleSeamoth, false);
            TechTypeExtensions.FromString("CyclopsRadModule", out RadPatches.radModuleCyclops, false);
                //Log("RadRadLocker " + RadPatches.radLocker.ToString());
            //Log("SeamothRadModule " + RadPatches.radModuleSeamoth.ToString());

        }



    }
}