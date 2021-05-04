using SMLHelper.V2.Json;
using SMLHelper.V2.Options.Attributes;
using System.Collections.Generic;
using SMLHelper.V2.Crafting;

namespace Radical_Radiation
{
    [Menu("Radical Radiation")]
    public class Config : ConfigFile
    {
        //public bool logging = false;
        [Slider("Reactor rod rad radius", 0, 99, DefaultValue = 15, Step = 1, Format = "{0:F0}"), OnChange(nameof(UpdateRadiusDict))]
        public int reactorRodRadius = 15;
        [Slider("Uraninite crystal rad radius", 0, 99, DefaultValue = 15, Step = 1, Format = "{0:F0}"), OnChange(nameof(UpdateRadiusDict))]
        public int uraniniteCrystalRadius = 15;
        [Slider("Drillable uraninite rad radius", 0, 99, DefaultValue = 30, Step = 1, Format = "{0:F0}"), OnChange(nameof(UpdateRadiusDict))]
        public int drillableUraniniteRadius = 30;
        [Slider("Nuclear reactor rad radius", 0, 99, DefaultValue = 30, Step = 1, Format = "{0:F0}"), OnChange(nameof(UpdateRadiusDict))]
        public int nuclearReactorRadius = 30;
        [Slider("Reactor rod crafting time multiplier", 1, 1000, DefaultValue = 1, Step = 1, Format = "{0:F0}", Tooltip = "Default crafting time is 9 seconds.")]
        public int rodCraftTimeMult = 1;
        [Slider("Seamoth rad protection %", 0, 100, DefaultValue = 0, Step = 1, Format = "{0:F0}", Tooltip = "Default value is 0")]
        public int SeamothRadProtect = 0;
        [Slider("Prawn suit rad protection %", 0, 100, DefaultValue = 100, Step = 1, Format = "{0:F0}", Tooltip = "Default value is 100")]
        public int ExosuitRadProtect = 100;
        [Slider("Cyclops and base rad protection %", 0, 100, DefaultValue = 0, Step = 1, Format = "{0:F0}", Tooltip = "Default value is 0")]
        public int cyclopsRadProtect = 0;
        [Toggle("Persistent radiation warning", Tooltip = "Radiation warning will be on even when you are protected from radiation")]
        public bool showRadWarning = false;
        [Toggle("Persistent radiation sound", Tooltip = "Radiation sound will be on even when you are protected from radiation")]
        public bool radSound = false;

        public List<Ingredient> radLockerIngredients = new List<Ingredient>()
        {
            new Ingredient(TechType.Titanium, 2),
            new Ingredient(TechType.Lead, 2)
        };


        public static void UpdateRadiusDict()
        {
            RadPatches.UpdateRadiusDict();
           // AddDebug("Update Radius Dict 1");
           //Main.Log("Update Radius Dict 1");
           //RadiationPatches.radRange.Add(TechType.ReactorRod, config.reactorRodRadius);
           //RadiationPatches.radRange.Add(TechType.DepletedReactorRod, config.reactorRodRadius);
           //RadiationPatches.radRange.Add(TechType.UraniniteCrystal, config.uraniniteCrystalRadius);
           //RadiationPatches.radRange.Add(TechType.DrillableUranium, config.drillableUraniniteRadius);
           //RadiationPatches.radRange.Add(TechType.NuclearReactor, config.nuclearReactorRadius);
        }
    }
}