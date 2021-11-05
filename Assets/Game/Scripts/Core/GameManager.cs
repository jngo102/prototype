using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    private ISaveSlot _slot;
    private SaveString _saveLevel;
    private SaveString _saveEntrance;

    private void Start()
    {
        Main.Input.Player.Menu.performed += OnPause;

        Main.Hook.PlayerDeath += OnPlayerDeath;
        Main.Hook.PlayerRecovery += OnPlayerRecovery;
        Main.Hook.PlayerSave += OnPlayerSave;
        Main.Hook.PlayerTransit += OnPlayerTransit;
    }

    //
    // Public API
    //

    public void Play(string levelName)
    {
        _slot = new PlayerPrefsSaveSlot(null);

        _saveLevel = _slot.GetString("SaveLevel", SaveMode.Persistent, SaveScope.Global);
        _saveEntrance = _slot.GetString("SaveEntrance", SaveMode.Persistent, SaveScope.Global);

        Open(new OpenArgs() {
            name = levelName
        });
    }

    public YieldInstruction Stop()
    {
        return Close();
    }

    public void Pause()
    {
        Time.timeScale = 0;
        Main.Input.Player.Disable();
    }

    public void Unpause()
    {
        if (Time.timeScale == 0)
        {
            Time.timeScale = 1;
            Main.Input.Player.Enable();
        }
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
        yield return Main.UI.Get<UICurtain>().Show();
        FindObjectOfType<Player>().Setup(message.Position, 0, true);
        yield return Main.UI.Get<UICurtain>().Hide();
    }

    private void OnPlayerDeath()
    {
        PlayerHealth.Instance.InstantlyRestoreAllHealth();

        Open(new OpenArgs() {
            name = _saveLevel.Value,
            entranceName = _saveEntrance.Value
        });
    }

    private void OnPlayerSave(PlayerSaveArgs message)
    {
        _saveLevel.Write(Main.Level.Name);
        _saveEntrance.Write(message.Entrance);
        _slot.Flush();

        PlayerHealth.Instance.InstantlyRestoreAllHealth();
    }

    private void OnPause(InputAction.CallbackContext context)
    {
        Main.UI.Get<UIPause>().Show();
    }

    //
    // Open/Close
    //
    private void Open(OpenArgs args) => StartCoroutine(OpenCoroutine(args));
    private IEnumerator OpenCoroutine(OpenArgs args)
    {
        Main.Input.Player.Enable();
        yield return Main.UI.Get<UICurtain>().Show();
        yield return Main.Level.Load(args.name);

        // ... initialize level ...
        if (_saveLevel == null)
        {
            _saveLevel.Write(args.name);
            _saveEntrance.Write(FindObjectOfType<Bench>()?.name);
        }

        var actors = FindObjectsOfType<Actor>().ToList();
        foreach (var actor in actors)
        {
            var items = actor.GetHandlers<ILoadable>();
            if (items.Length > 0)
            {
                _slot.BeginScope(actor.GetPath());
                
                foreach (var item in items)
                    item.Load(_slot);

                _slot.EndScope();
            }
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
        yield return Main.UI.Get<UICurtain>().Hide();
    }

    private struct OpenArgs
    {
        public string name;

        public string entranceName;
        public EntranceInfo entranceInfo;
    }

    private YieldInstruction Close() => StartCoroutine(CloseCoroutine());
    private IEnumerator CloseCoroutine()
    {
        Pause();
        yield return Main.UI.Get<UICurtain>().Show();
        yield return Main.Level.Unload();
        Unpause();

        Main.UI.Get<UIMenu>().Show();
        Main.UI.Get<UICurtain>().Hide();

        Main.Input.Player.Disable();
    }
}
