using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class Actor : MonoBehaviour
{
    private Dictionary<Type, object> _cache = new Dictionary<Type, object>();

    public List<T> GetHandlers<T>()
    {
        var type = typeof(T);
        
        var hasCache = _cache.TryGetValue(type, out var value);
        if (hasCache == false)
            value = _cache[type] = GetComponentsInChildren<T>().ToList();

        return (List<T>)value;
    }
}
