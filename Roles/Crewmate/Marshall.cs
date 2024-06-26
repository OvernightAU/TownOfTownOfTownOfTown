using UnityEngine;
using TOHE.Roles.Core;
using static TOHE.Options;
using static TOHE.Translator;

namespace TOHE.Roles.Crewmate;

internal class Marshall : RoleBase
{
    //===========================SETUP================================\\
    private const int Id = 11900;
    private static readonly HashSet<byte> playerIdList = [];
    public static bool HasEnabled => playerIdList.Any();
    public override bool IsEnable => HasEnabled;
    public override CustomRoles ThisRoleBase => CustomRoles.Crewmate;
    //==================================================================\\

    private static readonly Color RoleColor = Utils.GetRoleColor(CustomRoles.Marshall);

    public static void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Marshall);
        OverrideTasksData.Create(Id + 10, TabGroup.CrewmateRoles, CustomRoles.Marshall);
    }
    public override void Init()
    {
        playerIdList.Clear();
    }
    public override void Add(byte playerId)
    {
        playerIdList.Add(playerId);
    }
    private static bool GetExpose(PlayerControl pc)
    {
        if (!pc.IsAlive() || pc.Is(CustomRoles.Madmate)) return false;

        return pc.Is(CustomRoles.Marshall) && pc.GetPlayerTaskState().IsTaskFinished;
    }
    private static bool IsMarshallTarget(PlayerControl seer) => CustomRoles.Marshall.HasEnabled() && seer.Is(CustomRoleTypes.Crewmate);
    public override string GetMark(PlayerControl seer, PlayerControl target = null, bool isForMeeting = false)
    {
        target ??= seer;

        return IsMarshallTarget(seer) && GetExpose(target) ? Utils.ColorString(RoleColor, "★") : string.Empty;
    }

    private static bool VisibleToCrewmate(PlayerControl seer, PlayerControl target) => target.GetPlayerTaskState().IsTaskFinished && target.Is(CustomRoles.Marshall) && seer.Is(CustomRoleTypes.Crewmate);
    public override bool KnowRoleTarget(PlayerControl seer, PlayerControl target) => VisibleToCrewmate(seer, target);
    public override bool OthersKnowTargetRoleColor(PlayerControl seer, PlayerControl target) => VisibleToCrewmate(seer, target);
    
    public override bool OnRoleGuess(bool isUI, PlayerControl target, PlayerControl pc, CustomRoles role, ref bool guesserSuicide)
    {
        if (!isUI) Utils.SendMessage(GetString("GuessMarshallTask"), pc.PlayerId);
        else pc.ShowPopUp(GetString("GuessMarshallTask"));
        return true;
    }
}
