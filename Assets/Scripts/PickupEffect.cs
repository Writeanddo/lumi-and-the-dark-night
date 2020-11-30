using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupEffect : MonoBehaviour
{
    float timeToLive = 1.0f;
    float time;

    // Start is called before the first frame update
    void Start()
    {
        time = 0f;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        time += Time.fixedDeltaTime;
        if (time > timeToLive)
        {
            Destroy(gameObject);
        }
    }
}
