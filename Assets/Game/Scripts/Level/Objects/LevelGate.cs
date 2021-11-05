using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class LevelGate : MonoBehaviour, IEntrance
{
    [HideInInspector] [SerializeField] private Sprite _sprite;

    #region Position

    private enum Side { Top, Right, Bottom, Left }

    [SerializeField] private Side _side;

    private void OnValidate()
    {
        // Name
        var prefix = "Gate_" + _side + "_";
        if (name.StartsWith(prefix) == false)
            name = Regex.Replace(name, @"Gate_(Left|Right|Top|Bottom)_", prefix);
        
        if (name.StartsWith(prefix) == false)
            name = prefix + name;

        // Components
        var collider = GetComponent<BoxCollider2D>();
        collider.hideFlags = HideFlags.NotEditable;
        collider.isTrigger = true;

        var sprite = GetComponent<SpriteRenderer>();
        sprite.hideFlags = HideFlags.NotEditable;
        sprite.sprite = _sprite;
        sprite.color = LevelGeometry.Gate;

        // Transform
        var boundsComponent = GetComponentInParent<LevelBounds>();
        if (boundsComponent == null)
            return;

        var bounds = boundsComponent.Size;
        var rect = new Rect();
        rect.width = transform.localScale.x;
        rect.height = transform.localScale.y;
        rect.x = transform.localPosition.x;
        rect.y = transform.localPosition.y;

        if (_side == Side.Right || _side == Side.Left)
        {
            rect.width = 1;
            rect.height = Mathf.Clamp(rect.height, 1, bounds.y);

            var sign = _side == Side.Right ? +1 : -1;
            rect.x = sign * (bounds.x + 1) / 2f;

            var limit = (bounds.y - rect.height ) / 2;
            rect.y = Mathf.Clamp(rect.y, -limit, +limit);
        }
        else
        {
            rect.width = Mathf.Clamp(rect.width, 1, bounds.x);
            rect.height = 1;

            var sign = _side == Side.Top ? +1 : -1;
            rect.y = sign * (bounds.y + 1) / 2f;

            var limit = (bounds.x - rect.width) / 2;
            rect.x = Mathf.Clamp(rect.x, -limit, +limit);
        }

        transform.localPosition = rect.position;
        transform.localScale = rect.size;
        LevelGeometry.Round(transform);
    }

    private void Update()
    {
        if (Application.isPlaying)
            return;

        OnValidate();
    }

    #endregion

    #region Transition

    [FormerlySerializedAs("_levelName")]
    [Dropdown("GetSceneNames")]
    [SerializeField] private string _level = string.Empty;

    [FormerlySerializedAs("_doorName")]
    [FormerlySerializedAs("_transitionName")]
    [Dropdown("GetSceneTransitions")]
    [EnableIf("HasScene")]
    [SerializeField] private string _gate = string.Empty;

    [Button]
    [HideIf("HasScene")]
    [EnableIf("IsLoadAllowed")]
    private void Load()
    {
        #if UNITY_EDITOR
            var scenePath = GetScenePath(_level);
            var sceneMode = UnityEditor.SceneManagement.OpenSceneMode.Additive;

            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath, sceneMode);
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        #endif
    }

    [Button]
    [ShowIf("HasScene")]
    [EnableIf("IsUnloadAllowed")]
    private void Unload()
    {
        #if UNITY_EDITOR
            var scene = SceneManager.GetSceneByName(_level);

            UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene, true);
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        #endif
    }

    private bool HasScene()
    {
        for (var i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (scene.name == _level)
                return true;
        }

        return false;
    }

    private bool IsLoadAllowed()
    {
        for (var i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            var path = SceneUtility.GetScenePathByBuildIndex(i);
            var name = Path.GetFileNameWithoutExtension(path);
            if (name == _level)
                return true;
        }

        return false;
    }

    private bool IsUnloadAllowed()
    {
        return gameObject.scene.name != _level;
    }

    private DropdownList<string> GetSceneNames()
    {
        var result = new DropdownList<string>();

        // Start from 2 because we need to ommit Launcher & Template scenes
        var names = new List<string>();
        for (var i = 2; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            var path = SceneUtility.GetScenePathByBuildIndex(i);
            var name = Path.GetFileNameWithoutExtension(path);
            names.Add(name);
        }

        result.Add("(None)", string.Empty);

        if (_level != string.Empty && names.Contains(_level) == false)
            result.Add(_level, _level);

        foreach (var name in names)
            result.Add(name, name);

        return result;
    }

    private DropdownList<string> GetSceneTransitions()
    {
        var result = new DropdownList<string>();
        var names = 
            FindObjectsOfType<LevelGate>()
            .Where(x => x.gameObject.scene.name == _level)
            .Where(x => x != this)
            .Select(x => x.name)
            .ToList();

        result.Add("(None)", string.Empty);

        if (_gate != string.Empty && names.Contains(_gate) == false)
        {
            if (HasScene())
                result.Add("<" + _gate + ">", _gate);
            else 
                result.Add(_gate, _gate);
        }

        foreach (var name in names)
            result.Add(name, name);

        return result;
    }

    private string GetScenePath(string sceneName)
    {
        for (var i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            var path = SceneUtility.GetScenePathByBuildIndex(i);
            var name = Path.GetFileNameWithoutExtension(path);
            if (name == sceneName)
                return path;
        }

        return null;
    }

    private void OnDrawGizmos()
    {
        #if UNITY_EDITOR
        if (UnityEditor.Selection.activeGameObject != this.gameObject)
            return;
        #endif

         var target = FindObjectsOfType<LevelGate>().FirstOrDefault(x
                => x.name == _gate
                && x.gameObject.scene.name == _level
                && x.gameObject.activeInHierarchy
        );

        if (target != null)
        {
            var from = transform.position;
            var to = target.transform.position;

            Gizmos.color = LevelGeometry.Gate;
            Gizmos.DrawLine(from, to);

            // Add arrows
            
            for (var i = 1; i < 4; i++)
            {
                var back = (from - to).normalized;
                var normal1 = new Vector3(+back.y, -back.x, 0);
                var normal2 = new Vector3(-back.y, +back.x, 0);
                var middle = from + (to - from) * (i/4f);

                Gizmos.DrawLine(middle, middle + (back + normal1) / 2);
                Gizmos.DrawLine(middle, middle + (back + normal2) / 2);
            }
        }

    }

    #endregion

    public void Place(Player player, EntranceInfo info)
    {
        var entrance = info.Offset;
        var offset = Vector2.zero;
        var direction = 0f;

        switch (_side)
        {
            case Side.Left:
            case Side.Right:

                var offsetX = entrance.x * -1.2f;
                var offsetY = entrance.y - transform.localScale.y/2;

                offset = new Vector2(offsetX, offsetY);
                direction = _side == Side.Right ? -1 : +1;
                break;
            case Side.Top:
                offset = 2 * Vector2.down;
                direction = Mathf.Sign(entrance.x);
                break;
            case Side.Bottom:
                offset = new Vector2(entrance.x < 0 ? -2 : +2, +3);
                direction = Mathf.Sign(entrance.x);
                break;
        }

        player.Setup(transform.position + (Vector3)offset, direction, true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // We accept only one collider - Player collider
        var player = other.GetComponentInParent<Player>();
        if (player == null)
            return;
                
        var info = new EntranceInfo()
        {
            Offset = new Vector2(
                player.transform.position.x - transform.position.x,
                transform.localScale.y/2 - (transform.position.y - player.transform.position.y)
            )
        };

        Main.Hook.PlayerTransit.Invoke(
            new PlayerTransitArgs(
                _level,
                _gate,
                info
            )
        );
    }
}