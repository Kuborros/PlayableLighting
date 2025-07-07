using FP2Lib.Badge;
using HarmonyLib;

namespace PlayableLighting.Patches
{
    internal class PatchFPSaveManager
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPSaveManager), "GameClearBadgeCheck", MethodType.Normal)]
        static void PatchFPSaveManagerCheckEnd()
        {
            //Lighting game complete badge
            if (FPSaveManager.character == PlayableLighting.currentLightingID)
                BadgeHandler.UnlockBadge("kubo.lightingcomplete");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPSaveManager), "BadgeCheck", MethodType.Normal)]
        static void PatchFPSaveManagerBadgeCheck(int badgeID, int variable = 0)
        {
            //Lighting par time badges.
            if (FPSaveManager.character == PlayableLighting.currentLightingID)
            {
                //Par time one stage
                if (badgeID == BadgeHandler.GetBadgeDataByUid("kubo.lightingrunner").id)
                {
                    int stagetime = (FPStage.currentStage.milliSeconds + FPStage.currentStage.seconds * 100) + FPStage.currentStage.minutes * 6000;
                    if (stagetime > 0 && stagetime < FPSaveManager.GetStageParTime(FPStage.currentStage.stageID))
                    {
                        BadgeHandler.UnlockBadge("kubo.lightingrunner");
                    }
                }
                //Half par time.
                if (badgeID == BadgeHandler.GetBadgeDataByUid("kubo.lightingspeedrunner").id)
                {
                    int stagetime = (FPStage.currentStage.milliSeconds + FPStage.currentStage.seconds * 100) + FPStage.currentStage.minutes * 6000;
                    if (stagetime > 0 && stagetime < (FPSaveManager.GetStageParTime(FPStage.currentStage.stageID) / 2))
                    {
                        BadgeHandler.UnlockBadge("kubo.lightingspeedrunner");
                    }
                }
                //All par times
                if (badgeID == BadgeHandler.GetBadgeDataByUid("kubo.lightingmaster").id)
                {
                    int parTimes = 0;
                    if (FPSaveManager.timeRecord[30] > 0 || FPSaveManager.storyFlag[47] > 0)
                    {
                        parTimes--;
                    }
                    int stageNum = 1;
                    while (stageNum < FPSaveManager.timeRecord.Length && stageNum <= 32)
                    {
                        if (stageNum != 31)
                        {
                            if (FPSaveManager.timeRecord[stageNum] > 0 && FPSaveManager.timeRecord[stageNum] < FPSaveManager.GetStageParTime(stageNum))
                            {
                                parTimes++;
                            }
                        }
                        stageNum++;
                    }
                    if (parTimes >= 30)
                    {
                        BadgeHandler.UnlockBadge("kubo.lightingmaster");
                    }
                }
            }
        }
    }
}
