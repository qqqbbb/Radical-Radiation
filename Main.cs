
using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using BepInEx;
using Nautilus.Handlers;
using Nautilus.Assets;
using Nautilus.Utility;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Assets.Gadgets;
using BepInEx.Logging;
using UnityEngine;

namespace Radical_Radiation
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class Main : BaseUnityPlugin
    {
        private const string
            MODNAME = "Radical radiation",
            GUID = "qqqbbb.subnautica.radicalRadiation",
            VERSION = "3.0.0";

        public static Config config { get; } = OptionsPanelHandler.RegisterModOptions<Config>();
        public static ManualLogSource logger;


        [HarmonyPatch(typeof(IngameMenu), "QuitGameAsync")]
        internal class IngameMenu_QuitGameAsync_Patch
        {
            public static void Postfix(IngameMenu __instance, bool quitToDesktop)
            {
                if (!quitToDesktop)
                {
                    RadPatches.radLockerOpen = false;
                    RadPatches.closestRadObject = null;
                }
            }
        }

        private void Start()
        {
            config.Load();
            Assembly assembly = Assembly.GetExecutingAssembly();
            new Harmony($"qqqbbb_{assembly.GetName().Name}").PatchAll(assembly);
            logger = Logger;
            RadPatches.UpdateRadiusDict();
            RadModule.RegisterSeamothRadModule();
            RadModule.RegisterCyclopsRadModule();
            RadLocker.RegisterRadLocker();
            TechTypeExtensions.FromString("RadLocker", out RadPatches.radLocker, false);
            TechTypeExtensions.FromString("SeamothRadModule", out RadPatches.radModuleSeamoth, false);
            TechTypeExtensions.FromString("CyclopsRadModule", out RadPatches.radModuleCyclops, false);
                //Log("RadRadLocker " + RadPatches.radLocker.ToString());
            //Log("SeamothRadModule " + RadPatches.radModuleSeamoth.ToString());

        }



    }
}