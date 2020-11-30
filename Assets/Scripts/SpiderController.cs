using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderController : MonoBehaviour
{

    private Rigidbody2D rb;
    private Animator animator;
    private GameObject player;

    private int maxHealth = 2;
    private int currentHealth;

    private float moveSpeed = 4f;
    private float runSpeed = 6f;
    private float attackSpeed = 12f;  // was 14f
    private float webSpeed = 4f;
    private float dropSpeed = 8f;

    public bool startOnWeb = false;
    public bool startFacingLeft = false;
    private bool facingRight = false;

    public LayerMask whatIsObject;
    public LayerMask whatIsGround;

    private float frontCheckDistance = 1.3f;
    private float groundCheckDistance = 1f;
    private float headCheckDistance = 2f;
    private float edgeCheckDistance = 1f;

    private float flipTimer = 0f;
    private float flipTimerDuration = 1f;

    private float aggroRadius = 10f;
    private float aggroTimer = 0f;
    private float aggroDuration = 4f;

    private float attackDistance = 2f;
    private float attackTimer = 0f;
    private float attackDelay = 0.25f;
    private float attackDuration = 0.60f;  // was 0.50f
    private float attackHold = 0.5f;
    private float attackCooldown = 1f;
    private float attackCooldownTimer = 0f;
    private bool doJumpAttack = false;

    private float stuckTimer = 0f;
    private float stuckTimeLimit = 3f;

    private bool ignorePlayer = false;
    private float ignoreTimer = 0f;
    private float ignoreTimeLimit = 2f;

    private float startingMass;
    private float gravityScale;

    public GameObject lightBurstPrefab;
    public GameObject lightFlashPrefab;


    // animation or control states
    private bool isRunning = false;
    private bool isAttacking = false;
    private bool isOnWeb = false;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        player = GameObject.Find("Player");

        if (!startFacingLeft)
        {
            Flip();
        }

        startingMass = rb.mass;
        gravityScale = rb.gravityScale;

        if (startOnWeb)
        {
            AttachWeb();
        }

        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (flipTimer > 0)
        {
            flipTimer -= Time.deltaTime;
        }

        if (aggroTimer > 0)
        {
            aggroTimer -= Time.deltaTime;
        }

        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
            isAttacking = true;
        }
        else
        {
            isAttacking = false;
        }

        if (attackCooldownTimer > 0)
        {
            attackCooldown -= Time.deltaTime;
        }

        if (ignoreTimer > 0)
        {
            ignoreTimer -= Time.deltaTime;
        }
        else
        {
            ignorePlayer = false;
        }

        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isAttacking", isAttacking);
        animator.SetBool("isOnWeb", isOnWeb);

    }

    void FixedUpdate()
    {
        int facing = facingRight ? 1 : -1;

        bool isNearObject = Physics2D.Raycast(transform.position, facing * Vector2.right, frontCheckDistance, whatIsObject);
        bool isNearEdge = !Physics2D.Raycast(transform.position + (Vector3)(facing * Vector2.right), Vector2.down, edgeCheckDistance, whatIsObject);

        bool isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);
        bool isAtCeiling = Physics2D.Raycast(transform.position, Vector2.up, headCheckDistance, whatIsGround);

        bool inAttackRange = Physics2D.Raycast(transform.position, facing * Vector2.right, attackDistance, LayerMask.GetMask("Player"));

        bool canSeePlayer = Physics2D.Raycast(transform.position, facing * Vector2.right, aggroRadius, LayerMask.GetMask("Player"));
        bool isNearPlayer = Physics2D.OverlapCircle(transform.position, aggroRadius, LayerMask.GetMask("Player"));

        Vector2 distance = player.transform.position - transform.position;

        bool isSameHeight = Mathf.Abs(distance.y) < 2f;
        bool isJustBelowPlayer =  (distance.magnitude < aggroRadius) && (Mathf.Abs(distance.x) < 2f) && (distance.y > 0);
        bool isJustAbovePlayer = (distance.magnitude < aggroRadius) && (Mathf.Abs(distance.x) < 2f) && (distance.y < 0);

        bool isDirectlyAbovePlayer = (distance.magnitude < 1.5f*aggroRadius) && (Mathf.Abs(distance.x) < 0.5f) && (distance.y < 0);


        Vector2 newVelocity = rb.velocity;

        if (isOnWeb)
        {
            if (isNearPlayer)
            {
                if (facingRight != (distance.x > 0))
                {
                    // not facing player
                    Flip();
                }

                bool inDropAttackView = !Physics2D.Raycast(transform.position -0.5f*Vector3.up, distance.normalized, attackDistance, LayerMask.GetMask("Ground")) &&
                    !Physics2D.Raycast(transform.position + 0.8f*Vector3.up, distance.normalized, attackDistance, LayerMask.GetMask("Ground"));

                if (isDirectlyAbovePlayer)
                {
                    //DetachWeb();
                    //newVelocity.y = -attackSpeed;
                    newVelocity.y = -dropSpeed;
                }
                else if (canSeePlayer && Mathf.Abs(distance.y) < 0.4f)
                {
                    DetachWeb();
                    newVelocity = attackSpeed * distance.normalized + 1f * Vector2.up;
                    attackTimer = attackDuration + attackHold;
                }
                else if (distance.magnitude < aggroRadius)
                {
                   
                    if (!isGrounded && distance.y < 0)   // Descend
                    {
                        //transform.position = new Vector3 (transform.position.x, transform.position.y-2f*Time.deltaTime, transform.position.z);
                        newVelocity.y = -webSpeed;

                    }
                    else if (!isAtCeiling && distance.y > 0)  // Ascend
                    {
                        newVelocity.y = webSpeed;
                    }
                    else
                    {
                        newVelocity.y = 0;
                    }
                }
            }
            else  // not near player so return to ceiling
            {
                if (!isAtCeiling)
                {
                    newVelocity.y = webSpeed;
                }
                else
                {
                    newVelocity.y = 0;
                }
            }
        }
        else if (isGrounded)
        {
            if (isNearObject)
            {
                Flip();
                newVelocity.x = 0; //pause
                isRunning = false;
                attackTimer = 0f;
            }
            else if (isNearEdge && !isAttacking)
            {
                if (canSeePlayer)
                {
                    // jump attack
                    //newVelocity = attackSpeed * distance.normalized;
                    doJumpAttack = true;
                    attackTimer = attackDuration + attackHold;
                }
                else
                {
                    Flip();
                    newVelocity.x = 0; //pause
                    isRunning = false;
                    attackTimer = 0f;
                }
            }
            else if (!isNearPlayer && aggroTimer <= 0)
            {
                // walk
                newVelocity.x = facing * moveSpeed;
                isRunning = false;
            }
            else if (facingRight != (distance.x > 0) && !isJustBelowPlayer && !isJustAbovePlayer && !isAttacking && !ignorePlayer)
            {
                // not facing player
                Flip();
                newVelocity.x = 0; //pause
                isRunning = false;
            }
            else if (canSeePlayer || isAttacking)
            {
                ignorePlayer = false;
                if (isAttacking)
                {
                    if (attackTimer < attackHold)
                    {
                        newVelocity.x = 0; //pause after attack
                    }
                    else if (attackTimer < attackHold + attackDuration)
                    {
                        newVelocity.x = facing * attackSpeed;
                        if (doJumpAttack)
                        {
                            newVelocity.y = runSpeed;
                            doJumpAttack = false;
                        }
                    }
                }
                else if (inAttackRange && attackCooldownTimer <=0)
                {
                    // begin attack sequence
                    newVelocity.x = 0;
                    isRunning = false;
                    attackTimer = attackDelay + attackDuration + attackHold;
                    aggroTimer = aggroDuration;
                }
                else
                {
                    // run toward player
                    aggroTimer = aggroDuration;
                    newVelocity.x = facing * runSpeed;
                    isRunning = true;
                }
            }
            else if (distance.magnitude < 4f && distance.y > 2f) // player near and slightly above
            {
                // jump attack
                newVelocity.x = 0; //pause
                isRunning = false;
                doJumpAttack = true;
                attackTimer = attackDelay + attackDuration + attackHold;
                aggroTimer = aggroDuration;
            }
            else
            {
                // walk
                newVelocity.x = facing * moveSpeed;
                isRunning = false;
            }

            if (rb.velocity.magnitude < 0.05f)
            {
                stuckTimer += Time.fixedDeltaTime;
                if (stuckTimer > stuckTimeLimit)
                {
                    ignorePlayer = true;
                    ignoreTimer = ignoreTimeLimit;
                }
            }

        }
        else   // not grounded
        {
            if (rb.velocity.magnitude < 0.05f)
            {
                // stuck on something
                stuckTimer += Time.fixedDeltaTime;
                if (stuckTimer > stuckTimeLimit)
                {
                    newVelocity.x = runSpeed * Random.Range(-0.5f, 0.5f);
                    newVelocity.y = runSpeed;
                    stuckTimer = 0;
                }
            }

        }


        rb.velocity = newVelocity;
    }


    void Flip()
    {
        if (flipTimer <= 0)
        {
            facingRight = !facingRight;
            Vector3 Scaler = transform.localScale;
            Scaler.x *= -1;
            transform.localScale = Scaler;
            flipTimer = flipTimerDuration;
        }
    }

    void DetachWeb()
    {
        isOnWeb = false;
        rb.gravityScale = gravityScale;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void AttachWeb()
    {
        isOnWeb = true;
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

    }

    public void GetHurt(int damage = 1)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Instantiate(lightBurstPrefab, transform.position, transform.rotation);
            Destroy(gameObject);
        }
        else
        {
            GameObject burst = Instantiate(lightFlashPrefab, transform.position, transform.rotation, gameObject.transform);
            PulseScript ps = burst.GetComponent<PulseScript>();
            //ps.pulseAmount = 5;
        }

        if (isOnWeb)
            DetachWeb();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.gameObject.CompareTag("PlayerAttack"))
        {
            GetHurt();
        }
    }

    private void OnDrawGizmosSelected()
    {
        int facing = facingRight ? 1 : -1;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aggroRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position + (Vector3)(facing * Vector2.right), transform.position + (Vector3)(facing * Vector2.right + Vector2.down * edgeCheckDistance));

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)Vector3.down * groundCheckDistance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)(facing * Vector2.right * frontCheckDistance));

    }

}
