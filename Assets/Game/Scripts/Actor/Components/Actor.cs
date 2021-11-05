using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// This marks root GameObject of the Actor
public sealed class Actor : MonoBehaviour
{
    private Dictionary<Type, object> _cache = new Dictionary<Type, object>();

    public T[] GetHandlers<T>()
    {
        var type = typeof(T);
        
        var hasCache = _cache.TryGetValue(type, out var value);
        if (hasCache == false)
            value = _cache[type] = GetComponentsInChildren<T>();

        return (T[])value;
    }

    public string GetPath()
    {
        var names = new List<string>();
        var cursor = transform;
        do {
            names.Add(cursor.name);
            cursor = cursor.parent;
        } while (cursor != null);

        var result = new StringBuilder();
        for (var i = names.Count-1; i > 0; i--)
        {
            result.Append(names[i]);
            result.Append("/");
        }

        result.Append(names[0]);
        return result.ToString();
    }
}