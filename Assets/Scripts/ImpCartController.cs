using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpCartController : MonoBehaviour
{
    public Transform playerCheck;
    public Transform groundCheck;
    public LayerMask whatIsPlayer;
    public LayerMask whatIsGround;
    public GameObject impPrefab;
    public GameObject cameraZone;
    public AudioClip impLaughSound;

    public bool moveEnabled = true;
    public bool chargeEnabled = true;
    public float moveRange = 33f;
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float stopTime = 0f;

    bool moving = false;
    bool running = false;
    bool facingRight = true;
    float timer = 0;
    float rangeLeft = 0;
    float rangeRight = 0;
    float checkRadius = 0.15f;
    float lookDistance = 10f;
    int impCount = 8;
    float impMinX = -0.65f;
    float impMaxX = 0.60f;
    float impMinY = -0.35f;
    float impMaxY = 0.45f;
    float impMinGlow = 0.2f;
    float impMaxGlow = 0.3f;
    float impLeash = 0.15f;
    float impSpeed = 5f;
    float bounceMax = 0.025f;
    float bounceOriginalOffset;
    float bounceDelta;
    GameObject[] imps;
    Transform[] impGlows;
    Vector2[] impAnchors;
    Collider2D cartCollider;
    ParticleSystem particletrail;
    CameraZone cameraZoneScript;
    AudioSource audioSource;
    bool musicFadingIn = false;
    bool musicFadingOut = false;

    enum AnimationType 
    {
        IDLE,
        WALK,
        RUN
    }

    AnimationType currentAnim;

    void Awake()
    {
        //myBody = GetComponent<Rigidbody2D>();
        cartCollider = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();
        if (cameraZone != null)
        {
            cameraZoneScript = cameraZone.GetComponent<CameraZone>();
        }
        timer = stopTime;
        if (moveRange < 0)
        {
            facingRight = false;
            rangeRight = transform.position.x;
            rangeLeft = rangeRight + moveRange;
        }
        else
        {
            rangeLeft = transform.position.x;
            rangeRight = rangeLeft + moveRange;
        }
        ImpSetup();
        CartSetup();
    }

    void FixedUpdate()
    {
        bool isGrounded = true;
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);
        }

        bool seesPlayer = false;
        if (playerCheck != null)
        {
            Vector2 lookDirection = facingRight ? Vector2.right : Vector2.left;
            seesPlayer = Physics2D.Raycast(playerCheck.position, lookDirection, lookDistance, whatIsPlayer);
            // disable this when outside the camera zone
            if ((cameraZone != null) && !cameraZoneScript.targetInside)
            {
                seesPlayer = false;
            }
        }

        if (moveEnabled)
        {
            if (moving)
            {
                if (chargeEnabled && seesPlayer && !running)
                {
                    running = true;
                    PlaySound(impLaughSound);
                }
                if (running) ChangeAnimation(AnimationType.RUN);
                else ChangeAnimation(AnimationType.WALK);
                // assuming simple horizontal movement for now
                bool stopNow = !isGrounded;
                bool turnAround = false;
                if (isGrounded)
                {
                    Vector2 position = transform.position;
                    float moveSpeed = running ? runSpeed : walkSpeed;
                    float delta = moveSpeed * Time.fixedDeltaTime;
                    position.x += facingRight ? delta : -delta;
                    if (facingRight && (position.x >= rangeRight))
                    {
                        position.x = rangeRight;
                        stopNow = true;
                        turnAround = true;
                    }
                    if (!facingRight && (position.x <= rangeLeft))
                    {
                        position.x = rangeLeft;
                        stopNow = true;
                        turnAround = true;
                    }
                    transform.position = position;
                }
                // decrement timer and stop moving when expired
                if (stopNow)
                {
                    moving = false;
                    running = false;
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

        ImpUpdate();
        CartUpdate();
    }

    void Flip()
    {
        facingRight = !facingRight;
        //Vector3 scaler = transform.localScale;
        //scaler.x *= -1;
        //transform.localScale = scaler;
    }

    void ImpSetup()
    {
        particletrail = transform.Find("ParticleTrail").GetComponent<ParticleSystem>();
        imps = new GameObject[impCount];
        impGlows = new Transform[impCount];
        impAnchors = new Vector2[impCount];
        for (int i=0; i<impCount; i++)
        {
            imps[i] = Instantiate(impPrefab, transform);
            impGlows[i] = imps[i].transform.GetChild(0).GetComponent<Transform>();
            // setup anchors
            Vector2 anchor;
            float spaceX = (impMaxX - impMinX) / impCount;
            float spaceY = (impMaxY - impMinY) / 4;
            int col = i/2;
            int row = i%2;
            anchor.x = impMinX + spaceX + col * 2 * spaceX;
            anchor.y = impMinY + spaceY + row * 2 * spaceY;
            impAnchors[i] = anchor;
            //Debug.Log("anchor " + i + " x=" + anchor.x + " y=" + anchor.y);
            imps[i].transform.localPosition = anchor;
        }
    }

    void ImpUpdate()
    {
        for (int i=0; i<impCount; i++)
        {
            // randomly move imps within assigned space
            float speed = running ? impSpeed : (impSpeed/3);
            Vector2 position = imps[i].transform.localPosition;
            Vector2 randMove = Random.insideUnitCircle * speed * Time.fixedDeltaTime;
            position += randMove;
            Vector2 delta = position - impAnchors[i];
            delta = Vector2.ClampMagnitude(delta, impLeash);
            position = impAnchors[i] + delta;
            imps[i].transform.localPosition = position;
            // make sure imps face direction of movement
            Vector3 scale = imps[i].transform.localScale;
            scale.x = facingRight ? 1 : -1;
            imps[i].transform.localScale = scale;
            // adjust imp glow when running
            Vector2 glowScale = impGlows[i].localScale;
            glowScale.x = running ? impMaxGlow : impMinGlow;
            glowScale.y = running ? impMaxGlow : impMinGlow;
            impGlows[i].localScale = glowScale;
        }
        // emit particles when moving
        ParticleSystem.EmissionModule em = particletrail.emission;
        em.enabled = moving;
        // manage audio
        bool audioOn = ((cameraZoneScript != null) && cameraZoneScript.targetInside);
        if (audioOn)
        {
            if (!musicFadingOut)
            {
                musicFadingOut = true;
                musicFadingIn = false;
                Camera.main.GetComponent<FadeAudioSource>().StopMusic(1f);
            }
            if (audioSource.mute)
            {
                audioSource.mute = false;
            }
            if (audioSource.volume < 1)
            {
                audioSource.volume += Time.fixedDeltaTime;
            }
        }
        else if (!audioSource.mute)
        {
            if (!musicFadingIn)
            {
                musicFadingIn = true;
                musicFadingOut = false;
                Camera.main.GetComponent<FadeAudioSource>().StartMusic(GameData.musicVolume, 10f);
            }
            if (audioSource.volume > 0)
            {
                audioSource.volume -= (Time.fixedDeltaTime/4);
            }
            else
            {
                audioSource.mute = true;
            }
        }
    }

    void CartSetup()
    {
        bounceDelta = 4 * bounceMax * Time.fixedDeltaTime;
        bounceOriginalOffset = cartCollider.offset.y;
    }

    void CartUpdate()
    {
        if (moving)
        {
            Vector2 offset = cartCollider.offset;
            offset.y -= bounceDelta;
            if (running) offset.y -= bounceDelta;
            if ((offset.y <= bounceOriginalOffset - bounceMax) || (offset.y >= bounceOriginalOffset))
            {
                bounceDelta = -bounceDelta;
            }
            cartCollider.offset = offset;
        }
    }

    void ChangeAnimation(AnimationType anim, bool restart = false)
    {
        // avoid restarting same animation unintentionally
        if ((currentAnim == anim) && !restart) return;

        // record current animation
        currentAnim = anim;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.gameObject.CompareTag("PlayerAttack"))
        {
            Destroy(gameObject);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }
        if (playerCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(playerCheck.position, checkRadius);
        }
    }

    private void PlaySound(AudioClip sound, float volume = 1f)
    {
        if (sound != null)
        {
            audioSource.PlayOneShot(sound, volume);
        }
    }
}
