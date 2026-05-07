using UnityEngine;

public enum GripDirection
{
    Longitudinal,
    Lateral
}

public class BycicleTireMaterial_Base : ScriptableObject
{
    public float LateralStaticGrip = 1.0f;
    public float LongitudinalStaticGrip = 1.0f;

    public virtual float GetGripLimit(PhysicsMaterial groundMaterial, float slip, float downImpulse, GripDirection direction)
    {
        return 0f;
    }
}