using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    [HideInInspector]
    public static UnitManager Instance;
    public const byte maxPlayers = 16;
    public List<GameObject> allUnits = new List<GameObject>();

    public int[] relationsMask = new int[maxPlayers * maxPlayers]; //-1 is Enemy, 0 is Neutral, 1 is Ally

    public LayerMask ground;
    public LayerMask selectable;


    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null) { Instance = this; }
        else Destroy(gameObject);

        for (byte i = 0; i < maxPlayers; i++) { SetRelation(i, i, 2); }
    }

    public static int GetIndex(byte a, byte b)
    {
        return a * maxPlayers + b;
    }

    public int GetValue(byte a, byte b)
    {
        int index = GetIndex(a, b);

        return UnitManager.Instance.relationsMask[index];
    }

    public static bool IsAllied(byte a, byte b)
    {
        int i1 = GetIndex(a, b);
        int i2 = GetIndex(b, a); // mirror

        int min = Mathf.Min(Instance.relationsMask[i1], Instance.relationsMask[i2]);
        return min == 1;
    }

    public static bool IsNeutral(byte a, byte b)
    {
        int i1 = GetIndex(a, b);
        int i2 = GetIndex(b, a); // mirror

        int min = Mathf.Min(Instance.relationsMask[i1], Instance.relationsMask[i2]);
        return min == 0;
    }

    public static bool IsEnemy(byte a, byte b)
    {
        int i1 = GetIndex(a, b);
        int i2 = GetIndex(b, a); // mirror

        int min = Mathf.Min(Instance.relationsMask[i1], Instance.relationsMask[i2]);
        return min == -1;
    }

    public static int GetRelation(byte a, byte b)
    {
        int index = GetIndex(a, b);

        return UnitManager.Instance.relationsMask[index];
    }

    public static void SetRelation(byte a, byte b, int relation)
    {
        if (relation < -1 || relation > 1 || a == b)
            return;
        
        int i1 = GetIndex(a, b);
        int i2 = GetIndex(b, a); // mirror

        if (relation < UnitManager.Instance.relationsMask[i1])
        {
            UnitManager.Instance.relationsMask[i1] = relation;
            UnitManager.Instance.relationsMask[i2] = relation;
        }
        else
        {
            UnitManager.Instance.relationsMask[i1] = relation;
        }
    }
}
