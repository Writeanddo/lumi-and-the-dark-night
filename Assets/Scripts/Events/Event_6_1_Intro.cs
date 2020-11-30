using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Event_6_1_Intro : MonoBehaviour
{
    public GameObject megaOrb;
    public GameObject[] otherOrb;
    public GameObject boss;

    public PlayerUIScript playerUI;

    public AudioSource audioSource;
    //public AudioClip ominousMusic;

    void Awake()
    {
        OrbSwap();
    }

    void OrbSwap()
    {
        int r = Random.Range(0, otherOrb.Length+1);
        if (r < otherOrb.Length)
        {
            //Debug.Log("Swapping orb into position " + r);
            Vector2 pos = otherOrb[r].transform.position;
            otherOrb[r].transform.position = megaOrb.transform.position;
            megaOrb.transform.position = pos;
        }
    }

    public void BossReveal()
    {
        Camera.main.GetComponent<FadeAudioSource>().StartMusic(GameData.musicVolume);

        playerUI.ShowDialogText("In the darkened sky above the quarry was the Rift, and something from beyond.");
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            BossReveal();
            Destroy(GetComponent<Collider2D>());  //only do once
        }
    }
}
