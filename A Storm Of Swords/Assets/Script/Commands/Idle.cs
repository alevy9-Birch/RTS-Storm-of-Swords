using NUnit.Framework.Internal;
using System.Transactions;
using UnityEngine;

[CreateAssetMenu(fileName = "IdleCommand", menuName = "Commands/Idle")]
public class Idle : Command
{
    public override void FirstFrame()
    {
        base.FirstFrame();
        if (selectable is Unit)
        {
            //((Unit)selectable).MoveTo(selectable.transform.position);
        }
        else selectable.NextCommand();
    }

    public override void Execute()
    {
        if (selectable.queue.Count > 0)
            selectable.NextCommand();
    }
}
