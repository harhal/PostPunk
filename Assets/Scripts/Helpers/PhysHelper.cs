using System;
using System.Linq;
using UnityEngine;

public enum PhysDirection
{
    Forward,
    Right,
    Up
}

public enum PhysPlane
{
    Horizontal,
    Front,
    Vertical
}

public static class PhysHelper
{
    public static Vector3 UnrotateVector(Vector3 v, Quaternion rot)
    {
        return Quaternion.Inverse(rot) * v;
    }

    public static Vector3 UnrotateForces(Vector3 linearVelocity, Vector3 force, Quaternion rot, float mass, float deltaTime, ForceMode forceMode)
    {
        Vector3 unrotatedVelocity = UnrotateVector(linearVelocity, rot);
        Vector3 unrotatedForce = UnrotateVector(force, rot);

        return forceMode switch
        {
            ForceMode.Acceleration => 
                unrotatedVelocity / deltaTime + unrotatedForce / mass,
            ForceMode.Force => 
                unrotatedVelocity * mass / deltaTime + unrotatedForce,
            ForceMode.Impulse => 
                unrotatedVelocity * mass + unrotatedForce * deltaTime,
            ForceMode.VelocityChange => 
                unrotatedVelocity + unrotatedForce / mass * deltaTime,
            _ => Vector3.zero
        };
    }

    public static Vector3 UnrotateForces(Rigidbody rb, float deltaTime, ForceMode forceMode)
    {
        return UnrotateForces(rb.linearVelocity, rb.GetAccumulatedForce(), rb.rotation, rb.mass, deltaTime, forceMode);
    }

    public static float ExtractForce(Vector3 linearVelocity, Vector3 force, Quaternion rot, float mass, float deltaTime, ForceMode forceMode, PhysDirection direction)
    {
        Vector3 unrotatedForces = UnrotateForces(linearVelocity, force, rot, mass, deltaTime, forceMode);
        return direction switch
        {
            PhysDirection.Forward => unrotatedForces.z,
            PhysDirection.Right => unrotatedForces.x,
            PhysDirection.Up => unrotatedForces.y,
            _ => .0f
        };
    }

    public static float ExtractForce(Rigidbody rb, float deltaTime, ForceMode forceMode, PhysDirection direction)
    {
        return ExtractForce(rb.linearVelocity, rb.GetAccumulatedForce(), rb.rotation, rb.mass, deltaTime, forceMode, direction);
    }

    public static Vector3 ExtractForcesPlane(Vector3 linearVelocity, Vector3 force, Quaternion rot, float mass, float deltaTime, ForceMode forceMode, PhysPlane plane)
    {
        Vector3 unrotatedForces = UnrotateForces(linearVelocity, force, rot, mass, deltaTime, forceMode);
        return plane switch
        {
            PhysPlane.Horizontal => new Vector3(unrotatedForces.x, .0f, unrotatedForces.z),
            PhysPlane.Front => new Vector3(unrotatedForces.x, unrotatedForces.y, .0f),
            PhysPlane.Vertical => new Vector3(.0f, unrotatedForces.y, unrotatedForces.z),
            _ => Vector3.zero
        };
    }

    public static Vector3 ExtractForcesPlane(Rigidbody rb, float deltaTime, ForceMode forceMode, PhysPlane plane)
    {
        return ExtractForcesPlane(rb.linearVelocity, rb.GetAccumulatedForce(), rb.rotation, rb.mass, deltaTime, forceMode, plane);
    }

    public static float GetSlip(float v0, float v1, GripDirection direction)
    {
        if (direction == GripDirection.Longitudinal)
        {
            return Mathf.Abs(v0 - v1) / Mathf.Max(v0, v1, .0001f);
        }

        return Mathf.Abs(v1) / Mathf.Max(v0, .0001f);
    }

    public static float GetMaxLateralGripImpulse(Vector3 groundVelocity, float downImpulse, BycicleTireMaterial_Base tire, PhysicsMaterial groundMaterial)
    {
        float slip = GetSlip(groundVelocity.z, groundVelocity.x, GripDirection.Lateral);
        return tire.GetGripLimit(groundMaterial, slip, downImpulse, GripDirection.Lateral);
    }

    public static float GetMaxLongitudalGripImpulse(ImpulseEquation a, ImpulseEquation b, float downImpulse, BycicleTireMaterial_Base tire, PhysicsMaterial groundMaterial)
    {
        float slip = GetSlip(a.CommonVelocity, b.CommonVelocity, GripDirection.Longitudinal);
        return tire.GetGripLimit(groundMaterial, slip, downImpulse, GripDirection.Longitudinal);
    }

    public static Vector3 ClampInElipse(Vector3 v, Vector3 lim)
    {
        float focusSqrt = .0f;

        if (lim.x != .0f)
        {
            focusSqrt += v.x * v.x / lim.x / lim.x;
        }
        else
        {
            v.x = .0f;
        }

        if (lim.y != .0f)
        {
            focusSqrt += v.y * v.y / lim.y / lim.y;
        }
        else
        {
            v.y = .0f;
        }

        if (lim.z != .0f)
        {
            focusSqrt += v.z * v.z / lim.z / lim.z;
        }
        else
        {
            v.z = .0f;
        }

        return focusSqrt > 1.0f ? v / Mathf.Sqrt(focusSqrt) : v;
    }
}