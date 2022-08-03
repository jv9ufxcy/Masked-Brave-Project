using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;

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
    //private void Update()
    //{
    //    if (confiner.m_Damping > 0)
    //    {
    //        confiner.m_Damping = Mathf.MoveTowards(confiner.m_Damping, 0, dampenTimer*Time.deltaTime);
    //    }
    //}
    float dampenTimer=1;
    public void ChangeConfiner(int index)
    {
        DOVirtual.Float(dampenTimer, 0, .5f, ConfinerDamping);
        //StartCoroutine(DampingTimer());

        confiner.InvalidatePathCache();
        confiner.m_BoundingShape2D = boundingBoxes[index];
    }
    void ConfinerDamping(float x)
    {
        confiner.m_Damping = x;
    }
}
