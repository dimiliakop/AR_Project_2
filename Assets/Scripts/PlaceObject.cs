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
        if (isPlacementEnabled && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                touchPosition = touch.position;

                if (arRaycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
                {
                    Pose hitPose = hits[0].pose;
                    GameObject placedObject = Instantiate(placementIndicatorPrefab, hitPose.position, hitPose.rotation);
                    placedObject.tag = "PlacedObject"; // Tag the placed object
                    EnsureRigidbodyAndCollider(placedObject);
                }
            }
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
