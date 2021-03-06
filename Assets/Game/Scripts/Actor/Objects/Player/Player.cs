using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(ActorBody))]
public class Player : MonoBehaviour, IPreHitHandler, IHitHandler, IPreDamageHandler, IDamageHandler, IInteractionHander
{
    [SerializeField]
    private PlayerParams settings;

    // Player Input
    private InputManager input;
    private Vector2 inputDirection;
    private bool inputJump;
    private bool inputJumpDown;
    private bool inputAttackDown;
    private bool inputLookUp;
    private bool inputLookDown;

    // Timers
    private Timer jumpGraceTimer = new Timer();
    private Timer wallJumpCoyoteTimer = new Timer();
    private Timer jumpVarTimer = new Timer();
    private Timer jumpCoyoteTimer = new Timer();
    private Timer forceDirectionTimer = new Timer();

    private Timer attackGraceTimer = new Timer();
    private Timer attackCooldownTimer = new Timer();
    private Timer attackTimer = new Timer();

    private Timer recoilTimer = new Timer();
    private Timer invincibleTimer = new Timer();

    // State
    private Vector2 forceDirection;
    private Vector2 finalDirection;
    private Vector2 recoilVelocity;
    private DamageDirection attackDirection;
    private float wallJumpDir;
    private Vector2 velocity;
    private int lookAt;

    private bool isOnBench;














    private ActorBody body;
    private Animator animator;
    
    [SerializeField] Transform heldTransform;
    [SerializeField] Transform weaponTransform;


    private Bounds _lastGeometryBounds;
    private Bounds _lastPlayerBounds;
    private string _lastGeometryName = string.Empty;


    private InteractionInfo _interaction;



    #region Initialize

    private void Awake()
    {
        input = Main.Input;

        if (input != null)
        {
            input.Player.Attack.performed += OnAttack;
            input.Player.Focus.performed += OnFocus;
            input.Player.Jump.performed += OnJump;
            input.Player.Trigger.performed += OnTrigger;
        }
        else
        {
            Debug.LogWarning("Main.Input isn't found");
        }

        body = GetComponent<ActorBody>();
        animator = GetComponentInChildren<Animator>();
    }

    private void OnDestory()
    {
        if (input != null)
        {
            input.Player.Attack.performed -= OnAttack;
            input.Player.Focus.performed -= OnFocus;
            input.Player.Jump.performed -= OnJump;
            input.Player.Trigger.performed -= OnTrigger;
        }
    }

    #endregion

    #region Input

    private void UpdateInput()
    {
        var direction = input.Player.Direction.ReadValue<Vector2>();
        inputDirection.x = Mathf.Abs(direction.x) > 0.5f ? Mathf.Sign(direction.x) : 0;
        inputDirection.y = Mathf.Abs(direction.y) > 0.5f ? Mathf.Sign(direction.y) : 0;

        if (_interaction != null)
        {
            if (isOnBench)
            {
                if (inputAttackDown || inputJumpDown || inputDirection.x != 0 || input.Player.Trigger.triggered)
                {
                    _interaction.Trigger();
                }
            }
            else
            {
                if (input.Player.Trigger.triggered)
                {
                    _interaction.Trigger();
                }
            }
        }

        if (velocity.sqrMagnitude < 0.01f)
        {
            if (input.Player.LookUp.triggered)
                inputLookUp = true;

            if (input.Player.LookDown.triggered)
                inputLookDown = true;
        }

        if (input.Player.LookUp.phase != InputActionPhase.Performed)
            inputLookUp = false;

        if (input.Player.LookDown.phase != InputActionPhase.Performed)
            inputLookDown = false;

        lookAt = (inputLookUp ? +1 : 0) + (inputLookDown ? -1 : 0);
    }

    private void OnAttack(InputAction.CallbackContext ctx)
    {
        inputAttackDown = ctx.ReadValueAsButton();
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        inputJump = inputJumpDown = ctx.ReadValueAsButton();
    }

    private void OnFocus(InputAction.CallbackContext ctx)
    {
        // var focusing = ctx.ReadValueAsButton();
        // if (focusing)
        //     PlayerHealth.Instance.StartRestoringHealth(1);
        // else
        //     PlayerHealth.Instance.StopRestoringHealth();
    }

    private void OnTrigger(InputAction.CallbackContext ctx)
    {
        // if (_interaction != null)
        // {
            // if (_interaction.State == InteractionState.Possible)
                // _interaction.Activate();
            // else if (_interaction.State == InteractionState.Active)
                // _interaction.Deactivate();
        // }
    }

    #endregion

    #region Update

    private void Update()
    {
        // NB! The order is important!

        UpdateInput();
        UpdateMovement();
        UpdateAttack();
        UpdateHealth();
    }

    private void UpdateMovement()
    {
        // [Input]

        finalDirection = forceDirectionTimer ? forceDirection : inputDirection;

        velocity.x = finalDirection.x * settings.moveVelocity;

        // [Collision]

        if (body.collisions.below || body.collisions.above)
        {
            velocity.y = 0;
            jumpVarTimer.Zero();
        }

        if (body.collisions.below)
        {
            jumpCoyoteTimer.Start(settings.jumpCoyoteTime);
        }

        if ((body.collisions.right || body.collisions.left) && !body.collisions.below)
        {
            wallJumpCoyoteTimer.Start(settings.jumpCoyoteTime);
            wallJumpDir = body.collisions.right ? -1 : 1;
        }

        // [Jump]

        if (inputJumpDown)
        {
            inputJumpDown = false;
            jumpGraceTimer.Start(settings.jumpGraceTime);
        }

        // Do jump
        if (jumpGraceTimer && jumpCoyoteTimer)
        {
            jumpGraceTimer.Zero();
            jumpCoyoteTimer.Zero();
            jumpVarTimer.Start(settings.jumpTime);
        }

        // Do wall jump
        else if (jumpGraceTimer && wallJumpCoyoteTimer)
        {
            jumpGraceTimer.Zero();
            wallJumpCoyoteTimer.Zero();
            jumpVarTimer.Start(settings.jumpTime);

            forceDirection.x = wallJumpDir;
            forceDirection.y = 0;
            forceDirectionTimer.Start(settings.jumpWallPushTime);

            // TODO : MOVE
            // velocity.x += settings.wallJumpHeight * wallJumpDir;
        }

        // Continue jumping (jump higher)
        if (jumpVarTimer && inputJump)
        {
            velocity.y = settings.CalcVelocity(settings.gravity, settings.jumpTime, settings.jumpHeight);
        }
        else
        {
            jumpVarTimer.Zero();
        }

        // [Recoil]
        if (recoilTimer)
        {
            if (recoilVelocity.x != 0)
                velocity.x = recoilVelocity.x;
            
            if (recoilVelocity.y != 0)
                velocity.y = recoilVelocity.y;
            
            // In case you are on spikes or are dying
            if (recoilVelocity == Vector2.zero)
                velocity = Vector2.zero;
        }
        else
        {
            recoilVelocity = Vector2.zero;
        }

        // [Movement]

        var clinging = (body.collisions.right && finalDirection.x == 1) || (body.collisions.left && finalDirection.x == -1);
        var maxYSpeed = clinging ? settings.fallSpeedClinging : settings.fallSpeed;
        
        if (!jumpVarTimer && !recoilTimer)
            velocity.y += settings.gravity * Time.deltaTime;

        velocity.y = Mathf.Max(velocity.y, maxYSpeed);

        var prevPosition = transform.position;
        body.Move(velocity * Time.deltaTime);
        velocity = (transform.position - prevPosition) / Time.deltaTime;
    }

    private void UpdateAttack()
    {
        if (inputAttackDown)
        {
            inputAttackDown = false;
            attackGraceTimer.Start(settings.nailGraceTime);
        }

        if (attackGraceTimer && !attackCooldownTimer)
        {
            attackCooldownTimer.Start(settings.nailCooldownTime);
            attackTimer.Start(settings.nailDurationTime);

            // Define attack direction
            if (inputDirection.y > 0) attackDirection = DamageDirection.Up;
            else if (inputDirection.y < 0 && !body.collisions.below) attackDirection = DamageDirection.Down;
            else if (heldTransform.localScale.x < 0) attackDirection = DamageDirection.Left;
            else attackDirection = DamageDirection.Right;

            // Rotate nail collider
            var weaponTransformAngle = 0;
            if (attackDirection == DamageDirection.Up) weaponTransformAngle = 90;
            else if (attackDirection == DamageDirection.Down) weaponTransformAngle = -90;
            weaponTransform.localRotation = Quaternion.Euler(0, 0, weaponTransformAngle);
            weaponTransform.gameObject.SetActive(true);

            animator.SetTrigger("Slash");
        }

        if (!attackTimer)
        {
            attackDirection = DamageDirection.Unknown;
            weaponTransform.gameObject.SetActive(false);
            animator.ResetTrigger("Slash");
        }
    }

    private void UpdateHealth()
    {
        var noHealth = PlayerHealth.Instance.NoHealth;

        PlayerHealth.Instance.Update(Time.deltaTime);

        if (noHealth != PlayerHealth.Instance.NoHealth)
            Main.Hook.PlayerDeath.Invoke();
    }

    #endregion

    private void LateUpdate()
    {
        ProcessSpikes();

        SyncTimers();
        SyncRotation(finalDirection.x);
        SyncAnimator();
        SyncCamera();
    }

    private void ProcessSpikes()
    {
        for (var i = 0; i < body.collisions.belowColliders.Count; i++)
        {
            var collider = body.collisions.belowColliders[i];
            var geometry = collider.GetComponent<LevelGeometry>();
            if (geometry != null && geometry.Type == LevelGeometry.GeometryType.Solid)
            {
                if (_lastGeometryName != geometry.name)
                {
                    _lastGeometryName = geometry.name;
                    // Debug.Log("LastPosition = " + _lastGeometryName);
                }

                _lastGeometryBounds = geometry.Bounds;
                _lastPlayerBounds = body.collider.bounds;
                return;
            }
        }
    }

    private void SyncTimers()
    {
        forceDirectionTimer.Update();

        jumpGraceTimer.Update();
        jumpCoyoteTimer.Update();
        jumpVarTimer.Update();

        wallJumpCoyoteTimer.Update();

        recoilTimer.Update();
        invincibleTimer.Update();

        attackCooldownTimer.Update();
        attackGraceTimer.Update();
        attackTimer.Update();
    }

    private void SyncRotation(float direction = 0)
    {
        var currDir = Sign(heldTransform.lossyScale.x);
        var nextDir = Sign(direction);
        if (nextDir == 0)
            return;

        if (currDir != nextDir)
        {
            // Held
            var heldScale = heldTransform.localScale;
            heldScale.x *= -1;
            heldTransform.localScale = heldScale;

            // Animator
            if (body.collisions.below)
                animator.SetTrigger("Turn");
        }
    }

    private void SyncAnimator()
    {
        animator.SetFloat("vertSpeed", velocity.y);
        animator.SetBool("isRunning", finalDirection.x != 0.0f);
        animator.SetBool("isGrounded", body.collisions.below);
        animator.SetBool("isInvincible", invincibleTimer);
        animator.SetBool("isOnBench", isOnBench);
        animator.SetInteger("vertLookAt", lookAt);
    }

    private void SyncCamera()
    {
        GetComponent<PlayerCameraController>().LookAt(lookAt);
    }

    //
    // Public API
    //

    public void Setup(Vector2 position, float direction = 0, bool force = false, bool isOnBench = false)
    {
        animator.Play("idle");
        
        velocity = Vector2.zero;
        transform.position = position;
        Physics2D.SyncTransforms();

        this.isOnBench = isOnBench;

        if (force)
        {
            forceDirectionTimer.Start(1/6f);

            if (direction == 0)
                forceDirection = Vector2.zero;
            else if (direction < 0)
                forceDirection = Vector2.left;
            else
                forceDirection = Vector2.right;
        }

        GetComponent<PlayerCameraController>().Instant();
        SyncRotation(direction);
    }

    //
    // Utils
    //

    private static int Sign(float value)
    {
        if (value > 0) return +1;
        if (value < 0) return -1;
        return 0;
    }

    private static Vector2 GetRestorePosition(Bounds platform, Bounds player)
    {
        var deltaX = platform.center.x - player.center.x;
        var baseX = deltaX > 0 ? platform.min.x : platform.max.x;
        var offsetX = Mathf.Sign(deltaX) * (player.extents.x + 0.5f);

        var baseY = platform.max.y;
        var offsetY = player.extents.y;

        return new Vector2(
            baseX + offsetX,
            baseY + offsetY
        );
    }

    //
    // Hit & Damage
    //

    public void OnPreHit(DamageInfo info)
    {
        if (info.Type == DamageType.Slash)
            info.Direction = attackDirection;
    }

    public void OnHit(DamageInfo info)
    {
        if (info.Recoil)
        {
            recoilTimer.Start(settings.nailRecoilTime);

            var direction = info.Direction.GetVector();
            if (direction.y != 0)
                recoilVelocity = -direction * settings.CalcVelocity(settings.gravity, settings.nailRecoilTime, settings.nailRecoilHeight);
            else if (direction.x != 0)
                recoilVelocity = -direction * settings.nailRecoilVelocity;
        }
    }

    public void OnPreDamage(DamageInfo info)
    {
        if (invincibleTimer && info.Type != DamageType.Spikes)
            info.Cancel();
    }

    public void OnDamage(DamageInfo info)
    {
        invincibleTimer.Start(settings.damageInvincibleTime);
        PlayerHealth.Instance.InstantlyDrainHealth(1);
        PlayerCameraController.Instance.Shake();

        if (PlayerHealth.Instance.NoHealth)
        {
            recoilTimer.Start(settings.damageRecoilTime);
            recoilVelocity = Vector2.zero;
            Main.Hook.PlayerDeath.Invoke();
        }
        else if (info.Type == DamageType.Spikes)
        {
            recoilTimer.Start(settings.damageRecoilTime);
            recoilVelocity = Vector2.zero;

            Main.Hook.PlayerRecovery.Invoke(
                new PlayerRecoveryArgs(
                    GetRestorePosition(_lastGeometryBounds, _lastPlayerBounds)
                    - Vector2.up * body.collider.offset.y
                )
            );
        }
        else
        {
            var delta = info.Target.transform.position - info.Source.transform.position;

            var horizontal = body.collisions.below || Mathf.Abs(delta.x) > Mathf.Abs(delta.y);
            delta.x = horizontal ? Sign(delta.x) : 0;
            delta.y = horizontal ? 0 : Sign(delta.y);

            recoilTimer.Start(settings.damageRecoilTime);
            recoilVelocity = delta * settings.damageRecoilVelocity;
        }
    }

    //
    // Interact
    //

    public void OnInteractionEnter(InteractionInfo info)
    {
        _interaction = info;
    }

    public void OnInteractionTrigger(InteractionInfo info)
    {
        isOnBench = !isOnBench;
    }

    public void OnInteractionExit(InteractionInfo info)
    {
        _interaction = null;
    }
}