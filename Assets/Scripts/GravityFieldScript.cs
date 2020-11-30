using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityFieldScript : MonoBehaviour
{
    private float fieldAcceleration = 200f;
    private float fieldVelocity = 20f;
    //public float fieldVelocity = 10f;
    private float fieldDuration = 2.5f;


    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, fieldDuration);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        Rigidbody2D rb;
        rb = collision.gameObject.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            //rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y + fieldVelocity);

            Vector2 localVelocity = transform.InverseTransformVector(rb.velocity);

            //localVelocity.y += fieldVelocity;
            localVelocity.y = Mathf.Min(fieldVelocity, localVelocity.y + fieldAcceleration * Time.deltaTime);

            rb.velocity = transform.TransformVector(localVelocity);
        }
    }
}
