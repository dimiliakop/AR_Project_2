using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleObject : MonoBehaviour
{
    public bool isScalingEnabled = false; // Toggle for enabling/disabling scaling
    private GameObject selectedObject;
    private float initialDistance;
    private Vector3 initialScale;
    private float scaleFactor;

    void Update()
    {
        if (!isScalingEnabled)
            return;

        #if UNITY_EDITOR // Check if running in the Unity Editor
        HandleMouseScaling();
        #else
        HandleTouchScaling();
        #endif
    }

    private void HandleMouseScaling()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse button click to select object
        {
            selectedObject = GetTouchedObject(Input.mousePosition);
            if (selectedObject != null)
            {
                initialScale = selectedObject.transform.localScale;
                Debug.Log($"Selected object: {selectedObject.name} with initial scale {initialScale}");
            }
        }

        if (selectedObject != null && Input.mouseScrollDelta.y != 0) // Mouse wheel input
        {
            scaleFactor = 1 + Input.mouseScrollDelta.y * 0.1f; // Adjust scaling sensitivity
            selectedObject.transform.localScale = initialScale * scaleFactor;
            Debug.Log($"Scaling object: {selectedObject.name} to scale {selectedObject.transform.localScale}");
        }
    }

    private void HandleTouchScaling()
    {
        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            if (touchZero.phase == TouchPhase.Began || touchOne.phase == TouchPhase.Began)
            {
                selectedObject = GetTouchedObject(touchZero.position);
                if (selectedObject != null)
                {
                    initialDistance = Vector2.Distance(touchZero.position, touchOne.position);
                    initialScale = selectedObject.transform.localScale;
                    Debug.Log($"Selected object: {selectedObject.name} with initial scale {initialScale} and initial distance {initialDistance}");
                }
            }

            if (selectedObject != null && (touchZero.phase == TouchPhase.Moved || touchOne.phase == TouchPhase.Moved))
            {
                float currentDistance = Vector2.Distance(touchZero.position, touchOne.position);
                scaleFactor = currentDistance / initialDistance;
                selectedObject.transform.localScale = initialScale * scaleFactor;
                Debug.Log($"Scaling object: {selectedObject.name} to scale {selectedObject.transform.localScale}");
            }
        }
    }

    private GameObject GetTouchedObject(Vector2 touchPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(touchPosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log($"Touched object: {hit.collider.gameObject.name}");
            return hit.collider.gameObject;
        }
        return null;
    }
}
