using UnityEngine;

public class Bullet : MonoBehaviour, IHitHandler
{
    private Squash2D _squash;
    private Animator _animator;

    private Vector2 _velocity;
    private Timer _lifetime = new Timer(5);
    private bool _alive;

    public void Start()
    {
        _squash = GetComponent<Squash2D>();
        _animator = GetComponent<Animator>();

        _alive = true;
        _lifetime.Start();
    }

    public void SetLinear(Vector2 velocity)
    {
        _velocity = velocity;
    }

    private void Update()
    {
        if (_alive)
        {
            if (_lifetime)
            {
                _squash.Angle = Mathf.Atan2(_velocity.y, _velocity.x) * Mathf.Rad2Deg;
                _squash.ScaleAlong = 1.25f;
                _squash.ScaleAcross = 1 / _squash.ScaleAlong;

                transform.position += (Vector3)_velocity * Time.deltaTime;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    private void LateUpdate()
    {
        _lifetime.Update();
    }

    public void OnHit(DamageInfo info)
    {
        _alive = false;

        _squash.ScaleAcross = 1;
        _squash.ScaleAlong = 1;
        _animator.SetTrigger("Explode");

        // TODO: Normal to hit surface
        var rect = info.Target.GetComponentInChildren<BoxCollider2D>();
        if (rect != null)
        {
            var x = transform.position.x - rect.bounds.center.x;
            var y = transform.position.y - rect.bounds.center.y;
            var k = rect.bounds.size.y / rect.bounds.size.x;
            
            var up1 = y > +k*x;
            var up2 = y > -k*x;

            var angle = 0;
            /**/ if (up1 && up2) angle = 0;
            else if (up1 && !up2) angle = 90;
            else if (!up1 && !up2) angle = 180;
            else if (!up1 && up2) angle = 270;

            // var pos = Vector3.zero;
            // /**/ if (up1 && up2) pos = new Vector3(rect.bounds.center.x + rect.bounds.extents.x, transform.position.y);
            // else if (up1 && !up2) pos = new Vector3(transform.position.x, rect.bounds.center.y + rect.bounds.extents.y);
            // else if (!up1 && !up2) pos = new Vector3(rect.bounds.center.x - rect.bounds.extents.x, transform.position.y);
            // else if (!up1 && up2) pos = new Vector3(transform.position.x ,rect.bounds.center.y - rect.bounds.extents.y);

            // transform.position = pos;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

    }

    private void OnEvent_Complete()
    {
        Destroy(gameObject);
    }
}