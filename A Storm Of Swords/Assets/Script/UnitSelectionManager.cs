using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitSelectionManager : MonoBehaviour
{
    public static UnitSelectionManager Instance { get; set; }

    public List<GameObject> allUnitsList = new List<GameObject>();
    public List<GameObject> unitsSelected = new List<GameObject>();

    public LayerMask ground;
    public LayerMask selectable;

    private Camera cam;

    public int[] controlledFactions = new int[] { 0 };
    public int[] neutralFactions = new int[] {};
    public int[] enemyFactions = new int[] { 1 };

    [SerializeField]
    RectTransform boxVisual;

    Rect selectionBox;

    Vector2 startPosition;
    Vector2 endPosition;

    float startTime;


    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        cam = Camera.main;
        startPosition = Vector2.zero;
        endPosition = Vector2.zero;
        DrawVisual();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startPosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(0))
        {
            endPosition = Input.mousePosition;
            DrawVisual();
            DrawSelection();
        }

        if (Input.GetMouseButtonUp(0))
        {
            if ((boxVisual.rect.width > 10 || boxVisual.rect.height > 10))
            {
                SelectUnits();
            }
            else
            {
                DefaultSelection();
            }

            startPosition = Vector2.zero;
            endPosition = Vector2.zero;
            DrawVisual();
        }
    }

    void DefaultSelection()
    {
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        //If we are clicking clickable object
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, selectable))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                MultiSelect(hit.collider.gameObject);
            }
            else
            {
                SelectByClicking(hit.collider.gameObject);
            }
        }
        else //Not selecting clickable objects
        {
            if (Input.GetKey(KeyCode.LeftShift) == false)
            {
                DeselectAll();
            }
            else
            {

            }

        }
    }

    void MultiSelect(GameObject unit)
    {
        if (unitsSelected.Contains(unit) == false)
        {
            unitsSelected.Add(unit);
            SelectUnit(unit, true);
        }
        else
        {
            SelectUnit(unit, false);
            unitsSelected.Remove(unit);
        }
    }

    void SelectByClicking(GameObject unit)
    {
        DeselectAll();

        SelectUnit(unit, true);
    }

    void DeselectAll()
    {
        foreach (GameObject unit in unitsSelected)
        {
            Selectable.TriggerSelectionIndicator(unit, false);
        }

        unitsSelected.Clear();
    }

    void DragSelect(GameObject unit)
    {
        if (unitsSelected.Contains(unit) == false)
        {
            SelectUnit(unit, true);
        }
    }

    private void SelectUnit(GameObject unit, bool isSelected)
    {
        if (isSelected)
            unitsSelected.Add(unit);
        else
            unitsSelected.Remove(unit);
        
        Selectable.TriggerSelectionIndicator(unit, isSelected);
    }

    void DrawVisual()
    {
        // Calculate the starting and ending positions of the selection box.
        Vector2 boxStart = startPosition;
        Vector2 boxEnd = endPosition;

        // Calculate the center of the selection box.
        Vector2 boxCenter = (boxStart + boxEnd) / 2;

        // Set the position of the visual selection box based on its center.
        boxVisual.position = boxCenter;

        // Calculate the size of the selection box in both width and height.
        Vector2 boxSize = new Vector2(Mathf.Abs(boxStart.x - boxEnd.x), Mathf.Abs(boxStart.y - boxEnd.y));

        // Set the size of the visual selection box based on its calculated size.
        boxVisual.sizeDelta = boxSize;
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

    void SelectUnits()
    {
        if (!Input.GetKey(KeyCode.LeftShift))
        {
            UnitSelectionManager.Instance.DeselectAll();
        }

        foreach (GameObject unit in UnitSelectionManager.Instance.allUnitsList)
        {
            if (selectionBox.Contains(cam.WorldToScreenPoint(unit.transform.position)))
            {
                UnitSelectionManager.Instance.DragSelect(unit);
            }
        }
    }
}

