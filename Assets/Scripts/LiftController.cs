using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiftController : MonoBehaviour
{
    private Rigidbody2D rb;

    public Transform[] liftPositions;

    private Vector3 targetLiftPosition;
    public float liftSpeed = 5f;

    public bool moveToFirstPosition = false;

    private Transform cable;
    private float cableTopPosition;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (moveToFirstPosition)
        {
            ActivatePosition(0);
        }
        else
        {
            targetLiftPosition = transform.position;
        }

        cable = transform.Find("Cable").transform;
        cableTopPosition = cable.position.y + 0.5f * cable.localScale.y;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 distance = targetLiftPosition - transform.position;

        if (distance.magnitude > 0.01f)
        {
            //transform.position = Vector3.Lerp(transform.position, targetLiftPosition, liftSpeed * Time.deltaTime);
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            rb.velocity = new Vector2(0f, liftSpeed* (distance.y > 0 ? 1 : -1));
        }
        else
        {
            rb.velocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        AdjustCable();
    }

    void AdjustCable()
    {
        float cableBottomPosition = cable.position.y - 0.5f * cable.localScale.y;
        cable.localScale = new Vector3(cable.localScale.x, cableTopPosition-cableBottomPosition, cable.localScale.z);
        cable.position = new Vector3(cable.position.x, 0.5f * (cableTopPosition + cableBottomPosition), cable.position.z);
    }

    public void ActivatePosition(int n)
    {
        Debug.Log("Moving lift to position " + n);
        targetLiftPosition = liftPositions[n].position;
    }
}
