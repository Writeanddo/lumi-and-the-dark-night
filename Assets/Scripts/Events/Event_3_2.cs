using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Event_3_2 : MonoBehaviour
{
    public ParticleSystem particlesystem;
    private CameraController cam;
    //public float shakeInterval = 1f;
    private bool isShaking = false;
    private float timer;

    void Start()
    {
        var emission = particlesystem.emission;
        emission.enabled = false;
        particlesystem.Pause();
    }

    // Start is called before the first frame update
    void Awake()
    {
        cam = Camera.main.GetComponent<CameraController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            if (isShaking)
            {
                cam.Shake(0.25f, 0.25f);
            }
            timer = Random.Range(1f,4f);
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (!particlesystem.isPlaying)
            {
                particlesystem.Play();
            }
            var emission = particlesystem.emission;
            emission.enabled = true;
            isShaking = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            var emission = particlesystem.emission;
            emission.enabled = false;
            isShaking = false;
        }
    }
}
