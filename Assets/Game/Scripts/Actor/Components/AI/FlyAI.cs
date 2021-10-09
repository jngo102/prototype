using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ActorBody))]
public class FlyAI : MonoBehaviour
{
    private Player _player;

    private ActorBody _body;
    private SpriteRenderer _sprite;
    private Animator _animator;
    
    private Vector2 _steer; 
    private Vector2 _velocity;

    private float _maxVelocity = 2f;
    private float _maxSteer = 6f;

    private Timer _cooldown = new Timer(5f);

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

        
        var distance = _player.transform.position - transform.position;
        var targetAngle = (distance.x < 0 ? 45 : 135) * Mathf.Deg2Rad;
        var targetPos = _player.transform.position + 5f * new Vector3(Mathf.Cos(targetAngle), Mathf.Sin(targetAngle));

        _steer = (targetPos - transform.position).normalized;

        _steer.Scale(Vector2.one * _maxSteer);
        _velocity += _steer * Time.deltaTime;
        
        if (_velocity.magnitude > _maxVelocity)
        {
            _velocity.Normalize();
            _velocity.Scale(Vector2.one * _maxVelocity);
        }
        
        _body.Move(_velocity * Time.deltaTime);
        _sprite.flipX = distance.x > 0;

        if (!_cooldown)
        {
            _cooldown.Start();
            _animator.SetTrigger("Attack");
        }

        _cooldown.Update();
    }
}
