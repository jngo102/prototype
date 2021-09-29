using UnityEngine;

public class Bench : MonoBehaviour, IInteractHander
{
    [SerializeField]
    private Animator _label;

    public void OnInteractionEnter(InteractInfo info)
    {
        _label.SetTrigger("Show");
    }

    public void OnInteractionExit(InteractInfo info)
    {
        _label.SetTrigger("Hide");
    }

    public void OnInteractionTrigger(InteractInfo info)
    {
        SendMessageUpwards(
            PlayerSaveMessage.Name,
            new PlayerSaveMessage()
        );
    }
}