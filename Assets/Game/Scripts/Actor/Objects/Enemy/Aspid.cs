using UnityEngine;

[RequireComponent(typeof(ActorBody))]
public class Aspid : MonoBehaviour, IDamageHandler
{
    [SerializeField]
    private GameObject _bullet;
    [SerializeField]
    private float _bulletSpeed;

    private enum State
    {
        Idle,
        Сhase,
        Attack
    }

    private Player _player;

    private ActorBody _body;
    private SpriteRenderer _sprite;
    private Animator _animator;
    
    private Vector2 _steer; 
    private Vector2 _velocity;

    private float _maxVelocity = 2f;
    private float _maxSteer = 6f;

    private State _state = State.Idle;

    private Timer _cooldown = new Timer(5f);

    private Timer _recoil = new Timer(0.1f);
    private float _recoilVelocity = 10;
    private Vector2 _recoilDirection;

    private void Start()
    {
        _body = GetComponent<ActorBody>();
        _sprite = GetComponentInChildren<SpriteRenderer>();
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (_player == null)
            _player = FindObjectOfType<Player>();
        
        if (_player == null)
            return;

        switch (_state)
        {
            case State.Idle: Update_Idle(); break;
            case State.Сhase: Update_Chase(); break;
            case State.Attack: Update_Attack(); break;
        }

        // Recoil
        if (_recoil)
        {
            _velocity = _recoilDirection * _recoilVelocity;
        }
        // Movement
        else
        {
            _steer = _steer.normalized * _maxSteer;
            
            _velocity += _steer * Time.deltaTime;
            if (_velocity.magnitude > _maxVelocity)
                _velocity = _velocity.normalized * _maxVelocity;
        }

        _body.Move(_velocity * Time.deltaTime);
    }

    private void Update_Idle()
    {
        var distance = _player.transform.position - transform.position;
        if (distance.magnitude < 10)
        {
            _state = State.Сhase;
            _cooldown.Start();
        }
    }

    private void Update_Chase()
    {
        var distance = _player.transform.position - transform.position;
        var targetAngle = (distance.x < 0 ? 45 : 135) * Mathf.Deg2Rad;
        var targetPos = _player.transform.position + 5f * new Vector3(Mathf.Cos(targetAngle), Mathf.Sin(targetAngle));

        _steer = (targetPos - transform.position).normalized;
        
        _sprite.flipX = distance.x > 0;

        if (_cooldown == false)
        {
            _state = State.Attack;
            _cooldown.Start();
            _animator.SetTrigger("Attack");
        }
    }

    private void Update_Attack()
    {
        _steer = Vector2.zero;
    }

    private void OnEventAttack()
    {
        const int delta = 35;
        const int count = 3;

        var direction = (_player.transform.position - transform.position).normalized;

        var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        var angleFrom = angle - delta;
        var angleTo = angle + delta;

        for (var i = 0; i < count; i++)
        {
            var r = count == 1 ? 1/2f : i/(count-1f);
            var a = Mathf.LerpAngle(angleFrom, angleTo, r) * Mathf.Deg2Rad;
            var v = new Vector2(Mathf.Cos(a), Mathf.Sin(a));

            var bullet = Instantiate(_bullet);
            bullet.transform.position = transform.position;
            bullet.GetComponent<Bullet>().SetLinear(v * _bulletSpeed);
        }
    }

    private void OnEventAttackComplete()
    {
        _state = State.Сhase;
    }

    private void LateUpdate()
    {
        _cooldown.Update();
        _recoil.Update();
    }

    public void OnDamage(DamageInfo info)
    {
        _animator.SetTrigger("Damage");
        _recoil.Start();
        _recoilDirection = (info.Target.transform.position - info.Source.transform.position).normalized;
    }
}