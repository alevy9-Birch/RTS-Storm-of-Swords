using UnityEngine;

public class ReplaceMat : MonoBehaviour
{

    public string MatName;
    
    private void Start()
    {
        MeshRenderer mr = GetComponent<MeshRenderer>();
        mr.material = UnitManager.Instance.GetMat(GetComponent<Selectable>().teamID, MatName);
        Destroy(this);
    }
}
