using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Event_6_2_RiftCollapse : MonoBehaviour
{
    public GameObject rift;
    public GameObject burst1;
    public GameObject burst2;
    public GameObject burst3;
    public GameObject smoke;
    public GameObject boss;

    public MoonCredits moonCredits;

    public GameObject bonusOrbArea;
    public GameObject orbPrefab;

    public GameObject lookTarget;

    public SpriteRenderer dimmer;
    public SpriteRenderer moon;

    private Vector3 boss_position;
    private bool eventStarted = false;

    public PlayerUIScript playerUI;

    private AudioSource audiosource;
    private CameraController camController;
    public AudioClip finalMusic;
    public AudioClip explode;
    public AudioClip riftCollapse;

    // Start is called before the first frame update
    void Start()
    {
        boss_position = boss.transform.position;
        audiosource = Camera.main.GetComponent<AudioSource>();
        camController = Camera.main.GetComponent<CameraController>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        if (boss != null)
        {
            boss_position = boss.transform.position;
        }
        else if (!eventStarted)
        {
            transform.position = boss_position;
            eventStarted = true;
            StartCoroutine(CollapseSequence());
        }
    }

    IEnumerator CollapseSequence()
    {
        Camera.main.GetComponent<FadeAudioSource>().StopMusic(3f);

        DestroyAllEnemies();
        smoke.SetActive(true);

        camController.Look(lookTarget, 10f);

        burst1.SetActive(true);
        AudioSource.PlayClipAtPoint(explode, new Vector3(boss_position.x, boss_position.y, Camera.main.transform.position.z), 1.0f);
        yield return new WaitForSeconds(1f);

        burst2.SetActive(true);
        AudioSource.PlayClipAtPoint(explode, new Vector3(boss_position.x, boss_position.y, Camera.main.transform.position.z), 1.0f);
        yield return new WaitForSeconds(1f);

        burst3.SetActive(true);
        AudioSource.PlayClipAtPoint(explode, new Vector3(boss_position.x, boss_position.y, Camera.main.transform.position.z), 1.0f);
        yield return new WaitForSeconds(1f);

        var emission = smoke.GetComponent<ParticleSystem>().emission;
        emission.rateOverTime = 0;

        float moon_alpha_start = moon.color.a;
        float dimmer_alpha_start = dimmer.color.a;

        float dimmerTime = 4f;
        for (float i = 0; i < dimmerTime; i += Time.deltaTime)
        {
            // fade dimmer
            Color dimmerColor = dimmer.color;
            dimmerColor.a = Mathf.Lerp(dimmer_alpha_start, 0f, i/dimmerTime);
            dimmer.color = dimmerColor;

            // illum moon
            Color moonColor = moon.color;
            moonColor.a = Mathf.Lerp(moon_alpha_start, 1f, i / dimmerTime); ;
            moon.color = moonColor;

            yield return null;
        }

        rift.GetComponent<Animator>().SetBool("Visible", false);
        AudioSource.PlayClipAtPoint(riftCollapse, new Vector3(rift.transform.position.x, rift.transform.position.y, Camera.main.transform.position.z), 1.0f);
        yield return new WaitForSeconds(2f);

        rift.SetActive(false);
        audiosource.clip = finalMusic;
        Camera.main.GetComponent<FadeAudioSource>().StartMusic(GameData.musicVolume, 3f);

        moonCredits.PlayCredits();
        StartCoroutine(SpawnBonusOrbs());
        playerUI.GameOver();
    }


    void DestroyAllEnemies()
    {
        GameObject[] obj = GameObject.FindGameObjectsWithTag("DamagingTouch");

        foreach (GameObject o in obj)
        {
            BattyController batty = o.GetComponent<BattyController>();
            if (batty != null)
            {
                batty.GetHurt(2);
            }

            MeanieController meanie = o.GetComponent<MeanieController>();
            if (meanie != null)
            {
                meanie.GetHurt(2);
            }

            SpiderController spider = o.GetComponent<SpiderController>();
            if (spider != null)
            {
                spider.GetHurt(3);
            }
        }
    }

    IEnumerator SpawnBonusOrbs()
    {
        float xpos, ypos;
        Bounds area = bonusOrbArea.GetComponent<Collider2D>().bounds;
        Vector2 moon_center = moon.transform.position;
        float moon_radius = 2f;
        Vector2 distance;

        for (int i = 0; i < 16; i++)
        {
            xpos = Random.Range(area.min.x, area.max.x);
            ypos = Random.Range(area.min.y, area.max.y);
            distance = new Vector2(xpos, ypos) - moon_center;

            while ( distance.magnitude < moon_radius)
            {
                xpos = Random.Range(area.min.x, area.max.x);
                ypos = Random.Range(area.min.y, area.max.y);
                distance = new Vector2(xpos, ypos) - moon_center;
            }

            Instantiate(orbPrefab, new Vector3(xpos, ypos, 0), transform.rotation);
            yield return new WaitForSeconds(0.25f);
        }

    }

}
