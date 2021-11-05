using System;
using System.Collections.Generic;
using UnityEngine;

// ISave implementation on PlayerPrefs & JsonUtility
public class PlayerPrefsSaveSlot : ISaveSlot
{
    private const int cVersion = 0;

    private string _key;
    private string _scope;

    private Dictionary<string, string> _source;

    private Dictionary<string, SaveVar> _bench;
    private Dictionary<string, SaveVar> _persistent;

    public PlayerPrefsSaveSlot(string key)
    {
        if (key == null)
            Debug.LogWarning("No slot key was given your progress won't be saved");

        _key = key;
        _source = new Dictionary<string, string>();
        _bench = new Dictionary<string, SaveVar>();
        _persistent = new Dictionary<string, SaveVar>();

        if (_key != null)
        {
            var json = PlayerPrefs.GetString(_key);
            var list = JsonUtility.FromJson<JsonList>(json);

            if (list != null && list.variables != null)
                foreach (var variable in list.variables)
                    _source.Add(variable.key, variable.value);
        }
    }

    public SaveInt GetInt(string key, SaveMode mode = SaveMode.Bench, SaveScope scope = SaveScope.Local) => Get<SaveInt>(key, mode, scope);
    public SaveBool GetBool(string key, SaveMode mode = SaveMode.Bench, SaveScope scope = SaveScope.Local)  => Get<SaveBool>(key, mode, scope);
    public SaveString GetString(string key, SaveMode mode = SaveMode.Bench, SaveScope scope = SaveScope.Local) => Get<SaveString>(key, mode, scope);

    private T Get<T>(string key, SaveMode mode = SaveMode.Bench, SaveScope scope = SaveScope.Local) where T : SaveVar, new()
    {
        if (scope == SaveScope.Local && _scope == null)
            throw new InvalidOperationException("Can't create SaveScope.Local variable out of BeginScope() / EndScope() section");

        var id = scope == SaveScope.Global ? key : _scope + "::" + key;
        var dictionary = mode == SaveMode.Bench ? _bench : _persistent;

        var hasValue = dictionary.TryGetValue(id, out SaveVar value);
        if (hasValue && value is T)
            return (T)value;
        
        if (hasValue == false)
        {
            var result = new T();
            dictionary.Add(id, result);

            var hasSource = _source.TryGetValue(id, out string source);
            if (hasSource)
                result.Decode(source);

            return result;
        }

        throw new InvalidOperationException($"Variable '{key}' already has type '{value.GetType()}'");
    }

    public void Reset(SaveMode level = SaveMode.Bench) { _bench.Clear(); }
    public void BeginScope(string key) { _scope = key; }
    public void EndScope() { _scope = null; }

    //
    // Flush
    //

    public void Flush()
    {
        if (_key == null)
            return;

        var list = new JsonList();
        list.version = cVersion;
        list.variables = new List<JsonVar>();

        foreach (var pair in _persistent)
        {
            list.variables.Add(
                new JsonVar() {
                    key = pair.Key,
                    value = pair.Value.Encode()
                }
            );
        }

        var json = JsonUtility.ToJson(list);
        PlayerPrefs.SetString(_key, json);
        PlayerPrefs.Save();
    }

    [Serializable]
    private class JsonList
    {
        public int version;
        public List<JsonVar> variables;
    }

    [Serializable]
    private class JsonVar
    {
        public string key;
        public string value;
    }
}

