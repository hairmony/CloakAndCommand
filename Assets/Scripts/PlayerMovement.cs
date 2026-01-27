using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D myBody;
    private SpriteRenderer sr;
    private PlayerControls controls;

    [Header("Basic Movement")]
    [SerializeField]
    private float moveForce = 6f;

    public float moveX;
    public float moveY;
    public bool isGrounded = true;
    public bool canMove = true;
    private Vector2 lastMoveDirection = Vector2.right; // Default to right

    [Header("Time Slow")]
    public bool canTimeSlow = false;
    public float slowDuration = 2f;
    public float slowFactor = 0.5f;
    public float slowFactorPlayer = 1f;
    public bool isSlowing = false;
    private float slowTimer = 0f;

    [Header("Roll")]
    public bool canRoll = true;
    public float rollForce = 15f;
    public float rollDuration = 0.25f;
    private float rollTimer = 0.3f;
    public float rollCooldown = 0.5f;
    public bool isRolling = false;
    public float rollCooldownTimer = 0.4f;
    private CapsuleCollider2D collider;
    private Vector2 normalColliderSize;
    private Vector2 normalColliderOffset;
    public Vector2 rollColliderSize = new Vector2(0f, 0f);
    public Vector2 rollColliderOffset = new Vector2(0f, 0f);
    private Vector2 rollDirection;

    private void Awake()
    {
        myBody = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        controls = GetComponent<PlayerControls>();
        collider = GetComponent<CapsuleCollider2D>();
        normalColliderSize = collider.size;
        normalColliderOffset = collider.offset;

        myBody.gravityScale = 0f;
    }

    void Start()
    {

    }

    void Update()
    {
        if (canMove && !isRolling)
            PlayerMoveKeyboard();

        if (moveX != 0 && !isRolling) 
            sr.flipX = moveX < 0;

        // TIME SLOW
        if (canTimeSlow && !isSlowing && controls.fire3Pressed)
        {
            StartTimeSlow();
        }

        if (isSlowing)
        {
            slowTimer -= Time.unscaledDeltaTime;
            if (slowTimer <= 0f)
                EndTimeSlow();
        }

        //myBody.gravityScale = isSlowing ? normalGravity * slowFactorPlayer : normalGravity;

        //ROLL
        if (canRoll && !isRolling && rollCooldownTimer <= 0f && controls.rollPressed && canMove)
        {
            StartRoll();
        }

        // Update roll timer
        if (isRolling)
        {
            rollTimer -= Time.unscaledDeltaTime;
            if (rollTimer <= 0f)
                EndRoll();
        }

        if (rollCooldownTimer > 0f)
            rollCooldownTimer -= Time.unscaledDeltaTime;
    }

    private void FixedUpdate()
    {
        if (isRolling)
        {
            myBody.linearVelocity = rollDirection * rollForce;
        }
    }

    void PlayerMoveKeyboard()
    {
        if (isRolling) return;

        moveX = controls.horizontalInput;
        moveY = Input.GetAxisRaw("Vertical");
        Vector2 moveDirection = new Vector2(moveX, moveY).normalized;

        if (moveDirection != Vector2.zero)
        {
            lastMoveDirection = moveDirection;
        }

        float velocity = moveForce;

        if (isSlowing)
            velocity *= slowFactorPlayer;
        myBody.linearVelocity = moveDirection * velocity;
    }

    void StartTimeSlow()
    {
        isSlowing = true;
        slowTimer = slowDuration;
        moveForce /= (slowFactorPlayer);
        //myBody.gravityScale /= (slowFactorPlayer);

        Time.timeScale = slowFactor;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    void EndTimeSlow()
    {
        isSlowing = false;
        moveForce *= (slowFactorPlayer);

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        //myBody.gravityScale = normalGravity;
    }

    void StartRoll()
    {
        isRolling = true;
        rollTimer = rollDuration;
        rollCooldownTimer = rollCooldown;

        float h = controls.horizontalInput;
        float v = Input.GetAxisRaw("Vertical");
        rollDirection = new Vector2(h, v).normalized;

        // Rolling while not moving
        if (rollDirection == Vector2.zero)
        {
            rollDirection = lastMoveDirection;
        }

        collider.size = rollColliderSize;
        collider.offset = rollColliderOffset;
        canMove = false;
    }

    void EndRoll()
    {
        isRolling = false;
        canMove = true;

        collider.size = normalColliderSize;
        collider.offset = normalColliderOffset;

        //myBody.linearVelocity = new Vector2(0f, myBody.linearVelocity.y);
        myBody.linearVelocity = Vector2.zero;
    }

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    if (collision.gameObject.CompareTag("Ground"))
    //    {
    //        isGrounded = true;
    //        jumpsLeft = canDoubleJump ? extraJumps : 0;
    //    }
    //}

    //private void OnCollisionExit2D(Collision2D collision)
    //{
    //    if (collision.gameObject.CompareTag("Ground"))
    //    {
    //        isGrounded = false;
    //    }
    //}
}