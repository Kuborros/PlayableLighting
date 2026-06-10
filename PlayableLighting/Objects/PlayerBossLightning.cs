using BepInEx.Logging;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace PlayableLightning.Objects
{
    internal class PlayerBossLightning : PlayerBoss
    {
        public static int classID = -1;
        internal static ManualLogSource logSource = PlayableLightning.logSource;

        [Header("Boss Settings")]
        public FPHitBox walkRange;
        public float pursuitRange;
        public FPBaseObject targetToPursue;

        private GameObject chargeFX;
        private float shotDelay = 0;
        private float ghostTimer = 0;
        private float spinDelay = 10f;
        private bool stuffInitDone = false;

        private RuntimeAnimatorController baseProjectile;
        private RuntimeAnimatorController fullChargeProjectile;


        private AudioClip sfxFire, sfxBigFire, sfxChargeIntro, sfxChargeLoop;

        //Generic boss stuff
        public override void ResetStaticVars()
        {
            base.ResetStaticVars();
            classID = -1;
        }

        public void Action_FacePlayer()
        {
            if (targetPlayer != null)
            {
                if (position.x > targetPlayer.position.x)
                {
                    direction = FPDirection.FACING_LEFT;
                }
                else if (position.x < targetPlayer.position.x)
                {
                    direction = FPDirection.FACING_RIGHT;
                }
            }
        }

        private void CheckBoundaries()
        {
            if (faction != "Player")
            {
                FPPlayer fPPlayer = FPStage.FindNearestPlayer(this, 800f);
                if (fPPlayer != null)
                {
                    if (invincibility > 0f)
                    {
                        if (position.x > fPPlayer.position.x + 10f)
                        {
                            direction = FPDirection.FACING_LEFT;
                        }
                        else if (position.x < fPPlayer.position.x - 10f)
                        {
                            direction = FPDirection.FACING_RIGHT;
                        }
                    }
                    else if (state == new FPObjectState(State_Running))
                    {
                        if (position.x > fPPlayer.position.x + pursuitRange)
                        {
                            direction = FPDirection.FACING_LEFT;
                        }
                        else if (position.x < fPPlayer.position.x - pursuitRange)
                        {
                            direction = FPDirection.FACING_RIGHT;
                        }
                    }
                }
            }
            else if (targetToPursue != null)
            {
                if (invincibility > 0f)
                {
                    if (position.x > targetToPursue.position.x + 10f)
                    {
                        direction = FPDirection.FACING_LEFT;
                    }
                    else if (position.x < targetToPursue.position.x - 10f)
                    {
                        direction = FPDirection.FACING_RIGHT;
                    }
                }
                else if (state == new FPObjectState(State_Running))
                {
                    if (position.x > targetToPursue.position.x + pursuitRange)
                    {
                        direction = FPDirection.FACING_LEFT;
                    }
                    else if (position.x < targetToPursue.position.x - pursuitRange)
                    {
                        direction = FPDirection.FACING_RIGHT;
                    }
                }
            }
            if (walkRange.enabled && position.x > start.x + walkRange.right)
            {
                direction = FPDirection.FACING_LEFT;
            }
            else if (walkRange.enabled && position.x < start.x + walkRange.left)
            {
                direction = FPDirection.FACING_RIGHT;
            }
            if (colliderWall != null && spinDelay < 0f)
            {
                if (onGround)
                {
                    groundVel = 0f - groundVel;
                }
                else
                {
                    velocity.x = 0f - prevVelocity.x;
                }
                direction ^= FPDirection.FACING_RIGHT;
                spinDelay = 30f;
            }
            if (direction == FPDirection.FACING_RIGHT)
            {
                input.right = true;
                input.left = false;
            }
            else
            {
                input.left = true;
                input.right = false;
            }

        }

        private void InteractWithObjects()
        {
            if (health <= healthToFlinch)
            {
                healthToFlinch -= 25f;
                invincibility = 60f;
                Action_Hurt();
            }
            if (state != new FPObjectState(base.State_KO) && health <= 0f)
            {
                if (frozen)
                {
                    defrostTimer = 0f;
                    DestroyIceBlock();
                    animator.SetSpeed(1f);
                    frozen = false;
                }
                Action_PlaySound(sfxKO);
                Action_PlayVoice(vaKO);
                if (bgmBoss != null)
                {
                    FPAudio.StopMusic();
                }
                gscrSlowdownOnKO = FPStage.SetRequestGameSpeedChange(this, 0.25f, 20f, GameSpeedChangeRequest.GameSpeedChangePriority_Medium);
                velocity.x = velocity.x * 0.75f + hurtKnockbackX;
                velocity.y = 8f;
                onGround = false;
                if (velocity.x < 3f && velocity.x > -3f)
                {
                    if (hurtKnockbackX > 0f)
                    {
                        velocity.x = 3f;
                    }
                    else
                    {
                        velocity.x = -3f;
                    }
                }
                genericTimer = -20f;
                invincibility = 200f;
                hitStun = 0f;
                state = base.State_KO;
            }
            switch (DamageCheck())
            {
                case 1:
                    flashTime = 2f;
                    break;
                case 2:
                    activationMode = FPActivationMode.ALWAYS_ACTIVE;
                    velocity.y = 4.5f;
                    break;
                case 4:
                    state = State_Frozen;
                    break;
            }
            cannotBeFrozen = false;
        }

        private void Update()
        {
            if (!FPStage.objectsRegistered)
            {
                return;
            }
            PlayerBossUpdate();
            if (targetPlayer == null)
            {
                targetPlayer = FPStage.FindNearestPlayer(this, 100000f);
            }
            if (state == new FPObjectState(base.State_Init))
            {
                SetPlayerAnimation("Jumping", 0.5f, 0.5f);
                if (!bossActivated)
                {
                    state = State_Default;
                }
                else
                {
                    state = State_Running;
                }
            }
            else if (state == new FPObjectState(base.State_Guard))
            {
                CheckBoundaries();
                Process360Movement();
            }
            if (!(state != new FPObjectState(State_Frozen)) || !(state != new FPObjectState(base.State_KO)))
            {
                return;
            }
            if (onGround)
            {
                attackKnockback.x = groundVel * 0.5f;
            }
            else
            {
                attackKnockback.x = velocity.x * 0.5f;
            }
            attackKnockback.y = velocity.y * 0.5f;
            if (!FPStage.ConfirmClassWithPoolTypeID(typeof(FPPlayer), FPPlayer.classID))
            {
                return;
            }
            FPBaseObject objRef = null;
            while (FPStage.ForEach(FPPlayer.classID, ref objRef))
            {
                FPPlayer fPPlayer = (FPPlayer)objRef;
                if (fPPlayer.invincibilityTime <= 0f && faction != fPPlayer.faction && FPCollision.CheckOOBB(this, hbAttack, objRef, fPPlayer.hbTouch))
                {
                    if (fPPlayer.guardTime <= 15f)
                    {
                        fPPlayer.hitStun = 4f;
                    }
                    fPPlayer.hurtKnockbackX = attackKnockback.x;
                    fPPlayer.hurtKnockbackY = attackKnockback.y;
                    fPPlayer.healthDamage += 0.5f;
                    fPPlayer.Action_HitSpark(this);
                }
            }
        }

        private new void LateUpdate()
        {
            if (FPStage.objectsRegistered)
            {
                PlayerBossLateUpdate();
                if (frozen && state != new FPObjectState(State_Frozen))
                {
                    defrostTimer = 0f;
                    DestroyIceBlock();
                    animator.SetSpeed(1f);
                    frozen = false;
                }
                if (energy < 100f)
                {
                    energy += 0.4f * FPStage.deltaTime;
                }
                shotDelay += FPStage.deltaTime;
                spinDelay -= FPStage.deltaTime;
            }
        }

        private new void Start()
        {
            healthToFlinch = health - 25f;

            base.Start();
            classID = FPStage.RegisterObjectType(this, GetType(), 0);
            objectID = classID;

            //Mirror Match
            if (FPSaveManager.character == PlayableLightning.currentLightningID)
            {
                SpriteRenderer component = GetComponent<SpriteRenderer>();
                if (component != null)
                {
                    component.color = new Color(0f, 1f, 1f);
                }
                SpriteOutline component2 = GetComponent<SpriteOutline>();
                if (component2 != null)
                {
                    component2.enabled = true;
                }
            }

            FPBossHud componentHud = GetComponent<FPBossHud>();
            componentHud.targetBoss = this;
            componentHud.maxHealth = health;

            InitLightningSpecificStuff();
        }

        private void InitLightningSpecificStuff()
        {
            if (stuffInitDone) return;
            //Lightning stuff

            //Load projectile animations
            baseProjectile = PlayableLightning.dataBundle.LoadAsset<RuntimeAnimatorController>("BaseProjectile");
            fullChargeProjectile = PlayableLightning.dataBundle.LoadAsset<RuntimeAnimatorController>("FullChargeProjectile");

            //Audio
            sfxFire = PlayableLightning.dataBundle.LoadAsset<AudioClip>("LV1 Shot");
            sfxBigFire = PlayableLightning.dataBundle.LoadAsset<AudioClip>("LV3 Shot");
            sfxChargeIntro = PlayableLightning.dataBundle.LoadAsset<AudioClip>("Charge_Intro");
            sfxChargeLoop = PlayableLightning.dataBundle.LoadAsset<AudioClip>("Charge_Loop");

            //Spooky
            GameObject ghost = PlayableLightning.dataBundle.LoadAsset<GameObject>("DashGhost");
            GameObject.Instantiate(ghost);

            chargeFX = gameObject.transform.GetChild(1).gameObject;
            stuffInitDone = true;
        }

        private void Ghost()
        {
            Color start = new Color(1f, 1f, 1f, 0.8f);
            Color end = new Color(1f, 1f, 1f, 0f);
            SpriteGhost spriteGhost = (SpriteGhost)FPStage.CreateStageObject(SpriteGhost.classID, transform.position.x, transform.position.y);
            spriteGhost.transform.rotation = transform.rotation;
            spriteGhost.SetUp(gameObject.GetComponent<SpriteRenderer>().sprite, start, end, 0.5f, 3f);
            spriteGhost.transform.localScale = transform.localScale;
            spriteGhost.maxLifeTime = 0.5f;
            spriteGhost.growSpeed = 0f;
            spriteGhost.activationMode = FPActivationMode.ALWAYS_ACTIVE;
        }

        private void State_Default()
        {
            SetPlayerAnimation("FightStance");
            spriteRenderer.sprite = null;
            InitLightningSpecificStuff();
            attackStats = AttackStats_Idle;
            if (!bossActivated && FPStage.timeEnabled && targetPlayer != null && targetPlayer.position.x > position.x - bossActivation.x && targetPlayer.position.x < position.x + bossActivation.x && targetPlayer.position.y > position.y - bossActivation.y && targetPlayer.position.y < position.y + bossActivation.y)
            {
                if (bgmBoss != null)
                {
                    FPAudio.StopMusic();
                    FPAudio.PlayMusic(bgmBoss);
                }
                Action_PlayVoice(vaStart[Random.Range(0, vaStart.Length - 1)]);
                isTalking = true;
                voiceTimer = 240f;
                bossActivated = true;
                FPBossHud component = GetComponent<FPBossHud>();
                component?.MoveIn();
                state = State_Running;
            }
            if (!FPStage.ConfirmClassWithPoolTypeID(typeof(FPPlayer), FPPlayer.classID))
            {
                bossActivated = true;
                FPBossHud component2 = GetComponent<FPBossHud>();
                component2?.MoveIn();
                state = State_Running;
            }
        }

        public void State_Frozen()
        {
            if (!frozen && freezeTimer > 0f)
            {
                iceBlockBack = Object.Instantiate(FPResources.childSprite[1]);
                iceBlockBack.parentObject = this;
                iceBlockBack.yOffset = 6f;
                iceBlock = Object.Instantiate(FPResources.childSprite[0]);
                iceBlock.parentObject = this;
                iceBlock.yOffset = 6f;
                frozen = true;
            }
            if (defrostTimer < 60f)
            {
                defrostTimer += FPStage.deltaTime;
                animator.SetSpeed(0f);
            }
            else
            {
                defrostTimer = 0f;
                DestroyIceBlock();
                animator.SetSpeed(1f);
                frozen = false;
                state = base.State_Init;
            }
            InteractWithObjects();
        }

        private void State_Jumping()
        {
            InteractWithObjects();
            Process360Movement();
            RotatePlayerUpright();
            NoAuraFarming();
            genericTimer += FPStage.deltaTime;
            if (onGround)
            {
                ApplyGroundForces();
                state = State_Running;
                return;
            }
            SetPlayerAnimation("Jumping");
            animator.SetSpeed(1f);
            ApplyAirForces();
            ApplyGravityForce();
            if (genericTimer > 0f && (Random.Range(0, 100) % 3) == 3)
            {
                //Mid-air shots
                genericTimer = 0f;
                animator.SetSpeed(Mathf.Max(1f, 0.7f + Mathf.Abs(velocity.x * 0.05f)));
                state = State_BasicShots;
                Action_StopSound();
            }
            else
            {
                if (!(genericTimer > 20f) || !(velocity.y < 0f))
                {
                    return;
                }
                genericTimer = 0f;
                int attack = Random.Range(0, 100);

                if (attack < 40)
                {
                    genericTimer = 0f;
                    state = State_BasicShots;
                }
                else if (attack < 60)
                {
                    genericTimer = 0f;
                    if (colliderWall == null)
                    {
                        state = State_Lightning_WingSmash_P1;
                    }
                }
                else
                {
                    genericTimer = 0f;
                    Action_PlaySound(sfxChargeIntro);
                    state = State_ChargeShot;

                }
            }
        }

        private void State_Running()
        {
            InteractWithObjects();
            Process360Movement();
            NoAuraFarming();
            if (!FPStage.timeEnabled)
            {
                energy = 0f;
            }

            if (Random.Range(0, 3) > 1)
            {
                if (position.x >= start.x)
                {
                    direction = FPDirection.FACING_LEFT;
                }
                else
                {
                    direction = FPDirection.FACING_RIGHT;
                }
            }

            if (onGround)
            {
                ApplyGroundForces();
                angle = groundAngle;
                ApplyGroundAnimation();

                if (nextAttack % 2 == 1 && genericTimer >= 0f)
                {
                    if (position.x >= start.x)
                    {
                        direction = FPDirection.FACING_LEFT;
                    }
                    else
                    {
                        direction = FPDirection.FACING_RIGHT;
                    }
                    genericTimer = 0f;
                    specialAttackDirection = 2;
                    if (Random.Range(0, 3) < 2)
                    {
                        genericTimer = 0f;
                        state = State_BasicShots;
                    }
                    else
                    {
                        genericTimer = 0f;
                        Action_PlaySound(sfxChargeIntro);
                        state = State_ChargeShot;
                    }
                    nextAttack++;
                }
                else
                {
                    if (genericTimer > 40f && !FPStage.timeEnabled)
                    {
                        genericTimer = Random.Range(-60f, -30f);
                        state = State_Idle;
                        return;
                    }
                    if (genericTimer > 40f)
                    {
                        genericTimer = 0f;
                        velocity.y = 10f;
                        onGround = false;
                        state = State_Jumping;
                        Action_PlaySound(sfxJump);
                        return;
                    }
                }
            }
            else
            {
                ApplyAirForces();
                ApplyGravityForce();
                if (currentAnimation == "Walking" || currentAnimation == "Running" || currentAnimation == "TopSpeed" || currentAnimation == "Hit1" || currentAnimation == "Hit2")
                {
                    SetPlayerAnimation("Jumping", 0.5f, 0.5f);
                    animator.SetSpeed(1f);
                }
            }
            if (faction == "Player" && FPStage.FindNearestEnemy(this, pursuitRange, string.Empty) == null)
            {
                energy -= 0.4f * FPStage.deltaTime;
            }
            else
            {
                genericTimer += FPStage.deltaTime;
            }
            CheckBoundaries();
        }

        private void State_BasicShots()
        {
            if (faction != "Player")
            {
                Action_FacePlayer();
            }

            if (onGround)
            {
                ApplyGroundForces();
                angle = groundAngle;
                input.left = false;
                input.right = false;
                CheckBoundaries();
                if (velocity.x < 2 && velocity.x > -2)
                    SetPlayerAnimation("GroundCharge");
                else
                    SetPlayerAnimation("RunningShot");
                //Ground firing logic
                if (shotDelay > 10)
                {
                    Action_Lightning_NormalShotFire();
                    shotDelay = 0;
                    energy = -10f;
                }
            }
            else
            {
                ApplyAirForces();
                ApplyGravityForce();
                CheckBoundaries();
                //Air firing logic
                SetPlayerAnimation("Jumping_Loop");
                if (shotDelay > 10)
                {
                    Action_Lightning_NormalShotFire();
                    shotDelay = 0;
                    energy = -10f;
                }
            }
            genericTimer += FPStage.deltaTime;

            //State Exit
            if (genericTimer > 50f)
            {
                genericTimer = Random.Range(-20f, 10f);
                state = State_Running;
                if (!onGround)
                    SetPlayerAnimation("Jumping_Loop", 0.5f, 0.5f);
            }
            InteractWithObjects();
            Process360Movement();
        }

        private void State_ChargeShot()
        {
            input.right = false;
            input.left = false;

            if (faction != "Player")
            {
                Action_FacePlayer();
            }

            if (onGround)
            {
                SetPlayerAnimation("GroundCharge");
                ApplyGroundForces();
            }
            else
            {
                SetPlayerAnimation("AirCharge");
                ApplyAirForces();
                ApplyGravityForce();
            }

            genericTimer += FPStage.deltaTime;

            chargeFX.gameObject.SetActive(true);
            if (genericTimer < 20f)
            {
                chargeFX.GetComponent<Animator>().Play("Charge1_Intro");
            }
            else if (genericTimer >= 20f && genericTimer < 40f)
            {
                chargeFX.GetComponent<Animator>().Play("Charge2_Intro");
            }
            else if (genericTimer >= 40f)
            {
                chargeFX.GetComponent<Animator>().Play("Charge3_Intro");
            }
            if (genericTimer > 60f)
            {
                Action_StopSound();
                chargeFX.gameObject.SetActive(false);

                Action_Lightning_ChargedShotFire();

                if (onGround)
                {
                    state = State_Running;
                    genericTimer = Random.Range(-30f, 0f);
                }
                else
                {
                    SetPlayerAnimation("Jumping", 0.5f, 0.5f);
                    state = State_Running;
                    genericTimer = 0f;

                }
            }
            CheckBoundaries();
            InteractWithObjects();
            Process360Movement();
        }

        internal void State_Lightning_WingSmash_P1()
        {
            SetPlayerAnimation("WingSmash");

            if (genericTimer <= 15f)
            {
                genericTimer += FPStage.deltaTime;
                velocity.x = 0f;
                angle = 0f;
                if (velocity.x > 0f)
                {
                    velocity.x -= 0.125f * FPStage.deltaTime;
                }
                else if (velocity.x < 0f)
                {
                    velocity.x += 0.125f * FPStage.deltaTime;
                }
                if (velocity.y > 0f)
                {
                    velocity.y -= 0.125f * FPStage.deltaTime;
                }
                else if (velocity.y < 0f)
                {
                    velocity.y += 0.125f * FPStage.deltaTime;
                }
                if (position.x >= start.x)
                {
                    direction = FPDirection.FACING_LEFT;
                }
                else
                {
                    direction = FPDirection.FACING_RIGHT;
                }
            }
            else
            {
                genericTimer = 0f;
                state = new FPObjectState(State_Lightning_WingSmash_P2);
            }
        }

        internal void State_Lightning_WingSmash_P2()
        {
            genericTimer += FPStage.deltaTime;
            SetPlayerAnimation("WingSmash_Loop");
            superArmor = true;
            ghostTimer += FPStage.deltaTime;
            attackStats = new FPObjectState(AttackStats_Blink);

            if (Mathf.Repeat(genericTimer, 4f) < 1f)
            {
                FPStage.CreateStageObject(Sparkle.classID, position.x + UnityEngine.Random.Range(-24f, 24f), position.y + UnityEngine.Random.Range(-24f, 24f));
            }

            if (direction == FPDirection.FACING_LEFT)
            {
                velocity.x = Mathf.Min(Mathf.Min(velocity.x, 0f) * 0.5f - 6f, velocity.x);
                velocity.y = 0f;
            }
            else
            {
                velocity.x = Mathf.Max(Mathf.Max(velocity.x, 0f) * 0.5f + 6f, velocity.x);
                velocity.y = 0f;
            }

            if (input.up)
            {
                velocity.y += 7.5f * FPStage.deltaTime;
                if (direction == FPDirection.FACING_RIGHT)
                    angle = 10f;
                else
                    angle = -10f;
            }
            else if (input.down)
            {
                velocity.y -= 7.5f * FPStage.deltaTime;
                if (direction == FPDirection.FACING_RIGHT)
                    angle = -10f;
                else
                    angle = 10f;
            }

            energy -= 1.5f * FPStage.deltaTime;
            Process360Movement();

            if (onGround || colliderWall != null || genericTimer >= 40f || energy <= 0f)
            {
                hbAttack.enabled = false;
                superArmor = false;
                flashTime = 0f;
                if (onGround)
                {
                    state = new FPObjectState(State_Running);
                }
                else
                {
                    SetPlayerAnimation("Jumping_Loop");
                    state = new FPObjectState(State_Running);
                }
                attackStats = new FPObjectState(AttackStats_Idle);
            }
        }

        //AttackStats

        private new void AttackStats_Idle()
        {
            attackPower = 2f;
            attackHitstun = 4f;
            attackEnemyInvTime = 5f / animator.speed;
            attackKnockback.x = 0f;
            attackKnockback.y = 0f;
            attackSfx = 5;
        }

        //Actions

        internal void Action_Lightning_NormalShotFire()
        {
            //FPAudio.PlaySfx(sfxFire);
            Action_PlaySound(sfxFire);
            ProjectileBasic basicShot;
            if (direction == FPDirection.FACING_LEFT)
            {
                basicShot = (ProjectileBasic)FPStage.CreateStageObject(ProjectileBasic.classID, chargeFX.transform.position.x, chargeFX.transform.position.y);
                basicShot.velocity.x = Mathf.Cos(0.017453292f * angle) * -15f;
                basicShot.velocity.y = Mathf.Sin(0.017453292f * angle) * -15f;
            }
            else
            {
                basicShot = (ProjectileBasic)FPStage.CreateStageObject(ProjectileBasic.classID, chargeFX.transform.position.x, chargeFX.transform.position.y);
                basicShot.velocity.x = Mathf.Cos(0.017453292f * angle) * 15f;
                basicShot.velocity.y = Mathf.Sin(0.017453292f * angle) * 15f;
            }
            basicShot.animatorController = baseProjectile;
            basicShot.animator = basicShot.GetComponent<Animator>();
            basicShot.animator.runtimeAnimatorController = basicShot.animatorController;
            basicShot.attackPower = 5f;
            basicShot.direction = direction;
            basicShot.angle = angle;
            basicShot.scale = new Vector3(1, 1, 1);
            basicShot.damageElementType = -1;
            basicShot.explodeType = FPExplodeType.WHITEBURST;
            basicShot.ignoreTerrain = false;
            basicShot.ignoreInvincibility = false;
            basicShot.explodeTimer = 50f;
            basicShot.terminalVelocity = 0f;
            basicShot.gravityStrength = 0;
            basicShot.sfxExplode = null;
            basicShot.parentObject = this;
            basicShot.faction = faction;
            basicShot.timeBeforeCollisions = 0f;
            basicShot.halfHeight = 4;
            basicShot.halfWidth = 8;
        }

        internal void Action_Lightning_ChargedShotFire()
        {
            ProjectileBasic chargeShot;
            if (direction == FPDirection.FACING_LEFT)
            {
                chargeShot = (ProjectileBasic)FPStage.CreateStageObject(ProjectileBasic.classID, chargeFX.transform.position.x, chargeFX.transform.position.y);
                chargeShot.velocity.x = Mathf.Cos(0.017453292f * angle) * -10f;
                chargeShot.velocity.y = Mathf.Sin(0.017453292f * angle) * -10f;
            }
            else
            {
                chargeShot = (ProjectileBasic)FPStage.CreateStageObject(ProjectileBasic.classID, chargeFX.transform.position.x, chargeFX.transform.position.y);
                chargeShot.velocity.x = Mathf.Cos(0.017453292f * angle) * 10f;
                chargeShot.velocity.y = Mathf.Sin(0.017453292f * angle) * 10f;
            }
            chargeShot.animatorController = fullChargeProjectile;
            chargeShot.ignoreTerrain = true;
            chargeShot.halfHeight = 20;
            chargeShot.halfWidth = 24;
            chargeShot.animator = chargeShot.GetComponent<Animator>();
            chargeShot.animator.runtimeAnimatorController = chargeShot.animatorController;
            chargeShot.attackPower = 10;
            chargeShot.direction = direction;
            chargeShot.angle = angle;
            chargeShot.damageElementType = 3;
            chargeShot.explodeType = FPExplodeType.EXPLOSION;
            chargeShot.ignoreInvincibility = true;
            chargeShot.destroyOnHit = false;
            chargeShot.explodeTimer = 100f;
            chargeShot.terminalVelocity = 0f;
            chargeShot.gravityStrength = 0;
            chargeShot.parentObject = this;
            chargeShot.faction = faction;
            chargeShot.timeBeforeCollisions = 0f;
            FPAudio.PlaySfx(sfxBigFire);
        }

        //Other Stuff

        internal void NoAuraFarming()
        {
            if (transform.GetChild(0).gameObject.activeSelf)
                transform.GetChild(0).gameObject.SetActive(false);
            if (transform.GetChild(1).gameObject.activeSelf)
                transform.GetChild(1).gameObject.SetActive(false);
        }
    }
}
