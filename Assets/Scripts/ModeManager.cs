using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModeManager : MonoBehaviour
{
    public PlaceObject objectPlacement;
    public ScaleObject objectScaler;

    public void EnablePlacementMode()
    {
        objectPlacement.isPlacementEnabled = true;
        objectScaler.isScalingEnabled = false;
    }

    public void EnableScalingMode()
    {
        objectPlacement.isPlacementEnabled = false;
        objectScaler.isScalingEnabled = true;
    }
}
