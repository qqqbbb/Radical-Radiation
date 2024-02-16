using Nautilus.Json;
using Nautilus.Options.Attributes;
using System.Collections.Generic;
using UnityEngine;
using Nautilus.Commands;
using Nautilus.Handlers;
using Nautilus.Options;
using System.ComponentModel;

namespace Radical_Radiation
{
    [Menu("Radical Radiation")]
    public class Config : ConfigFile
    {
        [Slider("Reactor rod radiation radius", 0, 99, DefaultValue = 15, Step = 1, Format = "{0:F0}"), OnChange(nameof(UpdateRadiusDict))]
        public int reactorRodRadius = 15;
        [Slider("Uraninite crystal radiation radius", 0, 99, DefaultValue = 15, Step = 1, Format = "{0:F0}"), OnChange(nameof(UpdateRadiusDict))]
        public int uraniniteCrystalRadius = 15;
        [Slider("Drillable uraninite radiation radius", 0, 99, DefaultValue = 30, Step = 1, Format = "{0:F0}"), OnChange(nameof(UpdateRadiusDict))]
        public int drillableUraniniteRadius = 30;
        [Slider("Nuclear reactor radiation radius", 0, 99, DefaultValue = 30, Step = 1, Format = "{0:F0}"), OnChange(nameof(UpdateRadiusDict))]
        public int nuclearReactorRadius = 30;
        [Slider("Reactor rod crafting time multiplier", 1, 1000, DefaultValue = 1, Step = 1, Format = "{0:F0}", Tooltip = "Default crafting time is 9 seconds.")]
        public int rodCraftTimeMult = 1;
        [Slider("Seamoth radiation protection %", 0, 100, DefaultValue = 0, Step = 1, Format = "{0:F0}", Tooltip = "Default value is 0")]
        public int SeamothRadProtect = 0;
        [Slider("Prawn suit radiation protection %", 0, 100, DefaultValue = 100, Step = 1, Format = "{0:F0}", Tooltip = "Default value is 100")]
        public int ExosuitRadProtect = 100;
        [Slider("Cyclops radiation protection %", 0, 100, DefaultValue = 0, Step = 1, Format = "{0:F0}", Tooltip = "Default value is 0")]
        public int cyclopsRadProtect = 0;
        [Slider("Base radiation protection %", 0, 100, DefaultValue = 0, Step = 1, Format = "{0:F0}", Tooltip = "Default value is 0")]
        public int baseRadProtect = 0;
        [Toggle("Persistent radiation warning", Tooltip = "Radiation warning will be on even when you are protected from radiation")]
        public bool showRadWarning = false;
        [Toggle("Persistent radiation sound", Tooltip = "Radiation sound will be on even when you are protected from radiation")]
        public bool radSound = false;
        [Toggle("Radiation screen effects", Tooltip = "")]
        public bool screenFX = true;
        //[Slider("Lead Locker color: red", 0f, 1f, DefaultValue = .33f, Step = .01f, Format = "{0:R0}", Tooltip = "You have quit to desktop after changing this")]
        //public float radLockerRed = .33f;
        //[Slider("Lead Locker color: green", 0f, 1f, DefaultValue = .33f, Step = .01f, Format = "{0:R0}", Tooltip = "You have quit to desktop after changing this")]
        //public float radLockerGreen = .33f;
        //[Slider("Lead Locker color: blue", 0f, 1f, DefaultValue = .33f, Step = .01f, Format = "{0:R0}", Tooltip = "You have quit to desktop after changing this")]
        //public float radLockerBlue = .33f;
        public string lockerName = "Lead Locker";
        public string lockerDesc = "Locker insulated from radiation.";
        public string cyclopsRadModuleName = "Radiation protection module";
        public string cyclopsRadModuleDesc = "Protects cyclops from radiation.";
        public string seamothRadModuleName = "Radiation protection module";
        public string seamothRadModuleDesc = "Protects seamoth or prawn suit pilot from radiation.";

        public List<CraftData.Ingredient> radLockerIngredients = new List<CraftData.Ingredient>()
        {
            new CraftData.Ingredient(TechType.Titanium, 2),
            new CraftData.Ingredient(TechType.Lead, 2)
        };

        public List<CraftData.Ingredient> radModuleSeamoth = new List<CraftData.Ingredient>()
        {
            new CraftData.Ingredient(TechType.Titanium, 3),
            new CraftData.Ingredient(TechType.Lead, 3)
        };

        public List<CraftData.Ingredient> radModuleCyclops = new List<CraftData.Ingredient>()
        {
            new CraftData.Ingredient(TechType.Titanium, 4),
            new CraftData.Ingredient(TechType.Lead, 4)
        };

        public static void UpdateRadiusDict()
        {
            RadPatches.UpdateRadiusDict();
        }
    }
}