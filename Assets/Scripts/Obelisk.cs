using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obelisk : MonoBehaviour
{
    public GameObject glow;
    public Color[] damagedColor;

    public GameObject darkEnergyPrefab;
    public float energySpeed;
    public float energyAngle;
    private float energyTimer = 0f;
    private float energyTimeInterval = 2f;

    public GameObject battyPrefab;
    private float battyOffsetY = 2f;

    private GameObject boss;

    int health = 3;

    void Awake()
    {
        boss = GameObject.Find("EndBoss");
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.gameObject.CompareTag("PlayerAttack"))
        {
            GetHurt();
        }
    }

    private void Update()
    {
        if (energyTimer > 0)
        {
            energyTimer -= Time.deltaTime;
        }

        if (health > 0 && energyTimer <=0 && boss != null)
        {
            // spawn dark energy targeting boss
            GameObject energy = Instantiate(darkEnergyPrefab, transform.position, transform.rotation);
            energy.GetComponent<HomingScript>().SetTargetObject(boss, 0f, 0f);
            energy.GetComponent<HomingScript>().SetMinimumDistance(6f);
            energy.GetComponent<HomingScript>().SetSpeedAndAngle(energySpeed, energyAngle);
            energyTimer = energyTimeInterval;
        }
    }

    void GetHurt()
    {
        if (health == 3)
        {
            health = 2;
            GetComponent<SpriteRenderer>().color = damagedColor[health];
            SpawnBatty();
        }
        else if (health == 2)
        {
            health = 1;
            GetComponent<SpriteRenderer>().color = damagedColor[health];
        }
        else if (health == 1)
        {
            health = 0;
            GetComponent<SpriteRenderer>().color = damagedColor[health];
            glow.GetComponent<SpriteRenderer>().enabled = false;
            gameObject.tag = "Untagged";
            GetComponent<Collider2D>().enabled = false;
            if (boss)
            {
                boss.GetComponent<EndBossController>().ObeliskDestroyed();
            }
        }
    }

    void SpawnBatty()
    {
        Vector3 pos = transform.position;
        pos.y += battyOffsetY;
        GameObject batty = Instantiate(battyPrefab, pos, transform.rotation);
        BattyController controller = batty.GetComponent<BattyController>();
        controller.inFreeFlightMode = true;
        controller.moveDistance = 0;
        controller.startFacingLeft = (pos.x > 0);
    }
}
