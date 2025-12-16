using UnityEngine;

[CreateAssetMenu(fileName = "MoveCommand", menuName = "Commands/Move")]
public class Move : Command
{
    public override void FirstFrame()
    {
        base.FirstFrame();
        if (selectable is Unit)
        {
            if (targetUnit != null) ((Unit)selectable).MoveTo(targetUnit.gameObject.transform.position);
            else ((Unit)selectable).MoveTo(targetLocation);
        }
        else selectable.NextCommand();
    }

    public override void Execute()
    {
        if (((Unit)selectable).ReachedDestination())
        {
            Debug.Log("Destination Reached");
            selectable.NextCommand();
        }
    }
}
