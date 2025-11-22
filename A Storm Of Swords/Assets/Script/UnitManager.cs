using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    [HideInInspector]
    public static UnitManager Instance;
    public const byte maxPlayers = 16;
    public List<GameObject> allUnits = new List<GameObject>();

    [SerializeField, Visualize(maxPlayers, maxPlayers, cubeSize: 28f, spacing: 2f, autoFit: true)]
    public bool[] alliesMask = new bool[maxPlayers * maxPlayers];
    [SerializeField, Visualize(maxPlayers, maxPlayers, cubeSize: 28f, spacing: 2f, autoFit: true)]
    public bool[] enemiesMask = new bool[maxPlayers * maxPlayers];


    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null) { Instance = this; }
        else Destroy(gameObject);
    }
}
