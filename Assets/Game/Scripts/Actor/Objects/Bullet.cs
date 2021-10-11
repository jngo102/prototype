using UnityEngine;

public class Bullet : MonoBehaviour, IHitHandler
{
    private Squash2D _squash;
    private Vector2 _velocity;
    
    private Timer _lifetime = new Timer(5);

    public void Start()
    {
        _squash = GetComponent<Squash2D>();
        _lifetime.Start();
    }

    public void SetLinear(Vector2 velocity)
    {
        _velocity = velocity;
    }

    private void Update()
    {
        if (_lifetime)
        {
            _squash.Angle = Mathf.Atan2(_velocity.y, _velocity.x) * Mathf.Rad2Deg;
            _squash.ScaleAlong = 1.25f;
            _squash.ScaleAcross = 1 / 1.25f;

            transform.position += (Vector3)_velocity * Time.deltaTime;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LateUpdate()
    {
        _lifetime.Update();
    }

    public void OnHit(DamageInfo info)
    {
        Destroy(gameObject);
    }
}