using System;
using System.Collections;
using UnityEngine;

public class Command : ScriptableObject
{
    public string abilityName;
    protected Selectable selectable;
    protected bool interuptable = true;


    public void CommandInitialize(Selectable unit)
    {
        selectable = unit;
    }

    public virtual bool Execute() //Called Every Frame it Runs
    {
        return true;
    }

    public virtual bool SetUp()
    {
        return true;
    }

    public bool CanBeInterupted()
    {
        return interuptable;
    }

    public virtual Command Duplicate()
    {
        Command copy = (Command)Activator.CreateInstance(this.GetType());
        CopyTo(copy);
        return copy;
    }

    protected virtual void CopyTo(Command copy)
    {
        copy.abilityName = abilityName;
        copy.selectable = selectable;
        copy.interuptable = interuptable;
    }
}
