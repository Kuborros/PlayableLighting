using FP2Lib.Badge;
using HarmonyLib;

namespace PlayableLightning.Patches
{
    internal class PatchMenuClassic
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MenuClassic), "Update", MethodType.Normal)]
        private static void PatchStateDefault(float ___badgeCheckTimer)
        {
            if (___badgeCheckTimer > 0f && ___badgeCheckTimer < 26f && FPSaveManager.character == PlayableLightning.currentLightningID)
            {
                if ((___badgeCheckTimer + FPStage.deltaTime) >= 25f)
                {
                    FPSaveManager.BadgeCheck(BadgeHandler.Badges["kubo.lightningmaster"].id);
                }
            }
        }
    }
}
