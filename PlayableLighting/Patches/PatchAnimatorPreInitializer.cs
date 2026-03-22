using HarmonyLib;
using UnityEngine;

namespace PlayableLightning.Patches
{
    internal class PatchAnimatorPreInitializer
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(AnimatorPreInitializer), "Start", MethodType.Normal)]
        static void PatchAnimatorPreInit(ref AnimatorInitializationParams[] ___animatorsToInit)
        {
            AnimatorInitializationParams lightningInit = new AnimatorInitializationParams();
            lightningInit.animator = PlayableLightning.dataBundle.LoadAsset<Animator>("Lightning Animator Player");

            AnimatorInitializationClipParams[] clipsToInit = {

                new AnimatorInitializationClipParams("Idle"),
                new AnimatorInitializationClipParams("Running"),
                new AnimatorInitializationClipParams("Rolling"),
                new AnimatorInitializationClipParams("Jumping"),
                new AnimatorInitializationClipParams("Shoot"),
                new AnimatorInitializationClipParams("AirShoot"),
                new AnimatorInitializationClipParams("Pose1")

            };
            lightningInit.animationClipsToPlay = clipsToInit;

            ___animatorsToInit = ___animatorsToInit.AddToArray(lightningInit);
        }
    }
}
