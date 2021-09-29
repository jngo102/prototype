using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public void Start()
    {
        Main.Input.Player.Menu.performed += OnMenu;

        Main.Hook.PlayerTransit += () => Debug.Log("Transit");
    }

    //
    // Public API
    //
    public void StartUp(string levelName)
    {
        Open(new OpenArgs() {
            name = levelName
        });
    }

    //
    // Message Handlers
    //

    private void OnLevelTransitionMessage(LevelTransitionMessage message)
    {
        Open(new OpenArgs() {
            name = message.Level,
            transition = message
        });
    }

    private void OnPlayerRecoveryMessage(PlayerRecoveryMessage message) => StartCoroutine(OnRestoreMessageCoroutine(message));
    private IEnumerator OnRestoreMessageCoroutine(PlayerRecoveryMessage message)
    {
        yield return Main.UI.Get<UICurtain>().ShowAndWait();
        FindObjectOfType<Player>().SetPosition(message.Position);
        yield return Main.UI.Get<UICurtain>().HideAndWait();
    }

    private void OnPlayerDeathMessage(PlayerDeathMessage message)
    {
        Debug.Log("Process DEATH");
    }

    private void OnPlayerSaveMessage(PlayerSaveMessage message)
    {
        Debug.Log("Process SAVE");
        PlayerHealth.Instance.InstantlyRestoreAllHealth();
    }

    private void OnMenu(InputAction.CallbackContext context)
    {
        Close();
    }

    //
    // ...
    //
    private void Open(OpenArgs options) => StartCoroutine(OpenCoroutine(options));
    private IEnumerator OpenCoroutine(OpenArgs options)
    {
        Physics2D.simulationMode = SimulationMode2D.Script;

        Main.Input.Player.Enable();
        yield return Main.UI.Get<UICurtain>().ShowAndWait();
        yield return Main.Level.Load(options.name);

        // ... initialize level ...
        FindObjectOfType<LevelMessageBus>().Listener = this;

        // ... initialize player ...
        if (options.transition != null)
        {
            var gate = FindObjectsOfType<LevelGate>().FirstOrDefault(x => x.name == options.transition.Gate);
            if (gate == null)
                throw new Exception();

            gate.Place(
                FindObjectOfType<Player>(),
                options.transition
            );
        }

        FindObjectOfType<PlayerCameraController>().Setup(
            Camera.main,
            FindObjectOfType<Player>(),
            FindObjectOfType<LevelBounds>()
        );

        Main.UI.Get<HUD>().Show();

        Physics2D.simulationMode = SimulationMode2D.FixedUpdate;
        yield return Main.UI.Get<UICurtain>().HideAndWait();
    }

    private struct OpenArgs
    {
        public string name;

        public LevelTransitionMessage transition;
    }

    private void Close() => StartCoroutine(CloseCoroutine());
    private IEnumerator CloseCoroutine()
    {
        Main.Input.Player.Disable();
        yield return Main.UI.Get<UICurtain>().ShowAndWait();
        yield return Main.Level.Unload();
        Main.UI.Get<UIMenu>().Show();
        yield return Main.UI.Get<UICurtain>().HideAndWait();
    }
}
