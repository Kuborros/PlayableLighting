using HarmonyLib;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace PlayableLighting.Patches
{
    internal class PatchFPPlayer
    {

        public static FPPlayer player;
        public static PlayerShadow playerShadow;

        public static AudioClip basicShotSfx;
        public static AudioClip chargeShotSfx;
        public static AudioClip chargeSfx;

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
        private static float shotDelay = 10f;
        private static float chargeShotDelay = 50f;

        private static readonly float energyRecoveryBaseSpeed = 0.4f;
        private static readonly float baseProjectileDamage = 2f;
        private static readonly float baseChargeProjectileDamage = 2f;
        private static readonly float maxChargeProjectileDamage = 10f;

        private static RuntimeAnimatorController baseProjectile;
        private static RuntimeAnimatorController partChargeProjectile;
        private static RuntimeAnimatorController fullChargeProjectile;
        private static RuntimeAnimatorController uberChargeProjectile;

        internal static readonly MethodInfo m_AirMoves = SymbolExtensions.GetMethodInfo(() => Action_Lighting_AirMoves());
        internal static readonly MethodInfo m_FuelPickup = SymbolExtensions.GetMethodInfo(() => Action_Lighting_FuelPickup());
        internal static readonly MethodInfo m_GroundMoves = SymbolExtensions.GetMethodInfo(() => Action_Lighting_GroundMoves());

        private static readonly FPHitBox bulletHitbox = new FPHitBox { left = -8, right = 8, top = 4, bottom = -4, enabled = true };
        private static readonly FPHitBox chargeBulletHitbox = new FPHitBox { left = -10, right = 10, top = 10, bottom = -10, enabled = true };
        private static readonly FPHitBox fullChargeBulletHitbox = new FPHitBox { left = -30, right = 30, top = 14, bottom = -14, enabled = true };
        private static readonly FPHitBox uberChargeBulletHitbox = new FPHitBox { left = -35, right = 35, top = 25, bottom = -25, enabled = true };


        //Actions


        internal static void Action_Lighting_GravityBoots(bool reduceCost)
        {
            if (flightAbilityUseCount >= 4 || flightAbilityCooldown > 0f) return;

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
            if (flightAbilityUseCount >= 2 || flightAbilityCooldown > 0f) return;

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
            FPAudio.PlaySfx(basicShotSfx);
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
            basicShot.animatorController = baseProjectile;
            basicShot.animator = basicShot.GetComponent<Animator>();
            basicShot.animator.runtimeAnimatorController = basicShot.animatorController;
            basicShot.attackPower = baseProjectileDamage * player.GetAttackModifier();
            basicShot.direction = player.direction;
            //Very dumb hack
            if (player.direction == FPDirection.FACING_LEFT)
                basicShot.direction = FPDirection.FACING_RIGHT;
            else
                basicShot.direction = FPDirection.FACING_LEFT;
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
            basicShot.hbTouch = bulletHitbox;
            basicShot.halfHeight = 4;
            basicShot.halfWidth = 8;

            if (player.IsPowerupActive(FPPowerup.SHADOW_GUARD))
            {

            }

        }

        internal static void Action_Lighting_ChargedShotFire(Vector3 chargeScale)
        {
            float num = 8f;
            if (player.currentAnimation == "CrouchAttack_Loop")
            {
                num = -8f;
            }
            FPAudio.PlaySfx(chargeShotSfx);
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


            if (weaponCharge > 90f)
            {
                chargeShot.animatorController = fullChargeProjectile;
                chargeShot.hbTouch = fullChargeBulletHitbox;
                chargeShot.halfHeight = 14;
                chargeShot.halfWidth = 30;
            }
            else
            {
                chargeShot.animatorController = partChargeProjectile;
                chargeShot.hbTouch = chargeBulletHitbox;
                chargeShot.halfHeight = 10;
                chargeShot.halfWidth = 10;
            }
            chargeShot.animator = chargeShot.GetComponent<Animator>();
            chargeShot.animator.runtimeAnimatorController = chargeShot.animatorController;
            chargeShot.attackPower = (baseChargeProjectileDamage + Math.Min(maxChargeProjectileDamage, weaponCharge/10)) * player.GetAttackModifier();
            //Idiotic hack
            if (player.direction == FPDirection.FACING_LEFT)
                chargeShot.direction = FPDirection.FACING_RIGHT;
            else
                chargeShot.direction = FPDirection.FACING_LEFT;
            chargeShot.angle = player.angle;
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

            if (player.hasSpecialItem && weaponCharge > 90f)
            {
                chargeShot.animatorController = uberChargeProjectile;
                chargeShot.hbTouch = uberChargeBulletHitbox;
                chargeShot.halfHeight = 10;
                chargeShot.halfWidth = 10;

                chargeShot.attackPower *= 2;
            }

            weaponCharge = 0f;

        }

        internal static void Action_Lighting_FuelPickup()
        {
            player.hasSpecialItem = true;
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
            else if (player.input.attackPress && shotDelay < 0f)
            {
                player.SetPlayerAnimation("AirAttack", null, null, false, true);
                player.genericTimer = 0f;
                shotDelay = 10f;
                chargeShotDelay = 40f;
                Action_Lighting_NormalShotFire();
                player.idleTimer = -player.fightStanceTime;
                player.Action_StopSound();
            }
            else if (player.input.attackHold && shotDelay < 0f && chargeShotDelay < 0f && player.energy > 20f)
            {
                player.SetPlayerAnimation("AirAttack", null, null, false, true);
                shotDelay = 10f;
                chargeShotDelay = 40f;
                player.state = new FPObjectState(State_Lighting_AttackHold);
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
            else if (player.input.attackPress && shotDelay < 0f)
            {
                if (player.state == new FPObjectState(player.State_Crouching) && player.animator.GetCurrentAnimatorStateInfo(0).IsName("Crouching_Loop"))
                {
                    player.SetPlayerAnimation("CrouchAttack", null, null, false, true);
                    player.genericTimer = 0f;
                    shotDelay = 10f;
                    chargeShotDelay = 40f;
                    Action_Lighting_NormalShotFire();
                    player.idleTimer = -player.fightStanceTime;
                    player.Action_StopSound();
                }
                else if (player.state != new FPObjectState(State_Lighting_AttackHold))
                {
                    player.SetPlayerAnimation("AttackGround", null, null, false, true);
                    player.genericTimer = 0f;
                    shotDelay = 5f;
                    chargeShotDelay = 40f;
                    Action_Lighting_NormalShotFire();
                    player.idleTimer = -player.fightStanceTime;
                    player.Action_StopSound();
                }
            }
            else if (player.input.attackHold && chargeShotDelay < 0f && shotDelay < 0f && player.energy > 20f)
            {
                if (player.state == new FPObjectState(player.State_Crouching) && player.animator.GetCurrentAnimatorStateInfo(0).IsName("Crouching_Loop"))
                {
                    player.SetPlayerAnimation("CrouchAttack", null, null, false, true);
                    player.genericTimer = 0f;
                    shotDelay = 10f;
                    chargeShotDelay = 40f;
                    player.state = new FPObjectState(State_Lighting_AttackHold);
                    player.idleTimer = -player.fightStanceTime;
                    player.Action_StopSound();
                }
                else if (player.state != new FPObjectState(State_Lighting_AttackHold))
                {
                    player.SetPlayerAnimation("AttackGround", null, null, false, true);
                    player.genericTimer = 0f;
                    shotDelay = 5f;
                    chargeShotDelay = 50f;
                    player.state = new FPObjectState(State_Lighting_AttackHold);
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
            flightAbilityCooldown = 20f;
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
                player.energy -= 2f * FPStage.deltaTime;
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
            flightAbilityCooldown = 20f;
            player.attackStats = new FPObjectState(AttackStats_WingSmash);

            if (Mathf.Repeat(player.genericTimer, 4f) < 1f)
            {
                FPStage.CreateStageObject(Sparkle.classID, player.position.x + UnityEngine.Random.Range(-24f, 24f), player.position.y + UnityEngine.Random.Range(-24f, 24f));
            }


            if (player.direction == FPDirection.FACING_LEFT)
            {
                player.velocity.x = Mathf.Min(Mathf.Min(player.velocity.x, 0f) * 0.5f - 4f, player.velocity.x);
                player.velocity.y = 0f;
            }
            else
            {
                player.velocity.x = Mathf.Max(Mathf.Max(player.velocity.x, 0f) * 0.5f + 4f, player.velocity.x);
                player.velocity.y = 0f;
            }

            if (player.input.up)
            {
                player.velocity.y = player.velocity.y + 7.5f * FPStage.deltaTime;
                if (player.direction == FPDirection.FACING_RIGHT)
                    player.angle = 10f;
                else
                    player.angle = -10f;
            }
            else if (player.input.down)
            {
                player.velocity.y = player.velocity.y - 7.5f * FPStage.deltaTime;
                if (player.direction == FPDirection.FACING_RIGHT)
                    player.angle = -10f;
                else
                    player.angle = 10f;
            }

            player.energy -= 1.5f * FPStage.deltaTime;
            player.Process360Movement();

            if (player.onGround || player.onGrindRail || player.colliderWall != null || (!player.input.specialHold && !player.input.jumpHold) || player.genericTimer >= 100f || player.energy <= 0f)
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

        internal static void State_Lighting_AttackHold()
        {
            if (player.input.attackHold && player.energy > 0f)
            {
                SetAnimSpeedToVelocity(player);
                PlaySFXLooping(chargeSfx, 1f);
                player.genericTimer += FPStage.deltaTime;
                player.energyRecoverRate = 0f;
                player.blueFlashTimer = 5f;
                weaponCharge += 1f * FPStage.deltaTime;
                player.energy -= 1f * FPStage.deltaTime;

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
                        ApplyGroundForces(player, false);
                        player.angle = player.groundAngle;
                    }
                    player.jumpAbilityFlag = false;
                }
                else
                {
                    ApplyAirForces(player, false);
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
            else
            {
                StopSFXLooping();
                player.energyRecoverRate = energyRecoveryBaseSpeed;
                if (weaponCharge > 0f)
                {
                    Action_Lighting_ChargedShotFire(new Vector3(1,1,1));
                }
                if (player.onGround)
                {
                    player.state = new FPObjectState(player.State_Ground);
                }
                else
                {
                    player.SetPlayerAnimation("Jumping", 0.25f, 0.25f, false, true);
                    player.state = new FPObjectState(player.State_InAir);
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

        //Others

        private static void PlaySFXLooping(AudioClip clip, float volume)
        {
            //Channel 4 is used for Carol's bike, so we can repurpose it here.
            if (clip != null)
            {
                if (player.audioChannel[4].clip != clip)
                {
                    player.audioChannel[4].clip = clip;
                    player.audioChannel[4].Play();
                }
                player.audioChannel[4].volume = volume;
            }
        }

        private static void StopSFXLooping()
        {
            player.audioChannel[4].Stop();
            player.audioChannel[4].clip = null;
        }

        //Postfixes
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPPlayer), "Update", MethodType.Normal)]
        static void PatchPlayerUpdate(FPPlayer __instance, float ___speedMultiplier, float ___guardBuffer, float ___jumpMultiplier)
        {
            if (FPSaveManager.character == PlayableLighting.currentLightingID)
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

                flightAbilityCooldown -= FPStage.deltaTime;
                shotDelay -= FPStage.deltaTime;
                chargeShotDelay -= FPStage.deltaTime;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPPlayer), "Start", MethodType.Normal)]
        static void PatchPlayerStart(FPPlayer __instance)
        {
            if (FPSaveManager.character == PlayableLighting.currentLightingID)
            {
                player = __instance;
                //Append 2 extra spare audio channels
                //Channel 4 - Looping SFX
                //Channel 5 - Things normal game logic should not mess with
                for (int i = 4; i < 6; i++)
                {
                    GameObject gameObject = new GameObject("PlayerAudioSource");
                    gameObject.transform.parent = player.gameObject.transform;
                    player.audioChannel = player.audioChannel.AddToArray(gameObject.AddComponent<AudioSource>());
                    player.audioChannel[i].volume = FPSaveManager.volumeSfx;
                    player.audioChannel[i].playOnAwake = false;
                }

                //Load projectile animations
                baseProjectile = PlayableLighting.dataBundle.LoadAsset<RuntimeAnimatorController>("BaseProjectile");
                partChargeProjectile = PlayableLighting.dataBundle.LoadAsset<RuntimeAnimatorController>("PartChargeProjectile");
                fullChargeProjectile = PlayableLighting.dataBundle.LoadAsset<RuntimeAnimatorController>("FullChargeProjectile");
                uberChargeProjectile = PlayableLighting.dataBundle.LoadAsset<RuntimeAnimatorController>("UberChargeProjectile");

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
