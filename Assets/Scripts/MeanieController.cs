using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeanieController : MonoBehaviour
{
    //Rigidbody2D myBody;
    Animator animator;

    private int maxHealth = 2;
    private int currentHealth;

    public Transform frontCheck;
    public LayerMask whatIsBlocking;
    public Transform groundCheck;
    public Transform headCheck;
    public LayerMask whatIsGround;

    public GameObject burstPrefab;
    public GameObject puffPrefab;
    public GameObject flashPrefab;

    public bool moveEnabled = false;
    public float moveRange = 0f;        // zero means free ranging
    public float walkSpeed = 3f;
    public float stopTime = 1f;

    bool moving = false;
    bool facingRight = true;
    float timer = 0;
    float rangeLeft = 0;
    float rangeRight = 0;
    float checkRadius = 0.3f;

    enum AnimationType
    {
        IDLE,
        WALK
    }

    AnimationType currentAnim;

    void Awake()
    {
        //myBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        timer = stopTime;
        if (moveRange < 0)
        {
            Flip();
            rangeRight = transform.position.x;
            rangeLeft = rangeRight + moveRange;
        }
        else if (moveRange > 0)
        {
            rangeLeft = transform.position.x;
            rangeRight = rangeLeft + moveRange;
        }
        else // free range
        {
            rangeLeft = Mathf.NegativeInfinity;
            rangeRight = Mathf.Infinity;
        }
        currentHealth = maxHealth;
    }

    void FixedUpdate()
    {
        bool isGrounded = true;
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);
        }
        bool isBlocked = false;
        if (frontCheck != null)
        {
            isBlocked = Physics2D.OverlapCircle(frontCheck.position, checkRadius, whatIsBlocking);
        }

        if (moveEnabled)
        {
            if (moving)
            {
                ChangeAnimation(AnimationType.WALK);
                // assuming simple horizontal movement for now
                bool stopNow = !isGrounded;
                bool turnAround = false;
                if (isGrounded)
                {
                    if (isBlocked)
                    {
                        stopNow = true;
                        turnAround = true;
                    }
                    else
                    {
                        Vector2 position = transform.position;
                        float delta = walkSpeed * Time.fixedDeltaTime;
                        position.x += facingRight ? delta : -delta;
                        if (facingRight && (position.x >= rangeRight))
                        {
                            position.x = rangeRight;
                            stopNow = true;
                            turnAround = true;
                        }
                        else if (!facingRight && (position.x <= rangeLeft))
                        {
                            position.x = rangeLeft;
                            stopNow = true;
                            turnAround = true;
                        }
                        transform.position = position;
                    }
                }
                // decrement timer and stop moving when expired
                if (stopNow)
                {
                    moving = false;
                    timer = stopTime;
                }
                if (turnAround)
                {
                    Flip();
                }
            }
            else
            {
                ChangeAnimation(AnimationType.IDLE);
                // decrement timer and start moving when expired
                timer -= Time.fixedDeltaTime;
                if (timer <= 0)
                {
                    moving = true;
                }
            }
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    public void GetHurt(int damage = 1)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            DieInBurst();
        }
        else
        {
            Instantiate(flashPrefab, transform.position + 0.5f * Vector3.up, transform.rotation, gameObject.transform);
        }
    }

    void ChangeAnimation(AnimationType anim, bool restart = false)
    {
        // avoid restarting same animation unintentionally
        if ((currentAnim == anim) && !restart) return;

        if (anim == AnimationType.WALK) animator.Play("MeanieWalk");
        else animator.Play("MeanieIdle");

        // record current animation
        currentAnim = anim;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.gameObject.CompareTag("PlayerAttack"))
        {
            GetHurt();
        }
        else if (collision.collider.gameObject.CompareTag("MovableObject"))
        {
            HandleCrushable(collision);
        }
    }

    private void HandleCrushable(Collision2D collision)
    {
        bool checkHighLeft = collision.collider.OverlapPoint(headCheck.position + checkRadius * Vector3.up + checkRadius * Vector3.left);
        bool checkHighRight = collision.collider.OverlapPoint(headCheck.position + checkRadius * Vector3.up + checkRadius * Vector3.right);

        if (checkHighLeft || checkHighRight) // The other collider is touching head
        {
            Rigidbody2D rb = collision.rigidbody;

            if (rb != null)
            {
                if (rb.velocity.y <= 0 && rb.mass > 20f)
                {
                    bool isGrounded = true;
                    if (groundCheck != null)
                    {
                        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);
                    }

                    if (isGrounded)
                    {
                        DieInPuff();
                    }
                }
            }

        }

    }

    public void DieInBurst()
    {
        Instantiate(burstPrefab, transform.position + 0.5f*Vector3.up, transform.rotation);
        Destroy(gameObject);
    }

    public void DieInPuff()
    {
        Instantiate(puffPrefab, transform.position + 0.5f * Vector3.up, transform.rotation);
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }
        if (frontCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(frontCheck.position, checkRadius);
        }
        if (headCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(headCheck.position, checkRadius);
        }
    }
}
