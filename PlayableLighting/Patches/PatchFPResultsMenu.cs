using FP2Lib.Badge;
using HarmonyLib;

namespace PlayableLightning.Patches
{
    internal class PatchFPResultsMenu
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPResultsMenu), "Update", MethodType.Normal)]
        private static void PatchResultsUpdate(float ___badgeCheckTimer)
        {
            if (___badgeCheckTimer < 61f && !FPStage.currentStage.disableBadgeChecks && FPSaveManager.character == PlayableLightning.currentLightningID)
            {
                if ((___badgeCheckTimer + FPStage.deltaTime) >= 60f)
                {
                    FPSaveManager.BadgeCheck(BadgeHandler.Badges["kubo.lightningrunner"].id);
                    FPSaveManager.BadgeCheck(BadgeHandler.Badges["kubo.lightningspeedrunner"].id);
                    FPSaveManager.BadgeCheck(BadgeHandler.Badges["kubo.lightningmaster"].id);
                }
            }
        }
    }
}
