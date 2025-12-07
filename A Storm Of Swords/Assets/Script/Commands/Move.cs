using UnityEngine;

[CreateAssetMenu(fileName = "MoveCommand", menuName = "Commands/Move")]
public class Move : Command
{
    public override void FirstFrame()
    {
        base.FirstFrame();
        if (selectable is Unit)
        {
            if (targetLocation != null) ((Unit)selectable).MoveTo(targetLocation);
            if (targetUnit != null) ((Unit)selectable).MoveTo(targetUnit.gameObject.transform.position);
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
