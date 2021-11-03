//
// Handlers
//

using UnityEngine;

public interface IInteractionHander
{
    void OnInteractionEnter(InteractionInfo info);
    void OnInteractionTrigger(InteractionInfo info);
    void OnInteractionExit(InteractionInfo info);
}

//
// Interaction
// 

public class InteractionInfo
{
    public Actor Source { get; }
    public Actor Target { get; }

    public InteractionType Type { get; set; }
    public Vector2 BenchPoint;

    public InteractionInfo(Actor source, Actor target)
    {
        Debug.Assert(source != null);
        Debug.Assert(target != null);

        Source = source;
        Target = target;
    }

    public void Enter()
    {
        var sourceEnter = Source.GetHandlers<IInteractionHander>();
        for (var i = 0; i < sourceEnter.Length; i++)
            sourceEnter[i].OnInteractionEnter(this);

        var targetEnter = Target.GetHandlers<IInteractionHander>();
        for (var i = 0; i < targetEnter.Length; i++)
            targetEnter[i].OnInteractionEnter(this);
    }

    public void Trigger()
    {
        var sourceTrigger = Source.GetHandlers<IInteractionHander>();
        for (var i = 0; i < sourceTrigger.Length; i++)
            sourceTrigger[i].OnInteractionTrigger(this);

        var targetTrigger = Target.GetHandlers<IInteractionHander>();
        for (var i = 0; i < targetTrigger.Length; i++)
            targetTrigger[i].OnInteractionTrigger(this);
    }

    public void Exit()
    {
        var sourceExit = Source.GetHandlers<IInteractionHander>();
        for (var i = 0; i < sourceExit.Length; i++)
            sourceExit[i].OnInteractionExit(this);

        var targetExit = Target.GetHandlers<IInteractionHander>();
        for (var i = 0; i < targetExit.Length; i++)
            targetExit[i].OnInteractionExit(this);
    }
}