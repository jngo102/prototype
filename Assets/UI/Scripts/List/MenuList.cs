using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class MenuList : MenuSection
{
    [HideInInspector]
    [SerializeField]
    private MenuListItem item;
    
    [HideInInspector]
    [SerializeField]
    private List<GameObject> _items;

    [SerializeField]
    private List<MenuListEntry> data;
    
    private CanvasGroup _canvasGroup;

    private void Start()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        SetFocusByDefault(_items[0]);
    }

    private void Update()
    {
        // Animate
        var alpha = IsVisible ? 1 : 0;
        var alphaOffset = Mathf.Sign(alpha - _canvasGroup.alpha);
        _canvasGroup.alpha = Mathf.Clamp01(_canvasGroup.alpha + alphaOffset * 0.2f);

        // Disable
        if (!IsVisible && _canvasGroup.alpha == 0)
            gameObject.SetActive(false);
    }

    #if UNITY_EDITOR
    [Button(null, EButtonEnableMode.Editor)]
    private void Rebuld()
    {
        var viewsById = new Dictionary<int, MenuListItem>();
        foreach (var view in _items)
            if (view != null)
                viewsById[view.gameObject.GetInstanceID()] = view.GetComponent<MenuListItem>();

        var views = new List<GameObject>();
        foreach (var entry in data)
        {
            var hasView = viewsById.TryGetValue(entry.Id, out var view);
            if (hasView == false)
                view = (MenuListItem)UnityEditor.PrefabUtility.InstantiatePrefab(item, transform);

            entry.Id = view.gameObject.GetInstanceID();
            view.Label = entry.Label;
            view.Mode = entry.Mode;
            views.Add(view.gameObject);
        }

        for (var i = 0; i < _items.Count; i++)
        {
            var view = _items[i];
            if (views.Contains(view) == false)
                DestroyImmediate(view);
        }

        for (var i = 0; i < views.Count; i++)
            views[i].transform.SetSiblingIndex(i);

        _items = views;
    }
    #endif
}

[Serializable]
public class MenuListEntry
{
    public string Label;
    public MenuListItem.ItemMode Mode;

    [HideInInspector]
    public int Id;
}