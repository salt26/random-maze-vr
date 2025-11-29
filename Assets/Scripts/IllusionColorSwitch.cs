using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IllusionColorSwitch : MonoBehaviour
{
    public Camera mainCamera;
    public List<MeshRenderer> floorRenderers;
    public Material blackMaterial;
    public Material whiteMaterial;
    // Start is called before the first frame update
    void Start()
    {
        mainCamera.backgroundColor = Color.white;
        foreach (MeshRenderer renderer in floorRenderers)
        {
            renderer.material = whiteMaterial;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
