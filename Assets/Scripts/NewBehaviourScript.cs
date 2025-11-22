using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingMovementController : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;

    [SerializeField] private GameObject leftHand, rightHand;

    [SerializeField] private float speed = 4;
   
    private Vector3 previousPosLeft, previousPosRight, direction;
    private Vector3 gravity = new Vector3(0, -9.8f, 0);

    private void Start()
    {
        SetPreviousPos();
    }

    private void Update()
    {
        Vector3 leftHandVelocity = leftHand.transform.position - previousPosLeft;
        Vector3 rightHandVelocity = rightHand.transform.position - previousPosRight;
        float totalVelocity = +leftHandVelocity.magnitude * 0.8f + rightHandVelocity.magnitude * 0.8f;
        
        direction = Camera.main.transform.forward;
        characterController.Move(speed * Time.deltaTime * Vector3.ProjectOnPlane(direction, Vector3.up));

        characterController.Move(gravity * Time.deltaTime);
        SetPreviousPos();
    }

    private void SetPreviousPos()
    {
        previousPosLeft = leftHand.transform.position;
        previousPosRight = rightHand.transform.position;
    }
}