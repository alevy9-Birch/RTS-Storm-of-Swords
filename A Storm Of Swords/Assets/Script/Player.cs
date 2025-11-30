using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static UnityEditor.FilePathAttribute;

//This Script is a merging of Input Manager and UnitSelectionManager
public class Player : MonoBehaviour
{
    public static Player PlayerInstance { get; private set; }
    
    [Header("References")]
    public Camera cam;
    Vector3 mousePosition;
    protected RaycastHit hit;

    public LayerMask groundMask;
    public LayerMask selectableMask;

    protected bool shift = false;
    protected Coroutine activeInputs;

    [Header("Player Specific")]

    [Range(0, UnitManager.maxPlayers)]
    public byte myID;
    public List<Selectable> selectedUnits = new List<Selectable>();
    private List<Selectable> hoveredUnits = new List<Selectable>();
    bool controllable = true;

    private Command repeatCommand = null;
    private Command targetingCommand = null;

    [Header("Visuals")]
    public RectTransform boxVisual;
    Rect selectionBox;

    Vector3 worldStartPos;
    Vector2 startPosition;
    Vector2 endPosition;

    public bool canFollow;
    public Vector3 followPosition;

    private void Awake()
    {
        PlayerInstance = this;
    }

    protected void Start()
    {
        startPosition = Vector2.zero;
        endPosition = Vector2.zero;
        DrawVisual();
    }

    protected void Update()
    {
        mousePosition = Input.mousePosition;

        Ray ray = cam.ScreenPointToRay(mousePosition);
        Physics.Raycast(ray, out hit, Mathf.Infinity, groundMask | selectableMask);
        if (hit.collider == null) return;

        canFollow = selectedUnits.Count > 0;
        if (canFollow) followPosition = selectedUnits[0].transform.position;

        if (targetingCommand != null) TargetCommand();
        else DefaultInputs();

    }

    void TargetCommand()
    {
        DrawVisual(false);
        if (!Input.GetMouseButtonDown(0))
        {
            //Cancel Targetting
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape) || (repeatCommand == null && Input.GetKeyUp(KeyCode.LeftShift))) CancelTargeting();
            if (Input.GetKeyDown(KeyCode.LeftShift)) repeatCommand = null;
        }

        if (IsInLayerMask(hit.collider.gameObject, selectableMask))
        {
            if (targetingCommand.SetTarget(hit.point)) HandleCommand(targetingCommand);
        }
        else if (IsInLayerMask(hit.collider.gameObject, groundMask))
        {
            if (targetingCommand.SetTarget(hit.point)) HandleCommand(targetingCommand);
        }
        else
        {
            CancelTargeting();
        }
    }

    void CallCommand(Command command) //For the First Time a Command is Called
    {
        if (command.needsTargeting)
        {
            targetingCommand = command;
            if (Input.GetKey(KeyCode.LeftShift)) repeatCommand = command;
        }
        else
        {
            HandleCommand(command);
        }
    }

    void RecallCommand(Command command) //For when shift is held and a command is called
    {
        if (command.needsTargeting)
        {
            targetingCommand = command;
        }
        else
        {
            HandleCommand(command);
        }
    }

    void HandleCommand(Command command)
    {
        foreach (Selectable selectable in selectedUnits)
        {
            if (selectable.ContainsCommand(command.name) && selectable.GetCommand(command.name).Available())
            {
                if (command.instantApplication) command.Execute();
                
                if (command.needsTargeting)
                {
                    targetingCommand = null;
                    if (repeatCommand != null) CallCommand(repeatCommand.Duplicate());
                    else if (Input.GetKey(KeyCode.LeftShift)) RecallCommand(command.Duplicate());
                }

                if (Input.GetKey(KeyCode.LeftShift)) selectable.AddCommand(command);
                else selectable.OverrideCommand(command);

                if (command.singleUnitCommand) break;
            }
        }
    }

    void CancelTargeting()
    {
        targetingCommand = null;
        repeatCommand = null;
        Debug.Log("Cancelled Command");
    }

    public void DefaultInputs()
    {
        if (Input.GetMouseButtonDown(0)) worldStartPos = hit.point;

        if (Input.GetMouseButton(0))
        {
            startPosition = Camera.main.WorldToScreenPoint(worldStartPos);
            endPosition = Input.mousePosition;
            DrawSelection();

            if ((boxVisual.rect.width > 10 || boxVisual.rect.height > 10))
            {
                DrawVisual();
                List<Selectable> inDrag = UnitsInDragSelect();

                Selectable[] tempHoveredUnits = hoveredUnits.ToArray();
                foreach (Selectable selectable in tempHoveredUnits)
                {
                    HoverUnit(selectable, inDrag.Contains(selectable));
                }
                foreach (Selectable selectable in inDrag)
                {
                    HoverUnit(selectable);
                }
            }
            else
            {
                DrawVisual(false);
                foreach (Selectable selectable in hoveredUnits)
                {
                    HoverUnit(selectable, false);
                }
                hoveredUnits.Clear();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            DrawVisual(false);
            if ((boxVisual.rect.width > 10 || boxVisual.rect.height > 10))
            {
                DragSelect();
                foreach (Selectable selectable in hoveredUnits)
                {
                    HoverUnit(selectable, false);
                }
                hoveredUnits.Clear();
            }
            else
            {
                DefaultClick();
            }

            startPosition = Vector2.zero;
            endPosition = Vector2.zero;
            DrawVisual();
        }

        if (controllable && selectedUnits.Count > 0)
        {
            if (Input.GetMouseButton(1))
            {
                CallCommand(selectedUnits[0].defaultRightClick);
            }
            foreach (Command command in selectedUnits[0].commands)
            {
                if (command.keyboardShortcut == KeyCode.None) continue;
                if (Input.GetKeyDown(command.keyboardShortcut))
                {
                    CallCommand(command);
                }
            }
        }
    }

    bool IsInLayerMask(GameObject obj, LayerMask mask)
    {
        return (mask.value & (1 << obj.layer)) != 0;
    }

    //UnitSelection

    void DefaultClick()
    {
        bool ctrl = Input.GetKey(KeyCode.LeftControl);
        bool shift = Input.GetKey(KeyCode.LeftShift);

        if (IsInLayerMask(hit.collider.gameObject, selectableMask))
        {
            Selectable selectable = hit.collider.gameObject.GetComponent<Selectable>();
            bool controlled = UnitManager.IsMine(myID, selectable.teamID);
            bool preselected = selectedUnits.Contains(selectable);

            if (!controlled && !shift)
            {
                ClearSelectedUnits();
                selectedUnits.Add(selectable);
                controllable = false;
            }
            else
            {
                if (!controllable) { ClearSelectedUnits(); controllable = true; }
                Selectable[] tempSelected = selectedUnits.ToArray();
                switch (ctrl, shift, preselected)
                {
                    case (true, true, true):
                        foreach (Selectable current in tempSelected)
                        {
                            if (selectable.GetType() == current.GetType())
                                SelectUnit(current, false);
                        }
                        break;
                    case (true, false, true):
                        foreach (Selectable current in tempSelected)
                        {
                            if (selectable.GetType() != current.GetType())
                                SelectUnit(current, false);
                        }
                        break;
                    case (true, true, false):
                        foreach (Selectable current in UnitsOnScreen())
                        {
                            if (selectable.GetType() == current.GetType() && UnitManager.IsMine(myID, current.teamID))
                                SelectUnit(current);
                        }
                        break;
                    case (true, false, false):
                        selectedUnits.Clear();
                        foreach (Selectable current in UnitsOnScreen())
                        {
                            if (selectable.GetType() == current.GetType() && UnitManager.IsMine(myID, current.teamID))
                                SelectUnit(current);
                        }
                        break;
                    case (false, true, true):
                        SelectUnit(selectable, false);
                        break;
                    case (false, true, false):
                        SelectUnit(selectable);
                        break;
                    case (false, false, _):
                        selectedUnits.Clear();
                        SelectUnit(selectable);
                        break;
                }
            }
        }
    }

    void DragSelect()
    {
        bool shift = Input.GetKey(KeyCode.LeftShift);
        
        if (!shift && hoveredUnits.Count > 0) ClearSelectedUnits();
        foreach (Selectable selectable in hoveredUnits)
        {
            bool controlled = UnitManager.IsMine(myID, selectable.teamID);

            switch (shift, controlled)
            {
                case (true, true):
                    if (!controllable) { ClearSelectedUnits(); controllable = true; }
                    SelectUnit(selectable);
                    HoverUnit(selectable, false);
                    break;
                case (false, true):
                    if (!controllable) { ClearSelectedUnits(); break; }
                    SelectUnit(selectable);
                    controllable = true;
                    break;
                case (true, false):
                    HoverUnit(selectable, false);
                    break;
            }
        }
    }

    void DrawSelection()
    {
        if (Input.mousePosition.x < startPosition.x)
        {
            selectionBox.xMin = Input.mousePosition.x;
            selectionBox.xMax = startPosition.x;
        }
        else
        {
            selectionBox.xMin = startPosition.x;
            selectionBox.xMax = Input.mousePosition.x;
        }


        if (Input.mousePosition.y < startPosition.y)
        {
            selectionBox.yMin = Input.mousePosition.y;
            selectionBox.yMax = startPosition.y;
        }
        else
        {
            selectionBox.yMin = startPosition.y;
            selectionBox.yMax = Input.mousePosition.y;
        }
    }

    List<Selectable> UnitsInDragSelect()
    {
        List<Selectable> list = new List<Selectable>();
        foreach (GameObject unit in UnitManager.Instance.allUnits)
        {
            if (!UnitManager.IsMine(myID, unit.GetComponent<Selectable>().teamID))
                continue;
            if (selectionBox.Contains(cam.WorldToScreenPoint(unit.transform.position)))
            {
                list.Add(unit.GetComponent<Selectable>());
            }
        }
        return list;
    }

    List<Selectable> UnitsOnScreen()
    {
        List<Selectable> list = new List<Selectable>();
        foreach (GameObject unit in UnitManager.Instance.allUnits)
        {
            Vector3 vp = cam.WorldToViewportPoint(unit.transform.position);

            if (vp.z > 0f && vp.x > 0f && vp.x < 1f && vp.y > 0f && vp.y < 1f)
            {
                list.Add(unit.GetComponent<Selectable>());
            }
        }
        return list;
    }

    //2 is Selected, 1 is Highlighted, 0 is unselected
    private void SelectUnit(Selectable selectable, bool isSelected = true)
    {
        if (isSelected)
            selectedUnits.Add(selectable);
        else
            selectedUnits.Remove(selectable);

        selectable.SetSelectionIndicator(isSelected);
    }

    private void HoverUnit(Selectable selectable, bool isHovered = true)
    {
        if (isHovered)
            hoveredUnits.Add(selectable);
        else
            hoveredUnits.Remove(selectable);

        selectable.SetHoverIndicator(isHovered);
    }

    private void ClearSelectedUnits()
    {
        foreach(Selectable selectable in selectedUnits)
        {
            selectable.SetSelectionIndicator(false);
        }
        selectedUnits.Clear();
    }

    //Visuals

    void DrawVisual(bool draw = true)
    {
        if (draw) DrawVisual(startPosition, endPosition);
        else DrawVisual(Vector2.zero, Vector2.zero);
    }

    void DrawVisual(Vector2 start, Vector2 end)
    {
        boxVisual.position = (start + end) / 2;
        boxVisual.sizeDelta = new Vector2(Mathf.Abs(start.x - end.x), Mathf.Abs(start.y - end.y));
    }
}
