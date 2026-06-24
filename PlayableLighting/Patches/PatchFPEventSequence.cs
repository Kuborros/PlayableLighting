using HarmonyLib;
using UnityEngine;

namespace PlayableLightning.Patches
{
    internal class PatchFPEventSequence
    {
        //Lightning Anywhere System™️ v2.1
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPEventSequence), "Start", MethodType.Normal)]
        static void PatchStateDefault(FPEventSequence __instance)
        {
            if (__instance != null && FPSaveManager.character == PlayableLightning.currentLightningID)
            {
                if (__instance.transform.parent != null)
                {
                    Transform cutsceneNeera = __instance.transform.parent.gameObject.transform.Find("Cutscene_Neera");
                    if (cutsceneNeera != null)
                    {
                        if (cutsceneNeera.gameObject.GetComponent<Animator>().runtimeAnimatorController.name != "Lightning Animator Player")
                        {
                            cutsceneNeera.gameObject.GetComponent<Animator>().runtimeAnimatorController = PlayableLightning.dataBundle.LoadAsset<RuntimeAnimatorController>("Lightning Animator Player");
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
                            if (cutsceneNeera.gameObject.GetComponent<Animator>().runtimeAnimatorController.name != "Lightning Animator Player")
                            {
                                cutsceneNeera.gameObject.GetComponent<Animator>().runtimeAnimatorController = PlayableLightning.dataBundle.LoadAsset<RuntimeAnimatorController>("Lightning Animator Player");
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
                        if (cutsceneNeeraClassic.gameObject.GetComponent<Animator>().runtimeAnimatorController.name != "Lightning Animator Player")
                        {
                            cutsceneNeeraClassic.gameObject.GetComponent<Animator>().runtimeAnimatorController = PlayableLightning.dataBundle.LoadAsset<RuntimeAnimatorController>("Lightning Animator Player");
                        }
                    }
                }

                //Kalaw cutscene
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Battlesphere_Kalaw")
                {
                    GameObject cutsceneNeeraClassic = GameObject.Find("Cutscene_Neera");
                    if (cutsceneNeeraClassic != null)
                    {
                        if (cutsceneNeeraClassic.GetComponent<Animator>().runtimeAnimatorController.name != "Lightning Animator Player")
                        {
                            cutsceneNeeraClassic.GetComponent<Animator>().runtimeAnimatorController = PlayableLightning.dataBundle.LoadAsset<RuntimeAnimatorController>("Lightning Animator Player");
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
            if (__instance != null && FPSaveManager.character == PlayableLightning.currentLightningID)
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