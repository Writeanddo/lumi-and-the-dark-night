using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class TitleScreen : MonoBehaviour
{
    public GameObject player;
    public GameObject slopePrefab;
    public GameObject mountPrefab;
    public GameObject clearingPrefab;
    public GameObject background;
    public GameObject sceneFader;
    public MoonCredits moonCredits;

    public GameObject dialog;
    private Text dialogText;
    private CanvasGroup dialogGroup;
    private float dialogFadeTime = 1.0f;
    private float dialogDefaultDuration = 3f;
    private float dialogDuration = 3f;
    private float dialogBusyTimer = 0f;


    public SpriteRenderer dimmer;
    public GameObject darkEnergyPrefab;

    public Button startButton;
    public Button muteButton;
    public Button unmuteButton;
    public Button creditsButton;
    public Button quitButton;

    public AudioSource audioSource;
    public AudioClip buttonSound;
    public AudioClip thunderSound;
    public AudioClip ominousMusic;

    float slopeInitX = -12f;
    float slopeInitY = -7.7f;
    float slopeOverlap = 0.05f;
    float slopeWidth;
    GameObject[] slopes;

    float mountInitX = 0f;
    float mountInitY = 0f;
    float mountOverlap = 0.02f;
    float mountWidth;
    GameObject[] mounts;

    GameObject clearing;
    GameObject clearingGround;
    GameObject pit;
    GameObject groundfall;
    GameObject rift;
    GameObject darkness;
    GameObject darkness2;
    GameObject critter1;
    GameObject critter2;
    GameObject critter3;
    GameObject critter4;

    EventSystem eventSystem;
    GameObject selectedButton;
    bool altSelect = false;
    bool starting = false;

    void Awake()
    {
        eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
        SetSelectedButton(startButton);
        SlopeInit();
        MountInit();

        dialogText = dialog.GetComponent<Text>();
        dialogGroup = dialog.GetComponent<CanvasGroup>();
        dialog.SetActive(false);

        #if UNITY_WEBGL
        quitButton.gameObject.SetActive(false);
        #endif
    }

    void Start()
    {
        PlayerController playerController = player.GetComponent<PlayerController>();
        playerController.FreezePlayer(Mathf.Infinity);
        SetupMusic();
    }

    void Update()
    {
        Cursor.visible = !Screen.fullScreen;

        if (Input.GetKeyDown(KeyCode.JoystickButton1) ||
            Input.GetKeyDown(KeyCode.JoystickButton2) ||
            Input.GetKeyDown(KeyCode.JoystickButton3))
        {
            altSelect = true;
        }

        dialogBusyTimer -= Time.deltaTime;

        if (dialogBusyTimer > 0)
        {
            if (dialogBusyTimer > (dialogDuration + dialogFadeTime))
            {
                // fade in
                dialogGroup.alpha = (dialogDuration + 2 * dialogFadeTime - dialogBusyTimer) / dialogFadeTime;
            }
            else if (dialogBusyTimer > dialogFadeTime)
            {
                // hold time
                dialogGroup.alpha = 1f;
            }
            else
            {
                // fade out
                dialogGroup.alpha = (dialogBusyTimer / dialogFadeTime);
            }
        }
        else
        {
            dialog.SetActive(false);
        }

    }

    void FixedUpdate()
    {
        SlopeUpdate();
        MountUpdate();
        OtherUpdate();
        if (altSelect)
        {
            if (!starting)
            {
                selectedButton.GetComponent<Button>().onClick.Invoke();
            }
            altSelect = false;
        }
    }

    void SlopeInit()
    {
        slopes = new GameObject[3];
        Vector3 position = new Vector3(slopeInitX, slopeInitY, 0);
        slopes[0] = Instantiate(slopePrefab, position, Quaternion.identity);
        slopeWidth = slopes[0].GetComponent<SpriteRenderer>().bounds.size.x;
        position.x += slopeWidth - slopeOverlap;
        slopes[1] = Instantiate(slopePrefab, position, Quaternion.identity);
        position.x += slopeWidth - slopeOverlap;
        slopes[2] = Instantiate(slopePrefab, position, Quaternion.identity);
    }

    void SlopeUpdate()
    {
        float screenLeftEdge = Camera.main.ViewportToWorldPoint(Vector2.zero).x;
        float screenRightEdge = Camera.main.ViewportToWorldPoint(Vector2.one).x;
        for (int i=0; i<slopes.Length; i++)
        {
            GameObject obj = slopes[i];
            if (obj != null)
            {
                Bounds b = obj.GetComponent<SpriteRenderer>().bounds;
                Vector3 position = obj.transform.position;
                bool createClearing = false;
                if (b.max.x < screenLeftEdge)
                {
                    position.x += 3 * (slopeWidth - slopeOverlap);
                    if (starting)
                    {
                        Destroy(obj);
                        createClearing = true;
                    }
                    else
                    {
                        obj.transform.position = position;
                    }
                }
                else if (b.min.x > screenRightEdge)
                {
                    if (starting)
                    {
                        Destroy(obj);
                        createClearing = true;
                    }
                }
                if (createClearing && (clearing == null))
                {
                    clearing = Instantiate(clearingPrefab, position, Quaternion.identity);
                    clearingGround = clearing.transform.GetChild(0).gameObject;
                    pit = clearing.transform.GetChild(1).gameObject;
                    groundfall = clearing.transform.GetChild(2).gameObject;
                    rift = clearing.transform.GetChild(3).gameObject;
                    darkness = clearing.transform.GetChild(4).gameObject;
                    darkness2 = clearing.transform.GetChild(5).gameObject;
                    critter1 = clearing.transform.GetChild(6).gameObject;
                    critter2 = clearing.transform.GetChild(7).gameObject;
                    critter3 = clearing.transform.GetChild(8).gameObject;
                    critter4 = clearing.transform.GetChild(9).gameObject;
                }
            }
        }
    }

    void MountInit()
    {
        mounts = new GameObject[2];
        Vector3 position = new Vector3(mountInitX, mountInitY, 0);
        mounts[0] = Instantiate(mountPrefab, background.transform);
        mounts[0].transform.localPosition = position;
        mountWidth = mounts[0].GetComponent<SpriteRenderer>().bounds.size.x;
        position.x += mountWidth - mountOverlap;
        mounts[1] = Instantiate(mountPrefab, background.transform);
        mounts[1].transform.localPosition = position;
    }

    void MountUpdate()
    {
        float screenLeftEdge = Camera.main.ViewportToWorldPoint(Vector2.zero).x;
        for (int i=0; i<mounts.Length; i++)
        {
            GameObject obj = mounts[i];
            Bounds b = obj.GetComponent<SpriteRenderer>().bounds;
            if (b.max.x < screenLeftEdge)
            {
                Vector3 position = obj.transform.position;
                position.x += 2 * (mountWidth - mountOverlap);
                obj.transform.position = position;
            }
        }
    }

    void OtherUpdate()
    {
        // track selected button and restore when needed
        if (eventSystem.currentSelectedGameObject == null)
        {
            eventSystem.SetSelectedGameObject(selectedButton);
        }
        else
        {
            selectedButton = eventSystem.currentSelectedGameObject;
        }

        // fade title and buttons
        if (starting)
        {
            CanvasGroup cg = GetComponent<CanvasGroup>();
            if (cg.alpha > 0)
            {
                float alphaDelta = (Time.fixedDeltaTime/2f);
                cg.alpha -= alphaDelta;
            }
        }
    }

    void SetSelectedButton(Button button)
    {
        if (button != null)
        {
            button.Select();
            selectedButton = button.gameObject;
        }
    }

    void PlaySound(AudioClip clip, float volume = 1.0f)
    {
        if (audioSource != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }

    void DisableButtons()
    {
        startButton.interactable = false;
        muteButton.interactable = false;
        unmuteButton.interactable = false;
        creditsButton.interactable = false;
        quitButton.interactable = false;
    }

    public void StartGame()
    {
        Debug.Log("StartGame");
        PlaySound(buttonSound);
        StartCoroutine(StartGameCoroutine());
    }

    IEnumerator StartGameCoroutine()
    {
        DisableButtons();
        moonCredits.StopCredits();
        starting = true;

        // Fade music audio source
        Camera.main.GetComponent<FadeAudioSource>().StopMusic(5f);

        // wait for camera to be centered over pit
        Camera cam = Camera.main;
        CameraController camController = cam.GetComponent<CameraController>();
        while ((pit == null) || (cam.transform.position.x < pit.transform.position.x))
        {
            yield return null;
        }
        camController.Follow(pit);

        // wait for player to be centered over pit
        while (player.transform.position.x < pit.transform.position.x)
        {
            yield return null;
        }
        player.GetComponent<PlayerController>().autoRun = false;

        // show rift
        rift.GetComponent<SpriteRenderer>().enabled = true;
        rift.GetComponent<Animator>().SetBool("Visible", true);
        PlaySound(thunderSound);
        yield return new WaitForSeconds(3f);

        ShowDialogText("Something ancient and terrible spilled out of the Rift.", 5f);

        // emit darkness
        PlaySound(ominousMusic);
        ParticleSystem.EmissionModule em = darkness.GetComponent<ParticleSystem>().emission;
        em.enabled = true;
        ParticleSystem.EmissionModule em2 = darkness2.GetComponent<ParticleSystem>().emission;
        em2.enabled = true;

        // emit dark energy toward critter 1
        GameObject energy = Instantiate(darkEnergyPrefab, rift.transform.position, rift.transform.rotation);
        energy.GetComponent<HomingScript>().SetTargetObject(critter1, critter1.GetComponent<TransformCritter>().offset.x, critter1.GetComponent<TransformCritter>().offset.y);
        energy.GetComponent<HomingScript>().SetMinimumDistance(0.5f);
        energy.GetComponent<HomingScript>().SetSpeedAndAngle(10f, 45f);

        // emit dark energy toward critter 2
        energy = Instantiate(darkEnergyPrefab, rift.transform.position, rift.transform.rotation);
        energy.GetComponent<HomingScript>().SetTargetObject(critter2, critter2.GetComponent<TransformCritter>().offset.x, critter2.GetComponent<TransformCritter>().offset.y);
        energy.GetComponent<HomingScript>().SetMinimumDistance(0.5f);
        energy.GetComponent<HomingScript>().SetSpeedAndAngle(10f, -45f);

        // emit dark energy toward critter 3
        energy = Instantiate(darkEnergyPrefab, rift.transform.position, rift.transform.rotation);
        energy.GetComponent<HomingScript>().SetTargetObject(critter3, critter3.GetComponent<TransformCritter>().offset.x, critter3.GetComponent<TransformCritter>().offset.y);
        energy.GetComponent<HomingScript>().SetMinimumDistance(0.5f);
        energy.GetComponent<HomingScript>().SetSpeedAndAngle(10f, -15f);

        // emit dark energy toward critter 4
        energy = Instantiate(darkEnergyPrefab, rift.transform.position, rift.transform.rotation);
        energy.GetComponent<HomingScript>().SetTargetObject(critter4, critter4.GetComponent<TransformCritter>().offset.x, critter4.GetComponent<TransformCritter>().offset.y);
        energy.GetComponent<HomingScript>().SetMinimumDistance(0.5f);
        energy.GetComponent<HomingScript>().SetSpeedAndAngle(10f, 15f);

        // emit dark energy toward player
        energy = Instantiate(darkEnergyPrefab, rift.transform.position, rift.transform.rotation);
        energy.GetComponent<HomingScript>().SetTargetObject(player, 0f, 0f);
        energy.GetComponent<HomingScript>().SetMinimumDistance(0.5f);
        energy.GetComponent<HomingScript>().SetSpeedAndAngle(10f, 0f);
        Physics2D.IgnoreCollision(player.GetComponent<Collider2D>(), energy.GetComponent<Collider2D>());  // avoid knockback damage effect

        yield return new WaitForSeconds(2f);
        player.GetComponent<PlayerController>().lifeForce = 0;
        player.transform.Find("ParticleTrail").gameObject.SetActive(false);

        float dimmerTime = 5f;
        for (float i=0; i<dimmerTime; i += Time.deltaTime)
        {
            Color dimmerColor = dimmer.color;
            dimmerColor.a = (0.95f * i / dimmerTime);
            dimmer.color = dimmerColor;
            yield return null;
        }
        //yield return new WaitForSeconds(7f);


        // initiate ground shake
        camController.Shake(3f);
        yield return new WaitForSeconds(1f);

        // show pit and groundfall
        pit.GetComponent<SpriteRenderer>().enabled = true;
        em = groundfall.GetComponent<ParticleSystem>().emission;
        em.enabled = true;
        yield return new WaitForSeconds(2f);

        // remove groundfall and let Lumi fall
        em.enabled = false;
        clearingGround.SetActive(false);
        GameObject.Find("Sky").transform.parent = null;
        camController.lockY = false;
        yield return new WaitForSeconds(4f);

        // fade scene
        sceneFader.SetActive(true);
        float fadeTime = 2;
        for (float i=0; i<fadeTime; i += Time.deltaTime)
        {
            Image image = sceneFader.GetComponent<Image>();
            Color color = image.color;
            color.a = (i/fadeTime);
            image.color = color;
            yield return null;
        }

        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        Debug.Log("QuitGame");
        PlaySound(buttonSound);
        Application.Quit();
    }

    private void SetupMusic()
    {
        Camera.main.GetComponent<AudioSource>().volume = GameData.musicVolume;
        Camera.main.GetComponent<AudioSource>().mute = GameData.musicMuted;
        muteButton.gameObject.SetActive(!GameData.musicMuted);
        unmuteButton.gameObject.SetActive(GameData.musicMuted);
    }

    public void MuteMusic()
    {
        PlaySound(buttonSound);
        GameData.musicMuted = true;
        Camera.main.GetComponent<AudioSource>().mute = true;
        muteButton.gameObject.SetActive(false);
        unmuteButton.gameObject.SetActive(true);
        unmuteButton.Select();
    }

    public void UnmuteMusic()
    {
        PlaySound(buttonSound);
        GameData.musicMuted = true;
        Camera.main.GetComponent<AudioSource>().mute = false;
        unmuteButton.gameObject.SetActive(false);
        muteButton.gameObject.SetActive(true);
        muteButton.Select();
    }

    public void ShowCredits()
    {
        Debug.Log("ShowCredits");
        PlaySound(buttonSound);
        moonCredits.PlayCredits();
    }


    public void ShowDialogText(string s, float duration)
    {
        if (s != "")
        {
            if (dialogBusyTimer > 0)
            {
                s = dialogText.text + "\n" + s;
            }

            dialogDuration = duration;
            dialogBusyTimer = duration + 2 * dialogFadeTime;
            dialogText.text = s;
            dialog.SetActive(true);
            dialogGroup.alpha = 0f;
        }
    }

    public void ShowDialogText(string s)
    {
        // use default duration
        ShowDialogText(s, dialogDefaultDuration);
    }
}
