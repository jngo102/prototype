using UnityEngine;

public class Bench : MonoBehaviour, IInteractEnterHander, IInteractTriggerHandler, IInteractExitHander
{
    [SerializeField]
    private Animator _label;

    public void OnInteractionEnter(InteractInfo info)
    {
        _label.SetTrigger("Show");
    }

    public void OnInteractionTrigger(InteractInfo info)
    {
        // ................
        Debug.Log("Interact!");
    }

    public void OnInteractionExit(InteractInfo info)
    {
        _label.SetTrigger("Hide");
    }
}
