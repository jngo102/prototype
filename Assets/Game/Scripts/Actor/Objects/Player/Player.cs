using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(ActorBody))]
public class Player : MonoBehaviour, IHitHandler, IPreDamageHandler, IDamageHandler
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










    private ActorBody body;
    private Animator animator;
    
    [SerializeField] Transform heldTransform;
    [SerializeField] Transform weaponTransform;


    private Bounds _lastGeometryBounds;
    private Bounds _lastPlayerBounds;



    #region Initialize

    private void Awake()
    {
        input = Main.Input;

        if (input != null)
        {
            input.Player.Attack.performed += OnAttack;
            input.Player.Focus.performed += OnFocus;
            input.Player.Jump.performed += OnJump;
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
        }
    }

    #endregion

    #region Input

    private void UpdateInput()
    {
        var direction = input.Player.Direction.ReadValue<Vector2>();
        inputDirection.x = Mathf.Abs(direction.x) > 0.5f ? Mathf.Sign(direction.x) : 0;
        inputDirection.y = Mathf.Abs(direction.y) > 0.5f ? Mathf.Sign(direction.y) : 0;

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
                else if (inputDirection.y < 0) weaponTransformAngle = -90;

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
        PlayerHealth.Instance.Update(Time.deltaTime);
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

    private string lastName = string.Empty;

    private void ProcessSpikes()
    {
        for (var i = 0; i < body.collisions.belowColliders.Count; i++)
        {
            var collider = body.collisions.belowColliders[i];
            var geometry = collider.GetComponent<LevelGeometry>();
            if (geometry != null && geometry.Type == LevelGeometry.GeometryType.Solid)
            {
                if (lastName != geometry.name)
                {
                    lastName = geometry.name;
                    Debug.Log("LastPosition = " + lastName);
                }

                _lastGeometryBounds = geometry.Bounds;
                _lastPlayerBounds = body.collider.bounds;
                return;
            }
        }
    }

    public void SyncTimers()
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

    public void SyncRotation(float direction = 0)
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
        animator.SetInteger("vertLookAt", lookAt);
    }

    private void SyncCamera()
    {
        var camera = GetComponent<PlayerCameraController>();

        camera.LookAt(lookAt);
    }

    public void SetPosition(Vector2 position)
    {
        animator.Play("idle");

        velocity = Vector2.zero;
        transform.position = position;
        Physics2D.SyncTransforms();

        forceDirectionTimer.Start(invincibleTimer.time * 2/3);
        forceDirection = Vector2.zero;

        GetComponent<PlayerCameraController>().Setup(
            Camera.main,
            this,
            FindObjectOfType<LevelBounds>()
        );
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
        var offsetX = Mathf.Sign(deltaX) * (player.extents.x + 1f);

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

        if (info.Type == DamageType.Spikes)
        {
            recoilTimer.Start();
            recoilDirection = Vector2.zero;

            var restorePosition = GetRestorePosition(_lastGeometryBounds, _lastPlayerBounds)
                                - Vector2.up * body.collider.offset.y;

            FindObjectOfType<LevelMessageBus>().SendMessage(
                RestoreMessage.Name,
                new RestoreMessage(
                    restorePosition
                )
            );
        }
        else
        {
            var delta = info.Target.transform.position - info.Source.transform.position;
            var deltaIsHorizontal = Mathf.Abs(delta.x) > Mathf.Abs(delta.y);
            if (deltaIsHorizontal || body.collisions.below)
            {
                delta.x = Sign(delta.x);
                delta.y = 0;
            }
            else 
            {
                delta.x = 0;
                delta.y = Sign(delta.y);
            }

            recoilTimer.Start();
            recoilDirection = delta;
        }

        PlayerHealth.Instance.StartDrainingHealth(1);
        PlayerCameraController.Instance.Shake();
    }
}
