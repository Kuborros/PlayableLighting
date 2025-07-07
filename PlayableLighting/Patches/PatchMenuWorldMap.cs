using FP2Lib.Badge;
using HarmonyLib;

namespace PlayableLighting.Patches
{
    internal class PatchMenuWorldMap
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MenuWorldMap), "State_Default", MethodType.Normal)]
        private static void PatchStateDefault(bool ___cutsceneCheck, float ___badgeCheckTimer)
        {
            if (___cutsceneCheck && ___badgeCheckTimer > 0f && ___badgeCheckTimer < 26f && FPSaveManager.character == PlayableLighting.currentLightingID)
            {
                if ((___badgeCheckTimer + FPStage.deltaTime) >= 25f)
                {
                    FPSaveManager.BadgeCheck(BadgeHandler.Badges["kubo.lightingmaster"].id);
                }
            }
        }
    }
}
