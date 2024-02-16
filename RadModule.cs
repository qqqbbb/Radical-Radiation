using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets;
using Nautilus.Handlers;
using BepInEx;
using Nautilus.Utility;

namespace Radical_Radiation
{


    public static class RadModule
    {
        static Atlas.Sprite radSprite = null;

        public static void RegisterCyclopsRadModule()
        {
            PrefabInfo prefabInfo = PrefabInfo.WithTechType("CyclopsRadModule", Main.config.cyclopsRadModuleName, Main.config.cyclopsRadModuleDesc);
            prefabInfo.WithIcon(GetSprite());
            CustomPrefab customPrefab = new CustomPrefab(prefabInfo);
            PrefabTemplate prefabTemplate = new CloneTemplate(prefabInfo, TechType.HullReinforcementModule);
            customPrefab.SetGameObject(prefabTemplate);
            customPrefab.SetEquipment(EquipmentType.CyclopsModule).WithQuickSlotType(QuickSlotType.Passive);
            customPrefab.SetUnlock(TechType.RadiationSuit);
            customPrefab.SetRecipe(new Nautilus.Crafting.RecipeData()
            {
                craftAmount = 1,
                Ingredients = Main.config.radModuleCyclops,
            })
                .WithFabricatorType(CraftTree.Type.CyclopsFabricator);
            //.WithCraftingTime(2.5f);
            customPrefab.Register();
            //Main.logger.LogMessage("Registered seamoth rad module");
        }

        public static void RegisterSeamothRadModule()
        {
            PrefabInfo prefabInfo = PrefabInfo.WithTechType("SeamothRadModule", Main.config.seamothRadModuleName, Main.config.seamothRadModuleDesc);
            prefabInfo.WithIcon(GetSprite());
            CustomPrefab customPrefab = new CustomPrefab(prefabInfo);
            PrefabTemplate prefabTemplate = new CloneTemplate(prefabInfo, TechType.HullReinforcementModule);
            customPrefab.SetGameObject(prefabTemplate);
            customPrefab.SetVehicleUpgradeModule(EquipmentType.VehicleModule, QuickSlotType.Passive);
            //customPrefab.SetEquipment(EquipmentType.ExosuitModule).WithQuickSlotType(QuickSlotType.Passive);
            customPrefab.SetUnlock(TechType.RadiationSuit);
            customPrefab.SetRecipe(new Nautilus.Crafting.RecipeData()
            {
                craftAmount = 1,
                Ingredients = Main.config.radModuleSeamoth,
            })
                .WithFabricatorType(CraftTree.Type.SeamothUpgrades)
                .WithStepsToFabricatorTab("SeamothModules");
            //.WithCraftingTime(2.5f);
            customPrefab.Register();
            //Main.logger.LogMessage("Registered seamoth rad module");
        }

        public static Atlas.Sprite GetSprite()
        {
            if (radSprite == null)
            {
                //return SpriteManager.Get(TechType.VehicleHullModule3);
                string executingLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string fileLocation = Path.Combine(executingLocation, "Radmodule.png");
                radSprite = ImageUtils.LoadSpriteFromFile(fileLocation);
                //AssetsHelper.Assets.LoadAsset<UnityEngine.Sprite>("CyclopsDockingHatchIconG"));
            }
            return radSprite;
        }
    }

}