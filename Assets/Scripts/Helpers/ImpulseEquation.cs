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

    public ImpulseEquation Add(Rigidbody body, float ratio = 1.0f)
    {
        return Add(PhysHelper.UnrotateForces(body, Time.fixedDeltaTime, ForceMode.Impulse).z, body.mass, ratio);
    }

    public ImpulseEquation Add(BycicleWheelProcessor wheel, float ratio = 1.0f)
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

    public ImpulseEquation Remove(Rigidbody body, float? impulse = null, float ratio = 1.0f)
    {
        return Remove(impulse.HasValue ? impulse.Value : PhysHelper.UnrotateForces(body, Time.fixedDeltaTime, ForceMode.Impulse).z, body.mass, ratio);
    }

    public ImpulseEquation Remove(BycicleWheelProcessor wheel, float? impulse = null, float ratio = 1.0f)
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

    public float CommonVelocity => commonInertia != 0f ? commonImpulse / commonInertia : commonImpulse;
    public float CommonImpulse => commonImpulse;
    public float CommonInertia => commonInertia;
}