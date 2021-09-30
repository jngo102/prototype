using UnityEngine;

public class Bench : MonoBehaviour, IInteractionHander
{
    [SerializeField]
    private Animator _label;

    public void OnInteractionEnter(InteractionInfo info)
    {
        _label.SetTrigger("Show");
    }

    public void OnInteractionExit(InteractionInfo info)
    {
        _label.SetTrigger("Hide");
    }

    public void OnInteractionTrigger(InteractionInfo info)
    {
        Main.Hook.PlayerSave.Invoke();
    }
}