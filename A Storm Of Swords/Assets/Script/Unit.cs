using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Unit : Selectable
{
    protected NavMeshAgent agent;
    protected SphereCollider followCollider;

    public float speed = 1;
    public float followRange = 5f;
    public float attackInRange = 1f;
    public float attackOutOfRange = 1.2f;

    public int teamNumber = 0;

    [SerializeField]
    private bool drawGizmos = false;

    private List<GameObject> SelectablesInRange = new List<GameObject>();
    private LayerMask SelectableLayers;

    protected new virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        followCollider = GetComponent<SphereCollider>();
        SelectableLayers = LayerMask.NameToLayer("Selectable");

        base.inputManager = new InputManager();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == SelectableLayers)
        {
            SelectablesInRange.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == SelectableLayers)
        {
            SelectablesInRange.Remove(other.gameObject);
        }
    }
    
    //Gizmos

    private void OnDrawGizmos()
    {
        if (drawGizmos)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, followRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackInRange);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, attackOutOfRange);

        }
    }

    //Input Manager
    protected new class InputManager : Selectable.InputManager
    {
        protected static bool a = false;
        

        public override void CheckInput()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                a = !a;
            }
            
            if (Input.GetMouseButtonDown(2))
            {
                if (InputManager.hit.collider.gameObject.layer == InputManager.ground)
                {
                    if (a)
                    {
                        if (Input.GetKey(KeyCode.LeftShift))
                        {

                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        if (Input.GetKey(KeyCode.LeftShift))
                        {

                        }
                        else
                        {

                        }
                    }
                }
                else if (InputManager.hit.collider.gameObject.layer == InputManager.selectable)
                {
                    if (a)
                    {
                        if (Input.GetKey(KeyCode.LeftShift))
                        {

                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        if (Input.GetKey(KeyCode.LeftShift))
                        {

                        }
                        else
                        {

                        }
                    }
                }
            }
            
        }
    }
}
