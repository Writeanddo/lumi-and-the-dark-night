using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoonCredits : MonoBehaviour
{
    public GameObject dialogPoint;

    bool playing = false;
    bool stopping = false;

    public void PlayCredits()
    {
        if (!playing)
        {
            playing = true;
            StartCoroutine(PlayCreditsCoroutine());
        }
    }

    public void StopCredits()
    {
        if (playing)
        {
            stopping = true;
        }
    }

    IEnumerator PlayCreditsCoroutine()
    {
        DialoguePointScript script = dialogPoint.GetComponent<DialoguePointScript>();
        float duration = 2.5f;
        float pace = 3f;

        if (!stopping)
        {
            script.ShowText("Design, Art, Programming & Animation by TX Brothers", duration);
            yield return new WaitForSeconds(pace);
        }

        if (!stopping)
        {
            script.ShowText("Music:\nLumi's Theme and Lumi's Finale by Elegant Possum", duration);
            yield return new WaitForSeconds(pace);
        }

        if (!stopping)
        {
            script.ShowText("Music:\nCaves of Sorrow by Alexandr Zhelanov", duration);
            yield return new WaitForSeconds(pace);
        }

        if (!stopping)
        {
            script.ShowText("Font:\nDark Poestry by Daniel Hochard", duration);
            yield return new WaitForSeconds(pace);
        }

        if (!stopping)
        {
            script.ShowText("Font:\nRubik by Hubert and Fischer", duration);
            yield return new WaitForSeconds(pace);
        }

        if (!stopping)
        {
            script.ShowText("SFX:\nDemonic Woman Scream by nick121087", duration);
            yield return new WaitForSeconds(pace);
        }

        if (!stopping)
        {
            script.ShowText("SFX:\nFish Splashing by BlastwaveFx", duration);
            yield return new WaitForSeconds(pace);
        }
        if (!stopping)
        {
            script.ShowText("SFX:\nPterodactyl Scream by Mike Koenig", duration);
            yield return new WaitForSeconds(pace);
        }
        if (!stopping)
        {
            script.ShowText("SFX:\nWater Blurp by Fesliyan Studios", duration);
            yield return new WaitForSeconds(pace);
        }
        if (!stopping)
        {
            script.ShowText("SFX:\nHeavy Thunder by Blue Delta", duration);
            yield return new WaitForSeconds(pace);
        }
        if (!stopping)
        {
            script.ShowText("SFX:\nClick9 by stijn", duration);
            yield return new WaitForSeconds(pace);
        }

        playing = false;
        stopping = false;
    }
}
