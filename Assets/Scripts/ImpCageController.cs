using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpCageController : MonoBehaviour
{
    public GameObject impPrefab;
    public AudioClip impLaughSound;

    public float dropRange = 2f;
    public float dropSpeed = 2f;
    public float stopTime = 0f;
    public float riseSpeed = 1f;

    bool dropping = false;
    bool rising = false;
    float rangeTop = 0;
    float rangeBottom = 0;
    float timer = 0;

    int impCount = 4;
    float impMinX = -0.5f;
    float impMaxX = 0.5f;
    float impMinY = -0.20f;
    float impMaxY = 0.20f;
    float impMinGlow = 0.2f;
    float impMaxGlow = 0.3f;
    float impLeash = 0.15f;
    float impSpeed = 5f;

    GameObject[] imps;
    Transform[] impGlows;
    Vector2[] impAnchors;
    ParticleSystem particletrail;
    AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (dropRange < 0)
        {
            rangeBottom = transform.position.y;
            rangeTop = rangeBottom - dropRange;
        }
        else
        {
            rangeTop = transform.position.y;
            rangeBottom = rangeTop - dropRange;
        }
        ImpSetup();
    }

    void FixedUpdate()
    {
        if (timer > 0)
        {
            timer -= Time.fixedDeltaTime;
        }
        else if (dropping)
        {
            Vector2 position = transform.position;
            float delta = dropSpeed * Time.fixedDeltaTime;
            position.y -= delta;
            if (position.y <= rangeBottom)
            {
                dropping = false;
                rising = true;
                timer = stopTime;
            }
            transform.position = position;
        }
        else if (rising)
        {
            Vector2 position = transform.position;
            float delta = riseSpeed * Time.fixedDeltaTime;
            position.y += delta;
            if (position.y >= rangeTop)
            {
                rising = false;
                timer = stopTime;
            }
            transform.position = position;
        }

        ImpUpdate();
    }

    void ImpSetup()
    {
        particletrail = transform.Find("ParticleTrail").GetComponent<ParticleSystem>();
        imps = new GameObject[impCount];
        impGlows = new Transform[impCount];
        impAnchors = new Vector2[impCount];
        for (int i=0; i<impCount; i++)
        {
            imps[i] = Instantiate(impPrefab, transform);
            impGlows[i] = imps[i].transform.GetChild(0).GetComponent<Transform>();
            // setup anchors
            Vector2 anchor;
            float spaceX = (impMaxX - impMinX) / impCount;
            float spaceY = (impMaxY - impMinY) / 4;
            int col = i/2;
            int row = i%2;
            anchor.x = impMinX + spaceX + col * 2 * spaceX;
            anchor.y = impMinY + spaceY + row * 2 * spaceY;
            impAnchors[i] = anchor;
            //Debug.Log("anchor " + i + " x=" + anchor.x + " y=" + anchor.y);
            imps[i].transform.localPosition = anchor;
        }
    }

    void ImpUpdate()
    {
        for (int i=0; i<impCount; i++)
        {
            // randomly move imps within assigned space
            float speed = dropping ? impSpeed : (impSpeed/4);
            Vector2 position = imps[i].transform.localPosition;
            Vector2 randValue = Random.insideUnitCircle;
            Vector2 randMove = randValue * speed * Time.fixedDeltaTime;
            position += randMove;
            Vector2 delta = position - impAnchors[i];
            delta = Vector2.ClampMagnitude(delta, impLeash);
            position = impAnchors[i] + delta;
            imps[i].transform.localPosition = position;
            // randomly change imp facing
            if (randValue.magnitude >= 0.99)
            {
                Vector3 scale = imps[i].transform.localScale;
                scale.x = (randValue.x >= 0) ? 1 : -1;
                imps[i].transform.localScale = scale;
            }
            // adjust imp glow when running
            Vector2 glowScale = impGlows[i].localScale;
            glowScale.x = dropping ? impMaxGlow : impMinGlow;
            glowScale.y = dropping ? impMaxGlow : impMinGlow;
            impGlows[i].localScale = glowScale;
        }
        // emit particles when dropping
        ParticleSystem.EmissionModule em = particletrail.emission;
        em.enabled = dropping || rising;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.gameObject.CompareTag("Player"))
        {
            if ((dropRange != 0) && (timer <= 0) &&  !dropping && !rising)
            {
                dropping = true;
                PlaySound(impLaughSound);
            }
        }
    }

    private void PlaySound(AudioClip sound, float volume = 1f)
    {
        if (sound != null)
        {
            audioSource.PlayOneShot(sound, volume);
        }
    }
}
