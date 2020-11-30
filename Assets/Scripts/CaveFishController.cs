using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveFishController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sr;

    public float startingHeight;
    public float splashDownHeight;
    private Bounds waterBounds;
    
    private bool isJumping = false;
    private bool hasAttacked = false;

    private Transform player;
    private FishSpawner spawnController;

    public float chaseSpeed = 10f;
    public GameObject splashEffect;
    public AudioClip splashSound;

    // Start is called before the first frame update
    void Start()
    {
        startingHeight = transform.position.y;
    }

    void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = gameObject.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {

        if ((transform.position.y < splashDownHeight) && (rb.velocity.y < -4f))
        {
            Instantiate(splashEffect, transform.position, transform.rotation);
            AudioSource.PlayClipAtPoint(splashSound, new Vector3(transform.position.x,transform.position.y, Camera.main.transform.position.z), 1f);
            Destroy(gameObject);
        }

        Vector3 Scaler = transform.localScale;
        Scaler.x = (rb.velocity.x < 0) ? 1 : -1;  // backwards because sprite was facing left
        transform.localScale = Scaler;

        animator.SetFloat("SpeedY", rb.velocity.y);
    }

    private void FixedUpdate()
    {
        if (!isJumping && !hasAttacked)
        {
            Vector2 distance = ((Vector2) player.position - rb.position);

            bool isAboveWater = (rb.position.y > waterBounds.max.y);

            if (isAboveWater || distance.magnitude < 2f)
            {
                // lock in on current velocity
                sr.color = new Color(1, 1, 1, 1);
                isJumping = true;
            }
            else
            {
                // chase
                rb.velocity = chaseSpeed * distance.normalized;
            }

            //rb.position = Vector2.MoveTowards(rb.position, player.position, chaseSpeed * Time.fixedDeltaTime);
        }
    }

    public void Jump(Vector2 velocity, Bounds bounds, FishSpawner spawner)
    {
        rb.velocity = velocity;
        isJumping = true;
        waterBounds = bounds;
        splashDownHeight = waterBounds.max.y -0.1f;
    }

    public void Chase(Transform target, Bounds bounds,  FishSpawner spawner)
    {
        player = target;
        isJumping = false;
        //rb.gravityScale = 0;
        spawnController = spawner;
        sr.color = new Color(0, 0, 0, 1);
        waterBounds = bounds;
        splashDownHeight = waterBounds.max.y - 0.1f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            hasAttacked = true;
        }
    }
}
