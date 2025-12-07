using System;
using System.Collections;
using UnityEngine;

[System.Serializable]
public abstract class Command : ScriptableObject
{
    public string abilityName;
    public KeyCode keyboardShortcut;
    public Selectable selectable;

    [Header("Command Options")]
    public bool singleUnitCommand = false;

    public bool instantApplication = false;

    public bool hasCooldown = false;
    [ShowIf("hasCooldown")]
    public float cooldownLength = 0f;
    [ShowIf("hasCooldown")]
    public float cooldownTime = 0f;

    public bool needsTargeting = false;
    [ShowIf("needsTargeting")]
    public bool targetsUnits = false;
    [ShowIf("targetsUnits")]
    public bool targetsMine = false;
    [ShowIf("targetsUnits")]
    public bool targetAllied = false;
    [ShowIf("targetsUnits")]
    public bool targetNeutral = false;
    [ShowIf("targetsUnits")]
    public bool targetEnemies = false;
    [ShowIf("targetsUnits")]
    protected Selectable targetUnit;
    [ShowIf("needsTargeting")]
    public bool targetsLocation = false;
    [ShowIf("targetsLocation")]
    protected Vector3 targetLocation;
    [ShowIf("needsTargeting")]
    public Texture2D cursor;


    public bool uninteruptable = false;

    [HideInInspector]
    private bool firstFrame = true;


    public void CommandInitialize(Selectable unit)
    {
        selectable = unit;
    }

    public void NextFrame()
    {
        if (firstFrame) { FirstFrame(); firstFrame = false; }
        else { Execute(); }
    }

    public virtual void FirstFrame()
    {
        if (Available())
        {
            cooldownTime = Time.time + cooldownLength;
        }
        else
        {
            selectable.NextCommand();
        }
    }

    public virtual void Execute()
    {
        Debug.Log("Base Execute");
    }

    public Command Duplicate()
    {
        Command copy = Instantiate(this);
        CopyReferences(copy);
        copy.firstFrame = true;
        return copy;
    }

    public virtual bool Available()
    {
        if (hasCooldown)
        {
            return Time.time >= cooldownTime;
        }
        return true;
    }

    public bool SetTarget(Selectable sel)
    {
        if (targetsUnits)
        {
            if (UnitManager.IsMine(selectable.teamID, sel.teamID) && targetsMine)
                targetUnit = sel;
            else if (UnitManager.IsAllied(selectable.teamID, sel.teamID) && targetAllied)
                targetUnit = sel;
            else if (UnitManager.IsNeutral(selectable.teamID, sel.teamID) && targetNeutral)
                targetUnit = sel;
            else if (UnitManager.IsEnemy(selectable.teamID, sel.teamID) && targetEnemies)
                targetUnit = sel;
            else return false;
            return true;
        }
        return false;
    }

    public bool SetTarget(Vector3 targetPosition)
    {
        if (targetsLocation)
        {
            targetLocation = targetPosition;
            return true;
        }
        return false;
    }

    protected virtual void CopyReferences(Command copy)
    {
        copy.selectable = this.selectable;
    }
}
