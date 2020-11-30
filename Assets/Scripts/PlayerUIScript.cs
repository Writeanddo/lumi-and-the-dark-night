using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PlayerUIScript : MonoBehaviour
{

    public string levelName;
    public int orbsToCollect = 99;

    private int startingOrbs;
    private int currentOrbs;

    public Sprite healthEmpty;
    public Sprite healthFull;

    public Sprite unlimitedJumpIcon;
    public Sprite doubleJumpIcon;
    public Sprite jumpIcon;
    public Sprite dashIcon;
    public Sprite fieldIcon;
    public Sprite blasticon;
    public Sprite shiftIcon;

    private GameObject[] healthBlips = new GameObject[5];
    private GameObject[] icons = new GameObject[5];

    public GameObject levelText;
    public GameObject healthBar;
    public GameObject orbCount;
    public GameObject abilityIcons;
    public GameObject dialog;
    public GameObject sceneFader;
    public GameObject levelTime;
    public GameObject gameOver;

    private Text dialogText;
    private CanvasGroup dialogGroup;
    private float dialogFadeTime = 1.0f;
    private float dialogDefaultDuration = 3f;
    private float dialogDuration = 3f;
    private float dialogBusyTimer = 0f;

    private bool sceneFadeIn = false;
    private bool sceneFadeOut = false;
    private float sceneFadeDuration = 3f;

    PlayerController playerController;
    EventSystem eventSystem;
    public GameObject pausePanel;
    public Animator pauseAnimator;
    public Button resumeButton;
    public Button restartButton;
    public Button musicButton;
    public Button quitButton;
    private GameObject selectedButton;
    private bool isPaused = false;
    private bool isGameOver = false;

    public GameObject fpsDisplay;
    private float fpsTimer = 0f;
    private bool showFPS = false;

    public GameObject buttonDisplay;
    private bool showButtons = false;

    private float levelPlayTime = 0;

    // Start is called before the first frame update
    void Awake()
    {
        healthBlips[0] = GameObject.Find("HealthBar/Health1");
        healthBlips[1] = GameObject.Find("HealthBar/Health2");
        healthBlips[2] = GameObject.Find("HealthBar/Health3");
        healthBlips[3] = GameObject.Find("HealthBar/Health4");
        healthBlips[4] = GameObject.Find("HealthBar/Health5");

        icons[0] = GameObject.Find("AbilityIcons/Icon1");
        icons[1] = GameObject.Find("AbilityIcons/Icon2");
        icons[2] = GameObject.Find("AbilityIcons/Icon3");
        icons[3] = GameObject.Find("AbilityIcons/Icon4");
        icons[4] = GameObject.Find("AbilityIcons/Icon5");

        dialogText = dialog.GetComponent<Text>();
        dialogGroup = dialog.GetComponent<CanvasGroup>();
        dialog.SetActive(false);

        startingOrbs = GameData.orbCount;

        eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();

        playerController = GameObject.Find("Player").GetComponent<PlayerController>();

        SceneFadeIn();
    }

    void Start()
    {
        if (Camera.main.GetComponent<AudioSource>().playOnAwake)
        {
            Camera.main.GetComponent<FadeAudioSource>().StartMusic(GameData.musicVolume, 4f);
        }
        UpdateMusicSettings();

        //PlatformTester();
    }

    private void PlatformTester()
    {
        ShowDialogText(SystemInfo.operatingSystemFamily.ToString());
    }

    private void Update()
    {
        Cursor.visible = !Screen.fullScreen;

        // InputTester();

        dialogBusyTimer -= Time.deltaTime;

        if (dialogBusyTimer > 0)
        {
            if (dialogBusyTimer > (dialogDuration + dialogFadeTime))
            {
                // fade in
                dialogGroup.alpha = (dialogDuration + 2*dialogFadeTime - dialogBusyTimer) / dialogFadeTime;
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

        SelectUpdate();
        bool selectButtonPressed = Input.GetKeyDown(KeyCode.JoystickButton9);
#if (UNITY_WEBGL && !UNITY_EDITOR)
            if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX)
            {
                selectButtonPressed = Input.GetKeyDown(KeyCode.JoystickButton7);
            }
            if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.Windows)
            {
                selectButtonPressed = Input.GetKeyDown(KeyCode.JoystickButton7);
            }
#endif
        if (Input.GetKeyDown(KeyCode.Escape) ||
            selectButtonPressed)
        {
            if (isGameOver)
            {
                CompleteLevel();
            }
            else if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
        else if (isPaused)
        {
            // allow alternate joystick buttons to activate selected button
            if (Input.GetKeyDown(KeyCode.JoystickButton1) ||
                Input.GetKeyDown(KeyCode.JoystickButton2) ||
                Input.GetKeyDown(KeyCode.JoystickButton3))
            {
                selectedButton.GetComponent<Button>().onClick.Invoke();
            }
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            showFPS = showFPS ? false : true; // toggle
            fpsDisplay.SetActive(showFPS);
            Debug.Log("FPS = " + showFPS);
        }

        if (showFPS)
        {
            fpsTimer += Time.unscaledDeltaTime;
            if (fpsTimer >= 1f)
            {
                fpsTimer -= 1f;
                float fps = 1 / Time.unscaledDeltaTime;
                fpsDisplay.GetComponent<Text>().text = "FPS: " + fps.ToString("F0");
            }
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            showButtons = showButtons ? false : true;  //toggle
            buttonDisplay.SetActive(buttonDisplay);
            Debug.Log("Button display = " + showButtons);
        }

        if (showButtons)
        {
            buttonDisplay.GetComponent<Text>().text = ButtonTester();
        }

        if (!isPaused && !isGameOver)
        {
            levelPlayTime += Time.deltaTime;
        }
        levelTime.GetComponent<Text>().text = string.Format("Level Time: {0:D} s", (int)levelPlayTime);
    }

    private void InputTester()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ShowDialogText("Player hit the <color=\"orange\">One Key</color>");
        }
        else if (Input.GetKeyDown(KeyCode.JoystickButton4))
        {
            ShowDialogText("Player hit the <color=\"orange\">JoystickButton4</color>");
        }
        else if (Input.GetKeyDown(KeyCode.JoystickButton5))
        {
            ShowDialogText("Player hit the <color=\"orange\">JoystickButton5</color>");
        }
        else if (Input.GetKeyDown(KeyCode.JoystickButton6))
        {
            ShowDialogText("Player hit the <color=\"orange\">JoystickButton6</color>");
        }
        else if (Input.GetKeyDown(KeyCode.JoystickButton7))
        {
            ShowDialogText("Player hit the <color=\"orange\">JoystickButton7</color>");
        }
        else if (Input.GetKeyDown(KeyCode.JoystickButton8))
        {
            ShowDialogText("Player hit the <color=\"orange\">JoystickButton8</color>");
        }
        else if (Input.GetKeyDown(KeyCode.JoystickButton9))
        {
            ShowDialogText("Player hit the <color=\"orange\">JoystickButton9</color>");
        }
        else if (Input.GetKeyDown(KeyCode.JoystickButton10))
        {
            ShowDialogText("Player hit the <color=\"orange\">JoystickButton10</color>");
        }
        else if (Input.GetKeyDown(KeyCode.JoystickButton11))
        {
            ShowDialogText("Player hit the <color=\"orange\">JoystickButton11</color>");
        }
        else if (Input.GetKeyDown(KeyCode.JoystickButton12))
        {
            ShowDialogText("Player hit the <color=\"orange\">JoystickButton12</color>");
        }
        else if (Input.GetKeyDown(KeyCode.JoystickButton13))
        {
            ShowDialogText("Player hit the <color=\"orange\">JoystickButton13</color>");
        }
    }

    private string ButtonTester()
    {
        string s = "B0:" + (Input.GetKey(KeyCode.JoystickButton0) ? "<color=red><b>1</b></color>" : "0") +
            ", B1:" + (Input.GetKey(KeyCode.JoystickButton1) ? "<color=red><b>1</b></color>" : "0") +
            ". B2:" + (Input.GetKey(KeyCode.JoystickButton2) ? "<color=red><b>1</b></color>" : "0") +
            ", B3:" + (Input.GetKey(KeyCode.JoystickButton3) ? "<color=red><b>1</b></color>" : "0") +
            ", B4:" + (Input.GetKey(KeyCode.JoystickButton4) ? "<color=red><b>1</b></color>" : "0") +
            ", B5:" + (Input.GetKey(KeyCode.JoystickButton5) ? "<color=red><b>1</b></color>" : "0") +
            ", B6:" + (Input.GetKey(KeyCode.JoystickButton6) ? "<color=red><b>1</b></color>" : "0") +
            ", B7:" + (Input.GetKey(KeyCode.JoystickButton7) ? "<color=red><b>1</b></color>" : "0") +
            ", B8:" + (Input.GetKey(KeyCode.JoystickButton8) ? "<color=red><b>1</b></color>" : "0") +
            ", B9:" + (Input.GetKey(KeyCode.JoystickButton9) ? "<color=red><b>1</b></color>" : "0") +
            ", B10:" + (Input.GetKey(KeyCode.JoystickButton10) ? "<color=red><b>1</b></color>" : "0") +
            ", B11:" + (Input.GetKey(KeyCode.JoystickButton11) ? "<color=red><b>1</b></color>" : "0") +
            ", B12:" + (Input.GetKey(KeyCode.JoystickButton12) ? "<color=red><b>1</b></color>" : "0") +
            ", B13:" + (Input.GetKey(KeyCode.JoystickButton13) ? "<color=red><b>1</b></color>" : "0") +
            ", B14:" + (Input.GetKey(KeyCode.JoystickButton14) ? "<color=red><b>1</b></color>" : "0") +
            ", B15:" + (Input.GetKey(KeyCode.JoystickButton15) ? "<color=red><b>1</b></color>" : "0") +
            ", B16:" + (Input.GetKey(KeyCode.JoystickButton16) ? "<color=red><b>1</b></color>" : "0") +
            ", B17:" + (Input.GetKey(KeyCode.JoystickButton17) ? "<color=red><b>1</b></color>" : "0");

        return s;
    }

    private void FixedUpdate()
    {
        SceneFadeUpdate();
    }

    public void GameOver()
    {
        isGameOver = true;
        gameOver.SetActive(true);
    }

    public void CompleteLevel()
    {
        AchievementCheck();
        SceneFadeOut();
        GameData.respawned = false;
    }

    private void SceneFadeIn()
    {
        sceneFadeIn = true;
        sceneFader.SetActive(true);
    }

    private void SceneFadeOut()
    {
        sceneFadeOut = true;
        sceneFader.SetActive(true);
        Camera.main.GetComponent<FadeAudioSource>().StopMusic(sceneFadeDuration);
    }

    private void SceneFadeUpdate()
    {
        if (sceneFadeIn)
        {
            Image image = sceneFader.GetComponent<Image>();
            Color color = image.color;
            float alphaDelta = (Time.fixedDeltaTime/sceneFadeDuration);
            color.a -= alphaDelta;
            image.color = color;
            if (color.a <= 0)
            {
                sceneFadeIn = false;
                sceneFader.SetActive(false);
            }
        }
        else if (sceneFadeOut)
        {
            Image image = sceneFader.GetComponent<Image>();
            Color color = image.color;
            color.a += (Time.fixedDeltaTime/sceneFadeDuration);
            image.color = color;
            if (color.a >= 1)
            {
                sceneFadeOut = false;
                if (SceneManager.GetActiveScene().buildIndex == SceneManager.sceneCountInBuildSettings - 1)
                {
                    // That was the last scene so quit to main menu
                    QuitGame();
                }
                else
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                }
            }
        }
    }

    void SelectUpdate()
    {
        if (eventSystem.currentSelectedGameObject == null)
        {
            eventSystem.SetSelectedGameObject(selectedButton);
        }
        else
        {
            selectedButton = eventSystem.currentSelectedGameObject;
        }
    }

    private void Pause()
    {
        isPaused = true;
        ShowAbilityHelp(true);
        playerController.FreezePlayer(Mathf.Infinity);
        pauseAnimator.SetBool("Visible", true);
        Time.timeScale = 0;
    }

    private void Unpause()
    {
        isPaused = false;
        ShowAbilityHelp(false);
        playerController.FreezePlayer(0.3f);
        pauseAnimator.SetBool("Visible", false);
        Time.timeScale = 1;
    }

    public void ResumeGame()
    {
        Debug.Log("RESUMING");
        Unpause();
    }

    public void RestartPressed()
    {
        Debug.Log("RESTART");
        Unpause();
        GameData.respawned = false;
        RestartScene();
    }

    public void PauseGame()
    {
        Debug.Log("PAUSED");
        resumeButton.Select();
        Pause();
    }

    public void MusicPressed()
    {
        musicButton.Select();
        GameData.musicMuted = !GameData.musicMuted;
        UpdateMusicSettings();
    }

    private void UpdateMusicSettings()
    {
        Camera.main.GetComponent<AudioSource>().mute = GameData.musicMuted;
        string text = GameData.musicMuted ? "Unmute Music" : "Mute Music";
        musicButton.transform.GetChild(0).GetComponent<Text>().text = text;
    }

    public void QuitGame()
    {
        Debug.Log("QUIT");
        Unpause();
        GameData.orbCount = 0;
        GameData.respawned = false;
        SceneManager.LoadScene(0);
    }

    public void RestartScene()
    {
        GameData.orbCount = startingOrbs;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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

    public void SetUIActive( bool isActive)
    {
        ShowLevelText(isActive);
        ShowHealthBar(isActive);
        ShowOrbCount(isActive);
        ShowAbilityIcons(isActive);
    }

    public void ShowLevelText( bool isActive)
    {
        CanvasGroup cg = levelText.GetComponent<CanvasGroup>();
        cg.alpha = isActive ? 1f : 0f;
    }

    public void ShowHealthBar(bool isActive)
    {
        CanvasGroup cg = healthBar.GetComponent<CanvasGroup>();
        cg.alpha = isActive ? 1f : 0f;
    }

    public void ShowOrbCount(bool isActive)
    {
        CanvasGroup cg = orbCount.GetComponent<CanvasGroup>();
        cg.alpha = isActive ? 1f : 0f;
    }

    public void ShowAbilityIcons(bool isActive)
    {
        CanvasGroup cg = abilityIcons.GetComponent<CanvasGroup>();
        cg.alpha = isActive ? 1f : 0f;
    }

    public void ShowAbilityHelp(bool isActive)
    {
        for (int i=0; i<icons.Length; i ++)
        {
            icons[i].transform.GetChild(0).gameObject.SetActive(isActive);
        }
    }

    public void SetJumpIcon(bool canJump, bool canDoubleJump, bool canUnlimitedJump)
    {
        Image img = icons[0].GetComponent<Image>();

        if (canUnlimitedJump)
        {
            icons[0].SetActive(true);
            img.sprite = unlimitedJumpIcon;
        }
        else if (canDoubleJump)
        {
            icons[0].SetActive(true);
            img.sprite = doubleJumpIcon;
        }
        else if (canJump)
        {
            icons[0].SetActive(true);
            img.sprite = jumpIcon;
        }
        else
        {
            icons[0].SetActive(false);
        }
    }

    public void SetDashIcon(bool canDash)
    {
        icons[1].SetActive(canDash);
    }

    public void SetFieldIcon(bool canField)
    {
        icons[2].SetActive(canField);
    }

    public void SetBlastIcon(bool canBlast)
    {
        icons[3].SetActive(canBlast);
    }

    public void SetShiftIcon(bool canShift)
    {
        icons[4].SetActive(canShift);
    }

    public void SetPlayerHealth(int currentHealth, int maxhealth)
    {
        currentHealth = Mathf.Min(currentHealth, 5);
        maxhealth = Mathf.Min(maxhealth, 5);

        for (int i=0; i<5; i++)
        {
            Image img = healthBlips[i].GetComponent<Image>();

            if ( currentHealth > i)
            {
                healthBlips[i].SetActive(true);
                img.sprite = healthFull;
            }
            else if (maxhealth > i)
            {
                healthBlips[i].SetActive(true);
                img.sprite = healthEmpty;
            }
            else
            {
                healthBlips[i].SetActive(false);
            }
        }

    }

    public void UpdateOrbCount()
    {
        currentOrbs = GameData.orbCount;
        Text orbCountText = orbCount.GetComponentInChildren<Text>();
        orbCountText.text = currentOrbs.ToString();
        SetLevelText();
    }

    public void SetLevelText()
    {
        float percentComplete = (float)100.0 * (currentOrbs - startingOrbs) / orbsToCollect;
        Text levelNameText = levelText.GetComponentInChildren<Text>();
        levelNameText.text = levelName + " (" + percentComplete.ToString("F0") + "%)";
    }

    private void AchievementCheck()
    {
        Achievements achieve = new Achievements(GameData.achieveStatus);
        bool allOrbsCollected = ((currentOrbs - startingOrbs) == orbsToCollect);
        bool noRespawns = !GameData.respawned;
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "Level1")
        {
            if (allOrbsCollected && !achieve.getStatus(Achievements.Type.L1_ORBS))
            {
                Debug.Log("Achieved " + achieve.getName(Achievements.Type.L1_ORBS));
                achieve.setStatus(Achievements.Type.L1_ORBS);
            }
            if (noRespawns && !achieve.getStatus(Achievements.Type.L1_DEATH))
            {
                Debug.Log("Achieved " + achieve.getName(Achievements.Type.L1_DEATH));
                achieve.setStatus(Achievements.Type.L1_DEATH);
            }
        }
        else if (sceneName == "Level2")
        {
            if (allOrbsCollected && !achieve.getStatus(Achievements.Type.L2_ORBS))
            {
                Debug.Log("Achieved " + achieve.getName(Achievements.Type.L2_ORBS));
                achieve.setStatus(Achievements.Type.L2_ORBS);
            }
            if (noRespawns && !achieve.getStatus(Achievements.Type.L2_DEATH))
            {
                Debug.Log("Achieved " + achieve.getName(Achievements.Type.L2_DEATH));
                achieve.setStatus(Achievements.Type.L2_DEATH);
            }
        }
        else if (sceneName == "Level3")
        {
            if (allOrbsCollected && !achieve.getStatus(Achievements.Type.L3_ORBS))
            {
                Debug.Log("Achieved " + achieve.getName(Achievements.Type.L3_ORBS));
                achieve.setStatus(Achievements.Type.L3_ORBS);
            }
            if (noRespawns && !achieve.getStatus(Achievements.Type.L3_DEATH))
            {
                Debug.Log("Achieved " + achieve.getName(Achievements.Type.L3_DEATH));
                achieve.setStatus(Achievements.Type.L3_DEATH);
            }
        }
        else if (sceneName == "Level4")
        {
            if (allOrbsCollected && !achieve.getStatus(Achievements.Type.L4_ORBS))
            {
                Debug.Log("Achieved " + achieve.getName(Achievements.Type.L4_ORBS));
                achieve.setStatus(Achievements.Type.L4_ORBS);
            }
            if (noRespawns && !achieve.getStatus(Achievements.Type.L4_DEATH))
            {
                Debug.Log("Achieved " + achieve.getName(Achievements.Type.L4_DEATH));
                achieve.setStatus(Achievements.Type.L4_DEATH);
            }
        }
        else if (sceneName == "Level5")
        {
            if (allOrbsCollected && !achieve.getStatus(Achievements.Type.L5_ORBS))
            {
                Debug.Log("Achieved " + achieve.getName(Achievements.Type.L5_ORBS));
                achieve.setStatus(Achievements.Type.L5_ORBS);
            }
            if (noRespawns && !achieve.getStatus(Achievements.Type.L5_DEATH))
            {
                Debug.Log("Achieved " + achieve.getName(Achievements.Type.L5_DEATH));
                achieve.setStatus(Achievements.Type.L5_DEATH);
            }
        }
        else if (sceneName == "Level6")
        {
            if (allOrbsCollected && !achieve.getStatus(Achievements.Type.L6_ORBS))
            {
                Debug.Log("Achieved " + achieve.getName(Achievements.Type.L6_ORBS));
                achieve.setStatus(Achievements.Type.L6_ORBS);
            }
            if (noRespawns && !achieve.getStatus(Achievements.Type.L6_DEATH))
            {
                Debug.Log("Achieved " + achieve.getName(Achievements.Type.L6_DEATH));
                achieve.setStatus(Achievements.Type.L6_DEATH);
            }
        }
    }
}
