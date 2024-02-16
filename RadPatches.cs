using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ErrorMessage;
using System.Linq;


namespace Radical_Radiation
{ 
    class RadPatches
    { 
        static float radiateInterval = .2f;
        static float damageInterval = 2f;
        static float radDamageTime = 0f;
        static int defaulrRadItemRadius = 15;
        static int defaulrRadRadius = 30;
        //public static Dictionary<RadiatePlayerInRange, float> radPlayerInRange = new Dictionary<RadiatePlayerInRange, float>();
        public static RadiatePlayerInRange closestRadObject;
        public static Dictionary<TechType, int> radRange = new Dictionary<TechType, int>(); 
        public static TechType radLocker = TechType.None;
        public static TechType radModuleSeamoth = TechType.None;
        public static TechType radModuleCyclops = TechType.None;
        public static bool radLockerOpen = false;
        //public static HashSet<GameObject> goToMakeRad = new HashSet<GameObject>();
        public static Dictionary<string, string> cyclopsCont = new Dictionary<string, string> {{ "Locker01StorageRoot", "submarine_locker_01_01" }, { "Locker02StorageRoot", "submarine_locker_01_02" }, { "Locker03StorageRoot", "submarine_locker_01_03" }, { "Locker04StorageRoot", "submarine_locker_01_04" }, { "Locker05StorageRoot", "submarine_locker_01_05" } };
        public static Dictionary<RadiatePlayerInRange, SubRoot> radObjectSub = new Dictionary<RadiatePlayerInRange, SubRoot>();

        public static bool Approximately(float a, float b, float tolerance = 0.000001f)
        {
            return (Mathf.Abs(a - b) < tolerance);
        }

        public static GameObject GetEntityRoot(GameObject go)
        { // UWE.Utils.GetEntityRoot returns null
            PrefabIdentifier prefabIdentifier = go.GetComponent<PrefabIdentifier>();
            if (prefabIdentifier == null)
                prefabIdentifier = go.GetComponentInParent<PrefabIdentifier>();

            return prefabIdentifier != null ? prefabIdentifier.gameObject : go;
        }

        public static GameObject GeParentEntityRoot(GameObject go)
        {
            if (go.transform.parent == null)
                return null;
                          
            PrefabIdentifier prefabIdentifier = go.GetComponent<PrefabIdentifier>();
            if (prefabIdentifier == null)
                prefabIdentifier = go.GetComponentInParent<PrefabIdentifier>();

            return prefabIdentifier != null ? prefabIdentifier.gameObject : go;
        }

        public static void ModCompat()
        {
            TechType uranIngot = TechType.None;
            TechTypeExtensions.FromString("MIUraninite", out uranIngot, false);
               //uraninite = (TechType)Enum.Parse(typeof(TechType), "MIUraninite");
            if (uranIngot != TechType.None)
            {
                //radStuff.Add(uranIngot);
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
            radRange[TechType.BaseNuclearReactor] = Main.config.nuclearReactorRadius;
            radRange[TechType.Locker] = Main.config.uraniniteCrystalRadius > Main.config.reactorRodRadius ? Main.config.uraniniteCrystalRadius : Main.config.reactorRodRadius;
        }

        public static bool InventoryHasRad()
        {
            if (!Inventory.main || Inventory.main._container == null)
                return false;

            if (radLockerOpen)
                return true;

            //foreach (var pair in radRange)
            {
                if (Inventory.main._container.Contains(TechType.UraniniteCrystal) && radRange[TechType.UraniniteCrystal] > 0)
                    return true;
                else if (Inventory.main._container.Contains(TechType.ReactorRod) && radRange[TechType.ReactorRod] > 0)
                    return true;
                else if (Inventory.main._container.Contains(TechType.DepletedReactorRod) && radRange[TechType.DepletedReactorRod] > 0)
                    return true;
            }
            return false;
        }

        public static IEnumerator<GameObject> MakeRadCR(GameObject go)
        {
            //int waitFrames = 0;
            while (WaitScreen.IsWaiting)
                yield return null;

            while (go.transform.parent == null)
            {
                //waitFrames++;
                yield return null;
            }
            //AddDebug("MakeRad waitFrames " + waitFrames);
            MakeRad(go);
        }

        public static void MakeRad(GameObject go)
        {
            if (!go.activeSelf)
                return;

            if (!go.name.StartsWith("submarine_locker_"))
                go = GetEntityRoot(go);
            
            //Main.logger.LogMessage("MakeRad " + go.name + " parent " + go.transform.parent.name);
            TechType techType = CraftData.GetTechType(go);
            if (techType == TechType.None)
            {
                if (go.GetComponent<BaseNuclearReactor>())
                    techType = TechType.BaseNuclearReactor;
            }
            if (techType == TechType.Cyclops || techType == TechType.SmallLocker || techType == TechType.Exosuit || techType == TechType.Seamoth || techType == TechType.SmallStorage)
            {
                techType = TechType.Locker;
            }
            //if (go.transform.parent == null)
            //    AddDebug("MakeRad " + go.name + " no parent !!!");
            //else
            //    AddDebug("MakeRad " + go.name + " TT " + techType);

            int radius;
            if (radRange.ContainsKey(techType))
                radius = radRange[techType];
            else
            {
                if (go.GetComponent<Pickupable>())
                    radius = defaulrRadItemRadius;
                else
                    radius = defaulrRadRadius;
            }
            PlayerDistanceTracker pdt = go.EnsureComponent<PlayerDistanceTracker>();
            pdt.enabled = true;
            pdt.maxDistance = radius;
            RadiatePlayerInRange rpir = go.EnsureComponent<RadiatePlayerInRange>();
            SubRoot subRoot = go.GetComponentInParent<SubRoot>();
            if (subRoot)
            {
                radObjectSub[rpir] = subRoot;
                //AddDebug("RegisterRadObject " + rpir.name + " parent " + subRoot.name);
            }
            rpir.enabled = true;
            rpir.radiateRadius = radius;
            DamagePlayerInRadius dpir = go.EnsureComponent<DamagePlayerInRadius>();
            dpir.enabled = true;
            dpir.damageType = DamageType.Radiation;
            dpir.damageAmount = 1f;
            dpir.updateInterval = 2f;
            dpir.damageRadius = radius;
            dpir.tracker = pdt;
            rpir.tracker = pdt;
            //AddDebug("MakeRad " + go.name + " done ");
        }

        public static void MakeNotRad(GameObject go)
        {
            //AddDebug(" parent  " + go.transform.parent.name);
            if (!go.name.StartsWith("submarine_locker_"))
                go = GetEntityRoot(go);
            
            //Main.logger.LogMessage("MakeNotRad " + go.name + " parent " + go.transform.parent.name);
            //AddDebug("MakeNotRad " + go.name );

            PlayerDistanceTracker pdt = go.GetComponent<PlayerDistanceTracker>();
            if (pdt)
                pdt.enabled = false;

            RadiatePlayerInRange rpir = go.GetComponent<RadiatePlayerInRange>();
            if (rpir)
            {
                radObjectSub.Remove(rpir);
                rpir.enabled = false;
                if (closestRadObject == rpir)
                    closestRadObject = null;
                //AddDebug("RegisterRadObject " + rpir.name + " parent " + subRoot.name);
            }
            DamagePlayerInRadius dpir = go.GetComponent<DamagePlayerInRadius>();
            if (dpir)
                dpir.enabled = false;
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
            else if (closestRadObject == null)
            {
                //AddDebug("RadiatePlayerInv SetRadiationAmount 0 ");
                Player.main.SetRadiationAmount(0f);
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
                    if (pickupable != null && radRange.ContainsKey(tt) && radRange[tt] > 0)
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
                {
                    __instance.gameObject.EnsureComponent<RadObject>();
                }
            }
            //[HarmonyPostfix]
            //[HarmonyPatch("OnDestroy")]
            public static void OnDestroy_Postfix(Drillable __instance)
            {
                if (__instance.GetDominantResourceType() == TechType.UraniniteCrystal)
                    MakeRadCR(__instance.gameObject);
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
                if (!GameModeUtils.HasRadiation() || NoDamageConsoleCommand.main && NoDamageConsoleCommand.main.GetNoDamageCheat())
                {
                    Player.main.SetRadiationAmount(0f);
                    return false;
                }
                if (!__instance.isActiveAndEnabled)
                {
                    //AddDebug("RadiatePlayerInRange " + __instance.name + " not ActiveAndEnabled");
                    //RebuildRadDict();
                    return false;
                }
                //if (radPlayerInRange.ContainsKey(__instance))
                //    AddDebug("RadiatePlayerInRange has " + __instance.name);
                //else
                //    AddDebug("RadiatePlayerInRange has no " + __instance.name);

                if (InventoryHasRad())
                {
                    //AddDebug("InventoryHasRad");
                    return false;
                }
                //AddDebug("radPlayerInRange Count " + radPlayerInRange.Count);
                float distanceToPlayer = __instance.tracker.distanceToPlayer;
                //if (__instance.tracker.maxDistance < 111)
                //{
                //AddDebug(__instance.name + " distanceToPlayer " + distanceToPlayer + " radiateRadius " + __instance.radiateRadius + " max " + __instance.tracker.maxDistance);
                //}
                if (__instance.radiateRadius > 0 && distanceToPlayer < __instance.radiateRadius)
                {
                    //amount = Mathf.Clamp01(amount);
                    //radPlayerInRange.Remove(__instance);
                    //RegisterRadObject(__instance, distanceToPlayer);
                    //AddDebug("distanceToPlayer " + (int)distanceToPlayer);
                    //AddDebug("Radiate  " + __instance.gameObject.name + " " );
                    //AddDebug("radiateRadius  " + (int)__instance.radiateRadius);
                    bool closest = closestRadObject == null || closestRadObject && closestRadObject == __instance || closestRadObject.tracker.distanceToPlayer > distanceToPlayer;
                    //AddDebug(__instance.name + " radiate " + closest);
                    if (closest )
                    {
                        closestRadObject = __instance;
                            //AddDebug("Radiate  " + __instance.gameObject.name + " " + (int)distanceToPlayer);
                        float amount = Mathf.Clamp01(1f - distanceToPlayer / __instance.radiateRadius);
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
                                //Base playerBase = Player.main.currentSub.GetComponent<Base>();
                                //SubControl playerSub = Player.main.currentSub.GetComponent<SubControl>();
                                SubRoot playerSub = Player.main.currentSub;
                                if (playerSub && playerSub.isBase)
                                {
                                    //AddDebug("Player in Base ");
                                    if (radObjectSub.ContainsKey(__instance))
                                    {
                                        if (playerSub != radObjectSub[__instance])
                                            mult = GetBaseProtection();
                                    }
                                    else
                                    {
                                        //AddDebug("rad outside Base ");
                                        mult = GetBaseProtection();
                                        //AddDebug("GetBaseProtection() " + GetBaseProtection());

                                    }
                                }
                                else if(playerSub && playerSub.isCyclops)
                                {
                                    //SubControl radSub = __instance.gameObject.FindAncestor<SubControl>();
                                    //AddDebug("player in Sub " + radSub);
                                    //if (__instance.transform.parent)
                                    //    AddDebug("rad parent " + __instance.transform.parent.name);
                                    //if (__instance.transform.parent.parent)
                                    //    AddDebug("rad parent parent " + __instance.transform.parent.parent.name);

                                    if (radObjectSub.ContainsKey(__instance))
                                    {
                                        if (playerSub != radObjectSub[__instance])
                                            mult = GetBaseProtection();
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
                                if (radObjectSub.ContainsKey(__instance))
                                {
                                    //AddDebug("rad in sub ");
                                    SubRoot subRoot = radObjectSub[__instance];
                                    if (subRoot.isBase)
                                    {
                                        mult = GetBaseProtection();
                                    }
                                    else if (subRoot.isCyclops)
                                    {
                                        mult = GetSubProtection(subRoot);
                                    }
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
                    if (closestRadObject == __instance)
                        closestRadObject = null;
                    //AddDebug(__instance.name + " distanceToPlayer " + distanceToPlayer + " radiateRadius " + __instance.radiateRadius + " max " + __instance.tracker.maxDistance);
                    //AddDebug("distanceToPlayer " + (int)distanceToPlayer);
                    //UnregisterRadObject(__instance);
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
                if (type == DamageType.Radiation && target == Player.mainObject)
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

            //[HarmonyPostfix]
            //[HarmonyPatch("Start")]
            public static void Start_Postfix(Player __instance)
            {
               
                //pda = Player.main.GetPDA();
                //foreach (TechType tt in radStuff)
                //    Main.Log("radStuff  " + tt);
            }

            [HarmonyPostfix]
            [HarmonyPatch("Update")]
            public static void Update_Postfix(Player __instance)
            {
                //if (Input.GetKey(KeyCode.C))
                {
                    //AddDebug("radPlayerInRange " + radPlayerInRange.Count);
                    //foreach (var item in radPlayerInRange)
                    //{
                    //    AddDebug(" " + item.Key.name + " " + item.Value);
                    //}
                }
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
                if (FMODUWE.IsInvalidParameterId(__instance.fmodIndexIntensity))
                    __instance.fmodIndexIntensity = __instance.radiateSound.GetParameterIndex("intensity");

                if (radiationAmount == 0f && Main.config.radSound)
                {
                    if (closestRadObject != null || InventoryHasRad())
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

                bool show = closestRadObject || InventoryHasRad();
                if (Approximately(Player.main.radiationAmount, 0f) && !Main.config.showRadWarning)
                    show = false;

                //AddDebug("uGUI_RadiationWarning " + show);
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
                if (radRange.ContainsKey(techType))
                {
                    __instance.gameObject.EnsureComponent<RadObject>();
                }
            }

            //[HarmonyPostfix]
            //[HarmonyPatch("Drop", new Type[] { typeof(Vector3), typeof(Vector3), typeof(bool) })]
            public static void Drop_Postfix(Pickupable __instance)
            {
                TechType techType = __instance.GetTechType();
                if (radRange.ContainsKey(techType))
                {
                    //Main.Log(" Drop "+ __instance.gameObject.name);
                    //MakeRadioactive(__instance.gameObject, true, false, techType);
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
                UWE.CoroutineHost.StartCoroutine(ItemAddedToCont(__instance, item));
            }

            private static IEnumerator<GameObject> ItemAddedToCont(ItemsContainer __instance, InventoryItem item)
            {
                while (WaitScreen.IsWaiting)
                    yield return null;

                TechType techType = item.item.GetTechType();
                if (!radRange.ContainsKey(techType) || radRange[techType] <= 0)
                    yield break;

                GameObject containerGO = GetEntityRoot(__instance.tr.gameObject);
                //AddDebug("NotifyAddItem " + techType + " " + containerGO.name);
                //Main.logger.LogMessage("NotifyAddItem " + techType + " " + __instance.tr.parent.name);

                if (CraftData.GetTechType(containerGO) == radLocker)
                {
                    //AddDebug("NotifyAddItem radLocker ");
                    if (Player.main.GetPDA().isInUse)
                        radLockerOpen = true;

                    yield break;
                }
                //else if (__instance.tr.parent.gameObject.GetComponent("CyNukeReactorMono"))
                //{
                //AddDebug("Cyclops Nuke Reactor !!!");
                //    if (WaitScreen.IsWaiting)
                //        containersToMakeRad.Add(containerGO, radRange[TechType.BaseNuclearReactor]);
                //    else
                //        MakeRadioactive(containerGO, true, radRange[TechType.BaseNuclearReactor]);
                //}
                else if (Inventory.main._container != __instance)
                {
                    //AddDebug("Inventory.main._container != __instance " );
                    if (cyclopsCont.ContainsKey(__instance.tr.name) && __instance.tr.parent.parent && __instance.tr.parent.parent.name == "Cyclops-MainPrefab(Clone)")
                    {
                        string name = cyclopsCont[__instance.tr.name];
                        if (name != null)
                        {
                            Transform tr = __instance.tr.parent.Find(name);
                            if (tr)
                            {
                                containerGO = tr.gameObject;
                                //AddDebug("NotifyAddItem sub locker ");
                            }
                        }
                    }
                    containerGO.EnsureComponent<RadObject>();
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("NotifyRemoveItem")]
            public static void NotifyRemoveItem_Postfix(ItemsContainer __instance, InventoryItem item)
            {
                TechType techType = item.item.GetTechType();
                if (radRange.ContainsKey(techType))
                {
                    GameObject containerGO = GetEntityRoot(__instance.tr.gameObject);
                    //AddDebug("NotifyRemoveItem " + containerGO.name);

                    if (Inventory.main._container != __instance && CraftData.GetTechType(containerGO) != radLocker)
                    {
                        if (cyclopsCont.ContainsKey(__instance.tr.name) && __instance.tr.parent.parent && __instance.tr.parent.parent.name == "Cyclops-MainPrefab(Clone)")
                        {
                            string name = cyclopsCont[__instance.tr.name];
                            if (name != null)
                            { 
                                Transform tr = __instance.tr.parent.Find(name);
                                if (tr)
                                    containerGO = tr.gameObject;
                            }
                        }
                        int count = 0;
                        foreach (var pair in radRange)
                            count += __instance.GetCount(pair.Key);

                        if (count == 0)
                        {
                            //MakeRadioactive(containerGO, false);
                            RadObject radObject = containerGO.GetComponent<RadObject>();
                            if (radObject)
                            {
                                UnityEngine.Object.Destroy(radObject);
                            }
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
                {
                    __instance.gameObject.EnsureComponent<RadObject>();
                    //radObject.techType = TechType.BaseNuclearReactor;
                    //MakeRadioactive(__instance.gameObject, false, TechType.BaseNuclearReactor);
                }
            }
          
            [HarmonyPostfix]
            [HarmonyPatch("OnEquip")]
            public static void OnEquip_Postfix(BaseNuclearReactor __instance, string slot, InventoryItem item)
            {
                Pickupable pickupable = item.item;
                TechType tt = pickupable.GetTechType();
                if (pickupable && radRange.ContainsKey(tt) && radRange[tt] > 0)
                {
                    //AddDebug("Nuclear Reactor Equip rad");
                    __instance.gameObject.EnsureComponent<RadObject>();
                    //radObject.techType = TechType.BaseNuclearReactor;
                    //MakeRadioactive(__instance.gameObject, false, TechType.BaseNuclearReactor);
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
                    foreach (var pair in radRange)
                    {
                        if (__instance.container.Contains(pair.Key))
                        {
                            //AddDebug("Rad locker has rad");
                            radLockerOpen = true;
                            return;
                        }
                    }
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
                    __instance.gameObject.EnsureComponent<RadObject>();
                    //radObject.techType = TechType.ReactorRod;
                    //MakeRadioactive(__instance.gameObject, false, TechType.ReactorRod);
                }
            }
        }

        [HarmonyPatch(typeof(CrafterLogic), "NotifyPickup")]
        class CrafterLogic_NotifyPickup_patch
        {
            public static void Postfix(CrafterLogic __instance)
            {
                //AddDebug("NotifyPickup " + __instance.craftingTechType);
                if (__instance.craftingTechType == TechType.ReactorRod)
                {
                    MakeNotRad(__instance.gameObject);
                    RadObject radObject = __instance.GetComponent<RadObject>();
                    if (radObject)
                        UnityEngine.GameObject.Destroy(radObject);
                }
            }
        }

        //[HarmonyPatch(typeof(SeamothStorageInput))]
        static class SeamothStorageInput_OpenPDA_patch
        {
            //[HarmonyPostfix]
            //[HarmonyPatch("OpenPDA")]
            public static void OpenPDAPostfix(SeamothStorageInput __instance)
            {
                //AddDebug("SeamothStorageInput OpenPDA " + __instance.slotID);
                //seamothStorage = __instance.gameObject;
            }

            //[HarmonyPostfix]
            //[HarmonyPatch("OnClosePDA")]
            public static void OnClosePDAPostfix(SeamothStorageInput __instance)
            {
                //AddDebug("SeamothStorageInput OnClosePDA " + __instance.slotID);
                //seamothStorage = null;
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
                    if (techTag.type.ToString() == "RadLocker")
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
                    //if (radPlayerInRange.ContainsKey(radiatePlayerInRange))
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

        [HarmonyPatch(typeof(WaitScreen), "Remove")]
        class WaitScreen_Remove_Patch
        {
            public static void Postfix(WaitScreen.IWaitItem item)
            { // __instance is null !
                if (WaitScreen.main.items.Count == 0)
                {
                    UWE.CoroutineHost.StartCoroutine(LoadedGameSetup());
                    //LoadedGameSetup();
                    //AddDebug("loaded game world");
                }
            }
        }

        //[HarmonyPatch(typeof(Constructable), "DeconstructAsync")]
        class Constructable_DeconstructAsync_Patch
        {
            public static void Postfix(Constructable __instance)
            { 
                if (__instance.constructedAmount == 0 && __instance.techType == TechType.BaseNuclearReactor)
                {
                    //RadiatePlayerInRange rpir = __instance.GetComponent<RadiatePlayerInRange>();
                    //if (rpir && radPlayerInRange.ContainsKey(rpir))
                    {
                        //AddDebug("DeconstructAsync " + __instance.techType  + " " + __instance.constructedAmount);
                        //MakeRadioactive(__instance.gameObject, false);
                        //if (RadDictNeedsRebuild())
                            //RebuildRadDict();
                    }
                }
            }
        }

        public static IEnumerator<GameObject> LoadedGameSetup()
        {
            while (WaitScreen.IsWaiting)
                yield return null;
            
            //AddDebug("LoadedGameSetup IsWaiting " + WaitScreen.IsWaiting);
            ModCompat();
            //foreach (var go in goToMakeRad)
            {
                //MakeRadioactive(go);
                //go.EnsureComponent<RadObject>();
            }
            //goToMakeRad = new HashSet<GameObject>();
        }
        
    }
}