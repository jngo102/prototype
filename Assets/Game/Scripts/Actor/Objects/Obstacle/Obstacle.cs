using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Animations;

[RequireComponent(typeof(Actor))]
[RequireComponent(typeof(PositionConstraint))]
public class Obstacle : MonoBehaviour, IDamageHandler
{
    [SerializeField]
    private LevelGeometry _geometry;
    private BoxCollider2D _collider;

    [SerializeField] private int _hitpoints = 3;

    [BoxGroup("Effects")] [SerializeReference, TypePopup] private IEffect _onDamage;
    [BoxGroup("Effects")] [SerializeReference, TypePopup] private IEffect _onDestroy;

    private Rigidbody2D[] _piecies;

    private void Start()
    {
        _collider = gameObject.AddComponent<BoxCollider2D>();
        _collider.size = _geometry.transform.localScale;
    }

    public void OnDamage(DamageInfo info)
    {
        _hitpoints -= 1;

        if (_hitpoints <= 0)
        {
            // Disable Geometry & Collider
            _geometry.gameObject.SetActive(false);
            _collider.enabled = false;
            
            // Run Animation
            _onDestroy?.Play();
            _piecies = GetComponentsInChildren<Rigidbody2D>();
            foreach (var piece in _piecies)
            {
                piece.AddExplosionForce(10, info.Source.transform.position, 5, 0, ForceMode2D.Impulse);
                piece.AddTorque(Random.value, ForceMode2D.Impulse);
            }
        }
        else
        {
            _onDamage?.Play();
        }
    }

    private void Update()
    {
        if (_piecies != null)
        {
            for (var i = 0; i < _piecies.Length; i++)
            {
                if (_piecies[i].IsSleeping())
                    _piecies[i].simulated = false;
            }
        }
    }

    private void OnValidate()
    {
        var constraint = GetComponent<PositionConstraint>();

        constraint.hideFlags = HideFlags.None;
        constraint.translationAtRest = Vector3.zero;
        constraint.translationOffset = Vector3.zero;
        constraint.locked = true;

        for (var i = constraint.sourceCount-1; i >= 0; i--)
            constraint.RemoveSource(i);

        if (_geometry != null)
        {
            var source = new ConstraintSource();
            source.sourceTransform = _geometry.transform;
            source.weight = 1;
    
            constraint.AddSource(source);
            constraint.constraintActive = true;
        }
        else
        {
            constraint.constraintActive = false;
        }
    }
}

public static class Rigidbody2DExtention
{
    public static void AddExplosionForce(this Rigidbody2D body, float explosionForce, Vector2 explosionPosition, float explosionRadius, float upwardsModifier = 0.0F, ForceMode2D mode = ForceMode2D.Force)
    {
        var explosionDir = body.position - explosionPosition;
        var explosionDistance = explosionDir.magnitude;

        // Normalize without computing magnitude again
        if (upwardsModifier == 0)
        {
            explosionDir /= explosionDistance;
        }
        else
        {
            // From Rigidbody.AddExplosionForce doc:
            // If you pass a non-zero value for the upwardsModifier parameter, the direction
            // will be modified by subtracting that value from the Y component of the centre point.
            explosionDir.y += upwardsModifier;
            explosionDir.Normalize();
        }

        body.AddForce(Mathf.Lerp(0, explosionForce, (1 - explosionDistance/explosionRadius)) * explosionDir, mode);
    }
}