using System;
using System.Collections.Generic;
using UnityEngine;

public enum EquationMember
{
    Gear,
    Drive,
    Body,
    Steer
}

public class ImpulseEquation
{
    float commonImpulse = 0f;
    float commonInertia = 0f;

    public ImpulseEquation Add(Rigidbody body, Vector3 groundVelocity, float ratio = 1.0f)
    {
        return Add(PhysHelper.UnrotateForces(body, groundVelocity, Time.fixedDeltaTime, ForceMode.Impulse).z, body.mass, ratio);
    }

    public ImpulseEquation Add(BycicleWheel wheel, float ratio = 1.0f)
    {
        return Add(wheel.LinearImpulse, wheel.LinearInertia, ratio);
    }

    public ImpulseEquation Add(VirtualWheel wheel, float ratio = 1.0f)
    {
        return Add(wheel.LinearImpulse, wheel.LinearInertia, ratio);
    }

    public ImpulseEquation Add(ImpulseEquation right, float ratio = 1.0f)
    {
        return Add(right.commonImpulse, right.commonInertia, ratio);
    }

    public ImpulseEquation Add(float impulse, float inertia, float ratio = 1.0f)
    {
        commonImpulse += impulse / ratio;
        commonInertia += inertia / ratio / ratio;

        return this;
    }

    public ImpulseEquation Remove(Rigidbody body, Vector3 groundVelocity, float? impulse = null, float ratio = 1.0f)
    {
        return Remove(impulse.HasValue ? impulse.Value : PhysHelper.UnrotateForces(body, groundVelocity, Time.fixedDeltaTime, ForceMode.Impulse).z, body.mass, ratio);
    }

    public ImpulseEquation Remove(BycicleWheel wheel, float? impulse = null, float ratio = 1.0f)
    {
        return Remove(impulse.HasValue ? impulse.Value : wheel.LinearImpulse, wheel.LinearInertia, ratio);
    }

    public ImpulseEquation Remove(VirtualWheel wheel, float? impulse = null, float ratio = 1.0f)
    {
        return Remove(impulse.HasValue ? impulse.Value : wheel.LinearImpulse, wheel.LinearInertia, ratio);
    }

    public ImpulseEquation Remove(ImpulseEquation right, float ratio = 1.0f)
    {
        return Remove(right.commonImpulse, right.commonInertia, ratio);
    }

    public ImpulseEquation Remove(float impulse, float inertia, float ratio = 1.0f)
    {
        commonImpulse -= impulse / ratio;
        commonInertia -= inertia / ratio / ratio;

        return this;
    }

    public ImpulseEquation Clone()
    {
        return new ImpulseEquation().Add(commonImpulse, commonInertia);
    }

    public ImpulseEquation AddLoss(BycicleWheel wheel, float deltaTime, bool applyToCommonImpulse = false)
    {
        commonImpulse -= wheel.PredictLossImpulse(applyToCommonImpulse ? commonImpulse : wheel.LinearImpulse, deltaTime);
        return this;
    }

    public float CommonVelocity => commonInertia != 0f ? commonImpulse / commonInertia : commonImpulse;
    public float CommonImpulse => commonImpulse;
    public float CommonInertia => commonInertia;
}