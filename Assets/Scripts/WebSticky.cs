using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebSticky : MonoBehaviour
{
    private float duration_min = 2f;
    private float duration_max = 5f;

    // Start is called before the first frame update
    void Start()
    {

    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("PlayerAttack"))
        {
            Destroy(collider.gameObject);
            Destroy(gameObject);
        }
        else if (collider.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject, Random.Range(duration_min, duration_max));
        }
    }
}
