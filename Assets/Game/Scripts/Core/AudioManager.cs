using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using NaughtyAttributes;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AudioManager : MonoBehaviour
{
    [Range(0, 1)]
    [SerializeField]
    private float _volume = 0.5f;

    [HideInInspector]
    [SerializeField]
    private List<AudioClipReference> _clips;

    private AudioSource _source;
    private AudioListener _listener;

    #if UNITY_EDITOR

    [SerializeField]
    private UnityEngine.Object _clipsFolder = null;
    [SerializeField]
    private MonoScript _clipsEnum = null;

    private void OnValidate()
    {
        var clipsFolderPath = AssetDatabase.GetAssetPath(_clipsFolder);
        var clipsFolderIsValid = AssetDatabase.IsValidFolder(clipsFolderPath);
        if (clipsFolderIsValid == false)
        {
            _clipsFolder = null;
            Debug.LogWarning($"'{clipsFolderPath}' must be a Folder");
            return;
        }
    }

    [Button(null, EButtonEnableMode.Editor)]
    private void Refresh()
    {
        if (_clipsFolder == null)
            throw new InvalidOperationException();

        if (_clipsEnum == null)
            throw new InvalidOperationException();

        var folderPath = AssetDatabase.GetAssetPath(_clipsFolder);
        var scriptPath = AssetDatabase.GetAssetPath(_clipsEnum);

        _clips = AssetDatabase
            .FindAssets("t:AudioClip", new [] { folderPath })
            .Select(AudioClipReference.FromGUID)
            .OrderBy(x => x.Path)
            .ToList();

        WriteToEnum(scriptPath, _clips);
    }

    private void WriteToEnum(string path, IEnumerable<AudioClipReference> references)
    {
        var builder = new StringBuilder();

        // Add Sounds enum
        builder.AppendLine("// This file is auto-generated by " + nameof(AudioManager) + ", please DO NOT change it manually");
        builder.AppendLine("public enum " + Path.GetFileNameWithoutExtension(path));
        builder.AppendLine("{");

        foreach (var reference in references)
        {
            var fileName = Path.GetFileNameWithoutExtension(reference.Path);

            var fileSplit = fileName.Split('_');
            for (var i = 0; i < fileSplit.Length; i++)
                fileSplit[i] = GetVariableName(fileSplit[i]);

            builder.AppendLine("\t" + string.Join(string.Empty, fileSplit) + ",");
        }
        
        builder.AppendLine("}");

        // Add Sounds extensions to AudioManager
        var nameofThis = nameof(AudioManager);
        var nameofEnum = Path.GetFileNameWithoutExtension(path);

        builder.AppendLine($"\npublic static class {nameofThis}{nameofEnum}Extensions");
        builder.AppendLine("{");
        builder.AppendLine($"\tpublic static void Play(this {nameofThis} audio, {nameofEnum} sound) => audio.Play((int)sound);");
        builder.AppendLine("}");

        File.WriteAllText(path, builder.ToString());
    }

    private string GetVariableName(string filename)
    {
        return filename[0].ToString().ToUpperInvariant() + filename.Substring(1);
    }

    #endif

    public void Start()
    {
        _listener = gameObject.AddComponent<AudioListener>();

        _source = gameObject.AddComponent<AudioSource>();
        _source.playOnAwake = false;
        _source.volume = _volume;
    }

    public void Play(int index)
    {
        _source.PlayOneShot(_clips[index].Clip);
    }

    public void Play(AudioClip clip)
    {
        _source.PlayOneShot(clip);
    }

    [Serializable]
    private struct AudioClipReference
    {
        public static AudioClipReference FromGUID(string guid)
        {
            var result = new AudioClipReference();

            result.Path = AssetDatabase.GUIDToAssetPath(guid);
            result.Clip = AssetDatabase.LoadAssetAtPath<AudioClip>(result.Path);

            return result;
        }

        public string Path;
        public AudioClip Clip;
    }
}