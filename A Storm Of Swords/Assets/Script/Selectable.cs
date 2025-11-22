using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Selectable : MonoBehaviour
{
    public short teamNum;

    [SerializeField]
    public List<Command> commands = new List<Command>();
    protected Dictionary<string, Command> commandsDict = new();
    public Queue<Command> queue = new();
    private Command activeCommand;
    protected Selectable selectable;

    protected InputManager inputManager;
    public Byte teamID;

    //Start & Update

    protected virtual void Start()
    {
        UnitManager.Instance.allUnits.Add(this.gameObject);
        foreach (var command in commands)
        {
            command.CommandInitialize(this);
            if (!commandsDict.ContainsKey(command.abilityName))
                commandsDict.Add(command.abilityName, command);
        }
    }

    protected virtual void Update()
    {
        if (activeCommand == null)
        {
            if (queue.Count == 0)
            {
                activeCommand = GenerateCommand("Idle");
            }
            else
            {
                NextCommand();
            }
        }
        if (activeCommand.Execute())
            NextCommand();
    }
    private void OnDestroy()
    {
        UnitManager.Instance.allUnits.Remove(gameObject);
    }

    public Command GenerateCommand(string Name)
    {
        if (!commandsDict.ContainsKey(Name))
            return null;
        return commandsDict[Name].Duplicate();
    }

    //Issue Commands

    public void OverrideCommand(Command command)
    {
        if (activeCommand.CanBeInterupted())
        {
            activeCommand = command;
            queue.Clear();
        }
        else
        {
            queue.Clear();
            AddCommand(command);
        }
    }

    public void AddCommand(Command command)
    {
        queue.Enqueue(command);
    }

    public void InsertCommand(Command insertion)
    {
        queue.Enqueue(activeCommand); 

        for (int i = 1; i < queue.Count; i++)
        {
            queue.Enqueue(queue.Dequeue());
        }
    }

    public void InterruptCommand(Command interuption)
    {
        InsertCommand(activeCommand);
        activeCommand = interuption;
    }

    protected void NextCommand()
    {
        activeCommand = queue.Dequeue();
    }

    protected void NewPriorityCommand(Command newPriorityCommand)
    {
        queue.Enqueue(newPriorityCommand);

        for (int i = 1; i < queue.Count; i++)
        {
            queue.Enqueue(queue.Dequeue());
        }
    }

    //Selection Indicator

    public static void TriggerSelectionIndicator(GameObject unit, bool isVisible)
    {
        unit.transform.GetChild(0).gameObject.SetActive(isVisible);
    }

    public static void ToggleSelectionIndicator(GameObject unit)
    {
        GameObject targetObject = unit.transform.GetChild(0).gameObject;
        targetObject.SetActive(!targetObject.activeSelf);
    }
}
