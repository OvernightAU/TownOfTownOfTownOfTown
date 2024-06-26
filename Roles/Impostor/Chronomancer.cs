using static TOHE.Options;

namespace TOHE.Roles.Impostor;

internal class Chronomancer : RoleBase
{
    //===========================SETUP================================\\
    private const int Id = 900;
    private static readonly HashSet<byte> playerIdList = [];
    public static bool HasEnabled => playerIdList.Any();
    public override bool IsEnable => HasEnabled;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    //==================================================================\\

    private static Dictionary<byte, long> firstKill = [];
    private static Dictionary<byte, long> lastCooldownStart = [];
    private static Dictionary<byte, float> ChargedTime = [];

    private static OptionItem KillCooldown;

    public static void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.Chronomancer);
        KillCooldown = FloatOptionItem.Create(Id + 10, "ChronomancerKillCooldown", new(0f, 180f, 2.5f), 30f, TabGroup.ImpostorRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Chronomancer])
            .SetValueFormat(OptionFormat.Seconds);
    }

    public override void Init()
    {
        playerIdList.Clear();
        firstKill = [];
        lastCooldownStart = [];
        ChargedTime = [];
    }
    public override void Add(byte playerId)
    {
        long now = Utils.GetTimeStamp();
        playerIdList.Add(playerId);
        firstKill.Add(playerId, -1);
        ChargedTime.Add(playerId, 0);
        lastCooldownStart.Add(playerId, now);
    }

    public override void SetKillCooldown(byte id) => OnSetKillCooldown(id);

    private static void OnSetKillCooldown(byte id)
    {
        long now = Utils.GetTimeStamp();

        if (firstKill[id] == -1)
        {
            Main.AllPlayerKillCooldown[id] = KillCooldown.GetFloat();
            lastCooldownStart[id] = now;
            return;
        }
        if (now - firstKill[id] >= ChargedTime[id])
        {
            firstKill[id] = -1;
            Main.AllPlayerKillCooldown[id] = KillCooldown.GetFloat();
            lastCooldownStart[id] = now;
        }
        else Main.AllPlayerKillCooldown[id] = 0f;
        Logger.Info($"{Utils.GetPlayerById(id).GetNameWithRole()} kill cd set to {Main.AllPlayerKillCooldown[id]}", "Chronomancer");
    }

    public override void AfterMeetingTasks()
    {
        long now = Utils.GetTimeStamp();
        foreach (var playerId in playerIdList.ToArray())
        {
            if (Utils.GetPlayerById(playerId).IsAlive())
            { 
                firstKill[playerId] =  -1;
                lastCooldownStart[playerId] = now;
                ChargedTime[playerId] = 0;
                OnSetKillCooldown(playerId);
            }
           
        }
    }

    public static void OnCheckMurder(PlayerControl killer)
    {
        long now = Utils.GetTimeStamp();
        if (firstKill[killer.PlayerId] == -1)
        {
            firstKill[killer.PlayerId] = now;
            ChargedTime[killer.PlayerId] = (firstKill[killer.PlayerId] - lastCooldownStart[killer.PlayerId]) - KillCooldown.GetFloat();
        }
        OnSetKillCooldown(killer.PlayerId);
    }
}