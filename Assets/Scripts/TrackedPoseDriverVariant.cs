using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class TrackedPoseDriverVariant : TrackedPoseDriver
{
    private Transform m_MainCameraTransform;
    private Collider m_collider;
    private Rigidbody m_Rigidbody;
    
    protected override void Awake()
    {
        base.Awake();
        m_MainCameraTransform = GetComponentInChildren<Camera>().transform;
        m_collider = GetComponent<Collider>();
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Updates <see cref="Transform"/> properties, constrained by tracking type and tracking state.
    /// </summary>
    /// <param name="newPosition">The new local position to possibly set.</param>
    /// <param name="newRotation">The new local rotation to possibly set.</param>
    protected override void SetLocalTransform(Vector3 newPosition, Quaternion newRotation)
    {
        // position.x, z는 collider에 반영
        // position.y, rotation은 camera에 반영
        //Vector3 newYPosition = new Vector3(0f, newPosition.y, 0f);
        //base.SetLocalTransform(newPosition, newRotation);
        Vector3 localPosition = new Vector3(newPosition.x, 0f, newPosition.z);
        
        //Vector3 pos = new Vector3(newPosition.x, 0f, newPosition.z);
        //m_CharacterController.Move(Time.deltaTime * localPosition);
        transform.localPosition = localPosition;
        m_MainCameraTransform.localPosition = new Vector3(0f, newPosition.y, 0f);
        m_MainCameraTransform.localRotation = newRotation;
    }

    private void OnCollisionStay(Collision other)
    {
        Debug.Log($"OnCollisionStay: {other.gameObject.name}");
    }
}
