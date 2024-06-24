using System.Threading.Tasks;
using UnityEngine;
using Google.XR.ARCoreExtensions;
using UnityEngine.XR.ARFoundation;

public class CloudAnchorHosting : MonoBehaviour
{
    public ARAnchorManager anchorManager;
    public GameObject anchorPlacement;

    public async void HostCloudAnchor()
    {
        Debug.Log("HostCloudAnchor method called");
        var anchorPlacementScript = anchorPlacement.GetComponent<AnchorPlacement>();
        var anchor = anchorPlacementScript.GetCurrentAnchor();
        if (anchor != null)
        {
            Debug.Log("Hosting cloud anchor...");
            HostCloudAnchorResult result = await HostCloudAnchorAsync(anchor, 1);
            if (result != null && result.CloudAnchorState == CloudAnchorState.Success)
            {
                string cloudAnchorId = result.CloudAnchorId;
                Debug.Log("Cloud Anchor hosted successfully. ID: " + result.CloudAnchorId);
                PlayerPrefs.SetString("LastCloudAnchorID", cloudAnchorId);
                PlayerPrefs.Save();
            }
            else
            {
                Debug.LogError($"Failed to host Cloud Anchor. Error: {result.CloudAnchorState}");
                Debug.LogError($"Error Detail: {result}");
            }
        }
        else
        {
            Debug.LogError("No anchor to host.");
        }
    }

    private async Task<HostCloudAnchorResult> HostCloudAnchorAsync(ARAnchor anchor, int ttlDays)
    {
        var promise = anchorManager.HostCloudAnchorAsync(anchor, ttlDays);
        while (promise.State == PromiseState.Pending)
        {
            await Task.Yield();
        }
        return promise.Result;
    }
}