using HarmonyLib;

namespace PlayableLighting.Patches
{
    internal class PatchFPHudMaster
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPHudMaster), "GuideUpdate", MethodType.Normal)]
        static void PatchGuideUpdate(FPPlayer player, FPHudMaster __instance)
        {
            string text = "Jump";
            string text2 = "Single Shot";
            string text3 = "<c=energy>Wing Special</c>";
            string text4 = "Guard";

            if (player == null || player.characterID != PlayableLighting.currentLightingID)
            {
                return;
            }
            if (player.IsKOd(false))
            {
                text = "-";
                text2 = "-";
                text3 = "-";
                text4 = "-";
            }

            //Mid-air
            if (!player.onGround && player.state != new FPObjectState(player.State_LadderClimb))
            {
                text = "Double Jump";
                if (player.state != new FPObjectState(player.State_Ball) && player.state != new FPObjectState(player.State_Ball_Physics) && player.state != new FPObjectState(player.State_Ball_Vulnerable))
                {
                    if (player.input.left || player.input.right)
                    {
                        text3 = "<c=energy>Wing Smash</c>";
                    }
                    else if (player.input.up || player.input.down)
                    {
                        text3 = "<c=energy>Gravity Boots</c>";
                    }
                    else
                        text3 = "<c=energy>-</c>";
                }
                if (!player.input.attackHold)
                {
                    text2 = "Single Shot";
                }
                else
                {
                    text2 = "<c=energy>(Hold) Charge Shot</c>";
                }

            }

            //On the ground, excluding funky states
            if (player.state != new FPObjectState(player.State_LadderClimb) && player.state != new FPObjectState(player.State_Ball) && player.state != new FPObjectState(player.State_Ball_Physics) && player.state != new FPObjectState(player.State_Ball_Vulnerable))
            {
                if (player.input.down)
                {
                    text2 = "Crouch shot";
                }
                else
                {
                    if (!player.input.attackHold)
                    {
                        text2 = "Single Shot";
                    }
                    else
                    {
                        text2 = "<c=energy>(Hold) Charge Shot</c>";
                    }
                }
                if (player.input.up && !player.input.down)
                {
                    text3 = "<c=energy>Gravity Boots</c>";
                }
                if (player.onGround)
                {
                    text3 = "<c=energy>-</c>";
                }
            }

            if (player.displayMoveJump != string.Empty)
            {
                text = player.displayMoveJump;
            }
            if (player.displayMoveAttack != string.Empty)
            {
                text2 = player.displayMoveAttack;
            }
            if (player.displayMoveSpecial != string.Empty)
            {
                text3 = player.displayMoveSpecial;
            }
            if (player.displayMoveGuard != string.Empty)
            {
                text4 = player.displayMoveGuard;
            }
            __instance.hudGuide.text = string.Concat(new string[]
            {
            text,
            "\n",
            text2,
            "\n",
            text3,
            "\n",
            text4,
            "\n "
            });
        }
    }
}
