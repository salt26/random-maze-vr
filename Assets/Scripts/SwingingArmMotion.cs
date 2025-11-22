using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR;

public class SwingingArmMotion : MonoBehaviour
{
    // Game Objects
    [SerializeField] private GameObject xrOrigin;
    [SerializeField] private GameObject LeftHand;
    [SerializeField] private GameObject RightHand;
    [SerializeField] private GameObject MainCamera;

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
    [SerializeField] private float HandSpeed;

    private float _totalMoved;
    private Queue<(float, float)> _movementQueue = new Queue<(float, float)>();

    public float speed => Time.timeSinceLevelLoad > 1f ? (_totalMoved / 60) * BaseSpeed : 0f;

    void Start()
    {
        PlayerPositionPreviousFrame = transform.position; //set current positions
        PositionPreviousFrameLeftHand = LeftHand.transform.position; //set previous positions
        PositionPreviousFrameRightHand = RightHand.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // get positions of hands
        PositionCurrentFrameLeftHand = LeftHand.transform.position;
        PositionCurrentFrameRightHand = RightHand.transform.position;

        // position of player
        PlayerPositionCurrentFrame = transform.position;

        // get distance the hands and player has moved from last frame
        var playerDistanceMoved = Vector3.Distance(PlayerPositionCurrentFrame, PlayerPositionPreviousFrame);
        var leftHandDistanceMoved = Vector3.Distance(PositionPreviousFrameLeftHand, PositionCurrentFrameLeftHand);
        var rightHandDistanceMoved = Vector3.Distance(PositionPreviousFrameRightHand, PositionCurrentFrameRightHand);

        // aggregate to get hand speed
        HandSpeed = ((leftHandDistanceMoved - playerDistanceMoved) + (rightHandDistanceMoved - playerDistanceMoved));
        _totalMoved += HandSpeed;
        _movementQueue.Enqueue((Time.time, HandSpeed));
        while (_movementQueue.Count != 0 && _movementQueue.First().Item1 + 1 < Time.time)
        {
            _totalMoved -= _movementQueue.First().Item2;
            _movementQueue.Dequeue();
        }

        if (Time.timeSinceLevelLoad > 1f)
        {
            // get forward direction from the center eye camera and set it to the forward direction object
            Quaternion q = Quaternion.Euler(0, MainCamera.transform.eulerAngles.y, 0);
            characterController.Move((_totalMoved / 60) * BaseSpeed * Time.deltaTime * (q * Vector3.forward));
            // xrOrigin.transform.position += ForwardDirection.transform.forward * (_totalMoved / 60) * Speed * Time.deltaTime;
        }

        // set previous position of hands for next frame
        PositionPreviousFrameLeftHand = PositionCurrentFrameLeftHand;
        PositionPreviousFrameRightHand = PositionCurrentFrameRightHand;
        // set player position previous frame
        PlayerPositionPreviousFrame = PlayerPositionCurrentFrame;
    }
}
