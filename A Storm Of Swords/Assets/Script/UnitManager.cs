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
    public MaterialGroup[] materialGroups = new MaterialGroup[maxPlayers];

    public LayerMask ground;
    public LayerMask selectable;

    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else Destroy(gameObject);

        for (byte i = 0; i < maxPlayers; i++) { SetRelation(i, i, 2); }
        for (int i = 0; i < materialGroups.Length; i++) materialGroups[i].Initialize();
    }

    private void Update()
    {
        foreach (MaterialGroup matGroup in Instance.materialGroups)
        {
            matGroup.setTeamRelationsColor();
        }
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

    public static bool IsMine(byte a, byte b)
    {
        return a == b;
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

        foreach (MaterialGroup matGroup in Instance.materialGroups)
        {
            matGroup.setTeamRelationsColor();
        }
    }

    public static void RecalculateTeamColors()
    {
        foreach (GameObject unit in Instance.allUnits)
        {
            if (unit == null)
            {
                Debug.LogError("UnitManager: A null entry exists inside allUnits.");
                continue;
            }

            var sel = unit.GetComponent<Selectable>();
            if (sel == null)
            {
                Debug.LogError($"UnitManager: {unit.name} has no Selectable component.");
                continue;
            }

            sel.InitializeSelectionIndicators();
        }
    }

    public Material GetMat(byte teamID, string matName)
    {
        return materialGroups[teamID].GetMaterial(matName);
    }

    public Color GetTeamColor(byte teamID)
    {
        return materialGroups[teamID].myColor;
    }

    [System.Serializable]
    public struct MaterialGroup
    {
        public MaterialLibrary matLibrary;
        public Dictionary<string, Material> dict;
        public byte myTeam;
        public Color myColor;

        public void Initialize()
        {
            dict = new Dictionary<string, Material>();

            foreach (IntMaterialPair pair in matLibrary.matPairs)
            {
                dict.Add(pair.name, new Material(pair.value));
            }

            setColors();
            setTeamRelationsColor();
        }

        public void setColors()
        {
            foreach (Material mat in dict.Values)
            {
                if (mat.HasProperty("_TeamColor"))
                {
                    mat.SetColor("_TeamColor", myColor);
                }
            }
        }

        public void setTeamRelationsColor()
        {
            Color color = Color.white;

            if (IsMine(Player.PlayerInstance.myID, myTeam)) color = Color.green;
            else if (IsAllied(Player.PlayerInstance.myID, myTeam)) color = Color.yellow;
            else if (IsEnemy(Player.PlayerInstance.myID, myTeam)) color = Color.red;

            dict["UnitSelectionCircle"].color = color;
            dict["UnitSelectionBorder"].color = color;
        }

        public Material GetMaterial(string key)
        {
            return dict[key];
        }
    }

    [System.Serializable]
    public struct IntMaterialPair
    {
        public string name;
        public Material value;
    }
}
