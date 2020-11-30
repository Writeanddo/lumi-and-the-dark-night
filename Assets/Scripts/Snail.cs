using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snail : MonoBehaviour
{
    private Animator animator;

    private float moveDelay = 1f;
    private float moveIncrement = 0.15f;

    public Transform frontCheck;
    public Transform groundCheck;
    public LayerMask whatIsObject;
    public LayerMask whatIsGround;

    private bool facingRight = true;
    //private bool lastCheck = false;

    private float checkRadius = 0.15f;

    private bool isNearObject;
    private bool isNearEdge;
    private bool isGrounded;

    private float timer;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        timer = moveDelay;

        // Tweak colors
        GetComponent<SpriteRenderer>().color = new Color(Random.Range(0.75f, 1f), Random.Range(0.75f, 1f), Random.Range(0.75f, 1f), 1f);

    }

    // Update is called once per frame
    void Update()
    {
        int facing = facingRight ? 1 : -1;
        isNearObject = Physics2D.OverlapCircle(frontCheck.position, checkRadius, whatIsObject);
        isNearEdge = !Physics2D.Raycast(frontCheck.position, Vector2.down, 0.5f, whatIsObject);
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);

        if (timer > 0)
            timer -= Time.deltaTime;
        else
        {
            if (isGrounded)
            {
                if (!isNearObject && !isNearEdge)  // ok to move
                {
                    transform.position = transform.position + (Vector3.right * facing * moveIncrement);
                    animator.Play("Snail");
                }
                else
                {
                    Flip();
                }
                timer = moveDelay;

            }
        }

        //lastCheck = isNearObject || isNearEdge;

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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, checkRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(frontCheck.position, checkRadius);
    }
}
