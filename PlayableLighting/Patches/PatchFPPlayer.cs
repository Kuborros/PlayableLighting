using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace PlayableLightning.Patches
{
    internal class PatchFPPlayer
    {

        public static FPPlayer player;

        public static AudioClip basicShotExplodeSfx;

        internal static float guardBuffer;
        internal static float jumpMultiplier;
        internal static float speedMultiplier;

        private static float ghostTimer;
        private static float gravAngleX;
        private static float gravAngleY;

        private static bool dashFlag;
        private static bool uberShot;

        private static int flightAbilityUseCount = 0;
        private static float flightAbilityCooldown = 0f;

        private static float weaponCharge = 0f;
        private static float shotDelay = 10f;
        private static float chargeShotDelay = 50f;
        private static int weaponChargeLevel = 0;

        private static GameObject chargeFX;

        private static readonly float energyRecoveryBaseSpeed = 0.4f;
        private static readonly float baseProjectileDamage = 3f;
        private static readonly float baseChargeProjectileDamage = 3f;
        private static readonly float maxChargeProjectileDamage = 5f;

        private static readonly float lightningDashVel = 24f;

        private static RuntimeAnimatorController baseProjectile;
        private static RuntimeAnimatorController partChargeProjectile;
        private static RuntimeAnimatorController fullChargeProjectile;
        private static RuntimeAnimatorController uberChargeProjectile;

        internal static readonly MethodInfo m_AirMoves = SymbolExtensions.GetMethodInfo(() => Action_Lightning_AirMoves());
        internal static readonly MethodInfo m_FuelPickup = SymbolExtensions.GetMethodInfo(() => Action_Lightning_FuelPickup());
        internal static readonly MethodInfo m_GroundMoves = SymbolExtensions.GetMethodInfo(() => Action_Lightning_GroundMoves());


        //Actions


        internal static void Action_Lightning_GravityBoots()
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
            player.state = new FPObjectState(State_Lightning_GravityBoots_P1);
        }

        internal static void Action_Lightning_WingSmash()
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
            player.state = new FPObjectState(State_Lightning_WingSmash_P1);
        }

        internal static void Action_Lightning_NormalShotFire()
        {
            FPAudio.PlaySfx(player.sfxCarolAttack1);
            ProjectileBasic basicShot;
            if (player.direction == FPDirection.FACING_LEFT)
            {
                basicShot = (ProjectileBasic)FPStage.CreateStageObject(ProjectileBasic.classID, chargeFX.transform.position.x, chargeFX.transform.position.y);
                basicShot.velocity.x = Mathf.Cos(0.017453292f * player.angle) * -15f;
                basicShot.velocity.y = Mathf.Sin(0.017453292f * player.angle) * -15f;
            }
            else
            {
                basicShot = (ProjectileBasic)FPStage.CreateStageObject(ProjectileBasic.classID, chargeFX.transform.position.x, chargeFX.transform.position.y);
                basicShot.velocity.x = Mathf.Cos(0.017453292f * player.angle) * 15f;
                basicShot.velocity.y = Mathf.Sin(0.017453292f * player.angle) * 15f;
            }
            basicShot.animatorController = baseProjectile;
            basicShot.animator = basicShot.GetComponent<Animator>();
            basicShot.animator.runtimeAnimatorController = basicShot.animatorController;
            basicShot.attackPower = baseProjectileDamage * player.GetAttackModifier();
            basicShot.direction = player.direction;
            basicShot.angle = player.angle;
            basicShot.damageElementType = -1;
            basicShot.explodeType = FPExplodeType.WHITEBURST;
            basicShot.ignoreTerrain = false;
            basicShot.ignoreInvincibility = false;
            basicShot.explodeTimer = 50f;
            basicShot.terminalVelocity = 0f;
            basicShot.gravityStrength = 0;
            basicShot.sfxExplode = basicShotExplodeSfx;
            basicShot.parentObject = player;
            basicShot.faction = player.faction;
            basicShot.timeBeforeCollisions = 0f;
            basicShot.halfHeight = 6;
            basicShot.halfWidth = 12;
            

            if (player.IsPowerupActive(FPPowerup.SHADOW_GUARD))
            {

            }

        }

        internal static void Action_Lightning_ChargedShotFire(int chargeLevel)
        {
            ProjectileBasic chargeShot;
            if (player.direction == FPDirection.FACING_LEFT)
            {
                chargeShot = (ProjectileBasic)FPStage.CreateStageObject(ProjectileBasic.classID, chargeFX.transform.position.x, chargeFX.transform.position.y);
                chargeShot.velocity.x = Mathf.Cos(0.017453292f * player.angle) * -10f;
                chargeShot.velocity.y = Mathf.Sin(0.017453292f * player.angle) * -10f;
            }
            else
            {
                chargeShot = (ProjectileBasic)FPStage.CreateStageObject(ProjectileBasic.classID, chargeFX.transform.position.x, chargeFX.transform.position.y);
                chargeShot.velocity.x = Mathf.Cos(0.017453292f * player.angle) * 10f;
                chargeShot.velocity.y = Mathf.Sin(0.017453292f * player.angle) * 10f;
            }

            if (chargeLevel >= 3)
            {
                chargeShot.animatorController = fullChargeProjectile;
                chargeShot.ignoreTerrain = true;
                chargeShot.halfHeight = 20;
                chargeShot.halfWidth = 24;
            }
            else
            {
                chargeShot.animatorController = partChargeProjectile;
                chargeShot.ignoreTerrain = false;
                chargeShot.halfHeight = 16;
                chargeShot.halfWidth = 20;
            }
            chargeShot.animator = chargeShot.GetComponent<Animator>();
            chargeShot.animator.runtimeAnimatorController = chargeShot.animatorController;
            chargeShot.attackPower = (baseChargeProjectileDamage + Math.Min(maxChargeProjectileDamage, weaponCharge / 10)) * player.GetAttackModifier();
            chargeShot.direction = player.direction;
            chargeShot.angle = player.angle;
            chargeShot.damageElementType = 3;
            chargeShot.explodeType = FPExplodeType.EXPLOSION;
            chargeShot.ignoreInvincibility = true;
            chargeShot.destroyOnHit = false;
            chargeShot.explodeTimer = 100f;
            chargeShot.terminalVelocity = 0f;
            chargeShot.gravityStrength = 0;
            chargeShot.sfxExplode = basicShotExplodeSfx;
            chargeShot.parentObject = player;
            chargeShot.faction = player.faction;
            chargeShot.timeBeforeCollisions = 0f;
            FPAudio.PlaySfx(player.sfxCarolAttack2);
            weaponCharge = 0f;
        }

        internal static void Action_Lightning_UberShotFire()
        {
            ProjectileBasic chargeShot;
            if (player.direction == FPDirection.FACING_LEFT)
            {
                chargeShot = (ProjectileBasic)FPStage.CreateStageObject(ProjectileBasic.classID, chargeFX.transform.position.x, chargeFX.transform.position.y);
                chargeShot.velocity.x = Mathf.Cos(0.017453292f * player.angle) * -13f;
                chargeShot.velocity.y = Mathf.Sin(0.017453292f * player.angle) * -13f;
            }
            else
            {
                chargeShot = (ProjectileBasic)FPStage.CreateStageObject(ProjectileBasic.classID, chargeFX.transform.position.x, chargeFX.transform.position.y);
                chargeShot.velocity.x = Mathf.Cos(0.017453292f * player.angle) * 13f;
                chargeShot.velocity.y = Mathf.Sin(0.017453292f * player.angle) * 13f;
            }

            chargeShot.animatorController = uberChargeProjectile;
            chargeShot.animator = chargeShot.GetComponent<Animator>();
            chargeShot.animator.runtimeAnimatorController = chargeShot.animatorController;
            chargeShot.attackPower = 8 * player.GetAttackModifier();
            chargeShot.direction = player.direction;
            chargeShot.angle = player.angle;
            chargeShot.damageElementType = 3;
            chargeShot.explodeType = FPExplodeType.EXPLOSION;
            chargeShot.ignoreInvincibility = true;
            chargeShot.destroyOnHit = false;
            chargeShot.explodeTimer = 100f;
            chargeShot.terminalVelocity = 0f;
            chargeShot.gravityStrength = 0;
            chargeShot.sfxExplode = basicShotExplodeSfx;
            chargeShot.parentObject = player;
            chargeShot.faction = player.faction;
            chargeShot.timeBeforeCollisions = 0f;

            chargeShot.ignoreTerrain = true;
            chargeShot.halfHeight = 25;
            chargeShot.halfWidth = 25;
            chargeShot.explodeType = FPExplodeType.BIGEXPLOSION;

            FPAudio.PlaySfx(player.sfxCarolAttack3);
        }

        internal static void Action_Lightning_FuelPickup()
        {
            if (player.hasSpecialItem)
            {
                player.invincibilityTime = Mathf.Max(player.invincibilityTime, 240f);
                player.flashTime = Mathf.Max(player.flashTime, 240f);
                FPAudio.PlaySfx(16);
            }
            else
            {
                player.hasSpecialItem = true;
            }
        }

        internal static void Action_Lightning_AirMoves()
        {

            if (player.input.jumpPress && player.velocity.y < player.jumpStrength && !player.jumpAbilityFlag && player.targetWaterSurface == null)
            {
                player.jumpAbilityFlag = true;
                flightAbilityUseCount++;
                player.velocity.y = Mathf.Max(player.jumpStrength * jumpMultiplier, player.velocity.y);
                player.state = new FPObjectState(player.State_InAir);
                player.SetPlayerAnimation("Spring", new float?(0f), new float?(0f), true, true);
                player.genericTimer = 0f;
                player.jumpReleaseFlag = true;
                player.Action_PlaySoundUninterruptable(player.sfxDoubleJump);
                WhiteBurst whiteBurst = (WhiteBurst)FPStage.CreateStageObject(WhiteBurst.classID, player.position.x, player.position.y - 24f);
                whiteBurst.scale.x = 1.5f;
                whiteBurst.scale.y = 0.3f;
            }
            else if (dashFlag && player.guardTime <= 20f && player.guardTime > 5f && player.input.guardHold && player.energy > 30f)
            {
                //Air Dash
                dashFlag = false;
                player.genericTimer = 0;
                player.energy = -30f;
                ghostTimer = 0;
                if (player.direction == FPDirection.FACING_RIGHT)
                {
                    player.velocity.x = Mathf.Max(Mathf.Min(player.velocity.x + lightningDashVel, 18f), player.velocity.x);
                }
                else
                {
                    player.velocity.x = Mathf.Min(Mathf.Max(player.velocity.x - lightningDashVel, -18f), player.velocity.x);
                }
                player.Action_PlaySoundUninterruptable(player.sfxBigBoostLaunch);
                player.state = new FPObjectState(State_Lightning_Dash);
            }
            else if ((player.guardTime <= 0f || player.cancellableGuard) && (player.input.guardPress || (guardBuffer > 0f && player.input.guardHold)))
            {
                player.SetPlayerAnimation("GuardAir", null, null, false, true);
                player.animator.SetSpeed(Mathf.Max(1f, 0.7f + Mathf.Abs(player.velocity.x * 0.05f)));
                FPAudio.PlaySfx(15);
                player.Action_Guard(0f, false);
                //player.Action_ShadowGuard();
                GuardFlash guardFlash = (GuardFlash)FPStage.CreateStageObject(GuardFlash.classID, player.position.x, player.position.y);
                guardFlash.parentObject = player;
                dashFlag = true;
            }
            else if (player.input.attackPress && shotDelay < 0f)
            {
                if (player.targetWaterSurface == null) player.SetPlayerAnimation("Jumping_Loop");
                else player.SetPlayerAnimation("Swimming");
                player.genericTimer = 0f;
                shotDelay = 5f;
                chargeShotDelay = 40f;
                Action_Lightning_NormalShotFire();
                player.idleTimer = -player.fightStanceTime;
                player.Action_StopSound();
            }
            else if (player.input.attackHold && shotDelay < 0f && chargeShotDelay < 0f && player.energy > 20f)
            {
                if (player.targetWaterSurface == null) player.SetPlayerAnimation("Jumping_Loop");
                else player.SetPlayerAnimation("Swimming");
                shotDelay = 5f;
                chargeShotDelay = 40f;
                player.state = new FPObjectState(State_Lightning_AttackHold);
                player.idleTimer = -player.fightStanceTime;
                player.Action_StopSound();
            }
            //Gravity Boots
            else if ((player.input.up || player.input.down) && player.input.specialHold && player.state != new FPObjectState(State_Lightning_WingSmash_P2) && player.state != new FPObjectState(State_Lightning_GravityBoots_P2))
            {
                Action_Lightning_GravityBoots();
            }
            //Wing Smash
            else if ((player.input.left || player.input.right) && player.input.specialHold && player.state != new FPObjectState(State_Lightning_WingSmash_P2) && player.state != new FPObjectState(State_Lightning_GravityBoots_P2))
            {
                Action_Lightning_WingSmash();
            }
        }

        internal static void Action_Lightning_GroundMoves()
        {

            //Guard
            if (dashFlag && player.guardTime <= 20f && player.guardTime > 10f && player.input.guardHold && player.energy > 30f)
            {
                dashFlag = false;
                player.genericTimer = 0f;
                player.guardTime = 30f;
                player.energy = -30f;
                if (player.direction == FPDirection.FACING_LEFT)
                {
                    player.groundVel -= lightningDashVel / 2;
                }
                else
                {
                    player.groundVel += lightningDashVel / 2;
                }
                player.state = State_Lightning_Dash;
                player.Action_PlaySoundUninterruptable(player.sfxBoostLaunch);
            }
            else if ((player.guardTime <= 0f || player.cancellableGuard) && (player.input.guardPress || (guardBuffer > 0f && player.input.guardHold)))
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
                //player.Action_ShadowGuard();
                GuardFlash guardFlash = (GuardFlash)FPStage.CreateStageObject(GuardFlash.classID, player.position.x, player.position.y);
                guardFlash.parentObject = player;
                dashFlag = true;
            }
            else if (player.input.attackPress && shotDelay < 0f && player.state != new FPObjectState(State_Lightning_AttackHold))
            {
                if (player.velocity.x < 2 && player.velocity.x > -2 && player.velocity.y < 2 && player.velocity.y > -2)
                {
                    if (player.state == new FPObjectState(player.State_Crouching))
                        player.SetPlayerAnimation("Crouching_Loop");
                    else
                        player.SetPlayerAnimation("GroundCharge");
                }
                else
                    player.SetPlayerAnimation("RunningShot");
                player.genericTimer = 0f;
                shotDelay = 5f;
                chargeShotDelay = 40f;
                Action_Lightning_NormalShotFire();
                player.idleTimer = -player.fightStanceTime;
                player.Action_StopSound();
            }
            else if (player.input.attackHold && chargeShotDelay < 0f && shotDelay < 0f && player.energy > 20f && player.state != new FPObjectState(State_Lightning_AttackHold))
            {
                if (player.velocity.x < 2 && player.velocity.x > -2 && player.velocity.y < 2 && player.velocity.y > -2)
                {
                    if (player.state == new FPObjectState(player.State_Crouching))
                        player.SetPlayerAnimation("Crouching_Loop");
                    else
                        player.SetPlayerAnimation("GroundCharge");
                }
                else
                    player.SetPlayerAnimation("RunningShot");
                player.genericTimer = 0f;
                shotDelay = 5f;
                chargeShotDelay = 50f;
                player.state = new FPObjectState(State_Lightning_AttackHold);
                player.idleTimer = -player.fightStanceTime;
                player.Action_StopSound();
            }
            else if (player.input.up && player.input.specialHold)
            {
                Action_Lightning_GravityBoots();
            }
        }

        //States

        internal static void State_Lightning_GravityBoots_P1()
        {
            player.SetPlayerAnimation("MoonJump");

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

            if (player.genericTimer <= 30f)
            {
                player.genericTimer += FPStage.deltaTime;
                player.velocity.x = 0f;
                player.angle = 0f;
                if (player.velocity.x > 0f)
                {
                    player.velocity.x -= 0.125f * FPStage.deltaTime;
                }
                else if (player.velocity.x < 0f)
                {
                    player.velocity.x += 0.125f * FPStage.deltaTime;
                }
                if (player.velocity.y > 0f)
                {
                    player.velocity.y -= 0.125f * FPStage.deltaTime;
                }
                else if (player.velocity.y < 0f)
                {
                    player.velocity.y += 0.125f * FPStage.deltaTime;
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
                player.SetPlayerAnimation("MoonJump_Loop");
                player.state = new FPObjectState(State_Lightning_GravityBoots_P2);
            }
        }

        internal static void State_Lightning_GravityBoots_P2()
        {
            player.genericTimer += FPStage.deltaTime;
            player.energyRecoverRate = 0f;
            player.superArmor = true;
            player.invincibilityTime = Mathf.Max(player.invincibilityTime, 50f);
            ghostTimer += FPStage.deltaTime;
            flightAbilityCooldown = 20f;
            player.attackStats = new FPObjectState(AttackStats_GravityBoots);

            if (player.colliderRoof == null && player.colliderWall == null) player.velocity.x = gravAngleX;
            if (player.colliderRoof == null && player.colliderWall == null) player.velocity.y = gravAngleY;

            if (ghostTimer >= 0.5f)
            {
                Ghost();
                ghostTimer = 0f;
            }
            if (player.onGround || player.onGrindRail || player.colliderRoof != null || (!player.input.specialHold && !player.input.jumpHold) || player.genericTimer >= 50f || player.energy <= 0f)
            {
                player.energyRecoverRate = energyRecoveryBaseSpeed;
                player.hbAttack.enabled = false;
                player.superArmor = false;
                player.invincibilityTime = 0f;
                player.flashTime = 0f;
                if (player.onGround)
                {
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
            player.Process360Movement();
        }

        internal static void State_Lightning_WingSmash_P1()
        {
            player.SetPlayerAnimation("WingSmash");

            if (player.genericTimer <= 30f)
            {
                player.genericTimer += FPStage.deltaTime;
                player.velocity.x = 0f;
                player.angle = 0f;
                if (player.velocity.x > 0f)
                {
                    player.velocity.x -= 0.125f * FPStage.deltaTime;
                }
                else if (player.velocity.x < 0f)
                {
                    player.velocity.x += 0.125f * FPStage.deltaTime;
                }
                if (player.velocity.y > 0f)
                {
                    player.velocity.y -= 0.125f * FPStage.deltaTime;
                }
                else if (player.velocity.y < 0f)
                {
                    player.velocity.y += 0.125f * FPStage.deltaTime;
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
                player.state = new FPObjectState(State_Lightning_WingSmash_P2);
            }
        }

        internal static void State_Lightning_WingSmash_P2()
        {
            player.genericTimer += FPStage.deltaTime;
            player.energyRecoverRate = 0f;
            player.SetPlayerAnimation("WingSmash_Loop");
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
                player.velocity.y += 7.5f * FPStage.deltaTime;
                if (player.direction == FPDirection.FACING_RIGHT)
                    player.angle = 10f;
                else
                    player.angle = -10f;
            }
            else if (player.input.down)
            {
                player.velocity.y -= 7.5f * FPStage.deltaTime;
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
                player.invincibilityTime = 0f;
                player.flashTime = 0f;
                if (player.onGround)
                {
                    player.state = new FPObjectState(player.State_Ground);
                }
                else
                {
                    player.SetPlayerAnimation("Jumping_Loop", 0.25f, 0.25f, false, true);
                    player.state = new FPObjectState(player.State_InAir);
                }
                player.attackStats = new FPObjectState(AttackStats_Idle);
            }
        }

        internal static void State_Lightning_AttackHold()
        {
            if (player.input.attackHold)
            {
                SetAnimSpeedToVelocity(player);
                PlaySFXCh5(player.sfxMillaShieldSummon);
                PlaySFXLooping(player.sfxMillaShieldFire, 1.5f);
                player.genericTimer += FPStage.deltaTime;
                player.energyRecoverRate = 0f;
                weaponCharge += 1f * FPStage.deltaTime;

                chargeFX.gameObject.SetActive(true);
                if (weaponCharge < 20f && weaponChargeLevel == 0)
                {
                    chargeFX.GetComponent<Animator>().Play("Charge1_Intro");
                    weaponChargeLevel++;
                }
                else if (weaponCharge >= 20f && weaponChargeLevel == 1)
                {
                    chargeFX.GetComponent<Animator>().Play("Charge2_Intro");
                    weaponChargeLevel++;
                }
                else if (weaponCharge >= 90f && weaponChargeLevel == 2)
                {
                    chargeFX.GetComponent<Animator>().Play("Charge3_Intro");
                    weaponChargeLevel++;
                }

                if (player.onGround)
                {
                    if (player.input.jumpPress)
                    {
                        player.genericTimer = 0f;
                        player.Action_SoftJump();
                        if (player.targetWaterSurface == null) player.SetPlayerAnimation("Jumping_Loop");
                        else player.SetPlayerAnimation("Swimming");
                    }
                    else
                    {
                        ApplyGroundForces(player, false);
                        if (player.velocity.x < 2 && player.velocity.x > -2 && player.velocity.y < 2 && player.velocity.y > -2)
                        {
                            if (player.input.down)
                                player.SetPlayerAnimation("Crouching_Loop");
                            else
                                player.SetPlayerAnimation("GroundCharge");
                        }
                        else
                            player.SetPlayerAnimation("RunningShot");
                        player.angle = player.groundAngle;
                    }
                    player.jumpAbilityFlag = false;
                }
                else
                {
                    if (player.targetWaterSurface == null) player.SetPlayerAnimation("Jumping_Loop");
                    else player.SetPlayerAnimation("Swimming");
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
                        player.velocity.y += 0.3f * FPStage.deltaTime;
                        if (player.velocity.y < -4.5f)
                        {
                            player.velocity.y = -4.5f;
                        }
                    }
                }
                //Ubercharge
                if (player.hasSpecialItem && weaponCharge >= 90 && !uberShot)
                {
                    Action_Lightning_UberShotFire();
                    uberShot = true;
                }
            }
            else
            {
                StopSFXLooping();
                StopSFXCh5();
                chargeFX.gameObject.SetActive(false);
                player.energyRecoverRate = energyRecoveryBaseSpeed;
                uberShot = false;
                if (weaponCharge > 20f)
                {
                    Action_Lightning_ChargedShotFire(weaponChargeLevel);
                }
                if (player.onGround)
                {
                    player.state = new FPObjectState(player.State_Ground);
                }
                else
                {
                    if (player.targetWaterSurface == null) player.SetPlayerAnimation("Jumping_Loop");
                    else player.SetPlayerAnimation("Swimming");
                    player.state = new FPObjectState(player.State_InAir);
                }
            }
        }

        private static void State_Lightning_Dash()
        {
            player.SetPlayerAnimation("AirDash");
            player.genericTimer += FPStage.deltaTime;
            player.superArmor = true;
            ghostTimer += FPStage.deltaTime;
            if (!player.onGround)
            {
                player.velocity.y = 0f;
                if (player.targetWaterSurface != null)
                    ApplyWaterForces(player);
                else
                    ApplyAirForces(player, true);
            }
            else ApplyGroundForces(player, false);

            player.attackStats = new FPObjectState(AttackStats_Blink);

            if (ghostTimer >= 2f)
            {
                Ghost();
                ghostTimer = 0f;
            }

            if (player.genericTimer >= 15f)
            {
                player.genericTimer = 0f;
                player.hbAttack.enabled = false;
                player.superArmor = false;
                if (player.onGround)
                {
                    player.state = new FPObjectState(player.State_Ground);
                }
                else
                {
                    if (player.targetWaterSurface == null) player.SetPlayerAnimation("Jumping_Loop");
                    else player.SetPlayerAnimation("Swimming");

                    player.state = new FPObjectState(player.State_InAir);
                }
                return;
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
            player.attackHitstun = 3f;
            player.attackEnemyInvTime = 3f;
            player.attackKnockback.x = 0f;
            player.attackKnockback.y = 0f;
            player.attackSfx = 5;
            player.attackPower *= player.GetAttackModifier();
        }

        private static void AttackStats_Blink()
        {
            player.attackPower = 8f;
            player.attackHitstun = 0f;
            player.attackEnemyInvTime = 10f;
            player.attackKnockback.x = Mathf.Max(Mathf.Abs(player.prevVelocity.x * 0.375f), 4.5f);
            if (player.direction == FPDirection.FACING_LEFT)
            {
                player.attackKnockback.x = 0f - player.attackKnockback.x;
            }
            player.attackKnockback.y = player.prevVelocity.y * 0.5f;
            player.attackSfx = 5;
            player.attackPower *= player.GetAttackModifier();
        }

        //Others

        private static void PlaySFXLooping(AudioClip clip, float delay)
        {
            //Channel 4 is used for Carol's bike, so we can repurpose it here.
            if (clip != null)
            {
                if (player.audioChannel[4].clip != clip)
                {
                    player.audioChannel[4].clip = clip;
                    player.audioChannel[4].loop = true;
                    player.audioChannel[4].PlayDelayed(delay);
                }
            }
        }

        public static void StopSFXLooping()
        {
            if (player.audioChannel[4].clip != null)
            {
                player.audioChannel[4].Stop();
                player.audioChannel[4].clip = null;
            }
        }

        private static void PlaySFXCh5(AudioClip clip)
        {
            if (clip != null)
            {
                if (player.audioChannel[5].clip != clip)
                {
                    player.audioChannel[5].clip = clip;
                    player.audioChannel[5].loop = false;
                    player.audioChannel[5].Play();
                }
            }
        }

        public static void StopSFXCh5()
        {
            if (player.audioChannel[5].clip != null)
            {
                player.audioChannel[5].Stop();
                player.audioChannel[5].clip = null;
            }
        }

        private static void ChargedShotExplosion(ProjectileBasic projectile)
        {
            //Normal charge
            if (projectile.halfHeight < 20)
            {
                FPStage.CreateStageObject(Explosion.classID, projectile.position.x, projectile.position.y);
            }
            //Ubercharge
            else if (projectile.halfHeight == 20)
            {
                FPStage.CreateStageObject(BigExplosion.classID, projectile.position.x, projectile.position.y);
            }
            FPAudio.PlaySfx(projectile.sfxExplode);
        }

        private static void Ghost()
        {
            Color start = new Color(1f, 1f, 1f, 0.8f);
            Color end = new Color(1f, 1f, 1f, 0f);
            SpriteGhost spriteGhost = (SpriteGhost)FPStage.CreateStageObject(SpriteGhost.classID, player.transform.position.x, player.transform.position.y);
            spriteGhost.transform.rotation = player.transform.rotation;
            spriteGhost.SetUp(player.gameObject.GetComponent<SpriteRenderer>().sprite, start, end, 0.5f, 3f);
            spriteGhost.transform.localScale = player.transform.localScale;
            spriteGhost.maxLifeTime = 0.5f;
            spriteGhost.growSpeed = 0f;
            spriteGhost.activationMode = FPActivationMode.ALWAYS_ACTIVE;
        }

        private static void ResetStaticVars()
        {
            //Reset all static vars on level start
            //Prevents smuggling weird values between stages

            ghostTimer = 0f;
            gravAngleX = 0f;
            gravAngleY = 0f;

            flightAbilityUseCount = 0;
            flightAbilityCooldown = 0f;

            weaponChargeLevel = 0;
            weaponCharge = 0f;
            dashFlag = false;
            uberShot = false;
        }

        //Postfixes
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPPlayer), "Update", MethodType.Normal)]
        static void PatchPlayerUpdate(FPPlayer __instance, float ___speedMultiplier, float ___guardBuffer, float ___jumpMultiplier)
        {
            if (FPSaveManager.character == PlayableLightning.currentLightningID)
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
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPPlayer), "LateUpdate", MethodType.Normal)]
        static void PatchLateUpdate(FPPlayer __instance)
        {
            if (FPSaveManager.character == PlayableLightning.currentLightningID)
            {

                flightAbilityCooldown -= FPStage.deltaTime;
                shotDelay -= FPStage.deltaTime;
                chargeShotDelay -= FPStage.deltaTime;

                if (__instance.state != new FPObjectState(State_Lightning_AttackHold))
                {
                    chargeFX.gameObject.SetActive(false);
                    weaponChargeLevel = 0;
                    weaponCharge = 0f;
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPPlayer), "Start", MethodType.Normal)]
        static void PatchPlayerStart(FPPlayer __instance)
        {
            if (FPSaveManager.character == PlayableLightning.currentLightningID)
            {
                player = __instance;
                ResetStaticVars();

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
                baseProjectile = PlayableLightning.dataBundle.LoadAsset<RuntimeAnimatorController>("BaseProjectile");
                partChargeProjectile = PlayableLightning.dataBundle.LoadAsset<RuntimeAnimatorController>("PartChargeProjectile");
                fullChargeProjectile = PlayableLightning.dataBundle.LoadAsset<RuntimeAnimatorController>("FullChargeProjectile");
                uberChargeProjectile = PlayableLightning.dataBundle.LoadAsset<RuntimeAnimatorController>("UberChargeProjectile");

                //Sounds
                basicShotExplodeSfx = PlayableLightning.dataBundle.LoadAsset<AudioClip>("Hit");

                //Spooky
                GameObject ghost = PlayableLightning.dataBundle.LoadAsset<GameObject>("DashGhost");
                GameObject.Instantiate(ghost);

                chargeFX = player.gameObject.transform.GetChild(1).gameObject;
            }

            //Fast Ladders (truly the most OP item in MM8)
            //Works for all characters.
            if (__instance.IsPowerupActive(PlayableLightning.fastLaddersID))
            {
                __instance.climbingSpeed = 2 * __instance.climbingSpeed;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPPlayer), "State_InAir", MethodType.Normal)]
        [HarmonyPatch(typeof(FPPlayer), "State_Ground", MethodType.Normal)]
        [HarmonyPatch(typeof(FPPlayer), "State_Crouching", MethodType.Normal)]
        [HarmonyPatch(typeof(FPPlayer), "State_Ball", MethodType.Normal)]
        [HarmonyPatch(typeof(FPPlayer), "State_Ball_Inert", MethodType.Normal)]
        [HarmonyPatch(typeof(FPPlayer), "State_Hanging", MethodType.Normal)]
        [HarmonyPatch(typeof(FPPlayer), "State_LadderClimb", MethodType.Normal)]
        [HarmonyPatch(typeof(FPPlayer), "State_GrindRail", MethodType.Normal)]
        [HarmonyPatch(typeof(FPPlayer), "State_Defeat", MethodType.Normal)]
        internal static void NoAuraFarming()
        {
            if (FPSaveManager.character != PlayableLightning.currentLightningID) return;

            if (player.transform.GetChild(0).gameObject.activeSelf)
                player.transform.GetChild(0).gameObject.SetActive(false);

            StopSFXLooping();
            StopSFXCh5();

            uberShot = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPPlayer), "ReturnToGeneralState", [typeof(bool), typeof(bool)])]
        internal static void PatchReturnToGeneralState(FPPlayer __instance)
        {
            if (__instance.characterID == PlayableLightning.currentLightningID && player.currentAnimation == "Hide" && __instance.targetWaterSurface == null)
            {
                if (__instance.onGround)
                {
                    player.state = new FPObjectState(player.State_Ground);
                    player.State_Ground();
                }
                else
                {
                    player.state = new FPObjectState(player.State_InAir);
                    __instance.SetPlayerAnimation("Jumping_Loop");
                }
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
