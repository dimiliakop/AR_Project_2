using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlaceObject : MonoBehaviour
{
    public GameObject placementIndicatorPrefab; // The prefab to be placed
    private ARRaycastManager arRaycastManager; // AR Raycast Manager
    private Vector2 touchPosition; // Touch position
    private List<ARRaycastHit> hits = new List<ARRaycastHit>(); // List to store raycast hits
    public bool isPlacementEnabled = true; // Toggle for enabling/disabling placement

    void Start()
    {
        arRaycastManager = FindObjectOfType<ARRaycastManager>();
    }

    void Update()
    {
        if (isPlacementEnabled)
        {
            #if UNITY_EDITOR // Check if running in the Unity Editor
            if (Input.GetMouseButtonDown(0)) // Left mouse button click
            {
                touchPosition = Input.mousePosition;
                TryPlaceObject();
            }
            #else
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    touchPosition = touch.position;
                    TryPlaceObject();
                }
            }
            #endif
        }
    }

    private void TryPlaceObject()
    {
        if (arRaycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            GameObject placedObject = Instantiate(placementIndicatorPrefab, hitPose.position, hitPose.rotation);
            placedObject.tag = "PlacedObject"; // Tag the placed object

            // Adjust the position to place the object on the plane
            AdjustObjectPosition(placedObject, hitPose.position);

            EnsureRigidbodyAndCollider(placedObject);
        }
    }

    private void AdjustObjectPosition(GameObject obj, Vector3 planePosition)
    {
        // Get the object's collider to determine its bounds
        Collider objCollider = obj.GetComponent<Collider>();

        if (objCollider != null)
        {
            // Calculate the adjustment needed to place the object on the plane
            float objectHeight = objCollider.bounds.size.y;
            Vector3 adjustedPosition = planePosition;
            adjustedPosition.y += objectHeight / 2;

            // Apply the adjusted position
            obj.transform.position = adjustedPosition;
        }
        else
        {
            Debug.LogWarning("No Collider found on the placed object. Unable to adjust position.");
        }
    }


    private void EnsureRigidbodyAndCollider(GameObject obj)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = obj.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true; // Make Rigidbody kinematic
        rb.useGravity = false; // Ensure gravity is disabled

        if (obj.GetComponent<Collider>() == null)
        {
            obj.AddComponent<BoxCollider>(); // Add a default BoxCollider if none exists
        }
    }
}
