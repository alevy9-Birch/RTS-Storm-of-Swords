using UnityEngine;

public class MapBounds : MonoBehaviour
{

    public static MapBounds Instance;
    public Vector2 mapBounds;
    public Camera mapCamera;

    [Header("Testing")]
    public Vector3 targetPosition;
    public Vector2 targetSize;
    public float mapHeight = 100f;
    public float wallThickness = 1f;
    public Transform[] borderWalls = new Transform[4];

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Instance == null)
            Instance = this;
        else Destroy(gameObject);

        targetSize = mapBounds;
        targetPosition = transform.position;
        UpdateBorders();
    }

    // Update is called once per frame
    void ReviseBorder(Vector3 position, Vector2 size)
    {
        targetPosition = position;
        targetSize = size;
    }

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime);
        mapBounds = Vector2.Lerp(mapBounds, targetSize, Time.deltaTime);
        mapBounds = Vector2.MoveTowards(mapBounds, targetSize, Time.deltaTime);
        mapCamera.orthographicSize = Mathf.Max(mapBounds.x, mapBounds.y);
        UpdateBorders();
    }

    void UpdateBorders()
    {
        if (borderWalls.Length < 4) return;

        // Left wall
        borderWalls[0].position = transform.position + new Vector3(-mapBounds.x, mapHeight / 2, 0);
        borderWalls[0].localScale = new Vector3(wallThickness, mapHeight, 2 * mapBounds.y);

        // Right wall
        borderWalls[1].position = transform.position + new Vector3(mapBounds.x, mapHeight / 2, 0);
        borderWalls[1].localScale = new Vector3(wallThickness, mapHeight, 2 * mapBounds.y);

        // Front wall (positive Z)
        borderWalls[2].position = transform.position + new Vector3(0, mapHeight / 2, mapBounds.y);
        borderWalls[2].localScale = new Vector3(2 * mapBounds.x, mapHeight, wallThickness);

        // Back wall (negative Z)
        borderWalls[3].position = transform.position + new Vector3(0, mapHeight / 2, -mapBounds.y);
        borderWalls[3].localScale = new Vector3(2 * mapBounds.x, mapHeight, wallThickness);
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + Vector3.up * mapHeight / 2, new Vector3(2 * mapBounds.x, mapHeight, 2 * mapBounds.y));
    }
}
