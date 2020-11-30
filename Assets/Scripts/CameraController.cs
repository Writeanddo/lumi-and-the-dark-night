using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	// follow support
    public GameObject followTarget;
	public float followTimeDefault = 0.3f; // approx sec to reach target
	public bool followRotation = false;    // match rotation of target
	public float followRotationSpeed = 90; // degrees per second
	float followTime;
	Vector3 followVelocity;

	// look support
	public GameObject lookTarget;
	public float lookPanTimeDefault = 1f;  // approx sec to pan to target
	float lookDuration;
	float lookPanTime;
	Vector3 lookVelocity;

	// shake support
	public float shakeDurationDefault = 0.1f; // seconds
	public float shakeAmountDefault = 0.1f;   // world units
	float shakeDuration;
	float shakeAmount;
	Vector3 shakeOffset;

	// zoom support
	public float zoomTransitionDefault = 0.2f;
	int zoomPhase;
	float zoomOriginalSize;
	float zoomCurrentSize;
	float zoomTargetSize;
	float zoomHoldTime;
	float zoomToIncrement;
	float zoomBackIncrement;

	// camera zone support
	public bool useBoundsZoom = true;
	public float boundsZoomTransition = 1.5f;
	public GameObject[] cameraZones;

	// misc
	public bool useFixedUpdate = true; // should never change during the game
	public bool lockY = false;
	public bool lockX = false;
	bool firstFrame = true;
	float deltaTime;

	// INITIALIZATION

	void Awake()
	{
		deltaTime = useFixedUpdate ? Time.fixedDeltaTime : Time.deltaTime;
		Follow(followTarget);
		ZoomSetup(GetComponent<Camera>().orthographicSize);
	}

	// FOLLOW
	// Follow a target object
	// time = approximate time in seconds to reach target

	public void Follow(GameObject obj, float time)
	{
		if (obj != null)
		{
			Debug.Log("Camera now following " + obj.name);
			followTarget = obj;
			followTime= time;
			followVelocity = Vector3.zero;
		}
	}

	public void Follow(GameObject obj)
	{
		Follow(obj, followTimeDefault);
	}

	// LOOK
	// Temporarily look at a different target
	// duration = total time in seconds to be looking
	// panTime = approximate time in seconds to pan to target

	public void Look(GameObject obj, float duration, float panTime)
	{
		if (obj != null)
		{
			lookTarget = obj;
			lookDuration = duration;
			lookPanTime = panTime;
		}
	}

	public void Look(GameObject obj, float duration)
	{
		// use the default pan time
		Look(obj, duration, lookPanTimeDefault);
	}

	// SHAKE
	// Randomly shake the camera for specified duration and amount
	// duration = how many seconds to shake
	// amount = max displacement in world units

	public void Shake(float duration, float amount)
	{
		shakeDuration = duration;
		shakeAmount = amount;
		shakeOffset = Vector3.zero;
	}

	public void Shake(float duration)
	{
		// use the default shake amount
		Shake(duration, shakeAmountDefault);
	}

	public void Shake()
	{
		// use the default shake duration and amount
		Shake(shakeDurationDefault, shakeAmountDefault);
	}

	// BUMP
	// Instantly moves camera by specified amount
	// amount = x/y in world units

	public void Bump(Vector2 amount)
	{
		Vector3 bumpAmount = amount;
		transform.position += bumpAmount;
	}

	// ZOOM
	// Use the setup function to modify the default zoom (orthographic size)
	// Zoom the camera in/out with the specified timing
	// amount = zoom factor (for example 2x, zoom in if less than 1, zoom out of less than 1)
	// holdTime = seconds to hold at specified zoom
	// toTime = seconds to reach specified zoom
	// backTime = seconds to return from specified zoom

	public void ZoomSetup(float orthographicSize)
	{
		zoomOriginalSize = orthographicSize;
		zoomCurrentSize = zoomOriginalSize;
	}

	public void Zoom(float amount, float holdTime, float toTime, float backTime)
	{
		zoomPhase = 1;
		zoomTargetSize = zoomOriginalSize / amount;
		zoomCurrentSize = GetComponent<Camera>().orthographicSize;
		zoomHoldTime = holdTime;
		zoomToIncrement = Mathf.Abs(zoomTargetSize - zoomCurrentSize) / (toTime / deltaTime);
		zoomBackIncrement = Mathf.Abs(zoomOriginalSize - zoomTargetSize) / (backTime / deltaTime);
		zoomToIncrement = Mathf.Max(zoomToIncrement, 0.001f);
		zoomBackIncrement = Mathf.Max(zoomBackIncrement, 0.001f);
		//Debug.Log("Zoom amount=" + amount + " targetSize=" + zoomTargetSize);
	}

	public void Zoom(float amount, float holdTime)
	{
		// use the default transition time for to and back transitions
		Zoom(amount, holdTime, zoomTransitionDefault, zoomTransitionDefault);
	}

	// BOUNDS
	// Create a box collider defining the edges of your scene
	// Make sure the collider is on a layer that will not interact with game objects ("UI" seems good)
	// When moving the camera ensure that all four corners of the screen stay within the specified box
	// With multiple colliders, all apply when the camera target is within their bounds
	// Currently the Look function only uses the main (first) collider

	Vector3 BoundsTargetAdjust(Vector3 targetPosition, bool mainOnly = false)
	{
		// make sure camera z never changes
		Vector3 newPosition = targetPosition;
		newPosition.z = transform.position.z;

		// calc screen size
		float screenHalfHeight = GetComponent<Camera>().orthographicSize;
		float screenHalfWidth = screenHalfHeight * ((float)Screen.width/(float)Screen.height);
		float newOrthographicSize = zoomOriginalSize;
		//Debug.Log("Screen size w=" + 2*screenHalfWidth + " h=" + 2*screenHalfHeight);
		//Debug.Log("Target x=" + targetPosition.x + " y=" + targetPosition.y);

		// check bounds
		bool allValid = false;
		float allMinX = 0;
		float allMaxX = 0;
		float allMinY = 0;
		float allMaxY = 0;
		bool zoneUseBoundsZoom = false;
		bool usingZoneOrtho = false;
		for (int i=0; i<cameraZones.Length; i++)
		{
			CameraZone zone = null;
			if (cameraZones[i] != null)
			{
				zone = cameraZones[i].GetComponent<CameraZone>();
			}
			if (zone != null)
			{
				// check whether target is within the zone
				bool targetInBounds = zone.BoundsCheck(targetPosition);
				// apply camera constraints
				zone.targetInside = targetInBounds;
				if (targetInBounds)
				{
					// constrain orthographic size
					if (usingZoneOrtho)
					{
						newOrthographicSize = Mathf.Min(zone.maxOrthographicSize, newOrthographicSize);
					}
					else
					{
						newOrthographicSize = zone.maxOrthographicSize;
					}
					usingZoneOrtho = true;

					// compute cumulative bounding box
					if (zone.useBoundsEdge)
					{
						zoneUseBoundsZoom = true;
						if (!allValid)
						{
							allMinX = zone.GetBoundsLeft();
							allMaxX = zone.GetBoundsRight();
							allMinY = zone.GetBoundsBottom();
							allMaxY = zone.GetBoundsTop();
							allValid = true;			
						}
						else
						{
							allMinX = Mathf.Max(allMinX, zone.GetBoundsLeft());
							allMaxX = Mathf.Min(allMaxX, zone.GetBoundsRight());
							allMinY = Mathf.Max(allMinY, zone.GetBoundsBottom());
							allMaxY = Mathf.Min(allMaxY, zone.GetBoundsTop());
						}
					}
				}
			}
			if (mainOnly) break;
		}
		if (allValid)
		{
			// compute bounds constrainst based upon screen size
			float camMinX = allMinX + screenHalfWidth;
			float camMaxX = allMaxX - screenHalfWidth;
			float camMinY = allMinY + screenHalfHeight;
			float camMaxY = allMaxY - screenHalfHeight;
			//Debug.Log("Cummulative box x=" + allMinX + "," + allMaxX + " y=" + allMinY + "," + allMaxY);
			//Debug.Log("Camera constraints x=" + camMinX + "," + camMaxX + " y=" + camMinY + "," + camMaxY);

			// apply cumulative constraints
			float margin = 1.0f;
			if (camMaxX - camMinX < margin) newPosition.x = Mathf.Lerp(allMinX, allMaxX, 0.5f);
			else if (targetPosition.x < camMinX) newPosition.x = camMinX;
			else if (targetPosition.x > camMaxX) newPosition.x = camMaxX;
			if (camMaxY - camMinY < margin) newPosition.y = Mathf.Lerp(allMinY, allMaxY, 0.5f);
			else if (targetPosition.y < camMinY) newPosition.y = camMinY;
			else if (targetPosition.y > camMaxY) newPosition.y = camMaxY;
			//Debug.Log("New position x=" + newPosition.x + " y=" + newPosition.y);
		}

		// adjust zoom
		float zoomAmount = 1;
		if (usingZoneOrtho)
		{
			zoomAmount = zoomOriginalSize / newOrthographicSize;
		}
		if (useBoundsZoom && zoneUseBoundsZoom)
		{
			float boundsZoomAmount = BoundsZoomAdjust(allMinX, allMaxX, allMinY, allMaxY, zoomAmount);
			if (boundsZoomAmount > zoomAmount)
			{
				zoomAmount = boundsZoomAmount;
			}
		}
		if (zoomAmount != 1)
		{
			//Debug.Log("Camera zoom adjust " + zoomAmount);
			if (firstFrame)
			{
				Zoom(zoomAmount, 1, 0, 0);
			}
			else
			{
				Zoom(zoomAmount, 0, boundsZoomTransition, boundsZoomTransition);
			}
		}

		return newPosition;
	}

	float BoundsZoomAdjust(float minX, float maxX, float minY, float maxY, float currentZoom)
	{
		float zoomAdjustHeight = currentZoom;
		float zoomAdjustWidth = currentZoom;
		float screenHeight = 2 * zoomOriginalSize;
		float screenWidth = screenHeight * ((float)Screen.width/(float)Screen.height);
		float boundsHeight = maxY - minY;
		float boundsWidth = maxX - minX;
		float currentHeight = screenHeight / currentZoom;
		float currentWidth = screenWidth / currentZoom;
		if (currentHeight > boundsHeight)
		{
			zoomAdjustHeight = screenHeight / boundsHeight;
		}
		if (currentWidth > boundsWidth)
		{
			zoomAdjustWidth = screenWidth / boundsWidth;
		}
		float zoomAdjust = Mathf.Max(zoomAdjustHeight, zoomAdjustWidth);
		//Debug.Log("currentZoom=" + currentZoom + " currentHeight=" + currentHeight + " boundsHeight=" + boundsHeight + " zoomAdjust=" + zoomAdjust);
		//Debug.Log("Zoom adjust x=" + minX + "," + maxX + " y=" + minY + "," + maxY + " zoom=" + zoomAdjust);
		return zoomAdjust;
	}

	// UPDATE
	// Use the same update function as when your target moves for smooth following

	void Update()
	{
		if (!useFixedUpdate)
		{
			FollowUpdate();
			LookUpdate();
			ShakeUpdate();
			ZoomUpdate();
			firstFrame = false;
		}
	}

	void FixedUpdate()
	{
		if (useFixedUpdate)
		{
			FollowUpdate();
			LookUpdate();
			ShakeUpdate();
			ZoomUpdate();
			firstFrame = false;
		}
    }

	void FollowUpdate()
	{
        if ((followTarget != null) && (lookTarget == null))
        {
			float savedX = transform.position.x;
			float savedY = transform.position.y;
		    Vector3 targetPosition = BoundsTargetAdjust(followTarget.transform.position);
			if (lockX) targetPosition.x = savedX;
			if (lockY) targetPosition.y = savedY;
			transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref followVelocity, followTime, Mathf.Infinity, deltaTime);
			if (followRotation)
			{
				transform.rotation = Quaternion.RotateTowards(transform.rotation, followTarget.transform.rotation, followRotationSpeed * deltaTime);
			}
        }
	}

	void LookUpdate()
	{
		if (lookTarget != null)
		{
		    Vector3 targetPosition = BoundsTargetAdjust(lookTarget.transform.position, true);
			transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref lookVelocity, lookPanTime, Mathf.Infinity, deltaTime);
			lookDuration -= deltaTime;
			if (lookDuration <= 0)
			{
				lookTarget = null;
			}
		}
	}

	void ShakeUpdate()
	{
		if (shakeDuration > 0)
		{
			Vector3 centerPosition = transform.position - shakeOffset;
			shakeOffset = Random.insideUnitSphere * shakeAmount;
			transform.position = centerPosition + shakeOffset;

			shakeDuration -= deltaTime;
		}
	}

	void ZoomUpdate()
	{
		if (zoomPhase == 1)
		{
			// zoom to
			if (Mathf.Abs(zoomCurrentSize - zoomTargetSize) > 0.001f)
			{
				zoomCurrentSize = Mathf.MoveTowards(zoomCurrentSize, zoomTargetSize, zoomToIncrement);
				GetComponent<Camera>().orthographicSize = zoomCurrentSize;
				//Debug.Log("Zooming to currentSize=" + zoomCurrentSize + " targetSize=" + zoomTargetSize + " increment=" + zoomToIncrement);
			}
			else
			{
				zoomPhase = 2;
			}
		}
		else if (zoomPhase == 2)
		{
			// zoom hold
			if (zoomHoldTime > 0)
			{
				zoomHoldTime -= deltaTime;
			}
			else
			{
				zoomPhase = 3;
			}
		}
		else if (zoomPhase == 3)
		{
			// zoom back
			if (Mathf.Abs(zoomCurrentSize - zoomOriginalSize) > 0.001f)
			{
				zoomCurrentSize = Mathf.MoveTowards(zoomCurrentSize, zoomOriginalSize, zoomBackIncrement);
				GetComponent<Camera>().orthographicSize = zoomCurrentSize;
				//Debug.Log("Zooming back currentSize=" + zoomCurrentSize);
			}
			else
			{
				zoomPhase = 0;
			}
		}		
	}

}
