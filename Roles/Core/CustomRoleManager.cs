﻿using AmongUs.GameOptions;
using System;
using System.Text;
using TOHE.Roles.AddOns.Common;
using TOHE.Roles.AddOns.Crewmate;
using TOHE.Roles.AddOns.Impostor;
using TOHE.Roles.Crewmate;
using TOHE.Roles.Impostor;
using TOHE.Roles.Neutral;

namespace TOHE.Roles.Core;

public static class CustomRoleManager
{
    public static readonly Dictionary<CustomRoles, RoleBase> RoleClass = [];
    public static RoleBase GetStaticRoleClass(this CustomRoles role) => RoleClass.TryGetValue(role, out var roleClass) & roleClass != null ? roleClass : new VanillaRole(); 
    public static List<RoleBase> AllEnabledRoles => RoleClass.Values.Where(x => x.IsEnable).ToList();
    public static bool HasEnabled(this CustomRoles role) => role.GetStaticRoleClass().IsEnable;
    public static RoleBase GetRoleClass(this PlayerControl player) => GetRoleClassById(player.PlayerId);
    public static RoleBase GetRoleClassById(this byte playerId) => Main.PlayerStates.TryGetValue(playerId, out var statePlayer) && statePlayer != null ? statePlayer.RoleClass : new VanillaRole();

    public static RoleBase CreateRoleClass(this CustomRoles role) 
    {
        return (RoleBase)Activator.CreateInstance(role.GetStaticRoleClass().GetType()); // Converts this.RoleBase back to its type and creates an unique one.
    }

    /// <summary>
    /// If the role protect others players
    /// </summary>
    public static bool OnCheckMurderAsTargetOnOthers(PlayerControl killer, PlayerControl target)
    {
        // return true when need to cancel the kill target
        // "Any()" defines a function that returns true, and converts to false to cancel the kill
        return !AllEnabledRoles.Any(RoleClass => RoleClass.CheckMurderOnOthersTarget(killer, target) == true);
    }

    /// <summary>
    /// Builds Modified GameOptions
    /// </summary>
    public static void BuildCustomGameOptions(this PlayerControl player, ref IGameOptions opt, CustomRoles role)
    {
        if (player.IsAnySubRole(x => x is CustomRoles.EvilSpirit))
        {
            AURoleOptions.GuardianAngelCooldown = Spiritcaller.SpiritAbilityCooldown.GetFloat();
        }

        player.GetRoleClass()?.ApplyGameOptions(opt, player.PlayerId);

        switch (role)
        {
            case CustomRoles.ShapeshifterTOHE:
                AURoleOptions.ShapeshifterCooldown = Options.ShapeshiftCD.GetFloat();
                AURoleOptions.ShapeshifterDuration = Options.ShapeshiftDur.GetFloat();
                break;
            case CustomRoles.ScientistTOHE:
                AURoleOptions.ScientistCooldown = Options.ScientistCD.GetFloat();
                AURoleOptions.ScientistBatteryCharge = Options.ScientistDur.GetFloat();
                break;
            case CustomRoles.EngineerTOHE:
                AURoleOptions.EngineerCooldown = 0f;
                AURoleOptions.EngineerInVentMaxTime = 0f;
                break;
            default:
                opt.SetVision(false);
                break;
        }

        if (Grenadier.HasEnabled) Grenadier.ApplyGameOptionsForOthers(opt, player);
        if (Dazzler.HasEnabled) Dazzler.SetDazzled(player, opt);
        if (Deathpact.HasEnabled) Deathpact.SetDeathpactVision(player, opt);
        if (Spiritcaller.HasEnabled) Spiritcaller.ReduceVision(opt, player);
        if (Pitfall.HasEnabled) Pitfall.SetPitfallTrapVision(opt, player);

        // Add-ons
        if (Bewilder.IsEnable) Bewilder.ApplyGameOptions(opt, player);
        if (Ghoul.IsEnable) Ghoul.ApplyGameOptions(player);

        var playerSubRoles = player.GetCustomSubRoles();

        if (playerSubRoles.Any())
            foreach (var subRole in playerSubRoles.ToArray())
            {
                switch (subRole)
                {
                    case CustomRoles.Watcher:
                        Watcher.RevealVotes(opt);
                        break;
                    case CustomRoles.Flash:
                        Flash.SetSpeed(player.PlayerId, false);
                        break;
                    case CustomRoles.Torch:
                        Torch.ApplyGameOptions(opt);
                        break;
                    case CustomRoles.Tired:
                        Tired.ApplyGameOptions(opt, player);
                        break;
                    case CustomRoles.Bewilder:
                        Bewilder.ApplyVisionOptions(opt);
                        break;
                    case CustomRoles.Reach:
                        Reach.ApplyGameOptions(opt);
                        break;
                    case CustomRoles.Madmate:
                        Madmate.ApplyGameOptions(opt);
                        break;
                    case CustomRoles.Mare:
                        Mare.ApplyGameOptions(player.PlayerId);
                        break;
                }
            }
    }

    /// <summary>
    /// Check Murder as Killer in target
    /// </summary>
    public static bool OnCheckMurder(PlayerControl killer, PlayerControl target)
    {
        if (killer == target) return true;

        var killerRoleClass = killer.GetRoleClass();
        var killerSubRoles = killer.GetCustomSubRoles();

        // Forced check
        if (killerRoleClass.ForcedCheckMurderAsKiller(killer, target) == false)
        {
            Logger.Info("Cancels because for killer no need kill target", "ForcedCheckMurderAsKiller");
            return false;
        }

        // Check in target
        if (killer.RpcCheckAndMurder(target, true) == false)
        {
            Logger.Info("Cancels because target cancel kill", "OnCheckMurder.RpcCheckAndMurder");
            return false;
        }

        if (killerSubRoles.Any())
            foreach (var killerSubRole in killerSubRoles.ToArray())
            {
                switch (killerSubRole)
                {
                    case CustomRoles.Madmate when target.Is(CustomRoleTypes.Impostor) && !Madmate.MadmateCanKillImp.GetBool():
                    case CustomRoles.Infected when target.Is(CustomRoles.Infected) && !Infectious.TargetKnowOtherTargets:
                    case CustomRoles.Infected when target.Is(CustomRoles.Infectious):
                        return false;

                    case CustomRoles.Mare:
                        if (Mare.IsLightsOut)
                            return false;
                        break;

                    case CustomRoles.Unlucky:
                        Unlucky.SuicideRand(killer);
                        if (Unlucky.UnluckCheck[killer.PlayerId]) return false;
                        break;

                    case CustomRoles.Tired:
                        Tired.AfterActionTasks(killer);
                        break;

                    case CustomRoles.Clumsy:
                        if (!Clumsy.OnCheckMurder(killer))
                            return false;
                        break;

                    case CustomRoles.Swift:
                        if (!Swift.OnCheckMurder(killer, target))
                            return false;
                        break;
                }
            }

        // Check murder as killer
        if (!killerRoleClass.OnCheckMurderAsKiller(killer, target))
        {
            Logger.Info("Cancels because for killer no need kill target", "OnCheckMurderAsKiller");
            return false;
        }

        return true;
    }
    /// <summary>
    /// Tasks after killer murder target
    /// </summary>
    public static void OnMurderPlayer(PlayerControl killer, PlayerControl target, bool inMeeting)
    {
        // ############-INFO-##############
        // When using this code, keep in mind that killer and target can be equal (Suicide)
        // And the player can also die during the Meeting
        // ################################

        var killerRoleClass = killer.GetRoleClass();
        var targetRoleClass = target.GetRoleClass();

        var killerSubRoles = killer.GetCustomSubRoles();
        var targetSubRoles = target.GetCustomSubRoles();

        // Check suicide
        var isSuicide = killer.PlayerId == target.PlayerId;

        // target was murder by killer
        targetRoleClass.OnMurderPlayerAsTarget(killer, target, inMeeting, isSuicide);

        // Check target add-ons
        if (targetSubRoles.Any())
            foreach (var subRole in targetSubRoles.ToArray())
            {
                switch (subRole)
                {
                    case CustomRoles.Cyber:
                        Cyber.AfterCyberDeadTask(target, inMeeting);
                        break;

                    case CustomRoles.Bait when !inMeeting && !isSuicide:
                        Bait.BaitAfterDeathTasks(killer, target);
                        break;

                    case CustomRoles.Trapper when !inMeeting && !isSuicide && !killer.Is(CustomRoles.KillingMachine):
                        killer.TrapperKilled(target);
                        break;

                    case CustomRoles.Avanger when !inMeeting && !isSuicide:
                        Avanger.OnMurderPlayer(target);
                        break;

                    case CustomRoles.Burst when killer.IsAlive() && !inMeeting && !isSuicide && !killer.Is(CustomRoles.KillingMachine):
                        Burst.AfterBurstDeadTasks(killer, target);
                        break;

                    case CustomRoles.Oiiai when !isSuicide:
                        Oiiai.OnMurderPlayer(killer, target);
                        break;

                    case CustomRoles.EvilSpirit when !inMeeting && !isSuicide:
                        target.RpcSetRole(RoleTypes.GuardianAngel);
                        break;

                }
            }

        // Killer murder target
        killerRoleClass.OnMurderPlayerAsKiller(killer, target, inMeeting, isSuicide);

        // Check killer add-ons
        if (killerSubRoles.Any())
            foreach (var subRole in killerSubRoles.ToArray())
            {
                switch (subRole)
                {
                    case CustomRoles.TicketsStealer when !inMeeting && !isSuicide:
                        killer.Notify(string.Format(Translator.GetString("TicketsStealerGetTicket"), ((Main.AllPlayerControls.Count(x => x.GetRealKiller()?.PlayerId == killer.PlayerId) + 1) * Stealer.TicketsPerKill.GetFloat()).ToString("0.0#####")));
                        break;

                    case CustomRoles.Tricky:
                        Tricky.AfterPlayerDeathTasks(target);
                        break;
                }
            }

        // Check dead body for others roles
        CheckDeadBody(killer, target, inMeeting);

        // Check Lovers Suicide
        FixedUpdateInNormalGamePatch.LoversSuicide(target.PlayerId, inMeeting);
    }
    
    /// <summary>
    /// Check if this task is marked by a role and do something.
    /// </summary>
    public static void OthersCompleteThisTask(PlayerControl player, PlayerTask task)
        => Main.PlayerStates.Values.ToArray().Do(PlrState => PlrState.RoleClass.OnOthersTaskComplete(player, task));
    

    public static HashSet<Action<PlayerControl, PlayerControl, bool>> CheckDeadBodyOthers = [];
    /// <summary>
    /// If the role need check a present dead body
    /// </summary>
    public static void CheckDeadBody(PlayerControl killer, PlayerControl deadBody, bool inMeeting)
    {
        if (!CheckDeadBodyOthers.Any()) return;
        //Execute other viewpoint processing if any
        foreach (var checkDeadBodyOthers in CheckDeadBodyOthers.ToArray())
        {
            checkDeadBodyOthers(killer, deadBody, inMeeting);
        }
    }

    public static HashSet<Action<PlayerControl>> OnFixedUpdateOthers = [];
    /// <summary>
    /// Function always called in a task turn
    /// For interfering with other roles
    /// Registered with OnFixedUpdateOthers+= at initialization
    /// </summary>
    public static void OnFixedUpdate(PlayerControl player)
    {
        player.GetRoleClass()?.OnFixedUpdate(player);

        if (!OnFixedUpdateOthers.Any()) return;
        //Execute other viewpoint processing if any
        foreach (var onFixedUpdate in OnFixedUpdateOthers.ToArray())
        {
            onFixedUpdate(player);
        }
    }
    public static HashSet<Action<PlayerControl>> OnFixedUpdateLowLoadOthers = [];
    public static void OnFixedUpdateLowLoad(PlayerControl player)
    {
        player.GetRoleClass()?.OnFixedUpdateLowLoad(player);

        if (!OnFixedUpdateLowLoadOthers.Any()) return;
        //Execute other viewpoint processing if any
        foreach (var onFixedUpdateLowLoad in OnFixedUpdateLowLoadOthers.ToArray())
        {
            onFixedUpdateLowLoad(player);
        }
    }

    /// <summary>
    /// When others players on entered to vent
    /// </summary>
    public static bool OthersCoEnterVent(PlayerPhysics physics, int ventId)
    {
        return AllEnabledRoles.Any(RoleClass => RoleClass.OnCoEnterVentOthers(physics, ventId));
    }

    public static HashSet<Func<PlayerControl, PlayerControl, bool, string>> MarkOthers = [];
    public static HashSet<Func<PlayerControl, PlayerControl, bool, bool, string>> LowerOthers = [];
    public static HashSet<Func<PlayerControl, PlayerControl, bool, string>> SuffixOthers = [];

    public static string GetMarkOthers(PlayerControl seer, PlayerControl seen = null, bool isForMeeting = false)
    {
        if (!MarkOthers.Any()) return string.Empty;

        var sb = new StringBuilder(100);
        foreach (var marker in MarkOthers)
        {
            sb.Append(marker(seer, seen, isForMeeting));
        }
        return sb.ToString();
    }

    public static string GetLowerTextOthers(PlayerControl seer, PlayerControl seen = null, bool isForMeeting = false, bool isForHud = false)
    {
        if (!LowerOthers.Any()) return string.Empty;

        var sb = new StringBuilder(100);
        foreach (var lower in LowerOthers)
        {
            sb.Append(lower(seer, seen, isForMeeting, isForHud));
        }
        return sb.ToString();
    }

    public static string GetSuffixOthers(PlayerControl seer, PlayerControl seen = null, bool isForMeeting = false)
    {
        if (!SuffixOthers.Any()) return string.Empty;

        var sb = new StringBuilder(100);
        foreach (var suffix in SuffixOthers)
        {
            sb.Append(suffix(seer, seen, isForMeeting));
        }
        return sb.ToString();
    }

    public static void Initialize()
    {
        MarkOthers.Clear();
        LowerOthers.Clear();
        SuffixOthers.Clear();
        OnFixedUpdateOthers.Clear();
        OnFixedUpdateLowLoadOthers.Clear();
        CheckDeadBodyOthers.Clear();
    }
}
