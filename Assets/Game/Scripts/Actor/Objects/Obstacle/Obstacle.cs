using UnityEngine;

public class Obstacle : MonoBehaviour, IPreDamageHandler, IDamageHandler
{
    [SerializeField]
    private LevelGeometry _geometry;

    [SerializeField]
    private DamageDirection _direction;

    private Rigidbody2D[] _shards;

    public void OnPreDamage(DamageInfo info)
    {
        if (info.Direction != _direction)
            info.Cancel();
    }

    public void OnDamage(DamageInfo info)
    {
        // Destroy(info.Target.gameObject);
        
        Destroy(transform.GetChild(0).gameObject);
        
        _shards = GetComponentsInChildren<Rigidbody2D>();

        foreach (var shard in _shards)
        {
            shard.simulated = true;
            shard.AddExplosionForce(10, info.Source.transform.position, 5, 0, ForceMode2D.Impulse);
            shard.AddTorque(Random.value * 1, ForceMode2D.Impulse);
        }
    }

    private void Update()
    {
        if (_shards != null)
        {
            var hasAwake = false;
            for (var i = 0; i < _shards.Length && !hasAwake; i++)
                hasAwake |= _shards[i].IsAwake();

            if (hasAwake == false)
                Destroy(gameObject);
        }
    }
}

public static class Rigidbody2DExt {

    public static void AddExplosionForce(this Rigidbody2D rb, float explosionForce, Vector2 explosionPosition, float explosionRadius, float upwardsModifier = 0.0F, ForceMode2D mode = ForceMode2D.Force) {
        var explosionDir = rb.position - explosionPosition;
        var explosionDistance = explosionDir.magnitude / explosionRadius;

        // Normalize without computing magnitude again
        if (upwardsModifier == 0)
            explosionDir.Normalize();
        else {
            // From Rigidbody.AddExplosionForce doc:
            // If you pass a non-zero value for the upwardsModifier parameter, the direction
            // will be modified by subtracting that value from the Y component of the centre point.
            explosionDir.y += upwardsModifier;
            explosionDir.Normalize();
        }

        rb.AddForce(Mathf.Lerp(0, explosionForce, (1 - explosionDistance)) * explosionDir, mode);
    }
}