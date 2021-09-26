using BehaviorDesigner.Runtime;
using UnityEngine;

[RequireComponent(typeof(ActorBody))]
public class Enemy : MonoBehaviour, IDamageHandler
{
    public Vector2 Steer { get => _steer; set => _steer = value; }

    private ActorBody _body;
    private Vector2 _steer;
    private float _gravity;

    const float Gravity = 50f;
    const float Speed = 2f;
    
    void Start()
    {
        _body = GetComponent<ActorBody>();
        _steer = Vector2.left;
    }

    void Update()
    {
        if (_body.collisions.below)
            _gravity = 0;

        _gravity += Gravity * Time.deltaTime;
        
        _body.Move((_gravity*Vector2.down + _steer*Speed) * Time.deltaTime);
    }

    private void LateUpdate()
    {
        
    }


    public void OnDamage(DamageInfo info)
    {
        GetComponentInChildren<Animator>().SetTrigger("Damage");
    }
}
