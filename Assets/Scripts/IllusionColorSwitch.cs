using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IllusionColorSwitch : MonoBehaviour
{
    public Camera mainCamera;
    // Start is called before the first frame update
    void Start()
    {
        mainCamera.backgroundColor = Color.black;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
