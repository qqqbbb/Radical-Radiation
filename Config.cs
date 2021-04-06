using SMLHelper.V2.Json;
using SMLHelper.V2.Options.Attributes;

namespace Radical_Radiation
{
    [Menu("Radical Radiation")]
    public class Config : ConfigFile
    {
        //public bool logging = false;
        [Slider("Reactor rod rad radius", 1, 99, DefaultValue = 15, Step = 1, Format = "{0:F0}"), OnChange(nameof(UpdateRadiusDict1))]
        public int reactorRodRadius = 15;
        [Slider("Uraninite crystal rad radius", 1, 99, DefaultValue = 15, Step = 1, Format = "{0:F0}"), OnChange(nameof(UpdateRadiusDict1))]
        public int uraniniteCrystalRadius = 15;
        [Slider("Drillable uraninite rad radius", 1, 99, DefaultValue = 30, Step = 1, Format = "{0:F0}"), OnChange(nameof(UpdateRadiusDict1))]
        public int drillableUraniniteRadius = 30;
        [Slider("Nuclear reactor rad radius", 1, 99, DefaultValue = 30, Step = 1, Format = "{0:F0}"), OnChange(nameof(UpdateRadiusDict1))]
        public int nuclearReactorRadius = 30;
        [Toggle("Show the warning when you are protected from radiation")]
        public bool showRadWarning = false;

        public static void UpdateRadiusDict1()
        {
            RadiationPatches.UpdateRadiusDict();
           // ErrorMessage.AddDebug("Update Radius Dict 1");
           //Main.Log("Update Radius Dict 1");
           //RadiationPatches.radRange.Add(TechType.ReactorRod, config.reactorRodRadius);
           //RadiationPatches.radRange.Add(TechType.DepletedReactorRod, config.reactorRodRadius);
           //RadiationPatches.radRange.Add(TechType.UraniniteCrystal, config.uraniniteCrystalRadius);
           //RadiationPatches.radRange.Add(TechType.DrillableUranium, config.drillableUraniniteRadius);
           //RadiationPatches.radRange.Add(TechType.NuclearReactor, config.nuclearReactorRadius);
        }
    }
}