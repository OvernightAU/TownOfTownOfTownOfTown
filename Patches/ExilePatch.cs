﻿using AmongUs.Data;
using TOHE.Roles.Core;
using TOHE.Roles.Neutral;

namespace TOHE;

class ExileControllerWrapUpPatch
{
    public static GameData.PlayerInfo AntiBlackout_LastExiled;
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
    class BaseExileControllerPatch
    {
        public static void Postfix(ExileController __instance)
        {
            try
            {
                WrapUpPostfix(__instance.exiled);
            }
            finally
            {
                WrapUpFinalizer(__instance.exiled);
            }
        }
    }

    [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
    class AirshipExileControllerPatch
    {
        public static void Postfix(AirshipExileController __instance)
        {
            try
            {
                WrapUpPostfix(__instance.exiled);
            }
            finally
            {
                WrapUpFinalizer(__instance.exiled);
            }
        }
    }
    static void WrapUpPostfix(GameData.PlayerInfo exiled)
    {
        if (AntiBlackout.BlackOutIsActive) exiled = AntiBlackout_LastExiled;

        // Still not springing up in airships
        if (!GameStates.AirshipIsActive)
        {
            foreach (var state in Main.PlayerStates.Values)
            {
                state.HasSpawned = true;
            }
        }

        bool DecidedWinner = false;
        if (!AmongUsClient.Instance.AmHost) return;
        AntiBlackout.RestoreIsDead(doSend: false);
        
        Logger.Info($"{!Collector.CollectorWin(false)}", "!Collector.CollectorWin(false)");
        Logger.Info($"{exiled != null}", "exiled != null");

        if (!Collector.CollectorWin(false) && exiled != null)
        {
            // Reset player cam for exiled desync impostor
            if (Main.ResetCamPlayerList.Contains(exiled.PlayerId))
            {
                exiled.Object?.ResetPlayerCam(1f);
            }

            exiled.IsDead = true;
            Main.PlayerStates[exiled.PlayerId].deathReason = PlayerState.DeathReason.Vote;

            var role = exiled.GetCustomRole();
            var player = Utils.GetPlayerById(exiled.PlayerId);
            var exiledRoleClass = player.GetRoleClass();
           
            var emptyString = string.Empty;
            exiledRoleClass?.CheckExileTarget(exiled, ref DecidedWinner, isMeetingHud: false, name: ref emptyString);

            if (CustomWinnerHolder.WinnerTeam != CustomWinner.Terrorist) Main.PlayerStates[exiled.PlayerId].SetDead();
        }
        
        if (AmongUsClient.Instance.AmHost && Main.IsFixedCooldown)
        {
            Main.RefixCooldownDelay = Options.DefaultKillCooldown - 3f;
        }

        
        foreach (var player in Main.AllPlayerControls)
        {
            player.GetRoleClass()?.OnPlayerExiled(player, exiled);

            // Check Anti BlackOut
            if (player.GetCustomRole().IsImpostor() 
                && !player.IsAlive() // if player is dead impostor
                && AntiBlackout.BlackOutIsActive) // if Anti BlackOut is activated
            {
                player.ResetPlayerCam(1f);
            }

            // Check for remove pet
            player.RpcRemovePet();

            // Reset Kill/Ability cooldown
            player.ResetKillCooldown();
            player.RpcResetAbilityCooldown();
        }

        Main.MeetingIsStarted = false;
        Main.MeetingsPassed++;

        FallFromLadder.Reset();
        Utils.CountAlivePlayers(true);
        Utils.AfterMeetingTasks();
        Utils.SyncAllSettings();
        Utils.NotifyRoles(NoCache: true);

        if (RandomSpawn.IsRandomSpawn() || Options.CurrentGameMode == CustomGameMode.FFA)
        {
            _ = new LateTask(() =>
            {
                RandomSpawn.SpawnMap map = Utils.GetActiveMapId() switch
                {
                    0 => new RandomSpawn.SkeldSpawnMap(),
                    1 => new RandomSpawn.MiraHQSpawnMap(),
                    2 => new RandomSpawn.PolusSpawnMap(),
                    3 => new RandomSpawn.DleksSpawnMap(),
                    5 => new RandomSpawn.FungleSpawnMap(),
                    _ => null,
                };
                if (map != null) Main.AllPlayerControls.Do(map.RandomTeleport);

            }, 0.8f, "Random Spawn After Meeting");
        }
    }

    static void WrapUpFinalizer(GameData.PlayerInfo exiled)
    {
        // Even if an exception occurs in WrapUpPostfix, this is the only part that will be executed reliably.
        if (AmongUsClient.Instance.AmHost)
        {
            _ = new LateTask(() =>
            {
                exiled = AntiBlackout_LastExiled;
                AntiBlackout.SendGameData();
                if (AntiBlackout.BlackOutIsActive && // State in which the expulsion target is overwritten (need not be executed if the expulsion target is not overwritten)
                    exiled != null && // exiled is not null
                    exiled.Object != null) //exiled.Object is not null
                {
                    exiled.Object.RpcExileV2();
                }
            }, 0.8f, "Restore IsDead Task");

            _ = new LateTask(() =>
            {
                Main.AfterMeetingDeathPlayers.Do(x =>
                {
                    var player = Utils.GetPlayerById(x.Key);
                    var state = Main.PlayerStates[x.Key];
                    
                    Logger.Info($"{player.GetNameWithRole().RemoveHtmlTags()} died with {x.Value}", "AfterMeetingDeath");

                    state.deathReason = x.Value;
                    state.SetDead();
                    player?.RpcExileV2();

                    if (x.Value == PlayerState.DeathReason.Suicide)
                        player?.SetRealKiller(player, true);

                    // Reset player cam for dead desync impostor
                    if (Main.ResetCamPlayerList.Contains(x.Key))
                    {
                        player?.ResetPlayerCam(1f);
                    }

                    MurderPlayerPatch.AfterPlayerDeathTasks(player, player, false);
                });
                Main.AfterMeetingDeathPlayers.Clear();

            }, 0.8f, "AfterMeetingDeathPlayers Task");
        }
        //This should happen shortly after the Exile Controller wrap up finished for clients
        //For Certain Laggy clients 0.8f delay is still not enough. The finish time can differ.
        //If the delay is too long, it will influence other normal players' view

        GameStates.AlreadyDied |= !Utils.IsAllAlive;
        RemoveDisableDevicesPatch.UpdateDisableDevices();
        SoundManager.Instance.ChangeAmbienceVolume(DataManager.Settings.Audio.AmbienceVolume);
        Logger.Info("Start of Task Phase", "Phase");
    }

    [HarmonyPatch(typeof(PbExileController), nameof(PbExileController.PlayerSpin))]
    class PolusExileHatFixPatch
    {
        public static void Prefix(PbExileController __instance)
        {
            __instance.Player.cosmetics.hat.transform.localPosition = new(-0.2f, 0.6f, 1.1f);
        }
    }
}
