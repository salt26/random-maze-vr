using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainController : MonoBehaviour
{
    public static MainController mc;

    public int MazeColumns
    {
        get;
        private set;
    }

    public int MazeRows
    {
        get;
        private set;
    }

    public float InitialTime
    {
        get;
        private set;
    }

    public int SceneIndex
    {
        get;
        private set;
    }

    public enum Theme { None, Sunset, Desert, Illusion };
    public enum Level { None, Easy, Normal, Hard };

    public bool isBGMOff = false;
    public bool isSoundOff = false;
    public GameObject bgm;
    public Button bgmButton;
    public Button startButton;
    public Button themeSunsetButton;
    public Button themeDesertButton;
    public Button themeIllusionButton;
    public Button levelEasyButton;
    public Button levelNormalButton;
    public Button levelHardButton;
    public TMP_Text sizeText;
    public Image backgroundImage;
    public Sprite sunsetSprite;
    public Sprite desertSprite;
    public Sprite illusionSprite;
    public Theme theme = Theme.None;
    public Level level = Level.None;

    private bool _hasGameStart = false;
    private AudioSource bgmAudioSource;
    private TMP_Text bgmText;
    private TMP_Text themeSunsetText;
    private TMP_Text themeDesertText;
    private TMP_Text themeIllusionText;
    private TMP_Text levelEasyText;
    private TMP_Text levelNormalText;
    private TMP_Text levelHardText;

    // Start is called before the first frame update
    void Awake()
    {
        if (mc != null)
        {
            isBGMOff = mc.isBGMOff;
            isSoundOff = mc.isSoundOff;
            Destroy(mc.gameObject);
        }
        mc = this;
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        theme = Theme.None;
        level = Level.None;
        bgmAudioSource = bgm.GetComponent<AudioSource>();
        bgmText = bgm.GetComponent<TMP_Text>();
        themeSunsetText = themeSunsetButton.GetComponentInChildren<TMP_Text>();
        themeDesertText = themeDesertButton.GetComponentInChildren<TMP_Text>();
        themeIllusionText = themeIllusionButton.GetComponentInChildren<TMP_Text>();
        levelEasyText = levelEasyButton.GetComponentInChildren<TMP_Text>();
        levelNormalText = levelNormalButton.GetComponentInChildren<TMP_Text>();
        levelHardText = levelHardButton.GetComponentInChildren<TMP_Text>();
        UpdateLevel();
        UpdateStart();

        if (isBGMOff)
        {
            bgmAudioSource.volume = 0f;
            bgmText.text = "    BGM (Off)"; // TODO: `.text = ` 모두 찾아서 Cysharp.ZString으로 바꾸기
            bgmButton.colors = new ColorBlock
            {
                colorMultiplier = 1f,
                fadeDuration = 0.1f,
                normalColor = ColorUtil.DarkWhite,
                disabledColor = ColorUtil.DarkWhite,
                highlightedColor = ColorUtil.Blue,
                selectedColor = ColorUtil.Blue,
                pressedColor = ColorUtil.BrightBlue
            };

        }
        else
        {
            bgmAudioSource.volume = 1f;
            bgmText.text = "    BGM (On)";
            bgmButton.colors = new ColorBlock
            {
                colorMultiplier = 1f,
                fadeDuration = 0.1f,
                normalColor = ColorUtil.DarkWhite,
                disabledColor = ColorUtil.DarkWhite,
                highlightedColor = ColorUtil.Green,
                selectedColor = ColorUtil.Green,
                pressedColor = ColorUtil.BrightGreen
            };
        }
    }

#if !((UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_WP8_1))
    private void Update()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0 && !Cursor.visible)   // Main scene
        {
            Cursor.visible = true;
        }
    }
#endif

    public void SunsetTheme()
    {
        if (_hasGameStart) return;
        theme = Theme.Sunset;
        SceneIndex = (int)theme;    // 1
        UpdateStart();
        UpdateTheme();
    }

    public void DesertTheme()
    {
        if (_hasGameStart) return;
        theme = Theme.Desert;
        SceneIndex = (int)theme;    // 2
        UpdateStart();
        UpdateTheme();
    }

    public void IllusionTheme()
    {
        if (_hasGameStart) return;
        theme = Theme.Illusion;
        SceneIndex = (int)theme;    // 3
        UpdateStart();
        UpdateTheme();
    }

    public void EasyLevel()
    {
        if (_hasGameStart) return;
        level = Level.Easy;
        MazeColumns = 12;
        MazeRows = 12;
        InitialTime = 305;
        UpdateLevel();
        UpdateStart();
    }

    public void NormalLevel()
    {
        if (_hasGameStart) return;
        level = Level.Normal;
        MazeColumns = 18;
        MazeRows = 18;
        InitialTime = 350;
        UpdateLevel();
        UpdateStart();
    }

    public void HardLevel()
    {
        if (_hasGameStart) return;
        level = Level.Hard;
        MazeColumns = 24;
        MazeRows = 24;
        InitialTime = 395;
        UpdateLevel();
        UpdateStart();
    }

    private void UpdateStart()
    {
        if (_hasGameStart || theme == Theme.None || level == Level.None)
            startButton.interactable = false;
        else
            startButton.interactable = true;
    }

    private void UpdateTheme()
    {
        themeSunsetText.color = ColorUtil.DarkWhite;
        themeDesertText.color = ColorUtil.DarkWhite;
        themeIllusionText.color = ColorUtil.DarkWhite;
        backgroundImage.color = Color.white;

        switch (theme)
        {
            case Theme.Sunset:
                themeSunsetText.color = themeSunsetButton.colors.highlightedColor;
                backgroundImage.sprite = sunsetSprite;
                break;
            case Theme.Desert:
                themeDesertText.color = themeDesertButton.colors.highlightedColor;
                backgroundImage.sprite = desertSprite;
                break;
            case Theme.Illusion:
                themeIllusionText.color = themeIllusionButton.colors.highlightedColor;
                backgroundImage.color = ColorUtil.DarkWhite;
                backgroundImage.sprite = illusionSprite;
                break;
        }
    }

    private void UpdateLevel()
    {
        levelEasyText.color = ColorUtil.DarkWhite;
        levelNormalText.color = ColorUtil.DarkWhite;
        levelHardText.color = ColorUtil.DarkWhite;
        sizeText.text = MazeColumns + " X " + MazeRows; // TODO

        switch (level)
        {
            case Level.None:
                sizeText.color = Color.white;
                sizeText.text = string.Empty;
                break;
            case Level.Easy:
                levelEasyText.color = levelEasyButton.colors.highlightedColor;
                sizeText.color = levelEasyButton.colors.pressedColor;
                break;
            case Level.Normal:
                levelNormalText.color = levelNormalButton.colors.highlightedColor;
                sizeText.color = levelNormalButton.colors.pressedColor;
                break;
            case Level.Hard:
                levelHardText.color = levelHardButton.colors.highlightedColor;
                sizeText.color = levelHardButton.colors.pressedColor;
                break;
        }
    }

    public void StartGame()
    {
        if (_hasGameStart || theme == Theme.None || level == Level.None) return;
        _hasGameStart = true;
        SceneManager.LoadScene(SceneIndex);
        UpdateStart();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }

    public void BGMButton()
    {
        if (!isBGMOff)
        {
            isBGMOff = true;
            bgmAudioSource.volume = 0f;
            bgmText.text = "    BGM (Off)";
            bgmButton.colors = new ColorBlock
            {
                colorMultiplier = 1f,
                fadeDuration = 0.1f,
                normalColor = ColorUtil.DarkWhite,
                disabledColor = ColorUtil.DarkWhite,
                highlightedColor = ColorUtil.Blue,
                selectedColor = ColorUtil.Blue,
                pressedColor = ColorUtil.BrightBlue
            };
        }
        else
        {
            isBGMOff = false;
            bgmAudioSource.volume = 1f;
            bgmText.text = "    BGM (On)";
            bgmButton.colors = new ColorBlock
            {
                colorMultiplier = 1f,
                fadeDuration = 0.1f,
                normalColor = ColorUtil.DarkWhite,
                disabledColor = ColorUtil.DarkWhite,
                highlightedColor = ColorUtil.Green,
                selectedColor = ColorUtil.Green,
                pressedColor = ColorUtil.BrightGreen
            };
        }
    }

    public void GitHubButton(string username)
    {
        Application.OpenURL($"https://github.com/{username}");
    }
}
