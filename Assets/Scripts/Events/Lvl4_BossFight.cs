using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lvl4_BossFight : MonoBehaviour
{
    private CameraController cam;

    public GameObject batboss;
    public GameObject batbossFlying;
    public GameObject meaniePrefab;
    public GameObject impalerPrefab;

    public GameObject tunnelCollapseLeft;
    public GameObject tunnelCollapseRight;
    public GameObject bossBurstEffect;
    public GameObject collapseEffect;

    public Transform bossSpawnLeft;
    public Transform bossSpawnRight;

    public ParticleSystem fallingStalactites;

    public int BossHealth = 6;
    private int hitCounter = 0;
    public int activeMeanies = 0;

    public int phase = 0;

    private bool isReady = false;
    private float intervalTimer = 0f;
    private float actionInterval = 2f;

    public GameObject orbPrefab;

    // Start is called before the first frame update
    void Start()
    {
        batboss.SetActive(false);
        batbossFlying.SetActive(false);
        fallingStalactites.Pause();
    }

    void Awake()
    {
        cam = Camera.main.GetComponent<CameraController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (intervalTimer > 0)
        {
            intervalTimer -= Time.deltaTime;
        }

        if (phase == 1)
        {
            if (isReady && intervalTimer <= 0) // Time for boss flight
            {
                if (hitCounter == 2)
                {
                    StartPhase2();
                }
                else
                {
                    StartFlight();
                }
            }
        }

        if (phase == 2)
        {
            if (isReady && intervalTimer <= 0)
            {
                StartPhase1();
            }
        }
    }

    public void BeginEncounter()
    {
        phase = 0;
        StartCoroutine(CollapseTunnels());
        StartCoroutine(NextPhaseIn(3f));
    }

    public void StartPhase1()
    {
        phase = 1;
        Debug.Log("Starting Phase 1");

        StartCoroutine(SpawnMeanies());
        hitCounter = 0;
    }

    public void StartPhase2()
    {
        phase = 2;
        Debug.Log("Starting Phase 2");

        batboss.SetActive(true);
        batboss.GetComponent<BatBossController>().StartScream();
        isReady = false;

        Instantiate(orbPrefab, new Vector3(Random.Range(bossSpawnLeft.position.x, 0), 8f, 0f),transform.rotation);
        Instantiate(orbPrefab, new Vector3(Random.Range(0, bossSpawnRight.position.x), 8f, 0f), transform.rotation);
    }

    public void StartFlight()
    {
        batbossFlying.SetActive(true);
        if (Random.Range(0,2) == 0)
        {
            batbossFlying.transform.position = bossSpawnLeft.position;
            batbossFlying.GetComponent<BatBossController>().FlyingRight();
        }
        else
        {
            batbossFlying.transform.position = bossSpawnRight.position;
            batbossFlying.GetComponent<BatBossController>().FlyingLeft();
        }
        isReady = false;
    }

    public void EndFlight()
    {
        batbossFlying.SetActive(false);
        isReady = true;
        intervalTimer = actionInterval;
    }

    public void EndScream()
    {
        batboss.SetActive(false);
        isReady = true;
        intervalTimer = actionInterval;
    }

    public void HandleCrash()
    {
        batbossFlying.SetActive(false);
        bossBurstEffect.SetActive(true);
        StartCoroutine(OpenRightTunnel());
    }

    IEnumerator CollapseTunnels()
    {
        cam.Shake(2f, .25f);

        collapseEffect.SetActive(true);
        yield return new WaitForSeconds(1f);
        tunnelCollapseLeft.SetActive(true);
        tunnelCollapseRight.SetActive(true);

        yield return new WaitForSeconds(1f);
        collapseEffect.SetActive(false);
    }

    IEnumerator OpenRightTunnel()
    {
        cam.Shake(.5f, .25f);
        // dust cloud
        yield return new WaitForSeconds(1f);
        tunnelCollapseRight.SetActive(false);
    }

    IEnumerator SpawnMeanies()
    {
        float spawnInterval = 1.5f;
        GameObject meanie;

        isReady = false;
        
        meanie = Instantiate(meaniePrefab, bossSpawnLeft.position, transform.rotation);
        Debug.Log("spawned " + meanie);
        meanie.GetComponent<MeanieController>().walkSpeed = 4;
        Instantiate(impalerPrefab, meanie.transform);
        activeMeanies++;
        yield return new WaitForSeconds(spawnInterval);

        meanie = Instantiate(meaniePrefab, bossSpawnRight.position, transform.rotation);
        Debug.Log("spawned " + meanie);
        meanie.GetComponent<MeanieController>().walkSpeed = 4;
        Instantiate(impalerPrefab, meanie.transform);
        activeMeanies++;

        isReady = true;
        intervalTimer = actionInterval;
    }

    IEnumerator NextPhaseIn(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        if (phase == 1)
        {
            StartPhase2();
        }
        else
        {
            StartPhase1();
        }
    }

    public void MeanieWasDestroyed()
    {
        activeMeanies--;
    }

    public void BossTakesDamage()
    {
        BossHealth--;
        hitCounter++;

        if (BossHealth == 0)
        {
            batbossFlying.GetComponent<BatBossController>().CrashDive();
            cam.Look(bossBurstEffect, 5f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (phase == 0)
        {
            if (collider.gameObject.CompareTag("Player"))
            {
                BeginEncounter();
                Destroy(GetComponent<Collider2D>());  //only do once
            }
        }
    }

}
