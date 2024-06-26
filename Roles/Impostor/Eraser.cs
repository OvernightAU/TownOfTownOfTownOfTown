﻿using AmongUs.GameOptions;
using Hazel;
using TOHE.Roles.Crewmate;
using UnityEngine;
using static TOHE.Translator;

namespace TOHE.Roles.Impostor;

internal class Eraser : RoleBase
{
    //===========================SETUP================================\\
    private const int Id = 24200;
    private static readonly HashSet<byte> playerIdList = [];
    public static bool HasEnabled => playerIdList.Any();
    public override bool IsEnable => HasEnabled;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    //==================================================================\\

    private static OptionItem EraseLimitOpt;
    public static OptionItem HideVoteOpt;

    private static readonly HashSet<byte> didVote = [];
    private static readonly HashSet<byte> PlayerToErase = [];
    private static readonly Dictionary<byte, int> EraseLimit = [];
    private static readonly Dictionary<byte, int> TempEraseLimit = [];
    public static readonly Dictionary<byte, CustomRoles> ErasedRoleStorage = [];

    public static void SetupCustomOption()
    {
        Options.SetupRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.Eraser);
        EraseLimitOpt = IntegerOptionItem.Create(Id + 10, "EraseLimit", new(1, 15, 1), 2, TabGroup.ImpostorRoles, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Eraser])
            .SetValueFormat(OptionFormat.Times);
        HideVoteOpt = BooleanOptionItem.Create(Id + 11, "EraserHideVote", false, TabGroup.ImpostorRoles, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Eraser]);
    }
    public override void Init()
    {
        playerIdList.Clear();
        EraseLimit.Clear();
        PlayerToErase.Clear();
        didVote.Clear();
        TempEraseLimit.Clear();
        ErasedRoleStorage.Clear();
    }
    public override void Add(byte playerId)
    {
        playerIdList.Add(playerId);
        EraseLimit.Add(playerId, EraseLimitOpt.GetInt());
    }
    private static void SendRPC(byte playerId)
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SyncRoleSkill, SendOption.Reliable, -1);
        writer.WritePacked((int)CustomRoles.Eraser);
        writer.Write(playerId);
        writer.Write(EraseLimit[playerId]);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }
    public override void ReceiveRPC(MessageReader reader, PlayerControl NaN)
    {
        byte playerId = reader.ReadByte();
        int limit = reader.ReadInt32();
        if (!EraseLimit.ContainsKey(playerId))
        {
            EraseLimit.Add(playerId , limit);
        }
        else
            EraseLimit[playerId] = limit;
    }
    public override string GetProgressText(byte playerId, bool comms)
        => Utils.ColorString(EraseLimit[playerId] > 0 ? Utils.GetRoleColor(CustomRoles.Eraser) : Color.gray, EraseLimit.TryGetValue(playerId, out var x) ? $"({x})" : "Invalid");

    public override bool HideVote(PlayerVoteArea votedPlayer)
        => CheckForEndVotingPatch.CheckRole(votedPlayer.TargetPlayerId, CustomRoles.Eraser) && HideVoteOpt.GetBool() && TempEraseLimit[votedPlayer.TargetPlayerId] > 0;

    public override void OnVote(PlayerControl player, PlayerControl target)
    {
        if (!HasEnabled) return;
        if (player == null || target == null) return;
        if (target.Is(CustomRoles.Eraser)) return;
        if (EraseLimit[player.PlayerId] <= 0) return;

        if (didVote.Contains(player.PlayerId)) return;
        didVote.Add(player.PlayerId);

        Logger.Info($"{player.GetCustomRole()} votes for {target.GetCustomRole()}", "Vote Eraser");

        if (target.PlayerId == player.PlayerId)
        {
            Utils.SendMessage(GetString("EraserEraseSelf"), player.PlayerId, Utils.ColorString(Utils.GetRoleColor(CustomRoles.Eraser), GetString("EraserEraseMsgTitle")));
            return;
        }

        var targetRole = target.GetCustomRole();
        if (targetRole.IsTasklessCrewmate() || targetRole.IsNeutral() || Main.TasklessCrewmate.Contains(target.PlayerId) || CopyCat.playerIdList.Contains(target.PlayerId) || target.Is(CustomRoles.Stubborn))
        {
            Utils.SendMessage(string.Format(GetString("EraserEraseBaseImpostorOrNeutralRoleNotice"), target.GetRealName()), player.PlayerId, Utils.ColorString(Utils.GetRoleColor(CustomRoles.Eraser), GetString("EraserEraseMsgTitle")));
            return;
        }

        EraseLimit[player.PlayerId]--;
        SendRPC(player.PlayerId);

        if (!PlayerToErase.Contains(target.PlayerId))
            PlayerToErase.Add(target.PlayerId);

        Utils.SendMessage(string.Format(GetString("EraserEraseNotice"), target.GetRealName()), player.PlayerId, Utils.ColorString(Utils.GetRoleColor(CustomRoles.Eraser), GetString("EraserEraseMsgTitle")));

        Utils.NotifyRoles(SpecifySeer: player);
    }
    public override void OnReportDeadBody(PlayerControl reporter, PlayerControl target)
    {
        foreach (var eraserId in playerIdList.ToArray())
        {
            TempEraseLimit[eraserId] = EraseLimit[eraserId];
        }

        PlayerToErase.Clear();
        didVote.Clear();
    }
    public override void NotifyAfterMeeting()
    {
        foreach (var pc in PlayerToErase.ToArray())
        {
            var player = Utils.GetPlayerById(pc);
            if (player == null) continue;

            player.Notify(GetString("LostRoleByEraser"));
        }
    }
    public override void AfterMeetingTasks()
    {
        foreach (var pc in PlayerToErase.ToArray())
        {
            var player = Utils.GetPlayerById(pc);
            if (player == null) continue;
            if (!ErasedRoleStorage.ContainsKey(player.PlayerId))
            {
                ErasedRoleStorage.Add(player.PlayerId, player.GetCustomRole());
                Logger.Info($"Added {player.GetNameWithRole()} to ErasedRoleStorage", "Eraser");
            }
            else
            {
                Logger.Info($"Canceled {player.GetNameWithRole()} Eraser bcz already erased.", "Eraser");
                return;
            }
            player.RpcSetCustomRole(GetErasedRole(player.GetCustomRole().GetRoleTypes(), player.GetCustomRole()));
            player.ResetKillCooldown();
            player.SetKillCooldown();
            Logger.Info($"{player.GetNameWithRole()} Erase by Eraser", "Eraser");
        }
        Utils.MarkEveryoneDirtySettings();
    }

    // Erased RoleType - Impostor, Shapeshifter, Crewmate, Engineer, Scientist (Not Neutrals)
    public static CustomRoles GetErasedRole(RoleTypes roleType, CustomRoles role)
    {
        return role.IsVanilla()
            ? role
            : roleType switch
            {
                RoleTypes.Crewmate => CustomRoles.CrewmateTOHE,
                RoleTypes.Scientist => CustomRoles.ScientistTOHE,
                RoleTypes.Engineer => CustomRoles.EngineerTOHE,
                RoleTypes.Impostor => CustomRoles.ImpostorTOHE,
                RoleTypes.Shapeshifter => CustomRoles.ShapeshifterTOHE,
                _ => role,
            };
    }
}
