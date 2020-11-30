using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebBarrier : MonoBehaviour
{
    public GameObject webStickyPrefab;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.gameObject.CompareTag("PlayerAttack"))
        {
            Instantiate(webStickyPrefab, transform.position - 0.3f * Vector3.right - 0.7f * Vector3.up, transform.rotation);
            Instantiate(webStickyPrefab, transform.position + 0.3f * Vector3.right - 0.65f * Vector3.up, transform.rotation);
            Destroy(gameObject);
        }
    }
}
