using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundOnParticleCollision : MonoBehaviour
{
    private ParticleSystem part;
    private List<ParticleCollisionEvent> collisionEvents;

    public AudioClip sound;
    public float volume = 1f;
    public float minDistance = 10f;
    public float maxDistance = 20f;

    private void Start()
    {
        part = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
    }

    private void OnParticleCollision(GameObject other)
    {
        int numCollisionEvents = part.GetCollisionEvents(other, collisionEvents);

        // use first collision only
        if (numCollisionEvents > 0)
        {
            Vector3 pos = collisionEvents[0].intersection;
            //AudioSource.PlayClipAtPoint(sound, new Vector3(pos.x, pos.y, Camera.main.transform.position.z), volume);
            PlayClipAtPoint(sound, pos);
        }
    }

    private void PlayClipAtPoint(AudioClip clip, Vector3 pos)
    {
        GameObject audioPlayer = new GameObject("TempAudioPlayer");
        audioPlayer.transform.position = pos;
        AudioSource audioSource = (AudioSource) audioPlayer.AddComponent(typeof(AudioSource));
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.spatialBlend = 1;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
        audioSource.Play();
        Destroy(audioPlayer, clip.length);
    }
}
