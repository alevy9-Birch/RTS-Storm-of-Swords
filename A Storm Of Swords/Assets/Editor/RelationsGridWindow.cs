using UnityEngine;
using UnityEditor;

public class RelationsGridWindow : EditorWindow
{
    private UnitManager unitManager;
    private int[,] tempGrid; // For easier 2D access
    private int width, height;
    private float cellSize = 10f;
    private float spacing = 2f;

    [MenuItem("Window/Relations Grid")]
    public static void OpenWindow()
    {
        GetWindow<RelationsGridWindow>("Relations Grid");
    }

    private void OnEnable()
    {
        unitManager = FindObjectOfType<UnitManager>();
        if (unitManager != null)
        {
            width = height = UnitManager.maxPlayers;
            tempGrid = new int[width, height];

            // Copy values from 1D array to 2D temp grid
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    tempGrid[x, y] = unitManager.relationsMask[y * width + x];
        }
    }

    private void SyncTempGrid()
    {
        if (unitManager == null) return;

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                tempGrid[x, y] = unitManager.relationsMask[y * width + x];
    }

    private void OnGUI()
    {
        if (unitManager == null)
        {
            EditorGUILayout.HelpBox("No UnitManager found in the scene!", MessageType.Warning);
            if (GUILayout.Button("Refresh"))
                OnEnable();
            return;
        }

        SyncTempGrid(); // <-- Keep tempGrid updated with UnitManager

        EditorGUILayout.LabelField("Relations Grid", EditorStyles.boldLabel);

        // Draw the grid
        for (int y = 0; y < height; y++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < width; x++)
            {
                Color cellColor = Color.white;
                switch (tempGrid[x, y])
                {
                    case -1: cellColor = Color.red; break;
                    case 0: cellColor = Color.white; break;
                    case 1: cellColor = Color.green; break;
                    case 2: cellColor = Color.cyan; break;
                    default: cellColor = Color.yellow; break;
                }

                GUI.backgroundColor = cellColor;
                if (GUILayout.Button("", GUILayout.Width(cellSize), GUILayout.Height(cellSize)))
                {
                    if (tempGrid[x, y] == 2)
                    {

                    }
                    else
                    {
                        // Cycle through -1, 0, 1
                        tempGrid[x, y] = (tempGrid[x, y] + 2) % 3 - 1;

                        // Update the UnitManager array
                        unitManager.relationsMask[y * width + x] = tempGrid[x, y];
                        EditorUtility.SetDirty(unitManager);
                    }
                }

                GUI.backgroundColor = Color.white; // Reset after each button
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(spacing);
        }
    }
}
