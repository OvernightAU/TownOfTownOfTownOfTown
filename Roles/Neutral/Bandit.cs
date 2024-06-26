﻿using AmongUs.GameOptions;
using Hazel;
using TOHE.Roles.AddOns.Common;
using UnityEngine;
using static TOHE.Options;

namespace TOHE.Roles.Neutral;

internal class Bandit : RoleBase
{
    //===========================SETUP================================\\
    private const int Id = 16000;
    private static readonly HashSet<byte> playerIdList = [];
    public static bool HasEnabled => playerIdList.Any();
    public override bool IsEnable => HasEnabled;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    //==================================================================\\

    private static OptionItem KillCooldownOpt;
    private static OptionItem StealCooldown;
    private static OptionItem MaxSteals;
    private static OptionItem StealMode;
    private static OptionItem CanStealBetrayalAddon;
    private static OptionItem CanStealImpOnlyAddon;
    private static OptionItem CanUsesSabotage;
    private static OptionItem CanVent;

    private static readonly Dictionary<byte, float> killCooldown = [];
    private static readonly Dictionary<byte, int> TotalSteals = [];
    private static readonly Dictionary<byte, Dictionary<byte, CustomRoles>> Targets = [];

    private enum BanditStealModeOpt
    {
        BanditStealMode_OnMeeting,
        BanditStealMode_Instantly
    }

    public static void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Bandit);
        MaxSteals = IntegerOptionItem.Create(Id + 10, "BanditMaxSteals", new(1, 20, 1), 6, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Bandit]);
        KillCooldownOpt = FloatOptionItem.Create(Id + 11, "KillCooldown", new(0f, 180f, 2.5f), 20f, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Bandit])
            .SetValueFormat(OptionFormat.Seconds);
        StealCooldown = FloatOptionItem.Create(Id + 17, "BanditStealCooldown", new(0f, 180f, 2.5f), 10f, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Bandit])
            .SetValueFormat(OptionFormat.Seconds);
        StealMode = StringOptionItem.Create(Id + 12, "BanditStealMode", EnumHelper.GetAllNames<BanditStealModeOpt>(), 0, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Bandit]);
        CanStealBetrayalAddon = BooleanOptionItem.Create(Id + 13, "BanditCanStealBetrayalAddon", true, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Bandit]);
        CanStealImpOnlyAddon = BooleanOptionItem.Create(Id + 14, "BanditCanStealImpOnlyAddon", true, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Bandit]);
        CanUsesSabotage = BooleanOptionItem.Create(Id + 15, "CanUseSabotage", true, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Bandit]);
        CanVent = BooleanOptionItem.Create(Id + 16, "CanVent", true, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Bandit]);
    }

    public override void Init()
    {
        playerIdList.Clear();
        Targets.Clear();
        TotalSteals.Clear();
        killCooldown.Clear();
    }

    public override void Add(byte playerId)
    {
        playerIdList.Add(playerId);
        TotalSteals.Add(playerId, 0);
        Targets[playerId] = [];

        var pc = Utils.GetPlayerById(playerId);
        pc?.AddDoubleTrigger();

        if (!AmongUsClient.Instance.AmHost) return;
        if (!Main.ResetCamPlayerList.Contains(playerId))
            Main.ResetCamPlayerList.Add(playerId);
    }

    private static void SendRPC(byte playerId)
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SyncRoleSkill, SendOption.Reliable, -1);
        writer.WritePacked((int)CustomRoles.Bandit);
        writer.Write(playerId);
        writer.Write(TotalSteals[playerId]);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }

    public override void ReceiveRPC(MessageReader reader, PlayerControl NaN)
    {
        byte PlayerId = reader.ReadByte();
        int Limit = reader.ReadInt32();
        if (TotalSteals.ContainsKey(PlayerId))
            TotalSteals[PlayerId] = Limit;
        else
            TotalSteals.Add(PlayerId, 0);
    }

    public override void SetKillCooldown(byte id)
    {
        if (!killCooldown.ContainsKey(id)) killCooldown[id] = KillCooldownOpt.GetFloat();
        Main.AllPlayerKillCooldown[id] = killCooldown[id];
    }
    public override void ApplyGameOptions(IGameOptions opt, byte id) => opt.SetVision(false);
    public override bool CanUseImpostorVentButton(PlayerControl pc) => CanVent.GetBool();
    public override bool CanUseSabotage(PlayerControl pc) => CanUsesSabotage.GetBool();
    public override bool CanUseKillButton(PlayerControl pc) => true;

    private static CustomRoles? SelectRandomAddon(PlayerControl Target)
    {
        if (!AmongUsClient.Instance.AmHost) return null;
        var AllSubRoles = Main.PlayerStates[Target.PlayerId].SubRoles.ToList();
        for (int i = AllSubRoles.Count - 1; i >= 0; i--)
        {
            var role = AllSubRoles[i];
            if (role == CustomRoles.Cleansed || // making Bandit unable to steal Cleansed for obvious reasons. Although it can still be cleansed by cleanser.
                role == CustomRoles.LastImpostor ||
                role == CustomRoles.Lovers || // Causes issues involving Lovers Suicide
                (role.IsImpOnlyAddon() && !CanStealImpOnlyAddon.GetBool()) ||
                (role == CustomRoles.Nimble && CanVent.GetBool()) ||
                ((role.IsBetrayalAddon() || role is CustomRoles.Lovers) && !CanStealBetrayalAddon.GetBool()))
            { 
                    Logger.Info($"Removed {role} from list of stealable addons", "Bandit");
                    AllSubRoles.Remove(role);
            }
        }

        if (AllSubRoles.Count == 0)
        {
            Logger.Info("No stealable addons found on the target.", "Bandit");
            return null;
        }
        var rand = IRandom.Instance;
        var addon = AllSubRoles[rand.Next(0, AllSubRoles.Count)];
        return addon;
    }

    public static void StealAddon(PlayerControl killer, PlayerControl target, CustomRoles? SelectedAddOn)
    {
        if (StealMode.GetValue() == 1)
        {
            ExtendedPlayerControl.AddInSwitchAddons(target, killer, CustomRoles.NotAssigned, SelectedAddOn);
          
            Main.PlayerStates[target.PlayerId].RemoveSubRole((CustomRoles)SelectedAddOn);
            if (SelectedAddOn == CustomRoles.Aware) Aware.AwareInteracted.Remove(target.PlayerId);
            Logger.Info($"Successfully removed {SelectedAddOn} addon from {target.GetNameWithRole()}", "Bandit");

            if (SelectedAddOn == CustomRoles.Aware && !Aware.AwareInteracted.ContainsKey(target.PlayerId)) Aware.AwareInteracted[target.PlayerId] = [];
            killer.RpcSetCustomRole((CustomRoles)SelectedAddOn);
            Logger.Info($"Successfully Added {SelectedAddOn} addon to {killer.GetNameWithRole()}", "Bandit");
        }
        else
        {
          
            ExtendedPlayerControl.AddInSwitchAddons(target, killer, CustomRoles.NotAssigned, SelectedAddOn);
          
            Targets[killer.PlayerId][target.PlayerId] = (CustomRoles)SelectedAddOn;
            Logger.Info($"{killer.GetNameWithRole()} will steal {SelectedAddOn} addon from {target.GetNameWithRole()} after meeting starts", "Bandit");
        }
        TotalSteals[killer.PlayerId]++;
        SendRPC(killer.PlayerId);

        Utils.NotifyRoles(SpecifySeer: killer, SpecifyTarget: target, ForceLoop: true);
        Utils.NotifyRoles(SpecifySeer: target, SpecifyTarget: killer, ForceLoop: true);

        killer.ResetKillCooldown();
        killer.SetKillCooldown();

        if (!DisableShieldAnimations.GetBool())
            killer.RpcGuardAndKill(target);

        return;
    }

    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        bool flag = false;
        if (!target.HasSubRole() || target.Is(CustomRoles.Stubborn)) flag = true;

        var SelectedAddOn = SelectRandomAddon(target);
        if (SelectedAddOn == null || flag) // no stealable addons found on the target.
        {
            killer.Notify(Translator.GetString("Bandit_NoStealableAddons"));
            return true;
        }
        if (TotalSteals[killer.PlayerId] >= MaxSteals.GetInt())
        {
            Logger.Info("Max steals reached killing the player", "Bandit");
            TotalSteals[killer.PlayerId] = MaxSteals.GetInt();
            return true;
        }

        if (killer.CheckDoubleTrigger(target, () => { StealAddon(killer, target, SelectedAddOn); }))
        {
            // Double click
            killCooldown[killer.PlayerId] = KillCooldownOpt.GetFloat();
            return true;
        }
        else
        {
            // Single click
            killCooldown[killer.PlayerId] = StealCooldown.GetFloat();
            return false;
        }
    }

    public override void OnReportDeadBody(PlayerControl reportash, PlayerControl panagustava)
    {
        if (StealMode.GetValue() == 1) return;
        foreach (var kvp1 in Targets)
        {
            byte banditId = kvp1.Key;
            var banditpc = Utils.GetPlayerById(banditId);
            if (banditpc == null || !banditpc.IsAlive()) continue;
            var innerDictionary = kvp1.Value;
            foreach (var kvp2 in innerDictionary)
            {
                byte targetId = kvp2.Key;
                var target = Utils.GetPlayerById(banditId);
                if (target == null) continue;
                CustomRoles role = kvp2.Value;
                Main.PlayerStates[targetId].RemoveSubRole(role);
                if (role == CustomRoles.Aware) Aware.AwareInteracted.Remove(target.PlayerId);
                Logger.Info($"Successfully removed {role} addon from {target.GetNameWithRole()}", "Bandit");

                if (role == CustomRoles.Aware && !Aware.AwareInteracted.ContainsKey(target.PlayerId)) Aware.AwareInteracted[target.PlayerId] = [];
                banditpc.RpcSetCustomRole(role);
                Logger.Info($"Successfully Added {role} addon to {banditpc.GetNameWithRole()}", "Bandit");
            }
            Targets[banditId].Clear();
        }
    }
    public override string GetProgressText(byte playerId, bool comms) => Utils.ColorString(TotalSteals[playerId] < MaxSteals.GetInt() ? Utils.GetRoleColor(CustomRoles.Bandit).ShadeColor(0.25f) : Color.gray, TotalSteals.TryGetValue(playerId, out var stealLimit) ? $"({MaxSteals.GetInt() - stealLimit})" : "Invalid");
}
