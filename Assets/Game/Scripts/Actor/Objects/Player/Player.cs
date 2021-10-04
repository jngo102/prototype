using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(ActorBody))]
public class Player : MonoBehaviour, IHitHandler, IPreDamageHandler, IDamageHandler, IInteractionHander
{
    // Speeds & Boosts & Times
    const float gravity = -50f;
    const float moveSpeed = 6f;
    const float jumpSpeed = 8f;
    const float jumpHBoost = 0.5f;
    const float jumpWallHBoost = 5f;
    const float fallMaxSpeed = -15f;
    const float fallMaxSpeedClinging = -5f;
    const float recoilHSpeed = 12f;
    const float recoilVSpeed = 8f;

    [SerializeField] private float attackCooldownTime;
    [SerializeField] private float attackTime;

    // Player Input
    private InputManager input;
    private Vector2 inputDirection;
    private bool inputJump;
    private bool inputJumpDown;
    private bool inputAttackDown;
    private bool inputLookUp;
    private bool inputLookDown;

    // Timers
    private Timer jumpGraceTimer = new Timer(.05f);
    private Timer jumpWallGraceTimer = new Timer(.05f);
    private Timer jumpVarTimer = new Timer(.3f);
    private Timer jumpCoyoteTimer = new Timer(.05f);
    private Timer forceDirectionTimer = new Timer();

    private Timer attackCooldownTimer = new Timer();
    private Timer attackTimer = new Timer();

    private Timer recoilTimer = new Timer(.1f);
    private Timer invincibleTimer = new Timer(0.85f);

    // State
    private Vector2 forceDirection;
    private Vector2 finalDirection;
    private Vector2 recoilDirection;
    private float jumpWallDir;
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

        // var hasInput = velocity.sqrMagnitude > 0.01f
                    // || inputAttackDown
                    // || inputJumpDown;

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
        var focusing = ctx.ReadValueAsButton();
        if (focusing)
            PlayerHealth.Instance.StartRestoringHealth(1);
        else
            PlayerHealth.Instance.StopRestoringHealth();
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

        velocity.x = finalDirection.x * moveSpeed;

        // [Collision]

        if (body.collisions.below || body.collisions.above)
        {
            velocity.y = 0;
            jumpVarTimer.Zero();
        }

        if (body.collisions.below)
        {
            jumpCoyoteTimer.Start();
        }

        if ((body.collisions.right || body.collisions.left) && !body.collisions.below)
        {
            jumpWallGraceTimer.Start();
            jumpWallDir = body.collisions.right ? -1 : 1;
        }

        // [Jump]

        if (inputJumpDown)
        {
            inputJumpDown = false;
            jumpGraceTimer.Start();
        }

        if (jumpGraceTimer && jumpCoyoteTimer)
        {
            jumpGraceTimer.Zero();
            jumpVarTimer.Start();

            velocity.x += jumpHBoost * Mathf.Sign(finalDirection.x);
        }

        if (jumpGraceTimer && jumpWallGraceTimer)
        {
            jumpGraceTimer.Zero();
            jumpVarTimer.Start();

            velocity.x += jumpWallHBoost * jumpWallDir;

            forceDirection.x = jumpWallDir;
            forceDirection.y = 0;
            forceDirectionTimer.Start(.25f);
        }

        if (jumpVarTimer)
        {
            if (inputJump)
            {
                velocity.y = jumpSpeed;
            }
            else
            {
                jumpVarTimer.Zero();
            }
        }

        // [Recoil]
        if (recoilTimer)
        {
            if (recoilDirection.x != 0)
                velocity.x = recoilDirection.x * recoilHSpeed;
            
            if (recoilDirection.y != 0)
                velocity.y = recoilDirection.y * recoilVSpeed;
            
            // In case you are on spikes or are dying
            if (recoilDirection == Vector2.zero)
                velocity = Vector2.zero;
        }
        else
        {
            recoilDirection = Vector2.zero;
        }

        // [Movement]

        var clinging = (body.collisions.right && finalDirection.x == 1) || (body.collisions.left && finalDirection.x == -1);
        var maxYSpeed = clinging ? fallMaxSpeedClinging : fallMaxSpeed;
        
        if (!jumpVarTimer && !recoilTimer)
            velocity.y += gravity * Time.deltaTime;

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

            if (!attackCooldownTimer)
            {
                attackCooldownTimer.Start(attackCooldownTime);
                attackTimer.Start(attackTime);

                var weaponTransformAngle = 0;
                if (inputDirection.y > 0) weaponTransformAngle = 90;
                else if (inputDirection.y < 0 && !body.collisions.below) weaponTransformAngle = -90;

                weaponTransform.localRotation = Quaternion.Euler(0, 0, weaponTransformAngle);
                weaponTransform.gameObject.SetActive(true);

                animator.SetTrigger("Slash");
            }

        }

        if (!attackTimer)
        {
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
                    Debug.Log("LastPosition = " + _lastGeometryName);
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
        jumpWallGraceTimer.Update();

        recoilTimer.Update();
        invincibleTimer.Update();

        attackCooldownTimer.Update();
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
        animator.SetBool("isRunning", velocity.x != 0.0f);
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
            forceDirectionTimer.Start(invincibleTimer.time * 2/3);
            forceDirection = Vector2.zero;
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

    public void OnHit(DamageInfo info)
    {
        if (info.Recoil)
        {
            recoilTimer.Start();

            if (inputDirection.y > 0) recoilDirection.y = -1;
            else if (inputDirection.y < 0) recoilDirection.y = +1;
            else recoilDirection.x = -heldTransform.localScale.x;
        }
    }

    public void OnPreDamage(DamageInfo info)
    {
        if (invincibleTimer && info.Type != DamageType.Spikes)
            info.Cancel();
    }

    public void OnDamage(DamageInfo info)
    {
        invincibleTimer.Start();
        PlayerHealth.Instance.InstantlyDrainHealth(1);
        PlayerCameraController.Instance.Shake();

        if (PlayerHealth.Instance.NoHealth)
        {
            recoilTimer.Start();
            recoilDirection = Vector2.zero;
            Main.Hook.PlayerDeath.Invoke();
        }
        else if (info.Type == DamageType.Spikes)
        {
            recoilTimer.Start();
            recoilDirection = Vector2.zero;

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

            recoilTimer.Start();
            recoilDirection = delta;
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