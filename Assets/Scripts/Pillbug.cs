using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pillbug : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;

    // Physics
    private Vector3 myGravity;
    private float mass = 0.5f;
    private float gravityScale = 2.5f;


    public float moveSpeed = 1f;
    public float moveDelay = 2f;

    private bool facingRight = true;

    public LayerMask whatIsObject;
    public LayerMask whatIsGround;
    public LayerMask whatIsScary;

    private float frontCheckDistance = 1f;
    private float groundCheckDistance = 1f;
    private float edgeCheckDistance = 1f;

    private bool isNearScary;
    private bool isNearObject;
    private bool isNearEdge;
    private bool isGrounded;
    private bool isBalled = true;

    private float timer;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        timer = moveDelay;

        rb.mass = mass;
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        myGravity = Physics2D.gravity;

        // Tweak colors
        GetComponent<SpriteRenderer>().color = new Color(Random.Range(0.3f, 0.7f), Random.Range(0.3f, 0.7f), 1f, 1f);


    }

    // Update is called once per frame
    void Update()
    {
        int facing = facingRight ? 1 : -1;
        isNearScary = Physics2D.OverlapCircle(transform.position, frontCheckDistance, whatIsScary);
        isNearObject = Physics2D.Raycast(transform.position, facing * Vector2.right, frontCheckDistance, whatIsObject);
        isNearEdge = !Physics2D.Raycast(transform.position + (Vector3) (facing * Vector2.right), Vector2.down, edgeCheckDistance, whatIsObject);
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);

        Vector2 newVelocity = rb.velocity;

        if (isNearScary && !isBalled)
        {
            BallUp();
            newVelocity.x = 0;
        }

        if (timer > 0)
            timer -= Time.deltaTime;
        else
        {
            if (isGrounded)
            {
                isBalled = false;
                rb.transform.rotation = Quaternion.AngleAxis(0, Vector3.forward);
                rb.freezeRotation = true;
                if (!isNearObject && !isNearEdge)  // ok to move
                {
                    newVelocity.x = facing * moveSpeed;
                }
                else
                {
                    Flip();
                }

            }
            else //ball up
            {
                BallUp();
                newVelocity.x = 0;
            }

        }

        // Bump check
        if (isBalled)
        {
            Collider2D collider = Physics2D.OverlapCircle(transform.position, frontCheckDistance / 10f, whatIsScary);
            if (collider)
            {
                Vector2 pt = collider.ClosestPoint(transform.position);
                Rigidbody2D otherRigidbody = collider.gameObject.GetComponent<Rigidbody2D>();

                if (otherRigidbody != null)
                {
                    newVelocity.x = 0.25f * otherRigidbody.velocity.x;
                }
            }
        }

        rb.velocity = newVelocity;
        animator.SetBool("isBalled", isBalled);

    }

    private void FixedUpdate()
    {
        rb.AddForce(mass * gravityScale * myGravity);
    }

    void BallUp()
    {
        isBalled = true;
        timer = moveDelay;
        if (Random.Range(0, 2) == 1)
        {
            Flip();
        }
        rb.freezeRotation = false;
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
        Gizmos.DrawWireSphere(transform.position, frontCheckDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)Vector3.down * groundCheckDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)(facing * Vector2.right * frontCheckDistance));
    }


}
