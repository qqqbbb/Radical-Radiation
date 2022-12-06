
using HarmonyLib;
using QModManager.API.ModLoading;
using System.Reflection;
using System;
using SMLHelper.V2.Handlers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using System.Text;
using static ErrorMessage;

namespace Radical_Radiation
{
    class Testing
    {            
        //[HarmonyPatch(typeof(Player), "Update")]
        class Player_Update_Patch
        {
            static void Postfix(Player __instance)
            {
                AddDebug("rad " + Player.main.radiationAmount.ToString("0.0"));
                //AddDebug("health " + Player.main.liveMixin.health.ToString("0"));
                //AddDebug("health " + (int)Player.main.liveMixin.health);
                //AddDebug("timePassedAsFloat " + DayNightCycle.main.timePassedAsFloat);
                if (Input.GetKey(KeyCode.B))
                {
                    //AddDebug("currentSlot " + Main.config.escapePodSmokeOut[SaveLoadManager.main.currentSlot]);
                    //if (Player.main.IsInBase())
                    //    AddDebug("IsInBase");
                    //else if (Player.main.IsInSubmarine())
                    //    AddDebug("IsInSubmarine");
                    //else if (Player.main.inExosuit)
                    //    AddDebug("GetInMechMode");
                    //else if (Player.main.inSeamoth)
                    //    AddDebug("inSeamoth");
                    int x = Mathf.RoundToInt(Player.main.transform.position.x);
                    int y = Mathf.RoundToInt(Player.main.transform.position.y);
                    int z = Mathf.RoundToInt(Player.main.transform.position.z);
                    AddDebug(x + " " + y + " " + z);
                    AddDebug("" + Player.main.GetBiomeString());
                    //Inventory.main.container.Resize(8,8);   GetPlayerBiome()
                    //HandReticle.main.SetInteractText(nameof(startingFood) + " " + dict[i]);
                }

                if (Input.GetKey(KeyCode.C))
                {
                    Vehicle vehicle = Player.main.currentMountedVehicle;
                    if (vehicle)
                    {
                        RadPatches.IsProtected(vehicle);
                    }
 
                    //Survival survival = Player.main.GetComponent<Survival>();

                    //if (Input.GetKey(KeyCode.LeftShift))
                    //    survival.water++;
                    //else
                    //    survival.food++;
                }

                if (Input.GetKey(KeyCode.X))
                {
                    Survival survival = Player.main.GetComponent<Survival>();
                    if (Input.GetKey(KeyCode.LeftShift))
                        survival.water--;
                    else
                        survival.food--;
                }
                if (Input.GetKey(KeyCode.Z))
                {
                    GUIHand guiHand = __instance.GetComponent<GUIHand>();
                    Targeting.GetTarget(Player.main.gameObject, 5f, out GameObject target, out float targetDist);
                    if (target)
                    {

                    }
                    if (guiHand.activeTarget)
                    {
                        AddDebug("activeTarget " + guiHand.activeTarget.name);
                        AddDebug("activeTarget parent " + guiHand.activeTarget.transform.parent.name);
                        AddDebug("TechType " + CraftData.GetTechType(guiHand.activeTarget));
                    }
                    if (Input.GetAxis("Mouse ScrollWheel") > 0f)
                    {
                    }
                    else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
                    {
                    }

                    //else

                    //Inventory.main.DropHeldItem(true);
                    //Player.main.liveMixin.TakeDamage(99);
                    //Pickupable held = Inventory.main.GetHeld();
                    //AddDebug("isUnderwaterForSwimming " + Player.main.isUnderwaterForSwimming.value);
                    //AddDebug("isUnderwater " + Player.main.isUnderwater.value);
                    //LaserCutObject laserCutObject = 
                    //Inventory.main.quickSlots.Select(1);

                    if (guiHand.activeTarget)
                    {
                        //AddDebug("activeTarget " + Main.guiHand.activeTarget.name);
                        //AddDebug(" " + CraftData.GetTechType(Main.guiHand.activeTarget));
                        //RadiatePlayerInRange radiatePlayerInRange = Main.guiHand.activeTarget.GetComponent<RadiatePlayerInRange>();
                        //if (radiatePlayerInRange)
                        {

                        }
                        //else
                        //    AddDebug("no radiatePlayerInRange " );

                    }
                    //if (target)
                    //    Main.Message(" target " + target.name);
                    //else
                    //{
                    //TechType techType = CraftData.GetTechType(target);
                    //HarvestType harvestTypeFromTech = CraftData.GetHarvestTypeFromTech(techType);
                    //TechType harvest = CraftData.GetHarvestOutputData(techType);
                    //Main.Message("techType " + techType.AsString() );
                    //Main.Message("name " + target.name);
                    //}
                }
            }
        }


    }
}
