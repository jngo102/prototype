using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class ActorSteer : Action
{
    private ActorBody _body;
    private Enemy _enemy;

    [SerializeField]
    private Vector2 _steer;

    public override void OnStart()
    {
        _body = GetComponent<ActorBody>();
        _enemy = GetComponent<Enemy>();
    }

    public override TaskStatus OnUpdate()
    {
        _enemy.Steer = _steer;
        return TaskStatus.Success;
    }
}