using UnityEngine;

public class Idle : Command
{
    public override bool Execute()
    {
        return selectable.queue.Count > 0;
    }
}
