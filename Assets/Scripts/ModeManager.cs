using Google.XR.ARCoreExtensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;


public class ModeManager : MonoBehaviour
{
    public PlaceObject objectPlacement;
    public ScaleObject objectScaler;
    public ARPlaneManager arPlaneManager;

    public Button startGameButton;
    public Button placementModeButton;
    public Button scalingModeButton;
    public Button throwObjectButton;
    public Button endGameButton;

    public GameObject initialUIPanel;
    public GameObject gameUIPanel;

    public TextMeshProUGUI objectHitScoreText;
    public TextMeshProUGUI imagesHitScoreText;

    public GameObject throwableSpherePrefab;
    public float throwForce = 10f;

    private static int ObjectHitScore = 0;

    private Color enabledColor = Color.green;
    private Color disabledColor = Color.gray;
    //public Text objectCountText; // UI Text element to display the object count

    public ARAnchorManager anchorManager;
    public GameObject anchorPrefab;

    private ARCloudAnchor resolvedAnchor;
    private string cloudAnchorId;

    void Start()
    {
        DisableAllModes();
        ShowInitialUI();
    }

    private void DisableAllModes()
    {
        objectPlacement.isPlacementEnabled = false;
        objectScaler.isScalingEnabled = false;
        arPlaneManager.enabled = false;
        RemoveAllPlanes();
        SetPlaneVisualization(false);

        placementModeButton.image.color = disabledColor;
        scalingModeButton.image.color = disabledColor;
    }

    public void RemoveAllPlanes()
    {
        if (arPlaneManager != null)
        {
            // Iterate over the tracked planes and destroy their game objects
            foreach (var plane in arPlaneManager.trackables)
            {
                Destroy(plane.gameObject);
            }
        }
    }

    private void ShowInitialUI()
    {
        initialUIPanel.SetActive(true);
        gameUIPanel.SetActive(false);
    }

    public void StartGame()
    {
        if (objectPlacement.GetPlacedObjects().Count == 0)
        {
            Debug.LogWarning("Cannot start the game with 0 placed objects.");
            return; 
        }

        ShowGameUI();
    }

    public void EndGame()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    public void EnablePlacementMode()
    {
        objectPlacement.isPlacementEnabled = true;
        objectScaler.isScalingEnabled = false;

        arPlaneManager.enabled = true; 
        SetPlaneVisualization(true); 

        placementModeButton.image.color = enabledColor;
        scalingModeButton.image.color = disabledColor;
    }

    public void EnableScalingMode()
    {
        objectPlacement.isPlacementEnabled = false;
        objectScaler.isScalingEnabled = true;

        arPlaneManager.enabled = false; 
        SetPlaneVisualization(false); 

        placementModeButton.image.color = disabledColor;
        scalingModeButton.image.color = enabledColor;
    }

    private void SetPlaneVisualization(bool isVisible)
    {
        foreach (var plane in arPlaneManager.trackables)
        {
            plane.gameObject.SetActive(isVisible);
        }
    }

    private void ShowGameUI()
    {
        cloudAnchorId = PlayerPrefs.GetString("LastCloudAnchorID", null);
        if (!string.IsNullOrEmpty(cloudAnchorId))
        {
            Debug.Log("Cloud Anchor ID found: " + cloudAnchorId);
        }
        else
        {
            Debug.Log("No Cloud Anchor ID found.");
        }

        initialUIPanel.SetActive(false);
        gameUIPanel.SetActive(true);
        SetPlaneVisualization(true); // Show existing planes
        objectScaler.isScalingEnabled = false;
        objectPlacement.isPlacementEnabled = false;

        List<PlacedObjectData> placedObjects = objectPlacement.GetPlacedObjects();

        Debug.Log($"Number of placed objects: {placedObjects.Count}");

        foreach (var placedObjectData in placedObjects)
        {
            AdjustObjectPosition(placedObjectData.placedObject, placedObjectData.planePosition);
        }

        //objectCountText.text = $"{placedObjects.Count}";
    }

    private void AdjustObjectPosition(GameObject obj, Vector3 planePosition)
    {
        Collider objCollider = obj.GetComponent<Collider>();

        if (objCollider != null)
        {
            float objectHeight = objCollider.bounds.extents.y;
            Vector3 adjustedPosition = planePosition;
            adjustedPosition.y += objectHeight;

            obj.transform.position = adjustedPosition;
        }
        else
        {
            Debug.LogWarning("No Collider found on the placed object. Unable to adjust position.");
        }
    }

    private void UpdateScoreTextObjectHit()
    {
        objectHitScoreText.text = $"{ObjectHitScore}";
    }

    public void ThrowObject()
    {
        Camera arCamera = Camera.main;
        GameObject sphere = Instantiate(throwableSpherePrefab, arCamera.transform.position, arCamera.transform.rotation);
        Rigidbody rb = sphere.GetComponent<Rigidbody>();
        rb.isKinematic = false; 
        rb.AddForce(arCamera.transform.forward * throwForce, ForceMode.Impulse);

        sphere.AddComponent<SphereCollider>();
        sphere.AddComponent<ThrowableSphere>().modeManager = this;
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

    public void IncrementScoreObjectHit()
    {
        ObjectHitScore++;
        UpdateScoreTextObjectHit();
    }
}



public class ThrowableSphere : MonoBehaviour
{
    public ModeManager modeManager;
    private bool hasCollided = false; 

    void OnCollisionEnter(Collision collision)
    {
        if (!hasCollided && collision.gameObject.CompareTag("PlacedObject"))
        {
            hasCollided = true; 
            modeManager.IncrementScoreObjectHit();
            // Destroy(gameObject);
        }
    }
}
