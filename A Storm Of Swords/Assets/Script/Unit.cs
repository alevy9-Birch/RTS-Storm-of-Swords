using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Unit : Selectable
{
    protected NavMeshAgent agent;
    const float rotationAcceleration = 5f;

    protected override void Start()
    {
        base.Start();
        agent = GetComponent<NavMeshAgent>();
    }

    public bool MoveTo(Vector3 targetPosition, bool autoRotation = true)
    {
        agent.updateRotation = autoRotation;
        agent.SetDestination(targetPosition);
        return GetPathStatus();
    }

    public void FaceDirection(Vector3 worldDirection)
    {
        if (agent == null) return;

        worldDirection.y = 0;
        if (worldDirection.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRot = Quaternion.LookRotation(worldDirection);

        float maxDegrees = agent.angularSpeed * Time.deltaTime;

        float smooth = rotationAcceleration > 0 ?
            Mathf.Min(1f, rotationAcceleration * Time.deltaTime) :
            1f;

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRot,
            maxDegrees * smooth
        );
    }

    public void FacePoint(Vector3 point)
    {
        FaceDirection(point - selectable.transform.position);
    }

    public bool GetPathStatus()
    {
        if (agent.pathStatus == NavMeshPathStatus.PathComplete)
            return true;
        else return false;
    }

    public float RemainingDistance()
    {
        return agent.remainingDistance;
    }
}
