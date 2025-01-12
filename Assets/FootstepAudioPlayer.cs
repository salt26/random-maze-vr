using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepAudioPlayer : MonoBehaviour
{
    private AudioSource audioSource;
    private SwingingArmMotion swingingArmMotion;
    
    private const float THRESHOLD = 1f;
    private float _movedDistance;
    
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GameController.gc.GetComponent<AudioSource>();
        swingingArmMotion = GetComponent<SwingingArmMotion>();
        _movedDistance = 0;
    }

    // Update is called once per frame
    void Update()
    {
        _movedDistance += swingingArmMotion.speed * Time.deltaTime;
        if (_movedDistance > THRESHOLD)
        {
            _movedDistance = 0f;
            int index = Random.Range(0, GameController.gc.footsteps.Count - 1);
            audioSource.PlayOneShot(GameController.gc.footsteps[index]);
            GameController.gc.footsteps.Add(GameController.gc.footsteps[index]);
            GameController.gc.footsteps.RemoveAt(index);
        }
    }
}
