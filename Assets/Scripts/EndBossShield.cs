using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndBossShield : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip shieldSound;

    void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log("Shield touched");
        GetComponent<Animator>().Play("ShieldFlash");
        PlaySound(shieldSound);
    }

    private void PlaySound(AudioClip sound, float volume = 1f)
    {
        if (sound != null)
        {
            audioSource.PlayOneShot(sound, volume);
        }
    }
}
