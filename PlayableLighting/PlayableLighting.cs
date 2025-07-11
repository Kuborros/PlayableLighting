using BepInEx;
using BepInEx.Logging;
using FP2Lib.Badge;
using FP2Lib.Player;
using FP2Lib.Vinyl;
using HarmonyLib;
using PlayableLighting.Patches;
using System.IO;
using UnityEngine;

namespace PlayableLighting
{
    [BepInPlugin("com.kuborro.plugins.fp2.playablelighting", "Lighting", "1.0.0")]
    [BepInDependency("000.kuborro.libraries.fp2.fp2lib")]
    public class PlayableLighting : BaseUnityPlugin
    {
        internal static ManualLogSource logSource;

        public static AssetBundle dataBundle;
        public static AssetBundle tutorialScene;

        internal static FPCharacterID currentLightingID;

        private void Awake()
        {
            logSource = Logger;

            //Load AssetBundles
            string assetPath = Path.Combine(Path.GetFullPath("."), "mod_overrides\\LightingMod");
            dataBundle = AssetBundle.LoadFromFile(Path.Combine(assetPath, "playablelighting.assets"));
            //tutorialScene = AssetBundle.LoadFromFile(Path.Combine(assetPath, "tutoriallighting.scene"));

            if (dataBundle == null) //|| tutorialScene == null)
            {
                logSource.LogError("Failed to load AssetBundles! This mod cannot work without them, exiting. Please reinstall it.");
                return;
            }

            //Initialise music
            AudioClip lightingClear = dataBundle.LoadAsset<AudioClip>("M_Clear_Lighting");
            AudioClip lightingTheme = dataBundle.LoadAsset<AudioClip>("M_Theme_Lighting");

            //Add Vinyls
            VinylHandler.RegisterVinyl("kubo.m_clear_lighting", "Results - Lighting", lightingClear, VAddToShop.Naomi);
            VinylHandler.RegisterVinyl("kubo.m_theme_lighting", "Lighting's Theme", lightingTheme, VAddToShop.Fawnstar);

            //Add Badges
            //BadgeHandler.RegisterBadge("kubo.lightingrunner", "Winged Runner", "Beat any stage's par time as Lighting.", dataBundle.LoadAssetWithSubAssets<Sprite>("Lighting_Badges")[0], FPBadgeType.SILVER);
            //BadgeHandler.RegisterBadge("kubo.lightingspeedrunner", "Winged Speedrunner", "Beat any stage as Lighting in less than half of the par time.", dataBundle.LoadAssetWithSubAssets<Sprite>("Lighting_Badges")[1], FPBadgeType.SILVER);
            //BadgeHandler.RegisterBadge("kubo.lightingmaster", "Winged Master", "Beat the par times in all stages as Lighting.", dataBundle.LoadAssetWithSubAssets<Sprite>("Lighting_Badges")[2], FPBadgeType.GOLD);
            //BadgeHandler.RegisterBadge("kubo.lightingcomplete", "Future Preserved", "Finish the game as Lighting.", dataBundle.LoadAssetWithSubAssets<Sprite>("Lighting_Badges")[3], FPBadgeType.GOLD);

            //Load character select object
            GameObject lightingWheel = dataBundle.LoadAsset<GameObject>("Menu CS Character Lighting");

            PlayableChara lightingChar = new PlayableChara()
            {
                uid = "com.kuborro.lighting",
                Name = "Lighting",
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
                AirMoves = PatchFPPlayer.Action_Lighting_AirMoves,
                GroundMoves = PatchFPPlayer.Action_Lighting_GroundMoves,
                ItemFuelPickup = PatchFPPlayer.Action_Lighting_FuelPickup,
                eventActivatorCharacter = FPCharacterID.NEERA,
                Gender = CharacterGender.FEMALE,
                element = CharacterElement.METAL,
                powerupStartDescription = "You begin the stage with Charge Gem ready.",
                profilePic = dataBundle.LoadAsset<Sprite>("Lighting_portrait"),
                keyArtSprite = dataBundle.LoadAsset<Sprite>("Lighting_KeyArt"),
                endingKeyArtSprite = dataBundle.LoadAsset<Sprite>("Lighting_KeyArt"),
                charSelectName = dataBundle.LoadAsset<Sprite>("Lighting-File-Select"),
                piedSprite = null,//(Sprite)dataBundle.LoadAssetWithSubAssets("Lighting_Pie")[1],
                piedHurtSprite = null,//(Sprite)dataBundle.LoadAssetWithSubAssets("Lighting_Pie")[2],
                itemFuel = dataBundle.LoadAsset<Sprite>("ItemFuelCrystal"),
                worldMapPauseSprite = dataBundle.LoadAsset<Sprite>("lighting_Pause"),
                zaoBaseballSprite = dataBundle.LoadAsset<Sprite>("LightingZLBall"),
                livesIconAnim = [null, null, null], //dataBundle.LoadAssetWithSubAssets<Sprite>("Lighting_Stock"),
                sagaBlock = dataBundle.LoadAsset<RuntimeAnimatorController>("SagaLighting"),
                sagaBlockSyntax = dataBundle.LoadAsset<RuntimeAnimatorController>("Saga2Lighting"),
                resultsTrack = lightingClear,
                endingTrack = lightingTheme,
                menuPhotoPose = new MenuPhotoPose(),
                characterSelectPrefab = lightingWheel,
                prefab = dataBundle.LoadAsset<GameObject>("Player Lighting"),
                dataBundle = dataBundle
            };

            if (PlayerHandler.RegisterPlayableCharacterDirect(lightingChar))
            {
                currentLightingID = (FPCharacterID)PlayerHandler.GetPlayableCharaByUid(lightingChar.uid).id;
            }
            else
            {
                logSource.LogError("Something went veeryyy wrong when registering the character! Oh no!");
            }

            Harmony harmony = new Harmony("com.kuborro.plugins.fp2.playablelighting");
            harmony.PatchAll(typeof(PatchAnimatorPreInitializer));
            harmony.PatchAll(typeof(PatchFPHudMaster));
            harmony.PatchAll(typeof(PatchFPPlayer));
            harmony.PatchAll(typeof(PatchFPResultsMenu));
            harmony.PatchAll(typeof(PatchFPSaveManager));
            harmony.PatchAll(typeof(PatchMenuClassic));
            harmony.PatchAll(typeof(PatchMenuWorldMap));
        }
    }
}
