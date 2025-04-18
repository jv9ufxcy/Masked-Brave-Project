using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;

public class CameraController : MonoBehaviour {
    
    [SerializeField]
    Transform objectToFollow;
    [SerializeField] private int defaultCamIndex;
    CinemachineVirtualCamera currentVCam;
    CinemachineConfiner confiner;
    [SerializeField] private Collider2D[] boundingBoxes;
    public static CameraController instance;
    private void Awake()
    {
        instance = this;
    }
    // Use this for initialization
    void Start ()
    {
        currentVCam = vCams[0];
        confiner = GetComponent<CinemachineConfiner>();
	}
    public CinemachineVirtualCamera GetCurrentVCAM()
    {
        return currentVCam;
    }
    [SerializeField] private CinemachineVirtualCamera[] vCams;
    public void DefaultCam()
    {
        ChangeActiveCamera(defaultCamIndex);
    }
    public void ChangeActiveCamera(int index)
    {
        for (int i = 0; i < vCams.Length; i++)
        {
            if (i == index)
            {
                vCams[i].Priority = 1;
                currentVCam = vCams[i];
            }
            else
                vCams[i].Priority = 0;
        }
        Debug.Log("Changed to camera: "+index);
    }
    float dampenTimer=1;
    public void ChangeConfiner(int index)
    {
        ChangeActiveCamera(index);
        //DOVirtual.Float(dampenTimer, 0, .5f, ConfinerDamping);
        ////StartCoroutine(DampingTimer());

        //confiner.InvalidatePathCache();
        //confiner.m_BoundingShape2D = boundingBoxes[index];
    }
    void ConfinerDamping(float x)
    {
        confiner.m_Damping = x;
    }
}
