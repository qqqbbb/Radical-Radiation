using HarmonyLib;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using Nautilus.Utility;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;


namespace Radical_Radiation
{
    public static class RadLocker 
    {
        public static void RegisterRadLocker()
        {
            PrefabInfo prefabInfo = PrefabInfo.WithTechType("RadLocker", Main.config.lockerName, Main.config.lockerDesc);
            //prefabInfo.WithIcon(SpriteManager.Get(TechType.SmallLocker));
            prefabInfo.WithIcon(GetSprite());
            CustomPrefab customPrefab = new CustomPrefab(prefabInfo);
            CloneTemplate cloneTemplate = new CloneTemplate(customPrefab.Info, TechType.SmallLocker);
            customPrefab.SetGameObject( cloneTemplate);
            customPrefab.SetUnlock(TechType.UraniniteCrystal);
            cloneTemplate.ModifyPrefab += ModifyPrefab;
            GadgetExtensions.SetPdaGroupCategory(customPrefab, TechGroup.InteriorModules, TechCategory.InteriorModule);
            RecipeData recipeData = new RecipeData();
            recipeData.craftAmount = 1;
            recipeData.Ingredients = Main.config.radLockerIngredients;
            GadgetExtensions.SetRecipe(customPrefab, recipeData);
            customPrefab.Register();
            //Main.logger.LogMessage("Registered rad locker"); 
        }

        private static void ModifyPrefab(GameObject obj)
        {
            Vector3 scale = obj.transform.localScale;
            obj.transform.localScale = new Vector3(scale.x * 1.2f, scale.y * 1.2f, scale.z * 1.2f);
            MeshRenderer[] mrs = obj.GetAllComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer mr in mrs)
            {
                mr.material.color = new Color(.33f, .33f, .33f, 1f);
            }
        }

        public static Atlas.Sprite GetSprite()
        {
            string executingLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string fileLocation = Path.Combine(executingLocation, "LeadLocker.png");
            return ImageUtils.LoadSpriteFromFile(fileLocation);
        }

    }
}
