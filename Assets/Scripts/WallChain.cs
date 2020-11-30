using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallChain : MonoBehaviour
{
    Transform chain;
    bool swinging = false;
    bool swingRight = true;
    float swingMaxRot = 25f;
    float swingMaxRate = 80f;
    float swingDecay = 0.8f;
    float swingTarget;
    float swingRate;

    void Awake()
    {
        chain = transform.Find("Chain");
    }

    void FixedUpdate()
    {
        if (swinging)
        {
            float delta = swingRate * Time.fixedDeltaTime;
            float degree = chain.localRotation.eulerAngles.z;
            if (degree > 180) degree -= 360;
            if (swingRight)
            {
                if (degree < swingTarget)
                {
                    chain.Rotate(0, 0, delta);
                    //Debug.Log("Rotate right degree=" + degree + " target=" + swingTarget + " delta=" + delta);
                }
                else if (swingTarget > 0.1f)
                {
                    swingRight = false;
                    swingTarget = -(swingTarget*swingDecay);
                    swingRate = swingRate * swingDecay;
                    //Debug.Log("Swing right reversing degree=" + degree + " target=" + swingTarget);
                }
                else
                {
                    swinging = false;
                    //Debug.Log("Swing right stopped");
                }
            }
            else // swingLeft
            {
                if (degree > swingTarget)
                {
                    chain.Rotate(0, 0, -delta);
                    //Debug.Log("Rotate left degree=" + degree + " target=" + swingTarget + " delta=" + delta);
                }
                else if (swingTarget < -0.1f)
                {
                    swingRight = true;
                    swingTarget = -(swingTarget*swingDecay);
                    swingRate = swingRate * swingDecay;
                    //Debug.Log("Swing left reversing degree=" + degree + " target=" + swingTarget);
                }
                else
                {
                    swinging = false;
                    //Debug.Log("Swing left stopped");
                }
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            swinging = true;
            swingRight = (other.gameObject.transform.position.x > chain.position.x);
            swingTarget = swingRight ? swingMaxRot : -swingMaxRot;
            swingRate = swingMaxRate;
            //Debug.Log("Swing triggered");
        }
    }
}
