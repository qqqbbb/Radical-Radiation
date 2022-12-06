using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ErrorMessage;

namespace Radical_Radiation
{ 
    class RadPatches
    { 
        static float radiateInterval = .2f;
        static float damageInterval = 2f;
        static float radDamageTime = 0f;
        public static Dictionary<RadiatePlayerInRange, float> radPlayerInRange = new Dictionary<RadiatePlayerInRange, float>();
        public static Dictionary<TechType, float> radRange = new Dictionary<TechType, float>(); 
        public static HashSet<TechType> radStuff = new HashSet<TechType> { { TechType.ReactorRod }, { TechType.DepletedReactorRod }, { TechType.UraniniteCrystal } };
        public static TechType radLocker = TechType.None;
        public static TechType radModuleSeamoth = TechType.None;
        public static TechType radModuleCyclops = TechType.None;
        public static bool radLockerOpen = false;
        static PDA pda;
        public static GameObject subLocker;
        public static GameObject seamothStorage;

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
            //AddDebug("Update Radius Dict");
            radRange[TechType.ReactorRod] = Main.config.reactorRodRadius;
            radRange[TechType.DepletedReactorRod] = Main.config.reactorRodRadius;
            radRange[TechType.UraniniteCrystal] = Main.config.uraniniteCrystalRadius;
            radRange[TechType.DrillableUranium] = Main.config.drillableUraniniteRadius;
            radRange[TechType.NuclearReactor] = Main.config.nuclearReactorRadius;
            radRange[TechType.BaseNuclearReactor] = Main.config.nuclearReactorRadius;
        }

        public static void RebuildRadDict()
        {
            //AddDebug("Rebuild Rad Dict");
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
            //AddDebug("IsClosestToPlayer " + instance.name + " " + radPlayerInRange.ContainsKey(instance));
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

            if (closestInstance.Equals(instance))
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
                if (Inventory.main._container.Contains(tt) && radRange[tt] > 0)
                    return true;
            }
            return false;
        }

        public static void MakeRadioactive(GameObject go, bool active, float radius = 0f)
        {
            //AddDebug(" parent  " + go.transform.parent.name);
            //AddDebug(" parent.parent  " + go.transform.parent.parent.name);
            //Main.Log(go.name + " MakeRadioActive " + active);
            //if (active && radius == 0f)
            //    return;
            if (!uGUI.isLoading && !seamothStorage && !go.GetComponent<CyclopsLocker>())
            {
                //SeaMoth seaMoth = go.FindAncestor<SeaMoth>();
                //if (seaMoth)
                //{
                //    AddDebug("MakeRadioActive SeaMoth");
                    //go = seaMoth.gameObject;
                //}
                //else
                {
                    PrefabIdentifier prefabIdentifier = go.FindAncestor<PrefabIdentifier>();
                    if (prefabIdentifier)
                        go = prefabIdentifier.gameObject;
                }
            }
            //AddDebug("MakeRadioActive " + go.name + " " + active);
            //Main.Log("MakeRadioActive " + go.name + " " + active + " pos " + go.transform.position);

            PlayerDistanceTracker pdt = go.EnsureComponent<PlayerDistanceTracker>();
            pdt.enabled = active;
            pdt.maxDistance = radius;
            RadiatePlayerInRange rpir = go.EnsureComponent<RadiatePlayerInRange>();

            if (!active && rpir)
            {
                bool removed = radPlayerInRange.Remove(rpir);
                //Main.Log(go.name + " MakeRadioActive remove  radiatePlayerInRange " + removed);
            }
            rpir.enabled = active;
            rpir.radiateRadius = radius;
            DamagePlayerInRadius dpir = go.EnsureComponent<DamagePlayerInRadius>();
            dpir.enabled = active;
            dpir.damageType = DamageType.Radiation;
            dpir.damageAmount = 1f;
            dpir.updateInterval = 2f;
            dpir.damageRadius = radius;

            dpir.tracker = pdt;
            rpir.tracker = pdt;
        }

        public static void RadiatePlayerInv()
        {
            if (!GameModeUtils.HasRadiation() || (NoDamageConsoleCommand.main && NoDamageConsoleCommand.main.GetNoDamageCheat()))
            {
                //AddDebug("RadiatePlayerInv SetRadiationAmount 0");
                Player.main.SetRadiationAmount(0f);
                return;
            }
            if (InventoryHasRad())
            {
                //AddDebug("Player has ReactorRod");
                float amount = 1f;

                if (Inventory.main.equipment.GetCount(TechType.RadiationSuit) > 0)
                    amount -= 0.5f;
                if (Inventory.main.equipment.GetCount(TechType.RadiationHelmet) > 0)
                    amount -= 0.35f;
                if (Inventory.main.equipment.GetCount(TechType.RadiationGloves) > 0)
                    amount -= 0.15f;

                Player.main.SetRadiationAmount(amount);
                float damage = amount * 10f;
                //AddDebug("Radiate Player Inv " + amount);
                //damage = Mathf.Max(damage, 1F);
                if (damage > 0f && Time.time - radDamageTime > damageInterval)
                {
                    radDamageTime = Time.time;
                    //AddDebug(" damage Player inv " + damage);
                    Player.main.liveMixin.TakeDamage(damage, Player.main.transform.position, DamageType.Radiation);
                }
            }
            else if (radPlayerInRange.Count == 0)
            {
                //AddDebug("RadiatePlayerInv SetRadiationAmount 0 ");
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
                    TechType tt = pickupable.GetTechType();
                    if (pickupable != null && radStuff.Contains(tt) && radRange[tt] > 0)
                    {
                        //AddDebug("Nuclear Reactor has rad");
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsProtected(Vehicle vehicle)
        {
            //AddDebug("vehicle.slotIDs.Length " + vehicle.slotIDs.Length);
            for (int i = 0; i < vehicle.slotIDs.Length; i++)
            {
                TechType tt =  vehicle.modules.GetTechTypeInSlot(vehicle.slotIDs[i]);
                if (tt == radModuleSeamoth)
                {
                    //AddDebug("Protected " + tt);
                    return true;
                }
                //AddDebug("module "+ tt);
            }
            return false;
        }

        public static bool IsProtected(SubRoot sub)
        {
            Equipment modules = sub.upgradeConsole.modules;
            for (int index = 0; index < 6; ++index)
            {
                string slotName = SubRoot.slotNames[index];
                TechType techTypeInSlot = modules.GetTechTypeInSlot(slotName);
                //AddDebug("sub module " + techTypeInSlot);
                if (techTypeInSlot == radModuleCyclops)
                    return true;
            }
            return false;
        }

        public static bool IsSubLocker(GameObject go)
        {
            if (go.GetComponent<CyclopsLocker>())
                return true;
            else if (go.transform.parent && go.transform.parent.GetComponent<StorageContainer>() && go.transform.parent.parent && go.transform.parent.parent.GetComponent<SubControl>())
                return true;

            return false;
        }

        [HarmonyPatch(typeof(Drillable))]
        class Drillable_patch
        {
            [HarmonyPostfix]
            [HarmonyPatch( "Start")]
            public static void Start_Postfix(Drillable __instance)
            {
                //TechType techType = __instance.GetDominantResourceType();
                if (__instance.GetDominantResourceType() == TechType.UraniniteCrystal)
                    MakeRadioactive(__instance.gameObject, true, radRange[TechType.DrillableUranium]);
            }
            [HarmonyPostfix]
            [HarmonyPatch("OnDestroy")]
            public static void OnDestroy_Postfix(Drillable __instance)
            {
                if (__instance.GetDominantResourceType() == TechType.UraniniteCrystal)
                    MakeRadioactive(__instance.gameObject, false);
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
                    //AddDebug("Player.radiationAmount = 0");
                    return false;
                }

                if (__instance.tracker.distanceToPlayer < __instance.damageRadius)
                {
                    float damage = __instance.damageAmount * Player.main.radiationAmount * 10f;
                    //damage = Mathf.Max(damage, 1F);
                    //AddDebug(" damage Player " + damage);
                    Player.main.liveMixin.TakeDamage(damage, __instance.transform.position, __instance.damageType);
                }
                //else
                //AddDebug("Player too far");
                return false;
            }

        }

        [HarmonyPatch(typeof(RadiatePlayerInRange), "Radiate")]
        class RadiatePlayerInRange_Radiate_patch
        {
            public static bool Prefix(RadiatePlayerInRange __instance)
            {
                //AddDebug("RadiatePlayerInRange Radiate " + __instance.name);
                if (!GameModeUtils.HasRadiation() || (NoDamageConsoleCommand.main && NoDamageConsoleCommand.main.GetNoDamageCheat()))
                {
                    Player.main.SetRadiationAmount(0f);
                    return false;
                }
                if (!__instance.enabled)
                {
                    //AddDebug("disabled" );
                    return false;
                }
                if (InventoryHasRad())
                {
                    //AddDebug("InventoryHasRad");
                    return false;
                }
                //AddDebug("radPlayerInRange Count " + radPlayerInRange.Count);
                float distanceToPlayer = __instance.tracker.distanceToPlayer;
                //if (__instance.tracker.maxDistance < 111)
                //{
                //AddDebug("distanceToPlayer " + distanceToPlayer + " radiateRadius " + __instance.radiateRadius + " max " + __instance.tracker.maxDistance);
                //}
                if (__instance.radiateRadius > 0f && distanceToPlayer < __instance.radiateRadius)
                {
                    //AddDebug("tracker.maxDistance " + (int)__instance.tracker.maxDistance);
                    //amount = Mathf.Clamp01(amount);
                    //radPlayerInRange.Remove(__instance);
                    if (radPlayerInRange.ContainsKey(__instance))
                        radPlayerInRange[__instance] = distanceToPlayer;
                    else
                        radPlayerInRange.Add(__instance, distanceToPlayer);

                    //AddDebug("distanceToPlayer " + (int)distanceToPlayer);
                    //AddDebug("Radiate  " + __instance.gameObject.name + " " );
                    //AddDebug("radiateRadius  " + (int)__instance.radiateRadius);
                    if (IsClosestToPlayer(__instance))
                    {
                        //AddDebug("Radiate  " + __instance.gameObject.name + " " + (int)distanceToPlayer);
                        float amount = Mathf.Clamp01((1f - distanceToPlayer / __instance.radiateRadius));
                        float mult = 1f;

                        if (Inventory.main.equipment.GetCount(TechType.RadiationSuit) > 0)
                            mult -= 0.5f;
                        if (Inventory.main.equipment.GetCount(TechType.RadiationHelmet) > 0)
                            mult -= 0.35f;
                        if (Inventory.main.equipment.GetCount(TechType.RadiationGloves) > 0)
                            mult -= 0.15f;

                        if (mult > 0f)
                        {
                            if (Player.main.inExosuit)
                            {
                                float m = Main.config.ExosuitRadProtect * .01f;
                                mult *= (1f - m);
                                if (mult > 0f && IsProtected(Player.main.currentMountedVehicle))
                                    mult = 0f;
                            }
                            else if (Player.main.inSeamoth)
                            {
                                //AddDebug("inSeamoth   ");
                                float m = Main.config.SeamothRadProtect * .01f;
                                mult *= (1f - m);
                                if (mult > 0f && IsProtected(Player.main.currentMountedVehicle))
                                    mult = 0f;
                            }
                            if (Player.main.currentSub)
                            {
                                Base playerBase = Player.main.currentSub.GetComponent<Base>();
                                SubControl playerSub = Player.main.currentSub.GetComponent<SubControl>();
                                if (playerBase)
                                {
                                    //AddDebug("Player in Base ");
                                    Base radBase = __instance.gameObject.FindAncestor<Base>();
                                    if (radBase)
                                    {
                                        if (!playerBase.Equals(radBase))
                                            mult = GetBaseProtection();
                                    }
                                    else
                                    {
                                        //AddDebug("rad outside Base ");
                                        mult = GetBaseProtection();
                                        //AddDebug("GetBaseProtection() " + GetBaseProtection());

                                    }
                                }
                                else if(playerSub)
                                {
                          
                                    SubControl radSub = __instance.gameObject.FindAncestor<SubControl>();
                                    //AddDebug("player in Sub " + radSub);
                                    //if (__instance.transform.parent)
                                    //    AddDebug("rad parent " + __instance.transform.parent.name);
                                    //if (__instance.transform.parent.parent)
                                    //    AddDebug("rad parent parent " + __instance.transform.parent.parent.name);

                                    if (radSub && radSub.Equals(playerSub))
                                    {
                                        //AddDebug("the same sub " );
                                    }
                                    else
                                    {
                                        //AddDebug("Player in sub Protected " + IsProtected(Player.main._currentSub));
                                        mult = GetSubProtection(Player.main._currentSub);
                                    }
                                }
                            }
                            else // player outside
                            {
                                //AddDebug("Player outside ");
                                Base base_ = __instance.gameObject.FindAncestor<Base>();
                                SubControl sub = __instance.gameObject.FindAncestor<SubControl>();
                                if (sub)
                                {
                                    //AddDebug("rad in sub ");
                                    SubRoot subRoot = sub.GetComponent<SubRoot>();
                                    if (subRoot)
                                        mult = GetSubProtection(subRoot);
                                }
                                else if (base_)
                                {
                                    mult = GetBaseProtection();
                                    //AddDebug("rad in base " );
                                }
                            }
                        }
                        amount *= Mathf.Clamp01(mult);
                        //AddDebug("SetRadiationAmount " + amount);
                        Player.main.SetRadiationAmount(amount);
                    }
                }
                else // out of range
                {
                    //AddDebug("distanceToPlayer " + (int)distanceToPlayer);
                    radPlayerInRange.Remove(__instance);
                }
                return false;
            }

            private static float GetBaseProtection()
            {
                return 1f - Main.config.baseRadProtect * .01f;
            }

            private static float GetSubProtection(SubRoot subRoot)
            {
                if (IsProtected(subRoot))
                    return 0;

                return 1f - Main.config.cyclopsRadProtect * .01f;
            }
        }

        [HarmonyPatch(typeof(DamageSystem))]
        class DamageSystem_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch( "CalculateDamage")]
            public static bool CalculateDamage_Prefix(DamageSystem __instance, float damage, DamageType type, GameObject target, GameObject dealer, ref float __result)
            {// vanilla method blocks rad damage if player in exosuit
                if (type == DamageType.Radiation && target.Equals(Player.mainObject))
                {
                    //AddDebug(target.name + " damage Prefix " + damage);
                    __result = damage;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Player))]
        class Player_patch
        {
            static float radTime = 0f;

            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            public static void Start_Postfix(Player __instance)
            {
                ModCompat();
                pda = Player.main.GetPDA();
                //foreach (TechType tt in radStuff)
                //    Main.Log("radStuff  " + tt);
            }

            [HarmonyPostfix]
            [HarmonyPatch("Update")]
            public static void Update_Postfix(Player __instance)
            {
                //if (Input.GetKey(KeyCode.C))
                //{
                //    GameObject target = Player.main.GetComponent<GUIHand>().activeTarget;
                //    if (target)
                //    {
                //        AddDebug("target " + CraftData.GetTechType(target));
                //    }
                //}
                //Player.main.oxygenMgr.AddOxygen(115f); 
                //AddDebug("radRange.Count " + radRange.Count);
                //AddDebug("reactor Rod Radius " + );
                if (Time.time - radTime > radiateInterval)
                {
                    radTime = Time.time;
                    RadiatePlayerInv();
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch("UpdateRadiationSound")]
            public static bool UpdateRadiationSound_Prefix(Player __instance)
            {
                float radiationAmount = __instance.radiationAmount;
                //AddDebug("UpdateRadiationSound " + radiationAmount);
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

        [HarmonyPatch(typeof(Pickupable))]
        class Pickupable_patch
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(Pickupable), "Awake")]
            public static void Awake_Postfix(Pickupable __instance)
            {
                TechType techType = __instance.GetTechType();

                //AddDebug(__instance.gameObject.name + " Pickupable Awake " + __instance.GetTechType());
                if (radStuff.Contains(techType))
                {
                    if (__instance.isPickupable) // not in container
                    {
                        //AddDebug(__instance.gameObject.name + " Pickupable Awake " + __instance.GetTechType());
                        //Main.Log(__instance.gameObject.name + " Pickupable Awake " + __instance.GetTechName());
                        MakeRadioactive(__instance.gameObject, true, radRange[techType]);
                    }
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("Drop", new Type[] { typeof(Vector3), typeof(Vector3), typeof(bool) })]
            public static void Drop_Postfix(Pickupable __instance)
            {
                TechType techType = __instance.GetTechType();
                if (radStuff.Contains(techType))
                {
                    //Main.Log(" Drop "+ __instance.gameObject.name);
                    MakeRadioactive(__instance.gameObject, true, radRange[techType]);
                    //AddDebug("Drop " + __instance.gameObject.name);
                }
            }
        }

        [HarmonyPatch(typeof(ItemsContainer))]
        class ItemsContainer_patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("NotifyAddItem")]
            public static void NotifyAddItem_Postfix(ItemsContainer __instance, InventoryItem item)
            {
                //if (__instance.tr.parent.name == "SeamothStorageModule(Clone)")
                //    AddDebug("NotifyAddItem SeamothStorageModule" );

                TechType techType = item.item.GetTechType();
                //Main.Log("NotifyAddItem " + techType);
                //AddDebug("NotifyAddItem " + techType);
                if (radStuff.Contains(techType) && radRange[techType] > 0)
                {
                    GameObject go = __instance.tr.gameObject;
                    //AddDebug("NotifyAddItem " + techType + " " + __instance.tr.parent.name);
                    if (item.item.GetComponent<RadiatePlayerInRange>())
                        MakeRadioactive(item.item.gameObject, false);

                    if (seamothStorage)
                    {
                        MakeRadioactive(seamothStorage, true, radRange[techType]);
                        return;
                    }
                    if (CraftData.GetTechType(go) == radLocker)
                    {
                        //AddDebug("NotifyAddItem radLocker ");
                        if (pda.isInUse)
                            radLockerOpen = true;
                    }
                    else if (__instance.tr.parent.gameObject.GetComponent("CyNukeReactorMono"))
                    {
                        //AddDebug("Cyclops Nuke Reactor !!!");
                        MakeRadioactive(go, true, radRange[TechType.BaseNuclearReactor]);
                    }
                    else if (Inventory.main._container != __instance)
                    {
                        //AddDebug("Inventory.main._container != __instance " );
                        if (subLocker)
                        {
                            MakeRadioactive(subLocker, true, radRange[techType]);
                            //AddDebug("NotifyAddItem subLocker");
                        }
                        else
                            MakeRadioactive(go, true, radRange[techType]);
                    }
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("NotifyRemoveItem")]
            public static void NotifyRemoveItem_Postfix(ItemsContainer __instance, InventoryItem item)
            {
                TechType techType = item.item.GetTechType();
                if (radStuff.Contains(techType))
                {
                    GameObject go = __instance.tr.gameObject;

                    if (Inventory.main._container != __instance && CraftData.GetTechType(go) != radLocker)
                    {
                        if (subLocker)
                        {
                            go = subLocker;
                            //AddDebug("NotifyRemoveItem subLocker");
                        }
                        int count = 0;
                        foreach (TechType tt in radStuff)
                            count += __instance.GetCount(tt);

                        if (count == 0)
                        {
                            if (seamothStorage)
                            {
                                MakeRadioactive(seamothStorage, false);
                                return;
                            }
                            MakeRadioactive(go, false);
                        }
                    }
                }
            }

        }

        [HarmonyPatch(typeof(BaseNuclearReactor))]
        class BaseNuclearReactor_patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            public static void Postfix(BaseNuclearReactor __instance)
            {
                //AddDebug("BaseNuclearReactor start");
                if (ReactorHasRad(__instance))
                    MakeRadioactive(__instance.gameObject, true, radRange[TechType.NuclearReactor]);
            }
            [HarmonyPostfix]
            [HarmonyPatch("OnEquip")]
            public static void OnEquip_Postfix(BaseNuclearReactor __instance, string slot, InventoryItem item)
            {
                Pickupable pickupable = item.item;
                TechType tt = pickupable.GetTechType();
                if (pickupable && radStuff.Contains(tt) && radRange[tt] > 0)
                {
                    //AddDebug("Nuclear Reactor Equip rad");
                    MakeRadioactive(__instance.gameObject, true, radRange[TechType.NuclearReactor]);
                }
            }
        }

        [HarmonyPatch(typeof(StorageContainer))]
        class StorageContainer_patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Open", new Type[] { typeof(Transform) })]
            public static void Open_Postfix(StorageContainer __instance)
            {
                if (CraftData.GetTechType(__instance.gameObject) == radLocker)
                {
                    //AddDebug("Rad locker open");
                    foreach (TechType tt in radStuff)
                    {
                        if (__instance.container.Contains(tt))
                        {
                            //AddDebug("Rad locker has rad");
                            radLockerOpen = true;
                            return;
                        }
                    }
                }
                if (Player.main._currentSub && __instance.prefabRoot && Player.main._currentSub.gameObject.Equals(__instance.prefabRoot))
                {
                    //AddDebug(" SUB LOCKER ! " );
                    subLocker = __instance.gameObject;
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("OnClose")]
            public static void OnClose_Postfix(StorageContainer __instance)
            {
                if (CraftData.GetTechType(__instance.gameObject) == radLocker)
                {
                    //AddDebug("Rad locker close");
                    //MakeRadioactive(__instance.gameObject, false);
                    radLockerOpen = false;
                }
                subLocker = null;
            }
        }

        [HarmonyPatch(typeof(Crafter))]
        class Crafter_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("Craft")]
            static void Craft_Prefix(Crafter __instance, TechType techType, ref float duration)
            {
                if (techType == TechType.ReactorRod)
                {
                    //AddDebug("duration " + duration);
                    duration *= Main.config.rodCraftTimeMult;
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("CrafterOnDone")]
            public static void CrafterOnDone_Postfix(Crafter __instance)
            {// Pickupable spawns when you pick it up not when crafting finishes
                TechType techType = __instance.logic.craftingTechType;
                //AddDebug("CrafterOnDone " + techType);
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
                //AddDebug("TryPickupSingle " + techType + " " + __result);
                if (__result && techType == TechType.ReactorRod)
                {
                    MakeRadioactive(__instance.gameObject, false);
                }
            }
        }

        [HarmonyPatch(typeof(SeamothStorageInput))]
        static class SeamothStorageInput_OpenPDA_patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("OpenPDA")]
            public static void OpenPDAPostfix(SeamothStorageInput __instance)
            {
                //AddDebug("SeamothStorageInput OpenPDA " + __instance.slotID);
                seamothStorage = __instance.gameObject;
            }

            [HarmonyPostfix]
            [HarmonyPatch("OnClosePDA")]
            public static void OnClosePDAPostfix(SeamothStorageInput __instance)
            {
                //AddDebug("SeamothStorageInput OnClosePDA " + __instance.slotID);
                seamothStorage = null;
            }
        }

        //[HarmonyPatch(typeof(uGUI_SignInput), "Awake")]
        static class uGUI_SignInput_Awake_patch
        {
            public static void Postfix(uGUI_SignInput __instance)
            {
                TechTag techTag = __instance.gameObject.FindAncestor<TechTag>();
                if (techTag)
                {
                    if (techTag.type.ToString() == "RadRadLocker")
                    {
                        //AddDebug("RadRadLocker uGUI_SignInput Awake");
                        __instance.stringDefaultLabel = Main.config.lockerName;
                        __instance.text = Main.config.lockerName;

                    }
                    //Base baseComp = __instance.GetComponentInParent<Base>();

                    //if (baseComp == null)
                    //    AddDebug("baseComp = null");
                    //else if (baseComp.IsEmpty())
                    //    AddDebug("baseComp  Empty");
                    //else
                    //    AddDebug("baseComp not Empty");
                }


            }
        }

        [HarmonyPatch(typeof(RadiationsScreenFXController), "Update")]
        static class RadiationsScreenFXController_Update_patch
        {
            public static bool Prefix(RadiationsScreenFXController __instance)
            {
                //AddDebug("RadiationsScreenFXController Update " + __instance.fx.enabled);
                if (!Main.config.screenFX)
                {
                    __instance.fx.enabled = false;
                    return false;
                }
                return true;
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
                        //AddDebug("Pickupable Pickup " + __instance.isPickupable);
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
                    //AddDebug("Picked up UraniniteCrystal");
                }

            }
        }

        //[HarmonyPatch(typeof(PlayerDistanceTracker), "ScheduledUpdate")]
        class PlayerDistanceTracker_ScheduledUpdate_patch
        {
            public static void Postfix(PlayerDistanceTracker __instance)
            {
                //if (techType.ToString() == "MIUraninite")
                //{
                if (__instance.timeLastUpdate + __instance.timeBetweenUpdates >= Time.time)
                    return;
                float magnitude = (__instance.transform.position - Player.main.transform.position).magnitude;
                //AddDebug("maxDistance " + __instance.maxDistance + " magnitude " + magnitude);
                //}

            }
        }

        //[HarmonyPatch(typeof(StorageContainer), "Open")]
        class StorageContainer_Open1_patch
        {
            public static void Postfix(StorageContainer __instance)
            {

                //float magnitude = (__instance.transform.position - Player.main.transform.position).magnitude;
                //AddDebug("prefabRoot " + __instance.prefabRoot.name) ;
                //AddDebug("parent.parent " + __instance.transform.parent.parent.gameObject.name);
            }
        }

    }
}