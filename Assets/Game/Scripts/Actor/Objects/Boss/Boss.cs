using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Actor))]
[RequireComponent(typeof(ActorBody))]
[RequireComponent(typeof(Animator))]
public class Boss : MonoBehaviour, IDamageHandler
{
    // Parts
    private ActorBody _body;
    private Animator _animator;
    private SpriteRenderer _sprite;
    
    // Data
    private Player _player;

    // Behaviour
    private Vector2 _gravity = 60 * Vector2.down;
    private Vector2 _velocity;

    private Timer _jumpTimer = new Timer(4);
    private float _jumpVelocity = 25;

    private void Start()
    {
        _body = GetComponent<ActorBody>();
        _animator = GetComponent<Animator>();
        _sprite = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        // Initialize
        if (_player == null)
            _player = FindObjectOfType<Player>();

        if (_body.collisions.below)
            _velocity.x = 0;

        // Jump
        if (_jumpTimer == false)
        {
            _jumpTimer.Start();
            _velocity = (_player.transform.position - transform.position).normalized;
            _velocity += Vector2.up * Random.Range(0.5f, 3);
            _velocity.Normalize();
            _velocity *= _jumpVelocity;
            _sprite.flipX = _velocity.x > 0;
        }

        // Movement
        var grounded = _body.collisions.below;
        
        _velocity += _gravity * Time.deltaTime;
        _body.Move(_velocity * Time.deltaTime);

        if (grounded == false && _body.collisions.below)
            PlayerCameraController.Instance.Shake(0.5f);
    }

    private void LateUpdate()
    {
        _jumpTimer.Update();
    }

    public void OnDamage(DamageInfo info)
    {
        _animator.SetTrigger("Damage");
    }
}
