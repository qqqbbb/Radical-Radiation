using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using UWE;

namespace Radical_Radiation
{
    class RadLocker : Buildable
    {
        public RadLocker() : base("RadRadLocker", Main.config.lockerName, Main.config.lockerDesc) { }

        public override TechGroup GroupForPDA => TechGroup.InteriorModules;

        public override TechCategory CategoryForPDA => TechCategory.InteriorModule;

        //public override TechType RequiredForUnlock => TechType.BaseNuclearReactor;

        protected override Atlas.Sprite GetItemSprite() => SpriteManager.Get(TechType.SmallLocker);

        public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
        {
            CoroutineTask<GameObject> task = CraftData.GetPrefabForTechTypeAsync(TechType.SmallLocker);
            yield return task;
            GameObject originalPrefab = task.GetResult();
            GameObject resultPrefab = UnityEngine.Object.Instantiate(originalPrefab);
            Vector3 scale = resultPrefab.transform.localScale;
            resultPrefab.transform.localScale = new Vector3(scale.x * 1.2f, scale.y * 1.2f, scale.z * 1.2f);
            MeshRenderer[] smrs = resultPrefab.GetAllComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer smr in smrs)
            {
                smr.material.color = new Color(Main.config.radLockerRed, Main.config.radLockerRed, Main.config.radLockerRed, 1f);
            }
            //uGUI_SignInput si = parent.GetComponent<uGUI_SignInput>();
            //if (si) // null
            //    si.stringDefaultLabel = Main.config.lockerName;
            //else
            //    Main.Log("rad locker no uGUI_SignInput");
            gameObject.Set(resultPrefab);
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
