using System.Collections;
using System.Collections.Generic;
//using UnityEditorInternal;
using UnityEngine;

public class Event_1_1 : MonoBehaviour
{
    public GameObject fallingLight;
    public GameObject player;
    public DialoguePointScript dialog;
    public PlayerUIScript playerUI;

    private PlayerController playercontroller;
    private CameraController cam;

    private IEnumerator coroutine;

    void Start()
    {
        cam = Camera.main.GetComponent<CameraController>();
        playercontroller = player.GetComponent<PlayerController>();

        // Set initial player state
        playercontroller.ResetOrbCount();
        playercontroller.canJump = false;
        playercontroller.canDoubleJump = false;
        playercontroller.extraJumpsValue = 0;
        playercontroller.hasTrail = false;
        playercontroller.canDash = false;
        playercontroller.canField = false;
        playercontroller.canBlast = false;
        playercontroller.canRotate = false;
        playercontroller.isSitting = true;
        playercontroller.UpdateUI();
        playerUI.SetUIActive(false);

        fallingLight.SetActive(false);


        coroutine = PlayDialog("Lumi was alone in the darkness.", 2.0f);
        StartCoroutine(coroutine);

        coroutine = PlayDialog("But light was never far away...", 6.0f);
        StartCoroutine(coroutine);

        coroutine = FollowLight(7.0f);
        StartCoroutine(coroutine);
    }

    private void Update()
    {
        if (fallingLight == null)
        {
            playerUI.SetUIActive(true);
            cam.Follow(player);
            playercontroller.ForceStand();
            playercontroller.GainJump();
            Destroy(gameObject);
        }
    }

    IEnumerator PlayDialog(string text, float delay)
    {
        yield return new WaitForSeconds(delay);
        //dialog.ShowText(text, 1.5f);
        playerUI.ShowDialogText(text, 1.5f);
    }

    IEnumerator FollowLight(float delay)
    {
        yield return new WaitForSeconds(delay);
        cam.Follow(fallingLight);
        fallingLight.SetActive(true);
        //fallingLight.GetComponent<AudioSource>().Play();
        //Camera.main.GetComponent<AudioSource>().Play();
        Camera.main.GetComponent<FadeAudioSource>().StartMusic(GameData.musicVolume);
    }

}
