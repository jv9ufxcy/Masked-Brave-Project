using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour {
    
    [SerializeField]
    Transform objectToFollow;

    CinemachineVirtualCamera vCam;
    CinemachineConfiner confiner;
    [SerializeField] private Collider2D[] boundingBoxes;

    // Use this for initialization
    void Start ()
    {
        vCam = GetComponent<CinemachineVirtualCamera>();
        confiner = GetComponent<CinemachineConfiner>();
	}
	
    public void ChangeConfiner(int index)
    {
        confiner.InvalidatePathCache();
        confiner.m_BoundingShape2D = boundingBoxes[index];
    }
}
