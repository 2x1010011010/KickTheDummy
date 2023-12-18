using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BFX_BloodSettings : MonoBehaviour
{
    public float AnimationSpeed = 1;
    public bool AutomaticGroundHeightDetection = true;
    public float GroundHeight = 0;
    [Range(0, 1)]
    public float LightIntensityMultiplier = 1;
    [Range(15, 1000)]
    public float DecalLifeTimeSeconds = 30;
    public DecalRenderingModeEnum DecalRenderingMode        = DecalRenderingModeEnum.HorizontalSurfacesOnly;
    //public bool               ClampDecalSideSurface    = false;
    [Range(0, 15)]
    public float              DebugAnimationTime                = 0.25f;

    public enum DecalRenderingModeEnum
    {
        HorizontalSurfacesOnly,
        DiagonalSurfaces
    }
}
