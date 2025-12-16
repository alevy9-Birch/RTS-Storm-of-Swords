using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//This Script is a merging of Input Manager and UnitSelectionManager
public class Player : MonoBehaviour
{
    public static Player PlayerInstance { get; private set; }
    
    [Header("References")]
    public Camera cam;
    Vector3 mousePosition;
    public RaycastHit hit;

    public LayerMask groundMask;
    public LayerMask selectableMask;

    protected Coroutine activeInputs;

    [Range(10f, 50f)]
    public float boxMin = 10;

    [Header("Player Specific")]

    [Range(0, UnitManager.maxPlayers - 1)]
    public byte myID;
    public List<Selectable> selectedUnits = new List<Selectable>();
    public List<Selectable> hoveredUnits = new List<Selectable>();
    bool controllable = true;

    public Command targetingCommand = null;

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
        if (hit.collider == null)
        {
            Debug.Log("Mouse Hits Nothing");
            return;
        }

        canFollow = selectedUnits.Count > 0;
        if (canFollow) followPosition = selectedUnits[0].transform.position;

        if (targetingCommand != null) TargetCommand();
        else DefaultInputs();

    }

    void TargetCommand() //Find Target before Passing to Selectables
    {
        DrawVisual(false);
        if (Input.GetMouseButtonUp(0))
        {
            if (IsInLayerMask(hit.collider.gameObject, selectableMask))
            {
                targetingCommand.SetTarget(hit.collider.gameObject.GetComponent<Selectable>());
                HandleCommand(targetingCommand);
            }
            else if (IsInLayerMask(hit.collider.gameObject, groundMask))
            {
                targetingCommand.SetTarget(hit.point);
                HandleCommand(targetingCommand);
            }
            else
            {
                CancelTargeting();
            }
        }
        else
        {
            //Cancel Targetting
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyUp(KeyCode.LeftShift)) CancelTargeting();
        }
    }

    void CallCommand(Command command, bool preset = false) //Call a Command In Player
    {
        if (preset)
        {
            targetingCommand = null;
            if (IsInLayerMask(hit.collider.gameObject, selectableMask))
            {
                if (command.SetTarget(hit.collider.gameObject.GetComponent<Selectable>())) HandleCommand(command, preset);
            }
            else if (IsInLayerMask(hit.collider.gameObject, groundMask))
            {
                if (command.SetTarget(hit.point)) HandleCommand(command, preset);
            }
        }
        else if (command.needsTargeting)
        {
            targetingCommand = command;
        }
        else
        {
            HandleCommand(command);
        }
    }

    void HandleCommand(Command command, bool preset = false) //Find All Units In Selection with Command and Assign It
    {
        worldStartPos = hit.point;
        startPosition = Camera.main.WorldToScreenPoint(worldStartPos);

        foreach (Selectable selectable in selectedUnits.ToArray())
        {
            if (selectable == null)
            {
                selectedUnits.Remove(selectable);
                continue;
            }

            Command newCommand = selectable.GenerateCommand(command.abilityName);
            if (newCommand != null)
            {
                if (command.instantApplication)
                {
                    selectable.InsertCommand(newCommand);
                }

                if (Input.GetKey(KeyCode.LeftShift)) selectable.AddCommand(newCommand);
                else selectable.OverrideCommand(newCommand);

                if (command.singleUnitCommand)
                {
                    SelectUnit(selectable, false);
                    SelectUnit(selectable, true);
                    break;
                }
            }

            command.selectable = selectable;
        }
        if (Input.GetKey(KeyCode.LeftShift) && !preset && command.needsTargeting)
        {
            CallCommand(command);
        }
        else
        {
            targetingCommand = null;
        }
    }

    void CancelTargeting()
    {
        targetingCommand = null;
        //Debug.Log("Cancelled Command");
    }

    public void DefaultInputs()
    {
        Vector2 box = (endPosition - startPosition);
        bool dragging = Mathf.Abs(box.x) > boxMin || Mathf.Abs(box.y) > boxMin;

        if (Input.GetMouseButtonDown(0))
        {
            worldStartPos = hit.point;
            startPosition = Camera.main.WorldToScreenPoint(worldStartPos);
        }

        if (Input.GetMouseButton(0))
        {
            startPosition = Camera.main.WorldToScreenPoint(worldStartPos);
            endPosition = Input.mousePosition;


            if (dragging)
            {
                DrawSelection();
                DrawVisual(true);
                
                List<Selectable> inDrag = UnitsInDragSelect();
                foreach (Selectable selectable in hoveredUnits.ToArray())
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
                ClearHoveredUnits();
                DrawVisual(false);

            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (dragging)
            {
                DragSelect();
            }
            else
            {
                DefaultClick();
            }

            ClearHoveredUnits();

            startPosition = Vector2.zero;
            endPosition = Vector2.zero;
            DrawVisual(false);
        }

        if (controllable && selectedUnits.Count > 0)
        {
            if (Input.GetMouseButtonDown(1))
            {
                CallCommand(selectedUnits[0].defaultRightClick, true);
                return;
            }
            foreach (Command command in selectedUnits[0].commands)
            {
                if (command.keyboardShortcut == KeyCode.None) continue;
                if (Input.GetKeyDown(command.keyboardShortcut))
                {
                    CallCommand(command);
                    return;
                }
            }
        }
    }

    public bool IsInLayerMask(GameObject obj, LayerMask mask)
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

            if (!controlled)
            {
                if (!shift)
                {
                    ClearSelectedUnits();
                    SelectUnit(selectable);
                    controllable = false;
                }
            }
            else
            {
                if (!controllable) { ClearSelectedUnits(); controllable = true; }
                switch (ctrl, shift, preselected)
                {
                    case (true, true, true): //Unselect Type
                        foreach (Selectable current in selectedUnits.ToArray())
                        {
                            if (selectable.unitType == current.unitType)
                                SelectUnit(current, false);
                        }
                        break;
                    case (false, true, true): //Unselect Unit
                        SelectUnit(selectable, false);
                        break;
                    case (true, false, true): //Select Type from Selected
                        foreach (Selectable current in selectedUnits.ToArray())
                        {
                            if (selectable.unitType != current.unitType)
                                SelectUnit(current, false);
                        }
                        break;
                    case (true, true, false): //Add Type
                        foreach (Selectable current in UnitsOnScreen())
                        {
                            if (selectable.unitType == current.unitType && UnitManager.IsMine(myID, current.teamID))
                                SelectUnit(current);
                        }
                        break;
                    case (true, false, false): //Select Type
                        ClearSelectedUnits();
                        foreach (Selectable current in UnitsOnScreen())
                        {
                            if (selectable.unitType == current.unitType && UnitManager.IsMine(myID, current.teamID))
                                SelectUnit(current);
                        }
                        break;
                    case (false, true, false): //Add Unit
                        SelectUnit(selectable);
                        break;
                    case (false, false, _): //Select Unit
                        ClearSelectedUnits();
                        SelectUnit(selectable);
                        break;
                }
            }
        }
    }

    void DragSelect()
    {
        bool shift = Input.GetKey(KeyCode.LeftShift);
        
        foreach (Selectable selectable in hoveredUnits.ToArray()) //Clear if Shift isn't held and update Controllable
        {
            bool controlled = UnitManager.IsMine(myID, selectable.teamID);

            if (!controllable)
            {
                if (controlled)
                {
                    ClearSelectedUnits();
                    controllable = true;
                }
            }
            if (controllable)
            {
                if (!shift) ClearSelectedUnits();
                break;
            }
        }

        foreach (Selectable selectable in hoveredUnits.ToArray())
        {
            bool controlled = UnitManager.IsMine(myID, selectable.teamID);

            switch (shift, controlled)
            {
                case (_, false):
                    HoverUnit(selectable, false);
                    break;
                case (true, _):
                    SelectUnit(selectable);
                    HoverUnit(selectable, false);
                    break;
                case (false, _):
                    SelectUnit(selectable);
                    controllable = true;
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

        //Debug.Log(selectionBox.width + " " + selectionBox.height);
    }

    List<Selectable> UnitsInDragSelect()
    {
        List<Selectable> list = new List<Selectable>();
        foreach (GameObject unit in UnitManager.Instance.allUnits)
        {
            Bounds b = unit.GetComponent<Collider>().bounds;

            // convert bounds min/max to screen rect
            Vector3 min = cam.WorldToScreenPoint(b.min);
            Vector3 max = cam.WorldToScreenPoint(b.max);

            Rect screenRect = Rect.MinMaxRect(
                Mathf.Min(min.x, max.x),
                Mathf.Min(min.y, max.y),
                Mathf.Max(min.x, max.x),
                Mathf.Max(min.y, max.y)
            );

            if (selectionBox.Overlaps(screenRect, true))
            {
                list.Add(unit.GetComponent<Selectable>());
            }
        }
        return list;
    }

    //New Version, needs testing
    List<Selectable> UnitsOnScreen()
    {
        List<Selectable> list = new List<Selectable>();

        foreach (GameObject unit in UnitManager.Instance.allUnits)
        {
            Collider c = unit.GetComponent<Collider>();
            if (!c) continue;

            Bounds b = c.bounds;

            Vector3 min = cam.WorldToViewportPoint(b.min);
            Vector3 max = cam.WorldToViewportPoint(b.max);

            bool onScreen =
                (min.z > 0 || max.z > 0) &&   // in front of camera
                (max.x >= 0 && min.x <= 1) && // intersects horizontally
                (max.y >= 0 && min.y <= 1);   // intersects vertically

            if (onScreen)
                list.Add(unit.GetComponent<Selectable>());
        }

        return list;
    }

    //2 is Selected, 1 is Highlighted, 0 is unselected
    private void SelectUnit(Selectable selectable, bool isSelected = true)
    {
        if (isSelected)
        {
            if (!selectedUnits.Contains(selectable))
            {
                selectedUnits.Add(selectable);
            }
        }
        else
            selectedUnits.Remove(selectable);

        selectable.SetSelectionIndicator(isSelected);
    }

    private void HoverUnit(Selectable selectable, bool isHovered = true)
    {
        if (isHovered)
        {
            if (!hoveredUnits.Contains(selectable))
            {
                hoveredUnits.Add(selectable);
            }
        }
        else
            hoveredUnits.Remove(selectable);

        selectable.SetHoverIndicator(isHovered);
    }

    private void ClearSelectedUnits()
    {
        foreach(Selectable selectable in selectedUnits.ToArray())
        {
            SelectUnit(selectable, false);
        }
    }

    private void ClearHoveredUnits()
    {
        foreach (Selectable selectable in hoveredUnits.ToArray())
        {
            HoverUnit(selectable, false);
        }
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(hit.point, 1f);
    }
}
