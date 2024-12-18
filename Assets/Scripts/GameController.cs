﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cinemachine;
using TMPro;

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
    public TMP_Text mobileMenuButtonText;
    public GameObject mobileMenu;
    public TMP_Text mobileSoundButtonText;
    public GameObject pcMenuUI;
    public Button pcMenuButton;
    public TMP_Text pcMenuButtonText;
    public GameObject pcMenu;
    public TMP_Text pcSoundButtonText;
    public TMP_Text timeText;
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
    private static readonly int State = Animator.StringToHash("AnimationState");
    private static readonly int IsSprinting = Animator.StringToHash("IsSprinting");
    private static readonly int NeedTurnLeft = Animator.StringToHash("NeedTurnLeft");
    private static readonly int NeedTurnRight = Animator.StringToHash("NeedTurnRight");

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
            pcSoundButtonText.text = "Sound (Off)";
#else
            mobileSoundButtonText.text = "Sound (Off)";
#endif
        }
        else
        {
            _audioSource.volume = 1f;
            _player.audioSource.volume = 1f;
#if !((UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_WP8_1))
            pcSoundButtonText.text = "Sound (On)";
#else
            mobileSoundButtonText.text = "Sound (On)";
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
        Animator pAnimator = p.GetComponent<Animator>();
        pAnimator.SetInteger(State, 4);
        pAnimator.SetBool(IsSprinting, false);
        pAnimator.SetBool(NeedTurnLeft, false);
        pAnimator.SetBool(NeedTurnRight, false);

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
                mobileSoundButtonText.text = "Sound (Off)";    // TODO
            }
            else
            {
                _audioSource.volume = 1f;
                _player.audioSource.volume = 1f;
                mobileSoundButtonText.text = "Sound (On)";
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
            //MenuButton();
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
        
        // TODO: 텍스트 빌딩

        int sign = (int)Mathf.Sign(_time);
        int hour = Mathf.Abs((int)_time / 3600);
        int minute2 = Mathf.Abs((int)_time % 3600 / 600);
        int minute1 = Mathf.Abs((int)_time / 60 % 10);
        int second2 = Mathf.Abs((int)_time % 60 / 10);
        int second1 = Mathf.Abs((int)_time % 10);
        int secondDot1 = Mathf.Abs((int)(_time * 10) % 10);
        int secondDot2 = Mathf.Abs((int)(_time * 100) % 10);

        StringBuilder sb = new StringBuilder();

        if (sign <= 0)
        {
            sb.Append("<color=#FF1100>-");
        }
        else
        {
            if (_hasExited) sb.Append("<color=#00FF57>");
            else sb.Clear();
        }

        if (hour > 0)
            sb.AppendFormat("{0}:{1}{2}:{3}{4}.{5}{6}\n", hour, minute2, minute1, second2, second1, secondDot1, secondDot2);
        else if (minute2 > 0)
            sb.AppendFormat("{0}{1}:{2}{3}.{4}{5}\n", minute2, minute1, second2, second1, secondDot1, secondDot2);
        else
            sb.AppendFormat("{0}:{1}{2}.{3}{4}\n", minute1, second2, second1, secondDot1, secondDot2);

        if (sign <= 0 || _hasExited)
            sb.Append("</color>");

        if (_time <= 0f)
        {
            sb.Append("Time out!");
        }
        else if (_hasExited)
        {
            sb.Append("Congratulations!");
        }
#if !((UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_WP8_1))
        else if (!_hasPressedShift)
        {
            sb.Append("Press 'Left Shift' or 'Right Click' to dash.");
        }
#endif
        else
        {
            if (mazeColumns == 12 && mazeRows == 12)
            {
                sb.Append("Level: Easy");
            }
            else if (mazeColumns == 18 && mazeRows == 18)
            {
                sb.Append("Level: Normal");
            }
            else if (mazeColumns == 24 && mazeRows == 24)
            {
                sb.Append("Level: Hard");
            }
        }

        timeText.text = sb.ToString();

        Vector3 position = _player.transform.position;
        _distanceCurrentValue = new Vector2(
            position.x / _distanceMaxValue.x,
            position.z / _distanceMaxValue.y
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
            pcMenuButtonText.text = "Menu";    // TODO
            Cursor.visible = false;
#else
            mobileMenu.SetActive(false);
            mobileMenuButtonText.text = "Menu";
#endif
        }
        else
        {
            _isMenuShowed = true;
#if !((UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_WP8_1))
            pcMenu.SetActive(true);
            pcMenuButtonText.text = "Hide Menu";
            Cursor.visible = false;
#else
            mobileMenu.SetActive(true);
            mobileMenuButtonText.text = "Hide Menu";
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
            pcSoundButtonText.text = "Sound (Off)";
#else
            mobileSoundButtonText.text = "Sound (Off)";
#endif
        }
        else
        {
            MainController.mc.isSoundOff = false;
            _audioSource.volume = 1f;
            _player.audioSource.volume = 1f;
#if !((UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_WP8_1))
            pcSoundButtonText.text = "Sound (On)";
#else
            mobileSoundButtonText.text = "Sound (On)";
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
