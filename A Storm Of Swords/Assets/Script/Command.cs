using UnityEngine;

public abstract class Command : MonoBehaviour
{
    public string abilityName;
    public Selectable selectable;

    public void Start()
    {
        selectable = gameObject.GetComponent<Selectable>();
        selectable.AddCommand(abilityName, this);

        CommandInitialize();
    }

    protected virtual void CommandInitialize() { }
    public abstract bool Execute();
}
