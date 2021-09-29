using System;
using System.IO;
using System.Web;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour
{
    public static Main Instance { get; private set; }
    public static UIManager UI { get; private set; }
    public static LevelManager Level { get; private set; }
    public static GameManager Game { get; private set; }
    public static InputManager Input { get; private set; }
    public static HookManager Hook { get; private set; }

    private void Awake()
    {
        Instance = this;
        UI = GetComponent<UIManager>();
        Level = GetComponent<LevelManager>();
        Game = GetComponent<GameManager>();
        Hook = new HookManager();
        Input = new InputManager();
    }

    private void Start()
    {
        UnloadAllScenesButThis();

        var level = GetQueryLevel();
        if (level == null) UI.Get<UIMenu>().Show();
        else Game.StartUp(level);
    }

    private void UnloadAllScenesButThis()
    {
        for (var i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (scene != gameObject.scene)
                SceneManager.UnloadSceneAsync(scene);
        }
    }

    private string GetQueryLevel()
    {
        var hasUri = Uri.TryCreate(Application.absoluteURL, UriKind.Absolute, out Uri uri);
        if (hasUri == false)
            return null;

        var parse = HttpUtility.ParseQueryString(uri.Query);
        var level = parse.Get("level");

        for (var i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            var path = SceneUtility.GetScenePathByBuildIndex(i);
            var name = Path.GetFileNameWithoutExtension(path);
            if (name == level)
                return name;
        }

        return null;
    }
}