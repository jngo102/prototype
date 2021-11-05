using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class Rigidbody2DExplode : MonoBehaviour
{
    [BoxGroup("Explosion"), Label("Radius")]
    [SerializeField]
    private float _explosionRadius = 10;

    [BoxGroup("Explosion"), Label("Force")]
    [SerializeField]
    private float _explosionForce = 5;
    
    [BoxGroup("Explosion"), Label("Upwards")]
    [SerializeField]
    private float _explosionUpwards = 0;

    [BoxGroup("Torque"), Label("Value"), MinMaxSlider(-10, +10)]
    [SerializeField]
    private Vector2 _torqueValue;

    private Rigidbody2D[] _particles;
    private Vector3[] _positions;
    private Quaternion[] _rotations;


    [Button(null, EButtonEnableMode.Playmode)]
    public void Play()
    {
        if (_particles == null)
        {
            _particles = GetComponentsInChildren<Rigidbody2D>();
            _positions = new Vector3[_particles.Length];
            _rotations = new Quaternion[_particles.Length];

            for (var i = 0; i < _particles.Length; i++)
            {
                var particle = _particles[i];
                _positions[i] = particle.transform.localPosition;
                _rotations[i] = particle.transform.localRotation;

                particle.simulated = true;
                particle.AddTorque(Random.Range(_torqueValue.x, _torqueValue.y), ForceMode2D.Impulse);
                particle.AddExplosionForce(_explosionForce, new Vector2(-1.6f, -9f), _explosionRadius, _explosionUpwards, ForceMode2D.Impulse);
            }
        }
        else
        {
            Reset();
        }
    }

    public void Reset()
    {
        if (_particles != null)
        {
            for (var i = 0; i < _particles.Length; i++)
            {
                _particles[i].simulated = false;
                _particles[i].transform.localPosition = _positions[i];
                _particles[i].transform.localRotation = _rotations[i];
            }

            _particles = null;
        }
    }

    private void Update()
    {

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