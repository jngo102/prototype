using NaughtyAttributes;
using UnityEngine;

public class PlayerParams : ScriptableObject
{
    [BoxGroup("Movement")] [Label("Gravity (units/sec^2)")] public float gravity = -60f;
    [BoxGroup("Movement")] [Label("Velocity (units/sec)")] public float moveVelocity = 6f;
    [BoxGroup("Movement")] [Label("Max Fall Velocity (units/sec)")] public float fallSpeed = -15f;
    [BoxGroup("Movement")] [Label("Max Fall Velocity Clinging (units/sec)")] public float fallSpeedClinging = -4f;

    [BoxGroup("Jump")] [Label("Height (units)")] public float jumpHeight = 3f;
    [BoxGroup("Jump")] [Label("Variable Time (sec)")] public float jumpTime = 0.15f;
    [BoxGroup("Jump")] [Label("Grace Time (sec)")] public float jumpGraceTime = 0.1f;
    [BoxGroup("Jump")] [Label("Coyote Time (sec)")] public float jumpCoyoteTime = 0.1f;
    [BoxGroup("Jump")] [Label("Wall Push Time (sec)")] public float jumpWallPushTime = 0.15f;

    [BoxGroup("Nail")] [Label("Cooldown (sec)")] public float nailCooldownTime = 0.3f;
    [BoxGroup("Nail")] [Label("Duration (sec)")] public float nailDurationTime = 0.1f;
    [BoxGroup("Nail")] [Label("Grace Time (sec)")] public float nailGraceTime = 0.1f;
    [BoxGroup("Nail")] [Label("Recoil Time (sec)")] public float nailRecoilTime = 0.1f;
    [BoxGroup("Nail")] [Label("Recoil Velocity (units/sec)")] public float nailRecoilVelocity = 8f;
    [BoxGroup("Nail")] [Label("Recoil Jump Height (units)")] public float nailRecoilHeight = 2f;

    [BoxGroup("Damage")] [Label("Invincible Time (sec)")] public float damageInvincibleTime = 0.85f;
    [BoxGroup("Damage")] [Label("Recoil Time (sec)")] public float damageRecoilTime = 0.1f;
    [BoxGroup("Damage")] [Label("Recoil Velocity (units/sec)")] public float damageRecoilVelocity = 8f;

    public float CalcVelocity(float gravity, float jumpTime, float jumpHeight)
    {
        var a = 1 / (2 * Mathf.Abs(gravity));
        var b = jumpTime;
        var c = -jumpHeight;

        var d = b*b - 4*a*c;
        Debug.Assert(d > 0);

        var x1 = (-b + Mathf.Sqrt(d)) / (2 * a);
        
        return x1;
    }

    private void OnValidate()
    {

    }
}