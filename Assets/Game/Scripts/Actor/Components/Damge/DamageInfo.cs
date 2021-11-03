using UnityEngine;

//
// Handlers
//

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
    public DamageDirection Direction { get; set; }
    public bool Canceled { get; private set; }
    public bool Recoil { get; set; } = true;

    public DamageInfo(Actor source, Actor target)
    {
        Debug.Assert(source != null);
        Debug.Assert(target != null);

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
        for (var i = 0; i < sourcePreHit.Length; i++)
            sourcePreHit[i].OnPreHit(this);

        var targetPreDamage = Target.GetHandlers<IPreDamageHandler>();
        for (var i = 0; i < targetPreDamage.Length; i++)
            targetPreDamage[i].OnPreDamage(this);

        // Cancel is available in any Pre- handler
        if (Canceled)
            return;

        // [Do]

        var sourceHit = Source.GetHandlers<IHitHandler>();
        for (var i = 0; i < sourceHit.Length; i++)
            sourceHit[i].OnHit(this);

        var targetDamage = Target.GetHandlers<IDamageHandler>();
        for (var i = 0; i < targetDamage.Length; i++)
            targetDamage[i].OnDamage(this);

        // [Post]

        var sourcePostHit = Source.GetHandlers<IPostHitHandler>();
        for (var i = 0; i < sourcePostHit.Length; i++)
            sourcePostHit[i].OnPostHit(this);

        var targetPostDamage = Target.GetHandlers<IPostDamageHandler>();
        for (var i = 0; i < targetPostDamage.Length; i++)
            targetPostDamage[i].OnPostDamage(this);

        Debug.Assert(!Canceled, "Damage can be canceled only in Pre- phase");
    }
}