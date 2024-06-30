using Google.XR.ARCoreExtensions;
using System.Collections;
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
    public TextMeshProUGUI winText; // TextMeshProUGUI for the win text

    public GameObject throwableSpherePrefab;
    public GameObject muzzleFlashPrefab; // Muzzle flash effect prefab
    public float throwForce = 10f;

    private static int ObjectHitScore = 0;

    private Color enabledColor = Color.green;
    private Color disabledColor = Color.gray;

    public ARAnchorManager anchorManager;
    public GameObject anchorPrefab;

    private ARCloudAnchor resolvedAnchor;
    private string cloudAnchorId;

    // FallDetection prefab reference
    public GameObject fallDetectionPrefab;

    void Start()
    {
        DisableAllModes();
        ShowInitialUI();
        winText.gameObject.SetActive(false); // Ensure win text is hidden initially
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

        AdjustFallDetectionArea();
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
        arPlaneManager.enabled = false;

        initialUIPanel.SetActive(false);
        gameUIPanel.SetActive(true);
        SetPlaneVisualization(true);
        objectScaler.isScalingEnabled = false;
        objectPlacement.isPlacementEnabled = false;

        List<PlacedObjectData> placedObjects = objectPlacement.GetPlacedObjects();

        Debug.Log($"Number of placed objects: {placedObjects.Count}");

        foreach (var placedObjectData in placedObjects)
        {
            AdjustObjectPosition(placedObjectData.placedObject, placedObjectData.planePosition);
        }
    }

    private void AdjustObjectPosition(GameObject obj, Vector3 planePosition)
    {
        Collider objCollider = obj.GetComponent<Collider>();

        if (objCollider != null)
        {
            float objectHeight = objCollider.bounds.size.y / 2; // Use size.y / 2 to get the half height
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
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; // Use continuous collision detection
        rb.AddForce(arCamera.transform.forward * throwForce, ForceMode.Impulse);

        // Ensure the sphere has a collider
        if (sphere.GetComponent<Collider>() == null)
        {
            sphere.AddComponent<SphereCollider>();
        }

        // Ensure the sphere has the ThrowableSphere component
        ThrowableSphere throwableSphere = sphere.GetComponent<ThrowableSphere>();
        if (throwableSphere == null)
        {
            throwableSphere = sphere.AddComponent<ThrowableSphere>();
        }
        throwableSphere.modeManager = this;

        // Play the throw sound
        AudioSource audioSource = sphere.GetComponent<AudioSource>();
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }

        // Instantiate the muzzle flash effect
        if (muzzleFlashPrefab != null)
        {
            Instantiate(muzzleFlashPrefab, arCamera.transform.position, arCamera.transform.rotation);
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

    public void IncrementScoreObjectHit()
    {
        ObjectHitScore++;
        UpdateScoreTextObjectHit();
    }

    public void CheckForWin()
    {
        Debug.Log("Checking for win condition...");
        List<PlacedObjectData> placedObjects = objectPlacement.GetPlacedObjects();
        bool anyObjectLeft = false;

        foreach (var placedObjectData in placedObjects)
        {
            if (placedObjectData.placedObject != null)
            {
                anyObjectLeft = true;
                Debug.Log($"Object {placedObjectData.placedObject.name} still exists at position {placedObjectData.placedObject.transform.position}");
            }
        }

        if (!anyObjectLeft)
        {
            winText.text = "You Win!";
            winText.gameObject.SetActive(true);
            StartCoroutine(ExitGameAfterDelay(3f));
        }
    }

    private IEnumerator ExitGameAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        EndGame();
    }

    private void AdjustFallDetectionArea()
    {
        if (fallDetectionPrefab == null) return;

        // Create FallDetection GameObject if not already present
        GameObject fallDetectionObject = Instantiate(fallDetectionPrefab);
        fallDetectionObject.name = "FallDetection";

        Vector3 center = Vector3.zero;
        float maxX = float.MinValue, minX = float.MaxValue;
        float maxZ = float.MinValue, minZ = float.MaxValue;

        foreach (var plane in arPlaneManager.trackables)
        {
            if (plane == null) continue;

            center += plane.transform.position;

            foreach (Vector3 vertex in plane.boundary)
            {
                Vector3 worldVertex = plane.transform.TransformPoint(vertex);
                if (worldVertex.x > maxX) maxX = worldVertex.x;
                if (worldVertex.x < minX) minX = worldVertex.x;
                if (worldVertex.z > maxZ) maxZ = worldVertex.z;
                if (worldVertex.z < minZ) minZ = worldVertex.z;
            }
        }

        center /= arPlaneManager.trackables.count;
        float sizeX = maxX - minX;
        float sizeZ = maxZ - minZ;

        fallDetectionObject.transform.position = new Vector3(center.x, center.y - 1f, center.z);
        fallDetectionObject.transform.localScale = new Vector3(sizeX, 1, sizeZ);

        BoxCollider boxCollider = fallDetectionObject.GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            boxCollider.isTrigger = true;
        }

        FallDetection fallDetection = fallDetectionObject.GetComponent<FallDetection>();
        if (fallDetection != null)
        {
            fallDetection.modeManager = this;
        }
    }
}
public class ThrowableSphere : MonoBehaviour
{
    public ModeManager modeManager;
    private bool hasCollided = false;
    public float collisionEffectForce = 5f;
    public float destructionDelay = 0.5f;

    public GameObject collisionEffectPrefab;
    public AudioClip collisionSound;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision detected with: " + collision.gameObject.name);

        if (!hasCollided)
        {
            hasCollided = true;

            if (collision.gameObject.CompareTag("PlacedObject"))
            {
                modeManager.IncrementScoreObjectHit();

                Rigidbody objectRb = collision.gameObject.GetComponent<Rigidbody>();
                if (objectRb != null)
                {
                    objectRb.isKinematic = false;
                    objectRb.useGravity = true;
                    Vector3 forceDirection = collision.contacts[0].point - transform.position;
                    forceDirection = forceDirection.normalized;
                    objectRb.AddForce(forceDirection * collisionEffectForce, ForceMode.Impulse);
                    Debug.Log("Force applied to object: " + forceDirection * collisionEffectForce);
                }
            }

            if (collisionEffectPrefab != null)
            {
                Instantiate(collisionEffectPrefab, collision.contacts[0].point, Quaternion.identity);
                Debug.Log("Particle effect instantiated at: " + collision.contacts[0].point);
            }

            if (collisionSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(collisionSound);
                Debug.Log("Collision sound played: " + collisionSound.name);
            }

            Destroy(gameObject, destructionDelay);
        }
    }
}
