using NUnit.Framework.Internal;
using System.Transactions;
using UnityEngine;

[CreateAssetMenu(fileName = "IdleCommand", menuName = "Commands/Idle")]
public class Idle : Command
{
    public override void Execute()
    {
        if (selectable.queue.Count > 0)
            selectable.NextCommand();
    }
}

[CreateAssetMenu(fileName = "MoveCommand", menuName = "Commands/Move")]
public class Move : Command
{
    public Vector3 destination;
    public float speed;
    public bool flying = false;
    
    public override void Execute()
    {
        if (selectable.queue.Count > 0)
            selectable.NextCommand();
    }
}
