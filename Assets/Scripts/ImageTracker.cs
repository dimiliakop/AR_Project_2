using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ImageTracker : MonoBehaviour
{
    private ARTrackedImageManager trackedImageManager;
    public GameObject[] arPrefabs; // Prefabs to instantiate

    private Dictionary<string, GameObject> instantiatedPrefabs = new Dictionary<string, GameObject>();

    void Awake()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            InstantiatePrefabForTrackedImage(trackedImage);
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            UpdatePrefabForTrackedImage(trackedImage);
        }

        foreach (var trackedImage in eventArgs.removed)
        {
            RemovePrefabForTrackedImage(trackedImage);
        }
    }

    private void InstantiatePrefabForTrackedImage(ARTrackedImage trackedImage)
    {
        if (!instantiatedPrefabs.ContainsKey(trackedImage.referenceImage.name))
        {
            foreach (var arPrefab in arPrefabs)
            {
                if (trackedImage.referenceImage.name == arPrefab.name)
                {
                    var newPrefab = Instantiate(arPrefab, trackedImage.transform.position, Quaternion.identity);
                    newPrefab.name = arPrefab.name; // Ensure the instantiated prefab has the same name

                    // Adjust the position to lay flat on the plane
                    AdjustPrefabPosition(newPrefab, trackedImage.transform.position, trackedImage.transform.rotation);

                    instantiatedPrefabs[trackedImage.referenceImage.name] = newPrefab;
                    break; // Exit the loop once the prefab is instantiated
                }
            }
        }
    }

    private void UpdatePrefabForTrackedImage(ARTrackedImage trackedImage)
    {
        if (instantiatedPrefabs.TryGetValue(trackedImage.referenceImage.name, out var prefab))
        {
            prefab.transform.position = trackedImage.transform.position;
            prefab.transform.rotation = trackedImage.transform.rotation;
            prefab.SetActive(trackedImage.trackingState == TrackingState.Tracking);
        }
    }

    private void RemovePrefabForTrackedImage(ARTrackedImage trackedImage)
    {
        if (instantiatedPrefabs.TryGetValue(trackedImage.referenceImage.name, out var prefab))
        {
            Destroy(prefab);
            instantiatedPrefabs.Remove(trackedImage.referenceImage.name);
        }
    }

    private void AdjustPrefabPosition(GameObject prefab, Vector3 planePosition, Quaternion planeRotation)
    {
        // Get the prefab's collider to determine its bounds
        Collider prefabCollider = prefab.GetComponent<Collider>();

        if (prefabCollider != null)
        {
            // Calculate the height of the prefab using bounds.size.y
            float prefabHeight = prefabCollider.bounds.size.y;

            // Adjust the position to lay the prefab flat on the plane
            Vector3 adjustedPosition = planePosition;
            adjustedPosition.y += prefabHeight / 2; // Adjust by half the height to lay it flat

            // Apply the adjusted position and rotation
            prefab.transform.position = adjustedPosition;
            prefab.transform.rotation = planeRotation;

            // Optional: Adjust rotation to lay flat on the plane (e.g., gun laying flat)
            prefab.transform.rotation = Quaternion.Euler(0, prefab.transform.rotation.eulerAngles.y, 0);
        }
        else
        {
            Debug.LogWarning("No Collider found on the prefab. Unable to adjust position.");
        }
    }
}
