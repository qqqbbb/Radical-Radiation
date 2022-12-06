using System.Collections;
using UnityEngine;
using SMLHelper.V2.Crafting;
using System.Collections.Generic;

namespace Radical_Radiation
{
    using SMLHelper.V2.Assets;
    using SMLHelper.V2.Crafting;
    using SMLHelper.V2.Handlers;
    using SMLHelper.V2.Utility;
    using System.Collections.Generic;
    using UnityEngine;
    using System.IO;
    using System.Reflection;

    public class SeamothRadModule : Equipable
    {
        public SeamothRadModule() : base(
            classId: "SeamothRadModule",
            friendlyName: "Radiation protection module",
            description: "Protects seamoth or prawn suit driver from radiation.")
        {
            //OnStartedPatching += () => CraftTreeHandler.AddTabNode(CraftTree.Type.Workbench, "SeamothRadlModule", "Seamoth radiation protection module", GetItemSprite());
        }

        public override EquipmentType EquipmentType => EquipmentType.VehicleModule;

        //public override TechType RequiredForUnlock => TechType.RadiationSuit;

        public override TechGroup GroupForPDA => TechGroup.VehicleUpgrades;

        public override TechCategory CategoryForPDA => TechCategory.VehicleUpgrades;

        public override CraftTree.Type FabricatorType => CraftTree.Type.SeamothUpgrades;

        //public override string[] StepsToFabricatorTab => new[] { "SeamothModules" };
        public override string[] StepsToFabricatorTab => new[] { "CommonModules" };

        public override QuickSlotType QuickSlotType => QuickSlotType.Passive;

        public override GameObject GetGameObject()
        {
            // Get the ElectricalDefense module prefab and instantiate it
            var path = "WorldEntities/Tools/SeamothElectricalDefense";
            var prefab = Resources.Load<GameObject>(path);
            var obj = Object.Instantiate(prefab);

            // Get the TechTags and PrefabIdentifiers
            var techTag = obj.GetComponent<TechTag>();
            var prefabIdentifier = obj.GetComponent<PrefabIdentifier>();

            // Change them so they fit to our requirements.
            techTag.type = TechType;
            prefabIdentifier.ClassId = ClassID;

            return obj;
        }

        protected override TechData GetBlueprintRecipe()
        {
            TechData techData = new TechData();
            techData.Ingredients = Main.config.radModuleSeamoth;
            techData.craftAmount = 1;
            return techData;
        }

        protected override Atlas.Sprite GetItemSprite()
        {
            //return SpriteManager.Get(TechType.VehicleHullModule3);
            string executingLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string fileLocation = Path.Combine(executingLocation, "Radmodule.png");
            return ImageUtils.LoadSpriteFromFile(fileLocation);
        }
    }
}