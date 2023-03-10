using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Rewired;
using RisingSlash.FP2Mods.MillasToybox.GeneralScripts;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace RisingSlash.FP2Mods.MillasToybox
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("FP2.exe")]
    public class MillasToybox : BaseUnityPlugin
    {
        private ConfigEntry<bool> configDisplayGreeting;
        
        private ConfigEntry<string> configLastKnownScene;
        
        private string activeSceneName = "";
        private string previousSceneName = "";
        public static bool bSceneChanged = false;

        public static ManualLogSource sLogger;
        public static ConfigFile sConfig;
        
        public static GameObject goFP2Trainer;
        public static PlaneSwitcherVisualizer planeSwitcherVisualizer;
        public static CameraHotspotVisualizer cameraHotspotVisualizer;
        public static GameObject goFP2TrainerYourPlayerIndicator;
        
        public static DateTime fp2ReleaseDate = new DateTime(2022, 9, 13, 12, 0, 0);
        public static int fp2ReleaseDateInt = 20220913;

        public static bool hasInitialized = false;

        public enum DataPage
        {
            MOVEMENT,
            MOVEMENT_2,
            MOVEMENT_3,
            FPPlayerCustom,
            COMBAT,
            DPS,
            DPS_ALL,
            MULTIPLAYER_DEBUG,
            BOSS,
            NO_CLIP,
            CAMERA,
            CAMERA_ALL,
            LIST_ACTIVES,
            LIST_ACTIVES_PUSHERS,
            NONE
        }

        public enum InstructionPage
        {
            BASICS,
            BACKUPS,
            SPEEDRUN,
            QUICK_RESTART,
            NO_CLIP,
            MULTICHARACTER,
            //CHAR_INSTASWAP,

            //NETPLAY,
            BUGS,
            HOTKEYS_1,
            HOTKEYS_2,
            HOTKEYS_3,
            HOTKEYS_4,
            HOTKEYS_5,
            HOTKEYS_6,
            HOTKEYS_7,
            HOTKEYS_8,
            //HOTKEYS_9,
            //HOTKEYS_10,
            //HOTKEYS_11,
            QUICKBOOT,
            NONE
        }

        public static ConfigEntry<bool> enableWarps;
        public static ConfigEntry<bool> showDebug;
        public static ConfigEntry<bool> showLevelEditDebug;
        public static ConfigEntry<bool> enableNoClip;

        public static ConfigEntry<string> BootupLevel;

        public static ConfigEntry<string> PHKToggleInstructions;

        public static ConfigEntry<string> PHKSetWarpPoint;
        public static ConfigEntry<string> PHKGotoWarpPoint;

        public static ConfigEntry<string> PHKKOCharacter;
        public static ConfigEntry<string> PHKKOBoss;
        
        public static ConfigEntry<string> PHKInvinciblePlayers;
        public static ConfigEntry<string> PHKInvincibleBoss;

        public static ConfigEntry<string> PHKToggleNoClip;

        public static ConfigEntry<string> PHKSpawnExtraChar;
        public static ConfigEntry<string> PHKSwapBetweenSpawnedChars;
        public static ConfigEntry<string> PHKToggleMultiCharStart;
        public static ConfigEntry<string> PHKCyclePreferredAllyControlType;
        public static ConfigEntry<string> PHKStartSplitscreen;
        public static ConfigEntry<bool> EnableGetPlayerInstanceMultiplayerPatch;
        public static ConfigEntry<bool> EnableSplitScreen;
        
        public static ConfigEntry<string> PHKSwitchCurrentPlayerToLilac;
        public static ConfigEntry<string> PHKSwitchCurrentPlayerToCarol;
        public static ConfigEntry<string> PHKSwitchCurrentPlayerToCarolBike;
        public static ConfigEntry<string> PHKSwitchCurrentPlayerToMilla;
        public static ConfigEntry<string> PHKSwitchCurrentPlayerToNeera;
        public static ConfigEntry<string> PHKSwitchCurrentPlayerToNext;
        public static ConfigEntry<string> PHKSwitchCurrentPlayerToPrev;
        public static ConfigEntry<bool> UseInstaSwitch;
        
        public static ConfigEntry<string> PHKStartInputPlayback;
        public static ConfigEntry<string> PHKToggleLockP1ToGhostFiles;
        public static ConfigEntry<string> PHKToggleLockP2ToGhostFiles;
        public static ConfigEntry<string> PHKToggleLockP3ToGhostFiles;
        public static ConfigEntry<string> PHKToggleLockP4ToGhostFiles;

        public static ConfigEntry<string> PHKGetOutGetOutGetOut;

        public static ConfigEntry<string> PHKCameraZoomIn;
        public static ConfigEntry<string> PHKCameraZoomOut;
        public static ConfigEntry<string> PHKCameraZoomReset;

        public static ConfigEntry<string> PHKShowNextDataPage;
        public static ConfigEntry<string> PHKShowPreviousDataPage;
        public static ConfigEntry<string> PHKHideDataView;

        public static ConfigEntry<string> PHKGoToMainMenu;
        public static ConfigEntry<string> PHKLoadDebugRoom;

        public static ConfigEntry<string> PHKGoToLevelSelectMenu;

        public static ConfigEntry<string> PHKLoadAssetBundles;

        //public static ConfigEntry<string> PHKTogglePauseMenuOrTrainerMenu;
        public static ConfigEntry<string> PHKGoToLevelAtLastIndex;

        public static ConfigEntry<string> PHKIncreaseFontSize;
        public static ConfigEntry<string> PHKDecreaseFontSize;

        public static ConfigEntry<string> PHKReturnToCheckpoint;
        public static ConfigEntry<string> PHKRestartStage;

        public static ConfigEntry<string> PHKTogglePlaneSwitcherVisualizers;
        public static ConfigEntry<string> PHKToggleShowColliders;
        
        
        public static ConfigEntry<string> PHKDoSpeedBoost;
        public static ConfigEntry<string> PHKLaunchPhantomRemote;
        public static ConfigEntry<bool> EnablePhantomRemotePipes;
        public static ConfigEntry<float> SpeedBoostValue;
        public static ConfigEntry<bool> LaunchPhantomRemoteOnStart;

        public static ConfigEntry<string> PHKToggleRecordGhostData;
        public static ConfigEntry<string> PHKToggleEnableNetworkPlayers;

        public static ConfigEntry<string> PHKRebindAllHotkeys;
        
        public static ConfigEntry<string> PHKStartVersus;


        public static ConfigEntry<bool> MultiCharStartLastSetting;
        public static ConfigEntry<bool> VersusMultiplayerStart;
        public static ConfigEntry<bool> ShowPlaneSwitcherVisualizersLastSetting;
        public static ConfigEntry<bool> ShowCollidersLastSetting;
        public static ConfigEntry<string> PreferredAllyControlTypeLastSetting;
        public static ConfigEntry<bool> ShowInstructionsOnStart;
        public static ConfigEntry<bool> ShowInputNamesInTerminal;
        public static ConfigEntry<bool> EnableNetworking;
        public static ConfigEntry<bool> SaveGhostFiles;
        public static ConfigEntry<int> MultiCharStartNumChars;

        public static ConfigEntry<bool> LockP1ToGhostFiles;
        public static ConfigEntry<string> DEBUG_LoadSpecificGhostFile;
        public static ConfigEntry<string> DEBUG_LoadSpecificGhostFileP2;
        public static ConfigEntry<string> DEBUG_LoadSpecificGhostFileP3;
        public static ConfigEntry<string> DEBUG_LoadSpecificGhostFileP4;
        
        public static ConfigEntry<bool> EnableInvinciblePlayers;
        public static ConfigEntry<bool> EnableInvincibleBoss;
        
        public static ConfigEntry<bool> SixtyFPSHack;
        public static ConfigEntry<bool> DeterministicMode;
        
        public static ConfigEntry<bool> DisplayNametags;

        public static ConfigEntry<float> FontOpacity;
        public static ConfigEntry<string> DataDisplayCustomPageFields;

        public static bool hotkeysLoaded = false;

        //public static ConfigEntry<string> PHK;

        // public static ConfigEntry<string> PHKSaveTrainerData;
        // public static ConfigEntry<string> PHKLoadTrainerData;


        public static MillasToybox millasToyboxInstance;

        private static float fp2tDeltaTime;
        //private InputHandler inputHandler = null;

        public static int introSkipped = 0;

        public static Font fpMenuFont;
        public static Material fpMenuMaterial;

        public Dictionary<int, float> allActiveEnemiesHealth;
        public Dictionary<int, float> allActiveEnemiesHealthPrevious;
        public Dictionary<int, string> allActiveEnemiesNames;
        private List<FPBossHud> bossHuds;

        public bool noClip;
        public float noClipMoveSpeed = 30f;
        public Vector2 noClipStartPos = Vector2.zero;
        public int noClipCollisionLayer = -0;
        public float noClipGravityStrength = -0.7f;

        private DataPage currentDataPage = DataPage.MOVEMENT;
        private InstructionPage currentInstructionPage = InstructionPage.BASICS;
        public static bool showInstructions = true;

        public static string debugDisplay = "Never Updated";

        public float dps;
        public List<float> dpsHits;
        public double dpsTimer;

        public FP2TrainerDPSTracker dpsTracker;

        public Dictionary<int, string> fpElementTypeNames;
        private List<FPBaseEnemy> fpEnemies;

        public static FPPlayer fpplayer;
        public static List<FPPlayer> fpplayers;

        private FPTrainerLevelSelect fptls;

        private GameObject goDtTracker;

        public static GameObject goFancyTextPosition;
        public GameObject goStageHUD;
        private readonly float howLongToShowWarpInfo = 2f;

        public static List<AssetBundle> loadedAssetBundles;

        private FPBaseEnemy nearestEnemy;
        private FPBaseEnemy nearestEnemyPrevious;
        private float nearestEnemyPreviousHP;
        private FPPauseMenu pauseMenu;

        private GameObject player;

        private HashSet<string> playerValuesToShow;
        private List<FPHudDigit> positionDigits;

        public string sceneToLoad = "";

        public static bool multiplayerStart = false;
        public static bool doneMultiplayerStart = false;

        public bool showVarString = true;
        private GameObject stageHUD;
        private GameObject stageSelectMenu;
        public TextMesh textmeshFancyTextPosition;
        public static TextMesh cloneMeForText;

        private float timeoutShowWarpInfo;

        private static bool warped;
        private string warpMessage = "";

        private Vector2 warpPoint = new Vector2(211f, 50f);

        public float trainerZoomMin = 0.05f;
        public float trainerZoomMax = 10f;
        public float trainerZoomSpeed = 0.1f;
        public float trainerRequestZoomValue = 1f;

        public float originalZoomMin = 0.5f;
        public float originalZoomMax = 2f;
        public float originalZoomSpeed = 0.1f;
        public GameObject lifePetal;
        public GameObject shield;

        public static Dictionary<string, KeyMapping> customHotkeys;

        public bool planeSwitchVisualizersCreated = false;
        public bool planeSwitchVisualizersVisible = false;
        public List<GameObject> planeSwitchVisualizers;

        public static GameObject cacheGameObjectHunter = null;

        public static bool waitingForNextFrameForSpoilerGimmick = false;

        public FPStage fpStage;
        public FP2TrainerInputQueue p1inputQueue;

        public static bool skipRecording = false;

        public static FPCharacterID currentPreferredCharacter = FPCharacterID.LILAC;

        public static Dictionary<int, string> FPLayerNames;

        public static List<SplitScreenCamInfo> SplitScreenCameraInfos = new List<SplitScreenCamInfo>();

        private bool instaSwitchCharsSpawned = false;
        
        public Vector2 spawnVelocity = Vector2.zero;

        public void Awake() // Runs after Game Initialization.
        {
            millasToyboxInstance = this;
            sLogger = Logger;
            sConfig = Config;

            sLogger.LogInfo("OnApplicationStart");
            //MelonPreferences.Load();
            InitPrefs();
            sLogger.LogInfo("AfterInitPrefs");

            loadedAssetBundles = new List<AssetBundle>();

            playerValuesToShow = new HashSet<string>();
            playerValuesToShow.Add("Pos");
            playerValuesToShow.Add("Vel");
            playerValuesToShow.Add("Magnitude");
            playerValuesToShow.Add("InflictedDamage");
            playerValuesToShow.Add("Ground Angle");
            playerValuesToShow.Add("Ground Velocity");
            playerValuesToShow.Add("Ceiling Angle");
            playerValuesToShow.Add("Sensor Angle");
            playerValuesToShow.Add("Gravity Angle");
            playerValuesToShow.Add("Gravity Strength");
            playerValuesToShow.Add("HUD Position");

            fpElementTypeNames = new Dictionary<int, string>();
            fpElementTypeNames.Add(-1, "Normal");
            fpElementTypeNames.Add(0, "Wood");
            fpElementTypeNames.Add(1, "Earth");
            fpElementTypeNames.Add(2, "Water");
            fpElementTypeNames.Add(3, "Fire");
            fpElementTypeNames.Add(4, "Metal");

            bossHuds = new List<FPBossHud>();
            allActiveEnemiesHealth = null;
            allActiveEnemiesHealthPrevious = null;
            dpsTracker = new FP2TrainerDPSTracker();
            introSkipped = 0;

            FPPlayer2p.InitCustomControls();

            multiplayerStart = MultiCharStartLastSetting.Value;
            planeSwitchVisualizersVisible = ShowPlaneSwitcherVisualizersLastSetting.Value;
            FP2TrainerAllyControls.preferredAllyControlType = (AllyControlType)(Enum.Parse(typeof(AllyControlType),
                PreferredAllyControlTypeLastSetting.Value));
            showInstructions = ShowInstructionsOnStart.Value;

            InitFPLayerNames();
            sLogger.LogInfo("WakeupComplete");

            var remote = gameObject.AddComponent<FP2TrainerRemoteHandler>();
            remote.Trainer = this;

            var goTemp = new GameObject("RewiredInputTweakTool");
            DontDestroyOnLoad(goTemp);
            goTemp.AddComponent<RewiredInputTweaks>();
        }

        private void InitPrefs()
        {
            enableWarps = Config.Bind("General", "enableWarps", true);
            BootupLevel = Config.Bind("General", "bootupLevel", "");
            showDebug = Config.Bind("General", "showDebug", true);
            enableNoClip = Config.Bind("General", "enableNoClip", false);

            MultiCharStartLastSetting = Config.Bind("General", "MultiCharStartLastSetting", false);
            VersusMultiplayerStart = Config.Bind("General", "VersusMultiplayerStart", true);
            ShowPlaneSwitcherVisualizersLastSetting =
                Config.Bind("General", "ShowPlaneSwitcherVisualizersLastSetting", false);
            ShowCollidersLastSetting = Config.Bind("General", "ShowCollidersLastSetting", true);
            PreferredAllyControlTypeLastSetting = Config.Bind("General", "PreferredAllyControlTypeLastSetting",
                FP2TrainerAllyControls.AllyControlTypeName(AllyControlType.SINGLE_PLAYER));
            ShowInstructionsOnStart = Config.Bind("General", "ShowInstructionsOnStart", true);
            ShowInputNamesInTerminal = Config.Bind("General", "ShowInputNamesInTerminal", false);
            EnableNetworking = Config.Bind("General", "EnableNetworking", false);
            SaveGhostFiles = Config.Bind("General", "SaveGhostFiles", false);
            LockP1ToGhostFiles = Config.Bind("General", "LockP1ToGhostFiles", false);
            MultiCharStartNumChars = Config.Bind("General", "MultiCharStartNumChars", 2);
            DEBUG_LoadSpecificGhostFile = Config.Bind("General", "DEBUG_LoadSpecificGhostFile", "");
            DEBUG_LoadSpecificGhostFileP2 = Config.Bind("General", "DEBUG_LoadSpecificGhostFileP2", "");
            DEBUG_LoadSpecificGhostFileP3 = Config.Bind("General", "DEBUG_LoadSpecificGhostFileP3", "");
            DEBUG_LoadSpecificGhostFileP4 = Config.Bind("General", "DEBUG_LoadSpecificGhostFileP4", "");
            
            SixtyFPSHack = Config.Bind("General", "SixtyFPSHack", false);
            DeterministicMode = Config.Bind("General", "DeterministicMode", false);
            
            EnableInvinciblePlayers = Config.Bind("General", "EnableInvinciblePlayers", false);
            EnableInvincibleBoss = Config.Bind("General", "EnableInvincibleBoss", false);
            
            UseInstaSwitch = Config.Bind("General", "UseInstaSwitch", false);
            EnableSplitScreen = Config.Bind("General", "EnableSplitScreen", false);
            EnableGetPlayerInstanceMultiplayerPatch = Config.Bind("General", "EnableGetPlayerInstanceMultiplayerPatch", false);
            DisplayNametags = Config.Bind("General", "DisplayNametags", false);
            FontOpacity = Config.Bind("General", "FontOpacity", 0.50f);
            DataDisplayCustomPageFields = Config.Bind("General", "DataDisplayCustomPageFields"
                ,"forcefulAccelerationTimer, inputLock, crystalBonus, currentStage.stageName, currentStage.checkpointHasSpecialItem, crystalBonusTimer, idleTimer," +
                 "invincibilityTime, genericTimer, barTimer, ");
            // The only real reason that one defaults to false is because this is still meant to be a trainer.
            // If it gets moved into a standalone mod, it'll be made true by default.
            
            SpeedBoostValue = Config.Bind("General", "SpeedBoostValue", 15f);
            LaunchPhantomRemoteOnStart = Config.Bind("General", "LaunchPhantomRemoteOnStart", false);
            EnablePhantomRemotePipes = Config.Bind("General", "EnablePhantomRemotePipes", false);
            

            InitPrefsCustomHotkeys();
        }

        private static void InitPrefsCustomHotkeys()
        {
            PHKToggleInstructions = CreateEntryAndBindHotkey("PHKToggleInstructions", "F1");

            PHKSetWarpPoint = CreateEntryAndBindHotkey("PHKSetWarpPoint", "Shift+F4");
            PHKGotoWarpPoint = CreateEntryAndBindHotkey("PHKGotoWarpPoint", "F4");

            PHKKOCharacter = CreateEntryAndBindHotkey("PHKKOCharacter", "Shift+F1");
            PHKKOBoss = CreateEntryAndBindHotkey("PHKKOBoss", "Backspace");
            PHKInvincibleBoss = CreateEntryAndBindHotkey("PHKInvincibleBoss", "Shift+Backspace");
            PHKInvinciblePlayers = CreateEntryAndBindHotkey("PHKInvinciblePlayers", "Ctrl+Backspace");

            PHKToggleNoClip = CreateEntryAndBindHotkey("PHKToggleNoClip", "F2");

            PHKSpawnExtraChar = CreateEntryAndBindHotkey("PHKSpawnExtraChar", "F12");
            PHKSwapBetweenSpawnedChars = CreateEntryAndBindHotkey("PHKSwapBetweenSpawnedChars", "F11");
            PHKToggleMultiCharStart = CreateEntryAndBindHotkey("PHKToggleMultiCharStart", "Shift+F12");
            PHKCyclePreferredAllyControlType =
                CreateEntryAndBindHotkey("F", "Shift+F11");
            PHKStartInputPlayback =
                CreateEntryAndBindHotkey("PHKStartInputPlayback", "Insert");
            PHKToggleLockP1ToGhostFiles =
                CreateEntryAndBindHotkey("PHKToggleLockP1ToGhostFiles", "Shift+Insert");
            
            PHKStartSplitscreen = CreateEntryAndBindHotkey("PHKStartSplitscreen", "Slash");
            
            PHKSwitchCurrentPlayerToLilac = CreateEntryAndBindHotkey("PHKSwitchCurrentPlayerToLilac", "Alpha0");
            PHKSwitchCurrentPlayerToCarol = CreateEntryAndBindHotkey("PHKSwitchCurrentPlayerToCarol", "Alpha1");
            PHKSwitchCurrentPlayerToCarolBike = CreateEntryAndBindHotkey("PHKSwitchCurrentPlayerToCarolBike", "Alpha2");
            PHKSwitchCurrentPlayerToMilla = CreateEntryAndBindHotkey("PHKSwitchCurrentPlayerToMilla", "Alpha3");
            PHKSwitchCurrentPlayerToNeera = CreateEntryAndBindHotkey("PHKSwitchCurrentPlayerToNeera", "Alpha4");
            PHKSwitchCurrentPlayerToNext = CreateEntryAndBindHotkey("PHKSwitchCurrentPlayerToNext", "Alpha9");
            PHKSwitchCurrentPlayerToPrev = CreateEntryAndBindHotkey("PHKSwitchCurrentPlayerToPrev", "Alpha8");

            PHKGetOutGetOutGetOut = CreateEntryAndBindHotkey("PHKGetOutGetOutGetOut", "Delete");

            PHKCameraZoomIn = CreateEntryAndBindHotkey("PHKCameraZoomIn", "Plus");
            PHKCameraZoomOut = CreateEntryAndBindHotkey("PHKCameraZoomOut", "Minus");
            PHKCameraZoomReset = CreateEntryAndBindHotkey("PHKCameraZoomReset", "Period");

            PHKShowNextDataPage = CreateEntryAndBindHotkey("PHKShowNextDataPage", "PageDown");
            PHKShowPreviousDataPage = CreateEntryAndBindHotkey("PHKShowPreviousDataPage", "PageUp");
            PHKHideDataView = CreateEntryAndBindHotkey("PHKHideDataView", "Backslash");

            PHKGoToMainMenu = CreateEntryAndBindHotkey("PHKGoToMainMenu", "F7");
            PHKLoadDebugRoom = CreateEntryAndBindHotkey("PHKLoadDebugRoom", "F8");

            PHKGoToLevelSelectMenu = CreateEntryAndBindHotkey("PHKGoToLevelSelectMenu", "F9");

            PHKLoadAssetBundles = CreateEntryAndBindHotkey("PHKLoadAssetBundles", "Shift+F9");
            //PHKTogglePauseMenuOrTrainerMenu = CreateEntryAndBindHotkey("PHKTogglePauseMenuOrTrainerMenu", "F1");
            PHKGoToLevelAtLastIndex = CreateEntryAndBindHotkey("PHKGoToLevelAtLastIndex", "BackQuote");
            PHKIncreaseFontSize = CreateEntryAndBindHotkey("PHKIncreaseFontSize", "Shift+Plus");
            PHKDecreaseFontSize = CreateEntryAndBindHotkey("PHKDecreaseFontSize", "Shift+Minus");

            PHKReturnToCheckpoint = CreateEntryAndBindHotkey("PHKReturnToCheckpoint", "R");
            PHKRestartStage = CreateEntryAndBindHotkey("PHKRestartStage", "Shift+R");

            PHKTogglePlaneSwitcherVisualizers = CreateEntryAndBindHotkey("PHKTogglePlaneSwitcherVisualizers", "F3");
            PHKToggleShowColliders = CreateEntryAndBindHotkey("PHKToggleShowColliders", "Shift+F3");
            
            PHKDoSpeedBoost  = CreateEntryAndBindHotkey("PHKDoSpeedBoost", "Shift+W");
            PHKLaunchPhantomRemote = CreateEntryAndBindHotkey("PHKLaunchPhantomRemote", "Tilde");
            
            PHKStartVersus = CreateEntryAndBindHotkey("PHKStartVersus", "M");


            //PHKNextWarppointSaveSlot = CreateEntryAndBindHotkey("PHKNextWarppointSaveSlot", "F10");
            //PHKPrevWarppointSaveSlot = CreateEntryAndBindHotkey("PHKPrevWarppointSaveSlot", "F9");

            PHKRebindAllHotkeys = CreateEntryAndBindHotkey("PHKRebindAllHotkeys", "Pause");

            hotkeysLoaded = true;
        }

        private static ConfigEntry<string> CreateEntryAndBindHotkey(string identifier,
            string default_value)
        {
            var configEntry = sConfig.Bind("General", identifier, default_value);
            FP2TrainerCustomHotkeys.Add(configEntry);
            return configEntry;
        }

        public static void InitFPLayerNames()
        {
            FPLayerNames = new Dictionary<int, string>();
            int layerAsInt = 0;
            FPLayerNames.Add(layerAsInt, "Default");
            layerAsInt = 1;
            FPLayerNames.Add(layerAsInt, "TransparentFX");
            layerAsInt = 2;
            FPLayerNames.Add(layerAsInt, "Ignore Raycast");
            layerAsInt = 8; // Layer 3 (value =  4) is reserved by Unity but unnamed. Skip to Layer 4.
            FPLayerNames.Add(layerAsInt, "Water");
            layerAsInt = 16;
            FPLayerNames.Add(layerAsInt, "UI");
            
            // Skip two more layers to layer 8. From here, we can rely on powers of 2.
            layerAsInt = 256;
            FPLayerNames.Add(layerAsInt, "FG Plane A");
            layerAsInt *= 2;
            FPLayerNames.Add(layerAsInt, "FG Plane B");
            layerAsInt *= 2;
            FPLayerNames.Add(layerAsInt, "FG Plane C");
            layerAsInt *= 2;
            FPLayerNames.Add(layerAsInt, "FG Plane D");
            layerAsInt *= 2;
            FPLayerNames.Add(layerAsInt, "BG Layer 0");
            layerAsInt *= 2;
            FPLayerNames.Add(layerAsInt, "BG Layer 1");
            layerAsInt *= 2;
            FPLayerNames.Add(layerAsInt, "BG Layer 2");
            layerAsInt *= 2;
            FPLayerNames.Add(layerAsInt, "BG Layer 3");
            layerAsInt *= 2;
            FPLayerNames.Add(layerAsInt, "BG Layer 4");
            layerAsInt *= 2;
            FPLayerNames.Add(layerAsInt, "BG Layer 5");
            layerAsInt *= 2;
            FPLayerNames.Add(layerAsInt, "BG Layer 6");
            layerAsInt *= 2;
            FPLayerNames.Add(layerAsInt, "BG Layer 7");
            layerAsInt *= 2;
            FPLayerNames.Add(layerAsInt, "BG Layer 8");
            layerAsInt *= 2;
            FPLayerNames.Add(layerAsInt, "BG Layer 9");
            layerAsInt *= 2;
            FPLayerNames.Add(layerAsInt, "BG Layer 10");
            layerAsInt *= 2;
            FPLayerNames.Add(layerAsInt, "BG Layer 11");
            layerAsInt *= 2;
            FPLayerNames.Add(layerAsInt, "BG Layer 12");
            layerAsInt *= 2;
            FPLayerNames.Add(layerAsInt, "BG Layer 13");
            layerAsInt *= 2;
            FPLayerNames.Add(layerAsInt, "BG Layer 14");
            layerAsInt *= 2;
            FPLayerNames.Add(layerAsInt, "BG Layer 15");
            layerAsInt *= 2;
            FPLayerNames.Add(layerAsInt, "LightingSetup");
            layerAsInt *= 2;
            FPLayerNames.Add(layerAsInt, "Lighting");
        }

        public void
            OnSceneWasLoaded(int buildindex,
                string sceneName) // Runs when a Scene has Loaded and is passed the Scene's Build Index and Name.
        {
            sLogger.LogInfo("OnSceneWasLoaded: " + buildindex + " | " + sceneName);
            ResetSceneSpecificVariables();
            AttemptToFindFPFont();
            AttemptToFindPauseMenu();
            GrabAndTweakPauseMenu();
            //MelonPreferences.Save();


            if (goDtTracker != null)
            {
            }
            else
            {
                goDtTracker = new GameObject("FP2TrainerDeltaTimeTracker");
                goDtTracker.AddComponent<FP2TrainerDTTracker>();
                Log("Created DeltaTime tracker. Updates will occur on LateUpdate.");
            }
        }

        private void AttemptToFindPauseMenu()
        {
            if (stageSelectMenu == null)
                foreach (var pauseMenu in Resources.FindObjectsOfTypeAll(typeof(FPPauseMenu)) as FPPauseMenu[])
                {
                    this.pauseMenu = pauseMenu;
                    //stageSelectMenu = GameObject.Instantiate(pauseMenu.transform.gameObject);
                    stageSelectMenu = new GameObject("Stage Select Menu");
                    stageSelectMenu.transform.position = new Vector3(-376, -192, 0);
                    var resumeIcon =
                        Object.Instantiate(this.pauseMenu.transform.Find("Pause Icon - Resume").gameObject);
                    if (resumeIcon != null)
                    {
                        resumeIcon.name = "AnnStagePlayIcon";
                        resumeIcon.transform.parent = stageSelectMenu.transform;
                        resumeIcon.transform.localPosition = new Vector3(-112, -64, -4);
                    }

                    Log("Found a pauseMenu to modify.");
                    Log("...But created a new GameObject instead anyway. The frame was annoying.");
                    stageSelectMenu.name = "Ann Stage Select Menu";
                    break;
                }
        }

        private void AttemptToFindFPFont()
        {
            if (fpMenuFont != null) return;

            foreach (var textMesh in Resources.FindObjectsOfTypeAll(typeof(TextMesh)) as TextMesh[])
                if (textMesh.font != null && textMesh.font.name.Equals("FP Menu Font"))
                    //if (textMesh.font!= null && textMesh.font.name.Equals("FP Small Font Light"))
                {
                    Log("Found the FP Menu Font loaded in memory. Saving reference.");
                    //Log("Found the FP Small Font loaded in memory. Saving reference.");
                    fpMenuFont = textMesh.font;
                    fpMenuMaterial = textMesh.GetComponent<MeshRenderer>().materials[0];
                    break;
                }
        }

        private void ResetSceneSpecificVariables()
        {
            player = null;
            fpplayers = new List<FPPlayer>();
            fpplayer = null;
            stageHUD = null;

            dps = 0;
            dpsHits = new List<float>();
            dpsTimer = 0;

            goFancyTextPosition = null;
            goStageHUD = null;
            textmeshFancyTextPosition = null;

            doneMultiplayerStart = false;

            instaSwitchCharsSpawned = false;

            if (goFP2Trainer == null)
            {
                CreateFP2TrainerGameObject();
            }

            if (planeSwitcherVisualizer)
            {
                planeSwitcherVisualizer.Reset();
                planeSwitcherVisualizer.SpawnVisualizers();
                
                cameraHotspotVisualizer.Reset();
                cameraHotspotVisualizer.SpawnVisualizers();
            }

            planeSwitchVisualizersCreated = false;
            planeSwitchVisualizersVisible = ShowPlaneSwitcherVisualizersLastSetting.Value;

            if (FP2TrainerAllyControls.inputQueueForPlayers != null)
            {
                FP2TrainerAllyControls.inputQueueForPlayers.Clear();
            }
            else
            {
                FP2TrainerAllyControls.inputQueueForPlayers = new Dictionary<int, FP2TrainerInputQueue>();
            }

            skipRecording = LockP1ToGhostFiles.Value;
            FP2TrainerAllyControls.needToLoadInputs = LockP1ToGhostFiles.Value;
            
            SplitScreenCameraInfos.Clear();
        }

        private static void CreateFP2TrainerGameObject()
        {
            goFP2Trainer = new GameObject("FP2Trainer");
            GameObject.DontDestroyOnLoad(goFP2Trainer);
            goFP2Trainer.AddComponent<FP2TrainerCustomHotkeys>();
            planeSwitcherVisualizer = goFP2Trainer.AddComponent<PlaneSwitcherVisualizer>();
            cameraHotspotVisualizer = goFP2Trainer.AddComponent<CameraHotspotVisualizer>();
            goFP2Trainer.AddComponent<FP2TrainerCharacterNameTag>();
            if (SixtyFPSHack.Value)
            {
                goFP2Trainer.AddComponent<ForceRenderRate>();
            }
        }

        public static Font GetFPMenuFont()
        {
            return fpMenuFont;
        }

        public void CreateFancyTextObjects()
        {
            goStageHUD = GameObject.Find("Stage HUD");
            //GameObject goStageHUD = GameObject.Find("Hud Pause Menu");
            if (goStageHUD == null) return;

            Log("Successfully found HUD to attach text to.");

            //Why is this here??
            var tempHudMaster = goStageHUD.GetComponent<FPHudMaster>();
            this.lifePetal = tempHudMaster.pfHudLifePetal;
            this.shield = tempHudMaster.pfHudShield;

            goFancyTextPosition = GameObject.Find("Resume Text");
            if (goFancyTextPosition != null)
            {
                Log("Found Resume Text");
                goFancyTextPosition = Object.Instantiate(goFancyTextPosition);
                goFancyTextPosition.SetActive(true);
                textmeshFancyTextPosition = goFancyTextPosition.GetComponent<TextMesh>();
                textmeshFancyTextPosition.font = fpMenuFont;
                textmeshFancyTextPosition.GetComponent<MeshRenderer>().materials[0] = fpMenuMaterial;
                textmeshFancyTextPosition.characterSize = 10;
                textmeshFancyTextPosition.anchor = TextAnchor.UpperLeft;
                textmeshFancyTextPosition.color = new Color(1, 1, 1, FontOpacity.Value);
                Log("Successfully cloned Resume Text. Attaching to Stage HUD.");
            }
            else if (goStageHUD != null)
            {
                //goStageHUD.energyBarGraphic.transform.parent;
                Log("Looking for Energy Bar");
                var temp = goStageHUD.GetComponent<FPHudMaster>();
                GameObject temp2;
                if (temp != null)
                {
                    temp2 = temp.pfHudEnergyBar;
                }
                else
                {
                    Log("This aint it.");
                    return;
                }


                var energyBarGraphic = Object.Instantiate(temp2, temp2.transform.parent);

                energyBarGraphic.transform.localScale *= 2;

                goFancyTextPosition = energyBarGraphic;
                goFancyTextPosition.SetActive(true);
                //GameObject.Destroy(goFancyTextPosition.GetComponent<SpriteRenderer>()); // Can't have Sprite Renderer and Mesh Renderer.
                var tempGo = new GameObject();
                tempGo.transform.parent = goFancyTextPosition.transform;
                tempGo.transform.localPosition = Vector3.zero;

                goFancyTextPosition.transform.position = new Vector3(16, -80, 0);
                goFancyTextPosition = tempGo;

                textmeshFancyTextPosition = goFancyTextPosition.AddComponent<TextMesh>();
                if (textmeshFancyTextPosition != null)
                {
                    Log("Current value of fpMenuFont: " + fpMenuFont);
                    textmeshFancyTextPosition.font = fpMenuFont;
                    textmeshFancyTextPosition.GetComponent<MeshRenderer>().materials[0] = fpMenuMaterial;
                    textmeshFancyTextPosition.characterSize = 10;
                    textmeshFancyTextPosition.anchor = TextAnchor.UpperLeft;
                    textmeshFancyTextPosition.text =
                        "I exist!@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@\n@@@@@@@@@@@@@@@@@@@@@@@@";
                    Log("Attempting to clone energyBar. Attaching to Stage HUD.");
                }
                else
                {
                    Log("Tried to create textMesh but failed.");
                }
            }
            else
            {
                goFancyTextPosition = new GameObject();
                textmeshFancyTextPosition = goFancyTextPosition.AddComponent<TextMesh>();
                textmeshFancyTextPosition.font = fpMenuFont;
                textmeshFancyTextPosition.GetComponent<MeshRenderer>().materials[0] = fpMenuMaterial;
                textmeshFancyTextPosition.characterSize = 10;
                textmeshFancyTextPosition.anchor = TextAnchor.UpperLeft;
                Log(
                    "Could not clone Resume Text or Energy Bar. Manually creating TextMesh and Attaching to Stage HUD.");


                //Log("Could not clone Resume Text. Canceling.");
                //return;
            }

            goFancyTextPosition.transform.parent = goStageHUD.transform;
            goFancyTextPosition.transform.localPosition = new Vector3(10, 20, 0);
            textmeshFancyTextPosition.gameObject.name = "RSNTextMesh";
            if (cloneMeForText == null)
            {
                cloneMeForText = Instantiate(textmeshFancyTextPosition);
                DontDestroyOnLoad(cloneMeForText);
                cloneMeForText.gameObject.SetActive(false);
                cloneMeForText.gameObject.name = "RSNTextMeshCloneMe";
            }

            UpdateFancyText();
        }

        public static GameObject CloneHealthBar(FPPlayer targetPlayer)
        {
            GameObject newHud = null;
            var huds = GameObject.FindObjectsOfType<FPHudMaster>();
            if (huds.Length > 0)
            {
                newHud = GameObject.Instantiate(huds[0].gameObject,
                    (huds[0].transform.position + new Vector3(0f, -128f, 0f)), huds[0].transform.rotation);
                newHud.name = "Stage HUD " + huds.Length.ToString();

                var hudScript = newHud.GetComponent<FPHudMaster>();
                //hudScript.onlyShowHealth = true;
                hudScript.targetPlayer = targetPlayer;
                var bindPlayerToHud = targetPlayer.gameObject.AddComponent<BindPlayerToHudInstance>();
                bindPlayerToHud.fpplayer = targetPlayer;
                bindPlayerToHud.hudMaster = hudScript;
            }


            return newHud;
        }

        public void UpdateFancyText()
        {
            if (textmeshFancyTextPosition != null && showVarString)
                textmeshFancyTextPosition.text = debugDisplay;
            else if (!showVarString) textmeshFancyTextPosition.text = "";

            if (fpplayer != null && goFancyTextPosition != null)
                //goFancyTextPosition.transform.position = new Vector3(fpplayer.position.x - 10, fpplayer.position.y - 10, -1);
                goFancyTextPosition.transform.position = new Vector3(16, -80, 0);
        }

        public void
            OnSceneWasInitialized(int buildindex,
                string sceneName) // Runs when a Scene has Initialized and is passed the Scene's Build Index and Name.
        {
            if (DeterministicMode.Value)
            {
                UnityEngine.Random.InitState(fp2ReleaseDateInt);
                Log($"Seeding RNG with {fp2ReleaseDate.ToString()} ({fp2ReleaseDateInt})");
            }

            sLogger.LogInfo("OnSceneWasInitialized: " + buildindex + " | " + sceneName);
            SkipBootIntros();
            GrabAndTweakPauseMenu();
            //GrabAndUpdateCameraDetails();
            VisualizePlaneSwitchers();

            /*
            if (goFP2TrainerYourPlayerIndicator == null)
            {
                goFP2TrainerYourPlayerIndicator = FP2TrainerYourPlayerIndicator.CreateFPYourPlayerIndicator(
                    "YourPlayer", Vector3.zero, Quaternion.identity, goFP2Trainer.transform);
            }
            */
        }

        private void VisualizePlaneSwitchers()
        {
            if (planeSwitcherVisualizer != null)
            {
                MillasToybox.Log("PSV Not Null");
                planeSwitcherVisualizer.SpawnVisualizers();
                planeSwitcherVisualizer.SetActiveOfSpawnedVisualizers(planeSwitchVisualizersVisible);
                
                cameraHotspotVisualizer.SpawnVisualizers();
                cameraHotspotVisualizer.SetActiveOfSpawnedVisualizers(planeSwitchVisualizersVisible);
            }
            else
            {
                if (!planeSwitchVisualizersCreated)
                {
                    MillasToybox.Log("PSV Is Null");
                    planeSwitchVisualizers = new List<GameObject>();

                    List<PlaneSwitcher> planeSwitchers;
                    planeSwitchers = new List<PlaneSwitcher>((GameObject.FindObjectsOfType<PlaneSwitcher>()));
                    Debug.Log(System.String.Format("Found {0} PlaneSwitchers. Attempting to visualize.\n",
                        planeSwitchers.Count));
                    GameObject goCube;
                    Renderer renCube;
                    foreach (PlaneSwitcher ps in planeSwitchers)
                    {
                        MillasToybox.Log(System.String.Format("Adding Cube to {0} PlaneSwitchers.\n", ps.name));
                        goCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        goCube.name = ("Visualizer " + ps.name);
                        goCube.transform.position = new Vector3(ps.transform.position.x, ps.transform.position.y,
                            ps.transform.position.z);
                        goCube.transform.localScale = new Vector3(ps.xsize, ps.ysize, 1f);
                        renCube = goCube.GetComponent<Renderer>();

                        //renCube.material.color = new Color(1, 0, 0, 0.7f);
                        renCube.material.color = new Color(1, 0, 0, 1f);
                        goCube.SetActive(planeSwitchVisualizersVisible);
                        planeSwitchVisualizers.Add(goCube);
                        //renCube.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        //renCube.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    }

                    planeSwitchVisualizersCreated = true;
                }
            }
        }

        public void ShowPlaneSwitchVisualizers()
        {
            planeSwitchVisualizersVisible = true;

            if (planeSwitcherVisualizer != null)
            {
                planeSwitcherVisualizer.SetActiveOfSpawnedVisualizers(planeSwitchVisualizersVisible);
                cameraHotspotVisualizer.SetActiveOfSpawnedVisualizers(planeSwitchVisualizersVisible);
                return;
            }

            if (planeSwitchVisualizers == null)
            {
                return;
            }

            foreach (var psv in planeSwitchVisualizers)
            {
                psv.SetActive(planeSwitchVisualizersVisible);
            }
        }

        public void HidePlaneSwitchVisualizers()
        {
            planeSwitchVisualizersVisible = false;

            if (planeSwitcherVisualizer != null)
            {
                planeSwitcherVisualizer.SetActiveOfSpawnedVisualizers(planeSwitchVisualizersVisible);
                cameraHotspotVisualizer.SetActiveOfSpawnedVisualizers(planeSwitchVisualizersVisible);
                return;
            }

            if (planeSwitchVisualizers == null)
            {
                return;
            }

            foreach (var psv in planeSwitchVisualizers)
            {
                psv.SetActive(planeSwitchVisualizersVisible);
            }
        }

        public void TogglePlaneSwitchVisualizers()
        {
            if (planeSwitcherVisualizer)
            {
                /*planeSwitcherVisualizer.AnnihilateGameObjects("AM_BG0 (0)", "AM_BG0 (1)", "AM_BG0 (2)", "AM_BG0 (3)", "AM_BG0 (4)", "AM_BG0 (5)",
                    "AM_BG1");*/
            }

            planeSwitchVisualizersVisible = !planeSwitchVisualizersVisible;
            if (planeSwitchVisualizersVisible)
            {
                ShowPlaneSwitchVisualizers();
            }
            else
            {
                HidePlaneSwitchVisualizers();
            }
        }

        public void OnApplicationQuit() // Runs when the Game is told to Close.
        {
            sLogger.LogInfo("OnApplicationQuit");
            //MelonPreferences.Save();
        }

        public void Update()
        {
            OnGameObjectUpdate();
        }

        public void OnGameObjectUpdate()
        {
            try
            {
                previousSceneName = activeSceneName;
                activeSceneName = SceneManager.GetActiveScene().name;
                bSceneChanged = !activeSceneName.Equals(previousSceneName);

                if (bSceneChanged)
                {
                    hasInitialized = false;
                }

                if (!hasInitialized)
                {
                    try
                    {
                        OnSceneWasInitialized(SceneManager.GetActiveScene().buildIndex, activeSceneName);
                        OnSceneWasLoaded(SceneManager.GetActiveScene().buildIndex, activeSceneName);
                        hasInitialized = true;
                    }
                    catch (Exception e)
                    {
                        Log("EXCEPTION\n");
                        Log(e.ToString());
                        Log(e.Message);
                        Log(e.StackTrace);
                    }
                }

                if (DeterministicMode.Value)
                {
                    //UnityEngine.Random.InitState(fp2ReleaseDateInt);
                    //Log($"Seeding RNG with {fp2ReleaseDate.ToString()} ({fp2ReleaseDateInt})");
                }
                
                if (introSkipped < 1)
                {
                    SkipBootIntros();
                }

                if (waitingForNextFrameForSpoilerGimmick)
                {
                    GetReferencesToSpoilerGimmickPart2();
                }

                if (dpsTracker != null) dpsTracker.Update();

                if (timeoutShowWarpInfo > 0) timeoutShowWarpInfo -= FPStage.frameTime;
                if (timeoutShowWarpInfo < 0) timeoutShowWarpInfo = 0;
                try
                {
                    if (player == null)
                    {
                        player = GetFirstPlayerGameObject();
                        fpplayer = FPStage.currentStage.GetPlayerInstance_FPPlayer();
                        fpStage = FPStage.currentStage;

                        if (player != null) sLogger.LogInfo("Trainer found a Player Object: ");
                    }

                    if (stageHUD != null)
                    {
                    }
                    else
                    {
                        stageHUD = GetStageHUD();
                        if (stageHUD != null)
                        {
                            positionDigits = new List<FPHudDigit>();
                            for (var i = 0; i < 10; i++)
                            {
                                positionDigits.Add(stageHUD.AddComponent<FPHudDigit>());
                                if (i < 5)
                                    positionDigits[i].transform.position = new Vector3(i * 16, 64,
                                        positionDigits[i].transform.position.z);
                                else
                                    positionDigits[i].transform.position = new Vector3(i * 16 + 16, 64,
                                        positionDigits[i].transform.position.z);
                            }
                        }
                    }

                    debugDisplay = "";

                    if (FPStage.currentStage != null)
                    {
                        StartVersusIfSparringOrConfiged();
                    }

                    if (fpplayer != null)
                    {
                        if (hotkeysLoaded)
                        {
                            HandleHotkeys();
                        }

                        try
                        {
                            //This should probably be in its own script:

                            if (UseInstaSwitch.Value && !instaSwitchCharsSpawned)
                            {
                                MillasToybox.Log("Finna spawn extra chars for instaswap");
                                //FPPlayer2p.SpawnExtraCharactersViaSpawnPoint();
                                FPPlayer2p.SpawnExtraCharacters();
                                instaSwitchCharsSpawned = true;
                            }
                        }
                        catch (Exception e)
                        {
                            MillasToybox.Log(e.Message + e.StackTrace);
                        }

                        if (multiplayerStart && !doneMultiplayerStart)
                        {
                            //currentDataPage = DataPage.MULTIPLAYER_DEBUG;
                            while (fpplayers.Count < MultiCharStartNumChars.Value)
                            {
                                FPPlayer2p.SpawnExtraCharacter();
                            }

                            var joys = ReInput.players.Players[0].controllers.Joysticks; 
                            for (int i = 0; i < fpplayers.Count; i++)
                            {
                                int iClamp = Mathf.Clamp(i, 0, joys.Count);
                                Debug.Log("Binding" + fpplayers[i].gameObject.name + " to Controller " + iClamp + " - " + joys[iClamp]);
                                var manipulator = RewiredInputTweaks.AddControlManipulator(fpplayers[i]);
                                manipulator.assignedController = joys[iClamp];
                            }

                            if (EnableSplitScreen.Value)
                            {
                                StartSplitscreen(); // Probably need to include a way to stop this from happening automatically.
                            }

                            doneMultiplayerStart = true;
                        }

                        if (showInstructions)
                        {
                            HandleInstructionPageDisplay();
                        }

                        else
                        {
                            UpdateDPS();
                            if (timeoutShowWarpInfo > 0) debugDisplay += warpMessage + "\n";

                            if (fptls != null)
                            {
                                var snp = fptls.availableScenes[fptls.menuSelection];
                                debugDisplay += "Warp to: " + fptls.menuSelection + " | " + snp.name + "\n";
                                debugDisplay += "Level Select Parent Pos: " + stageSelectMenu.transform.position + "\n";
                                var tempGoButton = stageSelectMenu.transform.Find("AnnStagePlayIcon").gameObject;
                                debugDisplay += "Level Select Button Pos: " + tempGoButton.transform.position + "\n";
                                debugDisplay += "Level Select Button LocalPos: " +
                                                tempGoButton.transform.localPosition +
                                                "\n";
                            }


                            HandleDataPageDisplay();
                            //FPPlayer2p.ShowPressedButtons();
                        }

                        FPPlayer2p.CatchupIfPlayerTooFarAway();
                        //FPPlayer2p.UpdateObjectActivationForNonLeadPlayers();
                        //UpdateObjectActivationForAllPlayers(fpplayers);
                    }

                    debugDisplay = FP2TrainerAllyControls.funky + "\n" + debugDisplay;

                    if (goFancyTextPosition != null)
                        UpdateFancyText();
                    else
                        CreateFancyTextObjects();

                    HandleInvincibility();
                    
                    if (ShowInputNamesInTerminal.Value)
                    {
                        FPPlayer2p.ShowPressedButtons();
                    }
                }

                catch (Exception e)
                {
                    Log("Trainer Error During Update: " + e.Message + "(" + e.InnerException?.Message + ") @@" +
                        e.StackTrace);
                }

                if (fpStage != null)
                {
                    //FPStage.showColliders = ShowCollidersLastSetting.Value;
                    EnforceTenMinuteTimerPenalty();
                }

                if (!skipRecording)
                {
                    RecordPlayer1Input();
                }

                if (LockP1ToGhostFiles.Value && fpplayer != null)
                {
                    //This is probably resource intensive. Maybe there's a better way to prevent this value from being overwritten.
                    fpplayer.inputMethod = fpplayer.HandleAllyControlsGhost; 
                }

                if (EnableSplitScreen.Value)
                {
                    UpdateSplitScreens();
                }

                // Players without a proper validated stageListPos can't interact with many gameplay objects.
                // So we want to try to make sure they get validated.
                // Calling this every frame may be a signficant performance penalty though...
                ValidateAllFPPlayers();
            }
            catch (Exception e)
            {
                Log("EXCEPTION\n");
                Log(e.ToString());
                Log(e.Message);
                Log(e.StackTrace);
            }
        }

        private void RecordPlayer1Input()
        {
            if (fpplayer != null && FP2TrainerAllyControls.inputQueueForPlayers != null)
            {
                FP2TrainerAllyControls.GetInputQueue(fpplayer).AddTime(FPStage.deltaTime);
                var ipq = FP2TrainerAllyControls.RecordInput(fpplayer);
                if (!FP2TrainerAllyControls.inputQueueForPlayers.ContainsKey(fpplayer.GetInstanceID()))
                {
                    FP2TrainerAllyControls.inputQueueForPlayers.Add(fpplayer.GetInstanceID(), ipq);
                }
            }
        }

        private void EnforceTenMinuteTimerPenalty()
        {
            if (fpStage.minutes < 10)
            {
                fpStage.minutes += 10;
            }
        }

        private void HandleDataPageDisplay()
        {
            var collisionLayerName = fpplayer.collisionLayer.ToString();
            if (fpplayer.collisionLayer >= 0)
            {
                //collisionLayerName = $"{collisionLayerName}: {LayerMask.LayerToName(fpplayer.collisionLayer)}";
                collisionLayerName = $"{collisionLayerName}: {FPLayerNames[fpplayer.collisionLayer]}";
            }

            if (currentDataPage == DataPage.NO_CLIP)
            {
                debugDisplay += "NoClip Enabled: " + noClip.ToString() + "\n";
                debugDisplay += "Position: " + fpplayer.position.ToString() + "\n";
                debugDisplay += "Terrain Collision: " + fpplayer.terrainCollision.ToString() + "\n";
                debugDisplay += "Physics Enabled: " + fpplayer.enablePhysics.ToString() + "\n";
                debugDisplay += "Collision Layer: " + collisionLayerName + "\n";
                debugDisplay += "PlaneSwitcherVisualizers: " + planeSwitchVisualizersVisible.ToString() + "\n";
                debugDisplay += "Show Debug Colliders: " + ShowCollidersLastSetting.Value.ToString() + "\n";
            }

            string pad = "                    ";
            if (currentDataPage == DataPage.CAMERA)
            {
                foreach (var cam in GameObject.FindObjectsOfType<Camera>())
                {
                    if (cam.gameObject.name.Contains("Render"))
                    {
                        var strCamsAreJank = $"{pad}{cam}\n{pad} => {cam.transform.position}:TargText{cam.targetTexture}\n"; 
                        debugDisplay += strCamsAreJank;
                        Log(strCamsAreJank);
                    }
                }

                debugDisplay += "Position: " + fpplayer.position.ToString() + "\n";
            }
            if (currentDataPage == DataPage.CAMERA_ALL)
            {
                var strCamsAreJank = "";
                foreach (var cam in GameObject.FindObjectsOfType<Camera>())
                {
                    strCamsAreJank = $"{pad}{cam}\n{pad} => {cam.transform.position}:d:{cam.depth}\n"; 
                    debugDisplay += strCamsAreJank;
                    Log(strCamsAreJank);
                }

                debugDisplay += "Position: " + fpplayer.position.ToString() + "\n";
            }

            if (noClip)
            {
                debugDisplay += "NoClip: " + fpplayer.position.ToString() + "\n";
            }

            if (currentDataPage == DataPage.MOVEMENT)
            {
                debugDisplay += "Movement (1/3): \n";
                if (playerValuesToShow.Contains("Pos")) debugDisplay += "Pos: " + fpplayer.position + "\n";
                if (playerValuesToShow.Contains("Vel")) debugDisplay += "Vel: " + fpplayer.velocity + "\n";
                if (playerValuesToShow.Contains("Magnitude"))
                {
                    debugDisplay += "Acceleration: " + fpplayer.acceleration + "\n";
                    debugDisplay += "Magnitude: " + fpplayer.velocity.magnitude + "\n";
                    debugDisplay += "Accel: " + fpplayer.acceleration + "\n";
                    debugDisplay += "Air Accel: " + fpplayer.airAceleration + "\n";
                    debugDisplay += "Air Drag: " + fpplayer.airDrag + "\n";
                }

                debugDisplay += "Collision Layer: " + collisionLayerName + "\n";
                debugDisplay += "PlaneSwitcherVisualizers: " + planeSwitchVisualizersVisible.ToString() + "\n";
                debugDisplay += "Show Debug Colliders: " + ShowCollidersLastSetting.Value.ToString() + "\n";
            }
            else if (currentDataPage == DataPage.MOVEMENT_2)
            {
                debugDisplay += "Movement (2/3): \n";

                debugDisplay += "Collision Layer: " + collisionLayerName + "\n";
                debugDisplay += "PlaneSwitcherVisualizers: " + planeSwitchVisualizersVisible.ToString() + "\n";
                debugDisplay += "Show Debug Colliders: " + ShowCollidersLastSetting.Value.ToString() + "\n";

                if (playerValuesToShow.Contains("Ground Angle"))
                    debugDisplay += "Ground Angle: " + fpplayer.groundAngle + "\n";
                if (playerValuesToShow.Contains("Ground Velocity"))
                    debugDisplay += "Ground Velocity: " + fpplayer.groundVel + "\n";
                if (playerValuesToShow.Contains("Ceiling Angle"))
                    debugDisplay += "Ceiling Angle: " + fpplayer.ceilingAngle + "\n";
                if (playerValuesToShow.Contains("Sensor Angle"))
                    debugDisplay += "Sensor Angle: " + fpplayer.sensorAngle + "\n";
                if (playerValuesToShow.Contains("Gravity Angle"))
                    debugDisplay += "Gravity Angle: " + fpplayer.gravityAngle + "\n";
                if (playerValuesToShow.Contains("Gravity Strength"))
                    debugDisplay += "Gravity Strength: " + fpplayer.gravityStrength + "\n";
            }
            else if (currentDataPage == DataPage.MOVEMENT_3)
            {
                debugDisplay += "Movement (3/3): \n";

                debugDisplay += "Collision Layer: " + collisionLayerName + "\n";
                debugDisplay += GetFieldText(fpplayer, "groundFallback");
                debugDisplay += GetFieldText(fpplayer, "groundFallbackTimer");
                debugDisplay += GetFieldText(fpplayer, "position");
                debugDisplay += GetFieldText(fpplayer, "velocity");
                debugDisplay += GetFieldText(fpplayer, "angle");
                debugDisplay += GetFieldText(fpplayer, "prevGroundAngle");
                // debugDisplay += GetFieldText(fpplayer, "platformVelocity");
                // debugDisplay += GetFieldText(fpplayer, "scale");
                // debugDisplay += GetFieldText(fpplayer, "collisionLayer");
            }
            else if (currentDataPage == DataPage.FPPlayerCustom)
            {
                debugDisplay += "FPPlayer Custom:\n";

                debugDisplay += "Collision Layer: " + collisionLayerName + "\n";
                
                var fields = DataDisplayCustomPageFields.Value.Trim().Split(',');
                var field = "";
                for (int i = 0; i < fields.Length; i++)
                {
                    if (fields[i].IsNullOrWhiteSpace())
                    {
                        continue;
                    }

                    field = fields[i].Trim();
                    string currentStageToken = "currentstage.";
                    if (field.ToLower().StartsWith(currentStageToken))
                    {
                        debugDisplay += GetFieldText(FPStage.currentStage, field.Substring(currentStageToken.Length));
                    }
                    else
                    {
                        debugDisplay += GetFieldText(fpplayer, field);
                    }
                }
            }
            else if (currentDataPage == DataPage.COMBAT)
            {
                debugDisplay += "Combat: \n";
                debugDisplay += "Health: " + fpplayer.health + "\n";

                var tempDmgType = fpplayer.damageType;
                if (tempDmgType > 4) tempDmgType = -1;
                debugDisplay += "Hurt Damage Element: " + fpElementTypeNames[tempDmgType] + "\n";

                if (nearestEnemy != null)
                    debugDisplay += nearestEnemy.name + " Health: " + nearestEnemy.health + "\n";

                if (dpsTracker != null) debugDisplay += "DPS: " + dpsTracker + "\n";

                debugDisplay += "Energy: " + fpplayer.energy + "\n";
                debugDisplay += "Energy Recover Current: " + fpplayer.energyRecoverRateCurrent + "\n";
                debugDisplay += "Energy Recover: " + fpplayer.energyRecoverRate + "\n";
                debugDisplay += "Faction: " + fpplayer.faction + "\n";
                debugDisplay += "Attack Power: " + fpplayer.attackPower + "\n";
                debugDisplay += "Attack Hitstun: " + fpplayer.attackHitstun + "\n";
                debugDisplay += "Attack Knockback: " + fpplayer.attackKnockback + "\n";
                if (playerValuesToShow.Contains("InflictedDamage"))
                    debugDisplay += "InflictedDamage: " + fpplayer.damageInflicted + "\n";
                debugDisplay += "Guard Time: " + fpplayer.guardTime + "\n";
                debugDisplay += "ATK NME INV TIM: " + fpplayer.attackEnemyInvTime + "\n";
                debugDisplay += "Hit Stun: " + fpplayer.hitStun + "\n";
                debugDisplay += "Invul Time: " + fpplayer.invincibilityTime + "\n";
            }
            else if (currentDataPage == DataPage.DPS)
            {
                debugDisplay += "DPS: \n";
                if (dpsTracker != null)
                {
                    if (nearestEnemy != null && nearestEnemyPrevious != null)
                    {
                        debugDisplay += "Previous Nearest Enemy: " + nearestEnemyPrevious.name + "\n";
                        debugDisplay += "Prev Health: " + nearestEnemyPreviousHP + "\n";
                    }
                    else if (nearestEnemy == null)
                    {
                        debugDisplay += "Nearest Enemy Not Found\n";
                    }
                    else if (nearestEnemy == null)
                    {
                        debugDisplay += "Previous Nearest Enemy Not Found\n";
                    }

                    debugDisplay += dpsTracker.GetDPSBreakdownString();
                }
                else
                {
                    debugDisplay += "No DPS Tracker found?";
                }
            }
            else if (currentDataPage == DataPage.DPS_ALL)
            {
                if (dpsTracker != null)
                {
                    debugDisplay += "DPS ALL: \n";
                    debugDisplay += dpsTracker.GetDPSBreakdownString();
                }
                else
                {
                    debugDisplay += "No DPS Tracker found?";
                }
            }
            else if (currentDataPage == DataPage.MULTIPLAYER_DEBUG)
            {
                debugDisplay += "Multiplayer Debug: \n";
                var tempDmgType = -1;
                string isLeader = "";
                foreach (var mp_fpplayer in fpplayers)
                {
                    if (mp_fpplayer == FPStage.currentStage.GetPlayerInstance_FPPlayer())
                    {
                        isLeader = " - Leader";
                    }
                    else
                    {
                        isLeader = "";
                    }

                    debugDisplay += mp_fpplayer.name + isLeader + "\n";
                    debugDisplay += String.Format("{0:000.00}/{1:000.00} HP {2:000.00}/{3:000.00} EN\n",
                        mp_fpplayer.health, mp_fpplayer.healthMax,
                        mp_fpplayer.energy, 100f);

                    debugDisplay += mp_fpplayer.position.ToString() + "\n";
                    //debugDisplay += mp_fpplayer.name + " Energy: " + mp_fpplayer.energy + "\n";
                }
            }
            else if (currentDataPage == DataPage.BOSS)
            {
                debugDisplay += "Boss: \n";
                fpEnemies.Clear();
                foreach (var bh in bossHuds)
                {
                    if (!bh.transform.gameObject.activeInHierarchy)
                    {
                        ReacquireBossHuds();
                        break;
                    }

                    if (bh.targetBoss != null) fpEnemies.Add(bh.targetBoss);
                }

                if (fpEnemies.Count > 0)
                    foreach (var ene in fpEnemies)
                    {
                        if (ene == null) continue;

                        debugDisplay += ene.name + " Health: " + ene.health + "\n";
                        debugDisplay += ene.name + " Freeze Timer: " + ene.freezeTimer + "\n";
                        //debugDisplay += mp_fpplayer.name + " Energy Recover: " + mp_fpplayer.energyRecoverRate.ToString() + "\n";
                        //debugDisplay += mp_fpplayer.name + " Energy Recover Current: " + mp_fpplayer.energyRecoverRateCurrent.ToString() + "\n";
                        debugDisplay += ene.name + " Is Harmless: " + ene.isHarmless + "\n";
                        debugDisplay += ene.name + " Cannot Be Killed: " + ene.cannotBeKilled + "\n";
                        debugDisplay += ene.name + " Cannot Be Frozen: " + ene.cannotBeFrozen + "\n";
                        debugDisplay += ene.name + " Last Received Damage: " +
                                        ene.lastReceivedDamage + "\n";
                        debugDisplay += ene.name + " LRD (Unmodified): " +
                                        ene.lastReceivedDamageUnmodified + "\n";
                    }
                else
                    debugDisplay +=
                        "Unable to find relevant enemies.\nTry switching to this view while the healthbar is visible.\n";
            }
            else if (currentDataPage == DataPage.LIST_ACTIVES)
            {
                debugDisplay += "StageObjList: \n";

                if (FPStage.currentStage != null)
                {
                    var fieldInfo = FPStage.currentStage.GetType()
                        .GetField("stageObjList", BindingFlags.Static | BindingFlags.NonPublic);
                    FPBaseObject[] stageObjList = (FPBaseObject[])(fieldInfo.GetValue(FPStage.currentStage));
                    for (int i = 0; i < stageObjList.Length; i++)
                    {
                        if (stageObjList[i] != null && stageObjList[i].enabled && stageObjList[i].isActiveAndEnabled)
                        {
                            debugDisplay += $"{stageObjList[i].name}:Act+En?{stageObjList[i].isActiveAndEnabled}\n";
                        }
                    }
                }
            }
            else if (currentDataPage == DataPage.LIST_ACTIVES_PUSHERS)
            {
                debugDisplay += "StageObjList (Pushers Only): \n";

                if (FPStage.currentStage != null)
                {
                    var fieldInfo = FPStage.currentStage.GetType()
                        .GetField("stageObjList", BindingFlags.Static | BindingFlags.NonPublic);
                    FPBaseObject[] stageObjList = (FPBaseObject[])(fieldInfo.GetValue(FPStage.currentStage));
                    for (int i = 0; i < stageObjList.Length; i++)
                    {
                        if (stageObjList[i] != null
                            && stageObjList[i].name.StartsWith("Pusher"))
                        {
                            debugDisplay +=
                                $"{stageObjList[i].name}:[En?{stageObjList[i].enabled}][Act+En?{stageObjList[i].isActiveAndEnabled}]\n";
                        }
                    }
                }
            }
        }

        public static string GetFieldText(System.Object instance, string fieldName)
        {
            var txt = "";
            try
            {
                // this part based on: https://stackoverflow.com/questions/6961781/reflecting-a-private-field-from-a-base-class

                Type t = instance.GetType();
                FieldInfo fi = null;

                while (t != null) 
                {
                    fi = t.GetField(fieldName,
                        BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

                    if (fi != null) break;

                    t = t.BaseType; 
                }
                

                if (fi != null)
                {
                    var val = fi.GetValue(instance);
                    txt = $"{fieldName}: {val.ToString()}\n";
                }
                else
                {
                    txt = $"{fieldName}: [Invalid Field?]\n";
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            return txt;
        }

        public void HandleInstructionPageDisplay()
        {
            if (showInstructions && currentDataPage == DataPage.NONE)
            {
                currentDataPage = DataPage.MOVEMENT;
            }

            int numHotkeyLinesPerPage = 7;
            debugDisplay += String.Format("--[Instructions: ({0} / {1})]--\n",
                (int)(currentInstructionPage + 1), ((int)(InstructionPage.NONE) + 1));

            switch (currentInstructionPage)
            {
                /*
                 *"\"Milla's Toybox\" (or \"FP2 Trainer\" if you prefer),\n"
                 * This particular example line is roughly 48 characters long,
                 * any longer than that, and the text won't fit on-screen.
                 */
                case InstructionPage.BASICS:
                    debugDisplay += "**Basics**\n" +
                                    "\"Milla's Toybox\" (or \"FP2 Trainer\" if you prefer),\n" +
                                    "is a speedrun-focused trainer toolkit \nmod for Freedom Planet 2.\n" +
                                    "It is primarily used for gaining \nadditional information about how the game works\n" +
                                    "and experimenting with the physics and \nmechanics of the game to find new\n" +
                                    "techniques in the pursuit of going FAST.\n" +
                                    "But of course, there's other toys too. But first...\n" +
                                    String.Format("(Press {0} to continue)\n", PHKShowNextDataPage.Value);
                    break;
                case InstructionPage.BACKUPS:
                    debugDisplay += "**Backups**\n" +
                                    "It's highly recommended that you back up \nyour entire FP2 folder\n" +
                                    "and only install mods on a separate \ncopy to prevent this training\n" +
                                    "from unexpectedly breaking due to \ngame version updates.\n" +
                                    "You may backup your save files too, if you like.\n" +
                                    "Your save files can typically be found at:\n" +
                                    "C:\\Users\\<YOUR username here>\\AppData\n\\LocalLow\\GalaxyTrail\\Freedom Planet 2\n" +
                                    "(That's **LocalLOW**, not Local. A common mistake.)\n";
                    //"Your save files are currently stored at:\n{0}", Application.persistentDataPath;
                    break;
                case InstructionPage.SPEEDRUN:
                    debugDisplay += "**Speedrun Tools**\n" +
                                    "Data data DATA! Your speed, position, damage,\n" +
                                    "collision layer, dps, boss info, and more!\n" +
                                    String.Format("View DataViewer Next Page: {0}\n" +
                                                  "View DataViewer Previous Page: {1}\n" +
                                                  "Hide DataViewer{7}\n" +
                                                  "Set Warp Point: {2}\n" +
                                                  "Teleport to Warp Point: {3}\n" +
                                                  "Toggle PlaneSwitcherVisualizers: {4}\n\n" +
                                                  "Toggle Show All Debug Colliders: {6}\n\n" +
                                                  "Load ANY Stage Menu: {5}\n" +
                                                  "Confirm Stage Menu Choice: (Jump Button)\n",
                                        PHKShowNextDataPage.Value, PHKShowPreviousDataPage.Value, PHKSetWarpPoint.Value,
                                        PHKGotoWarpPoint.Value, PHKTogglePlaneSwitcherVisualizers.Value,
                                        PHKGoToLevelSelectMenu.Value, PHKToggleShowColliders.Value, 
                                        PHKHideDataView.Value);
                    break;
                case InstructionPage.QUICK_RESTART:
                    debugDisplay += "**Quick Restart**\n" +
                                    "For when going to the pause menu is too slow.\n" +
                                    String.Format("Reset to Checkpoint: {0}\n" +
                                                  "Restart Stage: {1}\n",
                                        PHKReturnToCheckpoint.Value, PHKRestartStage.Value);
                    break;
                case InstructionPage.NO_CLIP:
                    debugDisplay += "**NoClip**\n" +
                                    "This allows you to fly freely through the \nmap ignoring gravity and walls!\n" +
                                    "Most things can't touch you, but \nthe camera can still be locked in place.\n" +
                                    "And some crush-triggers may still KO you.\n" +
                                    String.Format("Toggle NoClip Mode: {0}\n" +
                                                  "Cancel NoClip and Return to Start:\n  (Attack Button)\n" +
                                                  "Exit NoClip at Current Position:\n  (Jump Button) or {0}\n",
                                        PHKToggleNoClip.Value);
                    break;
                case InstructionPage.MULTICHARACTER:
                    debugDisplay += "**Multi-Character**\n" +
                                    "Play as multiple characters at the same time!\n" +
                                    "Be warned, this is very buggy. \nThe game is not designed to support \nmore than one character at a time.\n" +
                                    "If you have multiple characters, KOed characters \n" +
                                    "are removed from play immediately until \n" +
                                    "there is only one left.\n" +
                                    String.Format("Spawn Additional Character: {0}\n" +
                                                  "Switch to Next Remaining Character: {1}\n" +
                                                  "Toggle Ally-spawn on level-start: {2}\n" +
                                                  "Cycle Ally Playstyle: {3}\n" +
                                                  "Insta-KO Current Character: {4}\n" +
                                                  "Insta-KO Current Boss: {5}\n" +
                                                  "Start Splitscreen: {6}\n",
                                        PHKSpawnExtraChar.Value, PHKSwapBetweenSpawnedChars.Value,
                                        PHKToggleMultiCharStart.Value, PHKCyclePreferredAllyControlType.Value,
                                        PHKKOCharacter.Value, PHKKOBoss.Value, PHKStartSplitscreen.Value);
                    break;
                /*
                case InstructionPage.CHAR_INSTASWAP:
                    debugDisplay += "**Character Insta-swap**\n" +
                                    "WIP. Switch to any character on the fly." +
                                    "Defaults to being set via numpad numbers." +
                                    "If you're a gamepad player, you'll want to remap those.\n" +
                                    String.Format("PHKSwitchCurrentPlayerToLilac: {0}\n" +
                                                  "PHKSwitchCurrentPlayerToCarol: {1}\n" +
                                                  "PHKSwitchCurrentPlayerToCarolBike: {2}\n" +
                                                  "PHKSwitchCurrentPlayerToMilla: {3}\n" +
                                                  "PHKSwitchCurrentPlayerToNeera: {4}\n" +
                                                  "PHKSwitchCurrentPlayerToNext: {5}\n" +
                                                  "PHKSwitchCurrentPlayerToPrev: {6}\n",
                                        PHKSwitchCurrentPlayerToLilac.Value, PHKSwitchCurrentPlayerToCarol.Value, 
                                        PHKSwitchCurrentPlayerToCarolBike.Value, PHKSwitchCurrentPlayerToMilla.Value,
                                        PHKSwitchCurrentPlayerToNeera.Value,
                                        PHKSwitchCurrentPlayerToNext.Value,
                                        PHKSwitchCurrentPlayerToPrev.Value);
                    break;
                    */
                /*case InstructionPage.NETPLAY:
                    debugDisplay += "**Basics**\n";
                    // PHKStartInputPlayback
                    break;*/
                case InstructionPage.BUGS:
                    debugDisplay += "**Bugs**\n" +
                                    "tbh I need sleep, I'll probably fill this up \nwith something useful later.\n+" +
                                    "If you have this version of the trainer,\nyou probably already know how to contact me.\n\n\n" +
                                    String.Format("GET OUT GET OUT GET OUT\n{0}\n",
                                        PHKGetOutGetOutGetOut.Value);
                    break;
                case InstructionPage.HOTKEYS_1:
                    debugDisplay += "**How to Rebind Hotkeys (1/2)**\n" +
                                    "If you want to change your \nHotkey Bindings, you can edit them\n" +
                                    "at <FP2 Install Dir>/UserData/MelonPreferences.cfg\n" +
                                    "If you don't see a config file there, \nlaunch the game for a few seconds,\n" +
                                    "then close it and check again for \na regenerated config file.\n";
                    break;
                case InstructionPage.HOTKEYS_2:
                    debugDisplay += "**How to Rebind Hotkeys (2/2)**\n" +
                                    "Hotkey Keybinds are Case-Sensitive and can \nonly be changed while the game is NOT RUNNING.\n" +
                                    "Edits made to the config file \nwhile the game is running\n" +
                                    "will be OVERWRITTEN!\n" +
                                    "If you need help setting it up, please ask.\n" +
                                    "If you make a mistake, don't worry!\n" +
                                    "Delete the file and relaunch the game \nto regenerate a new default config file.\n";
                    break;
                case InstructionPage.HOTKEYS_3:
                    debugDisplay += "**Current Hotkeys**\n";
                    debugDisplay += FP2TrainerCustomHotkeys.GetBindingString(1, 1 + numHotkeyLinesPerPage);
                    break;
                case InstructionPage.HOTKEYS_4:
                    debugDisplay += "**More Current Hotkeys**\n";
                    debugDisplay += FP2TrainerCustomHotkeys.GetBindingString(1 + (numHotkeyLinesPerPage * 1),
                        1 + (numHotkeyLinesPerPage * 2));
                    break;
                case InstructionPage.HOTKEYS_5:
                    debugDisplay += "**Even More Current Hotkeys**\n";
                    debugDisplay += FP2TrainerCustomHotkeys.GetBindingString(1 + (numHotkeyLinesPerPage * 2),
                        1 + (numHotkeyLinesPerPage * 3));
                    break;
                case InstructionPage.HOTKEYS_6:
                    debugDisplay += "**Even MORE More Current Hotkeys**\n";
                    debugDisplay += FP2TrainerCustomHotkeys.GetBindingString(1 + (numHotkeyLinesPerPage * 3),
                        1 + (numHotkeyLinesPerPage * 4));
                    break;
                case InstructionPage.HOTKEYS_7:
                    debugDisplay += "**Soooo many More Current Hotkeys**\n";
                    debugDisplay += FP2TrainerCustomHotkeys.GetBindingString(1 + (numHotkeyLinesPerPage * 4),
                        1 + (numHotkeyLinesPerPage * 5));
                    break;
                case InstructionPage.HOTKEYS_8:
                    debugDisplay += "**Current Hotkeys Do I Have To Keep Naming These**\n";
                    debugDisplay += FP2TrainerCustomHotkeys.GetBindingString(1 + (numHotkeyLinesPerPage * 5),
                        1 + (numHotkeyLinesPerPage * 6));
                    break;
                /*
                case InstructionPage.HOTKEYS_9:
                    debugDisplay += "**Current Hotkeys Nine**\n";
                    debugDisplay += FP2TrainerCustomHotkeys.GetBindingString(1 + (numHotkeyLinesPerPage * 6),
                        1 + (numHotkeyLinesPerPage * 7));
                    break;
                case InstructionPage.HOTKEYS_10:
                    debugDisplay += "**Current Hotkeys Ten**\n";
                    debugDisplay += FP2TrainerCustomHotkeys.GetBindingString(1 + (numHotkeyLinesPerPage * 7),
                        1 + (numHotkeyLinesPerPage * 8));
                    break;
                case InstructionPage.HOTKEYS_11:
                    debugDisplay += "**Current Hotkeys Eleven**\n";
                    debugDisplay += FP2TrainerCustomHotkeys.GetBindingString(1 + (numHotkeyLinesPerPage * 8),
                        1 + (numHotkeyLinesPerPage * 9));
                    break;
                    */
                case InstructionPage.QUICKBOOT:
                    debugDisplay += "**QuickBoot**\n";
                    debugDisplay +=
                        "Do you find yourself opening and closing\nthe game often and wish you had a faster way to\n" +
                        "immediately start a stage?\n" +
                        "By setting the \"bootupLevel\" in your config file\n" +
                        "to the name of a valid level in-game (CaseSensitive!)\n" +
                        "The game will immediately drop Lilac \n" +
                        "into that Level after the first set of Logos.\n\n" +
                        "(Tip: Change the value to Empty Quotes (\"\")\nif you want to continue booting to the Main Menu.)\n";
                    break;
                case InstructionPage.NONE:
                    debugDisplay += "That's all, folks!\n" +
                                    String.Format("When you're ready, press {0} to close this guide.\n",
                                        PHKToggleInstructions.Value) +
                                    "If you need more info, reach out \nto me either on GitHub,\n" +
                                    "GalaxyTrail Discord, or \nthe Freedom Planet Speedrunning Discord.\n" +
                                    "Please be sure that you've ACTUALLY \nread the readme and these instructions first\n" +
                                    "And are prepared to explain what you tried.\n" +
                                    "I spent hours writing these instructions,\n" +
                                    "so I'll be a little bit _unkind_ \n" +
                                    "if you didn't read before contacting me.\n";
                    break;
                default:
                    debugDisplay += "this is bugged. i have no idea how you got here.\n";
                    break;
            }

            debugDisplay += String.Format(
                "{0} / {1} -> View Next / Prev Page.\nPress {2} to toggle Instructions on or off. {3}: Close Dataviewer.\n",
                PHKShowNextDataPage.Value, PHKShowPreviousDataPage.Value, PHKToggleInstructions.Value, PHKHideDataView.Value);
        }

        public void UpdateDPS()
        {
            if (dpsTracker != null) dpsTracker.Update();

            UpdateDPSNearestEnemy();

            if (currentDataPage == DataPage.DPS_ALL) UpdateDPSALLEnemies();
        }

        private void UpdateDPSNearestEnemy()
        {
            // Show nearest enemy HP and update DPS.
            if (FPStage.currentStage != null)
            {
                //var activeEnemies = FPStage.GetActiveEnemies();
                nearestEnemy = FPStage.FindNearestEnemy(fpplayer, 2000f);
                if (nearestEnemy != null)
                {
                    if (nearestEnemy == nearestEnemyPrevious
                        && nearestEnemy.health < nearestEnemyPreviousHP)
                    {
                        dpsTracker.AddDamage(nearestEnemyPreviousHP - nearestEnemy.health);
                    }
                    else if (nearestEnemy != nearestEnemyPrevious)
                    {
                        // Do we want to reset on target changed??
                    }

                    nearestEnemyPreviousHP = nearestEnemy.health;
                }
                // Add toggle option to check against damage done to ALL enemies instead of just nearest
                // If adding, give warning that this may cause slowdown.

                nearestEnemyPrevious = nearestEnemy;
            }
        }

        private void UpdateDPSALLEnemies()
        {
            if (currentDataPage == DataPage.DPS_ALL)
            {
                var tempCachedEnemyList = FPStage.GetActiveEnemies();
                InitializeActiveEnemyList();
                PopulateTrainerActiveEnemyList(tempCachedEnemyList);

                if (allActiveEnemiesHealth != null
                    && allActiveEnemiesHealthPrevious != null
                    && allActiveEnemiesHealth.Count > 0)
                    foreach (var ene in allActiveEnemiesHealth)
                        if (allActiveEnemiesHealthPrevious.ContainsKey(ene.Key))
                        {
                            var dmg = allActiveEnemiesHealthPrevious[ene.Key] - ene.Value;
                            if (dmg > 0) dpsTracker.AddDamage(dmg, allActiveEnemiesNames[ene.Key]);
                        }

                allActiveEnemiesHealthPrevious = new Dictionary<int, float>(allActiveEnemiesHealth);
            }
        }

        public void InitializeActiveEnemyList()
        {
            /*
            if (allActiveEnemiesHealth == null)
            {
                allActiveEnemiesHealth = new Dictionary<int, float>();
                allActiveEnemiesNames = new Dictionary<int, string>();
            }
            else
            {
                allActiveEnemiesHealth.Clear();
                allActiveEnemiesNames.Clear();
            }
            */

            allActiveEnemiesHealth = new Dictionary<int, float>();
            allActiveEnemiesNames = new Dictionary<int, string>();
        }

        private void PopulateTrainerActiveEnemyList(List<FPBaseEnemy> tempCachedEnemyList)
        {
            foreach (var ene in tempCachedEnemyList)
                try
                {
                    allActiveEnemiesHealth.Add(ene.objectID, ene.health);
                    allActiveEnemiesNames.Add(ene.objectID, ene.name);
                }
                catch (ArgumentException e)
                {
                    Log(e.ToString());
                    Log(e.StackTrace);
                }
        }

        public static List<FPPlayer> GetFPPlayers()
        {
            var listPlayers =  new List<FPPlayer>(GameObject.FindObjectsOfType<FPPlayer>());
            foreach (var player in listPlayers)
            {
                FPStage.ValidateStageListPos(player);
            }
            return listPlayers.OrderBy(fpp => fpp.characterID).ToList();
        }

        public static void ValidateAllFPPlayers()
        {
            if (fpplayer != null)
            {
                FPStage.ValidateStageListPos(fpplayer);
            }

            foreach (var player in fpplayers)
            {
                if (player != null)
                {
                    FPStage.ValidateStageListPos(player);
                }
            }
        }

        private GameObject GetFirstPlayerGameObject()
        {
            GameObject playerGameObject = null;
            if (FPStage.player == null) return playerGameObject;

            sLogger.LogInfo("Number of Stage Players: " + FPStage.player.Length);
            if (FPStage.currentStage != null && FPStage.currentStage.GetPlayerInstance_FPPlayer() != null)
                playerGameObject = FPStage.currentStage.GetPlayerInstance_FPPlayer().gameObject;
            else if (FPStage.player.Length > 0) playerGameObject = FPStage.player[0].gameObject;

            return playerGameObject;
        }

        public void HandleHotkeys()
        {
            //if (InputControl.GetButton(Controls.buttons.guard) && InputControl.GetButtonDown(Controls.buttons.attack))
            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKLoadDebugRoom))
            {
                Log("Load Debug Room");
                //FP2TrainerAllyControls.DumpAllPlayerVars();
                SceneManager.LoadScene("StageDebugMenu", LoadSceneMode.Additive);
            }

            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKGoToMainMenu))
            {
                Log("Main Menu");
                //UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
                GoToMainMenuNoLogos();
            }

            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKLoadAssetBundles))
            {
                Log("Load Asset Bundles");
                LoadAssetBundlesFromModsFolder();
            }

            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKGoToLevelSelectMenu))
            {
                Log("Level Select");
                if (showInstructions)
                {
                    ToggleShowInstructions();
                }

                var availableScenes = new List<SceneNamePair>();
                var i = 0;
                for (i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
                {
                    var sceneName =
                        Path.GetFileNameWithoutExtension(SceneUtility
                            .GetScenePathByBuildIndex(i));
                    availableScenes.Add(new SceneNamePair(SceneManager.GetSceneByBuildIndex(i), sceneName));
                }

                for (i = 0; i < loadedAssetBundles.Count; i++)
                    foreach (var scenePath in loadedAssetBundles[i].GetAllScenePaths())
                    {
                        var sceneName = Path.GetFileNameWithoutExtension(scenePath);
                        availableScenes.Add(new SceneNamePair(SceneManager.GetSceneByPath(scenePath), sceneName,
                            scenePath));
                    }

                for (i = 0; i < availableScenes.Count; i++) Log(i + " | " + availableScenes[i].name);

                ShowLevelSelect(availableScenes);
                pauseMenu.gameObject.SetActive(false);
                //GameObject.Destroy(this.pauseMenu);
            }

            if (false /*FP2TrainerCustomHotkeys.GetButtonDown(PHKasdfasfd)*/)
            {
                Log("F5 -> Toggle Level Select Menu Visibility");
                ToggleLevelSelectVisibility();
            }

            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKShowNextDataPage))
            {
                if (showInstructions)
                {
                    IncrementInstructionPage();
                    Log("Next Instruction Page (" + Enum.GetName(typeof(InstructionPage), currentInstructionPage) +
                        ")");
                }
                else
                {
                    ToggleVariableDisplay();
                    Log("Next Data Page (" + Enum.GetName(typeof(DataPage), currentDataPage) + ")");
                }
            }

            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKShowPreviousDataPage))
            {
                if (showInstructions)
                {
                    DecrementInstructionPage();
                    Log("Previous Instruction Page (" + Enum.GetName(typeof(InstructionPage), currentInstructionPage) +
                        ")");
                }
                else
                {
                    ToggleVariableDisplayPrevious();
                    Log("Previous Data Page (" + Enum.GetName(typeof(DataPage), currentDataPage) + ")");
                }
            }
            
            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKHideDataView))
            {
                if (currentDataPage != DataPage.NONE)
                {
                    currentDataPage = DataPage.NONE;
                    Log("Hide Inspector");
                }
                else
                {
                    currentDataPage = DataPage.MOVEMENT;
                    Log("Reveal Inspector");
                }
            }

            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKGoToLevelAtLastIndex))
            {
                Log("Load last located scene: ");
                if (fptls != null)
                {
                    var iWantToGoToBed = fptls.availableScenes[fptls.availableScenes.Count - 1];
                    Log(iWantToGoToBed.name);
                    SceneManager.LoadScene(iWantToGoToBed.name);
                }
                else
                {
                    Log("...But the Level Selector hasn't been created yet... (Press F6?)");
                }
            }

            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKKOCharacter))
            {
                //TestDamageNumberPopups();

                if (fpplayer != null)
                {
                    Log("KO the Player");
                    InstaKOPlayer();
                }
                else
                {
                    Log("Attempted to KO the player, but no FPPlayer instance was found");
                }
            }
            
            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKKOBoss))
            {
                //TestDamageNumberPopups();

                ReacquireBossHuds();
                if (fpEnemies.Count > 0)
                {
                    Log("KO Boss");
                    foreach (var enemy in fpEnemies)
                    {
                        enemy.health = 0;
                    }
                }
                else
                {
                    Log("Attempted to KO all bosses, but no bosses were found... (Check for visible HUDs?)");
                }
            }
            
            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKInvincibleBoss))
            {
                EnableInvincibleBoss.Value = !EnableInvincibleBoss.Value;
                Log($"Toggled Invincible Bosses: {!EnableInvincibleBoss.Value} => {EnableInvincibleBoss.Value}");

                /*
                ReacquireBossHuds();
                if (fpEnemies.Count > 0)
                {
                    Log("KO Boss");
                    foreach (var enemy in fpEnemies)
                    {
                        enemy.health = 0;
                    }
                }
                else
                {
                    Log("Attempted to KO all bosses, but no bosses were found... (Check for visible HUDs?)");
                }
                */
            }
            
            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKInvinciblePlayers))
            {
                EnableInvinciblePlayers.Value = !EnableInvinciblePlayers.Value;
                Log($"Toggled Invincible Players: {!EnableInvinciblePlayers.Value} => {EnableInvinciblePlayers.Value}");
            }

            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKToggleInstructions))
            {
                //TestDamageNumberPopups();

                ToggleShowInstructions();
            }

            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKToggleNoClip))
            {
                Log("NoClip Toggle");
                ToggleNoClip();
            }
            
            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKReturnToCheckpoint))
            {
                Log("Return to Checkpoint");
                ReturnToCheckpoint();
            }
            
            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKRestartStage))
            {
                Log("Restart Stage");
                RestartLevel();
            }

            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKTogglePlaneSwitcherVisualizers))
            {
                TogglePlaneSwitchVisualizers();
                ShowPlaneSwitcherVisualizersLastSetting.Value = planeSwitchVisualizersVisible;
                Log(String.Format("Toggle PlaneSwitcher Visualizers: {0} -> {1}", !planeSwitchVisualizersVisible,
                    planeSwitchVisualizersVisible));
            }

            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKToggleShowColliders))
            {
                ToggleShowColliders();
            }
            
            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKDoSpeedBoost))
            {
                if (FPStage.currentStage != null && FPStage.currentStage.GetPlayerInstance_FPPlayer() != null)
                {
                    FPStage.currentStage.GetPlayerInstance_FPPlayer().groundVel = SpeedBoostValue.Value;
                }
            }
            
            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKLaunchPhantomRemote))
            {
                FP2TrainerRemoteHandler.LaunchPhantomCubeRemote();
            }
            
            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKStartVersus))
            {
                RSNVersusManager.Init();
            }

            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKGetOutGetOutGetOut))
            {
                Log("GET OUT GET OUT GET OUT");
                SpawnSpoilerGimmick();
            }

            HandleMultiplayerSpawnHotkeys();
            HandleInstaSwapHotkeys();
            HandleResizeFontHotkeys();
            HandleCameraHotkeys();


            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKGotoWarpPoint))
            {
                fpplayer.position = new Vector2(warpPoint.x, warpPoint.y);
                Log("Goto Warp: " + warpPoint);
                warpMessage = "Warping to " + warpPoint;
                timeoutShowWarpInfo = howLongToShowWarpInfo;
            }

            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKSetWarpPoint))
            {
                warpPoint = new Vector2(fpplayer.position.x, fpplayer.position.y);
                Log("Set Warp: " + warpPoint);
                warpMessage = "Set warp at " + warpPoint;
                timeoutShowWarpInfo = howLongToShowWarpInfo;
            }

            // I'd like to preserve the gamepad version of this somehow...

            /*
            if (InputControl.GetButton(Controls.buttons.pause) && InputControl.GetButtonDown(Controls.buttons.special))
            {
                ToggleNoClip();
            }

            if (InputControl.GetButton(Controls.buttons.guard) && InputControl.GetButtonDown(Controls.buttons.special))
            {
                ToggleNoClip();
            }
            */

            HandleNoClip();
        }

        public static void StartVersusIfSparringOrConfiged()
        {
            bool doStart = bSceneChanged && SceneManager.GetActiveScene().name.Equals("RoyalPalace_Sparring");

            if (bSceneChanged && VersusMultiplayerStart.Value)
            {
                doStart = true;
            }
            

            if (doStart)
            {
                Debug.Log("Auto-Starting Multiplayer.");
                //ArenaSpawner
                RSNVersusManager.Init();
                RSNVersusManager.DummyOutPlayerBosses();
                var arenaSpawner = Component.FindObjectOfType<ArenaSpawner>();
                if (arenaSpawner != null)
                {
                    for (int i = 0; i < arenaSpawner.challenges.Length;i++)
                    {
                        var challenge = arenaSpawner.challenges[i];
                        challenge.spawnAtStart = new FPBaseObject[0];
                        
                        for (int j = 0; j < challenge.roundObjectList.Length;j++)
                        {
                            var roundObject = challenge.roundObjectList[j];
                            roundObject.waitForObjectDestruction = true;
                            roundObject.objectList = new FPBaseObject[0];
                        }
                    }
                }
            }
        }

        public static void ToggleShowColliders()
        {
            ShowCollidersLastSetting.Value = !ShowCollidersLastSetting.Value;
            Log(String.Format("Toggle Show Debug Colliders: {0} -> {1}", !ShowCollidersLastSetting.Value,
                ShowCollidersLastSetting.Value));
        }

        private void ToggleShowInstructions()
        {
            if (fpplayer != null)
            {
                Log(String.Format("Toggle Instructions: ({0}) -> ({1})", showInstructions, !showInstructions));
                showInstructions = !showInstructions;

                ShowInstructionsOnStart.Value = showInstructions;

                if (showInstructions)
                {
                    currentInstructionPage = InstructionPage.BASICS;
                }
            }
        }

        private void HandleMultiplayerSpawnHotkeys()
        {
            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKSpawnExtraChar))
            {
                //TestDamageNumberPopups();

                if (fpplayer != null)
                {
                    Log("Shift + F2 -> Enable MultiCharacter");
                    /*
                    var goNewPlayer = GameObject.Instantiate(fpplayer.gameObject);
                    goNewPlayer.transform.position = new Vector3(fpplayer.position.x - 64, fpplayer.position.y,
                        fpplayer.gameObject.transform.position.z);
                    */

                    FPPlayer2p.SpawnExtraCharacter();
                    fpplayers = GetFPPlayers();
                    currentDataPage = DataPage.MULTIPLAYER_DEBUG;
                }
                else
                {
                    Log("Shift + F2 -> Attempted to start 2P but could not find 1P");
                }
            }

            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKToggleMultiCharStart))
            {
                multiplayerStart = !multiplayerStart;
                MultiCharStartLastSetting.Value = multiplayerStart;
                Log(String.Format("Toggle Multiplayer Start ({0} -> {1})", !multiplayerStart, multiplayerStart));
            }
            
            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKStartSplitscreen))
            {
                Log("Start Splitscreen");
                StartSplitscreen();
                Log("Start Splitscreen");
            }
            
            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKSwapBetweenSpawnedChars))
            {
                Log("Attempting to dump character info.");
                foreach (var fpp in fpplayers)
                {
                    DumpAllPlayerVarsAndComponents(fpp);
                } //DELETEME

                Log("Dumped character info.");

                FPPlayer2p.SwapBetweenActiveCharacters();
            }

            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKCyclePreferredAllyControlType))
            {
                FP2TrainerAllyControls.CyclePreferredAllyControlType();
            }
            
            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKStartInputPlayback))
            {
                Log("Attempting to force player to replay ghost.");
                skipRecording = true;
                fpplayer.inputMethod = fpplayer.HandleAllyControlsGhost;
                FP2TrainerAllyControls.needToLoadInputs = true;
                
                foreach (var fpp in fpplayers)
                {
                    DumpAllPlayerVarsAndComponents(fpp);
                }
            }
            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKToggleLockP1ToGhostFiles))
            {
                LockP1ToGhostFiles.Value = !LockP1ToGhostFiles.Value;
                Log($"Toggle Lock Player1 To Ghost Files: {!LockP1ToGhostFiles.Value} => {LockP1ToGhostFiles.Value}");

                if (LockP1ToGhostFiles.Value)
                {
                    Log("Attempting to force player to replay ghost.");
                    skipRecording = true;
                    fpplayer.inputMethod = fpplayer.HandleAllyControlsGhost;
                    FP2TrainerAllyControls.needToLoadInputs = true;
                
                    foreach (var fpp in fpplayers)
                    {
                        DumpAllPlayerVarsAndComponents(fpp);
                    }
                }
            }
        }

        private void HandleInstaSwapHotkeys()
        {
            if (!UseInstaSwitch.Value)
            {
                return;
            }

            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKSwitchCurrentPlayerToLilac))
            {
                currentPreferredCharacter = FPCharacterID.LILAC;
                fpplayer.characterID = FPCharacterID.LILAC;
                FPPlayer2p.PerformInstaSwap(currentPreferredCharacter);
            }
            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKSwitchCurrentPlayerToCarol))
            {
                currentPreferredCharacter = FPCharacterID.CAROL;
                fpplayer.characterID = FPCharacterID.CAROL;
                FPPlayer2p.PerformInstaSwap(currentPreferredCharacter);
            }
            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKSwitchCurrentPlayerToCarolBike))
            {
                currentPreferredCharacter = FPCharacterID.BIKECAROL;
                fpplayer.characterID = FPCharacterID.BIKECAROL;
                FPPlayer2p.PerformInstaSwap(currentPreferredCharacter);
            }
            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKSwitchCurrentPlayerToMilla))
            {
                currentPreferredCharacter = FPCharacterID.MILLA;
                fpplayer.characterID = FPCharacterID.MILLA;
                FPPlayer2p.PerformInstaSwap(currentPreferredCharacter);
            }
            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKSwitchCurrentPlayerToNeera))
            {
                currentPreferredCharacter = FPCharacterID.NEERA;
                fpplayer.characterID = FPCharacterID.NEERA;
                FPPlayer2p.PerformInstaSwap(currentPreferredCharacter);
            }
            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKSwitchCurrentPlayerToNext))
            {
                if (fpplayer.characterID >= FPCharacterID.NEERA)
                {
                    fpplayer.characterID = FPCharacterID.LILAC;
                }
                else
                {
                    fpplayer.characterID++;
                }
                
                currentPreferredCharacter = fpplayer.characterID;
                FPPlayer2p.PerformInstaSwap(currentPreferredCharacter);
            }
            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKSwitchCurrentPlayerToPrev))
            {
                if (fpplayer.characterID <= 0)
                {
                    fpplayer.characterID = FPCharacterID.NEERA;
                }
                else
                {
                    fpplayer.characterID--;
                }
                
                currentPreferredCharacter = fpplayer.characterID;
                FPPlayer2p.PerformInstaSwap(currentPreferredCharacter);
            }

        }

        private void HandleResizeFontHotkeys()
        {
            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKIncreaseFontSize))
            {
                Log("Shift + Plus -> Increase Font Size: ");
                if (textmeshFancyTextPosition != null) textmeshFancyTextPosition.characterSize++;
            }

            if (FP2TrainerCustomHotkeys.GetButtonDown(PHKDecreaseFontSize))
            {
                Log("Shift + Minus -> Decrease Font Size: ");
                if (textmeshFancyTextPosition != null) textmeshFancyTextPosition.characterSize--;
            }
        }

        private void HandleCameraHotkeys()
        {
            if (FP2TrainerCustomHotkeys.GetButton(PHKCameraZoomOut))
            {
                if (FPCamera.stageCamera != null)
                {
                    Log(String.Format("{0} -> Camera Zoom Out: {1} / {2}", PHKCameraZoomOut.Value,
                        FPCamera.stageCamera.GetZoom(), FPCamera.stageCamera.zoomMax));
                    trainerRequestZoomValue += 0.1f;
                    FPCamera.stageCamera.RequestZoom(trainerRequestZoomValue, FPCamera.ZoomPriority_VeryHigh);
                }
            }

            if (FP2TrainerCustomHotkeys.GetButton(PHKCameraZoomIn))
            {
                Log("Minus -> Camera Zoom In: ");
                if (FPCamera.stageCamera != null)
                {
                    Log(String.Format("{0} -> Camera Zoom In: {1} / {2}", PHKCameraZoomIn.Value,
                        FPCamera.stageCamera.GetZoom(), FPCamera.stageCamera.zoomMin));
                    trainerRequestZoomValue -= 0.1f;
                    FPCamera.stageCamera.RequestZoom(trainerRequestZoomValue, FPCamera.ZoomPriority_VeryHigh);
                }
            }

            if (FP2TrainerCustomHotkeys.GetButton(PHKCameraZoomReset))
            {
                Log("Numpad Period . -> Camera Reset: ");
                if (FPCamera.stageCamera != null)
                {
                    trainerRequestZoomValue = 1f;
                    FPCamera.stageCamera.RequestZoom(trainerRequestZoomValue);
                }
            }

            //FPCamera.stageCamera.RequestZoom(trainerRequestZoomValue);
        }

        public void HandleInvincibility()
        {
            if (EnableInvinciblePlayers.Value)
            {
                if (fpplayers != null)
                {
                    if (fpplayers.Count < 1)
                    {
                        fpplayers = GetFPPlayers();
                    }

                    foreach (var fpp in fpplayers)
                    {
                        fpp.health = fpp.healthMax;
                    }
                }

            }
            
            if (EnableInvincibleBoss.Value)
            {
                ReacquireBossHuds();
                foreach (var enemy in FPStage.GetActiveEnemies())
                {
                    enemy.health = 100;
                }
                
                if (bossHuds != null)
                {
                    foreach (var bossHud in bossHuds)
                    {
                        bossHud.targetBoss.health = bossHud.maxHealth;
                    }
                }

            }
            
            if (fpEnemies != null)
            {
                foreach (var boss in fpEnemies)
                {
                    boss.cannotBeKilled = EnableInvincibleBoss.Value;
                }
            }

        }

        private static bool InputGetKeyAnyShift()
        {
            return (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
        }

        private void ToggleNoClip()
        {
            if (noClip)
            {
                EndNoClip();
            }
            else
            {
                noClip = true;
                fpplayer.terrainCollision = false;
                noClipStartPos = fpplayer.position;
                noClipCollisionLayer = fpplayer.collisionLayer;
                noClipGravityStrength = fpplayer.gravityStrength;
            }
        }

        public void InstaKOPlayer()
        {
            if (fpplayer)
            {
                fpplayer.hurtKnockbackX = fpplayer.velocity.x;
                fpplayer.hurtKnockbackY = 0f;
                fpplayer.nextAttack = 0;
                fpplayer.genericTimer = -20f;
                fpplayer.superArmor = false;
                fpplayer.superArmorTimer = 0f;
                fpplayer.invincibilityTime = 200f;
                FPSaveManager.perfectRun = false;
                FPSaveManager.KOs++;
                fpplayer.recoveryTimer = 0f;
                fpplayer.state = fpplayer.State_KO;
                fpplayer.velocity.y = 8f;
                fpplayer.velocity.x *= 2f;
                fpplayer.Action_PlaySoundUninterruptable(fpplayer.sfxKO);
                fpplayer.Action_PlayVoice(fpplayer.vaKO);
            }
        }

        private void SimulateDPSDamageAdd()
        {
            Log("F2 -> Simulate DPS Damage Add: ");
            dpsTracker.AddDamage(5, "FP2 Trainer HotKey");
        }

        private void TestDamageNumberPopups()
        {
            Log("F1 -> Test Damage Number: ");
            if (fpplayer != null)
            {
                var fpcam = FPCamera.stageCamera;
                Log("1");
                if (fpcam != null)
                {
                    Log("2");
                    var relativePos = new Vector3(fpplayer.position.x - fpcam.xpos,
                        fpplayer.position.y - fpcam.ypos, fpplayer.gameObject.transform.position.z);
                    Log("3");
                    Log(relativePos.ToString());
                    Log("4");
                    var goDmgTest = FP2TrainerDamageNumber.CreateDMGNumberObject(relativePos, 5);
                    Log("5");
                }
            }
            else
            {
                Log("No player???");
            }
        }

        private void HandleNoClip()
        {
            //fpplayer.enablePhysics = false;

            if (noClip && fpplayer != null)
            {
                fpplayer.collisionLayer = -999;
                fpplayer.invincibilityTime = 100f;
                fpplayer.gravityStrength = 0;
                fpplayer.hitStun = -1;

                fpplayer.velocity.x = 0;
                fpplayer.velocity.y = 0;

                float modifiedNoClipMoveSpeed = noClipMoveSpeed;
                if (InputControl.GetButton(Controls.buttons.special))
                {
                    modifiedNoClipMoveSpeed *= 4f;
                }

                fpplayer.velocity = Vector2.zero;
                if (fpplayer.input.up
                    || InputControl.GetAxis(Controls.axes.vertical) > 0.2f)
                {
                    fpplayer.position.y += modifiedNoClipMoveSpeed * 1;
                }

                if (fpplayer.input.down
                    || InputControl.GetAxis(Controls.axes.vertical) < -0.2f)
                {
                    fpplayer.position.y -= modifiedNoClipMoveSpeed * 1;
                }

                if (fpplayer.input.right
                    || InputControl.GetAxis(Controls.axes.horizontal) > 0.2f)
                {
                    fpplayer.position.x += modifiedNoClipMoveSpeed * 1;
                }

                if (fpplayer.input.left
                    || InputControl.GetAxis(Controls.axes.horizontal) < -0.2f)
                {
                    fpplayer.position.x -= modifiedNoClipMoveSpeed * 1;
                }


                if (InputControl.GetButtonDown(Controls.buttons.attack))
                {
                    EndNoClip();
                }

                if (InputControl.GetButtonDown(Controls.buttons.jump))
                {
                    EndNoClipAndReturnToStartPosition();
                }
            }
        }

        private void EndNoClip()
        {
            fpplayer.invincibilityTime = 0f;
            fpplayer.gravityStrength = noClipGravityStrength;
            fpplayer.hitStun = 0f;
            fpplayer.collisionLayer = noClipCollisionLayer;
            fpplayer.terrainCollision = true;

            /*
            if (currentDataPage == DataPage.NO_CLIP)
            {
                currentDataPage++;
            }
            */

            noClip = false;

            //fpplayer.enablePhysics = true;
        }

        private void EndNoClipAndReturnToStartPosition()
        {
            fpplayer.position = noClipStartPos;
            EndNoClip();
        }

        private void ToggleVariableDisplay()
        {
            if (currentDataPage == DataPage.NONE)
                currentDataPage = DataPage.MOVEMENT;
            else
                currentDataPage++;

            UpdateAfterDataPageChange();
        }

        private void ToggleVariableDisplayPrevious()
        {
            if (currentDataPage == DataPage.MOVEMENT)
                currentDataPage = DataPage.NONE;
            else
                currentDataPage--;

            UpdateAfterDataPageChange();
        }

        private void IncrementInstructionPage()
        {
            if (currentInstructionPage < InstructionPage.NONE)
            {
                currentInstructionPage++;
            }

            UpdateAfterDataPageChange();
        }

        private void DecrementInstructionPage()
        {
            if (currentInstructionPage > InstructionPage.BASICS)
            {
                currentInstructionPage--;
            }

            UpdateAfterDataPageChange();
        }

        private void ResetInstructionPage()
        {
            currentInstructionPage = InstructionPage.BASICS;
            UpdateAfterDataPageChange();
        }

        private void UpdateAfterDataPageChange()
        {
            // After incrementing.
            if (currentDataPage == DataPage.NONE && !showInstructions && !(stageSelectMenu == null))
                showVarString = false;
            else
                showVarString = true;

            if (currentDataPage == DataPage.BOSS) ReacquireBossHuds();
            if (currentDataPage == DataPage.MULTIPLAYER_DEBUG)
            {
                fpplayers = GetFPPlayers();
            }
        }

        public void ReacquireBossHuds()
        {
            bossHuds = new List<FPBossHud>(Object.FindObjectsOfType<FPBossHud>());
            fpEnemies = new List<FPBaseEnemy>();
            foreach (var fpbh in bossHuds)
                if (fpbh != null && fpbh.targetBoss != null)
                    fpEnemies.Add(fpbh.targetBoss);
        }

        public static void LoadAssetBundlesFromModsFolder()
        {
            try
            {
                var pathApp = Application.dataPath;
                var pathMod = Path.Combine(Directory.GetParent(pathApp).FullName, "Mods");
                var pathModAssetBundles = Path.Combine(pathMod, "AssetBundles");

                var assetBundlePaths = Directory.GetFiles(pathModAssetBundles, "*.*");
                foreach (var abp in assetBundlePaths)
                {
                    Log(abp);
                    if (abp.Contains("."))
                    {
                        Log("Skipping this file, as it appears to have a " +
                            "file extension (.whatever) at the end, " +
                            "and is probably not an asset bundle.");
                        continue;
                    }

                    var currentAB = AssetBundle.LoadFromFile(abp);

                    if (currentAB == null)
                    {
                        Log("Failed to load AssetBundle. Bundle may already be loaded, or the file may be corrupt.");
                        continue;
                    }

                    //currentAB.LoadAllAssets(); //Uncomment if the scenes are still unloadable?
                    loadedAssetBundles.Add(currentAB);
                    Log("AssetBundle loaded successfully as loadedAssetBundles[" + (loadedAssetBundles.Count - 1) +
                        "]:");
                    Log("--------");
                    Log(currentAB.GetAllScenePaths().ToString());
                }
            }
            catch (NullReferenceException e)
            {
                Log("Null reference exception when trying to load asset bundles for modding. Canceling.");
                Log(e.StackTrace);
            }
        }

        public void ToggleLevelSelectVisibility()
        {
            if (stageSelectMenu != null)
                stageSelectMenu.SetActive(!stageSelectMenu.activeInHierarchy);
            // finna delete
            /*
                var ssm = stageSelectMenu.GetComponent<FPPauseMenu>();
                fptls = stageSelectMenu.AddComponent<FPTrainerLevelSelect>();
                if (ssm != null)
                {
                    UnityEngine.Object.Destroy(ssm);
                }
                if (fptls != null)
                {
                    UnityEngine.Object.Destroy(ssm);
                }
                */
            else
                Log("Attempted to toggle Level Select Visibility while it is not accessible.");
        }

        private void ShowLevelSelect(List<SceneNamePair> availableScenes)
        {
            if (stageSelectMenu != null)
            {
                Log("Level Select.");
                //stageSelectMenu.SetActive(true);

                fptls = stageSelectMenu.AddComponent<FPTrainerLevelSelect>();
                fptls.availableScenes = availableScenes;
                GameObject goButton = null;

                var tempGoButton = stageSelectMenu.transform.Find("AnnStagePlayIcon").gameObject;
                if (tempGoButton != null) goButton = tempGoButton;

                int i;

                fptls.pfButtons = new GameObject[availableScenes.Count];

                GameObject currentButton = null;
                TextMesh tm = null;
                MenuText mt = null;
                for (i = 0; i < availableScenes.Count; i++)
                {
                    currentButton = Object.Instantiate(goButton, stageSelectMenu.transform);
                    currentButton.transform.localPosition = new Vector3(0, -32 - 32 * i, 0);

                    tm = currentButton.GetComponent<TextMesh>();
                    mt = currentButton.GetComponent<MenuText>();
                    fptls.pfButtons.SetValue(currentButton, i);
                    if (tm != null)
                    {
                        tm.text = availableScenes[i].name;
                        mt.paragraph[0] = availableScenes[i].name;
                    }
                }

                Log("fptls button count: " + fptls.pfButtons.Length);
            }
            else
            {
                Log("Attempted to show level select, but the menu has not been prepared.");
            }

            PauseGameWithoutPauseMenu();
        }

        private void PauseGameWithoutPauseMenu()
        {
            FPStage.UpdateMenuInput();
            FPStage.SetStageRunning(false);
            FPAudio.ResumeSfx();
            FPAudio.PlayMenuSfx(2);
        }

        public void PerformStageTransition()
        {
            var component = GameObject.Find("Screen Transition").GetComponent<FPScreenTransition>();
            component.transitionType = FPTransitionTypes.LOCAL_WIPE;
            component.transitionSpeed = 48f;
            component.SetTransitionColor(0f, 0f, 0f);
            component.BeginTransition();
            FPAudio.PlayMenuSfx(3);
        }

        private static void WriteSceneObjectsToFile()
        {
            if (!warped)
            {
                warped = true;

                var allObjects = "";
                GameObject[] objs = GameObject.FindObjectsOfType<GameObject>();

                foreach (var obj in objs)
                {
                    allObjects += obj.name + " | " + obj.activeInHierarchy + "\r\n";
                    foreach (UnityEngine.MonoBehaviour mb in obj.GetComponents<MonoBehaviour>())
                    {
                        allObjects += "+MonoBehaviors: " + mb.GetType().Name + " | " + mb.isActiveAndEnabled + "\r\n";
                    }
                }
                // UMFGUI.AddConsoleText(allObjects);

                var fileName = "SceneObjects.txt";
                if (File.Exists(fileName))
                {
                    Debug.Log(fileName + " already exists.");
                    return;
                }

                var sr = File.CreateText(fileName);
                sr.WriteLine(allObjects);
                sr.Close();
            }
            else
            {
                sLogger.LogInfo("Warped already...");
            }
        }

        private void WriteAllAudioclipsToFile()
        {
            if (!warped)
            {
                warped = true;

                var allAudioClips = "";
                Object[] acs = Resources.FindObjectsOfTypeAll<AudioClip>();

                foreach (AudioClip ac in acs) allAudioClips += ac.name + "\r\n";
                // UMFGUI.AddConsoleText(allObjects);

                var fileName = "AllAvailableAudioClips.txt";
                if (File.Exists(fileName))
                {
                    Debug.Log(fileName + " already exists.");
                    return;
                }

                var sr = File.CreateText(fileName);
                sr.WriteLine(allAudioClips);
                sr.Close();
            }
            else
            {
                sLogger.LogInfo("Warped already...");
            }
        }

        public bool GetKeyPressed(string s)
        {
            try
            {
                var sTrim = s.Split('/')[1];
                //if (inputHandler != null)
                {
                    return false;
                    /*if (s.Contains("<Mouse>"))
                    {
                        return ((ButtonControl)Mouse.current[sTrim]).isPressed;
                    }
                    if (s.Contains("<Keyboard>"))
                    {
                        return ((KeyControl)Keyboard.current[sTrim]).isPressed;
                    }
                    if (s.Contains("<Gamepad>"))
                    {
                        return ((ButtonControl)Gamepad.current[sTrim]).isPressed;
                    }*/
                }
            }
            catch (Exception e)
            {
                // I should probably do a log here.
                sLogger.LogInfo(e.Message);
            }

            return false;
        }

        public bool GetKeyDown(string s)
        {
            try
            {
                var sTrim = s.Split('/')[1];
                //if (inputHandler != null)
                {
                    return false;

                    /*
                    if (s.Contains("<Mouse>"))
                    {
                        return ((ButtonControl)Mouse.current[sTrim]).wasPressedThisFrame;
                    }
                    if (s.Contains("<Keyboard>"))
                    {
                        return ((KeyControl)Keyboard.current[sTrim]).wasPressedThisFrame;
                    }
                    if (s.Contains("<Gamepad>"))
                    {
                        return ((ButtonControl)Gamepad.current[sTrim]).wasPressedThisFrame;
                    }*/
                }
            }
            catch (Exception e)
            {
                // I should probably do a log here.
                sLogger.LogInfo(e.Message);
            }

            return false;
        }

        public string GetCopiedTileName()
        {
            var result = "NULL";
            //f (copyTile != null) { result = copyTile.name; }
            return result;
        }

        public GameObject GetStageHUD()
        {
            var goHud = GameObject.Find("Stage HUD");
            if (goHud) sLogger.LogInfo("Found a Stage HUD.");
            //GameObject goPauseHud = GameObject.Find("Hud Pause Menu");

            return goHud;
        }

        public void SkipBootIntros()
        {
            var splash = Object.FindObjectOfType<MenuSplashScreen>();
            /*
            if (splash != null)
            {
                var propTimer = splash.GetType().GetField("timer", System.Reflection.BindingFlags.NonPublic
                                                                | System.Reflection.BindingFlags.Instance);
                propTimer.SetValue(splash, 9999);
            }*/

            if (introSkipped < 1)
            {
                string level = BootupLevel.Value;
                Log("BootupLevel: " + BootupLevel.Value);
                if (level != null && !level.Equals(""))
                {
                    GoToCustomBootLevelImmediate(level);
                }
                else
                {
                    GoToMainMenuNoLogos();
                }
            }

            /*
            if (introSkipped == 1)
            {
                string level = BootupLevel.Value;
                Log("BootupLevel: " + BootupLevel.Value);
                Log("level: " + level);
                if (level != null && !level.Equals(""))
                {
                    Log("1");
                    GoToCustomBootLevelImmediate(level);
                    
                }
                else
                {
                    Log("2");
                    GoToMainMenuNoLogos();
                }
            }
            */
        }

        public static void GoToMainMenuNoLogos()
        {
            GoToCustomBootLevel("MainMenu");
        }

        public static void GoToCustomBootLevel(string level)
        {
            Log("Now Loading Custom Boot: " + level);
            var component = GameObject.Find("Screen Transition").GetComponent<FPScreenTransition>();
            if (component != null)
            {
                component.transitionType = FPTransitionTypes.WIPE;
                component.transitionSpeed = 48f;
                component.sceneToLoad = level;
                FPSaveManager.menuToLoad = 2; // This is how we skip the intros.

                introSkipped++;
            }
        }

        public static void GoToCustomBootLevelImmediate(string level)
        {
            Log("Now Loading Custom Boot Immediate: " + level);
            var component = GameObject.Find("Screen Transition").GetComponent<FPScreenTransition>();
            if (component != null)
            {
                //component.transitionType = FPTransitionTypes.WIPE;
                //component.transitionSpeed = 48f;
                //component.sceneToLoad = level;
                //FPSaveManager.menuToLoad = 2; // This is how we skip the intros.

                SceneManager.LoadSceneAsync(level);

                introSkipped++;
            }
        }

        private IEnumerator LoadAsyncScene()
        {
            var asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Single);
            while (!asyncLoad.isDone) yield return null;
        }

        public static void SetFP2TDeltaTime(float dt)
        {
            fp2tDeltaTime = dt;
        }

        public static float GetFP2TDeltaTime()
        {
            return fp2tDeltaTime;
        }

        public void GrabAndUpdateCameraDetails()
        {
            if (FPCamera.stageCamera != null)
            {
                originalZoomMin = FPCamera.stageCamera.zoomMin;
                originalZoomMax = FPCamera.stageCamera.zoomMax;
                originalZoomSpeed = FPCamera.stageCamera.zoomSpeed;

                FPCamera.stageCamera.zoomMin = trainerZoomMin;
                FPCamera.stageCamera.zoomMax = trainerZoomMax;
                FPCamera.stageCamera.zoomSpeed = trainerZoomSpeed;
            }
        }

        public void GrabAndTweakPauseMenu()
        {
            TrainerPauseMenu.GrabAndTweakPauseMenu();
        }

        public void SpawnSpoilerGimmick()
        {
            /*
            if (cacheGameObjectHunter == null)
            {
                Log("~~~~1");
                GetReferencesToSpoilerGimmick();
                Log("~~~~2");
            }
            else
            {
                GetReferencesToSpoilerGimmickPart3();
            }
            */
        }

        private static void GetReferencesToSpoilerGimmick()
        {
            Scene currentScene = SceneManager.GetActiveScene();
            //SceneManager.LoadScene("Bakunawa5", LoadSceneMode.Additive);
            Log("@@@@@+");
            LoadAssetBundlesFromModsFolder();
            Log("@@@@@0");
            ListScenesToLog();
            SceneManager.LoadScene("SpoilerGimmick", LoadSceneMode.Additive);
            // The scenes will appear as available immediately, but will not actually load until the next update frame.
            waitingForNextFrameForSpoilerGimmick = true;


            /*
            Log("@@@@@3");
            if (goHunter != null)
            {
                Log("See you.");
                /*
                SceneManager.MoveGameObjectToScene(goHunter, SceneManager.GetActiveScene());
                SceneManager.MoveGameObjectToScene(goHunterKO, SceneManager.GetActiveScene());
                SceneManager.MoveGameObjectToScene(goArc, SceneManager.GetActiveScene());
                SceneManager.MoveGameObjectToScene(goMeter, SceneManager.GetActiveScene());
                */ /*
                
                GameObject.DontDestroyOnLoad(goHunter);
                GameObject.DontDestroyOnLoad(goHunterKO);
                GameObject.DontDestroyOnLoad(goArc);
                GameObject.DontDestroyOnLoad(goMeter);

                goHunter.transform.parent = cacheGameObjectHunter.transform;
                goHunterKO.transform.parent = cacheGameObjectHunter.transform;
                goArc.transform.parent = cacheGameObjectHunter.transform;
                goMeter.transform.parent = cacheGameObjectHunter.transform;
            }
            else
            {
                Log("@@@@@4");
                Log("Didn't find Hunter. Trying Approach 2.\n");
                foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
                {
                    if (go.name.Equals("Syntax Hunter"))
                    {
                        Log("Found Hunter.");
                        goHunter = go;
                    }
                    else if (go.name.Equals("HunterKOScreen"))
                    {
                        Log("Found HunterKO.");
                        goHunterKO = go;
                    }
                    else if (go.name.Equals("arc"))
                    {
                        Log("Found arc.");
                        goArc = go;
                    }
                    else if (go.name.Equals("Hud Stealth Meter"))
                    {
                        Log("Found Meter.");
                        goMeter = go;
                    }

                    if (goHunter && goHunterKO && goArc && goMeter)
                    {
                        Log("Found all.");
                        break;
                    }
                }
                Log("@@@@@5");
                Log(String.Format("Gimmick Objects: ({0},{1},{2},{3})\n", goHunter, goHunterKO, goArc, goMeter));
                Log("@@@@@6");
            }
            */
            //ListScenesToLog();
        }

        public static void GetReferencesToSpoilerGimmickPart2()
        {
            waitingForNextFrameForSpoilerGimmick = false;
            ListScenesToLog();
            Log("Are scenes loaded? " + SceneManager.GetSceneAt(0).isLoaded.ToString() +
                SceneManager.GetSceneAt(1).isLoaded.ToString());
            SceneManager.MergeScenes(SceneManager.GetSceneAt(0), SceneManager.GetSceneAt(1));

            //SceneManager.SetActiveScene(currentScene);

            Log("@@@@@1");
            ListScenesToLog();
            Log("@@@@@2");

            WriteSceneObjectsToFile();

            GameObject goHunter = GameObject.Find("Syntax Hunter");
            GameObject goHunterKO = GameObject.Find("HunterKO");
            GameObject goHunterKOScreen = GameObject.Find("HunterKOScreen");
            GameObject goArc = GameObject.Find("arc");
            GameObject goMeter = GameObject.Find("Hud Stealth Meter");

            Log("@@@@@3");
            Log(String.Format("Gimmick Objects: ({0},{1},{2},{3},{4})\n", goHunter, goHunterKO, goArc, goMeter,
                goHunterKOScreen));
            Log("@@@@@4");

            cacheGameObjectHunter = new GameObject("cacheHunter");

            goHunter.transform.parent = cacheGameObjectHunter.transform;
            goHunterKO.transform.parent = cacheGameObjectHunter.transform;
            goHunterKOScreen.transform.parent = cacheGameObjectHunter.transform;
            goArc.transform.parent = cacheGameObjectHunter.transform;
            goMeter.transform.parent = cacheGameObjectHunter.transform;

            //move this to part 2
            cacheGameObjectHunter.SetActive(false);
            Log("~~~~3");
            GameObject.DontDestroyOnLoad(cacheGameObjectHunter);
            Log("~~~~4");

            var temp = goHunter.transform.GetComponent<BFSyntaxHunt>();
            GameObject.Destroy(temp);

            var temp2 = goHunter.AddComponent<BFSyntaxHunt>();
            temp2.hudStealthMeter = goMeter.GetComponent<SpriteRenderer>();
            temp2.hudStealthMeterBar = goMeter.transform.Find("bar").GetComponent<SpriteRenderer>();
            temp2.koParent = goHunterKOScreen;
            temp2.body = goHunterKO;
            temp2.playerBody = goHunterKO.transform.Find("players").gameObject;
            temp2.dbWarn = goHunterKOScreen.transform.Find("DBWarn1").GetComponent<SpriteRenderer>();
            temp2.dbWarn2 = goHunterKOScreen.transform.Find("DBWarn2").GetComponent<SpriteRenderer>();


            GetReferencesToSpoilerGimmickPart3();
        }

        public static void GetReferencesToSpoilerGimmickPart3()
        {
            Log("~~~~5");
            var goNewHunter = GameObject.Instantiate(cacheGameObjectHunter);
            Log("~~~~6");
            goNewHunter.SetActive(true);
            //var goHunter = goNewHunter.transform.Find("Syntax Hunter").gameObject;
            //goHunter.gameObject.SetActive(true);
            Log("~~~~7");
            for (int i = 0; i < goNewHunter.transform.childCount; i++)
            {
                Transform child = goNewHunter.transform.GetChild(i);
                Log("~~~~8");
                Log(child.name + " " + child.transform.position + " " + child.gameObject.activeInHierarchy);
            }

            Log("~~~~9");
        }

        private static IEnumerable Suffering()
        {
            Log("tired");
            var ao = SceneManager.UnloadSceneAsync("Bakunawa5");
            yield return ao;
            Cursor.visible = false;
            //GameControl.player.SetActive(true);
            Log("Still Tired.");
            ListScenesToLog();
        }

        private static void ListScenesToLog()
        {
            Log("Scenes: vv look below vv");
            var qwer = SceneManager.sceneCount;
            for (int asdf = 0; asdf < qwer; asdf++)
            {
                Log(SceneManager.GetSceneAt(asdf).name);
            }
        }


        public static void StartMultiplayerHud(FPPlayer fpp)
        {
            //This SHOULD work with some tweaking. Dummied out so I can go to sleep.

            /*
            var gameObject = fpp.gameObject;
            var num = 0f;
            var maxPetals = Mathf.FloorToInt(fpp.healthMax);
            var hudLifePetals = new FPHudDigit[maxPetals];
            var isHudPetalFlashing = new bool[maxPetals];
            for (int i = 0; i < hudLifePetals.Length; i++)
            {
                gameObject = UnityEngine.Object.Instantiate(pfHudLifePetal, new Vector3(num, -31f, 0f), default(Quaternion));
                gameObject.transform.parent = base.transform; //Base in this case refers to the HUD Base.
                hudLifePetals[i] = gameObject.GetComponent<FPHudDigit>();
                num += 36f - (float)maxPetals * 2f;
            }
            float num2 = 88f - (float)maxPetals * 5f;
            var maxShields = maxPetals * 2;
            var hudShields = new FPHudDigit[maxShields];
            for (int j = 0; j < hudShields.Length; j++)
            {
                gameObject = UnityEngine.Object.Instantiate(pfHudShield, new Vector3(num2, -31f, -1f - (float)j * 0.1f), default(Quaternion));
                gameObject.transform.parent = base.transform;
                hudShields[j] = gameObject.GetComponent<FPHudDigit>();
                num2 += 18f - (float)maxPetals * 1f;
            }
            */
        }

        public void ReturnToCheckpoint()
        {
            if (FPStage.checkpointEnabled /*&& FPStage.currentStage.GetPlayerInstance_FPPlayer().state !=
                new FPObjectState(FPStage.currentStage.GetPlayerInstance_FPPlayer().State_KO)*/)
            {
                sceneToLoad = SceneManager.GetActiveScene().name;
                FPAudio.PlayMenuSfx(2);

                FPSaveManager.currentSave.Local_Restart();
                FPStage.checkpointEnabled = false;
                FPStage.checkpointPos = new Vector2(0f, 0f);
                FPSaveManager.stageDoorFlags = new bool[10];
                FPSaveManager.activatedDialogZones = new bool[10];
                FPSaveManager.bossRushId = 0;
            }
            else
            {
                RestartLevel();
            }
        }

        public void RestartLevel()
        {
            sceneToLoad = SceneManager.GetActiveScene().name;
            FPAudio.PlayMenuSfx(2);

            FPScreenTransition component = GameObject.Find("Screen Transition").GetComponent<FPScreenTransition>();
            component.transitionType = FPTransitionTypes.WIPE;
            component.transitionSpeed = 48f;
            component.sceneToLoad = sceneToLoad;
            component.SetTransitionColor(0f, 0f, 0f);
            component.BeginTransition();
            FPAudio.PlayMenuSfx(3);

            gameObject.AddComponent<SkipScreenTransitionCountdownBar>();
        }

        public static void Log(string txt)
        {
            if (sLogger == null)
            {
                return;
            }

            sLogger.LogInfo(txt);
        }
        
        public static void LogWarning(string txt)
        {
            if (sLogger == null)
            {
                return;
            }

            sLogger.LogWarning(txt);
        }
        
        public static void LogError(string txt)
        {
            if (sLogger == null)
            {
                return;
            }

            sLogger.LogError(txt);
        }

        public static void DumpSpriteRendererVars(GameObject go)
        {
            var fppVars = "";
            var sren = go.GetComponent<SpriteRenderer>();


            /*
            fppVars += String.Format(
                "{0}:\r\n" +
                "{1} = {2}\r\n"));
                */

            /*
            fppVars += $"{go.name}: \r\n" +
                       $"{nameof(sren.blah)} : {sren.barTimer}" +
                       $"{nameof(go.inputLock)} : {go.inputLock}" +
                       $"{nameof(go.idleTimer)} : {go.idleTimer}" +
                       $"{nameof(go.targetGimmick)} : {go.targetGimmick}" +
                       $"{nameof(sren.targetWaterSurface)} : {sren.targetWaterSurface}" +
                       $"{nameof(sren.chaseMode)} : {sren.chaseMode}" +
                       $"{nameof(sren.swapCharacter)} : {sren.swapCharacter}" +
                       $"{nameof(sren.hideChildObject)} : {sren.hideChildObject}" +
                       $"{nameof(sren.lastGround)} : {sren.lastGround}" +
                       $"{nameof(sren.lastSafePosition)} : {sren.lastSafePosition}";
                       
                       */

            // UMFGUI.AddConsoleText(allObjects);

            var fileName = "fppVars.txt";
            if (File.Exists(fileName))
            {
                Debug.Log(fileName + " already exists.");
                return;
            }

            var sr = File.CreateText(fileName);
            sr.WriteLine(fppVars);
            sr.Close();
        }

        public static void DumpAllPlayerVarsAndComponents(FPPlayer fpp)
        {
            var fppVars = "";
            var components = fpp.gameObject.GetComponents<Component>();

            fppVars += $"{fpp.name} - {fpp.GetInstanceID()}\n";
            foreach (var component in components)
            {
                fppVars += $"-----({component.name})-----\n";
                var fields = component.GetType().GetFields(BindingFlags.NonPublic |
                                                           BindingFlags.Instance);
                foreach (var field in fields)
                {
                    fppVars += $"{field.Name} = {field.GetValue(component)}\n";
                }

                fppVars += "\n";
            }

            var fileName = "fppVarsAndComponents.txt";
            if (File.Exists(fileName))
            {
                Debug.Log(fileName + " already exists.");
                return;
            }

            var sr = File.AppendText(fileName);
            sr.WriteLine(fppVars);
            sr.Close();
        }

        public static void ToggleSplitScreen()
        {
            EnableSplitScreen.Value = !EnableSplitScreen.Value;
            Log("Toggle Splitscreen... NOT IMPLEMENTED YET.");
        }

        public static void StartSplitscreen()
        {
            foreach (var ssci in SplitScreenCameraInfos)
            {
                ssci.SplitCamRenderTexture.Release();
            }

            SplitScreenCameraInfos.Clear();

            try
            {
                var numPlayers = fpplayers.Count;
                var sortedFPPlayers = fpplayers.OrderBy(fpp => fpp.characterID).ToList();
                EnableSplitScreen.Value = true;
                
                // Memo to go back and dispose these objects since we're creating new ones for player 1 as well...
                // Would also be nice to duplicate the HUD to give everyone their own...
                var goStageCamera = GameObject.Find("Stage Camera"); Log($"{goStageCamera}");
                var goRenderCamera = GameObject.Find("Render Camera"); Log($"{goRenderCamera}");
                var goPixelArtTarget = GameObject.Find("Pixel Art Target");  Log($"{goPixelArtTarget}");// has render cam as child object.
            
                var stageCamera = goStageCamera.GetComponent<FPCamera>(); Log($"{stageCamera}");
                var renderCamera = goRenderCamera.GetComponent<FPCameraFit>(); Log($"{renderCamera}");
                var pixelArtTarget = goPixelArtTarget.GetComponent<MeshRenderer>(); Log($"{pixelArtTarget}");

                var hud = GameObject.Find("Stage HUD");
                
                
                
                for (int p = 0; p < numPlayers; p++) 
                {
                    var cameraRect = SplitScreenCamInfo.GetCamRectByPlayerIndexAndCount(p, numPlayers);
                    Log($"Rect: {cameraRect}");
                    // Short verison for first player.
                    /*
                    if (p == 0)
                    {
                        SplitScreenCameraInfos.Add(new SplitScreenCamInfo(stageCamera, goRenderCamera, stageCamera.renderTarget)); // First cam is pretty much guarenteed.
                        stageCamera.target = fpplayers[p];
                        stageCamera.targetPlayer = fpplayers[p];
                        goRenderCamera.GetComponent<Camera>().rect = new Rect(cameraRect);
                        continue;
                    }
                    */

                    var goSplitScreenPixelArtTarget = GameObject.Instantiate(goPixelArtTarget);  Log($"{goSplitScreenPixelArtTarget}");//shouldn't we be using the FPStage instantiate instead???
                    //var goSplitScreenRenderCamera = goSplitScreenPixelArtTarget.transform.Find("Render Camera (Clone)"); Log($"{goSplitScreenRenderCamera}");
                    //var goSplitScreenRenderCamera = GameObject.Find("Render Camera (Clone)"); Log($"{goSplitScreenRenderCamera}");
                    var goSplitScreenRenderCamera = goSplitScreenPixelArtTarget.transform.GetChild(0); Log($"{goSplitScreenRenderCamera}");
                    /*
                    for (int i = 0; i < goSplitScreenPixelArtTarget.transform.childCount; i++)
                    {
                        Log($"New Pixel Art Target Children: {goSplitScreenPixelArtTarget.transform.GetChild(i).gameObject.ToString()}");
                    }*/

                    var goSplitScreenStageCamera = GameObject.Instantiate(goStageCamera);
                    
                    //var splitScreenRenderCamera = goSplitScreenRenderCamera.GetComponent<FPCameraFit>(); 
                
                    var splitScreenStageCamera = goSplitScreenStageCamera.GetComponent<FPCamera>();

                    SplitScreenCameraInfos.Add(new SplitScreenCamInfo(splitScreenStageCamera, goSplitScreenRenderCamera.gameObject, splitScreenStageCamera.renderTarget));
                    
                    /*
                     *SplitScreenCameraInfos.Add(new SplitScreenCamInfo(stageCamera, goRenderCamera.GetComponent<Camera>()));
                    SplitScreenCameraInfos.Add(new SplitScreenCamInfo(splitScreenStageCamera, goSplitScreenRenderCamera.GetComponent<Camera>()));
                     * 
                     */
                    
                    splitScreenStageCamera.renderTarget = new RenderTexture(stageCamera.renderTarget.width, stageCamera.renderTarget.height, stageCamera.renderTarget.depth, stageCamera.renderTarget.format);
                    // Reminder: RenderTextures are not auto-disposed. I should probably create and cache these at the start for reuse throughout the game rather than creating them on the fly.
                    
                    
                    // Move down to not overlap.
                    goSplitScreenPixelArtTarget.transform.position +=
                        new Vector3(0, goPixelArtTarget.transform.localScale.y * (p + 1), 0); Log($"{goSplitScreenPixelArtTarget}");
                
                    // Set the material on the new render target to be unique and use the new renderTexture we just made.
                    goSplitScreenPixelArtTarget.GetComponent<MeshRenderer>().material.mainTexture =
                        splitScreenStageCamera.renderTarget;
                    
                    // Set the targets to the players
                    if (numPlayers > 1)
                    {
                        // StageCamera has a SetCameraTarget method, but it's static and assumes one camera so we don't use it.
                        splitScreenStageCamera.target = sortedFPPlayers[p];
                        splitScreenStageCamera.targetPlayer = sortedFPPlayers[p];
                        Log($"Set new target to sortedFPPlayers[p] p:{p} fpp: {splitScreenStageCamera.target} : {splitScreenStageCamera.target.name}");
                    }
                    else
                    {
                        splitScreenStageCamera.target = stageCamera.target; Log($"{splitScreenStageCamera}");
                        Log($"Set target to {splitScreenStageCamera.target}, the original player.");
                    }

                    //cameraRect = SplitScreenCamInfo.GetCamRectByPlayerIndexAndCount(p, numPlayers);
                    var goSplitScreenRenderCameraCamComponent = goSplitScreenRenderCamera.GetComponent<Camera>();
                    var layerName = "BG Layer " + (15 - p);
                    LayerMask layerMask = LayerMask.GetMask(new string[]{layerName});
                    Debug.Log("Moving HUD to layer: " + layerName + " -> " + layerMask);

                    var delayedCullMaskSetter = goSplitScreenStageCamera.AddComponent<LockCameraCullingMask>();
                    delayedCullMaskSetter.cullingMaskValue = layerMask.value;

                    //var newHud = Instantiate(hud);
                    //newHud.name += "Player" + p;
                    var newHud = CloneHealthBar(sortedFPPlayers[p]);
                    Debug.Log(newHud);
                    newHud.layer = LayerMask.NameToLayer(layerName);
                    SetLayerInAllChildren(newHud, newHud.layer);
                    Debug.Log("Omigor Childs");


                    //"UI display"

                    goSplitScreenRenderCameraCamComponent.rect = new Rect(cameraRect);
                    
                }

                if (numPlayers > 2)
                {
                    FPSaveManager.SetResolution(640 * 2, 360 * 2);
                }
                else if (numPlayers == 2)
                {
                    FPSaveManager.SetResolution(640, 360 * 2);
                }
                else
                {
                    FPSaveManager.SetResolution(640, 360 * 1);
                }

                /*goSplitScreenPixelArtTarget.transform.position +=
                    new Vector3(0, 360, 0); Log($"{goSplitScreenPixelArtTarget}");*/
                
                    //DEBUG
                    //goRenderCamera.GetComponent<Camera>().rect = new Rect(0, 0, 1, 1); Log($"{goRenderCamera.GetComponent<Camera>().rect}");
                    //goSplitScreenRenderCamera.GetComponent<Camera>().rect = new Rect(0, 0, 1, 1); Log($"{goSplitScreenRenderCamera.GetComponent<Camera>().rect}");

                    //END DEBUG
                
                // FPCamera.CreateNewCamera is used to make Lighting cameras, but I don't know when or where it's used so for now it's not factored into this. Fix later.

                if (millasToyboxInstance.pauseMenu != null)
                {
                    var newPauseLayerName = "BG Layer " + (15 - 4);
                    LayerMask layerMaskForPause = LayerMask.GetMask(new string[]{newPauseLayerName});
                    millasToyboxInstance.pauseMenu.gameObject.layer = layerMaskForPause.value;

                    foreach (var goPauseItem in millasToyboxInstance.pauseMenu.pfButtons)
                    {
                        goPauseItem.layer = layerMaskForPause.value;
                    }
                    foreach (var goPauseItem in millasToyboxInstance.pauseMenu.pfItemBox)
                    {
                        goPauseItem.layer = layerMaskForPause.value;
                    }
                }

            }
            catch (Exception e)
            {
                sLogger.LogError($"{e.ToString()}\n{e.Message}\n{e.StackTrace}");
            }
        }

        public static void SetLayerInAllChildren(GameObject go, int layer)
        {
            for (int k = 0; k < go.transform.childCount; k++)
            {
                var childLayer1 = go.transform.GetChild(k);
                childLayer1.gameObject.layer = layer;
                
                SetLayerInAllChildren(childLayer1.gameObject, layer);
                
                /*
                for (int l = 0; l < childLayer1.transform.childCount; l++)
                {
                    childLayer1.transform.GetChild(l).gameObject.layer = layer;
                }*/
            }
        }

        public static void UpdateSplitScreens()
        {
            try
            {
                return;
                foreach (var camInfo in SplitScreenCameraInfos)
                {
                    camInfo.RenderCamera = camInfo.GoRenderCamera.GetComponent<Camera>();
                    sLogger.LogInfo($"UpdateRenderCam: {camInfo.RenderCamera}");
                    if (camInfo.FpCamera.lightingCamera != null)
                    {
                        camInfo.FpCamera.lightingCamera.rect = camInfo.RenderCamera.rect;
                        camInfo.FpCamera.lightingCamera.targetTexture = camInfo.SplitCamRenderTexture;
                    }


                    ParallaxLayer pl = null;
                    var highestLayerDepth = -1f;
                    int indexOfHighestLayerCam = -1;
                    bool flag = false;
                    
                    for (int pli = 0; pli < camInfo.FpCamera.parallaxLayers.Length; pli++)
                    {
                        // Set the camera rects to match player rect.
                        pl = camInfo.FpCamera.parallaxLayers[pli];
                        if (pl != null && pl.cam != null)
                        {
                            pl.cam.rect = camInfo.RenderCamera.rect;
                            pl.cam.targetTexture = camInfo.SplitCamRenderTexture; //Causes both views to stop clearing properly...

                            // Imitate CameraStart for handling Lighting and Foreground
                            if (pl.layerMask != StageLayerIDs.LIGHTING)
                            {
                                pl.cam.targetTexture = camInfo.FpCamera.renderTarget;
                                pl.cam.clearFlags = CameraClearFlags.Nothing;
                            }
                            else
                            {
                                pl.cam.targetTexture = camInfo.FpCamera.lightingTarget;
                                pl.cam.clearFlags = CameraClearFlags.Color;
                                pl.cam.backgroundColor = camInfo.FpCamera.shadowTint;
                                camInfo.FpCamera.lightingCamera = pl.cam;
                                flag = true;
                            }
                            if (pl.layerMask == StageLayerIDs.FG_PLANE)
                            {
                                pl.cam.cullingMask = 3856;
                            }
                            
                            // Get layer with highest depth.
                            if ( pl.cam.depth > highestLayerDepth)
                            {
                                highestLayerDepth = pl.cam.depth;
                                indexOfHighestLayerCam = pli;
                            }
                            
                            //UI Cam is affected by Lighting flag?
                            //... except the UI cam isn't accessible.
                            /*
                            if (!flag)
                            {
                                camInfo.FpCamera.uiCam.targetTexture = camInfo.FpCamera.renderTarget;
                                camInfo.FpCamera.uiCam.clearFlags = CameraClearFlags.Nothing;
                            }
                            else
                            {
                                camInfo.FpCamera.uiCam.targetTexture = camInfo.FpCamera.uiTarget;
                                camInfo.FpCamera.uiCam.clearFlags = CameraClearFlags.Color;
                                camInfo.FpCamera.uiCam.backgroundColor = Color.clear;
                            }
                            */
                        }
                        else
                        {
                            Log("funky parallax null");
                        }
                    }
                    pl = camInfo.FpCamera.parallaxLayers[indexOfHighestLayerCam];
                    pl.cam.clearFlags = CameraClearFlags.Color;
                    
                    
                    int num = -1;
                    float num2 = -1f;
                    for (int j = 0; j < camInfo.FpCamera.parallaxLayers.Length; j++)
                    {
                        //var pl = camInfo.FpCamera.parallaxLayers[j];
                        //pl.cam.rect = camInfo.RenderCamera.rect;
                        //pl.cam.targetTexture = camInfo.RenderCamera.targetTexture; //Causes both views to stop clearing properly...
                        
                        if (camInfo.FpCamera.parallaxLayers[j].layerDepth > num2)
                        {
                            num2 = camInfo.FpCamera.parallaxLayers[j].layerDepth;
                            num = j;
                        }
                    }
                    if (num > -1)
                    {
                        camInfo.FpCamera.parallaxLayers[num].cam.clearFlags = CameraClearFlags.Color;
                    }
                }
            }
            catch (Exception e)
            {
                sLogger.LogError($"{e}\n{e.Message}\n{e.StackTrace}");
            }
        }

        public static Vector3 GetPositionRelativeToCamera(FPCamera cam, Vector3 pos)
        {
            return pos - cam.transform.position;
        }

        /*
        public void OverwriteDeltaTime(float replacementDeltaTime)
        {
            var props = typeof(Time).GetProperties(BindingFlags.NonPublic |
                                                       BindingFlags.Instance);
            PropertyInfo prop = typeof(Time).GetProperty("deltaTime", BindingFlags.NonPublic |
                                                  BindingFlags.Instance);
            Log($"before prop (deltaTime): {prop.ToString()}");
            prop.SetValue(null, replacementDeltaTime);
            Log($"after prop (deltaTime): {prop.ToString()}");
        }
        */

        /*
        private void Awake()
        {
            // MillasToybox startup logic
            InitConfigs();
            
            Logger.LogInfo($"PluginParametersSet {MyPluginInfo.PLUGIN_GUID} is loaded!");
            Logger.LogInfo($"The value of {configGreeting.Definition.Key} in config file is {configGreeting.Value}.");
        }

        private void InitConfigs()
        {
            configGreeting = Config.Bind("General",      // The section under which the option is shown
                "GreetingText",  // The key of the configuration option in the configuration file
                "Hello, world!", // The default value
                "A greeting text to show when the game is launched"); // Description of the option to show in the config file

            configDisplayGreeting = Config.Bind("General.Toggles", 
                "DisplayGreeting",
                true,
                "Whether or not to show the greeting text");
            
            configLastKnownScene = Config.Bind("General",      // The section under which the option is shown
                "LastKnownScene",  // The key of the configuration option in the configuration file
                "Zao Land", // The default value
                "The scene name of the last scene that was active in-game before closing."); // Description of the option to show in the config file
        }

        private void Update()
        {
            previousSceneName = activeSceneName;
            activeSceneName = SceneManager.GetActiveScene().name;
            if (!activeSceneName.Equals(previousSceneName))
            {
                Logger.LogInfo($"Scene name changed from {previousSceneName} to {activeSceneName}.");
                Logger.LogInfo($"Saving last scene name to config file.");

                configLastKnownScene.Value = activeSceneName;
            }
        }
        */
        
        public float[] AssumeLRTBForPlayer(FPPlayer fpp)
        {
            float[] lrtb = new float[]
            {
                fpp.position.x - 320,
                fpp.position.x + 320,
                fpp.position.y - 180,
                fpp.position.y + 180
            };
            return lrtb;
        }
        
        public float[] AssumeLRTBForCamera(FPCamera fpc)
        {
            float[] lrtb = new float[]
            {
                fpc.left,
                fpc.right,
                fpc.top,
                fpc.bottom
            };
            return lrtb;
        }

        public bool GetActiveInHierarchy(GameObject go)
        {
            return go.activeInHierarchy;
        }
        
        public static void SetActiveSafe(GameObject obj, bool value)
        {
            if (obj.activeSelf != value)
            {
                obj.SetActive(value);
            }
        }

        public void UpdateObjectActivationForNonLeadPlayer(FPPlayer fpp)
        {
            if (fpp != FPStage.currentStage.GetPlayerInstance_FPPlayer())
            {
                UpdateObjectActivationForLRTB(AssumeLRTBForPlayer(fpp));
            }
        }
        
        public void UpdateObjectActivationForAllPlayers(List<FPPlayer> fpps)
        {
            List<LeftRightTopBottom> activeZones = new List<LeftRightTopBottom>();
            
            float[] activationAreas;
            LeftRightTopBottom lrtb;
            
            foreach (var fpp in fpps)
            {
                activationAreas = AssumeLRTBForPlayer(fpp);
                lrtb = new LeftRightTopBottom();
                lrtb.left = activationAreas[0];
                lrtb.right = activationAreas[1];
                lrtb.top = activationAreas[2];
                lrtb.bottom = activationAreas[3];
                activeZones.Add(lrtb);
            }

            UpdateObjectActivationForMultiLRTB(activeZones);
        }
        
        public void UpdateObjectActivationForFPStageCameras()
        {
            List<LeftRightTopBottom> activeZones = new List<LeftRightTopBottom>();
            
            float[] activationAreas;
            LeftRightTopBottom lrtb;
            
            foreach (var fpc in GameObject.FindObjectsOfType<FPCamera>())
            {
                activationAreas = AssumeLRTBForCamera(fpc);
                lrtb = new LeftRightTopBottom();
                lrtb.left = fpc.left;
                lrtb.right = fpc.right;
                lrtb.top = fpc.top;
                lrtb.bottom = fpc.bottom;
                activeZones.Add(lrtb);
            }

            UpdateObjectActivationForMultiLRTB(activeZones);
        }
        
        public void UpdateObjectActivationForMultiLRTB(List<LeftRightTopBottom> activationZones)
	    {
            // Unlike the original version of this, we actively skip deactivating any objects as the main P1 script will handle deactivating things without conflicting too much here.
            try
            {
                Type tfpStage = FPStage.currentStage.GetType();
                
                var stageObjList = (FPBaseObject[])(tfpStage.GetField("stageObjList", BindingFlags.Static | BindingFlags.NonPublic).GetValue(tfpStage));
                var stageObjTypeList = (FPObjectPool[])(tfpStage.GetField("stageObjTypeList", BindingFlags.Static | BindingFlags.NonPublic).GetValue(tfpStage));
                var stageObjPrevActiveState = (bool[])(tfpStage.GetField("stageObjPrevActiveState", BindingFlags.Static | BindingFlags.NonPublic).GetValue(tfpStage));
                //var currentStage = FPStage.currentStage;

                var stageObjTypeCount = (int)(tfpStage.GetField("stageObjTypeCount", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(FPStage.currentStage));
                var stageObjSpawnEnd = (int)(tfpStage.GetField("stageObjSpawnEnd", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(FPStage.currentStage));
                
		        Vector2 vector = default(Vector2);
		        int num = stageObjList.Length;
		        bool flag = false;
                bool isInZone = false;
		        for (int i = 0; i < stageObjTypeCount; i++)
		        {
			        stageObjTypeList[i].activeListSize = 0;
		        }
		        for (int i = 0; i < stageObjSpawnEnd; i++)
		        {
			        if (stageObjList[i] != null)
			        {
				        stageObjPrevActiveState[i] = GetActiveInHierarchy(stageObjList[i].gameObject);
			        }
		        }
		        for (int i = 0; i < stageObjSpawnEnd; i++)
		        {
			        if (stageObjList[i] != null)
			        {
				        if (stageObjList[i].stageListPos > -1)
				        {
					        if (stageObjList[i].allowFloatPositions)
					        {
						        Vector3 position = stageObjList[i].transform.position;
						        vector.x = position.x;
						        Vector3 position2 = stageObjList[i].transform.position;
						        vector.y = position2.y;
					        }
					        else
					        {
						        vector = stageObjList[i].position;
					        }
					        switch (stageObjList[i].activationMode)
					        {
					        case FPActivationMode.NEVER_ACTIVE:
						        SetActiveSafe(stageObjList[i].gameObject, value: false);
						        break;
					        case FPActivationMode.ALWAYS_ACTIVE:
						        SetActiveSafe(stageObjList[i].gameObject, value: true);
						        break;
					        case FPActivationMode.XY_RANGE:
                                isInZone = IsObjectInAnyActivationZone(activationZones, stageObjList[i], vector, stageObjList[i].activationMode);
                                if (isInZone)
						        {
							        SetActiveSafe(stageObjList[i].gameObject, value: true);
						        }
						        else
						        {
							        SetActiveSafe(stageObjList[i].gameObject, value: false);
						        }
						        break;
					        case FPActivationMode.X_RANGE:
                                isInZone = IsObjectInAnyActivationZone(activationZones, stageObjList[i], vector, stageObjList[i].activationMode);
						        if (isInZone)
						        {
							        SetActiveSafe(stageObjList[i].gameObject, value: true);
						        }
						        else
						        {
							        //SetActiveSafe(stageObjList[i].gameObject, value: false);
						        }
						        break;
					        case FPActivationMode.Y_RANGE:
                                isInZone = IsObjectInAnyActivationZone(activationZones, stageObjList[i], vector, stageObjList[i].activationMode);
						        if (isInZone)
						        {
							        SetActiveSafe(stageObjList[i].gameObject, value: true);
						        }
						        else
                                {
                                    //SetActiveSafe(stageObjList[i].gameObject, value: false);
						        }
						        break;
					        case FPActivationMode.XY_INVERT:
                                isInZone = !IsObjectInAnyActivationZone(activationZones, stageObjList[i], vector, FPActivationMode.XY_RANGE);
                                // Possibly bug here, but let's assume this works.
                                if (isInZone)
						        {
							        SetActiveSafe(stageObjList[i].gameObject, value: false);
						        }
						        else
						        {
							        SetActiveSafe(stageObjList[i].gameObject, value: true);
						        }
						        break;
					        }
				        }
				        else
				        {
					        SetActiveSafe(stageObjList[i].gameObject, value: false);
				        }
			        }
			        else
			        {
				        stageObjPrevActiveState[i] = false;
			        }
		        }
		        for (int i = 0; i < stageObjSpawnEnd; i++)
		        {
			        if (!(stageObjList[i] != null))
			        {
				        continue;
			        }
			        flag = GetActiveInHierarchy(stageObjList[i].gameObject);
			        if (flag)
			        {
				        //AddObjectToTypePoolActiveList(stageObjList[i]);
                        var mAddObjectToTypePoolActiveList = tfpStage.GetMethod("AddObjectToTypePoolActiveList",
                            BindingFlags.Static | BindingFlags.NonPublic);
                        var parms = new[] { stageObjList[i] };
                        mAddObjectToTypePoolActiveList.Invoke(null, BindingFlags.Static | BindingFlags.NonPublic, null, parms, CultureInfo.CurrentCulture);
                    }
			        if (stageObjPrevActiveState[i] != flag)
			        {
				        if (flag)
				        {
					        stageObjList[i].OnActivation();
				        }
				        else
				        {
					        stageObjList[i].OnDeactivation();
				        }
			        }
		        }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            
	    }

        public static bool IsObjectInAnyActivationZone(List<LeftRightTopBottom> activationZones, FPBaseObject stageObject,
            Vector2 vector, FPActivationMode activationMode)
        {
            bool isInZone;
            isInZone = false;
            switch (activationMode)
            {
                case FPActivationMode.XY_RANGE: 
                    foreach (var zone in activationZones)
                    {
                        if (zone.left - stageObject.activationRange.x <= vector.x &&
                            vector.x <= zone.right + stageObject.activationRange.x &&
                            zone.bottom - stageObject.activationRange.y <= vector.y &&
                            vector.y <= zone.top + stageObject.activationRange.y)
                        {
                            isInZone = true;
                        }

                        break;
                    }
                    break;
                
                case FPActivationMode.X_RANGE: 
                    foreach (var zone in activationZones)
                    {
                        if (zone.left - stageObject.activationRange.x <= vector.x &&
                            vector.x <= zone.right + stageObject.activationRange.x)
                        {
                            isInZone = true;
                        }

                        break;
                    }
                    break;
                
                case FPActivationMode.Y_RANGE: 
                    foreach (var zone in activationZones)
                    {
                        if (zone.bottom - stageObject.activationRange.y <= vector.y &&
                            vector.y <= zone.top + stageObject.activationRange.y)
                        {
                            isInZone = true;
                        }

                        break;
                    }
                    break;
            }
            foreach (var zone in activationZones)
            {
                if (zone.left - stageObject.activationRange.x <= vector.x &&
                    vector.x <= zone.right + stageObject.activationRange.x &&
                    zone.bottom - stageObject.activationRange.y <= vector.y &&
                    vector.y <= zone.top + stageObject.activationRange.y)
                {
                    isInZone = true;
                }

                break;
            }

            return isInZone;
        }

        public void UpdateObjectActivationForLRTB(float[] activationArea)
	    {
            // Unlike the original version of this, we actively skip deactivating any objects as the main P1 script will handle deactivating things without conflicting too much here.
            try
            {
                float left = activationArea[0];
		        float right = activationArea[1];
		        float top = activationArea[2];
		        float bottom = activationArea[3];

                Type tfpStage = FPStage.currentStage.GetType();
                
                var stageObjList = (FPBaseObject[])(tfpStage.GetField("stageObjList", BindingFlags.Static | BindingFlags.NonPublic).GetValue(tfpStage));
                var stageObjTypeList = (FPObjectPool[])(tfpStage.GetField("stageObjTypeList", BindingFlags.Static | BindingFlags.NonPublic).GetValue(tfpStage));
                var stageObjPrevActiveState = (bool[])(tfpStage.GetField("stageObjPrevActiveState", BindingFlags.Static | BindingFlags.NonPublic).GetValue(tfpStage));
                //var currentStage = FPStage.currentStage;

                var stageObjTypeCount = (int)(tfpStage.GetField("stageObjTypeCount", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(FPStage.currentStage));
                var stageObjSpawnEnd = (int)(tfpStage.GetField("stageObjSpawnEnd", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(FPStage.currentStage));
                
		        Vector2 vector = default(Vector2);
		        int num = stageObjList.Length;
		        bool flag = false;
		        for (int i = 0; i < stageObjTypeCount; i++)
		        {
			        stageObjTypeList[i].activeListSize = 0;
		        }
		        for (int i = 0; i < stageObjSpawnEnd; i++)
		        {
			        if (stageObjList[i] != null)
			        {
				        stageObjPrevActiveState[i] = GetActiveInHierarchy(stageObjList[i].gameObject);
			        }
		        }
		        for (int i = 0; i < stageObjSpawnEnd; i++)
		        {
			        if (stageObjList[i] != null)
			        {
				        if (stageObjList[i].stageListPos > -1)
				        {
					        if (stageObjList[i].allowFloatPositions)
					        {
						        Vector3 position = stageObjList[i].transform.position;
						        vector.x = position.x;
						        Vector3 position2 = stageObjList[i].transform.position;
						        vector.y = position2.y;
					        }
					        else
					        {
						        vector = stageObjList[i].position;
					        }
					        switch (stageObjList[i].activationMode)
					        {
					        case FPActivationMode.NEVER_ACTIVE:
						        //SetActiveSafe(stageObjList[i].gameObject, value: false);
						        break;
					        case FPActivationMode.ALWAYS_ACTIVE:
						        SetActiveSafe(stageObjList[i].gameObject, value: true);
						        break;
					        case FPActivationMode.XY_RANGE:
						        if (left - stageObjList[i].activationRange.x <= vector.x && vector.x <= right + stageObjList[i].activationRange.x && bottom - stageObjList[i].activationRange.y <= vector.y && vector.y <= top + stageObjList[i].activationRange.y)
						        {
							        SetActiveSafe(stageObjList[i].gameObject, value: true);
						        }
						        else
						        {
							        //SetActiveSafe(stageObjList[i].gameObject, value: false);
						        }
						        break;
					        case FPActivationMode.X_RANGE:
						        if (left - stageObjList[i].activationRange.x <= vector.x && vector.x <= right + stageObjList[i].activationRange.x)
						        {
							        SetActiveSafe(stageObjList[i].gameObject, value: true);
						        }
						        else
						        {
							        //SetActiveSafe(stageObjList[i].gameObject, value: false);
						        }
						        break;
					        case FPActivationMode.Y_RANGE:
						        if (bottom - stageObjList[i].activationRange.y <= vector.y && vector.y <= top + stageObjList[i].activationRange.y)
						        {
							        SetActiveSafe(stageObjList[i].gameObject, value: true);
						        }
						        else
                                {
                                    //SetActiveSafe(stageObjList[i].gameObject, value: false);
						        }
						        break;
					        case FPActivationMode.XY_INVERT:
						        if (left - stageObjList[i].activationRange.x <= vector.x && vector.x <= right + stageObjList[i].activationRange.x && bottom - stageObjList[i].activationRange.y <= vector.y && vector.y <= top + stageObjList[i].activationRange.y)
						        {
							        //SetActiveSafe(stageObjList[i].gameObject, value: false);
						        }
						        else
						        {
							        SetActiveSafe(stageObjList[i].gameObject, value: true);
						        }
						        break;
					        }
				        }
				        else
				        {
					        //SetActiveSafe(stageObjList[i].gameObject, value: false);
				        }
			        }
			        else
			        {
				        stageObjPrevActiveState[i] = false;
			        }
		        }
		        for (int i = 0; i < stageObjSpawnEnd; i++)
		        {
			        if (!(stageObjList[i] != null))
			        {
				        continue;
			        }
			        flag = GetActiveInHierarchy(stageObjList[i].gameObject);
			        if (flag)
			        {
				        //AddObjectToTypePoolActiveList(stageObjList[i]);
                        var mAddObjectToTypePoolActiveList = tfpStage.GetMethod("AddObjectToTypePoolActiveList",
                            BindingFlags.Static | BindingFlags.NonPublic);
                        var parms = new[] { stageObjList[i] };
                        mAddObjectToTypePoolActiveList.Invoke(null, BindingFlags.Static | BindingFlags.NonPublic, null, parms, CultureInfo.CurrentCulture);
                    }
			        if (stageObjPrevActiveState[i] != flag)
			        {
				        if (flag)
				        {
					        stageObjList[i].OnActivation();
				        }
				        else
				        {
					        //stageObjList[i].OnDeactivation();
				        }
			        }
		        }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            
	    }
    }
}
