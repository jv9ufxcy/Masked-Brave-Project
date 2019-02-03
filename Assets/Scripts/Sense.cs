using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Sense : MonoBehaviour
{
    public float checkRadius;
    public LayerMask checkLayers;

    private void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, checkRadius, checkLayers);
            //Debug.Log(colliders.Length);
            Array.Sort(colliders, new DistanceCompaerer(transform));

            foreach (Collider2D item in colliders)
            {
                Debug.Log(item.name);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, checkRadius);
    }
}
