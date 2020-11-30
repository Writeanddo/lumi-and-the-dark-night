using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeBlast : MonoBehaviour
{
    private float lifeTimer = 0f;
    private float maxLife = 5f;

    void Update()
    {
        lifeTimer += Time.deltaTime;

        if (lifeTimer > maxLife)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(gameObject);
    }
}
