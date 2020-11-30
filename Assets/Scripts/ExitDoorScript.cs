using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitDoorScript : MonoBehaviour
{
    public GameObject player;
    public PlayerUIScript playerUI;
    private PlayerController playerController;
    public DialoguePointScript dialog;

    private GameObject exitDoor;

    private bool hasKey;
    private bool playerNear = false;
    private bool exiting = false;

    private float upTimeValue = 0.4f;
    private float upTime = 0f;

    // Start is called before the first frame update
    void Start()
    {
        playerController = player.GetComponent<PlayerController>();
        exitDoor = gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerNear && hasKey && !exiting)
        {
            if (upTime > upTimeValue)
            {
                Debug.Log("Door opens!");
                exiting = true;
                StartCoroutine(OpenDoor());
            }

            if ((Input.GetAxis("Vertical") == 1))
            {
                upTime += Time.deltaTime;
            }
            else
            {
                upTime = 0;
            }

        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            hasKey = playerController.hasKey;
            playerNear = true;

            if (hasKey)
            {
                //playerUI.ShowDialogText("Press (Up) to Exit.");
                dialog.ShowText("Hold (Up) to Exit.", 2f);
            }
            else
            {
                //playerUI.ShowDialogText("The door is locked.");
                dialog.ShowText("The door is locked.", 2f);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerNear = false;
        }
    }

    IEnumerator OpenDoor()
    {
        // freeze player and start door open animation
        playerController.FreezePlayer(8f);
        exitDoor.GetComponent<Animator>().SetBool("Open", true);

        // move to center of door and wait for animation
        for (float i = 3f; i >= 0; i -= Time.deltaTime)
        {
            Vector2 position = player.transform.position;
            position.x = Mathf.MoveTowards(position.x, exitDoor.transform.position.x, Time.deltaTime);
            player.transform.position = position;
            yield return null;
        }

        // initiate scene fade out
        playerUI.CompleteLevel();

        // fade out player
        float fadeTime = 2;
        for (float i = fadeTime; i >= 0; i -= Time.deltaTime)
        {
            float a = (i / fadeTime);
            playerController.SetAlpha(i / fadeTime);
            yield return null;
        }
    }
}
