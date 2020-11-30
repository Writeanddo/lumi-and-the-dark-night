using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossImpalerScript : MonoBehaviour
{
    Collider2D parentCollider;
    BatBossController boss;
    MeanieController meanie;
    public GameObject hitEffectPrefab;

    // Start is called before the first frame update
    void Start()
    {
        parentCollider = GetComponentInParent<Collider2D>();
        if (parentCollider != null)
        {
            Physics2D.IgnoreCollision(parentCollider, GetComponent<Collider2D>());
        }

        meanie = GetComponentInParent<MeanieController>();
    }

    void Awake()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Boss"))
        {
            Instantiate(hitEffectPrefab, transform.position, transform.rotation);

            boss = collider.gameObject.GetComponent<BatBossController>();
            if (boss != null)
            {
                boss.TakeDamage();
            }
            if (meanie != null)
            {
                meanie.DieInPuff();
            }

        }
    }


}
