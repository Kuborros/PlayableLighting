using BepInEx;
using BepInEx.Logging;
using FP2Lib.Badge;
using FP2Lib.Item;
using FP2Lib.Player;
using FP2Lib.Vinyl;
using HarmonyLib;
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

        internal static FPCharacterID currentLightningID;
        internal static FPPowerup fastLaddersID;

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
            ItemHandler.RegisterItem("kubo.fast_ladders","Step Booster", stepBooster, "Lets you climb ladders faster!");

            //Sprites
            MenuPhotoPose menuPhotoPose = new MenuPhotoPose();

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
                bfImpaleSprite = dataBundle.LoadAsset<Sprite>("Lightning_KOFront"),
                livesIconAnim = dataBundle.LoadAssetWithSubAssets<Sprite>("Lightning_HudIcon"),
                sagaBlock = dataBundle.LoadAsset<RuntimeAnimatorController>("SagaLightning"),
                sagaBlockSyntax = dataBundle.LoadAsset<RuntimeAnimatorController>("Saga2Lightning"),
                resultsTrack = lightningClear,
                endingTrack = lightningTheme,
                playerBoss = null,
                menuPhotoPose = menuPhotoPose,
                characterSelectPrefab = dataBundle.LoadAsset<GameObject>("Menu CS Character Lightning"),
                menuInstructionPrefab = dataBundle.LoadAsset<GameObject>("MenuInstructionsLightning"),
                prefab = dataBundle.LoadAsset<GameObject>("Player Lightning"),
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
        }
    }
}
