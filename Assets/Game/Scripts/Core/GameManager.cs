using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    private string _saveLevel;
    private string _saveEntrance;

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
            entranceName = message.Entrance,
            entranceInfo = message.EntranceInfo
        });
    }

    private void OnPlayerRecovery(PlayerRecoveryArgs message) => StartCoroutine(OnRestoreMessageCoroutine(message));
    private IEnumerator OnRestoreMessageCoroutine(PlayerRecoveryArgs message)
    {
        yield return Main.UI.Get<UICurtain>().ShowAndWait();
        FindObjectOfType<Player>().Setup(message.Position, 0, true);
        yield return Main.UI.Get<UICurtain>().HideAndWait();
    }

    private void OnPlayerDeath()
    {
        PlayerHealth.Instance.InstantlyRestoreAllHealth();

        Open(new OpenArgs() {
            name = _saveLevel,
            entranceName = _saveEntrance
        });
    }

    private void OnPlayerSave()
    {
        _saveLevel = Main.Level.Name;
        _saveEntrance = FindObjectOfType<Bench>().name;
        PlayerHealth.Instance.InstantlyRestoreAllHealth();
    }

    private void OnMenu(InputAction.CallbackContext context)
    {
        Close();
    }

    //
    // Open/Close
    //
    private void Open(OpenArgs args) => StartCoroutine(OpenCoroutine(args));
    private IEnumerator OpenCoroutine(OpenArgs args)
    {
        Main.Input.Player.Enable();
        yield return Main.UI.Get<UICurtain>().ShowAndWait();
        yield return Main.Level.Load(args.name);

        // ... initialize level ...
        if (_saveLevel == null)
        {
            _saveLevel = args.name;
            _saveEntrance = FindObjectOfType<Bench>().name;
        }

        // ... initialize player ...
        if (args.entranceName != null)
        {
            var entrance = FindObjectsOfType<MonoBehaviour>()
                .Where(x => x.name == args.entranceName)
                .OfType<IEntrance>()
                .FirstOrDefault();
            
            if (entrance == null)
                throw new Exception($"There is no entrance with name '{args.entranceName}'");
            
            entrance.Place(FindObjectOfType<Player>(), args.entranceInfo);
        }

        // FIXME: Remove
        FindObjectOfType<PlayerCameraController>().Setup(
            Camera.main,
            FindObjectOfType<Player>(),
            FindObjectOfType<LevelBounds>()
        );

        Main.UI.Get<HUD>().Show();
        yield return Main.UI.Get<UICurtain>().HideAndWait();
    }

    private struct OpenArgs
    {
        public string name;

        public string entranceName;
        public EntranceInfo entranceInfo;
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
