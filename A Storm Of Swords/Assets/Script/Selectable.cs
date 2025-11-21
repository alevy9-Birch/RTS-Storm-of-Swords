using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Selectable : MonoBehaviour
{
    public short teamNum;

    public Queue<Command> queue = new();
    private Command activeCommand;
    protected Selectable selectable;

    public Dictionary<string, Command> commands = new();

    protected InputManager inputManager;

    //Start & Update

    protected void Start()
    {
        UnitSelectionManager.Instance.allUnitsList.Add(this.gameObject);

        //AssignCommands();
    }

    protected virtual void Update()
    {
        if (activeCommand == null)
        {
            if (queue.Count > 0)
            {
                NextCommand();
            }
            else
            {
                activeCommand = GetCommand("IdleCommand");
            }
        }

        if (activeCommand.Execute())
            NextCommand();
    }
    private void OnDestroy()
    {
        UnitSelectionManager.Instance.allUnitsList.Remove(gameObject);
    }

    //Issue Commands

    protected void OverrideCommand(Command command)
    {
        queue.Clear();
        queue.Enqueue(command);
    }

    protected void EnqueueCommand(Command command)
    {
        queue.Enqueue(command);
    }

    protected void ReplaceCommand(Command replacementCommand)
    {
        queue.Dequeue();
        queue.Enqueue(replacementCommand);

        for (int i = 1; i < queue.Count; i++)
        {
            queue.Enqueue(queue.Dequeue());
        }
    }

    protected void NextCommand()
    {
        Destroy(activeCommand);
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

    //Command Dictionary
    protected Command GetCommand(string commandName)
    {
        return (Command)gameObject.AddComponent(commands[commandName].GetType());
    }

    public void AddCommand(string name, Command command)
    {
        if (commands.ContainsKey(name))
        {
            commands[name] = command;
            Debug.Log("Command Override");
        }
        else
        {
            commands.Add(name, command);
        }
    }
    
    //Input Manager

    protected abstract class InputManager
    {
        static Camera cam;
        static Vector3 mousePosition;
        protected static RaycastHit hit;

        protected static LayerMask ground = LayerMask.GetMask("Ground");
        protected static LayerMask selectable = LayerMask.GetMask("Selectable");

        protected static bool shift = false;

        protected static void Update()
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out hit, Mathf.Infinity, ground | selectable);
        }
        public virtual void CheckInput()
        {
            GetInput();
        }
        public void GetInput()
        {
            shift = Input.GetKeyDown(KeyCode.LeftShift);
        }
    }

    public void CheckInput()
    {
        inputManager.CheckInput();
    }


}
