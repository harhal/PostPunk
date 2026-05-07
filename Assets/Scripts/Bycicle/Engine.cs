using System;
using UnityEngine;
using UnityEngine.UIElements;

public class BycicleEngine : MonoBehaviour
{   
    [SerializeField]
    private Rigidbody rigidBody;
   
    [SerializeField]
    private BycicleSteeringWheel steeringWheel;
   
    [SerializeField]
    private BycicleInputGear inputGear;
   
    [SerializeField]
    private BycicleTransmission transmission;
   
    [SerializeField]
    private BycicleDriveWheel driveWheel;
   
    [SerializeField]
    private bool savedJumpRequestInput;

    [SerializeField]
    private float jumpImpulse = 60.0f;

    public float DownImpulse => -PhysHelper.ExtractForce(rigidBody, Time.fixedDeltaTime, ForceMode.Impulse, PhysDirection.Up) + PhysHelper.UnrotateVector(Physics.gravity * rigidBody.mass / Time.fixedDeltaTime, rigidBody.rotation).y;

    public float VehicleVelocity => Vector3.Dot(rigidBody.linearVelocity, rigidBody.transform.forward);

    public Vector3 DisplayVelocity;

    public void SetPedalsInput(Vector2 input)
    {
        inputGear.SetPedalsInput(input);
    }

    public void SetSteeringInput(float input)
    {
        steeringWheel.PushSteeringInput(input);
    }

    public void SetBrakeInput(bool input)
    {
        driveWheel.SetBrakeActive(input);
    }

    public void RequestJump()
    {
        savedJumpRequestInput = true;
    }

    void FixedUpdate()
    {
        updateState();
        updateInnerForces();
        updateOutsideForces();
    }

    private void updateState()
    {
        DisplayVelocity = PhysHelper.UnrotateVector(rigidBody.linearVelocity, rigidBody.rotation);

        //fixWheelAngSpeed(driveWheel);
        driveWheel.Step(Time.fixedDeltaTime);

        //fixWheelAngSpeed(steeringWheel);
        steeringWheel.Step(Time.fixedDeltaTime);

        //fixGearAngSpeed();
        inputGear.Step(Time.fixedDeltaTime);

        transmission.UpdateTransmission(inputGear.AngSpeed, inputGear.BasicAngSpeed);

        DisplayVelocity = PhysHelper.UnrotateVector(rigidBody.linearVelocity, rigidBody.rotation);
    }

    private void fixWheelAngSpeed(BycicleWheelBase wheel)
    {        
        /*float fixTorque = driveWheel.GetGripForce(DownForce * 0.5f, DisplayVelocity, DisplayVelocity.z, Time.fixedDeltaTime);
        driveWheel.ApplyTorque(fixTorque);
        driveWheel.ApplyTorque_old(Time.fixedDeltaTime);*/
    }

    private void fixGearAngSpeed()
    {
        /*float connectedAngSpeed = transmission.ReverseTransmitSpeed(driveWheel.AngSpeed);

        inputGear.ApplyTorque(connectedAngSpeed, Time.fixedDeltaTime);
        inputGear.ApplyTorque_old(Time.fixedDeltaTime);*/
    }

    private void updateInnerForces()
    {
        inputGear.UpdateInputMode();

        inputGear.ApplyInputTorque(Time.fixedDeltaTime);

        inputGear.ApplyLossTorque(Time.fixedDeltaTime);        
        driveWheel.ApplyLossTorque(Time.fixedDeltaTime);
        steeringWheel.ApplyLossTorque(Time.fixedDeltaTime);

        float ratio = transmission.GetCurrentRatio();

        float gearsRadiusFactor = driveWheel.virtualWheel.Radius / inputGear.virtualWheel.Radius;

        var impulsesEquation = new ImpulseEquation()
            .Add(inputGear.LinearImpulse * gearsRadiusFactor, inputGear.LinearInertia, ratio)
            .Add(driveWheel)
            .Add(steeringWheel)
            .Add(rigidBody);

        Debug.LogFormat("CV1: {0:F20}", impulsesEquation.CommonVelocity);

        float inputGearMinImpulse = .0f;
        float inputGearMaxImpulse = inputGear.LinearImpulse * gearsRadiusFactor;

        float removeme = transmission.ReverseTransmitSpeed(impulsesEquation.CommonVelocity) * inputGear.LinearInertia;
        float inputGearImpulse = Mathf.Clamp(transmission.ReverseTransmitSpeed(impulsesEquation.CommonVelocity) * inputGear.LinearInertia, inputGearMinImpulse, inputGearMaxImpulse);

        impulsesEquation.Remove(inputGear, inputGearImpulse, ratio);
        Debug.LogFormat("CV2: {0:F20}", impulsesEquation.CommonVelocity);

        var untouchedImpulse = impulsesEquation
            .Clone()
            .Remove(driveWheel);

        var drivePotentialImpulse = impulsesEquation
            .Clone()
            .Remove(untouchedImpulse);


        float driveGripLimit = PhysHelper.GetMaxLongitudalGripImpulse(drivePotentialImpulse, untouchedImpulse, DownImpulse * .5f, driveWheel.GetTireMaterial(), driveWheel.GetGroundMaterial());

        float driveImpulse = Mathf.Clamp(impulsesEquation.CommonVelocity * driveWheel.LinearInertia, drivePotentialImpulse.CommonImpulse - driveGripLimit, drivePotentialImpulse.CommonImpulse + driveGripLimit);


        impulsesEquation.Remove(driveWheel, driveImpulse);
        Debug.LogFormat("CV3: {0:F20}", impulsesEquation.CommonVelocity);

        var rightImpulse = impulsesEquation
            .Clone()
            .Remove(rigidBody);

        var vehiclePotentialImpulse = impulsesEquation
            .Clone()
            .Remove(rightImpulse);


        float steeringGripLimit = PhysHelper.GetMaxLongitudalGripImpulse(vehiclePotentialImpulse, rightImpulse, DownImpulse * .5f, steeringWheel.GetTireMaterial(), steeringWheel.GetGroundMaterial());

        float vehicleImpulse = Mathf.Clamp(impulsesEquation.CommonVelocity * rigidBody.mass, vehiclePotentialImpulse.CommonImpulse - steeringGripLimit, vehiclePotentialImpulse.CommonImpulse + steeringGripLimit);

        float steeringImpulse = impulsesEquation.Remove(rigidBody, vehicleImpulse).CommonImpulse;
        Debug.LogFormat("CV4: {0:F20}", impulsesEquation.CommonVelocity);

        Debug.LogFormat("{0:F20} < {1:F20} => {2:F20} < {3:F20} | {4:F20} | {5:F20} | {6:F20}", 
            inputGearMinImpulse,
            removeme,
            inputGearImpulse,
            inputGearMaxImpulse,
            driveImpulse,
            vehicleImpulse,
            steeringImpulse);

        inputGear.ApplyForce(inputGearImpulse / gearsRadiusFactor - inputGear.LinearImpulse, Time.fixedDeltaTime, ForceMode.Impulse);
        driveWheel.ApplyForce(driveImpulse - driveWheel.LinearImpulse, Time.fixedDeltaTime, ForceMode.Impulse);
        rigidBody.AddForce(rigidBody.transform.forward * (vehicleImpulse - PhysHelper.ExtractForce(rigidBody, Time.fixedDeltaTime, ForceMode.Impulse, PhysDirection.Forward)), ForceMode.Impulse);
        steeringWheel.ApplyForce(steeringImpulse - steeringWheel.LinearImpulse, Time.fixedDeltaTime, ForceMode.Impulse);
        
        Vector3 vehicleVelocity1 = PhysHelper.UnrotateForces(rigidBody, Time.fixedDeltaTime, ForceMode.VelocityChange);
        float driveSideGripLimit = PhysHelper.GetMaxLateralGripImpulse(vehicleVelocity1, DownImpulse * .5f, driveWheel.GetTireMaterial(), driveWheel.GetGroundMaterial());
        Vector3 sideFriction1 = rigidBody.transform.right * Mathf.Clamp(PhysHelper.ExtractForce(rigidBody, Time.fixedDeltaTime, ForceMode.Impulse, PhysDirection.Right), -driveSideGripLimit, driveSideGripLimit);

        rigidBody.AddForce(-sideFriction1, ForceMode.Impulse);

        Vector3 vehicleVelocity2 = PhysHelper.UnrotateForces(rigidBody, Time.fixedDeltaTime, ForceMode.VelocityChange);
        float steeringSideGripLimit = PhysHelper.GetMaxLateralGripImpulse(vehicleVelocity2, DownImpulse * .5f, steeringWheel.GetTireMaterial(), steeringWheel.GetGroundMaterial());
        Vector3 sideFriction2 = rigidBody.transform.right * Mathf.Clamp(PhysHelper.ExtractForce(rigidBody, Time.fixedDeltaTime, ForceMode.Impulse, PhysDirection.Right), -steeringSideGripLimit, steeringSideGripLimit);
        rigidBody.AddForce(-sideFriction2, ForceMode.Impulse);

    }

    private void updateOutsideForces()
    {
        updateGrip();
        updateJumpRequest();

        if (!steeringWheel.IsInContact())
        {
            return;
        }

        float forwardVelocity = VehicleVelocity;
        Vector3 forwardVelocityV = forwardVelocity * rigidBody.transform.forward;

        float steeredAngle = steeringWheel.GetSteeredAngle(forwardVelocity, Time.fixedDeltaTime);

        Vector3 steeredVelocityV = Quaternion.Euler(.0f, steeredAngle * .5f, .0f) * forwardVelocityV;
        rigidBody.linearVelocity = rigidBody.linearVelocity - forwardVelocityV + steeredVelocityV;

        rigidBody.rotation *= Quaternion.Euler(.0f, steeredAngle, .0f);
    }

    private void updateGrip()
    {
        /*Vector3 nextVelocity = PhysHelper.SplitComp(rigidBody, Time.fixedDeltaTime, ForceMode.VelocityChange);

        float redistributedVelocity = (nextVelocity.z * rigidBody.mass + driveWheel.LinearVelocity * driveWheel.LinearInertia + steeringWheel.LinearVelocity * steeringWheel.LinearInertia) /
            (rigidBody.mass + driveWheel.LinearInertia + steeringWheel.LinearInertia);

        driveWheel.ApplyTorque(driveGripForce);        
        driveWheel.ApplyTorque_old(Time.fixedDeltaTime);
        
        steeringWheel.ApplyTorque(steerGripForce);   
        steeringWheel.ApplyTorque_old(Time.fixedDeltaTime);

        rigidBody.AddForce(Vector3.forward * (driveGripForce + steerGripForce), ForceMode.Force);

        Debug.LogFormat("{0:F2} -> {1:F2} {2:F2} {3:F2} <- {4:F2}", nextVelocity.z, driveWheel.LinearVelocity, steeringWheel.LinearVelocity, PhysHelper.SplitComp(rigidBody, Time.fixedDeltaTime, ForceMode.VelocityChange).z, redistributedVelocity);*/
    }

    private void updateJumpRequest()
    {
        if (!savedJumpRequestInput)
        {
            return;
        }


        if (driveWheel.IsInContact())
        {
            rigidBody.AddForce(Vector3.up * jumpImpulse, ForceMode.Impulse);
        }

        savedJumpRequestInput = false;
    }
}