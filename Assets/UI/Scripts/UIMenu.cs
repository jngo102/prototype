using System.Collections;
using System.Collections.Generic;
using System.IO;
using CodeStage.AdvancedFPSCounter;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIMenu : UIManager.UIBehaviour
{
    [Scene]
    [SerializeField] [BoxGroup("Data")] private string _levelName;
    [SerializeField] [BoxGroup("Data")] private bool _fps;
    
    // [Header("Parts")]
    [SerializeField] [BoxGroup("Options")] private MenuListItem _optionLevel;
    [SerializeField] [BoxGroup("Options")] private MenuListItem _optionMusic;
    [SerializeField] [BoxGroup("Options")] private MenuListItem _optionFPS;
    [SerializeField] private MenuList _start;

    [SerializeField] private TextMeshProUGUI _version;

    private GameObject _selection;

    private void Start()
    {
        _version.text = Application.version;
        _levelName = PlayerPrefs.GetString("_levelName", null);

        OnOptionsLevelClick(0);
        OnOptionsFPSClick();
        _optionMusic.Value = "Off";
    }

    //
    // Main
    //

    public void OnPlayClick() => StartCoroutine(OnPlayClickCoroutine());
    private IEnumerator OnPlayClickCoroutine()
    {
        yield return Get<UICurtain>().Show();
        Hide();
        Main.Game.Play(_levelName);
    }

    public void OnQuitClick()
    {
        Application.Quit();
    }

    //
    // Options
    //

    public void OnOptionsLevelClick(int shift)
    {
        var sceneNames = GetSceneNames();
        var sceneIndex = sceneNames.IndexOf(_levelName);
        var shiftIndex = (sceneNames.Count + sceneIndex + shift) % sceneNames.Count;
        var shiftName = sceneNames[shiftIndex];

        _optionLevel.Value = _levelName = shiftName;

        PlayerPrefs.SetString("_levelName", _levelName);
        PlayerPrefs.Save();
    }

    public void OnOptionsFPSClick()
    {
        _fps = !_fps;

        var counter = AFPSCounter.Instance;
        if (counter != null)
            counter.OperationMode = _fps ? OperationMode.Normal : OperationMode.Disabled;

        _optionFPS.Value = _fps ? "On" : "Off";
    }

    //
    // Tools
    //

    private List<string> GetSceneNames()
    {
        var sceneNames = new List<string>();
        for (var i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            var path = SceneUtility.GetScenePathByBuildIndex(i);
            var name = Path.GetFileNameWithoutExtension(path);
            if (name != gameObject.scene.name)
                sceneNames.Add(name);
        }

        return sceneNames;
    }
}
