using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class ActorBodyConditional : Conditional
{
    private ActorBody _body;

    [SerializeField] private State _top;
    [SerializeField] private State _bottom;
    [SerializeField] private State _right;
    [SerializeField] private State _left;
    [SerializeField] private State _bottomLeft;
    [SerializeField] private State _bottomRight;

    public override void OnStart()
    {
        _body = GetComponent<ActorBody>();
    }

    public override TaskStatus OnUpdate()
    {
        var result = true;
        
        result &= IsEqual(_body.collisions.above, _top);
        result &= IsEqual(_body.collisions.below, _bottom);
        result &= IsEqual(_body.collisions.right, _right);
        result &= IsEqual(_body.collisions.left, _left);
        result &= IsEqual(_body.collisions.leftEdge, _bottomLeft);
        result &= IsEqual(_body.collisions.rightEdge, _bottomRight);

        if (result)
            return TaskStatus.Success;

        return TaskStatus.Failure;
    }

    private bool IsEqual(bool value, State state)
    {
        return state == State.None || value == (state == State.True);
    }

    private enum State
    {
        None,
        True,
        False
    }
}