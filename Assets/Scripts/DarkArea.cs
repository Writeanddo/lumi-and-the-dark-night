using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarkArea : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            GetComponent<SpriteRenderer>().enabled = true;
        }
    }
}
