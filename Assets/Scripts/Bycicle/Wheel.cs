using System;
using UnityEngine;

[Serializable]
public class BycicleWheel : MonoBehaviour
{
    [field: SerializeField]
    public VirtualWheel virtualWheel {get; protected set;}

    [SerializeField]
    public BycicleTireMaterial_Base tireMaterial;

    [SerializeField]
    public PhysicsMaterial groundMaterial;

    public float DisplayAngSpeed => virtualWheel.DisplayAngSpeed;

    public float LinearVelocity => virtualWheel.LinearVelocity;

    public float LinearImpulse => virtualWheel.LinearImpulse;
    public float AngSpeed => virtualWheel.AngSpeedDeg;

    public float LinearInertia => virtualWheel.LinearInertia;

    public float Inertia => virtualWheel.Inertia;

    public float Angle => virtualWheel.AngleDeg;

    public float Radius => virtualWheel.Radius;

    public void ApplyForce(float force, float deltaTime, ForceMode forceMode, float lever = 1.0f, float sin = 1.0f)
    {
        virtualWheel.ApplyForce(force, deltaTime, forceMode, lever, sin);
    }

    public void ApplyTorque(float torque, float deltaTime, AngForceMode forceMode)
    {        
        virtualWheel.ApplyTorque(torque, deltaTime, forceMode);
    }

    public void ApplyLossTorque(float deltaTime)
    {        
        virtualWheel.ApplyLossTorque(deltaTime);
    }

    public void Step(float fixedDeltaTime)
    {
        virtualWheel.Step(fixedDeltaTime);
    }

    public virtual bool IsInContact()
    {
        return true;//wheelCollider.isGrounded;
    }

    public BycicleTireMaterial_Base GetTireMaterial()
    {
        return tireMaterial;
    }

    public PhysicsMaterial GetGroundMaterial()
    {
        return groundMaterial;
    }

    public float PredictLossImpulse(float impulse, float deltaTime)
    {
        return virtualWheel.PredictLossImpulse(impulse, deltaTime);
    }
}
