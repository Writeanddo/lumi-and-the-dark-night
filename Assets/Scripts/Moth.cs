using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moth : MonoBehaviour
{
    private Rigidbody2D rb;

    private float moveSpeed = 2f;
    private float moveDelay = 1f;

    private Vector3 startingPosition;
    public float moveDistance = 0f;

    private float timer = 0f;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startingPosition = transform.position;
        moveDistance = Mathf.Abs(moveDistance); // accept only positive values

        // Tweak colors
        GetComponent<SpriteRenderer>().color = new Color(Random.Range(0.75f, 1f), Random.Range(0.75f, 1f), Random.Range(0.75f, 1f), 1f);
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 distance = (transform.position - startingPosition);

        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            //Vector2 newVelocity = Random.Range(-1f, 1f) * Vector2.right + Random.Range(-1f, 1f) * Vector2.up;
            Vector2 newVelocity = (Vector2) Random.onUnitSphere * moveSpeed;

            if (moveDistance > 0)
            {
                if (distance.x > moveDistance && newVelocity.x > 0) // too far right and moving right
                    newVelocity.x = -newVelocity.x;

                if (distance.x < -moveDistance && newVelocity.x < 0) // too far left and moving left
                    newVelocity.x = -newVelocity.x;

                if (distance.y > moveDistance && newVelocity.y > 0) // too high and moving up
                    newVelocity.y = -newVelocity.y;

                if (distance.y < -moveDistance && newVelocity.y < 0) // too low and moving down
                    newVelocity.y = -newVelocity.y;
            }

            rb.velocity = newVelocity;

            timer = moveDelay;
        }
    }

    void FixedUpdate()
    {
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.GetMask("Ground", "Default"))
        {
            timer = 0;
        }
        if (collision.collider.gameObject.CompareTag("DeadlyTouch"))
        {
            Destroy(gameObject);
        }
        if (collision.collider.gameObject.CompareTag("DamagingTouch"))
        {
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (moveDistance > 0)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(startingPosition, Vector3.one*2f*moveDistance);
        }

    }

}
