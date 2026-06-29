using FP2Lib.Player;
using HarmonyLib;
using PlayableLightning.Objects;
using UnityEngine;

namespace PlayableLightning.Patches
{
    internal class PatchArenaSpawner
    {

        internal static GameObject lightningBoss = null;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ArenaSpawner), "Start", MethodType.Normal)]
        static void PatchArenaSpawnerStart(ArenaSpawner __instance)
        {
            lightningBoss = null;
            if (FPSaveManager.currentArenaChallenge != PlayableLightning.bossLightningID) return;

            if (FPStage.stageNameString == "Training" && lightningBoss == null)
            {
                lightningBoss = GameObject.Instantiate(PlayerHandler.PlayableChars["com.kuborro.lightning"].playerBoss.gameObject);
                lightningBoss.SetActive(false);
                lightningBoss.name = "Boss Lightning";

                if (lightningBoss != null && FPSaveManager.currentArenaChallenge == PlayableLightning.bossLightningID)
                {
                    __instance.syncChallengeID = false;

                    ArenaRoundSpawnList lightningList = new()
                    {
                        bossBattle = true,
                        waitForObjectDestruction = false,
                        objectList = new FPBaseObject[] { lightningBoss.GetComponent<PlayerBossLightning>() }
                    };
                    ArenaSpawnList spawnList = new()
                    {
                        name = "LightningBoss",
                        challengeID = PlayableLightning.bossLightningID,
                        rewardCrystals = 200,
                        rewardTimeCapsule = false,
                        timeCapsuleID = -1,
                        spawnAllies = false,
                        alliesAreHostile = false,
                        disableCorePickups = false,
                        spawnAtStart = new FPBaseObject[] { lightningBoss.GetComponent<PlayerBossLightning>() },
                        roundObjectList = new ArenaRoundSpawnList[] { lightningList },
                        spawnDelay = new float[] { 0 },
                        endCutscene = "",
                        victoryDelayOffset = 0
                    };

                    __instance.challenges = __instance.challenges.AddToArray(spawnList);
                    __instance.currentChallenge = PlayableLightning.bossLightningID;
                }
            }
        }
    }
}
