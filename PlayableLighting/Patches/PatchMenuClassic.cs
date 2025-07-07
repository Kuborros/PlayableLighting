using FP2Lib.Badge;
using HarmonyLib;

namespace PlayableLighting.Patches
{
    internal class PatchMenuClassic
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MenuClassic), "Update", MethodType.Normal)]
        private static void PatchStateDefault(float ___badgeCheckTimer)
        {
            if (___badgeCheckTimer > 0f && ___badgeCheckTimer < 26f && FPSaveManager.character == PlayableLighting.currentLightingID)
            {
                if ((___badgeCheckTimer + FPStage.deltaTime) >= 25f)
                {
                    FPSaveManager.BadgeCheck(BadgeHandler.Badges["kubo.lightingmaster"].id);
                }
            }
        }
    }
}
