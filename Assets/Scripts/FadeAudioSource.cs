using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeAudioSource : MonoBehaviour
{
    public float pauseDuration = 0;
    public float pauseFadeIn = 0;
    public float pauseFadeOut = 0;
    float pauseTimer = 0;
    bool paused = false;

    AudioSource audioSource;
    float savedVolume;
    float targetVolume;
    float deltaVolume;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        targetVolume = audioSource.volume;
        savedVolume = targetVolume;
    }

    public void StartMusic(float volume, float fadeTime = 0)
    {
        paused = false;
        pauseTimer = 0;
        targetVolume = volume;
        savedVolume = volume;
        deltaVolume = (targetVolume - audioSource.volume) * (Time.fixedDeltaTime/fadeTime);
        audioSource.Play();
    }

    public void StopMusic(float fadeTime = 0)
    {
        paused = false;
        targetVolume = 0;
        deltaVolume = -audioSource.volume * (Time.fixedDeltaTime/fadeTime);
    }

    public void PauseMusic()
    {
        paused = true;
        pauseTimer = pauseDuration;
        targetVolume = 0;
        deltaVolume = -audioSource.volume * (Time.fixedDeltaTime/pauseFadeOut);
    }

    public void ResumeMusic()
    {
        StartMusic(savedVolume, pauseFadeIn);
    }

    void FixedUpdate()
    {
        // transition to target volume
        float currentVolume = audioSource.volume;
        if (currentVolume < targetVolume)
        {
            currentVolume += deltaVolume;
            currentVolume = Mathf.Clamp(currentVolume, 0, targetVolume);
            audioSource.volume = currentVolume;
        }
        else if (currentVolume > targetVolume)
        {
            currentVolume += deltaVolume;
            currentVolume = Mathf.Clamp(currentVolume, targetVolume, 1);
            audioSource.volume = currentVolume;
        }

        // stop playing when volume is zero
        if (audioSource.volume == 0)
        {
            audioSource.Stop();
        }

        // handle optional pause
        if (pauseDuration > 0)
        {
            if (audioSource.isPlaying)
            {
                if (!paused && (audioSource.time + pauseFadeOut > audioSource.clip.length))
                {
                    PauseMusic();
                }
            }
            else if (paused)
            {
                pauseTimer -= Time.fixedDeltaTime;
                if (pauseTimer <= 0)
                {
                    ResumeMusic();
                }
            }
        }
    }
}
