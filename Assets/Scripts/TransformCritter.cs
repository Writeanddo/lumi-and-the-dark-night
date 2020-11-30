using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformCritter : MonoBehaviour
{
    public GameObject darkPuff;
    public GameObject meaniePrefab;

    public Vector2 offset = new Vector2 (0,0);

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("DamagingTouch"))
        {
            StartCoroutine(DoCritterTransform());
        }
    }

    IEnumerator DoCritterTransform()
    {
        GetComponent<SpriteRenderer>().enabled = false;
        Instantiate(darkPuff, transform.position, transform.rotation);
        yield return new WaitForSeconds(0.25f);
        GameObject meanie = Instantiate(meaniePrefab, transform.position, transform.rotation);
        meanie.GetComponent<MeanieController>().moveEnabled = false;
        meanie.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
        Destroy(gameObject);
    }

}
