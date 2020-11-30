using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateObjectOnTigger : MonoBehaviour
{
    public GameObject obj;

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            obj.SetActive(true);
            Destroy(gameObject);
        }
    }
}
