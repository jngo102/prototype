using System;
using System.Collections.Generic;
using System.Linq;
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
}