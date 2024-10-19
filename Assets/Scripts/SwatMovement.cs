using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SwatMovement : MonoBehaviour
{

    public float turnSpeed = 5f;

    [HideInInspector]
    public Camera mainCamera;
    
    [HideInInspector]
    public AudioSource audioSource;
    
    int _animationState;
    Animator _animator;
    Vector3 _movement;
    Rigidbody _rigidbody;
    float _footstepTiming = 0f;
    private Transform virtualCameraTransform;
    private static readonly int State = Animator.StringToHash("AnimationState");
    private static readonly int IsSprinting = Animator.StringToHash("IsSprinting");
    private static readonly int NeedTurnLeft = Animator.StringToHash("NeedTurnLeft");
    private static readonly int NeedTurnRight = Animator.StringToHash("NeedTurnRight");

    //Transform lookAt;
    //Transform follow;
#if (UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_WP8_1)
    float _vertical = 0f;
#endif

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

#if (UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_WP8_1)
    private void Start()
    {
        virtualCameraTransform = GameController.gc.virtualCamera.GetComponent<Transform>();
    }

    private void Update()
    {
        _vertical = MobileJoystick.instance.moveDirection.y;
    }
#endif

    private void FixedUpdate()
    {
#if !((UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_WP8_1))
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
#else
        float horizontal = 0f;
#endif

        bool hasHorizontalInput = !Mathf.Approximately(horizontal, 0f);
        bool hasVerticalInput = !Mathf.Approximately(vertical, 0f);
        bool isMoving = hasHorizontalInput || hasVerticalInput;

        int horizontalState = !hasHorizontalInput ? 1 : (horizontal > 0 ? 2 : 0);
        int verticalState = !hasVerticalInput ? 1 : (vertical > 0 ? 2 : 0);
        _animationState = verticalState * 3 + horizontalState;

#if !((UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_WP8_1))
        bool isSprinting = (Input.GetKey(KeyCode.LeftShift) || Input.GetMouseButton(1)) && _animationState == 7;
        
        if (isMoving && isSprinting) GameController.gc.SetShiftPressed();
#else
        bool isSprinting = MobileButton.instance.HasPressed && 
            _vertica > 0f && m_AnimationState == 7;

        if (isMoving)
            MobileJoystick.instance.SetCircleColor(new Color(1f, 0.8666667f, 0f));
        else
            MobileJoystick.instance.SetCircleColor(new Color(1f, 1f, 1f));

        if (isSprinting)
            MobileButton.instance.SetCircleColor(new Color(1f, 0.06666667f, 0f));
        else
            MobileButton.instance.SetCircleColor(new Color(1f, 1f, 1f));

        virtualCameraTransform.Rotate(
            new Vector3(0f, 2.2f * Mathf.Pow(MobileJoystick.instance.moveDirection.x, 2) * Mathf.Sign(MobileJoystick.instance.moveDirection.x), 0f));
#endif

        Vector3 eulerAngles = mainCamera.transform.eulerAngles;
        float angle = Mathf.Deg2Rad * eulerAngles.y;
        _movement = Vector3.RotateTowards(transform.forward, 
            new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)), turnSpeed * Time.fixedDeltaTime, 0f);

        float deltaAngle = Mathf.DeltaAngle(eulerAngles.y, eulerAngles.y);
        bool needTurnLeft = deltaAngle < -30;
        bool needTurnRight = deltaAngle > 30;

        if (isMoving)
        {
            if (isSprinting) _footstepTiming += Time.fixedDeltaTime / 0.38f;
            else _footstepTiming += Time.fixedDeltaTime / 0.49f;
        }

        if (_footstepTiming > 1f)
        {
            _footstepTiming = 0f;
            int index = UnityEngine.Random.Range(0, GameController.gc.footsteps.Count - 1);
            audioSource.PlayOneShot(GameController.gc.footsteps[index]);
            GameController.gc.footsteps.Add(GameController.gc.footsteps[index]);
            GameController.gc.footsteps.RemoveAt(index);
        }

        _animator.SetInteger(State, _animationState);
        _animator.SetBool(IsSprinting, isSprinting);
        _animator.SetBool(NeedTurnLeft, needTurnLeft);
        _animator.SetBool(NeedTurnRight, needTurnRight);
    }

    private void OnAnimatorMove()
    {
        _rigidbody.MovePosition(_animator.rootPosition);
        _rigidbody.MoveRotation(_animationState == 4 ? _animator.rootRotation : Quaternion.LookRotation(_movement));
    }

    private void OnAnimatorIK(int layerIndex)
    {
        _animator.SetLookAtWeight(.7f);
        _animator.SetLookAtPosition(transform.position + mainCamera.transform.forward * 1000f);
        //m_Animator.SetIKRotation(AvatarIKGoal.Left, Quaternion.Euler(0, mainCamera.transform.eulerAngles.y, 0));
    }
}
