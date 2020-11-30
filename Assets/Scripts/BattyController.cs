using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattyController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;
    private GameObject player;

    private int maxHealth = 2;
    private int currentHealth;

    private float speed = 2f;
    private float diveSpeed = 14f;

    public LayerMask whatIsObject;
    public LayerMask whatIsGround;

    private float frontCheckDistance = 1.0f;
    public float targetHeightMin = 5f;
    public float targetHeightMax = 6f;
    private float attackDistance;
    private Vector2 attackVector;

    public bool startFacingLeft = false;
    private bool facingRight = true;
    private bool movingUp = true;

    private bool isNearObject;
    private bool isTooLow;
    private bool isTooHigh;
    private bool isDiving = false;
    private bool canSeePlayer;

    private float diveCooldown = 2f;
    private float diveDelay = 0.25f;
    private float diveTimer = 0f;

    private float huntTimer = 0f;
    private float huntDuration = 5f;

    public bool inFreeFlightMode = false;

    private float flipTimer = 0f;
    private float flipCooldown = 1f;

    private Vector3 startingPosition;
    public float moveDistance = 16f;

    public GameObject burstPrefab;
    public GameObject flashPrefab;
    public AudioClip attackSound;

    private void Start()
    {
        if (startFacingLeft)
        {
            Flip();
        }

        attackDistance = 1.4f * targetHeightMax;
        startingPosition = transform.position;

        player = GameObject.Find("Player");

        currentHealth = maxHealth;
    }

    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (diveTimer > 0)
        {
            diveTimer -= Time.deltaTime;
        }

        if (huntTimer > 0)
        {
            huntTimer -= Time.deltaTime;
        }

        if (flipTimer > 0)
        {
            flipTimer -= Time.deltaTime;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        int facing = facingRight ? 1 : -1;
        attackVector = facing * Vector2.right + 1.0f * Vector2.down;

        isNearObject = Physics2D.Raycast(transform.position, facing * Vector2.right, frontCheckDistance, whatIsObject);
        isTooLow = Physics2D.Raycast(transform.position, Vector2.down, targetHeightMin, whatIsGround);
        isTooHigh = !Physics2D.Raycast(transform.position, Vector2.down, targetHeightMax, whatIsGround);
        canSeePlayer = Physics2D.Raycast(transform.position, attackVector, attackDistance, LayerMask.GetMask("Player"));

        Vector2 newVelocity = rb.velocity;

        float range = transform.position.x - startingPosition.x;

        if (isNearObject && flipTimer <= 0)
        {
            Flip();
            newVelocity.x = 0f;
            isDiving = false;
        }

        if (canSeePlayer && !isDiving && (diveTimer <= 0))
        {
            isDiving = true;
            newVelocity.x = 0f;
            newVelocity.y = 0f;
            diveTimer = diveCooldown;
            huntTimer = huntDuration;
            AudioSource.PlayClipAtPoint(attackSound, new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z), 1.0f);
        }

        if (isDiving && (diveTimer < (diveCooldown-diveDelay)))
        {
            newVelocity.x = facing * diveSpeed;
            newVelocity.y = -diveSpeed;

            if  (player.transform.position.y > transform.position.y + 1f)
            {
                // break out of dive if below player
                isDiving = false;
                diveTimer = 0f;
            }
        }
        else if (inFreeFlightMode)
        {
            isTooHigh = Physics2D.Raycast(transform.position, Vector2.up, 1f, whatIsGround);
            newVelocity.x = facing * speed;

            if (player.transform.position.y > transform.position.y + 1f)
            {
                movingUp = true;
            }

            if (player.transform.position.y < transform.position.y - 2f)
            {
                movingUp = false;
            }

            if (isTooLow)
            {
                movingUp = true;
            }

            if (isTooHigh)
            {
                movingUp = false;
            }

            if (movingUp)
            {
                newVelocity.y = speed;
            }
            else
            {
                newVelocity.y = -speed;
            }

        }
        else
        {
            newVelocity.x = facing * speed;

            if (isTooLow)
            {
                movingUp = true;
            }

            if (isTooHigh)
            {
                movingUp = false;
            }

            if (movingUp)
            {
                newVelocity.y = speed;
            }
            else
            {
                newVelocity.y = -speed;
            }
        }

        rb.velocity = newVelocity;


        if (!isDiving && facing * range > moveDistance)  // out of range
        {
            if (moveDistance > 0)
            {
                Flip();
                huntTimer = 0f;
            }
        }

        // hunt change facing
        if (!isDiving && !isTooLow && huntTimer > 0 && flipTimer <= 0)
        {
            if (facingRight && player.transform.position.x < (transform.position.x - 1f))
            {
                Flip();
            }
            else if (!facingRight && player.transform.position.x > (transform.position.x + 1f))
            {
                Flip();
            }
        }


        animator.SetBool("isDiving", isDiving);
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 Scaler = transform.localScale;
        Scaler.x *= -1;
        transform.localScale = Scaler;
        flipTimer = flipCooldown;
    }

    public void GetHurt(int damage = 1)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Instantiate(burstPrefab, transform.position, transform.rotation);
            Destroy(gameObject);
        }
        else
        {
            Instantiate(flashPrefab, transform.position, transform.rotation, gameObject.transform);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.gameObject.CompareTag("PlayerAttack"))
        {
            GetHurt();
        }
        else
        {
            isDiving = false;
            diveTimer = diveCooldown;
        }
    }

    private void OnDrawGizmosSelected()
    {
        int facing = facingRight ? 1 : -1;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3) (facing * Vector2.right * frontCheckDistance ));

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3) Vector3.down * targetHeightMin);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3) attackVector.normalized * attackDistance );

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(startingPosition - (Vector3)(Vector2.right * moveDistance), startingPosition + (Vector3)(Vector2.right * moveDistance));

    }

}
