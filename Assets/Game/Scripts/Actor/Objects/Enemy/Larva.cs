using UnityEngine;

[RequireComponent(typeof(ActorBody))]
public class Larva : MonoBehaviour, IDamageHandler
{
    public Vector2 Steer { get => _steer; set => _steer = value; }

    private ActorBody _body;
    private Animator _animator;
    private Vector2 _steer;
    private float _gravity;

    const float Gravity = 50f;
    const float Speed = 2f;
    
    void Start()
    {
        _body = GetComponent<ActorBody>();
        _animator = GetComponentInChildren<Animator>();
        _steer = Vector2.left;
    }

    void Update()
    {
        if (_body.collisions.below)
            _gravity = 0;

        if (_body.collisions.rightEdge)
            _steer = Vector2.left;
        
        if (_body.collisions.leftEdge)
            _steer = Vector2.right;

        _gravity += Gravity * Time.deltaTime;
        _body.Move((_gravity*Vector2.down + _steer*Speed) * Time.deltaTime);
    }

    public void OnDamage(DamageInfo info)
    {
        _animator.SetTrigger("Damage");
    }
}
