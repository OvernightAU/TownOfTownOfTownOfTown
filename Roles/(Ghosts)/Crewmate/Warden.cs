﻿using AmongUs.GameOptions;
using Hazel;
using UnityEngine;
using static TOHE.Options;
using static TOHE.Translator;

namespace TOHE.Roles._Ghosts_.Crewmate;

internal class Warden : RoleBase
{
    //===========================SETUP================================\\
    private const int Id = 27800;
    private static readonly HashSet<byte> PlayerIds = [];
    public static bool HasEnabled => PlayerIds.Any();
    public override bool IsEnable => HasEnabled;
    public override CustomRoles ThisRoleBase => CustomRoles.GuardianAngel;
    //==================================================================\\

    private static OptionItem AbilityCooldown;
    private static OptionItem IncreaseSpeed;
    private static OptionItem WardenCanAlertNum;

    private static readonly HashSet<byte> IsAffected = [];
    private static readonly Dictionary<byte, int> AbilityCount = [];
    public static void SetupCustomOptions()
    {
        SetupSingleRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Warden);
        AbilityCooldown = FloatOptionItem.Create(Id + 10, "AbilityCooldown", new(0f, 120f, 2.5f), 25f, TabGroup.CrewmateRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Warden])
            .SetValueFormat(OptionFormat.Seconds);
        IncreaseSpeed = FloatOptionItem.Create(Id + 11, "WardenIncreaseSpeed", new(1f, 3f, 0.5f), 1f, TabGroup.CrewmateRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Warden])
            .SetValueFormat(OptionFormat.Times);
        WardenCanAlertNum = IntegerOptionItem.Create(Id + 12, "WardenNotifyLimit", new(1, 20, 1), 2, TabGroup.CrewmateRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Warden])
               .SetValueFormat(OptionFormat.Players);
    }
    public override void Init()
    {
        IsAffected.Clear();
        AbilityCount.Clear();
    }
    public override void Add(byte PlayerId)
    {
        AbilityCount.TryAdd(PlayerId, WardenCanAlertNum.GetInt());
    }
    private static void SendRPC(byte playerId)
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SyncRoleSkill, SendOption.Reliable, -1);
        writer.WritePacked((int)CustomRoles.Warden);
        writer.Write(playerId);
        writer.Write(AbilityCount[playerId]);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }
    public override void ReceiveRPC(MessageReader reader, PlayerControl NaN)
    {
        byte PlayerId = reader.ReadByte();
        int Limit = reader.ReadInt32();
        AbilityCount[PlayerId] = Limit;
    }
    public override void ApplyGameOptions(IGameOptions opt, byte playerId)
    {
        AURoleOptions.GuardianAngelCooldown = AbilityCooldown.GetFloat();
        AURoleOptions.ProtectionDurationSeconds = 0f;
    }
    public override bool OnCheckProtect(PlayerControl killer, PlayerControl target)
    {
        var getTargetRole = target.GetCustomRole();
        if (AbilityCount[killer.PlayerId] > 0) 
        {
            if (getTargetRole.IsSpeedRole() || target.IsAnySubRole(x => x.IsSpeedRole()) || IsAffected.Contains(target.PlayerId)) goto Notifiers; // Incompactible speed-roles 

            IsAffected.Add(target.PlayerId);
            target.MarkDirtySettings();
            var tmpSpeed = Main.AllPlayerSpeed[target.PlayerId];
            Main.AllPlayerSpeed[target.PlayerId] = Main.AllPlayerSpeed[target.PlayerId] + IncreaseSpeed.GetFloat();


            _ = new LateTask(() =>
            {
                Main.AllPlayerSpeed[target.PlayerId] = Main.AllPlayerSpeed[target.PlayerId] - Main.AllPlayerSpeed[target.PlayerId] + tmpSpeed;
                target.MarkDirtySettings();
                if (IsAffected.Contains(target.PlayerId)) IsAffected.Remove(target.PlayerId);
            }, 2f, "Warden: Set Speed to default");

        Notifiers:
            target.Notify(Utils.ColorString(new Color32(179, 0, 0, byte.MaxValue), GetString("WardenWarn")));
            
            killer.RpcResetAbilityCooldown();
            AbilityCount[killer.PlayerId]--;
            SendRPC(killer.PlayerId);
        }
        return false;
    }

    public static bool CanGuard(byte id) => AbilityCount.TryGetValue(id, out var x) && x > 0;
    public override string GetProgressText(byte playerId, bool cooms) => Utils.ColorString(CanGuard(playerId) ? Utils.GetRoleColor(CustomRoles.Warden).ShadeColor(0.25f) : Color.gray, AbilityCount.TryGetValue(playerId, out var killLimit) ? $"({killLimit})" : "Invalid");


}