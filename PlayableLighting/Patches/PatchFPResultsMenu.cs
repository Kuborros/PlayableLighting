using FP2Lib.Badge;
using HarmonyLib;

namespace PlayableLighting.Patches
{
    internal class PatchFPResultsMenu
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPResultsMenu), "Update", MethodType.Normal)]
        private static void PatchResultsUpdate(float ___badgeCheckTimer)
        {
            if (___badgeCheckTimer < 61f && !FPStage.currentStage.disableBadgeChecks && FPSaveManager.character == PlayableLighting.currentLightingID)
            {
                if ((___badgeCheckTimer + FPStage.deltaTime) >= 60f)
                {
                    FPSaveManager.BadgeCheck(BadgeHandler.Badges["kubo.lightingrunner"].id);
                    FPSaveManager.BadgeCheck(BadgeHandler.Badges["kubo.lightingspeedrunner"].id);
                    FPSaveManager.BadgeCheck(BadgeHandler.Badges["kubo.lightingmaster"].id);
                }
            }
        }
    }
}
