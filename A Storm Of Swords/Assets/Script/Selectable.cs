using System.Collections.Generic;
using UnityEngine;

public class Selectable : MonoBehaviour
{
    [SerializeField]
    public List<Command> commands = new List<Command>();
    protected Dictionary<string, Command> commandsDict = new();
    public Queue<Command> queue = new();
    private Command activeCommand;
    protected Selectable selectable;

    protected InputManager inputManager;
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

            if (selectable.teamID == UnitSelectionManager.LocalInstance.myID && mine)
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

    private void OnDrawGizmos()
    {
        if (!DrawGizmos) return;

        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, gizmoRange);
    }
}
