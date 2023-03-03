
using HarmonyLib;
using System;
using System.Reflection;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;
using System.Collections.Generic;
using BepInEx;

namespace Radical_Radiation
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class Main : BaseUnityPlugin
    {
        private const string
            MODNAME = "Radical radiation",
            GUID = "qqqbbb.subnautica.radicalRadiation",
            VERSION = "2.0";

        public static Config config { get; } = OptionsPanelHandler.RegisterModOptions<Config>();


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

        private void Start()
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