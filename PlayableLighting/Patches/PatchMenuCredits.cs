using HarmonyLib;
using UnityEngine;

namespace PlayableLightning.Patches
{
    internal class PatchMenuCredits
    {
        [HarmonyPrefix]
        [HarmonyWrapSafe]
        [HarmonyPatch(typeof(MenuCredits), "Start", MethodType.Normal)]
        static void PatchMenuCreditsStartPre(ref float ___startY)
        {
            if (FPSaveManager.character == PlayableLightning.currentLightningID)
            {
                ___startY = -7000f;

                GameObject textCredits = GameObject.Find("ActorName (22)");
                if (textCredits != null)
                {
                    TextMesh textMesh = textCredits.GetComponent<TextMesh>();
                    if (textMesh != null)
                    {
                        textMesh.text += "\r\n\r\nPLAYABLE LIGHTNING MOD\r\n" +
                            "\r\nKubo - Code" +
                            "\r\nSesAeon - Sprites and Character" +
                            "\r\nAlejandra Caudillo de los Ríos - Lightning's VA" +
                            "\r\nSuperWillGaming - Sprites" +
                            "\r\nStarblue3 - Sprites" +
                            "\r\nCAPCOM - Original Music and Sprites" +
                            "\r\nYasha - Sprites";
                    }
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyWrapSafe]
        [HarmonyPatch(typeof(MenuCredits), "Start", MethodType.Normal)]
        static void PatchMenuCreditsStart(ref float ___endY, ref MenuCredits __instance)
        {
            __instance.transform.position = new Vector3(__instance.transform.position.x, __instance.transform.position.y - 50f, __instance.transform.position.z);
            ___endY = __instance.transform.position.y;
        }
    }
}
