using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class InteractionCollider2D : MonoBehaviour
{
    private static Collider2D[] _sTemp = new Collider2D[1];

    [SerializeField]
    private LayerMask _layers;

    private Collider2D _collider;
    private ContactFilter2D _filter;
    private Actor _source;

    private InteractionInfo _info;

    private List<Collider2D> _overlapped;

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
        
        _sTemp[0] = null;
        Physics2D.OverlapCollider(_collider, _filter, _sTemp);

        var collider = _sTemp[0];
        if (collider != null && _info == null)
        {
            // OnEnter
            var target = GetActorOrNull(collider);
            if (target != null)
            {
                _info = new InteractionInfo(_source, target);
                _info.Enter();
            }
        }
        else if (collider == null && _info != null)
        {
             // OnExit
            _info.Exit();
            _info = null;
        }
    }

    private void OnDisable()
    {
        if (_info != null)
        {
            // OnExit
            _info.Exit();
            _info = null;
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