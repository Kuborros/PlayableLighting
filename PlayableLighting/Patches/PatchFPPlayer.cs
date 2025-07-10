using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace PlayableLighting.Patches
{
    internal class PatchFPPlayer
    {

        public static FPPlayer player;
        public static PlayerShadow playerShadow;

        internal static float guardBuffer;
        internal static float jumpMultiplier;
        internal static float speedMultiplier;

        private static float ghostTimer;
        private static float gravAngleX;
        private static float gravAngleY;

        private static int flightAbilityUseCount = 0;
        private static float flightAbilityCooldown = 0f;

        private static int wingSmashComboStep = 0;
        private static int gravBootsComboStep = 0;
        private static float wingComboTimer = 0f;
        private static float gravComboTimer = 0f;

        private static float weaponCharge = 0f;

        private static readonly float energyRecoveryBaseSpeed = 0.4f;
        private static readonly float baseProjectileDamage = 2f;
        private static readonly float baseChargeProjectileDamage = 2f;
        private static readonly float maxChargeProjectileDamage = 10f;

        internal static readonly MethodInfo m_AirMoves = SymbolExtensions.GetMethodInfo(() => Action_Lighting_AirMoves());
        internal static readonly MethodInfo m_FuelPickup = SymbolExtensions.GetMethodInfo(() => Action_Lighting_FuelPickup());
        internal static readonly MethodInfo m_GroundMoves = SymbolExtensions.GetMethodInfo(() => Action_Lighting_GroundMoves());


        //Actions


        internal static void Action_Lighting_GravityBoots(bool reduceCost)
        {
            if (flightAbilityUseCount >= 4) return;

            player.genericTimer = 0f;
            ghostTimer = 0f;
            player.energyRecoverRate = 0f;

            if (player.onGround || player.onGrindRail)
            {
                player.Action_Jump();
            }
            flightAbilityUseCount++;
            player.jumpAbilityFlag = true;
            player.state = new FPObjectState(State_Lighting_GravityBoots_P1);
        }

        internal static void Action_Lighting_WingSmash(bool reduceCost)
        {
            if (flightAbilityUseCount >= 2) return;

            player.genericTimer = 0f;
            ghostTimer = 0f;
            player.energyRecoverRate = 0f;

            if (player.onGround || player.onGrindRail)
            {
                player.Action_Jump();
            }
            flightAbilityUseCount++;
            player.jumpAbilityFlag = true;
            player.state = new FPObjectState(State_Lighting_WingSmash_P1);
        }

        internal static void Action_Lighting_NormalShotFire()
        {
            float num = 8f;
            if (player.currentAnimation == "CrouchAttack_Loop")
            {
                num = -8f;
            }
            ProjectileBasic basicShot;
            if (player.direction == FPDirection.FACING_LEFT)
            {
                basicShot = (ProjectileBasic)FPStage.CreateStageObject(ProjectileBasic.classID, player.position.x - Mathf.Cos(0.017453292f * player.angle) * 32f + Mathf.Sin(0.017453292f * player.angle) * num, player.position.y + Mathf.Cos(0.017453292f * player.angle) * num - Mathf.Sin(0.017453292f * player.angle) * 32f);
                basicShot.velocity.x = Mathf.Cos(0.017453292f * player.angle) * -20f;
                basicShot.velocity.y = Mathf.Sin(0.017453292f * player.angle) * -20f;
            }
            else
            {
                basicShot = (ProjectileBasic)FPStage.CreateStageObject(ProjectileBasic.classID, player.position.x + Mathf.Cos(0.017453292f * player.angle) * 32f + Mathf.Sin(0.017453292f * player.angle) * num, player.position.y + Mathf.Cos(0.017453292f * player.angle) * num + Mathf.Sin(0.017453292f * player.angle) * 32f);
                basicShot.velocity.x = Mathf.Cos(0.017453292f * player.angle) * 20f;
                basicShot.velocity.y = Mathf.Sin(0.017453292f * player.angle) * 20f;
            }
            basicShot.animatorController = null; //LE IMPORTANTE TO FIXO
            basicShot.animator = basicShot.GetComponent<Animator>();
            basicShot.animator.runtimeAnimatorController = basicShot.animatorController;
            basicShot.attackPower = baseProjectileDamage * player.GetAttackModifier();
            basicShot.direction = player.direction;
            basicShot.angle = player.angle;
            basicShot.damageElementType = -1;
            basicShot.explodeType = FPExplodeType.WHITEBURST;
            basicShot.ignoreTerrain = false;
            basicShot.explodeTimer = 50f;
            basicShot.terminalVelocity = 0f;
            basicShot.gravityStrength = 0;
            basicShot.sfxExplode = null;
            basicShot.parentObject = player;
            basicShot.faction = player.faction;
            basicShot.timeBeforeCollisions = 0f;
        }

        internal static void Action_Lighting_ChargedShotFire(Vector3 chargeScale)
        {
            float num = 8f;
            if (player.currentAnimation == "CrouchAttack_Loop")
            {
                num = -8f;
            }
            ProjectileBasic chargeShot;
            if (player.direction == FPDirection.FACING_LEFT)
            {
                chargeShot = (ProjectileBasic)FPStage.CreateStageObject(ProjectileBasic.classID, player.position.x - Mathf.Cos(0.017453292f * player.angle) * 32f + Mathf.Sin(0.017453292f * player.angle) * num, player.position.y + Mathf.Cos(0.017453292f * player.angle) * num - Mathf.Sin(0.017453292f * player.angle) * 32f);
                chargeShot.velocity.x = Mathf.Cos(0.017453292f * player.angle) * -20f;
                chargeShot.velocity.y = Mathf.Sin(0.017453292f * player.angle) * -20f;
            }
            else
            {
                chargeShot = (ProjectileBasic)FPStage.CreateStageObject(ProjectileBasic.classID, player.position.x + Mathf.Cos(0.017453292f * player.angle) * 32f + Mathf.Sin(0.017453292f * player.angle) * num, player.position.y + Mathf.Cos(0.017453292f * player.angle) * num + Mathf.Sin(0.017453292f * player.angle) * 32f);
                chargeShot.velocity.x = Mathf.Cos(0.017453292f * player.angle) * 20f;
                chargeShot.velocity.y = Mathf.Sin(0.017453292f * player.angle) * 20f;
            }
            chargeShot.animatorController = null; //LE IMPORTANTE TO FIXO
            chargeShot.animator = chargeShot.GetComponent<Animator>();
            chargeShot.animator.runtimeAnimatorController = chargeShot.animatorController;
            chargeShot.attackPower = baseProjectileDamage * player.GetAttackModifier();
            chargeShot.direction = player.direction;
            chargeShot.angle = player.angle;
            chargeShot.scale = chargeScale;
            chargeShot.damageElementType = 3;
            chargeShot.explodeType = FPExplodeType.EXPLOSION;
            chargeShot.ignoreTerrain = false;
            chargeShot.explodeTimer = 50f;
            chargeShot.terminalVelocity = 0f;
            chargeShot.gravityStrength = 0;
            chargeShot.sfxExplode = null;
            chargeShot.parentObject = player;
            chargeShot.faction = player.faction;
            chargeShot.timeBeforeCollisions = 0f;
        }

        internal static void Action_Lighting_FuelPickup()
        {

        }

        internal static void Action_Lighting_AirMoves()
        {
            wingComboTimer -= FPStage.deltaTime;
            gravComboTimer -= FPStage.deltaTime;
            if (wingComboTimer < 0f)
                wingSmashComboStep = 0;
            if (gravComboTimer < 0f)
                gravBootsComboStep = 0;

            if (player.input.jumpPress && player.velocity.y < player.jumpStrength && !player.jumpAbilityFlag && player.targetWaterSurface == null)
            {
                player.jumpAbilityFlag = true;
                flightAbilityUseCount++;
                player.velocity.y = Mathf.Max(player.jumpStrength * jumpMultiplier, player.velocity.y);
                player.state = new FPObjectState(player.State_InAir);
                player.SetPlayerAnimation("DoubleJump", new float?(0f), new float?(0f), true, true);
                player.genericTimer = 0f;
                player.jumpReleaseFlag = true;
                player.Action_PlaySoundUninterruptable(player.sfxDoubleJump);
                WhiteBurst whiteBurst = (WhiteBurst)FPStage.CreateStageObject(WhiteBurst.classID, player.position.x, player.position.y - 24f);
                whiteBurst.scale.x = 1.5f;
                whiteBurst.scale.y = 0.3f;
            }
            else if ((player.guardTime <= 0f || player.cancellableGuard) && (player.input.guardPress || (guardBuffer > 0f && player.input.guardHold)))
            {
                player.SetPlayerAnimation("GuardAir", null, null, false, true);
                player.animator.SetSpeed(Mathf.Max(1f, 0.7f + Mathf.Abs(player.velocity.x * 0.05f)));
                FPAudio.PlaySfx(15);
                player.Action_Guard(0f, false);
                player.Action_ShadowGuard();
                GuardFlash guardFlash = (GuardFlash)FPStage.CreateStageObject(GuardFlash.classID, player.position.x, player.position.y);
                guardFlash.parentObject = player;
            }
            else if (player.input.attackPress)
            {
                player.SetPlayerAnimation("AirAttack", null, null, false, true);
                player.genericTimer = 0f;
                player.state = new FPObjectState(State_Lighting_AttackGun);
                player.idleTimer = -player.fightStanceTime;
                player.Action_StopSound();
            }
            //Gravity Boots combo trigger
            else if (gravBootsComboStep == 0 && player.input.down)
            {
                gravBootsComboStep = 1;
                gravComboTimer = 30f;
            }
            else if (gravBootsComboStep == 1 && player.input.up && gravComboTimer > 0f)
            {
                gravBootsComboStep = 2;
            }
            else if (gravBootsComboStep == 2 && player.input.jumpPress && gravComboTimer > 0f && player.state != new FPObjectState(State_Lighting_WingSmash_P2) && player.state != new FPObjectState(State_Lighting_GravityBoots_P2))
            {
                Action_Lighting_GravityBoots(true);
            }
            //Gravity Boots but without spamming buttons
            else if ((player.input.up || player.input.down) && player.input.specialHold && player.state != new FPObjectState(State_Lighting_WingSmash_P2) && player.state != new FPObjectState(State_Lighting_GravityBoots_P2))
            {
                Action_Lighting_GravityBoots(false);
            }
            //Wing Smash combo trigger
            else if (wingSmashComboStep == 0 && player.input.specialHold && (player.input.left || player.input.right))
            {
                wingSmashComboStep = 1;
                wingComboTimer = 20f;
            }
            else if (wingSmashComboStep == 1 && player.input.specialHold && !(player.input.left || player.input.right) && wingComboTimer > 0f)
            {
                wingSmashComboStep = 2;
            }
            else if (wingSmashComboStep == 2 && player.input.specialHold && (player.input.left || player.input.right) && wingComboTimer > 0f && player.state != new FPObjectState(State_Lighting_WingSmash_P2) && player.state != new FPObjectState(State_Lighting_GravityBoots_P2))
            {
                Action_Lighting_WingSmash(true);
            }
            //Wing Smash but without spamming buttons
            else if ((player.input.left || player.input.right) && player.input.specialHold && player.state != new FPObjectState(State_Lighting_WingSmash_P2) && player.state != new FPObjectState(State_Lighting_GravityBoots_P2))
            {
                Action_Lighting_WingSmash(false);
            }
        }

        internal static void Action_Lighting_GroundMoves()
        {
            wingComboTimer -= FPStage.deltaTime;
            gravComboTimer -= FPStage.deltaTime;
            if (wingComboTimer < 0f)
                wingSmashComboStep = 0;
            if (gravComboTimer < 0f)
                gravBootsComboStep = 0;

            //Guard
            if ((player.guardTime <= 0f || player.cancellableGuard) && (player.input.guardPress || (guardBuffer > 0f && player.input.guardHold)))
            {
                if (Mathf.Abs(player.groundVel) < 3f)
                {
                    player.SetPlayerAnimation("Guard", null, null, false, true);
                    player.idleTimer = Mathf.Min(player.idleTimer, 0f);
                    player.groundVel = 0f;
                }
                else
                {
                    player.SetPlayerAnimation("GuardRun", null, null, false, true);
                    player.animator.SetSpeed(Mathf.Max(1f, 0.7f + Mathf.Abs(player.velocity.x * 0.05f)));
                }
                FPAudio.PlaySfx(15);
                player.Action_Guard(0f, false);
                player.Action_ShadowGuard();
                GuardFlash guardFlash = (GuardFlash)FPStage.CreateStageObject(GuardFlash.classID, player.position.x, player.position.y);
                guardFlash.parentObject = player;
            }
            else if (player.input.attackPress)
            {
                if (player.state == new FPObjectState(player.State_Crouching) && player.animator.GetCurrentAnimatorStateInfo(0).IsName("Crouching_Loop"))
                {
                    player.SetPlayerAnimation("CrouchAttack", null, null, false, true);
                    player.genericTimer = 0f;
                    player.state = new FPObjectState(State_Lighting_AttackGun);
                    player.idleTimer = -player.fightStanceTime;
                    player.Action_StopSound();
                }
                else if (player.state != new FPObjectState(State_Lighting_AttackGun))
                {
                    player.SetPlayerAnimation("AttackGround", null, null, false, true);
                    player.genericTimer = 0f;
                    player.state = new FPObjectState(State_Lighting_AttackGun);
                    player.idleTimer = -player.fightStanceTime;
                    player.Action_StopSound();
                }
            }
            //Gravity Boots combo trigger
            else if (gravBootsComboStep == 0 && player.state == new FPObjectState(player.State_Crouching))
            {
                gravBootsComboStep = 1;
                gravComboTimer = 30f;
            }
            else if (gravBootsComboStep == 1 && player.input.up && gravComboTimer > 0f)
            {
                gravBootsComboStep = 2;
            }
            else if (gravBootsComboStep == 2 && player.input.jumpPress && gravComboTimer > 0f && player.state != new FPObjectState(State_Lighting_WingSmash_P2) && player.state != new FPObjectState(State_Lighting_GravityBoots_P2))
            {
                Action_Lighting_GravityBoots(true);
            }
            //Gravity Boots but without spamming buttons
            else if (player.input.up && player.input.specialHold) 
            {
                Action_Lighting_GravityBoots(false);
            }
        }



        //States

        internal static void State_Lighting_GravityBoots_P1()
        {
            player.SetPlayerAnimation("Jumping");

            if (player.input.left)
                gravAngleX = -5f;
            else if (player.input.right)
                gravAngleX = 5f;
            else
                gravAngleX = 0f;

            if (player.input.down)
                gravAngleY = -10f;
            else if (player.input.up)
                gravAngleY = 10f;

            if (player.genericTimer <= 10f)
            {
                player.genericTimer += FPStage.deltaTime;
                player.velocity.x = 0f;
                player.angle = 0f;
                if (player.velocity.x > 0f)
                {
                    player.velocity.x = player.velocity.x - 0.125f * FPStage.deltaTime;
                }
                else if (player.velocity.x < 0f)
                {
                    player.velocity.x = player.velocity.x + 0.125f * FPStage.deltaTime;
                }
                if (player.velocity.y > 0f)
                {
                    player.velocity.y = player.velocity.y - 0.125f * FPStage.deltaTime;
                }
                else if (player.velocity.y < 0f)
                {
                    player.velocity.y = player.velocity.y + 0.125f * FPStage.deltaTime;
                }
                if (player.input.left)
                {
                    player.direction = FPDirection.FACING_LEFT;
                }
                else if (player.input.right)
                {
                    player.direction = FPDirection.FACING_RIGHT;
                }
            }
            else
            {
                player.genericTimer = 0f;
                player.state = new FPObjectState(State_Lighting_GravityBoots_P2);
            }
        }

        internal static void State_Lighting_GravityBoots_P2()
        {
            player.genericTimer += FPStage.deltaTime;
            player.energyRecoverRate = 0f;
            player.SetPlayerAnimation("Jumping");
            player.superArmor = true;
            player.invincibilityTime = Mathf.Max(player.invincibilityTime, 50f);
            ghostTimer += FPStage.deltaTime;
            player.attackStats = new FPObjectState(AttackStats_GravityBoots);

            if (player.colliderRoof == null && player.colliderWall == null) player.velocity.x = gravAngleX;
            if (player.colliderRoof == null && player.colliderWall == null) player.velocity.y = gravAngleY;

            if (ghostTimer >= 0.5f)
            {
                player.Ghost();
                ghostTimer = 0f;
            }
            player.Process360Movement();

            //Hit ceiling or wall when still holding special
            if ((player.colliderRoof != null || player.colliderWall != null) && (player.input.specialHold || player.input.jumpHold) && player.energy >= 0f)
            {
                player.velocity.x = 0f;
                player.velocity.y = 0f;
                if (player.colliderRoof != null)
                {
                    player.angle = player.ceilingAngle;
                }
                else
                {
                    player.angle = player.groundAngle;
                }
                player.energy -= 1f * FPStage.deltaTime;
            }
            //'Ground' landing + Timeouts
            else if (player.onGround || player.onGrindRail || (!player.input.specialHold && !player.input.jumpHold) || player.genericTimer >= 50f || player.energy <= 0f)
            {
                player.energyRecoverRate = energyRecoveryBaseSpeed;
                player.hbAttack.enabled = false;
                player.superArmor = false;
                if (player.onGround)
                {
                    player.SetPlayerAnimation("Crouching", null, null, false, true);
                    player.state = new FPObjectState(player.State_Ground);
                }
                else
                {
                    player.SetPlayerAnimation("Jumping", 0.25f, 0.25f, false, true);
                    player.state = new FPObjectState(player.State_InAir);
                }
                player.attackStats = new FPObjectState(AttackStats_Idle);
            }
            else
            {
                player.energy -= 1.5f * FPStage.deltaTime;
            }
        }

        internal static void State_Lighting_WingSmash_P1()
        {
            player.SetPlayerAnimation("Jumping");

            if (player.genericTimer <= 5f)
            {
                player.genericTimer += FPStage.deltaTime;
                player.velocity.x = 0f;
                player.angle = 0f;
                if (player.velocity.x > 0f)
                {
                    player.velocity.x = player.velocity.x - 0.125f * FPStage.deltaTime;
                }
                else if (player.velocity.x < 0f)
                {
                    player.velocity.x = player.velocity.x + 0.125f * FPStage.deltaTime;
                }
                if (player.velocity.y > 0f)
                {
                    player.velocity.y = player.velocity.y - 0.125f * FPStage.deltaTime;
                }
                else if (player.velocity.y < 0f)
                {
                    player.velocity.y = player.velocity.y + 0.125f * FPStage.deltaTime;
                }
                if (player.input.left)
                {
                    player.direction = FPDirection.FACING_LEFT;
                }
                else if (player.input.right)
                {
                    player.direction = FPDirection.FACING_RIGHT;
                }
            }
            else
            {
                player.genericTimer = 0f;
                player.state = new FPObjectState(State_Lighting_WingSmash_P2);
            }
        }

        internal static void State_Lighting_WingSmash_P2()
        {
            player.genericTimer += FPStage.deltaTime;
            player.energyRecoverRate = 0f;
            player.SetPlayerAnimation("Jumping");
            player.superArmor = true;
            player.invincibilityTime = Mathf.Max(player.invincibilityTime, 50f);
            ghostTimer += FPStage.deltaTime;
            player.attackStats = new FPObjectState(AttackStats_WingSmash);

            if (Mathf.Repeat(player.genericTimer, 4f) < 1f)
            {
                FPStage.CreateStageObject(Sparkle.classID, player.position.x + UnityEngine.Random.Range(-24f, 24f), player.position.y + UnityEngine.Random.Range(-24f, 24f));
            }


            if (player.direction == FPDirection.FACING_LEFT)
            {
                player.velocity.x = Mathf.Min(Mathf.Min(player.velocity.x, 0f) * 0.5f - 5f, player.velocity.x);
                player.velocity.y = 0f;
            }
            else
            {
                player.velocity.x = Mathf.Max(Mathf.Max(player.velocity.x, 0f) * 0.5f + 5f, player.velocity.x);
                player.velocity.y = 0f;
            }

            if (player.input.up)
            {
                player.velocity.y = player.velocity.y + 5f * FPStage.deltaTime;
            }
            else if (player.input.down)
            {
                player.velocity.y = player.velocity.y - 5f * FPStage.deltaTime;
            }

            player.energy -= 1f * FPStage.deltaTime;
            player.Process360Movement();

            if (player.onGround || player.onGrindRail || !(player.input.left || player.input.right) || player.colliderWall != null || player.genericTimer >= 300f || player.energy <= 0f)
            {
                player.energyRecoverRate = energyRecoveryBaseSpeed;
                player.hbAttack.enabled = false;
                player.superArmor = false;
                if (player.onGround)
                {
                    player.SetPlayerAnimation("Landing", null, null, false, true);
                    player.state = new FPObjectState(player.State_Ground);
                }
                else
                {
                    player.SetPlayerAnimation("Jumping", 0.25f, 0.25f, false, true);
                    player.state = new FPObjectState(player.State_InAir);
                }
                player.attackStats = new FPObjectState(AttackStats_Idle);
            }
        }

        internal static void State_Lighting_AttackGun()
        {
            SetAnimSpeedToVelocity(player);
            player.genericTimer += FPStage.deltaTime;
            if (player.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.35f && player.velocity.y < 0f)
            {
                player.velocity.y = Mathf.Min(player.velocity.y + 0.4f, -1f);
            }





            if (player.onGround)
            {
                if (player.input.jumpPress)
                {
                    player.genericTimer = 0f;
                    player.Action_SoftJump();
                }
                else if (player.onGrindRail)
                {
                    player.PseudoGrindRail();
                }
                else
                {
                    ApplyGroundForces(player,false);
                    player.angle = player.groundAngle;
                }
                player.jumpAbilityFlag = false;
            }
            else
            {
                ApplyAirForces(player,false);
                ApplyGravityForce(player);
                RotatePlayerUpright(player);
                if (!player.input.jumpHold && player.jumpReleaseFlag)
                {
                    player.jumpReleaseFlag = false;
                    if (player.velocity.y > player.jumpRelease)
                    {
                        player.velocity.y = player.jumpRelease;
                    }
                }
                if (player.targetWaterSurface != null)
                {
                    ApplyWaterForces(player);
                    player.velocity.y = player.velocity.y + 0.3f * FPStage.deltaTime;
                    if (player.velocity.y < -4.5f)
                    {
                        player.velocity.y = -4.5f;
                    }
                }
            }
        }

        //AttackStats

        private static void AttackStats_WingSmash()
        {
            player.attackPower = 4f;
            player.attackHitstun = 4f;
            player.attackEnemyInvTime = 6f;
            player.attackKnockback.x = Mathf.Max(Mathf.Abs(player.prevVelocity.x * 1.5f), 4.5f);
            if (player.direction == FPDirection.FACING_LEFT)
            {
                player.attackKnockback.x = -player.attackKnockback.x;
            }
            player.attackKnockback.y = player.prevVelocity.y * 0.5f;
            player.attackSfx = 6;
            player.attackPower *= player.GetAttackModifier();
        }

        private static void AttackStats_GravityBoots()
        {
            player.attackPower = 4f;
            player.attackHitstun = 4f;
            player.attackEnemyInvTime = 6f;
            player.attackKnockback.x = player.prevVelocity.x * 0.5f;
            if (player.direction == FPDirection.FACING_LEFT)
            {
                player.attackKnockback.x = -player.attackKnockback.x;
            }
            player.attackKnockback.y = Mathf.Max(Mathf.Abs(player.prevVelocity.y * 1.5f), 4.5f);
            player.attackSfx = 6;
            player.attackPower *= player.GetAttackModifier();
        }

        private static void AttackStats_Idle()
        {
            player.attackPower = 2f;
            player.attackHitstun = 4f;
            player.attackEnemyInvTime = 5f / player.animator.speed;
            player.attackKnockback.x = 0f;
            player.attackKnockback.y = 0f;
            player.attackSfx = 5;
            player.attackPower *= player.GetAttackModifier();
        }

        //Postfixes
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPPlayer), "Update", MethodType.Normal)]
        static void PatchPlayerUpdate(FPPlayer __instance, float ___speedMultiplier, float ___guardBuffer, float ___jumpMultiplier)
        {
            //Value Yeeter 10000
            player = __instance;
            guardBuffer = ___guardBuffer;
            jumpMultiplier = ___jumpMultiplier;
            speedMultiplier = ___speedMultiplier;

            if (player.onGround || player.onGrindRail)
            {
                flightAbilityUseCount = 0;
            }
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
        [HarmonyPatch(typeof(FPPlayer), "ApplyAirForces", MethodType.Normal)]
        public static void ApplyAirForces(FPPlayer instance, bool ignoreDirectionalInput)
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

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(FPPlayer), "SetAnimSpeedToVelocity", MethodType.Normal)]
        public static void SetAnimSpeedToVelocity(FPPlayer instance)
        {
            // Replaced at runtime with reverse patch
            throw new NotImplementedException("Method failed to reverse patch!");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(FPPlayer), "RotatePlayerUpright", MethodType.Normal)]
        public static void RotatePlayerUpright(FPPlayer instance)
        {
            // Replaced at runtime with reverse patch
            throw new NotImplementedException("Method failed to reverse patch!");
        }
    }
}
