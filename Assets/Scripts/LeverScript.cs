using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeverScript : MonoBehaviour
{
    public GameObject player;
    public PlayerUIScript playerUI;
    public GameObject lift;
    public DialoguePointScript dialog;

    public bool isLevelExit = false;

    private CameraController cam;
    private PlayerController playerController;
    private GameObject lever;
    private LiftController liftController;

    public int leverNumber;

    public bool showDialogText = false;
    public string DialogText;

    private bool playerNear = false;
    private float leverTimer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        playerController = player.GetComponent<PlayerController>();
        lever = gameObject;
        cam = Camera.main.GetComponent<CameraController>();
        liftController = lift.GetComponent<LiftController>();
    }

    
    // Update is called once per frame
    void Update()
    {
        if (leverTimer > 0)
        {
            leverTimer -= Time.deltaTime;
        }

        if (playerNear)
        {
            if ((Input.GetAxis("Vertical") == 1) && leverTimer <= 0)
            {
                StartCoroutine(ActivateLever());
            }
        }
    }

    IEnumerator ActivateLever()
    {
        if (showDialogText)
        {
            playerUI.ShowDialogText(DialogText);
        }
        leverTimer = 4f;
        Flip();
        cam.Look(lift, 4f, 1f);
        liftController.ActivatePosition(leverNumber);
        yield return new WaitForSeconds(3.5f);
        Flip();

        if (isLevelExit)
        {
            if (playerUI != null)
                playerUI.CompleteLevel();
        }
    }

    void Flip()
    {
        Vector3 Scaler = transform.localScale;
        Scaler.x *= -1;
        transform.localScale = Scaler;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerNear = true;
            dialog.ShowText("Press (Up) to activate lever.", 2f);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerNear = false;
        }
    }

}
