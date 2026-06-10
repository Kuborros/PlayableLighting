using BepInEx;
using BepInEx.Logging;
using FP2Lib.Badge;
using FP2Lib.Challenge;
using FP2Lib.Item;
using FP2Lib.Player;
using FP2Lib.Vinyl;
using HarmonyLib;
using PlayableLightning.Objects;
using PlayableLightning.Patches;
using System.IO;
using UnityEngine;

namespace PlayableLightning
{
    [BepInPlugin("com.kuborro.plugins.fp2.playablelightning", "Lightning", "0.3.0.0")]
    [BepInDependency("000.kuborro.libraries.fp2.fp2lib")]
    public class PlayableLightning : BaseUnityPlugin
    {
        internal static ManualLogSource logSource;

        public static AssetBundle dataBundle;
        public static AssetBundle tutorialScene;

        public static GameObject bossLightning;

        internal static FPCharacterID currentLightningID;
        internal static FPPowerup fastLaddersID;
        internal static int bossLightningID;

        private void Awake()
        {
            logSource = Logger;

            //Load AssetBundles
            string assetPath = Path.Combine(Path.GetFullPath("."), "mod_overrides\\LightningMod");
            dataBundle = AssetBundle.LoadFromFile(Path.Combine(assetPath, "playablelightning.assets"));
            //tutorialScene = AssetBundle.LoadFromFile(Path.Combine(assetPath, "tutoriallightning.scene"));

            if (dataBundle == null) //|| tutorialScene == null)
            {
                logSource.LogError("Failed to load AssetBundles! This mod cannot work without them, exiting. Please reinstall it.");
                return;
            }

            //Initialise music
            AudioClip lightningClear = dataBundle.LoadAsset<AudioClip>("m_results_lightning");
            AudioClip lightningTheme = dataBundle.LoadAsset<AudioClip>("m_theme_lightning");

            //Add Vinyls
            VinylHandler.RegisterVinyl("kubo.m_clear_lightning", "Results - Lightning", lightningClear, VAddToShop.Naomi);
            VinylHandler.RegisterVinyl("kubo.m_theme_lightning", "Lightning's Theme", lightningTheme, VAddToShop.Fawnstar);

            //Add Badges
            BadgeHandler.RegisterBadge("kubo.lightningrunner", "Winged Runner", "Beat any stage's par time as Lightning.", null, FPBadgeType.SILVER);
            BadgeHandler.RegisterBadge("kubo.lightningspeedrunner", "Winged Speedrunner", "Beat any stage as Lightning in less than half of the par time.", null, FPBadgeType.SILVER);
            BadgeHandler.RegisterBadge("kubo.lightningmaster", "Winged Master", "Beat the par times in all stages as Lightning.", null, FPBadgeType.GOLD);
            BadgeHandler.RegisterBadge("kubo.lightningcomplete", "Future Preserved", "Finish the game as Lightning.", null, FPBadgeType.GOLD);

            //Add Items
            Sprite stepBooster = dataBundle.LoadAsset<Sprite>("StepBooster");
            if (ItemHandler.RegisterItem("kubo.fast_ladders","Step Booster", stepBooster, "Lets you climb ladders faster!"))
                fastLaddersID = (FPPowerup)ItemHandler.GetItemDataByUid("kubo.fast_ladders").itemID;

            //Sprites
            MenuPhotoPose menuPhotoPose = new MenuPhotoPose {
                airSprites = dataBundle.LoadAssetWithSubAssets<Sprite>("Lightning_CameraAir"),
                groundSprites = dataBundle.LoadAssetWithSubAssets<Sprite>("Lightning_CameraGround")
            };

            //Load Lightning
            GameObject playerLightning = dataBundle.LoadAsset<GameObject>("Player Lightning");
            //Assemble the Boss
            AssembleLightningBoss(playerLightning);

            //Load character select object
            PlayableChara lightningChar = new PlayableChara()
            {
                Name = "Lightning",
                uid = "com.kuborro.lightning",
                TutorialScene = "Tutorial1",
                characterType = "RANGED Type",
                skill1 = "Fly",
                skill2 = "Double Jump",
                skill3 = "Shoot",
                skill4 = "Guard",
                airshipSprite = 1,
                useOwnCutsceneActivators = false,
                enabledInAventure = false,
                enabledInClassic = true,
                AirMoves = PatchFPPlayer.Action_Lightning_AirMoves,
                GroundMoves = PatchFPPlayer.Action_Lightning_GroundMoves,
                ItemFuelPickup = PatchFPPlayer.Action_Lightning_FuelPickup,
                eventActivatorCharacter = FPCharacterID.NEERA,
                Gender = CharacterGender.FEMALE,
                element = CharacterElement.METAL,
                statDefaultAcceleration = 0.09f,
                statDefaultDeceleration = 0.09f,
                statDefaultTopSpeed = 7.5f,
                statDefaultAirAcceleration = 0.22125f,
                statDefaultJumpRelease = 4.5f,
                statDefaultJumpStrength = 10.5f,
                powerupStartDescription = "You begin the stage with Charge Gem ready.",
                profilePic = dataBundle.LoadAssetWithSubAssets<Sprite>("Lightning_Profile")[0],
                keyArtSprite = dataBundle.LoadAsset<Sprite>("Lightning_KeyArt"),
                endingKeyArtSprite = dataBundle.LoadAsset<Sprite>("Lightning_EndingArt"),
                charSelectName = dataBundle.LoadAsset<Sprite>("Lightning-File-Select"),
                piedSprite = (Sprite)dataBundle.LoadAssetWithSubAssets("Lightning_Pied")[0],
                piedHurtSprite = (Sprite)dataBundle.LoadAssetWithSubAssets("Lightning_Pied")[1],
                itemFuel = dataBundle.LoadAsset<Sprite>("Lightning_ItemFuel"),
                worldMapPauseSprite = dataBundle.LoadAssetWithSubAssets<Sprite>("Lightning_VictoryLoop")[0],
                zaoBaseballSprite = dataBundle.LoadAsset<Sprite>("Lightning_ZaoBall"),
                bfImpaleSprite = dataBundle.LoadAsset<Sprite>("Lightning_BakuStab"),
                livesIconAnim = dataBundle.LoadAssetWithSubAssets<Sprite>("Lightning_HudIconNew"),
                sagaBlock = dataBundle.LoadAsset<RuntimeAnimatorController>("SagaLightning"),
                sagaBlockSyntax = dataBundle.LoadAsset<RuntimeAnimatorController>("Saga2Lightning"),
                resultsTrack = lightningClear,
                endingTrack = lightningTheme,
                playerBoss = bossLightning.GetComponent<PlayerBossLightning>(),
                menuPhotoPose = menuPhotoPose,
                characterSelectPrefab = dataBundle.LoadAsset<GameObject>("Menu CS Character Lightning"),
                menuInstructionPrefab = dataBundle.LoadAsset<GameObject>("MenuInstructionsLightning"),
                prefab = playerLightning,
                dataBundle = dataBundle
            };

            if (PlayerHandler.RegisterPlayableCharacterDirect(lightningChar))
            {
                currentLightningID = (FPCharacterID)PlayerHandler.GetPlayableCharaByUid(lightningChar.uid).id;
            }
            else
            {
                logSource.LogError("Something went veeryyy wrong when registering the character! Oh no!");
            }

            if (ChallengeHandler.RegisterDojoBoss("kubo.lightningboss","Lightning",100,-1,"???",currentLightningID, dataBundle.LoadAsset<Sprite>("Lightning_Boss_Pic")))
            {
                bossLightningID = ChallengeHandler.GetChallengeDataByUID("kubo.lightningboss").id;
            }

            Harmony harmony = new Harmony("com.kuborro.plugins.fp2.playablelightning");
            harmony.PatchAll(typeof(PatchAnimatorPreInitializer));
            harmony.PatchAll(typeof(PatchFPHudMaster));
            harmony.PatchAll(typeof(PatchFPLadderTrigger));
            harmony.PatchAll(typeof(PatchFPPlayer));
            harmony.PatchAll(typeof(PatchFPResultsMenu));
            harmony.PatchAll(typeof(PatchFPSaveManager));
            harmony.PatchAll(typeof(PatchMenuClassic));
            harmony.PatchAll(typeof(PatchMenuWorldMap));
            harmony.PatchAll(typeof(PatchFPEventSequence));
            harmony.PatchAll(typeof(PatchMenuCredits));
            harmony.PatchAll(typeof(PatchArenaSpawner));
        }

        private void AssembleLightningBoss(GameObject playerLightning)
        {
            //Player Boss
            //EVIL fuckery
            GameObject dataSource = dataBundle.LoadAsset<GameObject>("Boss Lightning");
            FPPlayer pboss = playerLightning.GetComponent<FPPlayer>();

            bossLightning = new GameObject();
            bossLightning.SetActive(false);
            bossLightning.name = "Boss Lightning";
            bossLightning.layer = 8; //FG PLANE A
            bossLightning.transform.position = new Vector3(514, -336, -2);

            bossLightning.AddComponent<PlayerBossLightning>();
            PlayerBossLightning component = bossLightning.GetComponent<PlayerBossLightning>();
            component.enablePhysics = true;
            component.terrainCollision = true;
            component.playerTerrainCheck = true;
            component.halfWidth = 16;
            component.halfHeight = 32;
            component.useScaling = true;
            component.useRotation = true;
            component.direction = FPDirection.FACING_LEFT;
            component.activationMode = FPActivationMode.XY_RANGE;
            component.activationRange = new Vector2(1600, 1600);
            component.interactWithObjects = true;
            component.health = 200;
            component.hbWeakpoint = new FPHitBox { left = -5, top = 32, right = 25, bottom = -32, enabled = true, visible = true };
            component.topSpeed = 7.5f;
            component.acceleration = 0.09f;
            component.deceleration = 0.09f;
            component.airAceleration = 0.22125f;
            component.skidDeceleration = 0.75f;
            component.skidThreshold = 11;
            component.gravityStrength = -0.375f;
            component.jumpStrength = 10.5f;
            component.jumpRelease = 4.5f;
            component.fightStanceTime = 200;
            component.bossActivation = new Vector2(800, 800);
            component.characterID = BossCharacterID.SPADE;
            component.recoverAfterKO = true;
            component.pursuitRange = 800;
            component.walkRange = new FPHitBox { left = -300, top = -32, right = 200, bottom = -48, enabled = true, visible = true };
            component.start = new Vector2(514, -336);

            component.sfxJump = pboss.sfxJump;
            component.sfxSkid = pboss.sfxSkid;
            component.sfxHurt = pboss.sfxHurt;
            component.sfxKO = pboss.sfxKO;
            component.sfxShieldBlock = pboss.sfxShieldBlock;
            component.sfxShieldHit = pboss.sfxShieldHit;

            component.vaAttack = [null];
            component.vaHardAttack = pboss.vaHardAttack;
            component.vaSpecialA = pboss.vaSpecialA;
            component.vaSpecialB = [null];
            component.vaHit = pboss.vaHit;
            component.vaKO = pboss.vaKO;
            component.vaRevive = pboss.vaRevive;
            component.vaStart = [null];
            component.vaExtra = [null];

            bossLightning.AddComponent<SpriteOutline>();
            bossLightning.GetComponent<SpriteOutline>().enabled = false;
            bossLightning.GetComponent<SpriteOutline>().color = new Color(0, 139, 255, 255);
            bossLightning.GetComponent<SpriteOutline>().outlineSize = 1;

            bossLightning.AddComponent<SpriteRenderer>();
            bossLightning.GetComponent<SpriteRenderer>().sprite = dataBundle.LoadAssetWithSubAssets<Sprite>("Lightning_Idle")[0];
            bossLightning.GetComponent<SpriteRenderer>().material = dataSource.GetComponent<SpriteRenderer>().material;
            bossLightning.GetComponent<SpriteRenderer>().sortingOrder = 3;

            bossLightning.AddComponent<Animator>();
            bossLightning.GetComponent<Animator>().runtimeAnimatorController = dataBundle.LoadAsset<RuntimeAnimatorController>("Lightning Animator Player");

            bossLightning.AddComponent<FPBossHud>();
            bossLightning.GetComponent<FPBossHud>().maxPetals = 6;
            bossLightning.GetComponent<FPBossHud>().barWidth = 200;
            bossLightning.GetComponent<FPBossHud>().barSprite = dataSource.GetComponent<FPBossHud>().barSprite;
            bossLightning.GetComponent<FPBossHud>().pfHudBase = dataSource.GetComponent<FPBossHud>().pfHudBase;
            bossLightning.GetComponent<FPBossHud>().pfHudLifePetal = dataSource.GetComponent<FPBossHud>().pfHudLifePetal;

            GameObject dashAura = new GameObject();
            dashAura.name = "DashAura";
            dashAura.layer = 8;
            dashAura.transform.parent = bossLightning.transform;
            dashAura.AddComponent<SpriteRenderer>();
            dashAura.AddComponent<Animator>();
            dashAura.GetComponent<Animator>().runtimeAnimatorController = dataBundle.LoadAsset<RuntimeAnimatorController>("DashAura");
            dashAura.SetActive(false);

            GameObject chargeFX = new GameObject();
            chargeFX.name = "ChargeFX";
            chargeFX.layer = 8;
            chargeFX.transform.parent = bossLightning.transform;
            chargeFX.transform.localPosition = new Vector3(27, -3, 0);
            chargeFX.AddComponent<SpriteRenderer>();
            chargeFX.AddComponent<Animator>();
            chargeFX.GetComponent<Animator>().runtimeAnimatorController = dataBundle.LoadAsset<RuntimeAnimatorController>("ChargeFX");
            chargeFX.SetActive(false);

            DontDestroyOnLoad(bossLightning);
        }

    }
}
