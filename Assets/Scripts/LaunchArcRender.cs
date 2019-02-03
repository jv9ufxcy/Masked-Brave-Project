using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof (LineRenderer))]
public class LaunchArcRender : MonoBehaviour
{
    private Rigidbody2D myRB;

    private LineRenderer lineRenderer;
    [SerializeField] private float velocity;
    [SerializeField] private float angle;
    [SerializeField] private int resolution=10;

    private float gravity;
    private float radianAngle;

    // Use this for initialization
    void Awake ()
    {
        lineRenderer = GetComponent<LineRenderer>();
        gravity = Mathf.Abs(Physics2D.gravity.y);

	}
    private void OnValidate()
    {
        //check that lineRenderer isn't null and game is playing
        if (lineRenderer!=null&&Application.isPlaying)
        {
            RenderArc();
        }
    }
    private void Start()
    {
        RenderArc();
    }
    //populating the LineRenderer with the appropriate settings
    private void RenderArc()
    {
        lineRenderer.SetVertexCount (resolution + 1);
        lineRenderer.SetPositions (CalculateArcArray());
    }

    //create an array of Vector3 positions for arc
    Vector3[] CalculateArcArray()
    {
        Vector3[] arcArray = new Vector3[resolution + 1];

        radianAngle = Mathf.Deg2Rad * angle;
        float maxDistance = (velocity * velocity * Mathf.Sin(2 * radianAngle)) / gravity;
        for (int i = 0; i<= resolution; i++)
        {
            float howFarAlong = (float)i / (float)resolution;
            arcArray[i] = CalculateArcPoint(howFarAlong, maxDistance);
        }
        return arcArray;
    }

    private Vector3 CalculateArcPoint(float howFarAlong, float maxDistance)
    {
        float x = howFarAlong * maxDistance;
        float y = x * Mathf.Tan(radianAngle) - ((gravity * x * x) / (2 * velocity * velocity * Mathf.Cos(radianAngle) * Mathf.Cos(radianAngle)));
        return new Vector3(x, y);
    }
}
