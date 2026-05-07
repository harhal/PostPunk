using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "TireMaterial_Curve", menuName = "Bycicle/TireMaterial/Curve")]
public class BycicleTireMaterial_Curve : BycicleTireMaterial_Base
{
    [SerializeField]
    AnimationCurve LongitudinalGripLimitCurve = AnimationCurve.EaseInOut(.0f, .0f, 1.0f, 1.0f);

    [SerializeField]
    AnimationCurve LateralGripLimitCurve = AnimationCurve.EaseInOut(.0f, .0f, 1.0f, 1.0f);

    public override float GetGripLimit(PhysicsMaterial groundMaterial, float slip, float downImpulse, GripDirection direction)
    {
        AnimationCurve curve = direction switch
        {
            GripDirection.Lateral => LateralGripLimitCurve,
            GripDirection.Longitudinal => LongitudinalGripLimitCurve,
            _ => null
        };

        float staticGrip = direction switch
        {
            GripDirection.Lateral => LateralStaticGrip,
            GripDirection.Longitudinal => LongitudinalStaticGrip,
            _ => 0f
        };

        return Mathf.Abs(downImpulse * staticGrip * 
            Mathf.Lerp(curve.Evaluate(slip), isTangentNegative(curve, slip) ? groundMaterial.dynamicFriction : groundMaterial.dynamicFriction, groundMaterial.staticFriction));
    }

    private static bool isTangentNegative(AnimationCurve curve, float t)
    {
        foreach (Keyframe frame in curve.keys)
        {
            if (frame.time >= t)
            {
                return frame.inTangent > 0f;
            }
        }

        return true;
    }
}
