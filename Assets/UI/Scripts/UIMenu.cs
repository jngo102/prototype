using System.Collections;
using System.Collections.Generic;
using System.IO;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UIMenu : UIManager.UIBehaviour
{
    [Header("Parts")]
    [SerializeField] private GameObject _play;
    [SerializeField] private GameObject _quit;
    [SerializeField] private GameObject _options;
    [SerializeField] private GameObject _optionsLevel;
    [SerializeField] private GameObject _optionsLevelName;
    [SerializeField] private TextMeshProUGUI _version;
    
    [Header("Blocks")]
    [SerializeField] private GameObject _bMain;
    [SerializeField] private GameObject _bOptions;

    [Header("Data")]
    [Scene]
    [SerializeField] private string _levelName;

    private GameObject _selection;

    private void Start()
    {
        _version.text = Application.version;
        _levelName = PlayerPrefs.GetString("_levelName", null);

        var levelNameExists = GetSceneNames().IndexOf(_levelName) != -1;
        if (levelNameExists == false)
            _levelName = GetSceneNames()[0];

        _optionsLevelName.GetComponent<TextMeshProUGUI>().text = _levelName;

        _bMain.SetActive(true);
        _bOptions.SetActive(false);

        Main.Input.UI.Cancel.performed += OnCancel;
    }

    protected override IEnumerator OnShow()
    {
        // var music = new [] { Sounds.Track1, Sounds.Track2, Sounds.GardensVillage };
        // var musicIndex = Mathf.RoundToInt(Random.value * music.Length);
        // var track = music[musicIndex];
        // Main.Audio.Play(track);

        Main.Input.UI.Enable();
        Select(_play);
        yield break;
    }

    protected override IEnumerator OnHide()
    {
        Main.Input.UI.Disable();

        yield break;
    }

    private void Update()
    {
        if (IsShow && !IsTransit && _selection != null && EventSystem.current.currentSelectedGameObject != _selection)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(_selection);
            _selection = null;
        }
    }

    //
    // Main
    //
    public void OnPlayClick()
    {
        EventSystem.current.SetSelectedGameObject(null);
        StartCoroutine(OnPlayClickCoroutine());
    }

    private IEnumerator OnPlayClickCoroutine()
    {
        yield return Get<UICurtain>().ShowAndWait();
        Hide();
        Main.Game.StartUp(_levelName);
    }

    public void OnOptionsClick()
    {
        _bMain.SetActive(false);
        _bOptions.SetActive(true);
        Select(_optionsLevel);
    }

    public void OnQuitClick()
    {
        Application.Quit();
    }

    //
    // Options
    //

    public void OnOptionsLevelClick()
    {
        var sceneNames = GetSceneNames();
        var sceneIndex = sceneNames.IndexOf(_levelName);
        var nextSceneIndex = (sceneIndex + 1) % sceneNames.Count;
        var nextSceneName = sceneNames[nextSceneIndex];

        _levelName = nextSceneName;
        _optionsLevelName.GetComponent<TextMeshProUGUI>().text = _levelName;
        PlayerPrefs.SetString("_levelName", _levelName);
        PlayerPrefs.Save();
    }

    public void OnOptionsBackClick()
    {
        _bMain.SetActive(true);
        _bOptions.SetActive(false);
        Select(_options);
    }

    public void OnCancel(InputAction.CallbackContext context)
    {
        if (_bOptions.activeSelf)
            OnOptionsBackClick();
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

    private void Select(GameObject gameObject)
    {
        _selection = gameObject;
    }
}
