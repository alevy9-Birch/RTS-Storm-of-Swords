using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    
    static Camera cam;
    static Vector3 mousePosition;
    protected RaycastHit hit;

    protected LayerMask ground = LayerMask.GetMask("Ground");
    protected LayerMask selectable = LayerMask.GetMask("Selectable");

    protected bool shift = false;

    protected void Update()
    {
        mousePosition = Input.mousePosition;
        
        Ray ray = cam.ScreenPointToRay(mousePosition);
        Physics.Raycast(ray, out hit, Mathf.Infinity, ground | selectable);
    }
    public virtual void CheckInput()
    {
        GetInput();
    }

    public void GetInput()
    {
        shift = Input.GetKeyDown(KeyCode.LeftShift);
    }
}
