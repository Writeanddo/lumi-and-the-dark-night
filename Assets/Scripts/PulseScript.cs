using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PulseScript : MonoBehaviour
{
    public bool pulseEnabled = false;
    public bool destroyAfterCycle = false;
    public bool stopAtMax = false;

    private Vector3 origScale;

    public float pulseTime = 1.0f;
    public float pulseAmount = 0.2f;

    private float animTimer = 0f;
    private Transform transformCached;

    void Start()
    {
        transformCached = transform;
        origScale = transformCached.localScale;
    }

    void Update()
    {
        float scale;

        if (stopAtMax && animTimer > 0.5f* pulseTime)
        {
            pulseEnabled = false;
        }

        if (pulseEnabled)
        {
            if (transformCached.GetComponent<Renderer>() != null)
            {

                animTimer += Time.deltaTime;
                if (animTimer > pulseTime)
                {
                    if (destroyAfterCycle)
                    {
                        Destroy(gameObject); 
                    }
                    else
                    {
                        animTimer -= pulseTime;
                    }
                }

                if (animTimer <= pulseTime/2)
                {
                    scale = 1.0f + pulseAmount * animTimer / (0.5f * pulseTime) ;
                }
                else
                {
                    scale = 1.0f + pulseAmount - pulseAmount * (animTimer - 0.5f * pulseTime) / (0.5f * pulseTime);
                }

                transformCached.localScale = new Vector3(scale * origScale.x, scale * origScale.y, origScale.z);
            }
        }
    }
}
