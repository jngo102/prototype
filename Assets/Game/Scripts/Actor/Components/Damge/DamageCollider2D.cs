using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DamageCollider2D : MonoBehaviour
{
    private static List<Collider2D> _sTemp = new List<Collider2D>();

    [SerializeField]
    private LayerMask _layers;

    [SerializeField]
    private TriggerMode _mode;
    private enum TriggerMode { OnEnter, OnUpdate }

    [SerializeField]
    private DamageType _damageType;

    private Collider2D _collider;
    private ContactFilter2D _filter;
    private Actor _source;

    private List<Collider2D> _overlapped;

    private void OnDisable() => _overlapped.Clear();

    private void Start()
    {
        _collider = GetComponent<Collider2D>();
        _source = GetActorOrNull(_collider);

        _filter = new ContactFilter2D();
        _filter.useLayerMask = true;
        _filter.layerMask = _layers;

        _overlapped = new List<Collider2D>();
    }

    private void Update()
    {
        if (_source == null)
            return;

        Physics2D.OverlapCollider(_collider, _filter, _sTemp);

        for (var i = 0; i < _sTemp.Count; i++)
        {
            // [Collider]
            var collider = _sTemp[i];

            if (_overlapped.Contains(collider) && _mode == TriggerMode.OnEnter)
                continue;
            
            _overlapped.Add(collider);

            // [Actor]
            var target = GetActorOrNull(collider);
            if (target == null)
                continue;

            var info = new DamageInfo(_source, target);
            info.Type = _damageType;
            info.Process();
        }

        for (var i = 0; i < _overlapped.Count; i++)
        {
            var collider = _overlapped[i];
            var colliderIsOverlapped = _sTemp.Contains(collider);
            if (colliderIsOverlapped == false)
                _overlapped.RemoveAt(i--);
        }
    }

    private Actor GetActorOrNull(Collider2D collider)
    {
        var component = collider.GetComponentInParent<Actor>();
        if (component == null)
            Debug.LogError($"Can't find '{typeof(Actor).Name}' in '{collider.name}' or it's parents");
        
        return component;
    }
}