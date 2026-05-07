using System;
using UnityEngine;

public enum AngForceMode
{
    Torque,
    Momentum,
    RadAngSpeedChange,
    RadAngSpeedAcceleration,
    DegAngSpeedChange,
    DegAngSpeedAcceleration
}

[Serializable]
public class VirtualWheel
{
    [Header("Body")]

    [SerializeField]
    public float Radius = 0.2f;
    [SerializeField]
    public float Mass   = 0.5f;
    [SerializeField]
    public bool  Hollow = false;

    public float Inertia => Mass * Radius * Radius * (Hollow ? 1.0f : 0.5f);

    [Header("Resistance")]

    public float DryFriction    = 0.02f; // N*m

    [SerializeField]
    public float ViscousDamping = 0.01f; // N*m*s/rad

    [Header("Resistance")]

    public float BrakeFriction    = 0.0f; // N*m

    [Header("State")]

    [field: SerializeField]
    public float AngSpeed       { get; private set; } // rad/s
    public float DisplayAngSpeed  { get; private set; } // rad/s

    [field: SerializeField]
    public float Angle          { get; private set; }
    public float LinearVelocity  => AngSpeed * Radius;    // m/s
    public float LinearImpulse  => LinearVelocity * LinearInertia;
    public float AngSpeedDeg  => AngSpeed * Mathf.Rad2Deg; // deg/s
    public float AngleDeg  =>  Angle * Mathf.Rad2Deg/* - 180.0f*/; // deg

    public float LinearInertia => Inertia / Radius / Radius;

    public void ApplyLossTorque(float deltaTime)
    {
        ApplyTorque(-GetLossTorque(deltaTime), deltaTime, AngForceMode.Torque);
    }

    public void ApplyForce(float force, float deltaTime, ForceMode forceMode, float lever = 1.0f, float sin = 1.0f)
    {
        float torque = force * lever * sin * Radius;
        switch (forceMode)
        {
            case ForceMode.Force:
                ApplyTorque(torque, deltaTime, AngForceMode.Torque);
            break;
            case ForceMode.Impulse:
                ApplyTorque(torque, deltaTime, AngForceMode.Momentum);
            break;
            case ForceMode.Acceleration:
                ApplyTorque(torque, deltaTime, AngForceMode.RadAngSpeedAcceleration);
            break;
            case ForceMode.VelocityChange:
                ApplyTorque(torque, deltaTime, AngForceMode.RadAngSpeedChange);
            break;
        }
    }

    public void ApplyTorque(float torque, float deltaTime, AngForceMode forceMode)
    {
        AngSpeed += forceMode switch
        {
            AngForceMode.Torque => 
                torque / Inertia * deltaTime,
            AngForceMode.Momentum => 
                torque / Inertia,
            AngForceMode.RadAngSpeedAcceleration => 
                torque * deltaTime,
            AngForceMode.RadAngSpeedChange => 
                torque,
            AngForceMode.DegAngSpeedAcceleration => 
                torque * deltaTime * Mathf.Deg2Rad,
            AngForceMode.DegAngSpeedChange => 
                torque * Mathf.Deg2Rad,
            _ => .0f
        };
        
    }

    public void Step(float deltaTime)
    {
        Angle = Mathf.Repeat(Angle + AngSpeed * deltaTime, 2 * Mathf.PI);
        DisplayAngSpeed = AngSpeedDeg;
    }

    public void SetBrakeFriction(float brakeFriction)
    {
        BrakeFriction = brakeFriction;
    }

    public float GetTorqueByLimit(float limitAngSpeed)
    {
        return limitAngSpeed * ViscousDamping + DryFriction * Math.Sign(limitAngSpeed);
    }

    public float GetLossTorque(float deltaTime)
    {
        return Mathf.Min(DryFriction + BrakeFriction, Mathf.Abs(AngSpeed * Inertia / deltaTime)) * Math.Sign(AngSpeed) + ViscousDamping * AngSpeed;
    }
}