using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR;

public class SwingingArmMotion : MonoBehaviour
{
    // Game Objects
    [SerializeField] private XROrigin xrOrigin;
    [SerializeField] private Transform parent;
    [SerializeField] private Transform LeftHand;
    [SerializeField] private Transform RightHand;
    [SerializeField] private Transform MainCamera;

    //Vector3 Positions
    [SerializeField] private Vector3 PositionPreviousFrameLeftHand;
    [SerializeField] private Vector3 PositionPreviousFrameRightHand;
    [SerializeField] private Vector3 PlayerPositionPreviousFrame;
    [SerializeField] private Vector3 PlayerPositionCurrentFrame;
    [SerializeField] private Vector3 PositionCurrentFrameLeftHand;
    [SerializeField] private Vector3 PositionCurrentFrameRightHand;
    
    [SerializeField] private CharacterController characterController;

    //Speed
    [SerializeField] private float BaseSpeed = 100;
    [SerializeField] private float MovementBuffer = 1f;
    [SerializeField] private float ForwardBuffer = 0.5f;
    [FormerlySerializedAs("Thresh")] [SerializeField] private float Threshold = 0.005f;
    
    private Matrix4x4 _prevLeftHand;
    private Matrix4x4 _prevRightHand;

    private float _totalMoved;
    private Vector3 _totalForward;
    private Queue<(float, float)> _movementQueue = new();
    private Queue<(float, Vector3)> _forwardQueue = new();

    public float speed => Time.timeSinceLevelLoad > 1f ? (_totalMoved / 60) * BaseSpeed : 0f;
    
    private Vector4 _withW(Vector3 v, float w) => new (v.x, v.y, v.z, w);
    
    void Start()
    {
        _prevLeftHand = Matrix4x4.TRS(LeftHand.localPosition, LeftHand.localRotation, LeftHand.localScale);
        _prevRightHand = Matrix4x4.TRS(RightHand.localPosition, RightHand.localRotation, RightHand.localScale);
    }

    // Update is called once per frame
    void Update()
    {
        var relLeftHand = (Vector3)(_prevLeftHand.inverse * _withW(LeftHand.localPosition, 1));
        var relRightHand = (Vector3)(_prevRightHand.inverse * _withW(RightHand.localPosition, 1));
        relLeftHand.x = 0;
        relRightHand.x = 0;
        
        var leftNoneX = new Vector3(0, relLeftHand.y, relLeftHand.z);
        var rightNoneX = new Vector3(0, relRightHand.y, relRightHand.z);

        var leftHandMoved = relLeftHand.x < leftNoneX.magnitude ? leftNoneX.magnitude : 0;
        if (leftHandMoved < Threshold) leftHandMoved = 0;
        var rightHandMoved = relRightHand.x < rightNoneX.magnitude ? rightNoneX.magnitude : 0;
        if (rightHandMoved < Threshold) rightHandMoved = 0;
        var handMoved = leftHandMoved + rightHandMoved;
        _totalMoved += handMoved;
        _movementQueue.Enqueue((Time.time, handMoved));
        while (_movementQueue.Count > 0 && _movementQueue.Peek().Item1 + MovementBuffer < Time.time)
        {
            _totalMoved -= _movementQueue.Dequeue().Item2;
        }

        _totalForward += LeftHand.forward + RightHand.forward;
        _forwardQueue.Enqueue((Time.time, LeftHand.forward + RightHand.forward));
        while (_forwardQueue.Count > 0 && _forwardQueue.Peek().Item1 + ForwardBuffer < Time.time)
        {
            _totalForward -= _forwardQueue.Dequeue().Item2;
        }

        var forward = _totalForward;
        forward.y = 0;
        forward = forward.normalized;

        if (Time.timeSinceLevelLoad > 1f)
        {
            Vector3 velocity = (_totalMoved / (60 * MovementBuffer)) * BaseSpeed * forward;
            characterController.Move(velocity * Time.deltaTime);
        }

        _prevLeftHand = Matrix4x4.TRS(LeftHand.localPosition, LeftHand.localRotation, LeftHand.localScale);
        _prevRightHand = Matrix4x4.TRS(RightHand.localPosition, RightHand.localRotation, RightHand.localScale);
    }
}
