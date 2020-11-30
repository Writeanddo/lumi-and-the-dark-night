using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenCaptureScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ScreenCapture.CaptureScreenshot("Lumi Screenshot " + System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".png");
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            StartCoroutine(CaptureFrames(12, 2f));
        }
        
    }

    IEnumerator CaptureFrames(int frames, float duration)
    {

        for (int i=0; i < frames; i++)
        {
            ScreenCapture.CaptureScreenshot("Lumi Animation " + i.ToString() + ".png");
            yield return new WaitForSeconds(duration / (frames - 1));
        }

    }
}
