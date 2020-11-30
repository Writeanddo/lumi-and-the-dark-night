using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingScript : MonoBehaviour
{
    Rigidbody2D rb;

    public float initialSpeed = 10f;
    public float initialAngle = 45f;

    public float flightTime = 2f;

    private float minDistance = 1f;

    private float timer = 0f;
    public GameObject targetObject;

    private Vector2 targetOffset = new Vector2 (0,0);

    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0;
        rb.freezeRotation = true;
        SetSpeedAndAngle(initialSpeed, initialAngle);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if (targetObject != null)
        {
            Vector2 distance = (targetObject.transform.position + (Vector3)targetOffset) - transform.position;
            if (distance.magnitude < minDistance)
                Destroy(gameObject);
        }

    }

    private void FixedUpdate()
    {
        if (targetObject != null)
        {
            Vector2 newVelocity = rb.velocity;
            Vector2.SmoothDamp(transform.position, (Vector2)targetObject.transform.position + targetOffset, ref newVelocity, flightTime - timer);
            rb.velocity = newVelocity;
        }
    }

    public void SetTargetObject(GameObject target, float xoff = 0, float yoff = 0)
    {
        if (target != null)
        {
            targetObject = target;
            targetOffset = new Vector2(xoff, yoff);

        }
    }

    public void SetCurrentVelocity(Vector2 velocity)
    {
        rb.velocity = velocity;
    }

    public void SetSpeedAndAngle(float speed, float angle)
    {
        Vector2 newVelocity = Quaternion.Euler(0f,0f,angle) * Vector2.right*speed;
        rb.velocity = newVelocity;
    }

    public void SetMinimumDistance(float distance)
    {
        minDistance = distance;
    }

}
