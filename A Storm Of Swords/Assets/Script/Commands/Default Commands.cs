using NUnit.Framework.Internal;
using System.Transactions;
using UnityEngine;

[CreateAssetMenu(fileName = "IdleCommand", menuName = "Commands/Idle")]
public class Idle : Command
{
    public override bool Execute()
    {
        return selectable.queue.Count > 0;
    }

    public override bool SetUp()
    {
        return true;
    }

    protected override void CopyTo(Command copy)
    {
        base.CopyTo(copy);
    }
}

[CreateAssetMenu(fileName = "MoveCommand", menuName = "Commands/Move")]
public class Move : Command
{
    public Vector3 destination;
    public float speed;
    public bool flying = false;
    
    public override bool Execute()
    {
        return selectable.queue.Count > 0;
    }

    public override bool SetUp()
    {
        return true;
    }

    protected override void CopyTo(Command copy)
    {
        if (copy is Move other)
        {
            other.destination = destination;
            other.speed = speed;
            other.flying = flying;
        }

        base.CopyTo(copy);
    }
}
