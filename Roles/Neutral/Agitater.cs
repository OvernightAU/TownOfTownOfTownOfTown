﻿using AmongUs.GameOptions;
using Hazel;
using UnityEngine;
using static TOHE.Translator;
using TOHE.Roles.Core;

namespace TOHE.Roles.Neutral;
internal class Agitater : RoleBase
{
    //===========================SETUP================================\\
    private const int Id = 15800;
    private static readonly List<byte> playerIdList = [];
    public static bool HasEnabled => playerIdList.Any();
    public override bool IsEnable => HasEnabled;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    //==================================================================\\

    private static OptionItem BombExplodeCooldown;
    private static OptionItem PassCooldown;
    private static OptionItem AgitaterCanGetBombed;
    private static OptionItem AgiTaterBombCooldown;
    private static OptionItem AgitaterAutoReportBait;
    private static OptionItem HasImpostorVision;

    public static byte CurrentBombedPlayer = byte.MaxValue;
    public static byte LastBombedPlayer = byte.MaxValue;
    public static bool AgitaterHasBombed = false;
    public static long CurrentBombedPlayerTime = new();
    public static long AgitaterBombedTime = new();


    public static void SetupCustomOption()
    {
        Options.SetupRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Agitater);
        AgiTaterBombCooldown = FloatOptionItem.Create(Id + 10, "AgitaterBombCooldown", new(10f, 180f, 2.5f), 20f, TabGroup.NeutralRoles, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Agitater])
            .SetValueFormat(OptionFormat.Seconds);
        PassCooldown = FloatOptionItem.Create(Id + 11, "AgitaterPassCooldown", new(0f, 5f, 0.25f), 1f, TabGroup.NeutralRoles, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Agitater])
            .SetValueFormat(OptionFormat.Seconds);
        BombExplodeCooldown = FloatOptionItem.Create(Id + 12, "BombExplodeCooldown", new(1f, 10f, 1f), 10f, TabGroup.NeutralRoles, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Agitater])
            .SetValueFormat(OptionFormat.Seconds);
        AgitaterCanGetBombed = BooleanOptionItem.Create(Id + 13, "AgitaterCanGetBombed", false, TabGroup.NeutralRoles, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Agitater]);
        AgitaterAutoReportBait = BooleanOptionItem.Create(Id + 14, "AgitaterAutoReportBait", false, TabGroup.NeutralRoles, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Agitater]);
        HasImpostorVision = BooleanOptionItem.Create(Id + 15, "ImpostorVision", true, TabGroup.NeutralRoles, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Agitater]);
    }
    public override void Init()
    {
        playerIdList.Clear();
        CurrentBombedPlayer = byte.MaxValue;
        LastBombedPlayer = byte.MaxValue;
        AgitaterHasBombed = false;
        CurrentBombedPlayerTime = new();
    }

    public override void Add(byte playerId)
    {
        playerIdList.Add(playerId);
        CustomRoleManager.OnFixedUpdateOthers.Add(OnFixedUpdateOthers);

        if (!AmongUsClient.Instance.AmHost) return;
        if (!Main.ResetCamPlayerList.Contains(playerId))
            Main.ResetCamPlayerList.Add(playerId);
    }

    public static void ResetBomb()
    {
        CurrentBombedPlayer = byte.MaxValue;
        CurrentBombedPlayerTime = new();
        LastBombedPlayer = byte.MaxValue;
        AgitaterHasBombed = false;
        SendRPC(CurrentBombedPlayer, LastBombedPlayer);
    }
    public override bool CanUseKillButton(PlayerControl pc) => pc.IsAlive();
    public override void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = AgiTaterBombCooldown.GetFloat();
    public override void ApplyGameOptions(IGameOptions opt, byte id) => opt.SetVision(HasImpostorVision.GetBool());

    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        if (!HasEnabled) return false;
        if (AgitaterAutoReportBait.GetBool() && target.Is(CustomRoles.Bait)) return true;
        if (target.Is(CustomRoles.Pestilence))
        {
            target.RpcMurderPlayer(killer);
            ResetBomb();
            return false;
        }

        CurrentBombedPlayer = target.PlayerId;
        LastBombedPlayer = killer.PlayerId;
        CurrentBombedPlayerTime = Utils.GetTimeStamp();
        killer.RpcGuardAndKill(killer);
        killer.Notify(GetString("AgitaterPassNotify"));
        target.Notify(GetString("AgitaterTargetNotify"));
        AgitaterHasBombed = true;
        killer.ResetKillCooldown();
        killer.SetKillCooldown();
        
        _ = new LateTask(() =>
        {
            if (CurrentBombedPlayer != byte.MaxValue && GameStates.IsInTask)
            {
                var pc = Utils.GetPlayerById(CurrentBombedPlayer);
                if (pc != null && pc.IsAlive() && killer != null)
                {
                    Main.PlayerStates[CurrentBombedPlayer].deathReason = PlayerState.DeathReason.Bombed;
                    pc.SetRealKiller(Utils.GetPlayerById(playerIdList[0]));
                    pc.RpcMurderPlayer(pc);
                    Logger.Info($"{killer.GetNameWithRole()}  bombed {pc.GetNameWithRole()} bomb cd complete", "Agitater");
                    ResetBomb();
                }

            }
        }, BombExplodeCooldown.GetFloat(), "Agitater Bomb Kill");
        return false;
    }

    public override void OnReportDeadBody(PlayerControl reported, PlayerControl agitatergoatedrole)
    {
        if (CurrentBombedPlayer == byte.MaxValue) return;
        var target = Utils.GetPlayerById(CurrentBombedPlayer);
        var killer = Utils.GetPlayerById(playerIdList[0]);
        if (target == null || killer == null) return;
        
        target.SetRealKiller(killer);
        Main.PlayerStates[CurrentBombedPlayer].deathReason = PlayerState.DeathReason.Bombed;
        Main.PlayerStates[CurrentBombedPlayer].SetDead();
        target.RpcExileV2();
        MurderPlayerPatch.AfterPlayerDeathTasks(killer, target, true);
        ResetBomb();
        Logger.Info($"{killer.GetRealName()} bombed {target.GetRealName()} on report", "Agitater");
    }
    private static void OnFixedUpdateOthers(PlayerControl player)
    {
        if (!AgitaterHasBombed) return;

        if (!player.IsAlive())
        {
            ResetBomb();
        }
        else
        {
            var playerPos = player.GetCustomPosition();
            Dictionary<byte, float> targetDistance = [];
            float dis;

            foreach (var target in Main.AllAlivePlayerControls)
            {
                if (target.PlayerId != player.PlayerId && target.PlayerId != LastBombedPlayer)
                {
                    dis = Vector2.Distance(playerPos, target.transform.position);
                    targetDistance.Add(target.PlayerId, dis);
                }
            }

            if (targetDistance.Any())
            {
                var min = targetDistance.OrderBy(c => c.Value).FirstOrDefault();
                var target = Utils.GetPlayerById(min.Key);
                var KillRange = GameOptionsData.KillDistances[Mathf.Clamp(GameOptionsManager.Instance.currentNormalGameOptions.KillDistance, 0, 2)];
                if (min.Value <= KillRange && !player.inVent && !target.inVent && player.RpcCheckAndMurder(target, true))
                {
                    PassBomb(player, target);
                }
            }
        }
    }
    private static void PassBomb(PlayerControl player, PlayerControl target)
    {
        if (!AgitaterHasBombed) return;
        if (target.Data.IsDead) return;

        var now = Utils.GetTimeStamp();
        if (now - CurrentBombedPlayerTime < PassCooldown.GetFloat()) return;
        if (target.PlayerId == LastBombedPlayer) return;
        if (!AgitaterCanGetBombed.GetBool() && target.Is(CustomRoles.Agitater)) return;


        if (target.Is(CustomRoles.Pestilence))
        {
            target.RpcMurderPlayer(player);
            ResetBomb();
            return;
        }
        LastBombedPlayer = CurrentBombedPlayer;
        CurrentBombedPlayer = target.PlayerId;
        CurrentBombedPlayerTime = now;
        Utils.MarkEveryoneDirtySettings();


        player.Notify(GetString("AgitaterPassNotify"));
        target.Notify(GetString("AgitaterTargetNotify"));

        SendRPC(CurrentBombedPlayer, LastBombedPlayer);
        Logger.Msg($"{player.GetNameWithRole()} passed bomb to {target.GetNameWithRole()}", "Agitater Pass");
    }

    public static void SendRPC(byte newbomb, byte oldbomb)
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SyncRoleSkill, SendOption.Reliable, -1);
        writer.WritePacked((int)CustomRoles.Agitater);
        writer.Write(newbomb);
        writer.Write(oldbomb);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }

    public override void ReceiveRPC(MessageReader reader, PlayerControl NaN)
    {
        CurrentBombedPlayer = reader.ReadByte();
        LastBombedPlayer = reader.ReadByte();
    }
    public override void SetAbilityButtonText(HudManager hud, byte playerId)
        => hud.KillButton.OverrideText(GetString("AgitaterKillButtonText"));
    public override Sprite GetKillButtonSprite(PlayerControl player, bool shapeshifting) => CustomButton.Get("bombshell");
}
