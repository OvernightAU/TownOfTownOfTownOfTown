﻿using Hazel;

namespace TOHE.Roles.Neutral;

internal class Collector : RoleBase
{

    //===========================SETUP================================\\
    private const int Id = 14700;
    private static readonly HashSet<byte> playerIdList = [];
    public static bool HasEnabled => playerIdList.Any();
    public override bool IsEnable => false;
    public override CustomRoles ThisRoleBase => CustomRoles.Crewmate;
    //==================================================================\\

    private static OptionItem CollectorCollectAmount;

    private static readonly Dictionary<byte, byte> CollectorVoteFor = [];
    private static readonly Dictionary<byte, int> CollectVote = [];
    private static readonly Dictionary<byte, int> NewVote = [];

    private static bool calculated = false;

    public static void SetupCustomOption()
    {
        Options.SetupRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Collector);
        CollectorCollectAmount = IntegerOptionItem.Create(Id + 13, "CollectorCollectAmount", new(1, 100, 1), 20, TabGroup.NeutralRoles, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Collector])
            .SetValueFormat(OptionFormat.Votes);
    }
    public override void Init()
    {
        playerIdList.Clear();
        CollectorVoteFor.Clear();
        CollectVote.Clear();
        calculated = false;
    }
    public override void Add(byte playerId)
    {
        playerIdList.Add(playerId);
        CollectVote.TryAdd(playerId, 0);
    }
    private static void SendRPC(byte playerId)
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SyncRoleSkill, SendOption.Reliable, -1);
        writer.WritePacked((int)CustomRoles.Collector);
        writer.Write(playerId);
        writer.Write(CollectVote[playerId]);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }
    public override void ReceiveRPC(MessageReader reader, PlayerControl NaN)
    {
        byte PlayerId = reader.ReadByte();
        int Num = reader.ReadInt32();
        CollectVote.TryAdd(PlayerId, 0);
        CollectVote[PlayerId] = Num;
    }
    public override string GetProgressText(byte playerId, bool cooms)
    {
        int VoteAmount = CollectVote[playerId];
        int CollectNum = CollectorCollectAmount.GetInt();
        return Utils.ColorString(Utils.GetRoleColor(CustomRoles.Collector).ShadeColor(0.25f), $"({VoteAmount}/{CollectNum})");
    }
    public static void Clear()
    {
        CollectorVoteFor.Clear();
    }
    public static bool CollectorWin(bool check = true)
    {
        var pcArray = Main.AllPlayerControls.Where(x => x.Is(CustomRoles.Collector) && x.IsAlive() && CollectDone(x)).ToArray();
        if (pcArray.Any())
        {
            bool isWinConverted = false;
            foreach (var x in pcArray)
            {
                if (CustomWinnerHolder.CheckForConvertedWinner(x.PlayerId))
                {
                    isWinConverted = true;
                    break;
                }
            }
            if (check) return true;

            if (!isWinConverted)
            {
                CustomWinnerHolder.ResetAndSetWinner(CustomWinner.Collector);
                foreach (var winner in pcArray)
                    CustomWinnerHolder.WinnerIds.Add(winner.PlayerId);
            }
            return true;
        }
        return false;
    }
    private static bool CollectDone(PlayerControl player)
    {
        if (player.Is(CustomRoles.Collector))
        {
            var pcid = player.PlayerId;
            int VoteAmount = CollectVote[pcid];
            int CollectNum = CollectorCollectAmount.GetInt();
            if (VoteAmount >= CollectNum) return true;
        }
        return false;
    }
    public static void CollectorVotes(PlayerControl target, PlayerVoteArea ps)//集票者投票给谁
    {
        if (CheckForEndVotingPatch.CheckRole(ps.TargetPlayerId, CustomRoles.Collector))
            CollectorVoteFor.TryAdd(target.PlayerId, ps.TargetPlayerId);
    }
    public override void AfterMeetingTasks() => calculated = false;
    public static void CollectAmount(Dictionary<byte, int> VotingData, MeetingHud __instance)//得到集票者收集到的票
    {
        if (calculated) return;
        int VoteAmount;
        foreach (var pva in __instance.playerStates)
        {
            if (pva == null) continue;
            PlayerControl pc = Utils.GetPlayerById(pva.TargetPlayerId);
            if (pc == null) continue;
            foreach (var data in VotingData)
            {
                if (CollectorVoteFor.ContainsKey(data.Key) && pc.PlayerId == CollectorVoteFor[data.Key] && pc.Is(CustomRoles.Collector))
                {
                    VoteAmount = data.Value;
                    CollectVote.TryAdd(pc.PlayerId, 0);
                    CollectVote[pc.PlayerId] = CollectVote[pc.PlayerId] + VoteAmount;
                    SendRPC(pc.PlayerId);
                    Logger.Info($"{pc.GetNameWithRole().RemoveHtmlTags()}, collected {VoteAmount} votes from {Utils.GetPlayerById(data.Key).GetNameWithRole().RemoveHtmlTags()}", "Collected votes");
                }
            }
            if (CollectVote.ContainsKey(pc.PlayerId)) Logger.Info($"Total amount of votes collected {CollectVote[pc.PlayerId]}", "Collector total amount");
        }
        calculated = true;
    }
}
