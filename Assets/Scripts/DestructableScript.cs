using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructableScript : MonoBehaviour
{

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.gameObject.CompareTag("PlayerAttack"))
        {
            Destroy(gameObject);
        }
    }
}
