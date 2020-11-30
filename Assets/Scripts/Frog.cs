using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Frog : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;

    private float jumpSpeed = 5f;
    private float jumpDelay = 1f;
    private float jumpTimer = 0f;

    private bool facingRight = true;

    public Transform groundCheck;
    private float groundCheckDistance = 0.10f;

    public LayerMask whatIsObject;
    public LayerMask whatIsGround;
    public LayerMask whatIsScary;

    private float frontCheckDistance = 1f;
    private float edgeCheckDistance = 1f;

    private bool isNearScary;
    private bool isNearObject;
    private bool isNearEdge;
    private bool isGrounded;

    private bool isJumping = false;

    private float flipTimer = 0f;
    private float flipCooldown = 0.5f;

    private float stuckTimer = 0f;
    private float stuckTimeLimit = 3f;

    // Start is called before the first frame update
    void Start()
    {

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Tweak colors
        GetComponent<SpriteRenderer>().color = new Color(Random.Range(0.4f, 0.6f), Random.Range(0.65f, 0.85f), Random.Range(0.4f, 0.6f), 1f);
    }

    // Update is called once per frame
    void Update()
    {

        if (jumpTimer > 0)
        {
            jumpTimer -= Time.deltaTime;
        }

        if (flipTimer > 0)
        {
            flipTimer -= Time.deltaTime;
        }

        animator.SetBool("isJumping", isJumping);
    }

    private void FixedUpdate()
    {
        int facing = facingRight ? 1 : -1;
        isNearScary = Physics2D.OverlapCircle(transform.position, frontCheckDistance, whatIsScary);
        isNearObject = Physics2D.Raycast(transform.position, facing * Vector2.right, frontCheckDistance, whatIsObject);
        isNearEdge = !Physics2D.Raycast(transform.position + (Vector3)(facing * Vector2.right), Vector2.down, edgeCheckDistance, whatIsObject);
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckDistance, whatIsGround);

        Vector2 newVelocity = rb.velocity;

        if (isGrounded)
        {
            if (isNearObject || isNearEdge)
            {
                if (flipTimer <= 0)
                {
                    Flip();
                    flipTimer = flipCooldown;
                }
            }
            else if (jumpTimer <= 0)
            {
                // jump
                newVelocity.x = facing * 0.5f * jumpSpeed;
                newVelocity.y = jumpSpeed;
                jumpTimer = jumpDelay;
            }
        }
        else if (jumpTimer <= 0)
        {
            // not grounded and ready to jump

            if (rb.velocity.magnitude < 0.05f)
            {
                // stuck on something
                stuckTimer += Time.fixedDeltaTime;
                if (stuckTimer > stuckTimeLimit)
                {
                    // jump
                    newVelocity.x = facing * 0.5f * jumpSpeed;
                    newVelocity.y = jumpSpeed;
                    jumpTimer = jumpDelay;
                    stuckTimer = 0;
                }
            }
        }


        isJumping = !isGrounded;

        rb.velocity = newVelocity;
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 Scaler = transform.localScale;
        Scaler.x *= -1;
        transform.localScale = Scaler;
    }

    private void OnDrawGizmosSelected()
    {
        int facing = facingRight ? 1 : -1;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)Vector3.right * frontCheckDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + (Vector3)(facing * Vector2.right), transform.position + (Vector3)(facing * Vector2.right) + (Vector3)(Vector2.down * edgeCheckDistance));
    }


}
