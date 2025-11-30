using UnityEngine;
using System.Collections.Generic;
using static UnitManager;

[CreateAssetMenu(fileName = "MaterialLibrary", menuName = "Game/Material Library")]
public class MaterialLibrary : ScriptableObject
{
    public List<IntMaterialPair> matPairs;
}