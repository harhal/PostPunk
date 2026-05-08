using System;
using UnityEngine;

[Serializable]
public class BycicleWheel : MonoBehaviour
{
    [field: SerializeField]
    public VirtualWheel virtualWheel {get; protected set;}
}

[Serializable]
[RequireComponent(typeof(BycicleWheel))]
public class BycicleWheelProcessor : MonoBehaviour
{
    [SerializeField]
    protected bool bBreakIsActive = false;

    [SerializeField]
    protected float brakeTorque = 50.0f;

    [SerializeField]
    protected BycicleTireMaterial_Base tireMaterial;


    [SerializeField]
    protected PhysicsMaterial groundMaterial;

    protected BycicleWheel wheel;

    protected VirtualWheel virtualWheel => wheel.virtualWheel;

    public float DisplayAngSpeed => virtualWheel.DisplayAngSpeed;

    public float LinearVelocity => virtualWheel.LinearVelocity;

    public float LinearImpulse => virtualWheel.LinearImpulse;
    public float AngSpeed => virtualWheel.AngSpeedDeg;

    public float LinearInertia => virtualWheel.LinearInertia;

    public float Inertia => virtualWheel.Inertia;

    public float Angle => virtualWheel.AngleDeg;

    public float Radius => virtualWheel.Radius;

    void Awake()
    {
        wheel = GetComponent<BycicleWheel>();
        enabled = wheel == null;
    }

    public void SetBrakeActive(bool bActive)
    {
        virtualWheel.SetBrakeFriction(bBreakIsActive ? brakeTorque : .0f);
        bBreakIsActive = bActive;
    }

    public bool IsBrakeActive()
    {
        return bBreakIsActive;
    }

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

    public virtual Quaternion GetLocalRotation()
    {
        return Quaternion.identity;
    }

    public BycicleTireMaterial_Base GetTireMaterial()
    {
        return tireMaterial;
    }

    public PhysicsMaterial GetGroundMaterial()
    {
        return groundMaterial;
    }
}
