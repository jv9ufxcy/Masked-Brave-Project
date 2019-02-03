using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollingParallaxBackground : MonoBehaviour
{
    [SerializeField] private bool isScrollingEnabled, isParallaxEnabled;
    [SerializeField] private float backgroundSize;
    [SerializeField] float parallaxSpeed;

    private Transform cameraLocation;
    private Transform[] backgroundLayers;
    [SerializeField] private float viewableSpace = 10;
    private int leftIndex;
    private int rightIndex;
    private float lastCameraX;
    
	// Use this for initialization
	void Start ()
    {
        cameraLocation = Camera.main.transform;
        lastCameraX = cameraLocation.position.x;
        backgroundLayers = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            backgroundLayers[i] = transform.GetChild(i);
        }
        leftIndex = 0;
        rightIndex = backgroundLayers.Length - 1;
	}
    void Update()
    {
        if (isParallaxEnabled)
        {
            float deltaX = cameraLocation.position.x - lastCameraX;
            transform.position += Vector3.right * (deltaX * parallaxSpeed);
        }
        
        lastCameraX = cameraLocation.position.x;

        if (isScrollingEnabled)
        {
            if (cameraLocation.position.x < (backgroundLayers[leftIndex].position.x + viewableSpace))
                ScrollLeft();
            if (cameraLocation.position.x < (backgroundLayers[rightIndex].position.x - viewableSpace))
                ScrollRight();
        }
        
    }
    private void ScrollLeft()
    {
        int lastRight = rightIndex;
        backgroundLayers[rightIndex].position = Vector3.right * (backgroundLayers[leftIndex].position.x - backgroundSize);
        leftIndex = rightIndex;
        rightIndex--;
        if (rightIndex < 0)
            rightIndex = backgroundLayers.Length - 1;
    }
    private void ScrollRight()
    {
        int lastLeft = leftIndex;
        backgroundLayers[leftIndex].position = Vector3.right * (backgroundLayers[rightIndex].position.x + backgroundSize);
        rightIndex = leftIndex;
        leftIndex++;
        if (leftIndex ==backgroundLayers.Length)
            leftIndex = 0;
    }
    // Update is called once per frame
    
}
