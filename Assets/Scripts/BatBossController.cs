using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatBossController : MonoBehaviour
{
    private Animator animator;
    private GameObject player;
    private AudioSource audiosource;
    private CameraController cam;

    public Transform bossSpawnLeft;
    public Transform bossSpawnRight;

    public Lvl4_BossFight fightManager;
    public ParticleSystem fallingStalactites;

    private bool isAngry = false;
    private float angryInterval = 6.0f;
    //private float angryDuration = 3.0f;
    private float angryTimer = 2.0f;

    public bool isFlying = true;
    private bool facingRight = true;
    public float flyingSpeed = 10f;
    private float minDiveY = 6f;
    private float attackTimer = 0f;
    public float attackInterval = 1.7f;

    private int numAttacks = 0;
    public int maxAttacks = 3;

    public bool isScreaming = false;
    private int numScreams = 0;
    public int maxScreams = 3;

    private bool isHurt = false;
    private float hurtTimer = 0f;
    private float hurtDuration = 1f;

    public bool isCrashing = false;
    private float crashHeight = 2f;
    private Vector3 crashPosition;


    public AudioClip furyScream;
    public AudioClip sonicAttackSound;

    public GameObject sonicAttackCone;


    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
        crashPosition = new Vector3(bossSpawnRight.position.x-0.1f, crashHeight, 0f);
    }

    void Awake()
    {
        //rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audiosource = GetComponent<AudioSource>();
        cam = Camera.main.GetComponent<CameraController>();

        if (sonicAttackCone != null)
            sonicAttackCone.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (angryTimer > 0)
        {
            angryTimer -= Time.deltaTime;
        }

        if (hurtTimer > 0)
        {
            hurtTimer -= Time.deltaTime;
        }

        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }
        
    }

    private void FixedUpdate()
    {
        if (isHurt)
        {
            numAttacks = maxAttacks; // disable attacks for this flight
            // Red animation
            if (hurtTimer <=0)
            {
                isHurt = false;
            }
            animator.SetBool("isHurt", isHurt);
        }
        else if (isCrashing)
        {
            if (!facingRight) Flip();
            Vector2 distance = crashPosition-transform.position;
            if (distance.magnitude < 0.3f)
            {
                fightManager.HandleCrash();
            }
            else
            {
                transform.Translate(distance.normalized * 2f* flyingSpeed * Time.deltaTime);
                //transform.position = Vector3.Lerp(transform.position, crashPosition, flyingSpeed * Time.deltaTime);
                animator.SetBool("isHurt", true);
            }
        }
        else if (isFlying)
        {
            int facing = facingRight ? 1 : -1;
            float newX, newY;

            newX = transform.position.x + facing * flyingSpeed * Time.deltaTime;
            newY = YPositionAlongFlightPath(newX);
            transform.position = new Vector2(newX, newY);

            if (attackTimer <= 0 && numAttacks < maxAttacks)
            {
                StartCoroutine(Attack());
                attackTimer = attackInterval;
                numAttacks++;
            }

            if (facingRight && transform.position.x > bossSpawnRight.position.x)
            {
                fightManager.EndFlight();
                //Destroy(gameObject);
            }

            if (!facingRight && transform.position.x < bossSpawnLeft.position.x)
            {
                fightManager.EndFlight();
                //Destroy(gameObject);
            }


        }
        else if (isScreaming)
        {

            if (angryTimer <= 0)
            {
                if (numScreams < maxScreams)
                {
                    StartCoroutine(Fury());
                    angryTimer = angryInterval;
                    numScreams++;
                }
                else
                {
                    StartCoroutine(EndScream());
                }
            }
            animator.SetBool("isAngry", isAngry);
        }


    }

    public void FlyingRight()
    {
        transform.position = bossSpawnLeft.position;
        isFlying = true;
        numAttacks = 0;
        if (!facingRight) Flip();
     }

    public void FlyingLeft()
    {
        transform.position = bossSpawnRight.position;
        isFlying = true;
        numAttacks = 0;
        if (facingRight) Flip();
    }

    public void StartScream()
    {
        //animator.SetTrigger("FadeIn");
        isScreaming = true;
        numScreams = 0;
    }

    IEnumerator EndScream()
    {
        //fade out
        isScreaming = false;
        //animator.SetTrigger("FadeOut");
        yield return new WaitForSeconds(1.0f);

        fightManager.EndScream();
    }

    public void CrashDive()
    {
        isCrashing = true;
        player.GetComponent<PlayerController>().restartOnDeath = false;
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 Scaler = transform.localScale;
        Scaler.x *= -1;
        transform.localScale = Scaler;
    }

    IEnumerator Attack()
    {
        int facing = facingRight ? 1 : -1;
        sonicAttackCone.SetActive(true);
        PlaySound(sonicAttackSound);
        yield return new WaitForSeconds(sonicAttackSound.length);
        sonicAttackCone.SetActive(false);
    }

    IEnumerator Fury()
    {
        var emission = fallingStalactites.emission;

        isAngry = true;
        PlaySound(furyScream);
        // Activate falling stalactite emission
        if (fallingStalactites.isPaused)
            fallingStalactites.Play();
        emission.enabled = true;
        cam.Shake(2.5f, 0.2f);
        yield return new WaitForSeconds(furyScream.length);

        isAngry = false;
        // Deactivate falling stalactite emission
        emission.enabled = false;
    }

    public void TakeDamage()
    {
        isHurt = true;
        hurtTimer = hurtDuration;
        // let fight manager know boss took damage
        fightManager.BossTakesDamage();
    }

    private void PlaySound(AudioClip sound, float volume = 1f)
    {
        if (sound != null)
        {
            AudioSource.PlayClipAtPoint(sound, new Vector3(transform.position.x, transform.position.y, -10));
            //audiosource.PlayOneShot(sound, volume);
        }
    }

    private float YPositionAlongFlightPath(float xpos)
    {
        float ypos;

        if (xpos >= 0) // right half
        {
            ypos = minDiveY + (bossSpawnRight.position.y - minDiveY) * Mathf.Pow(xpos / bossSpawnRight.position.x,2 );
        }
        else
        {
            ypos = minDiveY + (bossSpawnLeft.position.y - minDiveY) * Mathf.Pow(xpos / bossSpawnLeft.position.x, 2);
        }
        return ypos;
    }

}
