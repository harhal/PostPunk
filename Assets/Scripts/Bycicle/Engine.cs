using UnityEngine;

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
    }

    private void updateState()
    {
        DisplayVelocity = PhysHelper.UnrotateVector(rigidBody.linearVelocity, rigidBody.rotation);
        transmission.UpdateTransmission(inputGear.AngSpeed, inputGear.BasicAngSpeed);
        DisplayVelocity = PhysHelper.UnrotateVector(rigidBody.linearVelocity, rigidBody.rotation);
    }

    private void updateInnerForces()
    {
        inputGear.UpdateInputMode();

        inputGear.ApplyInputTorque(Time.fixedDeltaTime);

        inputGear.ApplyLossTorque(Time.fixedDeltaTime);        
        driveWheel.ApplyLossTorque(Time.fixedDeltaTime);
        steeringWheel.ApplyLossTorque(Time.fixedDeltaTime);

        EqualizeSystemImpulses(out float inputGearImpulse, out float driveImpulse, out float steeringImpulse, out float vehicleDriveImpulse, out float vehicleSteeringImpulse);

        inputGear.ApplyForce(inputGearImpulse, Time.fixedDeltaTime, ForceMode.Impulse);
        driveWheel.ApplyForce(driveImpulse, Time.fixedDeltaTime, ForceMode.Impulse);
        steeringWheel.ApplyForce(steeringImpulse, Time.fixedDeltaTime, ForceMode.Impulse);

        rigidBody.AddForce(rigidBody.transform.forward * vehicleDriveImpulse, ForceMode.Impulse);
        rigidBody.AddForce(steeringWheel.GetLocalRotation() * rigidBody.transform.forward * vehicleSteeringImpulse, ForceMode.Impulse);
        
        Vector3 vehicleVelocity1 = PhysHelper.UnrotateForces(rigidBody, Time.fixedDeltaTime, ForceMode.VelocityChange);
        float driveSideGripLimit = PhysHelper.GetMaxLateralGripImpulse(vehicleVelocity1, DownImpulse * .5f, driveWheel.GetTireMaterial(), driveWheel.GetGroundMaterial());
        Vector3 sideFriction1 = rigidBody.transform.right * Mathf.Clamp(PhysHelper.ExtractForce(rigidBody, Time.fixedDeltaTime, ForceMode.Impulse, PhysDirection.Right), -driveSideGripLimit, driveSideGripLimit);

        rigidBody.AddForce(-sideFriction1, ForceMode.Impulse);

        Vector3 vehicleVelocity2 = PhysHelper.UnrotateVector(PhysHelper.UnrotateForces(rigidBody, Time.fixedDeltaTime, ForceMode.VelocityChange), steeringWheel.GetLocalRotation());
        float steeringSideGripLimit = PhysHelper.GetMaxLateralGripImpulse(vehicleVelocity2, DownImpulse * .5f, steeringWheel.GetTireMaterial(), steeringWheel.GetGroundMaterial());
        Vector3 sideFriction2 = rigidBody.transform.right * Mathf.Clamp(PhysHelper.ExtractForce(rigidBody, Time.fixedDeltaTime, ForceMode.Impulse, PhysDirection.Right), -steeringSideGripLimit, steeringSideGripLimit);
        rigidBody.AddForce(-sideFriction2, ForceMode.Impulse);

        float forwardVelocity = Vector3.Dot(rigidBody.linearVelocity, rigidBody.transform.forward);
        float steeredAngle = steeringWheel.GetSteeredAngle(forwardVelocity, Time.fixedDeltaTime);

        rigidBody.MoveRotation(Quaternion.LookRotation(Quaternion.AngleAxis(steeredAngle, Vector3.up) * rigidBody.transform.forward, Vector3.up));

        updateJumpRequest();

        driveWheel.Step(Time.fixedDeltaTime);
        steeringWheel.Step(Time.fixedDeltaTime);
        inputGear.Step(Time.fixedDeltaTime);
    }

    private void EqualizeSystemImpulses(out float inputGearImpulse, out float driveImpulse, out float steeringImpulse, out float vehicleDriveImpulse, out float vehicleSteeringImpulse)
    {
        float ratio = transmission.GetCurrentRatio();

        float gearsRadiusFactor = driveWheel.Radius / inputGear.Radius;

        var impulsesEquation = new ImpulseEquation()
            .Add(inputGear.LinearImpulse * gearsRadiusFactor, inputGear.LinearInertia, ratio)
            .Add(driveWheel)
            .Add(steeringWheel)
            .Add(rigidBody);

        float inputGearMinImpulse = .0f;
        float inputGearMaxImpulse = inputGear.LinearImpulse * gearsRadiusFactor;

        float inputGearFullImpulse = Mathf.Clamp(transmission.ReverseTransmitSpeed(impulsesEquation.CommonVelocity) * inputGear.LinearInertia, inputGearMinImpulse, inputGearMaxImpulse);

        impulsesEquation.Remove(inputGear, inputGearFullImpulse, ratio);

        var untouchedImpulse = impulsesEquation
            .Clone()
            .Remove(driveWheel);

        var drivePotentialImpulse = impulsesEquation
            .Clone()
            .Remove(untouchedImpulse);

        float driveGripLimit = PhysHelper.GetMaxLongitudalGripImpulse(drivePotentialImpulse, untouchedImpulse, DownImpulse * .5f, driveWheel.GetTireMaterial(), driveWheel.GetGroundMaterial());

        float driveFullImpulse = Mathf.Clamp(impulsesEquation.CommonVelocity * driveWheel.LinearInertia, drivePotentialImpulse.CommonImpulse - driveGripLimit, drivePotentialImpulse.CommonImpulse + driveGripLimit);

        impulsesEquation.Remove(driveWheel, driveFullImpulse);

        var rightImpulse = impulsesEquation
            .Clone()
            .Remove(rigidBody);

        var vehiclePotentialImpulse = impulsesEquation
            .Clone()
            .Remove(rightImpulse);

        float steeringGripLimit = PhysHelper.GetMaxLongitudalGripImpulse(vehiclePotentialImpulse, rightImpulse, DownImpulse * .5f, steeringWheel.GetTireMaterial(), steeringWheel.GetGroundMaterial());
        float vehicleFullImpulse = Mathf.Clamp(impulsesEquation.CommonVelocity * rigidBody.mass, vehiclePotentialImpulse.CommonImpulse - steeringGripLimit, vehiclePotentialImpulse.CommonImpulse + steeringGripLimit);
        float steeringFullImpulse = impulsesEquation.Remove(rigidBody, vehicleFullImpulse).CommonImpulse;

        float vehicleDeltaImpulse = vehicleFullImpulse - PhysHelper.ExtractForce(rigidBody, Time.fixedDeltaTime, ForceMode.Impulse, PhysDirection.Forward);

        inputGearImpulse = inputGearFullImpulse / gearsRadiusFactor - inputGear.LinearImpulse;
        driveImpulse = driveFullImpulse - driveWheel.LinearImpulse;
        steeringImpulse = steeringFullImpulse - steeringWheel.LinearImpulse;
        vehicleDriveImpulse = vehicleDeltaImpulse - steeringImpulse;
        vehicleSteeringImpulse = vehicleDeltaImpulse - vehicleDriveImpulse;
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