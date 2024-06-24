using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using Google.XR.ARCoreExtensions;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;

public class CloudAnchorResolver : MonoBehaviour
{
    public ARAnchorManager anchorManager;
    public GameObject anchorPrefab; // The prefab to instantiate at the resolved anchor

    private ARCloudAnchor resolvedAnchor;
    private string cloudAnchorId;

    private void Start()
    {
        cloudAnchorId = PlayerPrefs.GetString("LastCloudAnchorID", null);
        if (!string.IsNullOrEmpty(cloudAnchorId))
        {
            Debug.Log("Cloud Anchor ID found.");
        }
        else
        {
            Debug.Log("No Cloud Anchor ID found.");
        }
    }

    public async void ResolveCloudAnchor()
    {
        Debug.Log("Resolving Cloud Anchor...");
        ResolveCloudAnchorResult result = await ResolveCloudAnchorAsync(cloudAnchorId);

        if (result != null && result.CloudAnchorState == CloudAnchorState.Success)
        {
            resolvedAnchor = result.Anchor;
            Debug.Log("Cloud Anchor resolved successfully.");
            Debug.Log("Cloud Anchor resolved successfully. Pose: " + resolvedAnchor.transform.position);

            // Instantiate the prefab at the resolved anchor's position and rotation
            Instantiate(anchorPrefab, resolvedAnchor.transform.position, resolvedAnchor.transform.rotation);
        }
        else
        {
            Debug.Log("Failed to resolve Cloud Anchor. Error: " + (result != null ? result.CloudAnchorState.ToString() : "Unknown"));
            Debug.LogError("Failed to resolve Cloud Anchor. Error: " + (result != null ? result.CloudAnchorState.ToString() : "Unknown"));
        }
    }

    private Task<ResolveCloudAnchorResult> ResolveCloudAnchorAsync(string cloudAnchorId)
    {
        var tcs = new TaskCompletionSource<ResolveCloudAnchorResult>();
        StartCoroutine(ResolveCloudAnchorCoroutine(cloudAnchorId, tcs));
        return tcs.Task;
    }

    private IEnumerator ResolveCloudAnchorCoroutine(string cloudAnchorId, TaskCompletionSource<ResolveCloudAnchorResult> tcs)
    {
        var promise = anchorManager.ResolveCloudAnchorAsync(cloudAnchorId);
        yield return promise;

        if (promise.State == PromiseState.Done)
        {
            tcs.SetResult(promise.Result);
        }
        else
        {
            tcs.SetResult(null);
        }
    }
}