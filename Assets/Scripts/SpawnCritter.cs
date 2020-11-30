using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnCritter : MonoBehaviour
{
    public GameObject[] critters;

    // Start is called before the first frame update
    void Start()
    {
        int i = Random.Range(0, critters.Length);
        GameObject critter = Instantiate(critters[i], transform.position + 1.0f*Vector3.up, transform.rotation);
        Rigidbody2D rb = critter.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.velocity = new Vector2(Random.Range(-5f, 5f), Random.Range(0f, 5f));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
