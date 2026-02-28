using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PlayableLighting.Patches
{
    internal class PatchFPEventSequence
    {
        //Lighting Anywhere System™️ v2
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPEventSequence), "Start", MethodType.Normal)]
        static void PatchStateDefault(FPEventSequence __instance)
        {
            if (__instance != null && FPSaveManager.character == PlayableLighting.currentLightingID)
            {
                if (__instance.transform.parent != null)
                {
                    Transform cutsceneNeera = __instance.transform.parent.gameObject.transform.Find("Cutscene_Neera");
                    if (cutsceneNeera != null)
                    {
                        if (cutsceneNeera.gameObject.GetComponent<Animator>().runtimeAnimatorController.name != "Lighting Animator Player")
                        {
                            cutsceneNeera.gameObject.GetComponent<Animator>().runtimeAnimatorController = PlayableLighting.dataBundle.LoadAsset<RuntimeAnimatorController>("Lighting Animator Player");
                            //cutsceneNeera.Find("tail").gameObject.SetActive(false);
                        }
                    }
                }

                //Post-Merga fight special case
                if (__instance.transform.parent != null && FPStage.stageNameString == "Merga")
                {
                    Transform eventSequence = __instance.transform.parent.gameObject.transform;
                    if (eventSequence != null)
                    {
                        Transform cutsceneNeera = eventSequence.parent.gameObject.transform.Find("Cutscene_Neera");
                        if (cutsceneNeera != null)
                        {
                            if (cutsceneNeera.gameObject.GetComponent<Animator>().runtimeAnimatorController.name != "Lighting Animator Player")
                            {
                                cutsceneNeera.gameObject.GetComponent<Animator>().runtimeAnimatorController = PlayableLighting.dataBundle.LoadAsset<RuntimeAnimatorController>("Lighting Animator Player");
                                //cutsceneNeera.Find("tail").gameObject.SetActive(false);
                            }
                        }
                    }
                }

                //Snowfields magic
                if (__instance.transform.Find("Cutscene_Neera_Classic") != null)
                {
                    Transform cutsceneNeeraClassic = __instance.transform.Find("Cutscene_Neera_Classic");
                    if (cutsceneNeeraClassic != null)
                    {
                        if (cutsceneNeeraClassic.gameObject.GetComponent<Animator>().runtimeAnimatorController.name != "Lighting Animator Player")
                        {
                            cutsceneNeeraClassic.gameObject.GetComponent<Animator>().runtimeAnimatorController = PlayableLighting.dataBundle.LoadAsset<RuntimeAnimatorController>("Lighting Animator Player");
                            //cutsceneNeeraClassic.Find("tail").gameObject.SetActive(false);
                        }
                    }
                }
            }
        }


        //Special ending cutscene skipping code.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPEventSequence), "State_Event", MethodType.Normal)]
        static void PatchStateEvent(FPEventSequence __instance)
        {
            if (__instance != null && FPSaveManager.character == PlayableLighting.currentLightingID)
            {
                if (__instance.transform.parent != null && (FPStage.stageNameString == "Merga"))
                {
                    Transform eventSequence = __instance.transform.parent.gameObject.transform;
                    if (eventSequence != null)
                    {
                        Transform cutsceneNeera = eventSequence.parent.gameObject.transform.Find("Cutscene_Neera");
                        if (cutsceneNeera != null)
                        {
                            __instance.Action_SkipScene();
                        }
                    }
                }
            }
            if (__instance != null)
            {
                if (__instance.name == "Event Activator (Classic)" && __instance.transform.parent != null)
                {
                    if (__instance.transform.parent.gameObject.name == "Ending")
                        __instance.Action_SkipScene();
                }
            }
        }
    }
}
}
