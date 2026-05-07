using System;
using UnityEngine;

[Serializable]
public struct TireFrictionParams
{
    public float extremumSlip;
    public float extremumValue;
    public float asymptoteSlip;
    public float stiffness;

    public static TireFrictionParams Default => new TireFrictionParams
    {
        extremumSlip   = 0.4f,
        extremumValue  = 1.0f,
        asymptoteSlip  = 0.8f,
        stiffness      = 1.0f
    };
}

[CreateAssetMenu(fileName = "TireMaterial_Parametric", menuName = "Bycicle/TireMaterial/Parametric")]
public class BycicleTireMaterial_Parametric : BycicleTireMaterial_Base
{
    [SerializeField]
    TireFrictionParams LongitudinalGripLimitCurve = TireFrictionParams.Default;

    [SerializeField]
    TireFrictionParams LateralGripLimitCurve = TireFrictionParams.Default;

    public override float GetGripLimit(PhysicsMaterial groundMaterial, float slip, float downImpulse, GripDirection direction)
    {
        TireFrictionParams curve = direction switch
        {
            GripDirection.Lateral => LateralGripLimitCurve,
            GripDirection.Longitudinal => LongitudinalGripLimitCurve,
            _ => TireFrictionParams.Default
        };

        float staticGrip = direction switch
        {
            GripDirection.Lateral => LateralStaticGrip,
            GripDirection.Longitudinal => LongitudinalStaticGrip,
            _ => 0f
        };

        return Mathf.Abs(evaluateGripLimit(groundMaterial, slip, downImpulse, staticGrip, curve));
    }
    
    private static float evaluateGripLimit(PhysicsMaterial groundMaterial, float slip, float downForce, float staticGrip, TireFrictionParams frictionParams)
    {
        float factor;
        if (slip < frictionParams.extremumSlip)
        {
            float localT =  Mathf.InverseLerp(.0f, frictionParams.extremumSlip, slip);
            factor = squareInterp(.0f, frictionParams.extremumSlip, frictionParams.stiffness, localT);
        }
        else if (slip < frictionParams.asymptoteSlip)
        {
            float localT = Mathf.InverseLerp(frictionParams.extremumSlip, frictionParams.asymptoteSlip, slip);
            factor = squareInterp(frictionParams.extremumSlip, frictionParams.extremumSlip, frictionParams.stiffness, 1 - localT);
        }
        else
        {
            factor = groundMaterial.dynamicFriction;
        }

        return downForce * staticGrip * factor;
    }

    private static float squareInterp(float a, float b, float tan, float t)
    {
        float linearT = ((1 - tan) * t + tan) * t;
        return Mathf.Lerp(a, b, linearT);
    }
}