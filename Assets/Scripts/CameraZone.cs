using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZone : MonoBehaviour
{
    // The following options apply When the target is within the zone
    // Use the maxOrthographicSize as the preferred screen size
    // When useBoundsEdge is true, try to keep screen edges within boundaries, zoom in if needed
    // When there are overlapping camera zones, the camera will try to use the smallest bounds and zoom
    public float maxOrthographicSize = 8;
    public bool useBoundsEdge = false;

    // updated by camera controller
    public bool targetInside = false;

    public float GetBoundsTop()
    {
        return GetComponent<Collider2D>().bounds.max.y;
    }

    public float GetBoundsBottom()
    {
        return GetComponent<Collider2D>().bounds.min.y;
    }

    public float GetBoundsLeft()
    {
        return GetComponent<Collider2D>().bounds.min.x;
    }

    public float GetBoundsRight()
    {
        return GetComponent<Collider2D>().bounds.max.x;
    }

    public bool BoundsCheck(Vector3 position)
    {
		return (position.x >= GetBoundsLeft()) &&
		       (position.x <= GetBoundsRight()) &&
			   (position.y >= GetBoundsBottom()) &&
			   (position.y <= GetBoundsTop());
    }
}
