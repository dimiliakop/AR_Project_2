using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleObject : MonoBehaviour
{
    public bool isScalingEnabled = false; // Toggle for enabling/disabling scaling
    private GameObject selectedObject;
    private float initialDistance;
    private Vector3 initialScale;

    void Update()
    {
        if (isScalingEnabled && Input.touchCount == 2)
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
                }
            }

            if (selectedObject != null && (touchZero.phase == TouchPhase.Moved || touchOne.phase == TouchPhase.Moved))
            {
                float currentDistance = Vector2.Distance(touchZero.position, touchOne.position);
                float scaleFactor = currentDistance / initialDistance;
                selectedObject.transform.localScale = initialScale * scaleFactor;
            }
        }
    }

    private GameObject GetTouchedObject(Vector2 touchPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(touchPosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            return hit.collider.gameObject;
        }
        return null;
    }
}
