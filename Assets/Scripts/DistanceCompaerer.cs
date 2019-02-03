using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceCompaerer : IComparer
{
    private Transform compareTransform_UseProperty;
    public DistanceCompaerer(Transform CompareTransform)
    {
        compareTransform_UseProperty = CompareTransform;
    }
    public int Compare(object x, object y)
    {
        Collider2D xCollider = x as Collider2D;
        Collider2D yCollider = y as Collider2D;

        Vector3 offset = xCollider.transform.position - compareTransform_UseProperty.position;
        float xDistance = offset.sqrMagnitude;

        offset = yCollider.transform.position - compareTransform_UseProperty.position;
        float yDistance = offset.sqrMagnitude;
        return xDistance.CompareTo(yDistance);
    }
}