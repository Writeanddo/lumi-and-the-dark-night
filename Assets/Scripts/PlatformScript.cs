using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformScript : MonoBehaviour
{
    public float downTimeValue = 0.5f;
    private float downTime = 0f;
    private bool inContact = false;

    private PlatformEffector2D effector;

    // Start is called before the first frame update
    void Start()
    {
        effector = GetComponent<PlatformEffector2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if ((Input.GetAxis("Vertical") == -1) && inContact)
        {
            downTime += Time.deltaTime;
        }
        else
        {
            downTime = 0;
        }

        if (downTime > downTimeValue)
        {
            effector.rotationalOffset = 180f;
        }

        if (Input.GetButtonDown("Jump"))
        {
            effector.rotationalOffset = 0f;
        }

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        inContact = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        inContact = false;
    }

}
