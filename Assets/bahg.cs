using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class bahg : MonoBehaviour
{
    [SerializeField] private InputActionReference _input;
    private Lazy<InputAction> _inputAction;

    void Awake()
    {
        _inputAction = new(_input.ToInputAction);
    }
    
    // Update is called once per frame
    void Update()
    {
        var val = _inputAction.Value.ReadValue<Vector3>();
        Debug.Log(val);
    }
}
