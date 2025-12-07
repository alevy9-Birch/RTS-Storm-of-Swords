using System.Collections.Generic;
using UnityEngine;

public class Selectable : MonoBehaviour
{
    [SerializeField]
    public List<Command> commands = new List<Command>();
    protected Dictionary<string, Command> commandsDict = new();
    public Queue<Command> queue = new();
    public Command activeCommand;
    protected Selectable selectable;
    public Transform selectionIndicators;
    private const float SelectionIndicatorsRotateSpeed = 25f;

    public Command defaultIdle;
    public Command defaultRightClick;

    [Range(0, UnitManager.maxPlayers - 1)]
    public byte teamID;

    [Header("Gizmos")]
    public bool DrawGizmos = false;
    private float gizmoRange = -1f;
    private Color gizmoColor = Color.white;


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

        /*foreach (var command in commandsDict.Keys)
        {
            Debug.Log(command + " " + commandsDict[command]);
        }*/

        InitializeSelectionIndicators();
    }

    protected virtual void Update()
    {
        selectionIndicators.Rotate(transform.up, Time.deltaTime * SelectionIndicatorsRotateSpeed);

        if (activeCommand == null)
        {
            if (queue.Count == 0)
            {
                DefaultIdle();
            }
            else
            {
                NextCommand();
            }
        }
        else
        {
            activeCommand.NextFrame();
        }
    }

    protected void DefaultIdle()
    {
        activeCommand = GenerateCommand(defaultIdle);
    }

    private void OnDestroy()
    {
        UnitManager.Instance.allUnits.Remove(gameObject);
    }

    public Command GenerateCommand(string Name)
    {
        if (commandsDict.ContainsKey(Name))
        {
            return commandsDict[Name].Duplicate();
        }
        else
        {
            return null;
        }
    }

    public Command GenerateCommand(Command command)
    {
        if (command == null) return null;
        return command.Duplicate();
    }

    //Issue Commands

    public void OverrideCommand(Command command)
    {
        command.selectable = this;
        if (activeCommand.uninteruptable)
        {
            queue.Clear();
            AddCommand(command);
        }
        else
        {
            activeCommand = command;
            queue.Clear();
        }
    }

    public void AddCommand(Command command)
    {
        command.selectable = this;
        queue.Enqueue(command);
    }

    public bool ContainsCommand(string name)
    {
        return commandsDict.ContainsKey(name);
    }

    public Command GetCommand(string name)
    {
        return commandsDict[name];
    }

    public Command GetCommand(Command command)
    {
        return commandsDict[command.abilityName];
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

    public void NextCommand()
    {
        if (queue.Count > 0)
        {
            activeCommand = queue.Dequeue();
        }
        else
            DefaultIdle();
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

    public void SetSelectionIndicator(bool isVisible)
    {
        selectionIndicators.GetChild(0).gameObject.SetActive(isVisible);
    }

    public void SetHoverIndicator(bool isVisible)
    {
        selectionIndicators.GetChild(1).gameObject.SetActive(isVisible);
    }

    public void InitializeSelectionIndicators()
    {
        selectionIndicators.GetChild(0).GetComponent<MeshRenderer>().material = UnitManager.Instance.GetMat(teamID, "UnitSelectionCircle"); 
        selectionIndicators.GetChild(1).GetComponent<MeshRenderer>().material = UnitManager.Instance.GetMat(teamID, "UnitSelectionBorder");
    }

    //Commands Interface

    public List<Selectable> SelectablesInRange(float range, bool mine = false, bool ally = false, bool neutral = false, bool hostile = false)
    {
        List<Selectable> result = new List<Selectable>();
        var hits = Physics2D.OverlapCircleAll(transform.position, range, UnitManager.Instance.selectable);

        //Gizmo
        if (mine) gizmoColor = Color.cyan;
        else if (ally) gizmoColor = Color.green;
        else if (neutral) gizmoColor = Color.white;
        else if (hostile) gizmoColor = Color.red;
        else gizmoColor = Color.yellow;
        gizmoRange = range;

        //Logic
        foreach (var h in hits)
        {
            Selectable selectable = h.GetComponent<Selectable>();
            if (selectable == null) continue;

            if (mine && UnitManager.IsAllied(teamID, selectable.teamID))
                result.Add(selectable);
            else if (ally && UnitManager.IsAllied(teamID, selectable.teamID))
                result.Add(selectable);
            else if (neutral && UnitManager.IsNeutral(teamID, selectable.teamID))
                result.Add(selectable);
            else if (hostile && UnitManager.IsEnemy(teamID, selectable.teamID))
                result.Add(selectable);
        }
        return result;
    }

    

    //Gizmo

    private void OnDrawGizmos()
    {
        if (!DrawGizmos) return;

        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, gizmoRange);
    }
}
