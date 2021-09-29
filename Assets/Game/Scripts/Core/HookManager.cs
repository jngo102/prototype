using System;
using UnityEngine;

public class HookManager
{
    public Hook PlayerTransit;
    public Hook PlayerDeath;
    public Hook PlayerSave;
    public Hook PlayerRecovery;
}

public struct Hook
{
    private Action actions;
    
    public void Invoke()
    {
        if (actions == null) return;

        foreach (Action action in actions.GetInvocationList())
        {
            try { action.Invoke(); }
            catch (Exception e) { Debug.LogError(e); }
        }
    }

    public static Hook operator +(Hook hook, Action action)
    {
        hook.actions += action;
        return hook;
    }

    public static Hook operator -(Hook hook, Action action)
    {
        hook.actions -= action;
        return hook;
    }
}

public struct Hook<T>
{
    private Action<T> actions;
    
    public void Invoke(T args)
    {
        if (actions == null) return;

        foreach (Action<T> action in actions.GetInvocationList())
        {
            try { action.Invoke(args); }
            catch (Exception e) { Debug.LogError(e); }
        }
    }

    public static Hook<T> operator +(Hook<T> hook, Action<T> action)
    {
        hook.actions += action;
        return hook;
    }

    public static Hook<T> operator -(Hook<T> hook, Action<T> action)
    {
        hook.actions -= action;
        return hook;
    }
}