using System;
using System.Threading.Tasks;
using TOHE.Modules;
using TOHE.Roles.AddOns.Common;
using TOHE.Roles.AddOns.Crewmate;
using TOHE.Roles.AddOns.Impostor;
using TOHE.Roles._Ghosts_.Impostor;
using TOHE.Roles._Ghosts_.Crewmate;
using TOHE.Roles.Crewmate;
using TOHE.Roles.Double;
using TOHE.Roles.Impostor;
using TOHE.Roles.Neutral;
using UnityEngine;

namespace TOHE;

[Flags]
public enum CustomGameMode
{
    Standard = 0x01,
    FFA = 0x02,

    HidenSeekTOHE = 0x08, // HidenSeekTOHE must be after other game modes
    All = int.MaxValue
}

[HarmonyPatch]
public static class Options
{
    static Task taskOptionsLoad;
    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.Initialize)), HarmonyPostfix]
    public static void OptionsLoadStart_Postfix()
    {
        Logger.Msg("Mod option loading start", "Load Options");
        taskOptionsLoad = Task.Run(Load);
        //taskOptionsLoad.ContinueWith(t => { Logger.Msg("Mod option loading end", "Load Options"); });
    }
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start)), HarmonyPostfix]
    public static void WaitOptionsLoad_Postfix()
    {
        taskOptionsLoad.Wait();
        Logger.Info("Mod option loading eng", "Load Options");
    }

    // Presets
    private static readonly string[] presets =
    [
        Main.Preset1.Value, Main.Preset2.Value, Main.Preset3.Value,
        Main.Preset4.Value, Main.Preset5.Value
    ];

    // Custom Game Mode
    public static OptionItem GameMode;
    public static CustomGameMode CurrentGameMode
        => GameMode.GetInt() switch
        {
            1 => CustomGameMode.FFA,
            2 => CustomGameMode.HidenSeekTOHE, // HidenSeekTOHE must be after other game modes
            _ => CustomGameMode.Standard
        };

    public static readonly string[] gameModes =
    [
        "Standard",
        "FFA",


        "Hide&SeekTOHE", // HidenSeekTOHE must be after other game modes
    ];

    // 役職数・確率
    public static Dictionary<CustomRoles, int> roleCounts;
    public static Dictionary<CustomRoles, float> roleSpawnChances;
    public static Dictionary<CustomRoles, OptionItem> CustomRoleCounts;
    public static Dictionary<CustomRoles, OptionItem> CustomGhostRoleCounts;
    public static Dictionary<CustomRoles, StringOptionItem> CustomRoleSpawnChances;
    public static Dictionary<CustomRoles, IntegerOptionItem> CustomAdtRoleSpawnRate;
    public enum SpawnChance
    {
        Chance0,
        Chance5,
        Chance10,
        Chance15,
        Chance20,
        Chance25,
        Chance30,
        Chance35,
        Chance40,
        Chance45,
        Chance50,
        Chance55,
        Chance60,
        Chance65,
        Chance70,
        Chance75,
        Chance80,
        Chance85,
        Chance90,
        Chance95,
        Chance100,
    }
    private enum RatesZeroOne
    {
        RoleOff,
        RoleRate,
    }
    public static readonly string[] CheatResponsesName =
    [
        "Ban", "Kick", "NoticeMe","NoticeEveryone", "TempBan", "OnlyCancel"
    ];
    public static readonly string[] ConfirmEjectionsMode =
    [
        "ConfirmEjections.None",
        "ConfirmEjections.Team",
        "ConfirmEjections.Role"
    ];
    public static readonly string[] CamouflageMode =
    [
        "CamouflageMode.Default",
        "CamouflageMode.Host",
        "CamouflageMode.Random",
        "CamouflageMode.OnlyRandomColor",
        "CamouflageMode.Karpe",
        "CamouflageMode.Lauryn",
        "CamouflageMode.Moe",
        "CamouflageMode.Pyro",
        "CamouflageMode.ryuk",
        "CamouflageMode.Gurge44",
        "CamouflageMode.TommyXL"
    ];

    // 各役職の詳細設定
    //public static OptionItem EnableGM;
    public static float DefaultKillCooldown = Main.NormalOptions?.KillCooldown ?? 20;
    public static OptionItem GhostsDoTasks;


    // ------------ System Settings Tab ------------
    public static OptionItem GradientTagsOpt;
    public static OptionItem EnableKillerLeftCommand;
    public static OptionItem SeeEjectedRolesInMeeting;

    public static OptionItem KickLowLevelPlayer;
    public static OptionItem TempBanLowLevelPlayer;

    public static OptionItem ApplyAllowList;
    public static OptionItem AllowOnlyWhiteList;

    public static OptionItem KickOtherPlatformPlayer;
    public static OptionItem OptKickAndroidPlayer;
    public static OptionItem OptKickIphonePlayer;
    public static OptionItem OptKickXboxPlayer;
    public static OptionItem OptKickPlayStationPlayer;
    public static OptionItem OptKickNintendoPlayer;

    public static OptionItem KickPlayerFriendCodeNotExist;
    public static OptionItem TempBanPlayerFriendCodeNotExist;

    public static OptionItem AutoKickStart;
    public static OptionItem AutoKickStartTimes;
    public static OptionItem AutoKickStartAsBan;

    public static OptionItem TempBanPlayersWhoKeepQuitting;
    public static OptionItem QuitTimesTillTempBan;

    public static OptionItem ApplyVipList;
    public static OptionItem ApplyDenyNameList;
    public static OptionItem ApplyBanList;
    public static OptionItem ApplyModeratorList;
    public static OptionItem AllowSayCommand;

    //public static OptionItem ApplyReminderMsg;
    //public static OptionItem TimeForReminder;
    //public static OptionItem AutoKickStopWords;
    //public static OptionItem AutoKickStopWordsTimes;
    //public static OptionItem AutoKickStopWordsAsBan;
    //public static OptionItem AutoWarnStopWords;

    public static OptionItem MinWaitAutoStart;
    public static OptionItem MaxWaitAutoStart;
    public static OptionItem PlayerAutoStart;
    public static OptionItem AutoStartTimer;
    public static OptionItem AutoPlayAgain;
    public static OptionItem AutoPlayAgainCountdown;

    //public static OptionItem ShowLobbyCode;
    public static OptionItem LowLoadMode;
    public static OptionItem EndWhenPlayerBug;
    public static OptionItem HideExileChat;
    public static OptionItem RemovePetsAtDeadPlayers;

    public static OptionItem CheatResponses;
    public static OptionItem NewHideMsg;

    public static OptionItem AutoDisplayKillLog;
    public static OptionItem AutoDisplayLastRoles;
    public static OptionItem AutoDisplayLastResult;
    public static OptionItem OldKillLog;

    public static OptionItem SuffixMode;
    public static OptionItem HideHostText;
    public static OptionItem HideGameSettings;

    public static OptionItem PlayerCanSetColor;
    public static OptionItem PlayerCanSetName;
    public static OptionItem PlayerCanUseQuitCommand;
    public static OptionItem PlayerCanUseTP;
    public static OptionItem CanPlayMiniGames;
    public static OptionItem FormatNameMode;
    public static OptionItem DisableEmojiName;
    //public static OptionItem ColorNameMode;
    public static OptionItem ChangeNameToRoleInfo;
    public static OptionItem SendRoleDescriptionFirstMeeting;

    public static OptionItem NoGameEnd;
    public static OptionItem AllowConsole;
    public static OptionItem DisableAntiBlackoutProtects;

    public static OptionItem RoleAssigningAlgorithm;
    public static OptionItem KPDCamouflageMode;
    public static OptionItem EnableUpMode;
    public static OptionItem DisableVoteBan;

    // ------------ Game Settings Tab ------------

    // Hide & Seek Setting
    public static OptionItem NumImpostorsHnS;

    // Confirm Ejection
    public static OptionItem CEMode;
    public static OptionItem ShowImpRemainOnEject;
    public static OptionItem ShowNKRemainOnEject;
    public static OptionItem ShowTeamNextToRoleNameOnEject;
    public static OptionItem ConfirmEgoistOnEject;
    public static OptionItem ConfirmLoversOnEject;
    //public static OptionItem ConfirmSidekickOnEject;
    //public static OptionItem ExtendedEjections;

    // Maps Settings
    public static OptionItem RandomMapsMode;
    public static OptionItem SkeldChance;
    public static OptionItem MiraChance;
    public static OptionItem PolusChance;
    public static OptionItem DleksChance;
    public static OptionItem AirshipChance;
    public static OptionItem FungleChance;
    public static OptionItem UseMoreRandomMapSelection;

    public static OptionItem MapModification;
    public static OptionItem AirshipVariableElectrical;
    public static OptionItem DisableAirshipMovingPlatform;
    public static OptionItem DisableSporeTriggerOnFungle;
    public static OptionItem DisableZiplineOnFungle;
    public static OptionItem DisableZiplineFromTop;
    public static OptionItem DisableZiplineFromUnder;

    public static OptionItem ResetDoorsEveryTurns;
    public static OptionItem DoorsResetMode;

    public static OptionItem ChangeDecontaminationTime;
    public static OptionItem DecontaminationTimeOnMiraHQ;
    public static OptionItem DecontaminationTimeOnPolus;

    // Sabotage Settings
    public static OptionItem CommsCamouflage;
    public static OptionItem DisableOnSomeMaps;
    public static OptionItem DisableOnSkeld;
    public static OptionItem DisableOnMira;
    public static OptionItem DisableOnPolus;
    public static OptionItem DisableOnDleks;
    public static OptionItem DisableOnAirship;
    public static OptionItem DisableOnFungle;
    public static OptionItem DisableReportWhenCC;

    public static OptionItem SabotageCooldownControl;
    public static OptionItem SabotageCooldown;

    public static OptionItem SabotageTimeControl;
    public static OptionItem SkeldReactorTimeLimit;
    public static OptionItem SkeldO2TimeLimit;
    public static OptionItem MiraReactorTimeLimit;
    public static OptionItem MiraO2TimeLimit;
    public static OptionItem PolusReactorTimeLimit;
    public static OptionItem AirshipReactorTimeLimit;
    public static OptionItem FungleReactorTimeLimit;
    public static OptionItem FungleMushroomMixupDuration;

    public static OptionItem LightsOutSpecialSettings;
    public static OptionItem BlockDisturbancesToSwitches;
    public static OptionItem DisableAirshipViewingDeckLightsPanel;
    public static OptionItem DisableAirshipGapRoomLightsPanel;
    public static OptionItem DisableAirshipCargoLightsPanel;

    // Disable
    public static OptionItem DisableShieldAnimations;
    public static OptionItem DisableKillAnimationOnGuess;
    public static OptionItem DisableVanillaRoles;
    public static OptionItem DisableTaskWin;
    public static OptionItem DisableMeeting;
    public static OptionItem DisableSabotage;
    public static OptionItem DisableCloseDoor;

    public static OptionItem DisableDevices;
    public static OptionItem DisableSkeldDevices;
    public static OptionItem DisableSkeldAdmin;
    public static OptionItem DisableSkeldCamera;
    public static OptionItem DisableMiraHQDevices;
    public static OptionItem DisableMiraHQAdmin;
    public static OptionItem DisableMiraHQDoorLog;
    public static OptionItem DisablePolusDevices;
    public static OptionItem DisablePolusAdmin;
    public static OptionItem DisablePolusCamera;
    public static OptionItem DisablePolusVital;
    public static OptionItem DisableAirshipDevices;
    public static OptionItem DisableAirshipCockpitAdmin;
    public static OptionItem DisableAirshipRecordsAdmin;
    public static OptionItem DisableAirshipCamera;
    public static OptionItem DisableAirshipVital;
    public static OptionItem DisableFungleDevices;
    public static OptionItem DisableFungleBinoculars;
    public static OptionItem DisableFungleVital;
    public static OptionItem DisableDevicesIgnoreConditions;
    public static OptionItem DisableDevicesIgnoreImpostors;
    public static OptionItem DisableDevicesIgnoreNeutrals;
    public static OptionItem DisableDevicesIgnoreCrewmates;
    public static OptionItem DisableDevicesIgnoreAfterAnyoneDied;

    // Meeting Settings
    public static OptionItem SyncButtonMode;
    public static OptionItem SyncedButtonCount;
    public static int UsedButtonCount = 0;

    public static OptionItem AllAliveMeeting;
    public static OptionItem AllAliveMeetingTime;

    public static OptionItem AdditionalEmergencyCooldown;
    public static OptionItem AdditionalEmergencyCooldownThreshold;
    public static OptionItem AdditionalEmergencyCooldownTime;

    public static OptionItem VoteMode;
    public static OptionItem WhenSkipVote;
    public static OptionItem WhenSkipVoteIgnoreFirstMeeting;
    public static OptionItem WhenSkipVoteIgnoreNoDeadBody;
    public static OptionItem WhenSkipVoteIgnoreEmergency;
    public static OptionItem WhenNonVote;
    public static OptionItem WhenTie;

    // Other
    public static OptionItem LadderDeath;
    public static OptionItem LadderDeathChance;

    public static OptionItem FixFirstKillCooldown;
    public static OptionItem FixKillCooldownValue;
    public static OptionItem ShieldPersonDiedFirst;

    public static OptionItem KillFlashDuration;

    // Ghost
    public static OptionItem GhostIgnoreTasks;
    public static OptionItem GhostCanSeeOtherRoles;
    public static OptionItem GhostCanSeeOtherVotes;
    public static OptionItem GhostCanSeeDeathReason;
    public static OptionItem ConvertedCanBecomeGhost;
    public static OptionItem MaxImpGhost;
    public static OptionItem MaxCrewGhost;
    public static OptionItem DefaultAngelCooldown;


    // ------------ Task Management Tab ------------

    // Disable Tasks
    public static OptionItem DisableShortTasks;
    public static OptionItem DisableCleanVent;
    public static OptionItem DisableCalibrateDistributor;
    public static OptionItem DisableChartCourse;
    public static OptionItem DisableStabilizeSteering;
    public static OptionItem DisableCleanO2Filter;
    public static OptionItem DisableUnlockManifolds;
    public static OptionItem DisablePrimeShields;
    public static OptionItem DisableMeasureWeather;
    public static OptionItem DisableBuyBeverage;
    public static OptionItem DisableAssembleArtifact;
    public static OptionItem DisableSortSamples;
    public static OptionItem DisableProcessData;
    public static OptionItem DisableRunDiagnostics;
    public static OptionItem DisableRepairDrill;
    public static OptionItem DisableAlignTelescope;
    public static OptionItem DisableRecordTemperature;
    public static OptionItem DisableFillCanisters;
    public static OptionItem DisableMonitorTree;
    public static OptionItem DisableStoreArtifacts;
    public static OptionItem DisablePutAwayPistols;
    public static OptionItem DisablePutAwayRifles;
    public static OptionItem DisableMakeBurger;
    public static OptionItem DisableCleanToilet;
    public static OptionItem DisableDecontaminate;
    public static OptionItem DisableSortRecords;
    public static OptionItem DisableFixShower;
    public static OptionItem DisablePickUpTowels;
    public static OptionItem DisablePolishRuby;
    public static OptionItem DisableDressMannequin;

    public static OptionItem DisableCommonTasks;
    public static OptionItem DisableSwipeCard;
    public static OptionItem DisableFixWiring;
    public static OptionItem DisableEnterIdCode;
    public static OptionItem DisableInsertKeys;
    public static OptionItem DisableScanBoardingPass;

    public static OptionItem DisableLongTasks;
    public static OptionItem DisableSubmitScan;
    public static OptionItem DisableUnlockSafe;
    public static OptionItem DisableStartReactor;
    public static OptionItem DisableResetBreaker;
    public static OptionItem DisableAlignEngineOutput;
    public static OptionItem DisableInspectSample;
    public static OptionItem DisableEmptyChute;
    public static OptionItem DisableClearAsteroids;
    public static OptionItem DisableWaterPlants;
    public static OptionItem DisableOpenWaterways;
    public static OptionItem DisableReplaceWaterJug;
    public static OptionItem DisableRebootWifi;
    public static OptionItem DisableDevelopPhotos;
    public static OptionItem DisableRewindTapes;
    public static OptionItem DisableStartFans;

    public static OptionItem DisableOtherTasks;
    public static OptionItem DisableUploadData;
    public static OptionItem DisableEmptyGarbage;
    public static OptionItem DisableFuelEngines;
    public static OptionItem DisableDivertPower;
    public static OptionItem DisableActivateWeatherNodes;
    public static OptionItem DisableRoastMarshmallow;
    public static OptionItem DisableCollectSamples;
    public static OptionItem DisableReplaceParts;
    public static OptionItem DisableCollectVegetables;
    public static OptionItem DisableMineOres;
    public static OptionItem DisableExtractFuel;
    public static OptionItem DisableCatchFish;
    public static OptionItem DisablePolishGem;
    public static OptionItem DisableHelpCritter;
    public static OptionItem DisableHoistSupplies;
    public static OptionItem DisableFixAntenna;
    public static OptionItem DisableBuildSandcastle;
    public static OptionItem DisableCrankGenerator;
    public static OptionItem DisableMonitorMushroom;
    public static OptionItem DisablePlayVideoGame;
    public static OptionItem DisableFindSignal;
    public static OptionItem DisableThrowFisbee;
    public static OptionItem DisableLiftWeights;
    public static OptionItem DisableCollectShells;

    // Guesser Mode
    public static OptionItem GuesserMode;
    public static OptionItem CrewmatesCanGuess;
    public static OptionItem ImpostorsCanGuess;
    public static OptionItem NeutralKillersCanGuess;
    public static OptionItem PassiveNeutralsCanGuess;
    public static OptionItem CanGuessAddons;
    public static OptionItem ImpCanGuessImp;
    public static OptionItem CrewCanGuessCrew;
    public static OptionItem HideGuesserCommands;
    public static OptionItem ShowOnlyEnabledRolesInGuesserUI;


    // ------------ General Role Settings ------------

    // Imp
    public static OptionItem ImpsCanSeeEachOthersRoles;
    public static OptionItem ImpsCanSeeEachOthersAddOns;

    //public static OptionItem MadmateCanFixSabotage;
    public static OptionItem DefaultShapeshiftCooldown;
    public static OptionItem DeadImpCantSabotage;

    // Neutral
    public static OptionItem NonNeutralKillingRolesMinPlayer;
    public static OptionItem NonNeutralKillingRolesMaxPlayer;
    public static OptionItem NeutralKillingRolesMinPlayer;
    public static OptionItem NeutralKillingRolesMaxPlayer;
    public static OptionItem NeutralRoleWinTogether;
    public static OptionItem NeutralWinTogether;

    // Add-on
    public static OptionItem NameDisplayAddons;
    public static OptionItem AddBracketsToAddons;
    public static OptionItem NoLimitAddonsNumMax;

    // Impostors role settings
    public static OptionItem ShapeshiftCD;
    public static OptionItem ShapeshiftDur;

    // Crewmates role settings
    public static OptionItem ScientistCD;
    public static OptionItem ScientistDur;

    // Add-Ons settings 
    public static OptionItem LoverSpawnChances;
    public static OptionItem LoverKnowRoles;
    public static OptionItem LoverSuicide;
    public static OptionItem ImpCanBeInLove;
    public static OptionItem CrewCanBeInLove;
    public static OptionItem NeutralCanBeInLove;

    // Experimental Roles

    //public static OptionItem SpeedBoosterUpSpeed;
    //public static OptionItem SpeedBoosterTimes;


    public static VoteMode GetWhenSkipVote() => (VoteMode)WhenSkipVote.GetValue();
    public static VoteMode GetWhenNonVote() => (VoteMode)WhenNonVote.GetValue();

    public static readonly string[] voteModes =
    [
        "Default", "Suicide", "SelfVote", "Skip"
    ];
    public static readonly string[] tieModes =
    [
        "TieMode.Default", "TieMode.All", "TieMode.Random"
    ];
    /* public static readonly string[] addonGuessModeCrew =
     {
         "GuesserMode.All", "GuesserMode.Harmful", "GuesserMode.Random"
     }; */
    
    public static readonly string[] suffixModes =
    [
        "SuffixMode.None",
        "SuffixMode.Version",
        "SuffixMode.Streaming",
        "SuffixMode.Recording",
        "SuffixMode.RoomHost",
        "SuffixMode.OriginalName",
        "SuffixMode.DoNotKillMe",
        "SuffixMode.NoAndroidPlz",
        "SuffixMode.AutoHost"    
    ];
    public static readonly string[] roleAssigningAlgorithms =
    [
        "RoleAssigningAlgorithm.Default",
        "RoleAssigningAlgorithm.NetRandom",
        "RoleAssigningAlgorithm.HashRandom",
        "RoleAssigningAlgorithm.Xorshift",
        "RoleAssigningAlgorithm.MersenneTwister",
    ];
    public static readonly string[] formatNameModes =
    [
        "FormatNameModes.None",
        "FormatNameModes.Color",
        "FormatNameModes.Snacks",
    ];
    public static SuffixModes GetSuffixMode() => (SuffixModes)SuffixMode.GetValue();


    public static int SnitchExposeTaskLeft = 1;

    public static bool IsLoaded = false;

    static Options()
    {
        ResetRoleCounts();
    }
    public static void ResetRoleCounts()
    {
        roleCounts = [];
        roleSpawnChances = [];

        foreach (var role in CustomRolesHelper.AllRoles)
        {
            roleCounts.Add(role, 0);
            roleSpawnChances.Add(role, 0);
        }
    }

    public static void SetRoleCount(CustomRoles role, int count)
    {
        roleCounts[role] = count;

        if (CustomRoleCounts.TryGetValue(role, out var option))
        {
            option.SetValue(count - 1);
        }
    }

    public static int GetRoleSpawnMode(CustomRoles role) => CustomRoleSpawnChances.TryGetValue(role, out var sc) ? sc.GetChance() : 0;
    public static int GetRoleCount(CustomRoles role)
    {
        var mode = GetRoleSpawnMode(role);
        return mode is 0 ? 0 : CustomRoleCounts.TryGetValue(role, out var option) ? option.GetInt() : roleCounts[role];
    }
    public static float GetRoleChance(CustomRoles role)
    {
        return CustomRoleSpawnChances.TryGetValue(role, out var option) ? option.GetValue()/* / 10f */ : roleSpawnChances[role];
    }
    public static void Load()
    {
        //#######################################
        // 28100 lasted id for roles/add-ons (Next use 28200)
        // Limit id for roles/add-ons --- "59999"
        //#######################################

        // 22004 (Glow)


        // Start Load Settings
        if (IsLoaded) return;
        OptionSaver.Initialize();

        // Preset Option
        _ = PresetOptionItem.Create(0, TabGroup.SystemSettings)
                .SetColor(new Color32(255, 235, 4, byte.MaxValue))
                .SetHeader(true);

        // Game Mode
        GameMode = StringOptionItem.Create(60000, "GameMode", gameModes, 0, TabGroup.GameSettings, false)
            .SetHeader(true);


        #region Roles/Add-ons Settings
        CustomRoleCounts = [];
        CustomGhostRoleCounts = [];
        CustomRoleSpawnChances = [];
        CustomAdtRoleSpawnRate = [];

        // GM
        //EnableGM = BooleanOptionItem.Create(60001, "GM", false, TabGroup.GameSettings, false)
        //    .SetColor(Utils.GetRoleColor(CustomRoles.GM))
        //    .SetHidden(true)
        //    .SetHeader(true);

        ImpsCanSeeEachOthersRoles = BooleanOptionItem.Create(60001, "ImpsCanSeeEachOthersRoles", true, TabGroup.ImpostorRoles, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetHeader(true);
        ImpsCanSeeEachOthersAddOns = BooleanOptionItem.Create(60002, "ImpsCanSeeEachOthersAddOns", true, TabGroup.ImpostorRoles, false)
            .SetParent(ImpsCanSeeEachOthersRoles);


        Madmate.SetupMenuOptions();

        Refugee.SetupCustomOption();

        //MadmateCanFixSabotage = BooleanOptionItem.Create(50010, "MadmateCanFixSabotage", false, TabGroup.ImpostorRoles, false)
        //.SetGameMode(CustomGameMode.Standard);

        DefaultShapeshiftCooldown = FloatOptionItem.Create(60011, "DefaultShapeshiftCooldown", new(5f, 180f, 5f), 15f, TabGroup.ImpostorRoles, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetHeader(true)
            .SetValueFormat(OptionFormat.Seconds);
        DeadImpCantSabotage = BooleanOptionItem.Create(60012, "DeadImpCantSabotage", false, TabGroup.ImpostorRoles, false)
            .SetGameMode(CustomGameMode.Standard);

        NonNeutralKillingRolesMinPlayer = IntegerOptionItem.Create(60013, "NonNeutralKillingRolesMinPlayer", new(0, 15, 1), 0, TabGroup.NeutralRoles, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetHeader(true)
            .SetValueFormat(OptionFormat.Players);
        NonNeutralKillingRolesMaxPlayer = IntegerOptionItem.Create(60014, "NonNeutralKillingRolesMaxPlayer", new(0, 15, 1), 0, TabGroup.NeutralRoles, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetValueFormat(OptionFormat.Players);

        NeutralKillingRolesMinPlayer = IntegerOptionItem.Create(60015, "NeutralKillingRolesMinPlayer", new(0, 15, 1), 0, TabGroup.NeutralRoles, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetHeader(true)
            .SetValueFormat(OptionFormat.Players);
        NeutralKillingRolesMaxPlayer = IntegerOptionItem.Create(60016, "NeutralKillingRolesMaxPlayer", new(0, 15, 1), 0, TabGroup.NeutralRoles, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetValueFormat(OptionFormat.Players);


        NeutralRoleWinTogether = BooleanOptionItem.Create(60017, "NeutralRoleWinTogether", false, TabGroup.NeutralRoles, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetHeader(true);
        NeutralWinTogether = BooleanOptionItem.Create(60018, "NeutralWinTogether", false, TabGroup.NeutralRoles, false)
            .SetParent(NeutralRoleWinTogether)
            .SetGameMode(CustomGameMode.Standard);

        NameDisplayAddons = BooleanOptionItem.Create(60019, "NameDisplayAddons", true, TabGroup.Addons, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetHeader(true);
        AddBracketsToAddons = BooleanOptionItem.Create(60021, "BracketAddons", true, TabGroup.Addons, false)
            .SetParent(NameDisplayAddons);
        NoLimitAddonsNumMax = IntegerOptionItem.Create(60020, "NoLimitAddonsNumMax", new(1, 15, 1), 1, TabGroup.Addons, false)
            .SetGameMode(CustomGameMode.Standard);
        #endregion

        #region Impostors Settings
        // Impostor
        TextOptionItem.Create(10000000, "RoleType.VanillaRoles", TabGroup.ImpostorRoles) // Vanilla
            .SetGameMode(CustomGameMode.Standard)
            .SetHeader(true)
            .SetColor(new Color32(255, 25, 25, byte.MaxValue));

        SetupRoleOptions(300, TabGroup.ImpostorRoles, CustomRoles.ImpostorTOHE);

        SetupRoleOptions(400, TabGroup.ImpostorRoles, CustomRoles.ShapeshifterTOHE);
        ShapeshiftCD = FloatOptionItem.Create(402, "ShapeshiftCooldown", new(1f, 180f, 1f), 15f, TabGroup.ImpostorRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.ShapeshifterTOHE])
            .SetValueFormat(OptionFormat.Seconds);
        ShapeshiftDur = FloatOptionItem.Create(403, "ShapeshiftDuration", new(1f, 180f, 1f), 30f, TabGroup.ImpostorRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.ShapeshifterTOHE])
            .SetValueFormat(OptionFormat.Seconds);


        TextOptionItem.Create(10000001, "RoleType.ImpKilling", TabGroup.ImpostorRoles) // KILLING
            .SetGameMode(CustomGameMode.Standard)
            .SetHeader(true)
            .SetColor(new Color32(255, 25, 25, byte.MaxValue));// KILLING

        /*
         * Arrogance
         */
        Arrogance.SetupCustomOption();

        /*
         * Berserker
         */
        Berserker.SetupCustomOption();

        /*
         * Bomber
         */
        Bomber.SetupCustomOption();

        /*
         * Bounty Hunter
         */
        BountyHunter.SetupCustomOption();

        /*
         * Butcher
         */
        Butcher.SetupCustomOption();

        /*
         * Chronomancer
         */
        Chronomancer.SetupCustomOption();

        /*
         * Councillor
         */
        Councillor.SetupCustomOption();

        /*
         * Cursed Wolf (From: TOH_Y)
         */
        CursedWolf.SetupCustomOption();


        /*
         * Deathpact
         */
        Deathpact.SetupCustomOption();

        /*
         * Evil Guesser
         */
        EvilGuesser.SetupCustomOption();

        /*
         * Evil Tracker
         */
        EvilTracker.SetupCustomOption();

        /*
         * Greedy
         */
        Greedy.SetupCustomOption();

        /*
         * Hangman
         */
        Hangman.SetupCustomOption();

        /*
         * Inhibitor
         */
        Inhibitor.SetupCustomOption();

        /*
         * Instigator
         */
        Instigator.SetupCustomOption();

        /*
         * Killing Machine
         */
        KillingMachine.SetupCustomOption();

        /*
         * Ludopath
         */
        Ludopath.SetupCustomOption();

        /*
         * Lurker
         */
        Lurker.SetupCustomOption();

        // Mare.SetupCustomOption();

        /*
         * Mercenary
         */
        Mercenary.SetupCustomOption();

        /*
         * Ninja
         */
        Ninja.SetupCustomOption();

        /*
         * Quick Shooter
         */
        QuickShooter.SetupCustomOption();

        /*
         * Saboteur
         */
        Saboteur.SetupCustomOption();

        /*
         * Sniper
         */
        Sniper.SetupCustomOption();

        /*
         * Spellcaster
         */
        Witch.SetupCustomOption();

        /*
         * Trapster
         */
        Trapster.SetupCustomOption();

        /*
         * Underdog
         */
        Underdog.SetupCustomOption();

        /*
         * Zombie
         */
        Zombie.SetupCustomOption();

        /*
         * SUPPORT ROLES
         */
        TextOptionItem.Create(10000002, "RoleType.ImpSupport", TabGroup.ImpostorRoles)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(255, 25, 25, byte.MaxValue));

        /*
         * Anti Adminer
         */
        AntiAdminer.SetupCustomOption();

        /*
         * Blackmailer
        */
        Blackmailer.SetupCustomOption();

        /*
         * Camouflager
         */
        Camouflager.SetupCustomOption();

        /*
         * Cleaner
         */
        Cleaner.SetupCustomOption();

        /* 
         * Consigliere
         */
        Consigliere.SetupCustomOption();

        /*
         * Fireworker
         */
        Fireworker.SetupCustomOption();

        /*
         * Gangster
         */
        Gangster.SetupCustomOption();

        /*
         * Godfather
         */
        Godfather.SetupCustomOption();

        /*
         * Kamikaze
         */
        Kamikaze.SetupCustomOption();

        /*
         * Morphling
         */
        Morphling.SetupCustomOption();

        /*
         * Nemesis
         */
        Nemesis.SetupCustomOptions();
        /*
         * Time Thief
         */
        TimeThief.SetupCustomOption();

        /*
         * Vindicator
         */
        Vindicator.SetupCustomOption();

        /*
         * Visionary
         */
        Visionary.SetupCustomOption();

        /*
         * CONCEALING ROLES
         */
        TextOptionItem.Create(10000003, "RoleType.ImpConcealing", TabGroup.ImpostorRoles)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(255, 25, 25, byte.MaxValue));

        /*
         * Escapist
         */
        Escapist.SetupCustomOption();

        /*
         * Lightning
         */
        Lightning.SetupCustomOption();

        /*
         * Mastermind
         */
        Mastermind.SetupCustomOption();

        /*
         * Miner
         */
        Miner.SetupCustomOption();

        /*
         * Puppeteer
         */
        Puppeteer.SetupCustomOption();

        /*
         * Rift Maker
         */
        RiftMaker.SetupCustomOption();

        /*
         * Scavenger
         */
        Scavenger.SetupCustomOption();

        /*
         * ShapeMaster
         */
        ShapeMaster.SetupCustomOption();

        /*
         * Soul Catcher
         */
        SoulCatcher.SetupCustomOption();

        /*
         * Swooper
         */
        Swooper.SetupCustomOption();

        /*
         * Trickster
         */
        Trickster.SetupCustomOption();

        /*
         * Undertaker
         */
        Undertaker.SetupCustomOption();

        /*
         * Vampire
         */
        Vampire.SetupCustomOption();

        /*
         * Warlock
         */
        Warlock.SetupCustomOption();

        /*
         * Wildling
         */
        Wildling.SetupCustomOption();

        /*
         * HINDERING ROLES
         */
        TextOptionItem.Create(10000004, "RoleType.ImpHindering", TabGroup.ImpostorRoles)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(255, 25, 25, byte.MaxValue));

        /*
         * Anonymous
         */
        Anonymous.SetupCustomOption();

        /*
         * Dazzler
         */
        Dazzler.SetupCustomOption();

        /*
         * Stealth
         */
        Stealth.SetupCustomOption();

        /*
         * Devourer
         */
        Devourer.SetupCustomOption();

        /*
         * Eraser
         */
        Eraser.SetupCustomOption();

        /*
         * Pitfall
         */
        Pitfall.SetupCustomOption();

        /*
         * Penguin
         */
        Penguin.SetupCustomOption();

        /*
         * Twister
         */
        Twister.SetupCustomOption();

        /*
         * MADMATE ROLES
         */
        TextOptionItem.Create(10000005, "RoleType.Madmate", TabGroup.ImpostorRoles)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(255, 25, 25, byte.MaxValue));

        /*
         * Crewpostor
         */
        Crewpostor.SetupCustomOption();

        /*
         * Parasite
         */
        Parasite.SetupCustomOption();

        /*
         * Impostor Ghost Roles
        */
        TextOptionItem.Create(10000111, "RoleType.ImpGhost", TabGroup.ImpostorRoles)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(255, 25, 25, byte.MaxValue));

        Minion.SetupCustomOption();

        Bloodmoon.SetupCustomOption();

        #endregion

        #region Crewmates Settings
        /*
         * VANILLA ROLES
         */
        TextOptionItem.Create(10000006, "RoleType.VanillaRoles", TabGroup.CrewmateRoles)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(140, 255, 255, byte.MaxValue));
        
        /*
         * Crewmate
         */
        SetupRoleOptions(6000, TabGroup.CrewmateRoles, CustomRoles.CrewmateTOHE);

        /*
         * Engineer
         */
        SetupRoleOptions(6100, TabGroup.CrewmateRoles, CustomRoles.EngineerTOHE);

        /*
         * Scientist
         */
        SetupRoleOptions(6200, TabGroup.CrewmateRoles, CustomRoles.ScientistTOHE);
        ScientistCD = FloatOptionItem.Create(6202, "VitalsCooldown", new(1f, 250f, 1f), 3f, TabGroup.CrewmateRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.ScientistTOHE])
            .SetValueFormat(OptionFormat.Seconds);
        ScientistDur = FloatOptionItem.Create(6203, "VitalsDuration", new(1f, 250f, 1f), 15f, TabGroup.CrewmateRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.ScientistTOHE])
            .SetValueFormat(OptionFormat.Seconds);

        /*
         * Guardian Angel
         */
        GuardianAngelTOHE.SetupCustomOptions();

        /*
         * BASIC ROLES
         */
        TextOptionItem.Create(10000007, "RoleType.CrewBasic", TabGroup.CrewmateRoles)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(140, 255, 255, byte.MaxValue));

        /*
         * Addict
         */
        Addict.SetupCustomOption();

        /*
         * Alchemist
         */
        Alchemist.SetupCustomOption();

        /*
         * Celebrity
         */
        Celebrity.SetupCustomOptions();    

        /*
         * Cleanser
         */
        Cleanser.SetupCustomOption();

        /*
         * Doctor
         */
        Doctor.SetupCustomOptions();

        /*
         * Guess Master
         */
        GuessMaster.SetupCustomOption();

        /*
         * Lazy Guy
         */
        LazyGuy.SetupCustomOptions();

        /*
         * Luckey
         */
        //SetupRoleOptions(6900, TabGroup.CrewmateRoles, CustomRoles.Luckey);
        //LuckeyProbability = IntegerOptionItem.Create(6902, "LuckeyProbability", new(0, 100, 5), 50, TabGroup.CrewmateRoles, false)
        //    .SetParent(CustomRoleSpawnChances[CustomRoles.Luckey])
        //    .SetValueFormat(OptionFormat.Percent);

        /*
         * Mini
         */
        Mini.SetupCustomOption();

        /*
         * Mole
         */
        Mole.SetupCustomOption();

        /*
         * Superstar
         */
        SuperStar.SetupCustomOptions();

        /*
         * Task Manager
         */
        TaskManager.SetupCustomOptions();

        /*
         * Tracefinder
         */
        Tracefinder.SetupCustomOption();

        /*
         * Transporter
         */
        Transporter.SetupCustomOptions();

        /*
         * Randomizer
         */
        Randomizer.SetupCustomOptions();
        
        /*
         * SUPPORT ROLES
         */
        TextOptionItem.Create(10000008, "RoleType.CrewSupport", TabGroup.CrewmateRoles)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(140, 255, 255, byte.MaxValue));

        /*
         * Benefactor 
         */
        Benefactor.SetupCustomOption();

        /*
         * Chameleon
         */
        Chameleon.SetupCustomOption();

        /*
         * Coroner
         */
        Coroner.SetupCustomOption();

        /*
         * Deputy
         */
        Deputy.SetupCustomOption();

        /*
         * Detective
         */
        Detective.SetupCustomOptions();

        /*
         * Fortune Teller
         */
        FortuneTeller.SetupCustomOption();

        /*
         * Enigma
         */
        Enigma.SetupCustomOption();

        /*
         * Grenadier
         */
        Grenadier.SetupCustomOptions();

        /*
         * Inspector
         */
        Inspector.SetupCustomOption();

        /*
         * Investigator
         */
        Investigator.SetupCustomOption();

        /*
         *  Keeper
         */
        Keeper.SetupCustomOption();

        /*
         * Lighter
         */
        Lighter.SetupCustomOptions();

        /*
         * Mechanic
         */
        Mechanic.SetupCustomOption();

        /*
         * Medic
         */
        Medic.SetupCustomOption();

        /*
         * Medium
         */
        Medium.SetupCustomOption();

        /*
         * Merchant
         */
        Merchant.SetupCustomOption();

        /*
         * Mortician
         */
        Mortician.SetupCustomOption();

        /*
         * Observer
         */
        Observer.SetupCustomOption();

        /*
         * Oracle
         */
        Oracle.SetupCustomOption();

        /*
         * Pacifist
         */
        Pacifist.SetupCustomOptions();

        /*SetupRoleOptions(9300, TabGroup.CrewmateRoles, CustomRoles.Paranoia);
        ParanoiaNumOfUseButton = IntegerOptionItem.Create(9302, "ParanoiaNumOfUseButton", new(1, 20, 1), 3, TabGroup.CrewmateRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Paranoia])
            .SetValueFormat(OptionFormat.Times);
        ParanoiaVentCooldown = FloatOptionItem.Create(9303, "ParanoiaVentCooldown", new(0, 180, 1), 10, TabGroup.CrewmateRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Paranoia])
            .SetValueFormat(OptionFormat.Seconds); */

        /*
         * Psychic
         */
        Psychic.SetupCustomOption();

        /*
         * Snitch
         */
        Snitch.SetupCustomOption();

        /*
         * Spiritualist
         */
        Spiritualist.SetupCustomOption();

        /*
         * Spy
         */
        Spy.SetupCustomOption();

        /*
         * Time Manager
         */
        TimeManager.SetupCustomOption();

        /*
         * Time Master
         */
        TimeMaster.SetupCustomOptions();

        /*
         * Tracker
         */
        Tracker.SetupCustomOption();

        /*
         * Witness
         */
        Witness.SetupCustomOptions();


        TextOptionItem.Create(10000009, "RoleType.CrewKilling", TabGroup.CrewmateRoles)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(140, 255, 255, byte.MaxValue));

        Bastion.SetupCustomOptions();

        Bodyguard.SetupCustomOptions();
        
        Crusader.SetupCustomOption();
        
        Deceiver.SetupCustomOption();
        
        Jailer.SetupCustomOption();
        
        Judge.SetupCustomOption();
        
        Knight.SetupCustomOption();

        Retributionist.SetupCustomOptions();
        
        Reverie.SetupCustomOption();
        
        Sheriff.SetupCustomOption();

        Veteran.SetupCustomOptions();

        TextOptionItem.Create(10000010, "RoleType.CrewPower", TabGroup.CrewmateRoles)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(140, 255, 255, byte.MaxValue));

        Admirer.SetupCustomOption();

        Captain.SetupCustomOption();

        CopyCat.SetupCustomOption();

        Dictator.SetupCustomOptions();

        Guardian.SetupCustomOptions();

        Lookout.SetupCustomOptions();

        Marshall.SetupCustomOption();

        Mayor.SetupCustomOptions();

        Monarch.SetupCustomOption();
        
        Overseer.SetupCustomOption();
        
        President.SetupCustomOption();
        
        Swapper.SetupCustomOption();
        
        Telecommunication.SetupCustomOption();

        //ChiefOfPolice.SetupCustomOption();


        /*
         * Crewmate Ghost Roles
        */
        TextOptionItem.Create(10000101, "RoleType.CrewGhost", TabGroup.CrewmateRoles)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(140, 255, 255, byte.MaxValue));

        Warden.SetupCustomOptions();

        Hawk.SetupCustomOptions();

        #endregion

        #region Neutrals Settings
        // Neutral
        TextOptionItem.Create(10000011, "RoleType.NeutralBenign", TabGroup.NeutralRoles)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(127, 140, 141, byte.MaxValue));

        Amnesiac.SetupCustomOption();

        Follower.SetupCustomOption();

        Hater.SetupCustomOption();

        Imitator.SetupCustomOption();

        Lawyer.SetupCustomOption();

        Maverick.SetupCustomOption();

        Opportunist.SetupCustomOptions();
        
        Pixie.SetupCustomOption();
        
        Pursuer.SetupCustomOption();

        Romantic.SetupCustomOption();

        SchrodingersCat.SetupCustomOption();

        Shaman.SetupCustomOptions();

        Taskinator.SetupCustomOption();


        TextOptionItem.Create(10000012, "RoleType.NeutralEvil", TabGroup.NeutralRoles)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(127, 140, 141, byte.MaxValue));

        CursedSoul.SetupCustomOption();

        Doomsayer.SetupCustomOption();

        Executioner.SetupCustomOption();

        Innocent.SetupCustomOptions();

        Jester.SetupCustomOptions();

        Seeker.SetupCustomOption();

        Masochist.SetupCustomOptions();


        TextOptionItem.Create(10000013, "RoleType.NeutralChaos", TabGroup.NeutralRoles)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(127, 140, 141, byte.MaxValue));
        
        Collector.SetupCustomOption();
        
        Cultist.SetupCustomOption();

        Phantom.SetupCustomOptions();
        
        Pirate.SetupCustomOption();

        Provocateur.SetupCustomOptions();

        Revolutionist.SetupCustomOptions();
        

        /*
         * Solsticer
        */
        Solsticer.SetupCustomOption();

        SoulCollector.SetupCustomOption();

        Terrorist.SetupCustomOptions();

        Vector.SetupCustomOptions();
        
        Vulture.SetupCustomOption();

        Workaholic.SetupCustomOptions();

        TextOptionItem.Create(10000014, "RoleType.NeutralKilling", TabGroup.NeutralRoles)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(127, 140, 141, byte.MaxValue));
        
        Agitater.SetupCustomOption();

        Arsonist.SetupCustomOptions();

        Bandit.SetupCustomOption();

        BloodKnight.SetupCustomOption();

        Demon.SetupCustomOption();

        Glitch.SetupCustomOption();

        HexMaster.SetupCustomOption();

        Huntsman.SetupCustomOption();

        Infectious.SetupCustomOption();

        Jackal.SetupCustomOption();

        Jinx.SetupCustomOption();

        Juggernaut.SetupCustomOption();

        Medusa.SetupCustomOption();

        Necromancer.SetupCustomOption();

        Spiritcaller.SetupCustomOption();

        Pelican.SetupCustomOption();

        Pickpocket.SetupCustomOption();

        Poisoner.SetupCustomOption();

        PlagueBearer.SetupCustomOption();

        PlagueDoctor.SetupCustomOption();

        PotionMaster.SetupCustomOption();

        Pyromaniac.SetupCustomOption();

        if (!Quizmaster.InExperimental)
            Quizmaster.SetupCustomOption();

        SerialKiller.SetupCustomOption(); 

        Shroud.SetupCustomOption();

        Stalker.SetupCustomOption(); // Stalker (TOHY)

        Traitor.SetupCustomOption();

        Virus.SetupCustomOption();

        Werewolf.SetupCustomOption();

        Wraith.SetupCustomOption();

        #endregion

        #region Add-Ons Settings
        // Add-Ons 
        TextOptionItem.Create(10000015, "RoleType.Helpful", TabGroup.Addons) // HELPFUL
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(255, 154, 206, byte.MaxValue));


        Autopsy.SetupCustomOptions();

        Bait.SetupCustomOptions();
        
        /*
         * Beartrap
         */
        Trapper.SetupCustomOptions();

        Bewilder.SetupCustomOptions();

        Burst.SetupCustomOptions();

        Cyber.SetupCustomOptions();

        DoubleShot.SetupCustomOption();

        Flash.SetupCustomOption();

        Lazy.SetupCustomOptions();

        Loyal.SetupCustomOptions();

        Lucky.SetupCustomOptions();

        Necroview.SetupCustomOptions();

        Nimble.SetupCustomOptions();

        Overclocked.SetupCustomOptions();

        Seer.SetupCustomOptions();

        Silent.SetupCustomOptions();

        Sleuth.SetupCustomOptions();

        Tiebreaker.SetupCustomOptions();
    
        Torch.SetupCustomOptions();

        Watcher.SetupCustomOptions();

        TextOptionItem.Create(10000016, "RoleType.Harmful", TabGroup.Addons) // HARMFUL
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(255, 154, 206, byte.MaxValue));

        Unreportable.SetupCustomOptions();

        Fool.SetupCustomOptions();

        Fragile.SetupCustomOptions();

        Statue.SetupCustomOptions();

        Hurried.SetupCustomOption();

        Influenced.SetupCustomOption();

        Mundane.SetupCustomOption();

        Oblivious.SetupCustomOptions();

        Rascal.SetupCustomOptions();

        Unlucky.SetupCustomOptions();
        
        Tired.SetupCustomOptions();

        VoidBallot.SetupCustomOptions();

        TextOptionItem.Create(10000017, "RoleType.Mixed", TabGroup.Addons) // MIXED
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(255, 154, 206, byte.MaxValue));

        Antidote.SetupCustomOptions();

        Avanger.SetupCustomOptions();

        Aware.SetupCustomOptions();

        Bloodlust.SetupCustomOptions();

        Diseased.SetupCustomOptions();

        Ghoul.SetupCustomOptions();

        //BYE GLOW, SEE YOU NEVER - LOOOOOL
        // SetupAdtRoleOptions(22000, CustomRoles.Glow, canSetNum: true);
        //ImpCanBeGlow = BooleanOptionItem.Create(22003, "ImpCanBeGlow", true, TabGroup.Addons, false)
        //.SetParent(CustomRoleSpawnChances[CustomRoles.Glow]);
        //CrewCanBeGlow = BooleanOptionItem.Create(22004, "CrewCanBeGlow", true, TabGroup.Addons, false)
        //.SetParent(CustomRoleSpawnChances[CustomRoles.Glow]);
        //NeutralCanBeGlow = BooleanOptionItem.Create(22005, "NeutralCanBeGlow", true, TabGroup.Addons, false)
        //.SetParent(CustomRoleSpawnChances[CustomRoles.Glow]);

        Gravestone.SetupCustomOptions();

        Guesser.SetupCustomOptions();

        Oiiai.SetupCustomOptions();

        Rebound.SetupCustomOptions();

        Schizophrenic.SetupCustomOptions();

        Stubborn.SetupCustomOptions();

        Susceptible.SetupCustomOptions();

        TextOptionItem.Create(10000018, "RoleType.Impostor", TabGroup.Addons) // IMPOSTOR
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(255, 25, 25, byte.MaxValue));

        /*
         * Circumvent
         */
        Circumvent.SetupCustomOption();

        /*
         * Clumsy
         */
        Clumsy.SetupCustomOption();

        /*
         * Last Impostor
         */
        LastImpostor.SetupCustomOption();

        /*
         * Madmate
         */
        Madmate.SetupCustomMenuOptions();

        /*
         * Mare
         */
        Mare.SetupCustomOption();

        /*
         * Mimic
         */
        Mimic.SetupCustomOption();

        Stealer.SetupCustomOption();

        Swift.SetupCustomOption();

        /*
         * Tricky
         */
        Tricky.SetupCustomOption();

        TextOptionItem.Create(10000019, "RoleType.Misc", TabGroup.Addons) // NEUTRAL
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(127, 140, 141, byte.MaxValue));

        Egoist.SetupCustomOption();

        SetupLoversRoleOptionsToggle(23600);

        Reach.SetupCustomOptions();

        Rainbow.SetupCustomOptions();
        
        Workhorse.SetupCustomOption();

        #endregion

        #region Experimental Roles/Add-ons Settings
        TextOptionItem.Create(10000020, "OtherRoles.ImpostorRoles", TabGroup.OtherRoles)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(247, 70, 49, byte.MaxValue));

        Disperser.SetupCustomOption();        

        // 船员
        TextOptionItem.Create(10000021, "OtherRoles.CrewmateRoles", TabGroup.OtherRoles)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(140, 255, 255, byte.MaxValue));

        /*SetupRoleOptions(24700, TabGroup.OtherRoles, CustomRoles.SpeedBooster);
        SpeedBoosterUpSpeed = FloatOptionItem.Create(24703, "SpeedBoosterUpSpeed", new(0.1f, 1.0f, 0.1f), 0.2f, TabGroup.OtherRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.SpeedBooster])
            .SetValueFormat(OptionFormat.Multiplier);
        SpeedBoosterTimes = IntegerOptionItem.Create(24704, "SpeedBoosterTimes", new(1, 99, 1), 5, TabGroup.OtherRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.SpeedBooster])
            .SetValueFormat(OptionFormat.Times); */
                
        // 中立
        TextOptionItem.Create(10000022, "OtherRoles.NeutralRoles", TabGroup.OtherRoles)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(127, 140, 141, byte.MaxValue));
        
        Doppelganger.SetupCustomOption();

        God.SetupCustomOption();

        if (Quizmaster.InExperimental)
            Quizmaster.SetupCustomOption();


        // 副职
        TextOptionItem.Create(10000023, "OtherRoles.Addons", TabGroup.OtherRoles)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(255, 154, 206, byte.MaxValue));

        //SetupAdtRoleOptions(25300, CustomRoles.Ntr, tab: TabGroup.OtherRoles);

        Youtuber.SetupCustomOptions();

        #endregion

        #region System Settings
        GradientTagsOpt = BooleanOptionItem.Create(60031, "EnableGadientTags", false, TabGroup.SystemSettings, false)
            .SetHeader(true);
        EnableKillerLeftCommand = BooleanOptionItem.Create(60040, "EnableKillerLeftCommand", true, TabGroup.SystemSettings, false)
            .SetColor(Color.green)
            .HideInHnS();
        SeeEjectedRolesInMeeting = BooleanOptionItem.Create(60041, "SeeEjectedRolesInMeeting", true, TabGroup.SystemSettings, false)
            .SetColor(Color.green)
            .HideInHnS();
        
        KickLowLevelPlayer = IntegerOptionItem.Create(60050, "KickLowLevelPlayer", new(0, 100, 1), 0, TabGroup.SystemSettings, false)
            .SetValueFormat(OptionFormat.Level)
            .SetHeader(true);
        TempBanLowLevelPlayer = BooleanOptionItem.Create(60051, "TempBanLowLevelPlayer", false, TabGroup.SystemSettings, false)
            .SetParent(KickLowLevelPlayer)
            .SetValueFormat(OptionFormat.Times);
        ApplyAllowList = BooleanOptionItem.Create(60060, "ApplyWhiteList", false, TabGroup.SystemSettings, false);
        AllowOnlyWhiteList = BooleanOptionItem.Create(60061, "AllowOnlyWhiteList", false, TabGroup.SystemSettings, false);

        KickOtherPlatformPlayer = BooleanOptionItem.Create(60070, "KickOtherPlatformPlayer", false, TabGroup.SystemSettings, false);
        OptKickAndroidPlayer = BooleanOptionItem.Create(60071, "OptKickAndroidPlayer", false, TabGroup.SystemSettings, false)
            .SetParent(KickOtherPlatformPlayer);
        OptKickIphonePlayer = BooleanOptionItem.Create(60072, "OptKickIphonePlayer", false, TabGroup.SystemSettings, false)
            .SetParent(KickOtherPlatformPlayer);
        OptKickXboxPlayer = BooleanOptionItem.Create(60073, "OptKickXboxPlayer", false, TabGroup.SystemSettings, false)
            .SetParent(KickOtherPlatformPlayer);
        OptKickPlayStationPlayer = BooleanOptionItem.Create(60074, "OptKickPlayStationPlayer", false, TabGroup.SystemSettings, false)
            .SetParent(KickOtherPlatformPlayer);
        OptKickNintendoPlayer = BooleanOptionItem.Create(60075, "OptKickNintendoPlayer", false, TabGroup.SystemSettings, false)
            .SetParent(KickOtherPlatformPlayer); //Switch
        KickPlayerFriendCodeNotExist = BooleanOptionItem.Create(60080, "KickPlayerFriendCodeNotExist", true, TabGroup.SystemSettings, false);
        TempBanPlayerFriendCodeNotExist = BooleanOptionItem.Create(60081, "TempBanPlayerFriendCodeNotExist", false, TabGroup.SystemSettings, false)
            .SetParent(KickPlayerFriendCodeNotExist);
        AutoKickStart = BooleanOptionItem.Create(60140, "AutoKickStart", false, TabGroup.SystemSettings, false);
        AutoKickStartTimes = IntegerOptionItem.Create(60141, "AutoKickStartTimes", new(0, 99, 1), 1, TabGroup.SystemSettings, false)
            .SetParent(AutoKickStart)
            .SetValueFormat(OptionFormat.Times);
        AutoKickStartAsBan = BooleanOptionItem.Create(60142, "AutoKickStartAsBan", false, TabGroup.SystemSettings, false)
            .SetParent(AutoKickStart);
        TempBanPlayersWhoKeepQuitting = BooleanOptionItem.Create(60150, "TempBanPlayersWhoKeepQuitting", false, TabGroup.SystemSettings, false);
        QuitTimesTillTempBan = IntegerOptionItem.Create(60151, "QuitTimesTillTempBan", new(1, 15, 1), 4, TabGroup.SystemSettings, false)
            .SetValueFormat(OptionFormat.Times)
            .SetParent(TempBanPlayersWhoKeepQuitting);
        ApplyVipList = BooleanOptionItem.Create(60090, "ApplyVipList", true, TabGroup.SystemSettings, false).SetHeader(true);
        ApplyDenyNameList = BooleanOptionItem.Create(60100, "ApplyDenyNameList", true, TabGroup.SystemSettings, true);
        ApplyBanList = BooleanOptionItem.Create(60110, "ApplyBanList", true, TabGroup.SystemSettings, true);
        ApplyModeratorList = BooleanOptionItem.Create(60120, "ApplyModeratorList", false, TabGroup.SystemSettings, false);
        AllowSayCommand = BooleanOptionItem.Create(60121, "AllowSayCommand", false, TabGroup.SystemSettings, false)
            .SetParent(ApplyModeratorList);
        //ApplyReminderMsg = BooleanOptionItem.Create(60130, "ApplyReminderMsg", false, TabGroup.SystemSettings, false);
        /*TimeForReminder = IntegerOptionItem.Create(60131, "TimeForReminder", new(0, 99, 1), 3, TabGroup.SystemSettings, false)
            .SetParent(TimeForReminder)
            .SetValueFormat(OptionFormat.Seconds); */
        /*AutoKickStopWords = BooleanOptionItem.Create(60160, "AutoKickStopWords", false, TabGroup.SystemSettings, false);
        AutoKickStopWordsTimes = IntegerOptionItem.Create(60161, "AutoKickStopWordsTimes", new(0, 99, 1), 3, TabGroup.SystemSettings, false)
            .SetParent(AutoKickStopWords)
            .SetValueFormat(OptionFormat.Times);
        AutoKickStopWordsAsBan = BooleanOptionItem.Create(60162, "AutoKickStopWordsAsBan", false, TabGroup.SystemSettings, false)
            .SetParent(AutoKickStopWords);
        AutoWarnStopWords = BooleanOptionItem.Create(60163, "AutoWarnStopWords", false, TabGroup.SystemSettings, false); */
        MinWaitAutoStart = FloatOptionItem.Create(60170, "MinWaitAutoStart", new(0f, 10f, 0.5f), 1.5f, TabGroup.SystemSettings, false).SetHeader(true);
        MaxWaitAutoStart = FloatOptionItem.Create(60180, "MaxWaitAutoStart", new(0f, 10f, 0.5f), 1.5f, TabGroup.SystemSettings, false);
        PlayerAutoStart = IntegerOptionItem.Create(60190, "PlayerAutoStart", new(1, 15, 1), 14, TabGroup.SystemSettings, false);
        AutoStartTimer = IntegerOptionItem.Create(60200, "AutoStartTimer", new(10, 600, 1), 20, TabGroup.SystemSettings, false)
            .SetValueFormat(OptionFormat.Seconds);
        AutoPlayAgain = BooleanOptionItem.Create(60210, "AutoPlayAgain", false, TabGroup.SystemSettings, false);
        AutoPlayAgainCountdown = IntegerOptionItem.Create(60211, "AutoPlayAgainCountdown", new(1, 20, 1), 10, TabGroup.SystemSettings, false)
            .SetParent(AutoPlayAgain)
            .SetValueFormat(OptionFormat.Seconds);
        /*ShowLobbyCode = BooleanOptionItem.Create(60220, "ShowLobbyCode", true, TabGroup.SystemSettings, false)
            .SetColor(Color.blue); */
        LowLoadMode = BooleanOptionItem.Create(60230, "LowLoadMode", true, TabGroup.SystemSettings, false)
            .SetHeader(true)
            .SetColor(Color.green);
        EndWhenPlayerBug = BooleanOptionItem.Create(60240, "EndWhenPlayerBug", true, TabGroup.SystemSettings, false)
            .SetColor(Color.blue);
        HideExileChat = BooleanOptionItem.Create(60292, "HideExileChat", true, TabGroup.SystemSettings, false)
            .SetColor(Color.blue)
            .HideInHnS();
        RemovePetsAtDeadPlayers = BooleanOptionItem.Create(60294, "RemovePetsAtDeadPlayers", false, TabGroup.SystemSettings, false)
            .SetColor(Color.magenta);

        CheatResponses = StringOptionItem.Create(60250, "CheatResponses", CheatResponsesName, 0, TabGroup.SystemSettings, false)
            .SetHeader(true);
        DisableVoteBan = BooleanOptionItem.Create(60260, "DisableVoteBan", false, TabGroup.SystemSettings, true);

        AutoDisplayKillLog = BooleanOptionItem.Create(60270, "AutoDisplayKillLog", true, TabGroup.SystemSettings, false)
            .SetHeader(true)
            .HideInHnS();
        OldKillLog = BooleanOptionItem.Create(60291, "RevertOldKillLog", false, TabGroup.SystemSettings, false)
            .HideInHnS();
        AutoDisplayLastRoles = BooleanOptionItem.Create(60280, "AutoDisplayLastRoles", true, TabGroup.SystemSettings, false)
            .HideInHnS();
        AutoDisplayLastResult = BooleanOptionItem.Create(60290, "AutoDisplayLastResult", true, TabGroup.SystemSettings, false)
            .HideInHnS();
        
        SuffixMode = StringOptionItem.Create(60300, "SuffixMode", suffixModes, 0, TabGroup.SystemSettings, true)
            .SetHeader(true);
        HideHostText = BooleanOptionItem.Create(60311, "HideHostText", false, TabGroup.SystemSettings, false);
        HideGameSettings = BooleanOptionItem.Create(60310, "HideGameSettings", false, TabGroup.SystemSettings, false);
        //DIYGameSettings = BooleanOptionItem.Create(60320, "DIYGameSettings", false, TabGroup.SystemSettings, false);
        PlayerCanSetColor = BooleanOptionItem.Create(60330, "PlayerCanSetColor", false, TabGroup.SystemSettings, false);
        PlayerCanUseQuitCommand = BooleanOptionItem.Create(60331, "PlayerCanUseQuitCommand", false, TabGroup.SystemSettings, false);
        PlayerCanSetName = BooleanOptionItem.Create(60332, "PlayerCanSetName", false, TabGroup.SystemSettings, false);
        PlayerCanUseTP = BooleanOptionItem.Create(60333, "PlayerCanUseTP", false, TabGroup.SystemSettings, false);
        CanPlayMiniGames = BooleanOptionItem.Create(60334, "CanPlayMiniGames", false, TabGroup.SystemSettings, false);
        FormatNameMode = StringOptionItem.Create(60340, "FormatNameMode", formatNameModes, 0, TabGroup.SystemSettings, false);
        DisableEmojiName = BooleanOptionItem.Create(60350, "DisableEmojiName", true, TabGroup.SystemSettings, false);
        ChangeNameToRoleInfo = BooleanOptionItem.Create(60360, "ChangeNameToRoleInfo", true, TabGroup.SystemSettings, false)
            .HideInHnS();
        SendRoleDescriptionFirstMeeting = BooleanOptionItem.Create(60370, "SendRoleDescriptionFirstMeeting", false, TabGroup.SystemSettings, false)
            .HideInHnS();

        NoGameEnd = BooleanOptionItem.Create(60380, "NoGameEnd", false, TabGroup.SystemSettings, false)
            .SetColor(Color.red)
            .SetHeader(true);
        AllowConsole = BooleanOptionItem.Create(60382, "AllowConsole", false, TabGroup.SystemSettings, false)
            .SetColor(Color.red);
        DisableAntiBlackoutProtects = BooleanOptionItem.Create(60384, "DisableAntiBlackoutProtects", false, TabGroup.SystemSettings, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(Color.red);

        RoleAssigningAlgorithm = StringOptionItem.Create(60400, "RoleAssigningAlgorithm", roleAssigningAlgorithms, 4, TabGroup.SystemSettings, true)
            .RegisterUpdateValueEvent((object obj, OptionItem.UpdateValueEventArgs args) => IRandom.SetInstanceById(args.CurrentValue))
            .SetHeader(true);
        KPDCamouflageMode = StringOptionItem.Create(60410, "KPDCamouflageMode", CamouflageMode, 0, TabGroup.SystemSettings, false)
            .HideInHnS()
            .SetHeader(true)
            .SetColor(new Color32(255, 192, 203, byte.MaxValue));
        //DebugModeManager.SetupCustomOption();
        EnableUpMode = BooleanOptionItem.Create(60430, "EnableYTPlan", false, TabGroup.SystemSettings, false)
            .HideInHnS()
            .SetColor(Color.cyan)
            .SetHeader(true);
        #endregion 

        #region Game Settings
        //FFA
        FFAManager.SetupCustomOption();

        // Hide & Seek
        TextOptionItem.Create(10000055, "MenuTitle.Hide&Seek", TabGroup.GameSettings)
            .SetGameMode(CustomGameMode.HidenSeekTOHE)
            .SetColor(Color.red);

        // Num impostors in Hide & Seek
        NumImpostorsHnS = IntegerOptionItem.Create(60891, "NumImpostorsHnS", new(1, 3, 1), 1, TabGroup.GameSettings, false)
            .SetHeader(true)
            .SetColor(Color.red)
            .SetGameMode(CustomGameMode.HidenSeekTOHE)
            .SetValueFormat(OptionFormat.Players);



        // Confirm Ejections Mode
        TextOptionItem.Create(10000024, "MenuTitle.Ejections", TabGroup.GameSettings)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(255, 238, 232, byte.MaxValue));
        CEMode = StringOptionItem.Create(60440, "ConfirmEjectionsMode", ConfirmEjectionsMode, 2, TabGroup.GameSettings, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetHeader(true)
            .SetColor(new Color32(255, 238, 232, byte.MaxValue));
        ShowImpRemainOnEject = BooleanOptionItem.Create(60441, "ShowImpRemainOnEject", true, TabGroup.GameSettings, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(255, 238, 232, byte.MaxValue));
        ShowNKRemainOnEject = BooleanOptionItem.Create(60442, "ShowNKRemainOnEject", true, TabGroup.GameSettings, false)
            .SetParent(ShowImpRemainOnEject)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(255, 238, 232, byte.MaxValue));
        ShowTeamNextToRoleNameOnEject = BooleanOptionItem.Create(60443, "ShowTeamNextToRoleNameOnEject", false, TabGroup.GameSettings, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(255, 238, 232, byte.MaxValue));
        ConfirmEgoistOnEject = BooleanOptionItem.Create(60444, "ConfirmEgoistOnEject", true, TabGroup.GameSettings, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(255, 238, 232, byte.MaxValue))
            .SetHeader(true);
        ConfirmLoversOnEject = BooleanOptionItem.Create(60445, "ConfirmLoversOnEject", true, TabGroup.GameSettings, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(255, 238, 232, byte.MaxValue));

        //Maps Settings
        TextOptionItem.Create(10000025, "MenuTitle.MapsSettings", TabGroup.GameSettings)
            .SetColor(new Color32(19, 188, 233, byte.MaxValue));
        // Random Maps Mode
        RandomMapsMode = BooleanOptionItem.Create(60450, "RandomMapsMode", false, TabGroup.GameSettings, false)
            .SetHeader(true)
            .SetColor(new Color32(19, 188, 233, byte.MaxValue));
        SkeldChance = IntegerOptionItem.Create(60451, "SkeldChance", new(0, 100, 5), 10, TabGroup.GameSettings, false)
            .SetParent(RandomMapsMode)
            .SetValueFormat(OptionFormat.Percent);
        MiraChance = IntegerOptionItem.Create(60452, "MiraChance", new(0, 100, 5), 10, TabGroup.GameSettings, false)
            .SetParent(RandomMapsMode)
            .SetValueFormat(OptionFormat.Percent);
        PolusChance = IntegerOptionItem.Create(60453, "PolusChance", new(0, 100, 5), 10, TabGroup.GameSettings, false)
            .SetParent(RandomMapsMode)
            .SetValueFormat(OptionFormat.Percent);
        DleksChance = IntegerOptionItem.Create(60457, "DleksChance", new(0, 100, 5), 10, TabGroup.GameSettings, false)
            .SetParent(RandomMapsMode)
            .SetValueFormat(OptionFormat.Percent);
        AirshipChance = IntegerOptionItem.Create(60454, "AirshipChance", new(0, 100, 5), 10, TabGroup.GameSettings, false)
            .SetParent(RandomMapsMode)
            .SetValueFormat(OptionFormat.Percent);
        FungleChance = IntegerOptionItem.Create(60455, "FungleChance", new(0, 100, 5), 10, TabGroup.GameSettings, false)
            .SetParent(RandomMapsMode)
            .SetValueFormat(OptionFormat.Percent);
        UseMoreRandomMapSelection = BooleanOptionItem.Create(60456, "UseMoreRandomMapSelection", false, TabGroup.GameSettings, false)
            .SetParent(RandomMapsMode)
            .SetValueFormat(OptionFormat.Percent);

        NewHideMsg = BooleanOptionItem.Create(60460, "NewHideMsg", true, TabGroup.GameSettings, false)
            .SetHidden(true)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(193, 255, 209, byte.MaxValue));

        // Random Spawn
        RandomSpawn.SetupCustomOption();
        
        MapModification = BooleanOptionItem.Create(60480, "MapModification", false, TabGroup.GameSettings, false)
            .SetColor(new Color32(19, 188, 233, byte.MaxValue));
        // Airship Variable Electrical
        AirshipVariableElectrical = BooleanOptionItem.Create(60481, "AirshipVariableElectrical", false, TabGroup.GameSettings, false)
            //.SetGameMode(CustomGameMode.Standard)
            .SetParent(MapModification)
            .SetColor(new Color32(19, 188, 233, byte.MaxValue));
        // Disable Airship Moving Platform
        DisableAirshipMovingPlatform = BooleanOptionItem.Create(60482, "DisableAirshipMovingPlatform", false, TabGroup.GameSettings, false)
            //.SetGameMode(CustomGameMode.Standard)
            .SetParent(MapModification)
            .SetColor(new Color32(19, 188, 233, byte.MaxValue));
        // Disable Spore Trigger On Fungle
        DisableSporeTriggerOnFungle = BooleanOptionItem.Create(60483, "DisableSporeTriggerOnFungle", false, TabGroup.GameSettings, false)
            //.SetGameMode(CustomGameMode.Standard)
            .SetParent(MapModification)
            .SetColor(new Color32(19, 188, 233, byte.MaxValue));
        // Disable Zipline On Fungle
        DisableZiplineOnFungle = BooleanOptionItem.Create(60490, "DisableZiplineOnFungle", false, TabGroup.GameSettings, false)
            //.SetGameMode(CustomGameMode.Standard)
            .SetParent(MapModification)
            .SetColor(new Color32(19, 188, 233, byte.MaxValue));
        // Disable Zipline From Top
        DisableZiplineFromTop = BooleanOptionItem.Create(60491, "DisableZiplineFromTop", false, TabGroup.GameSettings, false)
            //.SetGameMode(CustomGameMode.Standard)
            .SetParent(DisableZiplineOnFungle)
            .SetColor(new Color32(19, 188, 233, byte.MaxValue));
        // Disable Zipline From Under
        DisableZiplineFromUnder = BooleanOptionItem.Create(60492, "DisableZiplineFromUnder", false, TabGroup.GameSettings, false)
            //.SetGameMode(CustomGameMode.Standard)
            .SetParent(DisableZiplineOnFungle)
            .SetColor(new Color32(19, 188, 233, byte.MaxValue));
        // Reset Doors After Meeting
        ResetDoorsEveryTurns = BooleanOptionItem.Create(60500, "ResetDoorsEveryTurns", false, TabGroup.GameSettings, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(19, 188, 233, byte.MaxValue));
        // Reset Doors Mode
        DoorsResetMode = StringOptionItem.Create(60501, "DoorsResetMode", EnumHelper.GetAllNames<DoorsReset.ResetMode>(), 2, TabGroup.GameSettings, false)
            .SetParent(ResetDoorsEveryTurns)
            .SetColor(new Color32(19, 188, 233, byte.MaxValue));
        // Change decontamination time on MiraHQ/Polus
        ChangeDecontaminationTime = BooleanOptionItem.Create(60503, "ChangeDecontaminationTime", false, TabGroup.GameSettings, false)
            .SetColor(new Color32(19, 188, 233, byte.MaxValue));
        // Decontamination time on MiraHQ
        DecontaminationTimeOnMiraHQ = FloatOptionItem.Create(60504, "DecontaminationTimeOnMiraHQ", new(0.5f, 10f, 0.25f), 3f, TabGroup.GameSettings, false)
            .SetParent(ChangeDecontaminationTime)
            .SetValueFormat(OptionFormat.Seconds)
            .SetColor(new Color32(19, 188, 233, byte.MaxValue));
        // Decontamination time on Polus
        DecontaminationTimeOnPolus = FloatOptionItem.Create(60505, "DecontaminationTimeOnPolus", new(0.5f, 10f, 0.25f), 3f, TabGroup.GameSettings, false)
            .SetParent(ChangeDecontaminationTime)
            .SetValueFormat(OptionFormat.Seconds)
            .SetColor(new Color32(19, 188, 233, byte.MaxValue));
        // Sabotage
        TextOptionItem.Create(10000026, "MenuTitle.Sabotage", TabGroup.GameSettings)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(243, 96, 96, byte.MaxValue))
            .SetHeader(true);
        // CommsCamouflage
        CommsCamouflage = BooleanOptionItem.Create(60510, "CommsCamouflage", true, TabGroup.GameSettings, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetHeader(true)
            .SetColor(new Color32(243, 96, 96, byte.MaxValue));
        DisableOnSomeMaps = BooleanOptionItem.Create(60511, "DisableOnSomeMaps", false, TabGroup.GameSettings, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetParent(CommsCamouflage);
        DisableOnSkeld = BooleanOptionItem.Create(60512, "DisableOnSkeld", false, TabGroup.GameSettings, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetParent(DisableOnSomeMaps);
        DisableOnMira = BooleanOptionItem.Create(60513, "DisableOnMira", false, TabGroup.GameSettings, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetParent(DisableOnSomeMaps);
        DisableOnPolus = BooleanOptionItem.Create(60514, "DisableOnPolus", false, TabGroup.GameSettings, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetParent(DisableOnSomeMaps);
        DisableOnDleks = BooleanOptionItem.Create(60517, "DisableOnDleks", false, TabGroup.GameSettings, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetParent(DisableOnSomeMaps);
        DisableOnAirship = BooleanOptionItem.Create(60515, "DisableOnAirship", false, TabGroup.GameSettings, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetParent(DisableOnSomeMaps);
        DisableOnFungle = BooleanOptionItem.Create(60516, "DisableOnFungle", false, TabGroup.GameSettings, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetParent(DisableOnSomeMaps);
        DisableReportWhenCC = BooleanOptionItem.Create(60520, "DisableReportWhenCC", false, TabGroup.GameSettings, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetParent(CommsCamouflage);
        // Sabotage Cooldown Control
        SabotageCooldownControl = BooleanOptionItem.Create(60530, "SabotageCooldownControl", false, TabGroup.GameSettings, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(243, 96, 96, byte.MaxValue));
        SabotageCooldown = FloatOptionItem.Create(60535, "SabotageCooldown", new(1f, 60f, 1f), 30f, TabGroup.GameSettings, false)
            .SetParent(SabotageCooldownControl)
            .SetValueFormat(OptionFormat.Seconds)
            .SetGameMode(CustomGameMode.Standard);
        // Sabotage Duration Control
        SabotageTimeControl = BooleanOptionItem.Create(60540, "SabotageTimeControl", false, TabGroup.GameSettings, false)
            .SetColor(new Color32(243, 96, 96, byte.MaxValue))
            .SetGameMode(CustomGameMode.Standard);
        // The Skeld
        SkeldReactorTimeLimit = FloatOptionItem.Create(60541, "SkeldReactorTimeLimit", new(5f, 90f, 1f), 30f, TabGroup.GameSettings, false)
            .SetParent(SabotageTimeControl)
            .SetValueFormat(OptionFormat.Seconds)
            .SetGameMode(CustomGameMode.Standard);
        SkeldO2TimeLimit = FloatOptionItem.Create(60542, "SkeldO2TimeLimit", new(5f, 90f, 1f), 30f, TabGroup.GameSettings, false)
            .SetParent(SabotageTimeControl)
            .SetValueFormat(OptionFormat.Seconds)
            .SetGameMode(CustomGameMode.Standard);
        // Mira HQ
        MiraReactorTimeLimit = FloatOptionItem.Create(60543, "MiraReactorTimeLimit", new(5f, 90f, 1f), 45f, TabGroup.GameSettings, false)
            .SetParent(SabotageTimeControl)
            .SetValueFormat(OptionFormat.Seconds)
            .SetGameMode(CustomGameMode.Standard);
        MiraO2TimeLimit = FloatOptionItem.Create(60544, "MiraO2TimeLimit", new(5f, 90f, 1f), 45f, TabGroup.GameSettings, false)
            .SetParent(SabotageTimeControl)
            .SetValueFormat(OptionFormat.Seconds)
            .SetGameMode(CustomGameMode.Standard);
        // Polus
        PolusReactorTimeLimit = FloatOptionItem.Create(60545, "PolusReactorTimeLimit", new(5f, 90f, 1f), 60f, TabGroup.GameSettings, false)
            .SetParent(SabotageTimeControl)
            .SetValueFormat(OptionFormat.Seconds)
            .SetGameMode(CustomGameMode.Standard);
        // The Airship
        AirshipReactorTimeLimit = FloatOptionItem.Create(60546, "AirshipReactorTimeLimit", new(5f, 90f, 1f), 90f, TabGroup.GameSettings, false)
            .SetParent(SabotageTimeControl)
            .SetValueFormat(OptionFormat.Seconds)
            .SetGameMode(CustomGameMode.Standard);
        // The Fungle
        FungleReactorTimeLimit = FloatOptionItem.Create(60547, "FungleReactorTimeLimit", new(5f, 90f, 1f), 60f, TabGroup.GameSettings, false)
            .SetParent(SabotageTimeControl)
            .SetValueFormat(OptionFormat.Seconds)
            .SetGameMode(CustomGameMode.Standard);
        FungleMushroomMixupDuration = FloatOptionItem.Create(60548, "FungleMushroomMixupDuration", new(5f, 90f, 1f), 10f, TabGroup.GameSettings, false)
            .SetParent(SabotageTimeControl)
            .SetValueFormat(OptionFormat.Seconds)
            .SetGameMode(CustomGameMode.Standard);
        // LightsOutSpecialSettings
        LightsOutSpecialSettings = BooleanOptionItem.Create(60550, "LightsOutSpecialSettings", false, TabGroup.GameSettings, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(243, 96, 96, byte.MaxValue));
        BlockDisturbancesToSwitches = BooleanOptionItem.Create(60551, "BlockDisturbancesToSwitches", false, TabGroup.GameSettings, false)
            .SetParent(LightsOutSpecialSettings)
            .SetGameMode(CustomGameMode.Standard);
        DisableAirshipViewingDeckLightsPanel = BooleanOptionItem.Create(60552, "DisableAirshipViewingDeckLightsPanel", false, TabGroup.GameSettings, false)
            .SetParent(LightsOutSpecialSettings)
            .SetGameMode(CustomGameMode.Standard);
        DisableAirshipGapRoomLightsPanel = BooleanOptionItem.Create(60553, "DisableAirshipGapRoomLightsPanel", false, TabGroup.GameSettings, false)
            .SetParent(LightsOutSpecialSettings)
            .SetGameMode(CustomGameMode.Standard);
        DisableAirshipCargoLightsPanel = BooleanOptionItem.Create(60554, "DisableAirshipCargoLightsPanel", false, TabGroup.GameSettings, false)
            .SetParent(LightsOutSpecialSettings)
            .SetGameMode(CustomGameMode.Standard);


        // Disable
        TextOptionItem.Create(10000027, "MenuTitle.Disable", TabGroup.GameSettings)
            .SetColor(new Color32(255, 153, 153, byte.MaxValue))
            .HideInHnS();

        DisableShieldAnimations = BooleanOptionItem.Create(60560, "DisableShieldAnimations", false, TabGroup.GameSettings, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetHeader(true)
            .SetColor(new Color32(255, 153, 153, byte.MaxValue));
        DisableKillAnimationOnGuess = BooleanOptionItem.Create(60561, "DisableKillAnimationOnGuess", false, TabGroup.GameSettings, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(255, 153, 153, byte.MaxValue));
        DisableVanillaRoles = BooleanOptionItem.Create(60562, "DisableVanillaRoles", true, TabGroup.GameSettings, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(255, 153, 153, byte.MaxValue));
        DisableTaskWin = BooleanOptionItem.Create(60563, "DisableTaskWin", false, TabGroup.GameSettings, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(255, 153, 153, byte.MaxValue));
        DisableMeeting = BooleanOptionItem.Create(60564, "DisableMeeting", false, TabGroup.GameSettings, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(255, 153, 153, byte.MaxValue));
        // Disable Sabotage / CloseDoor
        DisableSabotage = BooleanOptionItem.Create(60565, "DisableSabotage", false, TabGroup.GameSettings, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(255, 153, 153, byte.MaxValue));
        DisableCloseDoor = BooleanOptionItem.Create(60566, "DisableCloseDoor", false, TabGroup.GameSettings, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(255, 153, 153, byte.MaxValue));
        // Disable Devices
        DisableDevices = BooleanOptionItem.Create(60570, "DisableDevices", false, TabGroup.GameSettings, false)
            .SetColor(new Color32(255, 153, 153, byte.MaxValue))
            .HideInHnS();
        //.SetGameMode(CustomGameMode.Standard);
        DisableSkeldDevices = BooleanOptionItem.Create(60571, "DisableSkeldDevices", false, TabGroup.GameSettings, false)
            .SetParent(DisableDevices);
        //.SetGameMode(CustomGameMode.Standard);
        DisableSkeldAdmin = BooleanOptionItem.Create(60572, "DisableSkeldAdmin", false, TabGroup.GameSettings, false)
            .SetParent(DisableSkeldDevices);
        //.SetGameMode(CustomGameMode.Standard);
        DisableSkeldCamera = BooleanOptionItem.Create(60573, "DisableSkeldCamera", false, TabGroup.GameSettings, false)
            .SetParent(DisableSkeldDevices);
        //.SetGameMode(CustomGameMode.Standard);
        DisableMiraHQDevices = BooleanOptionItem.Create(60574, "DisableMiraHQDevices", false, TabGroup.GameSettings, false)
            .SetParent(DisableDevices);
        //.SetGameMode(CustomGameMode.Standard);
        DisableMiraHQAdmin = BooleanOptionItem.Create(60575, "DisableMiraHQAdmin", false, TabGroup.GameSettings, false)
            .SetParent(DisableMiraHQDevices);
        //.SetGameMode(CustomGameMode.Standard);
        DisableMiraHQDoorLog = BooleanOptionItem.Create(60576, "DisableMiraHQDoorLog", false, TabGroup.GameSettings, false)
            .SetParent(DisableMiraHQDevices);
        //.SetGameMode(CustomGameMode.Standard);
        DisablePolusDevices = BooleanOptionItem.Create(60577, "DisablePolusDevices", false, TabGroup.GameSettings, false)
            .SetParent(DisableDevices);
        //.SetGameMode(CustomGameMode.Standard);
        DisablePolusAdmin = BooleanOptionItem.Create(60578, "DisablePolusAdmin", false, TabGroup.GameSettings, false)
            .SetParent(DisablePolusDevices);
        //.SetGameMode(CustomGameMode.Standard);
        DisablePolusCamera = BooleanOptionItem.Create(60579, "DisablePolusCamera", false, TabGroup.GameSettings, false)
            .SetParent(DisablePolusDevices);
        //.SetGameMode(CustomGameMode.Standard);
        DisablePolusVital = BooleanOptionItem.Create(60580, "DisablePolusVital", false, TabGroup.GameSettings, false)
            .SetParent(DisablePolusDevices);
        //.SetGameMode(CustomGameMode.Standard);
        DisableAirshipDevices = BooleanOptionItem.Create(60581, "DisableAirshipDevices", false, TabGroup.GameSettings, false)
            .SetParent(DisableDevices);
        //.SetGameMode(CustomGameMode.Standard);
        DisableAirshipCockpitAdmin = BooleanOptionItem.Create(60582, "DisableAirshipCockpitAdmin", false, TabGroup.GameSettings, false)
            .SetParent(DisableAirshipDevices);
        //.SetGameMode(CustomGameMode.Standard);
        DisableAirshipRecordsAdmin = BooleanOptionItem.Create(60583, "DisableAirshipRecordsAdmin", false, TabGroup.GameSettings, false)
            .SetParent(DisableAirshipDevices);
        //.SetGameMode(CustomGameMode.Standard);
        DisableAirshipCamera = BooleanOptionItem.Create(60584, "DisableAirshipCamera", false, TabGroup.GameSettings, false)
            .SetParent(DisableAirshipDevices);
        //.SetGameMode(CustomGameMode.Standard);
        DisableAirshipVital = BooleanOptionItem.Create(60585, "DisableAirshipVital", false, TabGroup.GameSettings, false)
            .SetParent(DisableAirshipDevices);
        //.SetGameMode(CustomGameMode.Standard);
        DisableFungleDevices = BooleanOptionItem.Create(60586, "DisableFungleDevices", false, TabGroup.GameSettings, false)
            .SetParent(DisableDevices);
        //.SetGameMode(CustomGameMode.Standard);
        DisableFungleBinoculars = BooleanOptionItem.Create(60587, "DisableFungleBinoculars", false, TabGroup.GameSettings, false)
            .SetParent(DisableFungleDevices);
        //.SetGameMode(CustomGameMode.Standard);
        DisableFungleVital = BooleanOptionItem.Create(60588, "DisableFungleVital", false, TabGroup.GameSettings, false)
            .SetParent(DisableFungleDevices);
        //.SetGameMode(CustomGameMode.Standard);
        DisableDevicesIgnoreConditions = BooleanOptionItem.Create(60589, "IgnoreConditions", false, TabGroup.GameSettings, false)
            .SetParent(DisableDevices);
        //.SetGameMode(CustomGameMode.Standard);
        DisableDevicesIgnoreImpostors = BooleanOptionItem.Create(60590, "IgnoreImpostors", false, TabGroup.GameSettings, false)
            .SetParent(DisableDevicesIgnoreConditions);
        //.SetGameMode(CustomGameMode.Standard);
        DisableDevicesIgnoreNeutrals = BooleanOptionItem.Create(60591, "IgnoreNeutrals", false, TabGroup.GameSettings, false)
            .SetParent(DisableDevicesIgnoreConditions);
        //.SetGameMode(CustomGameMode.Standard);
        DisableDevicesIgnoreCrewmates = BooleanOptionItem.Create(60592, "IgnoreCrewmates", false, TabGroup.GameSettings, false)
            .SetParent(DisableDevicesIgnoreConditions);
        //.SetGameMode(CustomGameMode.Standard);
        DisableDevicesIgnoreAfterAnyoneDied = BooleanOptionItem.Create(60593, "IgnoreAfterAnyoneDied", false, TabGroup.GameSettings, false)
            .SetParent(DisableDevicesIgnoreConditions);
        //.SetGameMode(CustomGameMode.Standard);

        //Disable Short Tasks
        DisableShortTasks = BooleanOptionItem.Create(60594, "DisableShortTasks", false, TabGroup.TaskSettings, false)
            .HideInFFA()
            .SetHeader(true)
            .SetColor(new Color32(239, 89, 175, byte.MaxValue));
        DisableCleanVent = BooleanOptionItem.Create(60595, "DisableCleanVent", false, TabGroup.TaskSettings, false)
            .SetParent(DisableShortTasks);
        DisableCalibrateDistributor = BooleanOptionItem.Create(60596, "DisableCalibrateDistributor", false, TabGroup.TaskSettings, false)
            .SetParent(DisableShortTasks);
        DisableChartCourse = BooleanOptionItem.Create(60597, "DisableChartCourse", false, TabGroup.TaskSettings, false)
            .SetParent(DisableShortTasks);
        DisableStabilizeSteering = BooleanOptionItem.Create(60598, "DisableStabilizeSteering", false, TabGroup.TaskSettings, false)
            .SetParent(DisableShortTasks);
        DisableCleanO2Filter = BooleanOptionItem.Create(60599, "DisableCleanO2Filter", false, TabGroup.TaskSettings, false)
            .SetParent(DisableShortTasks);
        DisableUnlockManifolds = BooleanOptionItem.Create(60600, "DisableUnlockManifolds", false, TabGroup.TaskSettings, false)
            .SetParent(DisableShortTasks);
        DisablePrimeShields = BooleanOptionItem.Create(60601, "DisablePrimeShields", false, TabGroup.TaskSettings, false)
            .SetParent(DisableShortTasks);
        DisableMeasureWeather = BooleanOptionItem.Create(60602, "DisableMeasureWeather", false, TabGroup.TaskSettings, false)
            .SetParent(DisableShortTasks);
        DisableBuyBeverage = BooleanOptionItem.Create(60603, "DisableBuyBeverage", false, TabGroup.TaskSettings, false)
            .SetParent(DisableShortTasks);
        DisableAssembleArtifact = BooleanOptionItem.Create(60604, "DisableAssembleArtifact", false, TabGroup.TaskSettings, false)
            .SetParent(DisableShortTasks);
        DisableSortSamples = BooleanOptionItem.Create(60605, "DisableSortSamples", false, TabGroup.TaskSettings, false)
            .SetParent(DisableShortTasks);
        DisableProcessData = BooleanOptionItem.Create(60606, "DisableProcessData", false, TabGroup.TaskSettings, false)
            .SetParent(DisableShortTasks);
        DisableRunDiagnostics = BooleanOptionItem.Create(60607, "DisableRunDiagnostics", false, TabGroup.TaskSettings, false)
            .SetParent(DisableShortTasks);
        DisableRepairDrill = BooleanOptionItem.Create(60608, "DisableRepairDrill", false, TabGroup.TaskSettings, false)
            .SetParent(DisableShortTasks);
        DisableAlignTelescope = BooleanOptionItem.Create(60609, "DisableAlignTelescope", false, TabGroup.TaskSettings, false)
            .SetParent(DisableShortTasks);
        DisableRecordTemperature = BooleanOptionItem.Create(60610, "DisableRecordTemperature", false, TabGroup.TaskSettings, false)
            .SetParent(DisableShortTasks);
        DisableFillCanisters = BooleanOptionItem.Create(60611, "DisableFillCanisters", false, TabGroup.TaskSettings, false)
            .SetParent(DisableShortTasks);
        DisableMonitorTree = BooleanOptionItem.Create(60612, "DisableMonitorTree", false, TabGroup.TaskSettings, false)
            .SetParent(DisableShortTasks);
        DisableStoreArtifacts = BooleanOptionItem.Create(60613, "DisableStoreArtifacts", false, TabGroup.TaskSettings, false)
            .SetParent(DisableShortTasks);
        DisablePutAwayPistols = BooleanOptionItem.Create(60614, "DisablePutAwayPistols", false, TabGroup.TaskSettings, false)
            .SetParent(DisableShortTasks);
        DisablePutAwayRifles = BooleanOptionItem.Create(60615, "DisablePutAwayRifles", false, TabGroup.TaskSettings, false)
            .SetParent(DisableShortTasks);
        DisableMakeBurger = BooleanOptionItem.Create(60616, "DisableMakeBurger", false, TabGroup.TaskSettings, false)
            .SetParent(DisableShortTasks);
        DisableCleanToilet = BooleanOptionItem.Create(60617, "DisableCleanToilet", false, TabGroup.TaskSettings, false)
            .SetParent(DisableShortTasks);
        DisableDecontaminate = BooleanOptionItem.Create(60618, "DisableDecontaminate", false, TabGroup.TaskSettings, false)
            .SetParent(DisableShortTasks);
        DisableSortRecords = BooleanOptionItem.Create(60619, "DisableSortRecords", false, TabGroup.TaskSettings, false)
            .SetParent(DisableShortTasks);
        DisableFixShower = BooleanOptionItem.Create(60620, "DisableFixShower", false, TabGroup.TaskSettings, false)
            .SetParent(DisableShortTasks);
        DisablePickUpTowels = BooleanOptionItem.Create(60621, "DisablePickUpTowels", false, TabGroup.TaskSettings, false)
            .SetParent(DisableShortTasks);
        DisablePolishRuby = BooleanOptionItem.Create(60622, "DisablePolishRuby", false, TabGroup.TaskSettings, false)
            .SetParent(DisableShortTasks);
        DisableDressMannequin = BooleanOptionItem.Create(60623, "DisableDressMannequin", false, TabGroup.TaskSettings, false)
            .SetParent(DisableShortTasks);
        DisableFixAntenna = BooleanOptionItem.Create(60656, "DisableFixAntenna", false, TabGroup.TaskSettings, false)
            .SetParent(DisableShortTasks);
        DisableBuildSandcastle = BooleanOptionItem.Create(60657, "DisableBuildSandcastle", false, TabGroup.TaskSettings, false)
            .SetParent(DisableShortTasks);
        DisableCrankGenerator = BooleanOptionItem.Create(60658, "DisableCrankGenerator", false, TabGroup.TaskSettings, false)
            .SetParent(DisableShortTasks);
        DisableMonitorMushroom = BooleanOptionItem.Create(60659, "DisableMonitorMushroom", false, TabGroup.TaskSettings, false)
            .SetParent(DisableShortTasks);
        DisablePlayVideoGame = BooleanOptionItem.Create(60660, "DisablePlayVideoGame", false, TabGroup.TaskSettings, false)
            .SetParent(DisableShortTasks);
        DisableFindSignal = BooleanOptionItem.Create(60661, "DisableFindSignal", false, TabGroup.TaskSettings, false)
            .SetParent(DisableShortTasks);
        DisableThrowFisbee = BooleanOptionItem.Create(60662, "DisableThrowFisbee", false, TabGroup.TaskSettings, false)
            .SetParent(DisableShortTasks);
        DisableLiftWeights = BooleanOptionItem.Create(60663, "DisableLiftWeights", false, TabGroup.TaskSettings, false)
            .SetParent(DisableShortTasks);
        DisableCollectShells = BooleanOptionItem.Create(60664, "DisableCollectShells", false, TabGroup.TaskSettings, false)
            .SetParent(DisableShortTasks);


        //Disable Common Tasks
        DisableCommonTasks = BooleanOptionItem.Create(60627, "DisableCommonTasks", false, TabGroup.TaskSettings, false)
            .HideInFFA()
            .SetColor(new Color32(239, 89, 175, byte.MaxValue));
        DisableSwipeCard = BooleanOptionItem.Create(60628, "DisableSwipeCardTask", false, TabGroup.TaskSettings, false)
            .SetParent(DisableCommonTasks);
        DisableFixWiring = BooleanOptionItem.Create(60629, "DisableFixWiring", false, TabGroup.TaskSettings, false)
            .SetParent(DisableCommonTasks);
        DisableEnterIdCode = BooleanOptionItem.Create(60630, "DisableEnterIdCode", false, TabGroup.TaskSettings, false)
            .SetParent(DisableCommonTasks);
        DisableInsertKeys = BooleanOptionItem.Create(60631, "DisableInsertKeys", false, TabGroup.TaskSettings, false)
            .SetParent(DisableCommonTasks);
        DisableScanBoardingPass = BooleanOptionItem.Create(60632, "DisableScanBoardingPass", false, TabGroup.TaskSettings, false)
            .SetParent(DisableCommonTasks);
        DisableRoastMarshmallow = BooleanOptionItem.Create(60624, "DisableRoastMarshmallow", false, TabGroup.TaskSettings, false)
            .SetParent(DisableCommonTasks);
        DisableCollectSamples = BooleanOptionItem.Create(60625, "DisableCollectSamples", false, TabGroup.TaskSettings, false)
            .SetParent(DisableCommonTasks);
        DisableReplaceParts = BooleanOptionItem.Create(60626, "DisableReplaceParts", false, TabGroup.TaskSettings, false)
            .SetParent(DisableCommonTasks);


        //Disable Long Tasks
        DisableLongTasks = BooleanOptionItem.Create(60640, "DisableLongTasks", false, TabGroup.TaskSettings, false)
            .HideInFFA()
            .SetColor(new Color32(239, 89, 175, byte.MaxValue));
        DisableSubmitScan = BooleanOptionItem.Create(60641, "DisableSubmitScanTask", false, TabGroup.TaskSettings, false)
            .SetParent(DisableLongTasks);
        DisableUnlockSafe = BooleanOptionItem.Create(60642, "DisableUnlockSafeTask", false, TabGroup.TaskSettings, false)
            .SetParent(DisableLongTasks);
        DisableStartReactor = BooleanOptionItem.Create(60643, "DisableStartReactorTask", false, TabGroup.TaskSettings, false)
            .SetParent(DisableLongTasks);
        DisableResetBreaker = BooleanOptionItem.Create(60644, "DisableResetBreakerTask", false, TabGroup.TaskSettings, false)
            .SetParent(DisableLongTasks);
        DisableAlignEngineOutput = BooleanOptionItem.Create(60645, "DisableAlignEngineOutput", false, TabGroup.TaskSettings, false)
            .SetParent(DisableLongTasks);
        DisableInspectSample = BooleanOptionItem.Create(60646, "DisableInspectSample", false, TabGroup.TaskSettings, false)
            .SetParent(DisableLongTasks);
        DisableEmptyChute = BooleanOptionItem.Create(60647, "DisableEmptyChute", false, TabGroup.TaskSettings, false)
            .SetParent(DisableLongTasks);
        DisableClearAsteroids = BooleanOptionItem.Create(60648, "DisableClearAsteroids", false, TabGroup.TaskSettings, false)
            .SetParent(DisableLongTasks);
        DisableWaterPlants = BooleanOptionItem.Create(60649, "DisableWaterPlants", false, TabGroup.TaskSettings, false)
            .SetParent(DisableLongTasks);
        DisableOpenWaterways = BooleanOptionItem.Create(60650, "DisableOpenWaterways", false, TabGroup.TaskSettings, false)
            .SetParent(DisableLongTasks);
        DisableReplaceWaterJug = BooleanOptionItem.Create(60651, "DisableReplaceWaterJug", false, TabGroup.TaskSettings, false)
            .SetParent(DisableLongTasks);
        DisableRebootWifi = BooleanOptionItem.Create(60652, "DisableRebootWifi", false, TabGroup.TaskSettings, false)
            .SetParent(DisableLongTasks);
        DisableDevelopPhotos = BooleanOptionItem.Create(60653, "DisableDevelopPhotos", false, TabGroup.TaskSettings, false)
            .SetParent(DisableLongTasks);
        DisableRewindTapes = BooleanOptionItem.Create(60654, "DisableRewindTapes", false, TabGroup.TaskSettings, false)
            .SetParent(DisableLongTasks);
        DisableStartFans = BooleanOptionItem.Create(60655, "DisableStartFans", false, TabGroup.TaskSettings, false)
            .SetParent(DisableLongTasks);
        DisableCollectVegetables = BooleanOptionItem.Create(60633, "DisableCollectVegetables", false, TabGroup.TaskSettings, false)
            .SetParent(DisableLongTasks);
        DisableMineOres = BooleanOptionItem.Create(60634, "DisableMineOres", false, TabGroup.TaskSettings, false)
            .SetParent(DisableLongTasks);
        DisableExtractFuel = BooleanOptionItem.Create(60635, "DisableExtractFuel", false, TabGroup.TaskSettings, false)
            .SetParent(DisableLongTasks);
        DisableCatchFish = BooleanOptionItem.Create(60636, "DisableCatchFish", false, TabGroup.TaskSettings, false)
            .SetParent(DisableLongTasks);
        DisablePolishGem = BooleanOptionItem.Create(60637, "DisablePolishGem", false, TabGroup.TaskSettings, false)
            .SetParent(DisableLongTasks);
        DisableHelpCritter = BooleanOptionItem.Create(60638, "DisableHelpCritter", false, TabGroup.TaskSettings, false)
            .SetParent(DisableLongTasks);
        DisableHoistSupplies = BooleanOptionItem.Create(60639, "DisableHoistSupplies", false, TabGroup.TaskSettings, false)
            .SetParent(DisableLongTasks);
        


        //Disable Divert Power, Weather Nodes and etc. situational Tasks
        DisableOtherTasks = BooleanOptionItem.Create(60665, "DisableOtherTasks", false, TabGroup.TaskSettings, false)
            .HideInFFA()
            .SetColor(new Color32(239, 89, 175, byte.MaxValue));
        DisableUploadData = BooleanOptionItem.Create(60666, "DisableUploadDataTask", false, TabGroup.TaskSettings, false)
            .SetParent(DisableOtherTasks);
        DisableEmptyGarbage = BooleanOptionItem.Create(60667, "DisableEmptyGarbage", false, TabGroup.TaskSettings, false)
            .SetParent(DisableOtherTasks);
        DisableFuelEngines = BooleanOptionItem.Create(60668, "DisableFuelEngines", false, TabGroup.TaskSettings, false)
            .SetParent(DisableOtherTasks);
        DisableDivertPower = BooleanOptionItem.Create(60669, "DisableDivertPower", false, TabGroup.TaskSettings, false)
            .SetParent(DisableOtherTasks);
        DisableActivateWeatherNodes = BooleanOptionItem.Create(60670, "DisableActivateWeatherNodes", false, TabGroup.TaskSettings, false)
            .SetParent(DisableOtherTasks);



        TextOptionItem.Create(10000028, "MenuTitle.Guessers", TabGroup.TaskSettings)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(Color.yellow)
            .SetHeader(true);
        GuesserMode = BooleanOptionItem.Create(60680, "GuesserMode", false, TabGroup.TaskSettings, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(Color.yellow)
            .SetHeader(true);
        CrewmatesCanGuess = BooleanOptionItem.Create(60681, "CrewmatesCanGuess", false, TabGroup.TaskSettings, false)
            .SetParent(GuesserMode);
        ImpostorsCanGuess = BooleanOptionItem.Create(60682, "ImpostorsCanGuess", false, TabGroup.TaskSettings, false)
            .SetParent(GuesserMode);
        NeutralKillersCanGuess = BooleanOptionItem.Create(60683, "NeutralKillersCanGuess", false, TabGroup.TaskSettings, false)
            .SetParent(GuesserMode);
        PassiveNeutralsCanGuess = BooleanOptionItem.Create(60684, "PassiveNeutralsCanGuess", false, TabGroup.TaskSettings, false)
            .SetParent(GuesserMode);
        CanGuessAddons = BooleanOptionItem.Create(60685, "CanGuessAddons", true, TabGroup.TaskSettings, false)
            .SetParent(GuesserMode);
        CrewCanGuessCrew = BooleanOptionItem.Create(60686, "CrewCanGuessCrew", true, TabGroup.TaskSettings, false)
            .SetHidden(true)
            .SetParent(GuesserMode);
        ImpCanGuessImp = BooleanOptionItem.Create(60687, "ImpCanGuessImp", true, TabGroup.TaskSettings, false)
            .SetHidden(true)
            .SetParent(GuesserMode);
        HideGuesserCommands = BooleanOptionItem.Create(60688, "GuesserTryHideMsg", true, TabGroup.TaskSettings, false)
            .SetParent(GuesserMode)
            .SetColor(Color.green);

        ShowOnlyEnabledRolesInGuesserUI = BooleanOptionItem.Create(60689, "ShowOnlyEnabledRolesInGuesserUI", true, TabGroup.TaskSettings, false)
            .SetHeader(true)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(Color.cyan);


        TextOptionItem.Create(10000029, "MenuTitle.GuesserModeRoles", TabGroup.TaskSettings)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(Color.yellow)
            .SetHeader(true);

        Onbound.SetupCustomOptions();



        // Meeting Settings
        TextOptionItem.Create(10000030, "MenuTitle.Meeting", TabGroup.GameSettings)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(147, 241, 240, byte.MaxValue));
        // Sync Button
        SyncButtonMode = BooleanOptionItem.Create(60700, "SyncButtonMode", false, TabGroup.GameSettings, false)
            .SetHeader(true)
            .SetColor(new Color32(147, 241, 240, byte.MaxValue))
            .SetGameMode(CustomGameMode.Standard);
        SyncedButtonCount = IntegerOptionItem.Create(60701, "SyncedButtonCount", new(0, 100, 1), 10, TabGroup.GameSettings, false)
            .SetParent(SyncButtonMode)
            .SetValueFormat(OptionFormat.Times)
            .SetGameMode(CustomGameMode.Standard);
        // 全员存活时的会议时间
        AllAliveMeeting = BooleanOptionItem.Create(60710, "AllAliveMeeting", false, TabGroup.GameSettings, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(147, 241, 240, byte.MaxValue));
        AllAliveMeetingTime = FloatOptionItem.Create(60711, "AllAliveMeetingTime", new(1f, 300f, 1f), 10f, TabGroup.GameSettings, false)
            .SetParent(AllAliveMeeting)
            .SetValueFormat(OptionFormat.Seconds);
        // 附加紧急会议
        AdditionalEmergencyCooldown = BooleanOptionItem.Create(60720, "AdditionalEmergencyCooldown", false, TabGroup.GameSettings, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(147, 241, 240, byte.MaxValue));
        AdditionalEmergencyCooldownThreshold = IntegerOptionItem.Create(60721, "AdditionalEmergencyCooldownThreshold", new(1, 15, 1), 1, TabGroup.GameSettings, false)
            .SetParent(AdditionalEmergencyCooldown)
            .SetGameMode(CustomGameMode.Standard)
            .SetValueFormat(OptionFormat.Players);
        AdditionalEmergencyCooldownTime = FloatOptionItem.Create(60722, "AdditionalEmergencyCooldownTime", new(1f, 60f, 1f), 1f, TabGroup.GameSettings, false)
                .SetParent(AdditionalEmergencyCooldown)
            .SetGameMode(CustomGameMode.Standard)
            .SetValueFormat(OptionFormat.Seconds);
        // 投票相关设定
        VoteMode = BooleanOptionItem.Create(60730, "VoteMode", false, TabGroup.GameSettings, false)
            .SetColor(new Color32(147, 241, 240, byte.MaxValue))
            .SetGameMode(CustomGameMode.Standard);
        WhenSkipVote = StringOptionItem.Create(60731, "WhenSkipVote", voteModes[0..3], 0, TabGroup.GameSettings, false)
            .SetParent(VoteMode)
            .SetGameMode(CustomGameMode.Standard);
        WhenSkipVoteIgnoreFirstMeeting = BooleanOptionItem.Create(60732, "WhenSkipVoteIgnoreFirstMeeting", false, TabGroup.GameSettings, false)
            .SetParent(WhenSkipVote)
            .SetGameMode(CustomGameMode.Standard);
        WhenSkipVoteIgnoreNoDeadBody = BooleanOptionItem.Create(60733, "WhenSkipVoteIgnoreNoDeadBody", false, TabGroup.GameSettings, false)
            .SetParent(WhenSkipVote)
            .SetGameMode(CustomGameMode.Standard);
        WhenSkipVoteIgnoreEmergency = BooleanOptionItem.Create(60734, "WhenSkipVoteIgnoreEmergency", false, TabGroup.GameSettings, false)
            .SetParent(WhenSkipVote)
            .SetGameMode(CustomGameMode.Standard);
        WhenNonVote = StringOptionItem.Create(60735, "WhenNonVote", voteModes, 0, TabGroup.GameSettings, false)
            .SetParent(VoteMode)
            .SetGameMode(CustomGameMode.Standard);
        WhenTie = StringOptionItem.Create(60745, "WhenTie", tieModes, 0, TabGroup.GameSettings, false)
            .SetParent(VoteMode)
            .SetGameMode(CustomGameMode.Standard);
        // 其它设定
        TextOptionItem.Create(10000031, "MenuTitle.Other", TabGroup.GameSettings)
            .HideInFFA()
            .SetColor(new Color32(193, 255, 209, byte.MaxValue));
        // 梯子摔死
        LadderDeath = BooleanOptionItem.Create(60760, "LadderDeath", false, TabGroup.GameSettings, false)
            .SetColor(new Color32(193, 255, 209, byte.MaxValue))
            .HideInFFA();
        LadderDeathChance = StringOptionItem.Create(60761, "LadderDeathChance", EnumHelper.GetAllNames<SpawnChance>()[1..], 0, TabGroup.GameSettings, false)
            .SetParent(LadderDeath);

        // 修正首刀时间
        FixFirstKillCooldown = BooleanOptionItem.Create(60770, "FixFirstKillCooldown", true, TabGroup.GameSettings, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(193, 255, 209, byte.MaxValue));
        FixKillCooldownValue = FloatOptionItem.Create(60771, "FixKillCooldownValue", new(0f, 180f, 2.5f), 15f, TabGroup.GameSettings, false)
            .SetValueFormat(OptionFormat.Seconds)
            .SetParent(FixFirstKillCooldown);
        // 首刀保护
        ShieldPersonDiedFirst = BooleanOptionItem.Create(60780, "ShieldPersonDiedFirst", false, TabGroup.GameSettings, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(193, 255, 209, byte.MaxValue));

        // 杀戮闪烁持续
        KillFlashDuration = FloatOptionItem.Create(60790, "KillFlashDuration", new(0.1f, 0.45f, 0.05f), 0.3f, TabGroup.GameSettings, false)
            .SetColor(new Color32(193, 255, 209, byte.MaxValue))
            .SetValueFormat(OptionFormat.Seconds)
            .SetGameMode(CustomGameMode.Standard);
        // 幽灵相关设定
        TextOptionItem.Create(10000032, "MenuTitle.Ghost", TabGroup.GameSettings)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(217, 218, 255, byte.MaxValue));
        // 幽灵设置
        GhostIgnoreTasks = BooleanOptionItem.Create(60800, "GhostIgnoreTasks", false, TabGroup.GameSettings, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetHeader(true)
            .SetColor(new Color32(217, 218, 255, byte.MaxValue));
        GhostCanSeeOtherRoles = BooleanOptionItem.Create(60810, "GhostCanSeeOtherRoles", true, TabGroup.GameSettings, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(217, 218, 255, byte.MaxValue));
        GhostCanSeeOtherVotes = BooleanOptionItem.Create(60820, "GhostCanSeeOtherVotes", true, TabGroup.GameSettings, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(217, 218, 255, byte.MaxValue));
        GhostCanSeeDeathReason = BooleanOptionItem.Create(60830, "GhostCanSeeDeathReason", true, TabGroup.GameSettings, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(217, 218, 255, byte.MaxValue));
        ConvertedCanBecomeGhost = BooleanOptionItem.Create(60840, "ConvertedCanBeGhostRole", false, TabGroup.GameSettings, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetColor(new Color32(217, 218, 255, byte.MaxValue));
        MaxImpGhost = IntegerOptionItem.Create(60850, "MaxImpGhostRole", new(0, 15, 1), 15, TabGroup.GameSettings, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetValueFormat(OptionFormat.Times)
            .SetColor(new Color32(217, 218, 255, byte.MaxValue));
        MaxCrewGhost = IntegerOptionItem.Create(60860, "MaxCrewGhostRole", new(0, 15, 1), 15, TabGroup.GameSettings, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetValueFormat(OptionFormat.Times)
            .SetColor(new Color32(217, 218, 255, byte.MaxValue));
        DefaultAngelCooldown = FloatOptionItem.Create(60870, "DefaultAngelCooldown", new(2.5f, 120f, 2.5f), 35f, TabGroup.GameSettings, false)
            .SetGameMode(CustomGameMode.Standard)
            .SetValueFormat(OptionFormat.Seconds)
            .SetColor(new Color32(217, 218, 255, byte.MaxValue));
        #endregion


        // End Load Settings
        OptionSaver.Load();
        IsLoaded = true;
    }

    public static void SetupRoleOptions(int id, TabGroup tab, CustomRoles role, CustomGameMode customGameMode = CustomGameMode.Standard, bool zeroOne = false)
    {
        var spawnOption = StringOptionItem.Create(id, role.ToString(), zeroOne ? EnumHelper.GetAllNames<RatesZeroOne>() : EnumHelper.GetAllNames<SpawnChance>(), 0, tab, false).SetColor(Utils.GetRoleColor(role))
            .SetHeader(true)
            .SetGameMode(customGameMode) as StringOptionItem;
        
        var countOption = IntegerOptionItem.Create(id + 1, "Maximum", new(1, 15, 1), 1, tab, false)
        .SetParent(spawnOption)
            .SetValueFormat(OptionFormat.Players)
            .SetGameMode(customGameMode);

        if (role.IsGhostRole())
        {
            CustomGhostRoleCounts.Add(role, countOption);
        }

        CustomRoleSpawnChances.Add(role, spawnOption);
        CustomRoleCounts.Add(role, countOption);
    }
    private static void SetupLoversRoleOptionsToggle(int id, CustomGameMode customGameMode = CustomGameMode.Standard)
    {
        var role = CustomRoles.Lovers;
        var spawnOption = StringOptionItem.Create(id, role.ToString(), EnumHelper.GetAllNames<RatesZeroOne>(), 0, TabGroup.Addons, false).SetColor(Utils.GetRoleColor(role))
            .SetHeader(true)
            .SetGameMode(customGameMode) as StringOptionItem;

        LoverSpawnChances = IntegerOptionItem.Create(id + 2, "LoverSpawnChances", new(0, 100, 5), 50, TabGroup.Addons, false)
        .SetParent(spawnOption)
            .SetValueFormat(OptionFormat.Percent)
            .SetGameMode(customGameMode);

        LoverKnowRoles = BooleanOptionItem.Create(id + 4, "LoverKnowRoles", true, TabGroup.Addons, false)
        .SetParent(spawnOption)
            .SetGameMode(customGameMode);

        LoverSuicide = BooleanOptionItem.Create(id + 3, "LoverSuicide", true, TabGroup.Addons, false)
        .SetParent(spawnOption)
            .SetGameMode(customGameMode);

        ImpCanBeInLove = BooleanOptionItem.Create(id + 5, "ImpCanBeInLove", true, TabGroup.Addons, false)
        .SetParent(spawnOption)
            .SetGameMode(customGameMode);

        CrewCanBeInLove = BooleanOptionItem.Create(id + 6, "CrewCanBeInLove", true, TabGroup.Addons, false)
        .SetParent(spawnOption)
            .SetGameMode(customGameMode);

        NeutralCanBeInLove = BooleanOptionItem.Create(id + 7, "NeutralCanBeInLove", true, TabGroup.Addons, false)
        .SetParent(spawnOption)
            .SetGameMode(customGameMode);


        var countOption = IntegerOptionItem.Create(id + 1, "NumberOfLovers", new(2, 2, 1), 2, TabGroup.Addons, false)
            .SetParent(spawnOption)
            .SetHidden(true)
            .SetGameMode(customGameMode);

        CustomRoleSpawnChances.Add(role, spawnOption);
        CustomRoleCounts.Add(role, countOption);
    }

    public static void SetupAdtRoleOptions(int id, CustomRoles role, CustomGameMode customGameMode = CustomGameMode.Standard, bool canSetNum = false, TabGroup tab = TabGroup.Addons, bool canSetChance = true)
    {
        var spawnOption = StringOptionItem.Create(id, role.ToString(), EnumHelper.GetAllNames<RatesZeroOne>(), 0, tab, false).SetColor(Utils.GetRoleColor(role))
            .SetHeader(true)
            .SetGameMode(customGameMode) as StringOptionItem;

        var countOption = IntegerOptionItem.Create(id + 1, "Maximum", new(1, canSetNum ? 10 : 1, 1), 1, tab, false)
        .SetParent(spawnOption)
            .SetValueFormat(OptionFormat.Players)
            .SetHidden(!canSetNum)
            .SetGameMode(customGameMode);

        var spawnRateOption = IntegerOptionItem.Create(id + 2, "AdditionRolesSpawnRate", new(0, 100, 5), canSetChance ? 65 : 100, tab, false)
        .SetParent(spawnOption)
            .SetValueFormat(OptionFormat.Percent)
            .SetHidden(!canSetChance)
            .SetGameMode(customGameMode) as IntegerOptionItem;

        CustomAdtRoleSpawnRate.Add(role, spawnRateOption);
        CustomRoleSpawnChances.Add(role, spawnOption);
        CustomRoleCounts.Add(role, countOption);
    }

    public static void SetupSingleRoleOptions(int id, TabGroup tab, CustomRoles role, int count = 1, CustomGameMode customGameMode = CustomGameMode.Standard, bool zeroOne = false)
    {
        var spawnOption = StringOptionItem.Create(id, role.ToString(), zeroOne ? EnumHelper.GetAllNames<RatesZeroOne>() : EnumHelper.GetAllNames<SpawnChance>(), 0, tab, false).SetColor(Utils.GetRoleColor(role))
            .SetHeader(true)
            .SetGameMode(customGameMode) as StringOptionItem;

        var countOption = IntegerOptionItem.Create(id + 1, "Maximum", new(count, count, count), count, tab, false)
            .SetParent(spawnOption)
            .SetHidden(true)
            .SetGameMode(customGameMode);

        if (role.IsGhostRole())
        {
            CustomGhostRoleCounts.Add(role, countOption);
        }

        CustomRoleSpawnChances.Add(role, spawnOption);
        CustomRoleCounts.Add(role, countOption);
    }
    public class OverrideTasksData
    {
        public static Dictionary<CustomRoles, OverrideTasksData> AllData = [];
        public CustomRoles Role { get; private set; }
        public int IdStart { get; private set; }
        public OptionItem doOverride;
        public OptionItem assignCommonTasks;
        public OptionItem numLongTasks;
        public OptionItem numShortTasks;

        public OverrideTasksData(int idStart, TabGroup tab, CustomRoles role)
        {
            IdStart = idStart;
            Role = role;
            Dictionary<string, string> replacementDic = new() { { "%role%", Utils.ColorString(Utils.GetRoleColor(role), Utils.GetRoleName(role)) } };
            doOverride = BooleanOptionItem.Create(idStart++, "doOverride", false, tab, false)
                .SetParent(CustomRoleSpawnChances[role])
                .SetValueFormat(OptionFormat.None);
            doOverride.ReplacementDictionary = replacementDic;
            assignCommonTasks = BooleanOptionItem.Create(idStart++, "assignCommonTasks", true, tab, false)
                .SetParent(doOverride)
                .SetValueFormat(OptionFormat.None);
            assignCommonTasks.ReplacementDictionary = replacementDic;
            numLongTasks = IntegerOptionItem.Create(idStart++, "roleLongTasksNum", new(0, 99, 1), 3, tab, false)
                .SetParent(doOverride)
                .SetValueFormat(OptionFormat.Pieces);
            numLongTasks.ReplacementDictionary = replacementDic;
            numShortTasks = IntegerOptionItem.Create(idStart++, "roleShortTasksNum", new(0, 99, 1), 3, tab, false)
                .SetParent(doOverride)
                .SetValueFormat(OptionFormat.Pieces);
            numShortTasks.ReplacementDictionary = replacementDic;

            if (!AllData.ContainsKey(role)) AllData.Add(role, this);
            else Logger.Warn("重複したCustomRolesを対象とするOverrideTasksDataが作成されました", "OverrideTasksData");
        }
        public static OverrideTasksData Create(int idStart, TabGroup tab, CustomRoles role)
        {
            return new OverrideTasksData(idStart, tab, role);
        }
    }
}
