using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Event_2_1 : MonoBehaviour
{
    public GameObject player;
    public GameObject entryDoor;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Sequencer());
    }

    IEnumerator Sequencer()
    {
        CameraController cameraController = Camera.main.GetComponent<CameraController>();
        PlayerController playerController = player.GetComponent<PlayerController>();

        // set initial player state
        playerController.canDash = false;
        playerController.canField = false;
        playerController.canBlast = false;
        playerController.canRotate = false;
        playerController.UpdateUI();

        float sequenceDuration = 4f;

        // set player to be initially invisible
        playerController.SetAlpha(0);
        playerController.FreezePlayer(sequenceDuration);

        // zoom camera and center on doorway
        cameraController.Look(entryDoor, sequenceDuration, 0);
        cameraController.Zoom(2, sequenceDuration, 0, 1);

        // trigger door open animation
        yield return new WaitForSeconds(0.5f);
        entryDoor.GetComponent<Animator>().SetBool("Open", true);

        // wait for animation, etc
        yield return new WaitForSeconds(2f);

        // fade in player
        float fadeTime = 1.5f;
        for (float i=0; i<fadeTime; i += Time.deltaTime)
        {
            float a = (i/fadeTime);
            playerController.SetAlpha(i/fadeTime);
            yield return null;
        }
    }
}
