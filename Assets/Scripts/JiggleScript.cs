using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JiggleScript : MonoBehaviour
{
    public Vector3 originalPos;
    public Vector3 origLocalPos;
    public Vector3 targetPos;

    public float jiggleAmount = 0.25f;
    public float jiggleSpeed = 10f;

    public bool useLocalPosition = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void OnEnable()
    {
        originalPos = transform.position;
        origLocalPos = transform.localPosition;

        if (useLocalPosition)
        {
            targetPos = origLocalPos;
        }
        else
        {
            targetPos = originalPos;
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (useLocalPosition)
        {
            JiggleLocal();
        }
        else
        {
            JiggleGlobal();
        }
    }

    void JiggleGlobal()
    {
        Vector3 delta = targetPos - transform.position;

        if (delta.magnitude > 0.01f)
        {
            // Move towards target position
            transform.position = Vector3.Slerp(transform.position, targetPos, jiggleSpeed * Time.deltaTime);
        }
        else
        {
            // Pick new target position
            targetPos = originalPos + Random.insideUnitSphere * jiggleAmount;
        }
    }

    void JiggleLocal()
    {
        Vector3 delta = targetPos - transform.localPosition;

        if (delta.magnitude > 0.01f)
        {
            // Move towards target position
            transform.localPosition = Vector3.Slerp(transform.localPosition, targetPos, jiggleSpeed * Time.deltaTime);
        }
        else
        {
            // Pick new target position
            targetPos = origLocalPos + Random.insideUnitSphere * jiggleAmount;
        }
    }

}
