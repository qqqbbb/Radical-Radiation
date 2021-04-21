using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;

namespace Radical_Radiation
{
    class RadLocker : Buildable
    {
        public RadLocker() : base("RadRadLocker", "Lead Locker", "Locker insulated from radiation.") { }

        public override TechGroup GroupForPDA => TechGroup.InteriorModules;

        public override TechCategory CategoryForPDA => TechCategory.InteriorModule;

        //public override TechType RequiredForUnlock => TechType.BaseNuclearReactor;

        protected override Atlas.Sprite GetItemSprite() => SpriteManager.Get(TechType.SmallLocker);

        public override GameObject GetGameObject()
        {
            GameObject parent = UnityEngine.Object.Instantiate<GameObject>(CraftData.GetPrefabForTechType(TechType.SmallLocker, true));
            Vector3 scale = parent.transform.localScale;
            parent.transform.localScale = new Vector3(scale.x * 1.2f, scale.y * 1.2f, scale.z * 1.2f);
            //var fbColor = parent.GetAllComponentsInChildren<SkinnedMeshRenderer>();
            //foreach (var fabricatorColor in fbColor)
            //{
            //    fabricatorColor.material.color
            //}
            return parent;
        }

        protected override TechData GetBlueprintRecipe()
        {
            TechData recipe = new TechData()
            {
                craftAmount = 1,
                Ingredients = Main.config.radLockerIngredients
            };
            return recipe;
        }

    }
}
