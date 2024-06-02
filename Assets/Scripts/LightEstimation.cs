using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Rendering;

[RequireComponent(typeof(Light))]
public class LightEstimation : MonoBehaviour
{
    private Light arLight;
    private ARCameraManager arCameraManager;

    void Awake()
    {
        arLight = GetComponent<Light>();
        arCameraManager = FindObjectOfType<ARCameraManager>();
    }

    void OnEnable()
    {
        arCameraManager.frameReceived += FrameChanged;
    }

    void OnDisable()
    {
        arCameraManager.frameReceived -= FrameChanged;
    }

    private void FrameChanged(ARCameraFrameEventArgs args)
    {
        if (args.lightEstimation.averageBrightness.HasValue)
        {
            arLight.intensity = args.lightEstimation.averageBrightness.Value;
        }

        if (args.lightEstimation.colorCorrection.HasValue)
        {
            arLight.color = args.lightEstimation.colorCorrection.Value;
        }

        if (args.lightEstimation.mainLightDirection.HasValue)
        {
            arLight.transform.rotation = Quaternion.LookRotation(args.lightEstimation.mainLightDirection.Value);
        }

        if (args.lightEstimation.mainLightIntensityLumens.HasValue)
        {
            arLight.intensity = args.lightEstimation.mainLightIntensityLumens.Value;
        }

        if (args.lightEstimation.ambientSphericalHarmonics.HasValue)
        {
            RenderSettings.ambientMode = AmbientMode.Skybox;
            RenderSettings.ambientProbe = args.lightEstimation.ambientSphericalHarmonics.Value;
        }
    }
}
