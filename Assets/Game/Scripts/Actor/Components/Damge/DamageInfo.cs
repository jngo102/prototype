
//
// Handlers
//

using UnityEngine;

public interface IPreDamageHandler
{
    void OnPreDamage(DamageInfo info);
}

public interface IDamageHandler
{
    void OnDamage(DamageInfo info);
}

public interface IPostDamageHandler
{
    void OnPostDamage(DamageInfo info);
}

public interface IPreHitHandler
{
    void OnPreHit(DamageInfo info);
}

public interface IHitHandler
{
    void OnHit(DamageInfo info);
}

public interface IPostHitHandler
{
    void OnPostHit(DamageInfo info);
}

//
// DamageInfo
//

public class DamageInfo
{
    public Actor Source { get; }
    public Actor Target { get; }
    public DamageType Type { get; set; }
    public bool Canceled { get; private set; }

    public DamageInfo(Actor source, Actor target)
    {
        Source = source;
        Target = target;
    }

    public void Cancel()
    {
        Canceled = true;
    }

    public void Process()
    {
        // [Pre]

        var sourcePreHit = Source.GetHandlers<IPreHitHandler>();
        for (var j = 0; j < sourcePreHit.Count; j++)
            sourcePreHit[j].OnPreHit(this);

        var targetPreDamage = Target.GetHandlers<IPreDamageHandler>();
        for (var j = 0; j < targetPreDamage.Count; j++)
            targetPreDamage[j].OnPreDamage(this);

        // Cancel is available in any Pre- handler
        if (Canceled)
            return;

        // [Do]

        var sourceHit = Source.GetHandlers<IHitHandler>();
        for (var j = 0; j < sourceHit.Count; j++)
            sourceHit[j].OnHit(this);

        var targetDamage = Target.GetHandlers<IDamageHandler>();
        for (var j = 0; j < targetDamage.Count; j++)
            targetDamage[j].OnDamage(this);

        // [Post]

        var sourcePostHit = Source.GetHandlers<IPostHitHandler>();
        for (var j = 0; j < sourcePostHit.Count; j++)
            sourcePostHit[j].OnPostHit(this);

        var targetPostDamage = Target.GetHandlers<IPostDamageHandler>();
        for (var j = 0; j < targetPostDamage.Count; j++)
            targetPostDamage[j].OnPostDamage(this);

        Debug.Assert(!Canceled, "Damage can be canceled only in Pre- phase");
    }
}