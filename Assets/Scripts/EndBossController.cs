using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndBossController : MonoBehaviour
{
    public GameObject head;
    public GameObject headMad;
    public GameObject[] tentacle;
    public GameObject shield;
    public GameObject hurtPrefab;
    public GameObject blastPrefab;
    private GameObject player;

    float[] tentacleCenter;
    float[] tentacleDirection;
    float tentacleRange = 20f;
    float tentacleSpeed = 3f;
    float tentacleMoveProb = 0.5f;
    float tentacleFlipProb = 0; // 0.002f;
    float tentacleSpeedProb = 0.01f;
    float tentacleSpeedRange = 0.4f;
    float tentacleFlailRange = 7f;

    bool headLookingRight = true;
    float headFlipProb = 0; // 0.005f;

    Vector3 tether;
    float hoverSpeed = 1f;
    float hoverRangeX = 5f;
    float hoverRangeY = 2f;
    float madSpeed = 3f;
    float madRangeX = 17f;
    float madRangeY = 6f;

    public bool startMad = false;
    bool mad = false;
    float hurtTimer = 0;
    float hurtSpeed = 7f;
    int hurtCount = 0;
    int hurtDead = 5;
    int obeliskCount = 4;

    float blastInterval = 5f;
    float blastTimer = 3f;
    float blastSpeed = 15f;

    AudioSource audioSource;
    public AudioClip blastSound;
    public AudioClip finalMusic;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        tether = transform.position;
        tentacleCenter = new float[12];
        tentacleDirection = new float[12];
        for (int i=0; i<12; i++)
        {
            float angle = tentacle[i].transform.rotation.eulerAngles.z;
            //float angle = 360 -(5 + 30*i);
            tentacleCenter[i] = angle;
            tentacleDirection[i] = tentacle[i].transform.localPosition.y;
        }
        player = GameObject.Find("Player");
        if (startMad)
        {
            GetMad();
        }
    }

    void FixedUpdate()
    {
        HeadUpdate();
        TentacleUpdate();
        HoverUpdate();
        BlastUpdate();
    }

    bool ProbabilityCheck(float prob)
    {
        float r = Random.Range(0, 1.0f);
        return (r <= prob);
    }

    float ComputeAngleDelta(float a, float b)
    {
        float angle = a - b;
        if (angle < -180) angle += 360;
        if (angle > 180) angle -= 360;
        return angle;
    }

    void HeadUpdate()
    {
        if (headFlipProb == 0)
        {
            if (headLookingRight && (player.transform.position.x < transform.position.x))
            {
                HeadFlip();
            }
            else if (!headLookingRight && (player.transform.position.x > transform.position.x))
            {
                HeadFlip();
            }
        }
        else if (ProbabilityCheck(headFlipProb))
        {
            HeadFlip();
        }
    }

    void HeadFlip()
    {
        Vector3 scale = head.transform.localScale;
        scale.x *= -1;
        head.transform.localScale = scale;
        headLookingRight = !headLookingRight;
    }

    void TentacleUpdate()
    {
        bool flail = (Vector3.Distance(transform.position, player.transform.position) < tentacleFlailRange);
        for (int i=0; i<tentacle.Length; i++)
        {
            if (ProbabilityCheck(tentacleMoveProb) || flail)
            {
                float dir = tentacleDirection[i];
                Quaternion q = tentacle[i].transform.rotation;
                float a = q.eulerAngles.z;
                a += -dir * (tentacleSpeed*Time.fixedDeltaTime);
                if (flail)
                {
                    TentacleCheckFlail(i, ref a);
                }
                //Debug.Log("Tentacle " + i + " scaleX=" + scaleX + " oldA=" + q.eulerAngles.z + " newA=" + a);
                if (TentacleCheckMove(i, a))
                {
                    q = Quaternion.Euler(0, 0, a);
                    tentacle[i].transform.rotation = q;
                }
                else
                {
                    tentacleDirection[i] *= -1;                    
                }
            }
            if (ProbabilityCheck(tentacleFlipProb))
            {
                if (TentacleCheckFlip(i))
                {
                    Vector3 scale = tentacle[i].transform.localScale;
                    scale.y *= -1;
                    tentacle[i].transform.localScale = scale;
                }
            }
            if (ProbabilityCheck(tentacleSpeedProb) || flail)
            {
                float speed = flail ? 1f : Random.Range(1-tentacleSpeedRange, 1.0f);
                tentacle[i].GetComponent<Animator>().speed = speed;
            }
        }
    }

    bool TentacleCheckFlail(int i, ref float a)
    {
        // compute angle toward player
        Vector2 p1 = tentacle[i].transform.position;
        Vector2 p2 = player.transform.position;
        float angle = Mathf.Atan2(p2.y-p1.y, p2.x-p1.x) * Mathf.Rad2Deg;
        //Debug.Log("Tentacle " + i + " a=" + a + " angle=" + angle);

        // check whether angle is within tentacle range
        float delta = ComputeAngleDelta(angle, tentacleCenter[i]);
        if ((delta < tentacleRange) && (delta > -tentacleRange))
        {
            //Debug.Log("Flailed tentacle " + i + " angle=" + angle);
            a = angle;
            return true;
        }

        return false;
    }

    bool TentacleCheckMove(int i, float a)
    {
        // move would exceed max range
        float delta = ComputeAngleDelta(a, tentacleCenter[i]);
        //Debug.Log("Tentacle" + i + " a=" + a + " center=" +  tentacleCenter[i] + " delta=" + delta);
        if ((delta > tentacleRange) || (delta < -tentacleRange)) return false;

        return TentacleCheckMargin(i, a, false);
    }

    bool TentacleCheckFlip(int i)
    {
        float a = tentacle[i].transform.rotation.eulerAngles.z;
        return TentacleCheckMargin(i, a, true);
    }

    bool TentacleCheckMargin(int i, float a, bool flip)
    {
        float margin1 = 20f;
        float margin2 = 20f;
        int j = (i > 0) ? i-1 : tentacle.Length-1;
        int k = (i < tentacle.Length-1) ? i+1 : 0;
        float myScale = flip ? -tentacle[i].transform.localScale.y : tentacle[i].transform.localScale.x;
        float leftScale = tentacle[j].transform.localScale.y;
        float rightScale = tentacle[k].transform.localScale.y;
        float leftAngle = tentacle[j].transform.rotation.eulerAngles.z;
        float rightAngle = tentacle[k].transform.rotation.eulerAngles.z;
        float leftGap = ComputeAngleDelta(leftAngle, a);
        float rightGap = ComputeAngleDelta(a, rightAngle);

        if (myScale > 0) // bending left
        {
            // check left tentacle
            if (leftScale > 0)
            {
                if (leftGap < 0) return false;
            }
            else
            {
                if (leftGap < margin2) return false;
            }
            // check right tentacle
            if (rightScale > 0)
            {
                if (rightGap < 0) return false;
            }
            else
            {
                if (rightGap < margin1) return false;
            }
        }
        else // bending right
        {
            // check left tentacle
            if (leftScale < 0)
            {
                if (leftGap < 0) return false;
            }
            else
            {
                if (leftGap < margin1) return false;
            }
            // check right tentacle
            if (rightScale < 0)
            {
                if (rightGap < 0) return false;
            }
            else
            {
                if (rightGap < margin2) return false;
            }
        }

        // okay
        return true;
    }

    void HoverUpdate()
    {
        Vector3 target;
        if (hurtTimer > 0)
        {
            hurtTimer -= Time.fixedDeltaTime;
            float delta = hurtSpeed * Time.fixedDeltaTime;
            Vector3 away = transform.position;
            away.x = (player.transform.position.x < transform.position.x) ? 100f : -100f;
            away.y = (player.transform.position.y < transform.position.y) ? 100f : -100f;
            target = Vector3.MoveTowards(transform.position, away, delta);
        }
        else
        {
            float speed = mad ? madSpeed+(hurtCount*0.9f) : hoverSpeed;
            float delta = speed * Time.fixedDeltaTime;
            target = Vector3.MoveTowards(transform.position, player.transform.position, delta);
        }

        float rangeX = mad ? madRangeX : hoverRangeX;
        target.x = Mathf.Clamp(target.x, tether.x - rangeX, tether.x + rangeX);
        float rangeY = mad ? madRangeY : hoverRangeY;
        target.y = Mathf.Clamp(target.y, tether.y - rangeY, tether.y + rangeY);
        transform.position = target;
    }

    void BlastUpdate()
    {
        if (mad)
        {
            if (blastTimer > 0)
            {
                blastTimer -= Time.fixedDeltaTime;
            }
            else if (hurtTimer > 0)
            {
                blastTimer = 0;
            }
            else
            {
                blastTimer = blastInterval - (hurtCount*0.5f);
                GameObject blast = Instantiate(blastPrefab, headMad.transform.GetChild(0).position, Quaternion.identity);
                Rigidbody2D rb = blast.GetComponent<Rigidbody2D>();
                Vector3 dir = player.transform.position - blast.transform.position;
                rb.velocity = Vector3.Normalize(dir) * blastSpeed;
                PlaySound(blastSound);
            }
        }
    }

    public void ObeliskDestroyed()
    {
        Debug.Log("Obelisk destroyed");
        obeliskCount--;
        if (obeliskCount == 0)
        {
            Debug.Log("Shield destroyed");
            GetMad();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.gameObject.CompareTag("PlayerAttack"))
        {
            GetHurt();
        }
    }

    void GetMad()
    {
        StartCoroutine(TransitionMusic(finalMusic));
        Destroy(shield);
        head.SetActive(false);
        headMad.SetActive(true);
        mad = true;
        CameraController camController = Camera.main.GetComponent<CameraController>();
        camController.Shake(3f, 0.4f);
    }

    void GetHurt()
    {
        if (hurtTimer <= 0)
        {
            hurtCount++;
            hurtTimer = 2f;
            Instantiate(hurtPrefab, headMad.transform.GetChild(0));
            CameraController camController = Camera.main.GetComponent<CameraController>();
            camController.Shake(hurtTimer, 0.4f);
        }
        if (hurtCount == hurtDead)
        {
            GetDead();
        }
    }

    void GetDead()
    {
        Debug.Log("BOSS IS DEAD!");
        Destroy(gameObject);
    }

    private void PlaySound(AudioClip sound, float volume = 1f)
    {
        if (sound != null)
        {
            audioSource.PlayOneShot(sound, volume);
        }
    }

    IEnumerator TransitionMusic(AudioClip newMusic)
    {
        AudioSource musicAudioSource = Camera.main.GetComponent<AudioSource>();
        musicAudioSource.GetComponentInParent<FadeAudioSource>().StopMusic(2f);
        yield return new WaitForSeconds(3f);
        musicAudioSource.clip = newMusic;
        musicAudioSource.GetComponentInParent<FadeAudioSource>().StartMusic(GameData.musicVolume, 2f);
    }
}
