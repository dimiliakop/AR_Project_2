using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using Google.XR.ARCoreExtensions;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;

public class CloudAnchorResolver : MonoBehaviour
{
    public ARAnchorManager anchorManager;
    public GameObject anchorPrefab;
    public ARPlaneManager arPlaneManager;

    private ARCloudAnchor resolvedAnchor;
    private string cloudAnchorId;

    private void Start()
    {
        if (arPlaneManager == null)
        {
            arPlaneManager = FindObjectOfType<ARPlaneManager>();
        }
        cloudAnchorId = PlayerPrefs.GetString("LastCloudAnchorID", null);
        if (!string.IsNullOrEmpty(cloudAnchorId))
        {
            Debug.Log("Cloud Anchor ID found: " + cloudAnchorId);
        }
        else
        {
            Debug.Log("No Cloud Anchor ID found.");
        }
    }

    public async void ResolveCloudAnchor()
    {
        Debug.Log("Resolving Cloud Anchor...");
        if (anchorManager == null)
        {
            Debug.LogError("ARAnchorManager is not assigned.");
            return;
        }

        ResolveCloudAnchorResult result = await ResolveCloudAnchorAsync(cloudAnchorId);

        if (result != null && result.CloudAnchorState == CloudAnchorState.Success)
        {
            resolvedAnchor = result.Anchor;
            Debug.Log("Cloud Anchor resolved successfully. Pose: " + resolvedAnchor.transform.position);

            Instantiate(anchorPrefab, resolvedAnchor.transform.position, resolvedAnchor.transform.rotation);
            
        }
        else
        {
            Debug.LogError("Failed to resolve Cloud Anchor. Error: " + (result != null ? result.CloudAnchorState.ToString() : "Unknown"));
        }
    }

    private async Task<ResolveCloudAnchorResult> ResolveCloudAnchorAsync(string cloudAnchorId)
    {
        Debug.Log("Inside ResolveCloudAnchorAsync...");
        if (string.IsNullOrEmpty(cloudAnchorId))
        {
            Debug.LogError("CloudAnchorId is null or empty.");
            return null;
        }

        var promise = anchorManager.ResolveCloudAnchorAsync(cloudAnchorId);

        while (promise.State == PromiseState.Pending)
        {
            await Task.Yield();
        }

        Debug.Log("Promise state: " + promise.State);
        return promise.Result;
    }
}