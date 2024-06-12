using System.Collections.Generic;
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

    private int score = 0;

    private Color enabledColor = Color.green;
    private Color disabledColor = Color.gray;
    public Text objectCountText; // UI Text element to display the object count

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
        SetPlaneVisualization(false);

        placementModeButton.image.color = disabledColor;
        scalingModeButton.image.color = disabledColor;
    }

    private void ShowInitialUI()
    {
        initialUIPanel.SetActive(true);
        gameUIPanel.SetActive(false);
    }

    public void StartGame()
    {
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

        arPlaneManager.enabled = true; // Enable plane detection
        SetPlaneVisualization(true); // Show existing planes

        placementModeButton.image.color = enabledColor;
        scalingModeButton.image.color = disabledColor;
    }

    public void EnableScalingMode()
    {
        objectPlacement.isPlacementEnabled = false;
        objectScaler.isScalingEnabled = true;

        arPlaneManager.enabled = false; // Disable plane detection
        SetPlaneVisualization(false); // Hide existing planes

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
        initialUIPanel.SetActive(false);
        gameUIPanel.SetActive(true);
        SetPlaneVisualization(true); // Show existing planes
        objectScaler.isScalingEnabled = false;
        objectPlacement.isPlacementEnabled = false;

        // Get the list of placed objects and their plane positions
        List<PlacedObjectData> placedObjects = objectPlacement.GetPlacedObjects();

        // Print the number of placed objects
        Debug.Log($"Number of placed objects: {placedObjects.Count}");

        // Adjust the position of each placed object to ensure it's on top of the plane
        foreach (var placedObjectData in placedObjects)
        {
            AdjustObjectPosition(placedObjectData.placedObject, placedObjectData.planePosition);
        }

        objectCountText.text = $"{placedObjects.Count}";
    }

    private void AdjustObjectPosition(GameObject obj, Vector3 planePosition)
    {
        // Get the object's collider to determine its bounds
        Collider objCollider = obj.GetComponent<Collider>();

        if (objCollider != null)
        {
            // Calculate the adjustment needed to place the object on the plane
            float objectHeight = objCollider.bounds.extents.y;
            Vector3 adjustedPosition = planePosition;
            adjustedPosition.y += objectHeight;

            // Apply the adjusted position
            obj.transform.position = adjustedPosition;
        }
        else
        {
            Debug.LogWarning("No Collider found on the placed object. Unable to adjust position.");
        }
    }

    private void UpdateScoreText()
    {
        objectHitScoreText.text = $"{score}";
    }

    public void ThrowObject()
    {
        Camera arCamera = Camera.main;
        GameObject sphere = Instantiate(throwableSpherePrefab, arCamera.transform.position, arCamera.transform.rotation);
        Rigidbody rb = sphere.GetComponent<Rigidbody>();
        rb.isKinematic = false; // Ensure the Rigidbody is non-kinematic
        rb.AddForce(arCamera.transform.forward * throwForce, ForceMode.Impulse);

        sphere.AddComponent<SphereCollider>();
        sphere.AddComponent<ThrowableSphere>().modeManager = this;
    }

    public void IncrementScore()
    {
        score++;
        UpdateScoreText();
    }
}

public class ThrowableSphere : MonoBehaviour
{
    public ModeManager modeManager;
    private bool hasCollided = false; // Flag to ensure single collision handling

    void OnCollisionEnter(Collision collision)
    {
        if (!hasCollided && collision.gameObject.CompareTag("PlacedObject"))
        {
            hasCollided = true; // Set the flag to true
            modeManager.IncrementScore();
            // Destroy(gameObject);
        }
    }
}
