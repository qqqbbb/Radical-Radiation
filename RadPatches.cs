using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Radical_Radiation
{ // make player drop rad items on death?
    class RadPatches
    {
        static float radiateInterval = .2f;
        static float damageInterval = 2f;
        //static Animation radWarnAnim;
        static float radDamageTime = 0f;
        public static Dictionary<RadiatePlayerInRange, float> radPlayerInRange = new Dictionary<RadiatePlayerInRange, float>();
        //public static Dictionary<TechType, float> radRange = new Dictionary<TechType, float> { { TechType.ReactorRod, Main.config.reactorRodRadius }, { TechType.DepletedReactorRod, Main.config.reactorRodRadius }, { TechType.UraniniteCrystal, Main.config.uraniniteCrystalRadius }, { TechType.DrillableUranium, Main.config.drillableUraniniteRadius }, { TechType.NuclearReactor, Main.config.nuclearReactorRadius } };
        public static Dictionary<TechType, float> radRange = new Dictionary<TechType, float>(); 
        public static HashSet<TechType> radStuff = new HashSet<TechType> { { TechType.ReactorRod }, { TechType.DepletedReactorRod }, { TechType.UraniniteCrystal } };
        public static TechType radLocker = TechType.None;
        public static TechType radMachine = TechType.None;
        public static bool radLockerOpen = false;
        static PDA pda;
        //static bool buildingMachine = false;

        public static void ModCompat()
        {
            TechType uranIngot = TechType.None;
            TechTypeExtensions.FromString("MIUraninite", out uranIngot, false);
               // TryParse does not work if techtype added by SMLhelper
               //uraninite = (TechType)Enum.Parse(typeof(TechType), "MIUraninite");
            if (uranIngot != TechType.None)
            {
                radStuff.Add(uranIngot);
                radRange[uranIngot] = radRange[TechType.UraniniteCrystal];
                //Main.Log("uraninite " + uranIngot);
            }
        }

        public static void UpdateRadiusDict()
        {
            if (Main.config == null)
                return;
            //TechType tt = TechType.uran
            //ErrorMessage.AddDebug("Update Radius Dict");
            radRange[TechType.ReactorRod] = Main.config.reactorRodRadius;
            radRange[TechType.DepletedReactorRod] = Main.config.reactorRodRadius;
            radRange[TechType.UraniniteCrystal] = Main.config.uraniniteCrystalRadius;
            radRange[TechType.DrillableUranium] = Main.config.drillableUraniniteRadius;
            radRange[TechType.NuclearReactor] = Main.config.nuclearReactorRadius;
            radRange[TechType.BaseNuclearReactor] = Main.config.nuclearReactorRadius;
        }

        public static void RebuildRadDict()
        {
            //ErrorMessage.AddDebug("Rebuild Rad Dict");
            Dictionary<RadiatePlayerInRange, float> newDict = new Dictionary<RadiatePlayerInRange, float>();
            foreach (var keyValuePair in radPlayerInRange)
            {
                if (keyValuePair.Key != null)
                    newDict.Add(keyValuePair.Key, keyValuePair.Value);
            }
            radPlayerInRange = new Dictionary<RadiatePlayerInRange, float>(newDict);
        }

        public static bool IsClosestToPlayer(RadiatePlayerInRange instance)
        {
            if (!radPlayerInRange.ContainsKey(instance))
                return false;

            bool rebuildDict = false;
            float closest = float.PositiveInfinity;
            RadiatePlayerInRange closestInstance = null;

            foreach (var keyValuePair in radPlayerInRange)
            {
                if (keyValuePair.Key == null)
                    rebuildDict = true;
                else
                {
                    if (keyValuePair.Value < closest)
                    {
                        closestInstance = keyValuePair.Key;
                        closest = keyValuePair.Value;
                    }
                }
            }
            if (rebuildDict)
                RebuildRadDict();

            if (closestInstance == instance)
                return true;

            return false;
        }

        public static bool InventoryHasRad()
        {
            if (!Inventory.main || Inventory.main._container == null)
                return false;

            if (radLockerOpen)
                return true;

            foreach (TechType tt in radStuff)
            {
                if (Inventory.main._container.Contains(tt))
                    return true;
            }
            return false;
        }

        public static void MakeRadioactive(GameObject go, bool active, float radius = 0f)
        {
            //ErrorMessage.AddDebug(go.name + " MakeRadioActive " + active);
            //Main.Log(go.name + " MakeRadioActive " + active);
            //if (active && radius == 0f)
            //    return;

            PlayerDistanceTracker playerDistanceTracker = go.EnsureComponent<PlayerDistanceTracker>();
            playerDistanceTracker.enabled = active;
            playerDistanceTracker.maxDistance = radius;
            RadiatePlayerInRange radiatePlayerInRange = go.EnsureComponent<RadiatePlayerInRange>();

            if (!active && radiatePlayerInRange)
            {
                bool removed = radPlayerInRange.Remove(radiatePlayerInRange);
                //Main.Log(go.name + " MakeRadioActive remove  radiatePlayerInRange " + removed);
            }
            radiatePlayerInRange.enabled = active;
            radiatePlayerInRange.radiateRadius = radius;
            DamagePlayerInRadius damagePlayerInRadius = go.EnsureComponent<DamagePlayerInRadius>();
            damagePlayerInRadius.enabled = active;
            damagePlayerInRadius.damageType = DamageType.Radiation;
            damagePlayerInRadius.damageAmount = 1f;
            damagePlayerInRadius.updateInterval = 2f;
            damagePlayerInRadius.damageRadius = radius;

            damagePlayerInRadius.tracker = playerDistanceTracker;
            radiatePlayerInRange.tracker = playerDistanceTracker;
        }

        public static void RadiatePlayerInv()
        {
            if (!GameModeUtils.HasRadiation() || (NoDamageConsoleCommand.main && NoDamageConsoleCommand.main.GetNoDamageCheat()))
            {
                Player.main.SetRadiationAmount(0f);
                return;
            }
            if (InventoryHasRad())
            {
                //ErrorMessage.AddDebug("Player has ReactorRod");
                float amount = 1f;

                if (Inventory.main.equipment.GetCount(TechType.RadiationSuit) > 0)
                    amount -= 0.5f;
                if (Inventory.main.equipment.GetCount(TechType.RadiationHelmet) > 0)
                    amount -= 0.35f;
                if (Inventory.main.equipment.GetCount(TechType.RadiationGloves) > 0)
                    amount -= 0.15f;

                Player.main.SetRadiationAmount(amount);
                float damage = amount * 10f;
                //ErrorMessage.AddDebug("Radiate Player Inv " + amount);
                //damage = Mathf.Max(damage, 1F);
                if (damage > 0f && Time.time - radDamageTime > damageInterval)
                {
                    radDamageTime = Time.time;
                    //ErrorMessage.AddDebug(" damage Player inv " + damage);
                    Player.main.liveMixin.TakeDamage(damage, Player.main.transform.position, DamageType.Radiation);
                }
            }
            else if (radPlayerInRange.Count == 0)
            {
                //ErrorMessage.AddDebug("RadiatePlayerInv SetRadiationAmount 0 ");
                Player.main.SetRadiationAmount(0f);
            }
            else
            { // should need this only if player removes nuclear reactor
                bool RebuildDict = false;
                foreach (var keyValuePair in radPlayerInRange)
                {
                    if (keyValuePair.Key == null)
                        RebuildDict = true;
                }
                if (RebuildDict)
                    RebuildRadDict();
            }
        }

        private static bool ReactorHasRad(BaseNuclearReactor reactor)
        {
            int length = BaseNuclearReactor.slotIDs.Length;
            for (int index = 0; index < length; ++index)
            {
                string slotId = BaseNuclearReactor.slotIDs[index];
                InventoryItem itemInSlot = reactor.equipment.GetItemInSlot(slotId);
                if (itemInSlot != null)
                {
                    Pickupable pickupable = itemInSlot.item;
                    if (pickupable != null && radStuff.Contains(pickupable.GetTechType()))
                    {
                        //ErrorMessage.AddDebug("Nuclear Reactor has rad");
                        return true;
                    }
                }
            }
            return false;
        }

        [HarmonyPatch(typeof(Player), "Start")]
        class Player_Start_patch
        {
            public static void Postfix(Player __instance)
            {
                ModCompat();
                pda = Player.main.GetPDA();
                //foreach (TechType tt in radStuff)
                //    Main.Log("radStuff  " + tt);
            }
        }

        [HarmonyPatch(typeof(Drillable), "Start")]
        class Drillable_Start_patch
        {
            public static void Postfix(Drillable __instance)
            {
                //TechType techType = __instance.GetDominantResourceType();
                if (__instance.GetDominantResourceType() == TechType.UraniniteCrystal)
                {
                    MakeRadioactive(__instance.gameObject, true, radRange[TechType.DrillableUranium]);
                }
            }
        }

        [HarmonyPatch(typeof(Drillable), "OnDestroy")]
        class Drillable_OnDestroy_patch
        {
            public static void Postfix(Drillable __instance)
            {
                if (__instance.GetDominantResourceType() == TechType.UraniniteCrystal)
                {
                    MakeRadioactive(__instance.gameObject, false);
                }
            }
        }

        [HarmonyPatch(typeof(DamagePlayerInRadius), "DoDamage")]
        class DamagePlayerInRadius_DoDamage_patch
        {
            public static bool Prefix(DamagePlayerInRadius __instance)
            {
                if (!__instance.enabled || !__instance.gameObject.activeInHierarchy || __instance.damageRadius <= 0f)
                    return false;

                if (__instance.damageType != DamageType.Radiation)
                    return true;

                if (Player.main.radiationAmount == 0f || InventoryHasRad())
                {
                    //ErrorMessage.AddDebug("Player.radiationAmount = 0");
                    return false;
                }

                if (__instance.tracker.distanceToPlayer < __instance.damageRadius)
                {
                    float damage = __instance.damageAmount * Player.main.radiationAmount * 10f;
                    //damage = Mathf.Max(damage, 1F);
                    //ErrorMessage.AddDebug(" damage Player " + damage);
                    Player.main.liveMixin.TakeDamage(damage, __instance.transform.position, __instance.damageType);
                }
                //else
                //ErrorMessage.AddDebug("Player too far");
                return false;
            }
        }

        [HarmonyPatch(typeof(RadiatePlayerInRange), "Radiate")]
        class RadiatePlayerInRange_Radiate_patch
        {
            public static bool Prefix(RadiatePlayerInRange __instance)
            {
                if (!GameModeUtils.HasRadiation() || (NoDamageConsoleCommand.main && NoDamageConsoleCommand.main.GetNoDamageCheat()) || Player.main.inExosuit)
                {
                    Player.main.SetRadiationAmount(0f);
                    return false;
                }
                if (!__instance.enabled || InventoryHasRad())
                    return false;

                //ErrorMessage.AddDebug("radPlayerInRange Count " + radPlayerInRange.Count);

                float distanceToPlayer = __instance.tracker.distanceToPlayer;

                if (__instance.radiateRadius > 0f && distanceToPlayer < __instance.radiateRadius)
                {
                    float amount = Mathf.Clamp01((1f - distanceToPlayer / __instance.radiateRadius));
                    float mult = 1f;

                    if (Inventory.main.equipment.GetCount(TechType.RadiationSuit) > 0)
                        mult -= 0.5f;
                    if (Inventory.main.equipment.GetCount(TechType.RadiationHelmet) > 0)
                        mult -= 0.35f;
                    if (Inventory.main.equipment.GetCount(TechType.RadiationGloves) > 0)
                        mult -= 0.15f;

                    //if (Player.main.IsInBase())
                    //    mult -= 0.20f;
                    //else if (Player.main.IsInSubmarine())
                    //    mult -= 0.20f;
                    //if (Player.main.inExosuit)
                    //    mult = 0f;
                    //if (Player.main.inSeamoth)
                    //    mult -= 0.10f;

                    amount *= mult;
                    //ErrorMessage.AddDebug("tracker.maxDistance " + (int)__instance.tracker.maxDistance);
                    //amount = Mathf.Clamp01(amount);
                    //radPlayerInRange.Remove(__instance);
                    if (radPlayerInRange.ContainsKey(__instance))
                        radPlayerInRange[__instance] = distanceToPlayer;
                    else
                        radPlayerInRange.Add(__instance, distanceToPlayer);

                    //ErrorMessage.AddDebug("distanceToPlayer " + (int)distanceToPlayer);
                    //ErrorMessage.AddDebug("Radiate  " + __instance.gameObject.name + " " + amount);
                    //ErrorMessage.AddDebug("radiateRadius  " + (int)__instance.radiateRadius);
                    if (IsClosestToPlayer(__instance))
                    {
                        //ErrorMessage.AddDebug("Radiate  " + __instance.gameObject.name + " " + amount + " " + (int)distanceToPlayer);
                        Player.main.SetRadiationAmount(amount);
                    }
                }
                else // out of range
                {
                    //ErrorMessage.AddDebug("distanceToPlayer " + (int)distanceToPlayer);
                    radPlayerInRange.Remove(__instance);
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(Player), "Update")]
        class Player_Update_patch
        {

            static float radTime = 0f;
            public static void Postfix(Player __instance)
            {
                //if (Input.GetKey(KeyCode.C))
                //{
                //    GameObject target = Player.main.GetComponent<GUIHand>().activeTarget;
                //    if (target)
                //    {
                //        ErrorMessage.AddDebug("target " + CraftData.GetTechType(target));
                //    }
                //}
                //Player.main.oxygenMgr.AddOxygen(115f); 
                //ErrorMessage.AddDebug("radRange.Count " + radRange.Count);
                //ErrorMessage.AddDebug("reactor Rod Radius " + );
                if (Time.time - radTime > radiateInterval)
                {
                    radTime = Time.time;
                    RadiatePlayerInv();
                }
            }
        }

        //[HarmonyPatch(typeof(uGUI_RadiationWarning), "Initialize")]
        class uGUI_RadiationWarning_Initialize_patch
        {
            public static void Postfix(uGUI_RadiationWarning __instance)
            {
                if (__instance.transform.localScale.x == .8f)
                    return;

                __instance.transform.localScale = new Vector3(.8f, .8f, 1f);
                //ErrorMessage.AddDebug("uGUI_RadiationWarning Initialize");
                GameObject background = __instance.gameObject.FindChild("Background");
                //radWarnAnim = background?.GetComponent<Animation>();
                //radWarnAnim?.Stop();
                return;
            }
        }

        [HarmonyPatch(typeof(uGUI_RadiationWarning), "Update")]
        class uGUI_RadiationWarning_Update_patch
        {
            public static void Postfix(uGUI_RadiationWarning __instance)
            {
                if (!Player.main)
                    return;

                bool show = radPlayerInRange.Count > 0 || InventoryHasRad();
                if (Player.main.radiationAmount == 0f && !Main.config.showRadWarning)
                    show = false;

                __instance.warning.SetActive(show);
            }
        }

        [HarmonyPatch(typeof(Player), "UpdateRadiationSound")]
        class Player_UpdateRadiationSound_patch
        {
            public static bool Prefix(Player __instance)
            {
                float radiationAmount = __instance.radiationAmount;
                //ErrorMessage.AddDebug("UpdateRadiationSound " + radiationAmount);
                if (__instance.fmodIndexIntensity < 0)
                    __instance.fmodIndexIntensity = __instance.radiateSound.GetParameterIndex("intensity");
                if (radiationAmount == 0f && Main.config.radSound)
                { 
                    if (InventoryHasRad() || radPlayerInRange.Count > 0)
                        radiationAmount = .1f;
                }
                if (radiationAmount > 0f)
                {

                    __instance.radiateSound.SetParameterValue(__instance.fmodIndexIntensity, radiationAmount);
                    __instance.radiateSound.Play();
                }
                else
                    __instance.radiateSound.Stop();

                return false;
            }
        }

        [HarmonyPatch(typeof(Pickupable), "Awake")]
        class Pickupable_Awake_patch
        {
            public static void Postfix(Pickupable __instance)
            {
                TechType techType = __instance.GetTechType();

                //ErrorMessage.AddDebug(__instance.gameObject.name + " Pickupable Awake " + __instance.GetTechType());
                if (radStuff.Contains(techType))
                {
                    if (__instance.isPickupable) // not in container
                    {
                        //ErrorMessage.AddDebug(__instance.gameObject.name + " Pickupable Awake " + __instance.GetTechType());
                        //Main.Log(__instance.gameObject.name + " Pickupable Awake " + __instance.GetTechName());
                        MakeRadioactive(__instance.gameObject, true, radRange[techType]);
                    }
                }
            }
        }

        //[HarmonyPatch(typeof(Pickupable), "Initialize")]
        class Pickupable_Initialize_patch
        {
            public static void Postfix(Pickupable __instance)
            {
                TechType techType = __instance.GetTechType();

                //ErrorMessage.AddDebug(__instance.gameObject.name + " Pickupable Initialize " + techType);
                if (radStuff.Contains(techType) && __instance.isPickupable)
                {
                    //ErrorMessage.AddDebug(__instance.gameObject.name + " Pickupable Awake " + techType);
                    //Main.Log(__instance.gameObject.name + " Pickupable Initialize " + __instance.GetTechName());
                    MakeRadioactive(__instance.gameObject, true, radRange[techType]);
                }
            }
        }

        [HarmonyPatch(typeof(ItemsContainer), "NotifyAddItem")]
        class ItemsContainer_AddItem_patch
        {
            public static void Postfix(ItemsContainer __instance, InventoryItem item)
            {
                TechType techType = item.item.GetTechType();
                //Main.Log("NotifyAddItem " + techType);
                //ErrorMessage.AddDebug("NotifyAddItem " + techType);
                if (radStuff.Contains(techType))
                {
                    GameObject go = __instance.tr.gameObject;
                    if (item.item.GetComponent<RadiatePlayerInRange>())
                        MakeRadioactive(item.item.gameObject, false);

                    if (CraftData.GetTechType(go) == radLocker)
                    {
                        if (pda.isInUse)
                            radLockerOpen = true;
                    }
                    else if (__instance.tr.parent.gameObject.GetComponent("CyNukeReactorMono"))
                    {
                        //ErrorMessage.AddDebug("Cyclops Nuke Reactor !!!");
                        MakeRadioactive(go, true, radRange[TechType.BaseNuclearReactor]);
                    }
                    else if (Inventory.main._container != __instance)
                        MakeRadioactive(go, true, radRange[techType]);
                }
            }
        }

        [HarmonyPatch(typeof(ItemsContainer), "NotifyRemoveItem")]
        class ItemsContainer_NotifyRemoveItem_patch
        {
            public static void Postfix(ItemsContainer __instance, InventoryItem item)
            {
                TechType techType = item.item.GetTechType();
                if (radStuff.Contains(techType))
                {
                    GameObject go = __instance.tr.gameObject;
                    if (Inventory.main._container != __instance && CraftData.GetTechType(go) != radLocker)
                    {
                        int count = 0;
                        foreach (TechType tt in radStuff)
                            count += __instance.GetCount(tt);

                        if (count == 0)
                            MakeRadioactive(go, false);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Pickupable), "Drop", new Type[] { typeof(Vector3), typeof(Vector3), typeof(bool) })]
        class Pickupable_Drop_Patch
        {
            public static void Postfix(Pickupable __instance)
            {
                TechType techType = __instance.GetTechType();
                if (radStuff.Contains(techType))
                {
                    //Main.Log(" Drop "+ __instance.gameObject.name);
                    MakeRadioactive(__instance.gameObject, true, radRange[techType]);
                    //ErrorMessage.AddDebug("Drop " + __instance.gameObject.name);
                }
            }
        }

        [HarmonyPatch(typeof(BaseNuclearReactor), "Start")]
        class BaseNuclearReactor_Start_patch
        {
            public static void Postfix(BaseNuclearReactor __instance)
            {
                //ErrorMessage.AddDebug("BaseNuclearReactor start");
                if (ReactorHasRad(__instance))
                    MakeRadioactive(__instance.gameObject, true, radRange[TechType.NuclearReactor]);
            }
        }

        [HarmonyPatch(typeof(BaseNuclearReactor), "OnEquip")]
        class BaseNuclearReactor_OnEquip_patch
        {
            public static void Postfix(BaseNuclearReactor __instance, string slot, InventoryItem item)
            {
                Pickupable pickupable = item.item;
                if (pickupable && radStuff.Contains(pickupable.GetTechType()))
                {
                    //ErrorMessage.AddDebug("Nuclear Reactor Equip rad");
                    MakeRadioactive(__instance.gameObject, true, radRange[TechType.NuclearReactor]);
                }
            }
        }

        [HarmonyPatch(typeof(StorageContainer), "Open", new Type[] { typeof(Transform) })]
        class StorageContainer_Open_patch
        {
            public static void Postfix(StorageContainer __instance)
            {
                if (CraftData.GetTechType(__instance.gameObject) == radLocker)
                {
                    //ErrorMessage.AddDebug("Rad locker open");
                    foreach (TechType tt in radStuff)
                    {
                        if (__instance.container.Contains(tt))
                        {
                            //ErrorMessage.AddDebug("Rad locker has rad");
                            radLockerOpen = true;
                            break;
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(StorageContainer), "OnClose")]
        class StorageContainer_OnClose_patch
        {
            public static void Postfix(StorageContainer __instance)
            {
                if (CraftData.GetTechType(__instance.gameObject) == radLocker)
                {
                    //ErrorMessage.AddDebug("Rad locker close");
                    //MakeRadioactive(__instance.gameObject, false);
                    radLockerOpen = false;
                }
            }
        }

        [HarmonyPatch(typeof(Crafter), "Craft")]
        class Crafter_Craft_Patch
        {
            static void Prefix(Crafter __instance, TechType techType, ref float duration)
            {
                if (techType == TechType.ReactorRod)
                {
                    //ErrorMessage.AddDebug("duration " + duration);
                    duration *= Main.config.rodCraftTimeMult;
                }
            }
        }

        [HarmonyPatch(typeof(Crafter), "CrafterOnDone")]
        class Crafter_CrafterOnDone_patch
        {
            public static void Postfix(Crafter __instance)
            {
                TechType techType = __instance.logic.craftingTechType;
                //ErrorMessage.AddDebug("CrafterOnDone " + techType);
                if (techType == TechType.ReactorRod)
                {
                    MakeRadioactive(__instance.gameObject, true, radRange[TechType.ReactorRod]);
                }
            }
        }

        [HarmonyPatch(typeof(CrafterLogic), "TryPickupSingle")]
        class CrafterLogic_TryPickupSingle_patch
        {
            public static void Postfix(CrafterLogic __instance, TechType techType, bool __result)
            {
                //TechType techType = __instance.logic.craftingTechType;
                //ErrorMessage.AddDebug("TryPickupSingle " + techType + " " + __result);
                if (__result && techType == TechType.ReactorRod)
                {
                    MakeRadioactive(__instance.gameObject, false);
                }
            }
        }

        //[HarmonyPatch(typeof(BaseNuclearReactor), "OnUnequip")]
        static class BaseNuclearReactor_OnUnequip_patch
        {
            public static void Postfix(BaseNuclearReactor __instance, string slot, InventoryItem item)
            {
                Pickupable pickupable = item.item;
                if (pickupable != null)
                {
                    if (ReactorHasRad(__instance) == false)
                        MakeRadioactive(__instance.gameObject, false, radRange[TechType.NuclearReactor]);
                }
            }
        }

        //[HarmonyPatch(typeof(BaseDeconstructable), "Deconstruct")]
        static class BaseDeconstructable_Deconstruct_patch
        {
            public static void Postfix(BaseDeconstructable __instance)
            {
                if (__instance.recipe == TechType.BaseNuclearReactor)
                {
                    Base baseComp = __instance.GetComponentInParent<Base>();
                    //ErrorMessage.AddDebug("BaseDeconstructable Deconstruct ");
                    //if (baseComp == null)
                    //    ErrorMessage.AddDebug("baseComp = null");
                    //else if (baseComp.IsEmpty())
                    //    ErrorMessage.AddDebug("baseComp  Empty");
                    //else
                    //    ErrorMessage.AddDebug("baseComp not Empty");
                }

                //ConstructableBase component1 = __instance.GetComponent<ConstructableBase>();
                //Pickupable pickupable = item.item;
                //if (pickupable != null)
                //{
                //    if (ReactorHasRad(__instance) == false)
                //        MakeRadioactive(__instance.gameObject, false, radRange[TechType.NuclearReactor]);
                //}
            }
        }

        //[HarmonyPatch(typeof(Constructable), "OnConstructedChanged")]
        static class Constructable_Deconstruct_patch
        {
            public static void Postfix(Constructable __instance, bool constructed)
            {
                //ErrorMessage.AddDebug("Constructable OnConstructedChanged " + constructed);
                //Pickupable pickupable = item.item;
                //if (pickupable != null)
                //{
                //    if (ReactorHasRad(__instance) == false)
                //        MakeRadioactive(__instance.gameObject, false, radRange[TechType.NuclearReactor]);
                //}
            }
        }

        //[HarmonyPatch(typeof(Pickupable), "Pickup")]
        class Pickupable_Pickup_Patch
        { // does not fire when exosuit picks up
            public static void Postfix(Pickupable __instance)
            {
                RadiatePlayerInRange radiatePlayerInRange = __instance.GetComponent<RadiatePlayerInRange>();
                if (radiatePlayerInRange)
                {
                    if (radPlayerInRange.ContainsKey(radiatePlayerInRange))
                    {
                        //ErrorMessage.AddDebug("Pickupable Pickup " + __instance.isPickupable);
                        //Main.Log(" Pickupable Pickup " + __instance.isPickupable);
                        //radPlayerInRange.Remove(radiatePlayerInRange);
                        //MakeRadioactive(__instance.gameObject, false);
                    }
                }
            }
        }

        //[HarmonyPatch(typeof(Inventory), "Pickup")]
        class Inventory_Pickup_patch
        {
            public static void Postfix(Inventory __instance, Pickupable pickupable)
            {
                TechType techType = pickupable.GetTechType();
                if (techType == TechType.UraniniteCrystal || techType == TechType.ReactorRod)
                {
                    //ErrorMessage.AddDebug("Picked up UraniniteCrystal");
                }

            }
        }

        //[HarmonyPatch(typeof(GhostCrafter), "Craft")]
        class GhostCrafter_Craft_patch
        {
            public static void Postfix(GhostCrafter __instance, TechType techType)
            {
                if (techType.ToString() == "MIUraninite")
                {
                    //ErrorMessage.AddDebug("Craft MIUraninite" );
                }

                //Main.Log("GhostCrafter Craft " + techType);
                if (techType == TechType.UraniniteCrystal || techType == TechType.ReactorRod)
                {
                    //ErrorMessage.AddDebug("Picked up UraniniteCrystal");
                }
            }
        }


    }
}