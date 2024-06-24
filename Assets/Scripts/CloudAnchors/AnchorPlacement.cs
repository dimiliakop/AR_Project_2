using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class AnchorPlacement : MonoBehaviour
{
    public GameObject anchorPrefab;
    private ARRaycastManager raycastManager;
    private ARAnchorManager anchorManager;
    private ARAnchor currentAnchor;
    private GameObject placedAnchor;

    private void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        anchorManager = GetComponent<ARAnchorManager>();

        if (raycastManager == null)
        {
            Debug.LogError("ARRaycastManager component is not found on the GameObject.");
        }

        if (anchorManager == null)
        {
            Debug.LogError("ARAnchorManager component is not found on the GameObject.");
        }
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 touchPosition = Input.mousePosition;
            PlaceAnchor(touchPosition);
        }
#elif UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                Vector2 touchPosition = touch.position;
                PlaceAnchor(touchPosition);
            }
        }
#endif
    }

    private void PlaceAnchor(Vector2 touchPosition)
    {
        if (raycastManager == null)
        {
            Debug.LogError("ARRaycastManager is not initialized.");
            return;
        }

        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        bool isHit = raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon);

        Debug.Log($"Raycast Hit: {isHit}, Hits Count: {hits.Count}");

        if (isHit && hits.Count > 0)
        {
            // Check if the hit trackable is an ARPlane
            ARRaycastHit? planeHit = null;
            foreach (var hit in hits)
            {
                if (hit.trackable is ARPlane)
                {
                    planeHit = hit;
                    break;
                }
            }

            if (planeHit.HasValue)
            {
                Pose hitPose = planeHit.Value.pose;
                ARPlane hitPlane = planeHit.Value.trackable as ARPlane;

                if (hitPlane != null)
                {
                    if (currentAnchor != null)
                    {
                        Destroy(currentAnchor.gameObject);
                    }

                    currentAnchor = anchorManager.AttachAnchor(hitPlane, hitPose);
                    if (currentAnchor != null)
                    {
                        if (placedAnchor != null)
                        {
                            Destroy(placedAnchor);
                        }
                        placedAnchor = Instantiate(anchorPrefab, currentAnchor.transform);
                        Debug.Log("Anchor placed successfully.");
                    }
                    else
                    {
                        Debug.LogError("Failed to create anchor.");
                    }
                }
            }
            else
            {
                Debug.Log("No ARPlane found at touch position.");
            }
        }
        else
        {
            Debug.Log("No plane found at touch position.");
        }
    }

    public ARAnchor GetCurrentAnchor()
    {
        return currentAnchor;
    }
}