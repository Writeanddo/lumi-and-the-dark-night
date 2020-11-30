using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private CameraController cam;
    private Animator animator;
    private AudioSource audiosource;
    private ParticleSystem particletrail;
    private Transform lightglow;
    SpriteRenderer sr;
    SpriteRenderer lgsr;


    // Movement speeds
    public float speed = 6f;
    public float airSpeed = 4f;
    public float jumpSpeed = 8f;

    // Physics
    private Vector3 myGravity = new Vector3(0f, -9.81f, 0f);
    private float playerMass = 10f;
    private float playerGravityScale = 2.5f;

    // Ability states
    public int lifeForce = 10;
    private int lastLifeForce;  // implemented to test through inspector
    public bool canJump = true;
    public bool canDoubleJump = true;
    public bool canDash = true;
    public bool canField = true;
    public bool canBlast = true;
    public bool canRotate = false;
    public bool canUnlimitedJump = false;

    // Trail effect
    public bool hasTrail = true;
    private float trailRate = 5f;

    // Heath system
    public int maxHealth = 3;
    public int currentHealth = 3;
    private float hurtMoveDelay = 0.4f;
    private float hurtAgainDelay = 1f;
    private float lastHurtCountdown = 0f;
    private bool isHurt = false;
    Vector2 lastHitPoint;

    // Input tracking
    private float moveInput = 0f;
    private float rotateInput = 0f;
    private bool jumpPressed;
    private bool jumpHeld;
    private bool jumpReleased;
    private bool dashPressed;
    private bool blastPressed;
    private bool fieldPressed;

    // Movement
    private bool facingRight = true;
    private bool isGrounded;
    public Transform groundCheck;
    public float checkRadius = 0.35f;
    public LayerMask whatIsGround;
    private float groundCheckTimer;
    private float groundCheckForgivenessTime = 0.1f;
    public bool autoRun = false;

    // Idle settings
    public bool isSitting = false;
    private float idleTimeLimit = 10f;
    private float idleTimeCounter = 0f;

    // Jump settings
    private int extraJumps;
    public int extraJumpsValue = 1;
    private float jumpTimeCounter;
    private float jumpTime = 0.4f;
    private bool isJumping;
    
    // Dash settings
    private float dashTime = 0.25f;
    private float dashSpeed = 20f;
    private float dashTimeCounter;
    private bool isDashing;
    private bool dashReady = true;

    // Gravity Field settings
    private float fieldCooldown = 2.6f;
    private float fieldTimeCounter = 0f;
    private Vector3 fieldPosition = new Vector3(3.5f, -0.60f, 0f);
    public GameObject fieldPrefab;

    // Blast settings
    private float blastCooldown = 0.4f;
    private float blastSpeed = 15f;
    private float blastTimeCounter = 0f;
    public Rigidbody2D blastPrefab;

    // Rotate settings
    private float rotateDuration = 4f;
    private float rotateCooldown = 4.5f;
    private float rotateTimeCounter = 0f;
    
    // Player freeze settings
    private float freezeMoveCountdown;
    private bool canMove = false;

    // Misc
    public bool hasKey = false;
    private Vector2 lastSpawnPoint;
    private float playerAlpha = 1;

    // Sticky
    private float stickyTimer = 0f;
    private float stickyDuration = 0.5f;
    private bool isSticky = false;

    // Death
    public bool restartOnDeath = false;
    public GameObject deathEffect;
    public bool isDead = false;

    // Effects and Audio
    private float jumpSoundVolume = 0.5f;
    public GameObject pickupEffects;
    public AudioClip pickupCollectSound;
    public AudioClip jumpSound;
    public AudioClip doubleJumpSound;
    public AudioClip dashSound;
    public AudioClip fieldSound;
    public AudioClip blastSound;
    public AudioClip hurtSound;
    public AudioClip powerUpSound;
    public AudioClip keySound;

    // Lighting effects
    public SpriteRenderer dimmer;
    public bool adjustBackgroundColor = false;

    // Player UI
    public PlayerUIScript playerUI;

    // Gamepad button mapping
#if (UNITY_EDITOR || UNITY_STANDALONE_OSX)
    KeyCode jump_keycode = KeyCode.JoystickButton1;
    KeyCode dash_keycode = KeyCode.JoystickButton2;
    KeyCode field_keycode = KeyCode.JoystickButton3;
    KeyCode blast_keycode = KeyCode.JoystickButton0;
#else
    KeyCode jump_keycode = KeyCode.JoystickButton0;
    KeyCode dash_keycode = KeyCode.JoystickButton1;
    KeyCode field_keycode = KeyCode.JoystickButton3;
    KeyCode blast_keycode = KeyCode.JoystickButton2;
#endif

    // Start is called before the first frame update
    void Start()
    {
        UpdateStartingHealth();
        UpdateHealthEffects();

        rb.mass = playerMass;
        rb.gravityScale = 0;
        rb.freezeRotation = true;
    }

    void Awake()
    {
        //Physics2D.gravity = myGravity;
        //Debug.Log("My Gravity " + myGravity);        

        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main.GetComponent<CameraController>();
        animator = GetComponent<Animator>();
        audiosource = GetComponent<AudioSource>();

        lightglow = transform.Find("Lightglow");
        sr = GetComponent<SpriteRenderer>();
        lgsr = lightglow.GetComponent<SpriteRenderer>();


        Transform pt = transform.Find("ParticleTrail");
        particletrail = pt.GetComponent<ParticleSystem>();

        rb.mass = playerMass;

        lastLifeForce = lifeForce;
        lastSpawnPoint = rb.position;
    }

    void Update()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);

        // implemented to update lifeForce via the inspector
        if (lifeForce != lastLifeForce)
        {
            UpdateHealthEffects();
        }
        lastLifeForce = lifeForce;

        // Handle timers

        if (lastHurtCountdown > 0)
        {
            lastHurtCountdown -= Time.deltaTime;
        }

        if (isHurt && (lastHurtCountdown <= (hurtAgainDelay - hurtMoveDelay)))
        {
            isHurt = false;
        }

        if (freezeMoveCountdown > 0)
        {
            freezeMoveCountdown -= Time.deltaTime;
        }
        else
        {
            if (!canMove)
                canMove = true;
        }

        if (blastTimeCounter > 0)
        {
            blastTimeCounter -= Time.deltaTime;
        }

        if (fieldTimeCounter > 0)
        {
            fieldTimeCounter -= Time.deltaTime;
        }

        if (rotateTimeCounter > 0)
        {
            rotateTimeCounter -= Time.deltaTime;
        }

        if (groundCheckTimer > 0)
        {
            groundCheckTimer -= Time.deltaTime;
        }

        if (stickyTimer > 0)
        {
            stickyTimer -= Time.deltaTime;
            isSticky = true;
        }
        else
        {
            isSticky = false;
        }


        if (canMove && !isDead)
        {
            // Get Inputs
            moveInput = Input.GetAxis("Horizontal");

            if (canJump)
            {
                if (!jumpPressed)  // saving last jumpPressed state for processing in FixedUpdate
                {
                    jumpPressed = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(jump_keycode);
                }

                jumpHeld = Input.GetKey(KeyCode.Space) || Input.GetKey(jump_keycode);
                jumpReleased = Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(jump_keycode);
            }


            if (!fieldPressed)
            {
                fieldPressed = (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(field_keycode)) && (fieldTimeCounter <= 0) && isGrounded && canField;
            }

            if (!blastPressed)
            {
                blastPressed = (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(blast_keycode)) && (blastTimeCounter <= 0) && canBlast;
            }

            if (!dashPressed)
            {
                dashPressed = (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(dash_keycode)) && !isDashing && canDash;
            }

            if ((Input.GetKeyDown(KeyCode.LeftBracket) || Input.GetKeyDown(KeyCode.JoystickButton4)) && (rotateTimeCounter <= 0) && isGrounded && canRotate)
            {
                rotateInput = -90f;
            }

            if ((Input.GetKeyDown(KeyCode.RightBracket) || Input.GetKeyDown(KeyCode.JoystickButton5)) && (rotateTimeCounter <= 0) && isGrounded && canRotate)
            {
                rotateInput = 90f;
            }

            if (isGrounded == true)
            {
                extraJumps = extraJumpsValue;
            }

            if (jumpReleased)
            {
                isJumping = false;
            }

            if ((facingRight == false) && (moveInput > 0.2))
            {
                Flip();
            }
            else if ((facingRight == true) && (moveInput < -0.2))
            {
                Flip();
            }
        }
        else if (autoRun)
        {
            moveInput = 1;
        }
        else
        {
            // throw away last inputs
            moveInput = 0f;
            jumpPressed = false;
            jumpHeld = false;
            dashPressed = false;
        }

        if ( (Mathf.Abs(moveInput) > 0.01f) || isJumping || isDashing || !canMove )
        {
            idleTimeCounter = 0f;
            isSitting = false;
        }
        else if (idleTimeCounter < idleTimeLimit)
        {
            idleTimeCounter += Time.deltaTime;
        }

        if ((idleTimeCounter >= idleTimeLimit) || (speed == 0f))
        {
            isSitting = true;
        }

        Vector2 localVelocity = rb.transform.InverseTransformVector(rb.velocity);  // needed for animator


        if (isGrounded && !wasGrounded)
        {
            //cam.Shake();
            //cam.Zoom(1.02f, 0.01f, 0f, 0.01f);
            //cam.Bump(new Vector2(0f, -0.3f));
        }

        if (isGrounded)
        {
            groundCheckTimer = groundCheckForgivenessTime;
        }

        // Update animator variables
        animator.SetFloat("SpeedX", Mathf.Abs(moveInput));
        animator.SetFloat("SpeedY", localVelocity.y);
        animator.SetBool("isJumping", isJumping);
        animator.SetBool("isSitting", isSitting);
        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("isHurt", isHurt);

    }

    void FixedUpdate()
    {

        Vector2 localVelocity = rb.transform.InverseTransformVector(rb.velocity);
        int facing = facingRight ? 1 : -1;

        if (isHurt)
        {
            // don't accept input while hurt
            //localVelocity.x = -4f;
            //localVelocity.y = 4f;

            Vector2 knockbackDirection = -rb.transform.InverseTransformPoint(lastHitPoint).normalized;
            localVelocity = 4f * knockbackDirection;
        }
        else
        {
            if (dashPressed)
            {
                if (!isSticky && dashReady)
                {
                    isDashing = true;
                    dashTimeCounter = dashTime;
                    dashReady = false;
                    PlaySound(dashSound, 0.5f);
                }
                // clear state after processing
                dashPressed = false;
            }

            if (isDashing)
            {
                if (!isSticky && dashTimeCounter > 0)
                {
                    //rb.mass = 0f;
                    localVelocity.x = dashSpeed;
                    localVelocity.y = 0f;
                    dashTimeCounter -= Time.fixedDeltaTime;
                }
                else
                {
                    rb.mass = playerMass;
                    isDashing = false;
                }
            }
            else if (isGrounded == true)
            {
                //rb.velocity = new Vector2(moveInput * speed, rb.velocity.y);
                //rb.velocity = rb.transform.TransformVector(new Vector2(moveInput * speed * facing, localVelocity.y));
                localVelocity.x = moveInput * speed * facing;
                dashReady = true;
            }
            else
            {
                //rb.velocity = new Vector2(moveInput * airspeed, rb.velocity.y);
                //rb.velocity = rb.transform.TransformVector(new Vector2(moveInput * airSpeed * facing, localVelocity.y));
                localVelocity.x = moveInput * airSpeed * facing;
                if (canUnlimitedJump)
                {
                    dashReady = true;
                }
            }

            if (blastPressed)
            {
                // make sure to start outside player collider
                // still need to fix vectors for rotation shift

                Rigidbody2D blast = Instantiate(blastPrefab, transform.position, transform.rotation);
                //Rigidbody2D blast = Instantiate(blastPrefab, transform.position + facing * Vector3.right * 0.6f, transform.rotation);
                Physics2D.IgnoreCollision(gameObject.GetComponent<Collider2D>(), blast.GetComponent<Collider2D>());
                //blast.velocity = new Vector2(blastSpeed * facing, 0f);
                blast.velocity = rb.transform.TransformVector(new Vector2(blastSpeed, 0f));
                blastTimeCounter = blastCooldown;
                PlaySound(blastSound, 0.5f);
                blastPressed = false;
            }

            if (fieldPressed)
            {
                Vector3 fieldOffset = fieldPosition;
                fieldOffset = AdjustFieldPosition(fieldOffset);
                //fieldOffset.x *= facing;
                //GameObject field = Instantiate(fieldPrefab, transform.position + fieldOffset , transform.rotation);
                GameObject field = Instantiate(fieldPrefab, rb.transform.TransformPoint(fieldOffset), transform.rotation);
                fieldTimeCounter = fieldCooldown;
                PlaySound(fieldSound, 1.0f);
                fieldPressed = false;
            }

            if (jumpPressed)
            {
                if (isGrounded || groundCheckTimer > 0)
                {
                    // do primary jump
                    isJumping = true;
                    jumpTimeCounter = jumpTime;
                    localVelocity.y = jumpSpeed;

                    PlaySound(jumpSound, jumpSoundVolume);
                }
                else if (extraJumps > 0)
                {
                    // do extra jump
                    isJumping = true;
                    jumpTimeCounter = jumpTime;
                    localVelocity.y = jumpSpeed;

                    if (!canUnlimitedJump)
                        extraJumps--;
                    PlaySound(doubleJumpSound, jumpSoundVolume);
                }
                // clear state after processing
                jumpPressed = false;
            }


            if (jumpHeld && isJumping)
            {
                if (jumpTimeCounter > 0)
                {
                    //rb.velocity = rb.transform.TransformVector(Vector2.up * jumpSpeed);
                    localVelocity.y = jumpSpeed;
                    jumpTimeCounter -= Time.fixedDeltaTime;
                }
                else
                {
                    isJumping = false;
                }
            }

        }

        if (isSticky)
        {
            localVelocity = 0.5f * localVelocity;
        }

        rb.velocity = rb.transform.TransformVector(localVelocity);

        // Apply gravity if not dashing
        if (!isDashing && !isDead)
        {
            rb.AddForce(playerMass * playerGravityScale * myGravity);
        }

        // Apply bouyancy if in water
        if (!isDead && Physics2D.OverlapCircle(transform.position, checkRadius, LayerMask.GetMask("Water")))
        {
            rb.AddForce(-2f *playerMass * playerGravityScale * myGravity);
        }

        if ( (rotateTimeCounter > 0) && (rotateTimeCounter < (rotateCooldown - rotateDuration)))
        {
            // undo rotate
            rotateInput = -Quaternion.Angle(transform.rotation, Quaternion.Euler(0, 0, 0));
        }

        if (rotateInput != 0)
        {
            myGravity = Quaternion.AngleAxis(rotateInput, Vector3.forward) * myGravity;
            //Physics2D.gravity = myGravity;
            Debug.Log("My Gravity " + myGravity);
            rb.transform.Rotate(0f, 0f, rotateInput, Space.Self);
            rotateInput = 0;
            if (rotateTimeCounter <= 0)  // only happens with new input
                rotateTimeCounter = rotateCooldown;
        }


    }

    void Flip()
    {
        if (!isDashing && !isHurt)
        {
            facingRight = !facingRight;
            Vector3 Scaler = transform.localScale;
            Scaler.x *= -1;
            transform.localScale = Scaler;
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Pickup"))
        {
            PlaySound(pickupCollectSound);
            Instantiate(pickupEffects, collider.gameObject.transform.position, Quaternion.identity);
            Destroy(collider.gameObject);

            GainLifeForce();
        }
        else if (collider.gameObject.CompareTag("Hidden"))
        {
            Destroy(collider.gameObject);
        }
        else if (collider.gameObject.CompareTag("DeadlyTouch"))
        {
            lastHitPoint = FindContactPoint(collider);
            GetHurt(currentHealth);
        }
        else if (collider.gameObject.CompareTag("DamagingTouch") || collider.gameObject.CompareTag("Boss"))
        {
            lastHitPoint = FindContactPoint(collider);
            GetHurt(1);
        }
        else if (collider.gameObject.CompareTag("Respawn"))
        {
            lastSpawnPoint = collider.transform.position;
        }
        else if (collider.gameObject.CompareTag("Key"))
        {
            hasKey = true;
            Destroy(collider.gameObject);
            PlaySound(keySound, 0.5f);
        }
        else if (collider.gameObject.CompareTag("StickyTouch"))
        {
            GetSticky();
        }
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("DeadlyTouch"))
        {
            GetHurt(currentHealth);
        }
        else if (collider.gameObject.CompareTag("DamagingTouch") || collider.gameObject.CompareTag("Boss"))
        {
            GetHurt(1);
        }
        else if (collider.gameObject.CompareTag("StickyTouch"))
        {
            GetSticky();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.gameObject.CompareTag("DeadlyTouch"))
        {
            lastHitPoint = FindContactPoint(collision.collider);
            GetHurt(currentHealth);
        }
        if (collision.collider.gameObject.CompareTag("DamagingTouch") || collision.collider.gameObject.CompareTag("Boss"))
        {
            lastHitPoint = FindContactPoint(collision.collider);
            GetHurt(1);
        }
        if (collision.collider.gameObject.CompareTag("StickyTouch"))
        {
            GetSticky();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.gameObject.CompareTag("DeadlyTouch"))
        {
            GetHurt(currentHealth);
        }
        if (collision.collider.gameObject.CompareTag("DamagingTouch") || collision.collider.gameObject.CompareTag("Boss"))
        {
            GetHurt(1);
        }
        if (collision.collider.gameObject.CompareTag("StickyTouch"))
        {
            GetSticky();
        }
    }

    private void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("DeadlyTouch"))
        {
            GetHurt(currentHealth);
        }
        if (other.CompareTag("DamagingTouch"))
        {
            GetHurt(1);
        }
        if (other.CompareTag("StickyTouch"))
        {
            GetSticky();
        }
    }

    public void GetHurt(int damage)
    {
        if (!isDead)
        {
            if (lastHurtCountdown <= 0)
            {
                currentHealth -= damage;
                isHurt = true;
                isDashing = false; // break dash
                PlaySound(hurtSound);
                lastHurtCountdown = hurtAgainDelay;
                UpdateHealthEffects();
            }

            //Debug.Log("Health = " + currentHealth);

            if (currentHealth < 1)
            {
                StartCoroutine(DeathEffect());
            }

        }
    }

    public void GetSticky()
    {
        stickyTimer = stickyDuration;
    }

    IEnumerator DeathEffect()
    {
        isDead = true;
        yield return new WaitForSeconds(hurtMoveDelay);

        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        lightglow.GetComponent<SpriteRenderer>().enabled = false;
        Instantiate(deathEffect, transform.position, transform.rotation);

        yield return new WaitForSeconds(2f);
        Respawn();

        yield return new WaitForSeconds(0.5f);
        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<Collider2D>().enabled = true;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        lightglow.GetComponent<SpriteRenderer>().enabled = true;
    }

    public void Respawn()
    {
        GameData.respawned = true;
        if (restartOnDeath)
        {
            playerUI.RestartScene();
        }
        else
        {
            currentHealth = maxHealth;
            isHurt = false;
            isDead = false;
            isJumping = false;
            rb.position = lastSpawnPoint;
            FreezePlayer(1.0f);
            UpdateUI();
        }
    }

    public void FreezePlayer(float holdtime = 1f)
    {
        freezeMoveCountdown = holdtime;
        rb.velocity = Vector3.zero;
        canMove = false;
    }

    private void PlaySound(AudioClip sound, float volume = 1f)
    {
        if (sound != null)
        {
            //AudioSource.PlayClipAtPoint(sound, new Vector3(transform.position.x, transform.position.y, -10));
            audiosource.PlayOneShot(sound, volume);
        }
    }

    private void UpdateMoveEffects()
    {
        if (lifeForce == 0)  //immobile
        {
            speed = 0f;
            airSpeed = 0f;
            jumpSpeed = 0f;
        }
        else if (lifeForce == 1) //weak
        {
            speed = 4f;
            airSpeed = 3f;
            jumpSpeed = 4f;
            canJump = true;
        }
        else if (lifeForce == 2)
        {
            speed = 5f;
            airSpeed = 3.5f;
            jumpSpeed = 6f;
        }
        else //strong
        {
            speed = 6f;
            airSpeed = 4f;
            jumpSpeed = 8f;
        }
    }

    public void UpdateGlowEffects()
    {
        //Transform lg = transform.Find("Lightglow");
        //SpriteRenderer sr = GetComponent<SpriteRenderer>();
        //SpriteRenderer lgsr = lightglow.GetComponent<SpriteRenderer>();

        float lightglow_radius_min = 1.0f;
        float lightglow_radius_max = 1.5f;
        float lightglow_opacity_min = (0f / 255f);  // was min 75f
        float lightglow_opacity_max = (100f / 255f);
        float player_color_min = 0.4f;
        float player_color_max = 1.0f;

        float bg_color_min = (5f / 255f);
        float bg_color_max = (30f / 255f);
        float dimmer_opacity_min = 0f;
        float dimmer_opacity_max = 0.25f;

        int max_life_glow = 8;

        float glow = Mathf.Clamp((float) lifeForce/max_life_glow,0f,1f);

        float lightglow_radius = lightglow_radius_min + glow * (lightglow_radius_max - lightglow_radius_min);
        float lightglow_opacity = lightglow_opacity_min + glow * (lightglow_opacity_max - lightglow_opacity_min);
        float player_color = player_color_min + glow * (player_color_max - player_color_min);
        float bg_color = bg_color_min + glow * (bg_color_max - bg_color_min);
        float dimmer_opacity = dimmer_opacity_max - glow * (dimmer_opacity_max - dimmer_opacity_min);

        //Debug.Log("glow = " + glow);
        //Debug.Log("lightglow_radius = " + lightglow_radius);
        //Debug.Log("lightglow_opacity = " + lightglow_opacity);
        //Debug.Log("player_color = " + player_color);
        //Debug.Log("bg_color = " + bg_color);
        //Debug.Log("dimmer_opacity = " + dimmer_opacity);

        lightglow.localScale = new Vector3(lightglow_radius, lightglow_radius, 1.0f);
        lgsr.color = new Color(1f, 1f, 1f, lightglow_opacity);
        sr.color = new Color(player_color, player_color, player_color, playerAlpha);
        if (adjustBackgroundColor)
            Camera.main.backgroundColor = new Color(bg_color, bg_color, bg_color, 0);
        if (dimmer != null)
            dimmer.color = new Color(0f, 0f, 0f, dimmer_opacity);
    }

    public void UpdateTrailEffects()
    {
        var emission = particletrail.emission;
        emission.rateOverDistance = trailRate * (currentHealth - 1) /4f;
    }

    public void ResetOrbCount()
    {
        GameData.orbCount = 0;
        maxHealth = 1;
        currentHealth = 1;
        UpdateHealthEffects();
    }

    public void UpdateStartingHealth()
    {
        if (GameData.orbCount > 4)
            maxHealth = 2;
        if (GameData.orbCount > 24)
            maxHealth = 3;
        if (GameData.orbCount > 49)
            maxHealth = 4;
        if (GameData.orbCount > 99)
            maxHealth = 5;
        currentHealth = maxHealth;
    }

    public void UpdateHealthEffects()
    {
        // set max health

        if (GameData.orbCount > 4 && maxHealth < 2)
            GainMaxHealth();
        if (GameData.orbCount > 24 && maxHealth < 3)
            GainMaxHealth();
        if (GameData.orbCount > 49 && maxHealth < 4)
            GainMaxHealth();
        if (GameData.orbCount > 99 && maxHealth < 5)
            GainMaxHealth();

        UpdateMoveEffects();
        UpdateGlowEffects();
        UpdateTrailEffects();
        UpdateUI();

    }

    public void GainMaxHealth()
    {
        if (maxHealth < 5)
        {
            maxHealth++;
            playerUI.ShowDialogText("Lumi's Health is increased!", 3f);
            currentHealth = maxHealth;
        }
    }

    public void GainLifeForce()
    {
        lifeForce++;
        GameData.orbCount++;

        if (lifeForce == 2)
            playerUI.ShowDialogText("With each fragment of Light, Lumi's strength was increasing.", 3f);

        UpdateHealthEffects();

        // healing from orbs
        if (currentHealth < maxHealth)
            currentHealth++;

    }

    public void GainJump()
    {
        if (!canJump)
        {
            canJump = true;
            UpdateUI();
        }
    }

    public void GainDoubleJump()
    {
        if (!canDoubleJump)
        {
            canDoubleJump = true;
            extraJumpsValue = 1;

            // add trail effect
            hasTrail = true;
            PlaySound(powerUpSound);
            UpdateUI();
            playerUI.ShowDialogText("Lumi gains <color=orange>Double Jump!</color>", 3f);
        }
    }

    public void GainDash()
    {
        if (!canDash)
        {
            canDash = true;
            PlaySound(powerUpSound);
            UpdateUI();
            playerUI.ShowDialogText("Lumi gains <color=orange>Warp Rush!</color>\nPress (Z Key) or (B Button).", 3f);
        }
    }

    public void GainBlast()
    {
        if (!canBlast)
        {
            canBlast = true;
            PlaySound(powerUpSound);
            UpdateUI();
            playerUI.ShowDialogText("Lumi gains <color=orange>Light Blast!</color>\nPress (C Key) or (X Button).", 3f);
        }
    }


    public void GainField()
    {
        if (!canField)
        {
            canField = true;
            PlaySound(powerUpSound);
            UpdateUI();
            playerUI.ShowDialogText("Lumi gains <color=orange>Gravity Field!</color>\nPress (X Key) or (Y Button).", 3f);
        }
    }

    public void GainRotate()
    {
        if (!canRotate)
        {
            canRotate = true;
            PlaySound(powerUpSound);
            UpdateUI();
            playerUI.ShowDialogText("Lumi gains <color=orange>Gravity Shift!</color>\nPress (< >) or (LT/RT).", 3f);
        }
    }

    public void GainUnlimitedJump()
    {
        if (!canUnlimitedJump)
        {
            canUnlimitedJump = true;
            extraJumpsValue = 1;
            UpdateUI();
            playerUI.ShowDialogText("Lumi gains <color=orange>Unlimited Jump!</color>", 3f);
        }
    }

    public void ForceStand()
    {
        idleTimeCounter = 0f;
        isSitting = false;
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
    }

    public int GetProperty(string propertyName)
    {
        switch(propertyName)
        {
            case "lifeForce":
                return lifeForce;
            case "hasKey":
                return hasKey ? 1 : 0;
            case "canDash":
                return canDash ? 1 : 0;
            default:
                return 0;
        }
    }

    public void SetAlpha(float value)
    {
        playerAlpha = value;
        UpdateGlowEffects();
    }

    public void UpdateUI()
    {
        if (playerUI != null)
        {
            playerUI.UpdateOrbCount();
            playerUI.SetPlayerHealth(currentHealth, maxHealth);

            playerUI.SetJumpIcon(canJump, canDoubleJump, canUnlimitedJump);
            playerUI.SetDashIcon(canDash);
            playerUI.SetFieldIcon(canField);
            playerUI.SetBlastIcon(canBlast);
            playerUI.SetShiftIcon(canRotate);
        }

    }

    public void SetUIActive(bool isActive)
    {
        playerUI.SetUIActive(isActive);
    }

    private Vector2 FindContactPoint(Collider2D collider)
    {
        ContactPoint2D[] contacts = new ContactPoint2D[10];
        int numberContacts = collider.GetContacts(contacts);

        Vector2 pt = Vector2.zero;

        if (numberContacts == 0)  // expected for triggers
        {
            pt = collider.ClosestPoint(rb.position);
        }

        for (int i = 0; i < numberContacts; i++)
        {
            //Debug.Log("Contact point [" + i + "] is " + contacts[i].point);
            pt += contacts[i].point;
        }

        if (numberContacts > 1)
        {
            pt *= (1f / numberContacts);
        }
        //Debug.Log("Contact point is " + pt + "with " + numberContacts + " contacts");
        return pt;
    }

    private Vector3 AdjustFieldPosition(Vector3 offset)
    {
        Vector2 hitPoint;
        int facing = facingRight ? 1 : -1;
        RaycastHit2D hitWall = Physics2D.Raycast(transform.position, facing * Vector2.right, offset.x, LayerMask.GetMask("Ground"));

        if (hitWall.collider != null)
        {
            if (!hitWall.collider.gameObject.CompareTag("MovableObject"))
            {
                hitPoint = Physics2D.ClosestPoint(transform.position, hitWall.collider);
                //offset.x = Mathf.Clamp(hitPoint.x - rb.transform.position.x, 0f, offset.x);
                offset.x = offset.x - 0.5f;
            }
        }

        return offset;
    }
}
