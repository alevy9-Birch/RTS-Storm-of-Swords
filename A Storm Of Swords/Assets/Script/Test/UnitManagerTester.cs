using UnityEngine;

public class UnitManagerTester : MonoBehaviour
{
    [Range(0, UnitManager.maxPlayers - 1)]
    public byte teamA;

    [Range(0, UnitManager.maxPlayers - 1)]
    public byte teamB;

    [Header("Call Functions")]
    public bool SetAllies;
    public bool SetNeutral;
    public bool SetEnemies;
    public bool PrintRelation;
    public bool IsSelf;
    public bool IsAllied;
    public bool IsNeutral;
    public bool IsEnemy;

    public void Update()
    {
        if (SetAllies)
        {
            UnitManager.SetRelation(teamA, teamB, 1);
            UnitManager.RecalculateTeamColors();
            SetAllies = false;
        }
        if (SetNeutral)
        {
            UnitManager.SetRelation(teamA, teamB, 0);
            UnitManager.RecalculateTeamColors();
            SetNeutral = false;
        }
        if (SetEnemies)
        {
            UnitManager.SetRelation(teamA, teamB, -1);
            UnitManager.RecalculateTeamColors();
            SetEnemies = false;
        }
        if (PrintRelation)
        {
            Debug.Log(UnitManager.GetRelation(teamA, teamB));
            UnitManager.RecalculateTeamColors();
            PrintRelation = false;
        }

        IsSelf = teamA == teamB;
        IsAllied = UnitManager.IsAllied(teamA, teamB);
        IsNeutral = UnitManager.IsNeutral(teamA, teamB);
        IsEnemy = UnitManager.IsEnemy(teamA, teamB);
    }
}
