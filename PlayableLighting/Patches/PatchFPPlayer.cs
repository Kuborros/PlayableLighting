using HarmonyLib;
using System;
using System.Reflection;

namespace PlayableLighting.Patches
{
    internal class PatchFPPlayer
    {

        public static FPPlayer player;
        public static PlayerShadow playerShadow;

        internal static readonly MethodInfo m_AirMoves = SymbolExtensions.GetMethodInfo(() => Action_Lighting_AirMoves());
        internal static readonly MethodInfo m_FuelPickup = SymbolExtensions.GetMethodInfo(() => Action_Lighting_FuelPickup());
        internal static readonly MethodInfo m_GroundMoves = SymbolExtensions.GetMethodInfo(() => Action_Lighting_GroundMoves());


        //Actions






        internal static void Action_Lighting_FuelPickup()
        {

        }

        internal static void Action_Lighting_AirMoves()
        {

        }

        internal static void Action_Lighting_GroundMoves()
        {

        }


        //Postfixes
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPPlayer), "Update", MethodType.Normal)]
        static void PatchPlayerUpdate(FPPlayer __instance, float ___speedMultiplier)
        {
            player = __instance;
        }

        //Reverse Patches
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(FPPlayer), "ApplyGroundForces", MethodType.Normal)]
        public static void ApplyGroundForces(FPPlayer instance, bool ignoreDirectionalInput)
        {
            // Replaced at runtime with reverse patch
            throw new NotImplementedException("Method failed to reverse patch!");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(FPPlayer), "ApplyWaterForces", MethodType.Normal)]
        public static void ApplyWaterForces(FPPlayer instance)
        {
            // Replaced at runtime with reverse patch
            throw new NotImplementedException("Method failed to reverse patch!");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(FPPlayer), "ApplyGravityForce", MethodType.Normal)]
        public static void ApplyGravityForce(FPPlayer instance)
        {
            // Replaced at runtime with reverse patch
            throw new NotImplementedException("Method failed to reverse patch!");
        }
    }
}
