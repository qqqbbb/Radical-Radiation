using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UWE;
using static ErrorMessage;

namespace Radical_Radiation
{
    internal class RadObject : MonoBehaviour
    {
        public RadObject()
        {
        }

        public void OnDestroy() 
        {
            //AddDebug("RadObject OnDestroy " + this.name);
            //RadPatches.MakeNotRadioactive(this.gameObject);
            RadiatePlayerInRange rpir = this.GetComponent<RadiatePlayerInRange>();
            if (rpir)
            {
                RadPatches.radObjectSub.Remove(rpir);
                if (RadPatches.closestRadObject == rpir)
                    RadPatches.closestRadObject = null;
            }
        }

        public void OnEnable()
        { // parent is null for this frame
            //AddDebug("RadObject OnEnable " + this.name);
            //if (WaitScreen.IsWaiting)
            //    RadPatches.goToMakeRad.Add(this.gameObject);
            //else
                CoroutineHost.StartCoroutine(RadPatches.MakeRadCR(this.gameObject));
        }

        public void OnDisable()
        {
            //AddDebug("RadObject OnDisable " + this.name);
            if (!WaitScreen.IsWaiting)
                RadPatches.MakeNotRad(this.gameObject);
        }



    }
}
