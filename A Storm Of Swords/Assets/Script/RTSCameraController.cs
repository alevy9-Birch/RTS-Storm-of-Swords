using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.EventSystems;

public class RTSCameraController : MonoBehaviour
{
    public static RTSCameraController instance;

    [Header("General")]
    public bool followSelection;
    Vector3 newPosition;
    Vector3 dragStartPosition;
    Vector3 dragCurrentPosition;

    [Header("Optional Functionality")]
    [SerializeField] bool moveWithKeyboad;
    [SerializeField] bool moveWithEdgeScrolling;
    [SerializeField] bool moveWithMouseDrag;

    [Header("Keyboard Movement")]
    [SerializeField] float fastSpeed = 0.05f;
    [SerializeField] float normalSpeed = 0.01f;
    [SerializeField] float movementSensitivity = 1f; // Hardcoded Sensitivity
    float movementSpeed;

    [Header("Edge Scrolling Movement")]
    public float edgeSize = 50f;
    bool isCursorSet = false;
    public Texture2D cursorDefaultArrow;
    public Texture2D cursorArrowUp;
    public Texture2D cursorArrowDown;
    public Texture2D cursorArrowLeft;
    public Texture2D cursorArrowRight;
    public Texture2D cursorSelectArrow;

    [Header("Camera Zoom")]
    public Animator animator;
    public string stateName = "MyAnimation";
    public float scrollSensitivity = 0.2f;
    public float zoomSpeed = 1f;
    public float targetNormalizedTime = 0f;
    public float normalizedTime = 0f;

    CursorArrow currentCursor = CursorArrow.DEFAULT;

    [Header("Minimap Renderer")]
    public Camera myCam;
    public LineRenderer line;
    public LayerMask cornersMask;

    enum CursorArrow
    {
        UP,
        DOWN,
        LEFT,
        RIGHT,
        DEFAULT,
        Target
    }

    private void Start()
    {
        instance = this;

        newPosition = transform.position;

        movementSpeed = normalSpeed;
        Cursor.SetCursor(cursorDefaultArrow, Vector2.one * 64, CursorMode.Auto);

        //Camera Zoom
        animator.speed = 0f; // pause animator
        animator.Play(stateName, 0, 0f);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.F))
        {
            followSelection = !followSelection;
        }
        if (!isCursorSet)
        {
            if (Player.PlayerInstance.hit.collider != null && Player.PlayerInstance.IsInLayerMask(Player.PlayerInstance.hit.collider.gameObject, Player.PlayerInstance.groundMask))
                Cursor.SetCursor(cursorDefaultArrow, Vector2.one * 64, CursorMode.Auto);
            else
                Cursor.SetCursor(cursorSelectArrow, Vector2.one * 64, CursorMode.Auto);
        }

        HandleCameraZoom();
        HandleCameraMovement();
    }

    void LateUpdate()//Minimap
    {
        if (!myCam) return;

        Vector3[] points = new Vector3[4];

        points[0] = CastCornerRay(0f, 0f); // Bottom Left
        points[1] = CastCornerRay(1f, 0f); // Bottom Right
        points[2] = CastCornerRay(1f, 1f); // Top Right
        points[3] = CastCornerRay(0f, 1f); // Top Left

        // Draw rectangle
        line.SetPosition(0, points[0]);
        line.SetPosition(1, points[1]);
        line.SetPosition(2, points[2]);
        line.SetPosition(3, points[3]);
        line.SetPosition(4, points[0]);
    }

    Vector3 CastCornerRay(float vx, float vy)
    {
        Ray ray = myCam.ViewportPointToRay(new Vector3(vx, vy, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, 5000, cornersMask))
            return hit.point + Vector3.up * 5;
        return ray.origin + ray.direction * 5000;
    }

    void HandleCameraZoom()
    {
        float scroll = Input.mouseScrollDelta.y;

        targetNormalizedTime += scroll * scrollSensitivity * Time.deltaTime;
        targetNormalizedTime = Mathf.Clamp01(targetNormalizedTime);
        normalizedTime = Mathf.Lerp(normalizedTime, targetNormalizedTime, Time.deltaTime * zoomSpeed);

        animator.Play(stateName, 0, normalizedTime);
        animator.Update(0f); // force immediate evaluation
    }

    void HandleCameraMovement()
    {
        // Mouse Drag
        if (moveWithMouseDrag)
        {
            HandleMouseDragInput();
        }

        // Keyboard Control
        if (moveWithKeyboad)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                movementSpeed = fastSpeed;
            }
            else
            {
                movementSpeed = normalSpeed;
            }

            if (Input.GetKey(KeyCode.UpArrow))
            {
                newPosition += (transform.forward * movementSpeed);
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                newPosition += (transform.forward * -movementSpeed);
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                newPosition += (transform.right * movementSpeed);
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                newPosition += (transform.right * -movementSpeed);
            }
        }

        // Edge Scrolling
        if (moveWithEdgeScrolling)
        {
            // Targeting Command
            if (Player.PlayerInstance.targetingCommand != null)
            {
                ChangeCursor(CursorArrow.Target);
                isCursorSet = true;
            }

            // Move Right
            else if (Input.mousePosition.x > Screen.width - edgeSize)
            {
                newPosition += (transform.right * movementSpeed);
                ChangeCursor(CursorArrow.RIGHT);
                isCursorSet = true;
            }

            // Move Left
            else if (Input.mousePosition.x < edgeSize)
            {
                newPosition += (transform.right * -movementSpeed);
                ChangeCursor(CursorArrow.LEFT);
                isCursorSet = true;
            }

            // Move Up
            else if (Input.mousePosition.y > Screen.height - edgeSize)
            {
                newPosition += (transform.forward * movementSpeed);
                ChangeCursor(CursorArrow.UP);
                isCursorSet = true;
            }

            // Move Down
            else if (Input.mousePosition.y < edgeSize)
            {
                newPosition += (transform.forward * -movementSpeed);
                ChangeCursor(CursorArrow.DOWN);
                isCursorSet = true;
            }
            else
            {
                if (isCursorSet)
                {
                    ChangeCursor(CursorArrow.DEFAULT);
                    isCursorSet = false;
                }
            }
        }

        //Check Map Bounds
        Vector3 center = MapBounds.Instance.transform.position;
        Vector2 size = MapBounds.Instance.mapBounds;

        if (followSelection && Player.PlayerInstance.canFollow) //Follow is On
            newPosition = Player.PlayerInstance.followPosition;

        newPosition = new Vector3(
            Mathf.Clamp(newPosition.x, center.x - size.x, center.x + size.x),
            newPosition.y,
            Mathf.Clamp(newPosition.z, center.z - size.y, center.z + size.y)
        );

        //Move Camera
        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementSensitivity);
        Cursor.lockState = CursorLockMode.Confined;
    }

    private void ChangeCursor(CursorArrow newCursor)
    {
        // Only change cursor if its not the same cursor
        if (currentCursor != newCursor)
        {
            switch (newCursor)
            {
                case CursorArrow.UP:
                    Cursor.SetCursor(cursorArrowUp, Vector2.one * 64, CursorMode.Auto);
                    break;
                case CursorArrow.DOWN:
                    Cursor.SetCursor(cursorArrowDown, Vector2.one * 64, CursorMode.Auto);
                    break;
                case CursorArrow.LEFT:
                    Cursor.SetCursor(cursorArrowLeft, Vector2.one * 64, CursorMode.Auto);
                    break;
                case CursorArrow.RIGHT:
                    Cursor.SetCursor(cursorArrowRight, Vector2.one * 64, CursorMode.Auto);
                    break;
                case CursorArrow.Target:
                    Cursor.SetCursor(Player.PlayerInstance.targetingCommand.cursor, Vector2.one * 64, CursorMode.Auto);
                    break;
                case CursorArrow.DEFAULT:
                    Cursor.SetCursor(cursorDefaultArrow, Vector2.one * 64, CursorMode.Auto);
                    break;
            }

            currentCursor = newCursor;
        }
    }



    private void HandleMouseDragInput()
    {
        if (Input.GetMouseButtonDown(2) && EventSystem.current.IsPointerOverGameObject() == false)
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float entry;

            if (plane.Raycast(ray, out entry))
            {
                dragStartPosition = ray.GetPoint(entry);
            }
        }
        if (Input.GetMouseButton(2) && EventSystem.current.IsPointerOverGameObject() == false)
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float entry;

            if (plane.Raycast(ray, out entry))
            {
                dragCurrentPosition = ray.GetPoint(entry);
                if (Player.PlayerInstance)
                newPosition = transform.position + dragStartPosition - dragCurrentPosition;
            }
        }
    }
}