﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cinemachine;

public class GameController : MonoBehaviour
{
    public static GameController gc;

    public float edgeLength;
    public Vector3 edgeBasePosition;
    public Vector3 edgeBaseRotation;
    public bool useColorMatching;
    public GameObject[] corners;
    public GameObject[] edges;
    public GameObject colliderCorner;
    public GameObject colliderEdge;
    public GameObject colliderExit;
    public GameObject playerPrefab;
    public List<AudioClip> footsteps;
    public GameObject mobileMenuUI;
    public Button mobileMenuButton;
    public Text mobileMenuButtonText;
    public GameObject mobileMenu;
    public Text mobileSoundButtonText;
    public GameObject pcMenuUI;
    public Button pcMenuButton;
    public Text pcMenuButtonText;
    public GameObject pcMenu;
    public Text pcSoundButtonText;
    public Text timeText;
    public DistanceSlider progressSlider;
    public AudioClip exitClip;
    public AudioClip timeoutClip;
    public Camera mainCamera;
    public CinemachineVirtualCamera virtualCamera;
    public GameObject touchInput;

    public float initialTime = 300f;
    public Vector3 initialPlayerPosition;

    public bool mazeFromFile = false;

    public int mazeColumns = 15;
    public int mazeRows = 15;
    public int mazeInnerColumns = 5;
    public int mazeInnerRows = 5;
    public float mazeDropProbability = 0.02f;   // When this is 0.0f, only one way exists

    private float _time;
    private int[,] _maze;
    private bool _hasExited = false;
#if !((UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_WP8_1))
    private bool _hasPressedShift = false;
#endif
    private bool _isTimeout = false;
    private bool _isMenuShowed = false;
    private Vector2 _distanceMaxValue;
    private Vector2 _distanceCurrentValue;

    private SwatMovement _player;
    private AudioSource _audioSource;

    private void Awake()
    {
        if (gc != null)
        {
            Destroy(gc.gameObject);
        }
        gc = this;

        GameObject p = Instantiate(playerPrefab, initialPlayerPosition, /*Quaternion.Euler(0f, 180f, 0f)*/ Quaternion.identity);
        _player = p.GetComponent<SwatMovement>();
        _player.mainCamera = mainCamera;
        virtualCamera.Follow = GameObject.FindGameObjectWithTag("Jaw").GetComponent<Transform>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
#if !((UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_WP8_1))
        Cursor.visible = false;
        touchInput.SetActive(false);
        virtualCamera.AddCinemachineComponent<CinemachinePOV>();
        virtualCamera.GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.m_MaxSpeed = 250f;
        virtualCamera.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_MaxSpeed = 250f;
        mobileMenuUI.SetActive(false);
        pcMenuUI.SetActive(true);
#else
        touchInput.SetActive(true);
        mobileMenuUI.SetActive(true);
        pcMenuUI.SetActive(false);
#endif
        if (MainController.mc.isSoundOff)
        {
            _audioSource.volume = 0f;
            _player.audioSource.volume = 0f;
#if !((UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_WP8_1))
            pcSoundButtonText.text = "<color=#E69900>◆</color> Sound (Off)";
#else
            mobileSoundButtonText.text = "Sound (Off) <color=#E69900>◆</color>";
#endif
        }
        else
        {
            _audioSource.volume = 1f;
            _player.audioSource.volume = 1f;
#if !((UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_WP8_1))
            pcSoundButtonText.text = "<color=#E69900>◆</color> Sound (On)";
#else
            mobileSoundButtonText.text = "Sound (On) <color=#E69900>◆</color>";
#endif
        }

        MazeGenerator mMazeGenerator = new MazeGenerator();
        bool fileExist = false;

        // simple setting
        mMazeGenerator.SetRatio(.75f);

        if (MainController.mc != null)
        {
            mazeColumns = MainController.mc.MazeColumns;
            mazeRows = MainController.mc.MazeRows;
            initialTime = MainController.mc.InitialTime;
        }

        mazeColumns = Mathf.Clamp(mazeColumns, 5, 26);
        mazeRows = Mathf.Clamp(mazeRows, 5, 26);
        mazeInnerColumns = Mathf.Clamp(mazeInnerColumns, 0, mazeColumns);
        mazeInnerRows = Mathf.Clamp(mazeInnerRows, 0, mazeRows);
        mazeDropProbability = Mathf.Clamp(mazeDropProbability, 0f, 1f);

        if (mazeFromFile)
        {
            // There is no exit collider.
            /*
            string path = "Assets/Resources/Map.txt";

            try
            {
                StreamReader reader = new StreamReader(path);
                string line;
                int cols = System.Convert.ToInt32(reader.ReadLine());
                int rows = System.Convert.ToInt32(reader.ReadLine());
                m_Maze = new int[2 * cols, 2 * rows];
                for (int i = 0; i < 2 * cols; i++)
                {
                    line = reader.ReadLine();
                    for (int j = 0; j < 2 * rows; j++)
                    {
                        m_Maze[i, j] = line[j] == '#' ? 1 : 0;
                    }
                }
                reader.Close();
                fileExist = true;
            }
            catch (FileNotFoundException)
            {

            }
            */
        }
        if (!fileExist)
        {
            _maze = mMazeGenerator.FromDimensions(mazeColumns, mazeRows, mazeInnerColumns, mazeInnerRows, mazeDropProbability);
            // make two entrances
            _maze[0, 1] = 0;
            _maze[2 * mazeColumns, 2 * mazeRows - 1] = 0;

            //Debug.Log(m_MazeGenerator.ConvertToString(m_Maze));
        }

        if (useColorMatching)
        {
            int[,] mazeColor = new int[_maze.GetLength(0), _maze.GetLength(1)];
            for (int i = 0; i < _maze.GetLength(0); i += 2)
            {
                for (int j = 0; j < _maze.GetLength(1); j += 2)
                {
                    if (_maze[i, j] == 0)
                    {
                        mazeColor[i, j] = -1;
                        continue;
                    }
                    // i % 2 * 2 + j % 2 == 0
                    int r = Random.Range(0, corners.Length);
                    Instantiate(corners[r],
                                new Vector3(edgeLength * j / 2f, 0f, edgeLength * i / 2f) + transform.position + edgeBasePosition,
                                Quaternion.Euler(edgeBaseRotation.x, 90f * Random.Range(0, 4) + edgeBaseRotation.y, edgeBaseRotation.z));
                    mazeColor[i, j] = r;
                }
            }

            for (int i = 0; i < _maze.GetLength(0); i += 2)
            {
                for (int j = 1; j < _maze.GetLength(1); j += 2)
                {
                    if (_maze[i, j] == 0) continue;
                    // i % 2 * 2 + j % 2 == 1
                    int r = Random.Range(0, edges.Length);
                    if (j + 1 < _maze.GetLength(1) && mazeColor[i, j - 1] == mazeColor[i, j + 1])
                        r = mazeColor[i, j - 1];
                    Instantiate(edges[r],
                                new Vector3(edgeLength * j / 2f, 0f, edgeLength * i / 2f) + transform.position + edgeBasePosition,
                                Quaternion.Euler(edgeBaseRotation.x, 90f + 180f * Random.Range(0, 2) + edgeBaseRotation.y, edgeBaseRotation.z));
                }
            }

            for (int i = 1; i < _maze.GetLength(0); i += 2)
            {
                for (int j = 0; j < _maze.GetLength(1); j += 2)
                {
                    if (_maze[i, j] == 0) continue;
                    // i % 2 * 2 + j % 2 == 2
                    int r = Random.Range(0, edges.Length);
                    if (i + 1 < _maze.GetLength(0) && mazeColor[i - 1, j] == mazeColor[i + 1, j])
                        r = mazeColor[i - 1, j];
                    Instantiate(edges[r],
                        new Vector3(edgeLength * j / 2f, 0f, edgeLength * i / 2f) + transform.position + edgeBasePosition,
                        Quaternion.Euler(edgeBaseRotation.x, 180f * Random.Range(0, 2) + edgeBaseRotation.y, edgeBaseRotation.z));
                }
            }
        }
        else
        {
            for (int i = 0; i < _maze.GetLength(0); i++)
            {
                for (int j = 0; j < _maze.GetLength(1); j++)
                {
                    if (_maze[i, j] == 0) continue;
                    int type = i % 2 * 2 + j % 2;
                    switch (type)
                    {
                        case 0:
                            Instantiate(corners[Random.Range(0, corners.Length)],
                                new Vector3(edgeLength * j / 2f, 0f, edgeLength * i / 2f) + transform.position + edgeBasePosition,
                                Quaternion.Euler(edgeBaseRotation.x, 90f * Random.Range(0, 4) + edgeBaseRotation.y, edgeBaseRotation.z));
                            break;
                        case 1:
                            Instantiate(edges[Random.Range(0, edges.Length)],
                                new Vector3(edgeLength * j / 2f, 0f, edgeLength * i / 2f) + transform.position + edgeBasePosition,
                                Quaternion.Euler(edgeBaseRotation.x, 90f + 180f * Random.Range(0, 2) + edgeBaseRotation.y, edgeBaseRotation.z));
                            break;
                        case 2:
                            Instantiate(edges[Random.Range(0, edges.Length)],
                                new Vector3(edgeLength * j / 2f, 0f, edgeLength * i / 2f) + transform.position + edgeBasePosition,
                                Quaternion.Euler(edgeBaseRotation.x, 180f * Random.Range(0, 2) + edgeBaseRotation.y, edgeBaseRotation.z));
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        // make exit collider
        Instantiate(colliderExit,
            new Vector3(edgeLength * (2 * mazeRows - 1) / 2f, 0f, edgeLength * mazeColumns) + transform.position,
            Quaternion.Euler(0, 90f + 180f * Random.Range(0, 2), 0));

        GameObject p = Instantiate(playerPrefab, 
            new Vector3(edgeLength * (mazeRows - 0.25f), 0f, edgeLength * (mazeColumns + 0.5f)),
            Quaternion.Euler(0, 216f, 0));
        p.GetComponent<SwatMovement>().enabled = false;
        p.GetComponent<Animator>().SetInteger("AnimationState", 4);
        p.GetComponent<Animator>().SetBool("IsSprinting", false);
        p.GetComponent<Animator>().SetBool("NeedTurnLeft", false);
        p.GetComponent<Animator>().SetBool("NeedTurnRight", false);

        for (int i = -1; i > -9; i--)
        {
            CreateColliders(i, 8);
        }

        for (int j = -1; j > -9; j--)
        {
            CreateColliders(8, j);
        }

        for (int i = _maze.GetLength(0) + 7; i > -9; i--)
        {
            CreateColliders(i, -8);
            CreateColliders(i, _maze.GetLength(1) + 7);
        }

        for (int j = _maze.GetLength(1) + 7; j > -9; j--)
        {
            CreateColliders(-8, j);
            CreateColliders(_maze.GetLength(0) + 7, j);
        }

        _time = initialTime;
        _distanceMaxValue = new Vector2(edgeLength * (mazeRows - 1), edgeLength * (mazeColumns - 1));
        _distanceCurrentValue = new Vector2(0f, 0f);
        // distanceMinValue = new Vector2(0f, 0f);

        if (MainController.mc != null)
        {
            if (MainController.mc.isSoundOff)
            {
                _audioSource.volume = 0f;
                _player.audioSource.volume = 0f;
                //soundButtonText.text = "<color=#E69900>◆</color> Sound (Off)";
                mobileSoundButtonText.text = "Sound (Off) <color=#E69900>◆</color>";
            }
            else
            {
                _audioSource.volume = 1f;
                _player.audioSource.volume = 1f;
                //soundButtonText.text = "<color=#E69900>◆</color> Sound (On)";
                mobileSoundButtonText.text = "Sound (On) <color=#E69900>◆</color>";
            }
        }
    }

    private void CreateColliders(int i, int j)
    {
        int type = Mathf.Abs(i) % 2 * 2 + Mathf.Abs(j) % 2;
        switch (type)
        {
            case 0:
                Instantiate(colliderCorner,
                    new Vector3(edgeLength * j / 2f, 0f, edgeLength * i / 2f) + transform.position,
                    Quaternion.Euler(0, 90f * Random.Range(0, 4), 0));
                break;
            case 1:
                Instantiate(colliderEdge,
                    new Vector3(edgeLength * j / 2f, 0f, edgeLength * i / 2f) + transform.position,
                    Quaternion.Euler(0, 90f + 180f * Random.Range(0, 2), 0));
                break;
            case 2:
                Instantiate(colliderEdge,
                    new Vector3(edgeLength * j / 2f, 0f, edgeLength * i / 2f) + transform.position,
                    Quaternion.Euler(0, 180f * Random.Range(0, 2), 0));
                break;
            default:
                break;
        }
    }

    public void SetExited()
    {
        if (_hasExited) return;
        _hasExited = true;

        _audioSource.Stop();
        _audioSource.clip = exitClip;
        _audioSource.loop = false;
        _audioSource.Play();

        if (!_isMenuShowed)
        {
            MenuButton();
        }
    }
#if !((UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_WP8_1))
    public void SetShiftPressed()
    {
        _hasPressedShift = true;
    }
#endif

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            MenuButton();
        }

#if !((UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_WP8_1))
        if (_isMenuShowed && Input.GetKeyDown(KeyCode.O))
        {
            SoundButton();
            MenuButton();
        }
        if (_isMenuShowed && Input.GetKeyDown(KeyCode.M))
        {
            MoveButton();
            MenuButton();
        }
        if (_isMenuShowed && Input.GetKeyDown(KeyCode.N))
        {
            NewButton();
            MenuButton();
        }
        if (_isMenuShowed && Input.GetKeyDown(KeyCode.Q))
        {
            QuitButton();
            MenuButton();
        }
#endif
    }

    private void FixedUpdate()
    {
        if (!_hasExited)
        {
            _time -= Time.fixedDeltaTime;
            if (!_isTimeout && _time <= 0f)
            {
                _isTimeout = true;
                _audioSource.clip = timeoutClip;
                _audioSource.loop = true;
                _audioSource.Play();
            }
        }

        int sign = (int)Mathf.Sign(_time);
        int hour = Mathf.Abs((int)_time / 3600);
        int minute2 = Mathf.Abs((int)_time % 3600 / 600);
        int minute1 = Mathf.Abs((int)_time / 60 % 10);
        int second2 = Mathf.Abs((int)_time % 60 / 10);
        int second1 = Mathf.Abs((int)_time % 10);
        int secondDot1 = Mathf.Abs((int)(_time * 10) % 10);
        int secondDot2 = Mathf.Abs((int)(_time * 100) % 10);

        if (sign <= 0)
        {
            timeText.text = "<color=#FF1100>-";
        }
        else
        {
            if (_hasExited) timeText.text = "<color=#00FF57>";
            else timeText.text = "";
        }

        if (hour > 0)
            timeText.text += hour + ":" + minute2 + "" + minute1 + ":" + second2 + "" + second1 + "." + secondDot1 + "" + secondDot2 + "\n";
        else if (minute2 > 0)
            timeText.text += minute2 + "" + minute1 + ":" + second2 + "" + second1 + "." + secondDot1 + "" + secondDot2 + "\n";
        else
            timeText.text += minute1 + ":" + second2 + "" + second1 + "." + secondDot1 + "" + secondDot2 + "\n";

        if (sign <= 0 || _hasExited)
            timeText.text += "</color>";

        if (_time <= 0f)
        {
            timeText.text += "Time out!";
        }
        else if (_hasExited)
        {
            timeText.text += "Congraturations!";
        }
#if !((UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_WP8_1))
        else if (!_hasPressedShift)
        {
            timeText.text += "Press 'Left Shift' or 'Right Click' to dash.";
        }
#endif
        else
        {
            if (mazeColumns == 12 && mazeRows == 12)
            {
                timeText.text += "Level: Easy";
            }
            else if (mazeColumns == 18 && mazeRows == 18)
            {
                timeText.text += "Level: Normal";
            }
            else if (mazeColumns == 24 && mazeRows == 24)
            {
                timeText.text += "Level: Hard";
            }
        }

        _distanceCurrentValue = new Vector2(
            _player.GetComponent<Transform>().position.x / _distanceMaxValue.x,
            _player.GetComponent<Transform>().position.z / _distanceMaxValue.y
        );
        progressSlider.SetValues(_distanceCurrentValue);
    }

    public void MenuButton()
    {
        if (_isMenuShowed)
        {
            _isMenuShowed = false;
#if !((UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_WP8_1))
            pcMenu.SetActive(false);
            pcMenuButton.GetComponent<RectTransform>().anchorMax = new Vector2(0.11f, 0.98f);
            pcMenuButtonText.text = "<color=#E6C700>◆</color> Menu";
            Cursor.visible = false;
#else
            mobileMenu.SetActive(false);
            mobileMenuButtonText.text = "Menu <color=#E6C700>◆</color>";
#endif
        }
        else
        {
            _isMenuShowed = true;
#if !((UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_WP8_1))
            pcMenu.SetActive(true);
            pcMenuButton.GetComponent<RectTransform>().anchorMax = new Vector2(0.15f, 0.98f);
            pcMenuButtonText.text = "<color=#E6C700>◆</color> Hide Menu";
            Cursor.visible = false;
#else
            mobileMenu.SetActive(true);
            mobileMenuButtonText.text = "Hide Menu <color=#E6C700>◆</color>";
#endif
        }
    }

    public void SoundButton()
    {
        if (!MainController.mc.isSoundOff)
        {
            MainController.mc.isSoundOff = true;
            _audioSource.volume = 0f;
            _player.audioSource.volume = 0f;
#if !((UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_WP8_1))
            pcSoundButtonText.text = "<color=#E69900>◆</color> Sound (Off)";
#else
            mobileSoundButtonText.text = "Sound (Off) <color=#E69900>◆</color>";
#endif
        }
        else
        {
            MainController.mc.isSoundOff = false;
            _audioSource.volume = 1f;
            _player.audioSource.volume = 1f;
#if !((UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_WP8_1))
            pcSoundButtonText.text = "<color=#E69900>◆</color> Sound (On)";
#else
            mobileSoundButtonText.text = "Sound (On) <color=#E69900>◆</color>";
#endif
        }
    }

    public void MoveButton()
    {
        _player.transform.position = initialPlayerPosition;
    }

    public void NewButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitButton()
    {
        Cursor.visible = true;
        SceneManager.LoadScene(0);
    }
}
