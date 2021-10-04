using UnityEngine;

public class Bench : MonoBehaviour, IInteractionHander, IEntrance
{
    [SerializeField] private Animator _label;
    [SerializeField] private ParticleSystem _fxIdle;
    [SerializeField] private ParticleSystem _fxActivate;

    private bool _active;

    public void OnInteractionEnter(InteractionInfo info)
    {
        // info.Type = InteractionType.Bench;
        // info.BenchPoint = transform.position;

        // In case Place() call
        if (_active)
            return;

        _label.SetBool("Show", true);
        _fxIdle.Play();
    }

    public void OnInteractionExit(InteractionInfo info)
    {
        _label.SetBool("Show", false);
        _fxIdle.Stop();
    }

    public void OnInteractionTrigger(InteractionInfo info)
    {
        _active = !_active;

        if (_active)
        {
            _label.SetBool("Show", false);
            _fxActivate.gameObject.SetActive(true);
            Main.Hook.PlayerSave.Invoke();
            PlayerCameraController.Instance.Shake();
        }
    }

    public void Place(Player player)
    {
        player.Setup(transform.position + 0.5f*Vector3.up, 0, true, true);
        _active = true;
        _fxIdle.Play();
    }
}