using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlaceObject : MonoBehaviour
{
    public GameObject placementIndicatorPrefab; // The prefab to be placed
    public ARRaycastManager arRaycastManager; // AR Raycast Manager
    public ARPlaneManager arPlaneManager; // AR Plane Manager

    private Vector2 touchPosition; // Touch position
    private List<ARRaycastHit> hits = new List<ARRaycastHit>(); // List to store raycast hits
    public bool isPlacementEnabled = true; // Toggle for enabling/disabling placement

    private List<PlacedObjectData> placedObjects = new List<PlacedObjectData>(); // List to store placed objects and their plane positions

    void Start()
    {
        if (arRaycastManager == null)
        {
            arRaycastManager = FindObjectOfType<ARRaycastManager>();
        }

        if (arPlaneManager == null)
        {
            arPlaneManager = FindObjectOfType<ARPlaneManager>();
        }

        // Enable plane detection and visualization
        if (arPlaneManager != null)
        {
            arPlaneManager.enabled = true;
            foreach (var plane in arPlaneManager.trackables)
            {
                plane.gameObject.SetActive(true);
            }
        }
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

            // Ensure Rigidbody and Collider are properly configured before adjusting position
            EnsureRigidbodyAndCollider(placedObject);

            // Adjust the position to place the object on the plane
            AdjustObjectPosition(placedObject, hitPose.position);

            // Store the placed object and its plane position
            placedObjects.Add(new PlacedObjectData(placedObject, hitPose.position));

            // Adjust the rotation to face the camera
            AdjustObjectRotation(placedObject);
        }
    }

    private void AdjustObjectPosition(GameObject obj, Vector3 planePosition)
    {
        // Get the object's collider to determine its bounds
        Collider objCollider = obj.GetComponent<Collider>();

        if (objCollider != null)
        {
            // Adjust the position to place the object exactly on the plane
            Vector3 adjustedPosition = planePosition;
            adjustedPosition.y += objCollider.bounds.extents.y; // Move up by half the height of the collider

            // Apply the adjusted position
            obj.transform.position = adjustedPosition;
        }
        else
        {
            Debug.LogWarning("No Collider found on the placed object. Unable to adjust position.");
        }
    }

    private void AdjustObjectRotation(GameObject obj)
    {
        Camera arCamera = Camera.main;
        Vector3 directionToCamera = arCamera.transform.position - obj.transform.position;
        directionToCamera.y = 0; // Keep the rotation only on the y-axis
        Quaternion lookRotation = Quaternion.LookRotation(directionToCamera);
        obj.transform.rotation = lookRotation;
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

    // Method to get the list of placed objects and their plane positions
    public List<PlacedObjectData> GetPlacedObjects()
    {
        return placedObjects;
    }
}

// Class to store placed object and its corresponding plane position
[System.Serializable]
public class PlacedObjectData
{
    public GameObject placedObject;
    public Vector3 planePosition;

    public PlacedObjectData(GameObject placedObject, Vector3 planePosition)
    {
        this.placedObject = placedObject;
        this.planePosition = planePosition;
    }
}
