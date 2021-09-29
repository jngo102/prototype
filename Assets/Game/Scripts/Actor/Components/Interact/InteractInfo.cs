//
// Handlers
//

using UnityEngine;

public interface IInteractHander
{
    void OnInteractionEnter(InteractInfo info);
    void OnInteractionTrigger(InteractInfo info);
    void OnInteractionExit(InteractInfo info);
}

//
// Interaction
// 

public class InteractInfo
{
    public Actor Source { get; }
    public Actor Target { get; }

    public InteractInfo(Actor source, Actor target)
    {
        Debug.Assert(source != null);
        Debug.Assert(target != null);

        Source = source;
        Target = target;
    }

    public void Enter()
    {
        var sourceEnter = Source.GetHandlers<IInteractHander>();
        for (var i = 0; i < sourceEnter.Count; i++)
            sourceEnter[i].OnInteractionEnter(this);

        var targetEnter = Target.GetHandlers<IInteractHander>();
        for (var i = 0; i < targetEnter.Count; i++)
            targetEnter[i].OnInteractionEnter(this);
    }

    public void Trigger()
    {
        var sourceTrigger = Source.GetHandlers<IInteractHander>();
        for (var i = 0; i < sourceTrigger.Count; i++)
            sourceTrigger[i].OnInteractionTrigger(this);

        var targetTrigger = Target.GetHandlers<IInteractHander>();
        for (var i = 0; i < targetTrigger.Count; i++)
            targetTrigger[i].OnInteractionTrigger(this);
    }

    public void Exit()
    {
        var sourceExit = Source.GetHandlers<IInteractHander>();
        for (var i = 0; i < sourceExit.Count; i++)
            sourceExit[i].OnInteractionExit(this);

        var targetExit = Target.GetHandlers<IInteractHander>();
        for (var i = 0; i < targetExit.Count; i++)
            targetExit[i].OnInteractionExit(this);
    }
}