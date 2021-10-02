using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    private string _saveLevel;
    private Vector2 _savePoint;

    public void Start()
    {
        Main.Input.Player.Menu.performed += OnMenu;

        Main.Hook.PlayerDeath += OnPlayerDeath;
        Main.Hook.PlayerRecovery += OnPlayerRecovery;
        Main.Hook.PlayerSave += OnPlayerSave;
        Main.Hook.PlayerTransit += OnPlayerTransit;
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

    private void OnPlayerTransit(PlayerTransitArgs message)
    {
        Open(new OpenArgs() {
            name = message.Level,
            transition = message
        });
    }

    private void OnPlayerRecovery(PlayerRecoveryArgs message) => StartCoroutine(OnRestoreMessageCoroutine(message));
    private IEnumerator OnRestoreMessageCoroutine(PlayerRecoveryArgs message)
    {
        yield return Main.UI.Get<UICurtain>().ShowAndWait();
        FindObjectOfType<Player>().SetPosition(message.Position);
        yield return Main.UI.Get<UICurtain>().HideAndWait();
    }

    private void OnPlayerDeath()
    {
        // Remove!
        Open(new OpenArgs() {
            name = _saveLevel,
            death = true
        });
    }

    private void OnPlayerSave()
    {
        _saveLevel = Main.Level.Name;
        _savePoint = FindObjectOfType<Player>().transform.position;
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
        else if (options.death)
        {
            PlayerHealth.Instance.InstantlyRestoreAllHealth();
            FindObjectOfType<Player>().SetPosition(_savePoint);
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

        public PlayerTransitArgs transition;
        public bool death;
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
