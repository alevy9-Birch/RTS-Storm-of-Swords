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

    protected override void Start()
    {
        base.Start();
        agent = GetComponent<NavMeshAgent>();
        followCollider = GetComponent<SphereCollider>();
    }
}
