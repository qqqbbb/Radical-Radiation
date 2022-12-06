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

    public class CyclopsRadModule : Equipable
    {
        public CyclopsRadModule() : base(
            classId: "CyclopsRadModule",
            friendlyName: "Radiation protection module",
            description: "Protects cyclops from radiation.")
        {
        }

        public override EquipmentType EquipmentType => EquipmentType.CyclopsModule;

        //public override TechType RequiredForUnlock => TechType.RadiationSuit;

        public override TechGroup GroupForPDA => TechGroup.Cyclops;

        public override TechCategory CategoryForPDA => TechCategory.CyclopsUpgrades;

        public override CraftTree.Type FabricatorType => CraftTree.Type.CyclopsFabricator;

        public override QuickSlotType QuickSlotType => QuickSlotType.Passive;

        public override GameObject GetGameObject()
        {
            return  UnityEngine.Object.Instantiate<GameObject>(CraftData.GetPrefabForTechType(TechType.CyclopsHullModule1, true));
        }

        protected override TechData GetBlueprintRecipe()
        {
            TechData techData = new TechData();
            techData.Ingredients = Main.config.radModuleCyclops;
            techData.craftAmount = 1;
            return techData;
        }

        protected override Atlas.Sprite GetItemSprite()
        {
            //return SpriteManager.Get(TechType.CyclopsHullModule1);
            string executingLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string fileLocation = Path.Combine(executingLocation, "Radmodule.png");
            return ImageUtils.LoadSpriteFromFile(fileLocation);
        }
    }
}