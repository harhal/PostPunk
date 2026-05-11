using UnityEngine;

public class BycicleEngine : MonoBehaviour
{   
    [SerializeField]
    private Rigidbody rigidBody;
   
    [SerializeField]
    private SteeringHandles steeringHandles;
   
    [SerializeField]
    private BycicleInputGear inputGear;
   
    [SerializeField]
    private BycicleTransmission transmission;
   
    [SerializeField]
    private BycicleWheel inputGearWheel;
   
    [SerializeField]
    private BycicleWheel driveWheel;
   
    [SerializeField]
    private BycicleWheel steeringWheel;
    
    [SerializeField]
    private BycicleWheelCollider driveWheelCollider;
    
    [SerializeField]    
    private BycicleWheelCollider steeringWheelCollider;

    [SerializeField]    
    private BycicleJumpSpring jumpSpring;

    [SerializeField]
    private float wheelsDist = 1.0f;

    public float DownImpulse => -PhysHelper.ExtractForce(rigidBody, Time.fixedDeltaTime, ForceMode.Impulse, PhysDirection.Up) + PhysHelper.UnrotateVector(Physics.gravity * rigidBody.mass / Time.fixedDeltaTime, rigidBody.rotation).y;

    public float VehicleVelocity => Vector3.Dot(rigidBody.linearVelocity, rigidBody.transform.forward);

    public Vector3 DisplayVelocity;

    void Awake()
    {
        PrintPerfectInputImpulse();
    }

    private void updateState()
    {
        DisplayVelocity = PhysHelper.UnrotateVector(rigidBody.linearVelocity, rigidBody.rotation);
        transmission.UpdateTransmission(inputGearWheel.AngSpeed, inputGear.BasicAngSpeed);
        DisplayVelocity = PhysHelper.UnrotateVector(rigidBody.linearVelocity, rigidBody.rotation);
        driveWheelCollider.UpdateContactPoints();
        steeringWheelCollider.UpdateContactPoints();
    }

    private void PrintPerfectInputImpulse()
    {
        float firstTransmissionRatio = .64f;
        float transmissionsRatio = 1.25f;

        for (int idx = 0; idx < 10; idx++)
        {
            float ratio = firstTransmissionRatio * Mathf.Pow(transmissionsRatio, idx);

            float angInputVelocity = 360f * Mathf.Deg2Rad;
            float driveLinearVelocity = angInputVelocity * ratio * driveWheel.Radius;

            ImpulseEquation impulsesEquation = new ImpulseEquation()
                .Add(angInputVelocity * inputGearWheel.Radius * inputGearWheel.LinearInertia, inputGearWheel.LinearInertia, ratio)
                .Add(driveLinearVelocity * driveWheel.LinearInertia, driveWheel.LinearInertia)
                .Add(driveLinearVelocity * rigidBody.mass, rigidBody.mass)
                .Add(driveLinearVelocity * steeringWheel.LinearInertia, steeringWheel.LinearInertia);

            float InputImpulse = impulsesEquation.CommonImpulse * ratio;
        
            ImpulseEquation impulsesEquation1 = new ImpulseEquation()
                .Add(InputImpulse, inputGearWheel.LinearInertia, ratio)
                .Add(0f, driveWheel.LinearInertia)
                .Add(0f, rigidBody.mass)
                .Add(0f, steeringWheel.LinearInertia);

            Debug.LogFormat("{0}x{1:F2}({2:F2}): {3:F2}/{4:F2} = {5:F2};\nInput momentum = {6:F2}; {7:F2}", idx, ratio, driveLinearVelocity, impulsesEquation.CommonImpulse, impulsesEquation.CommonInertia, impulsesEquation.CommonVelocity,
                InputImpulse * inputGearWheel.Radius, impulsesEquation1.CommonImpulse);
        }
    }

    void FixedUpdate()
    {
        updateState();

        bool steeringWheelHasContact = steeringWheelCollider.ContactPoints.Count > 0;
        bool driveWheelHasContact = driveWheelCollider.ContactPoints.Count > 0;

        inputGear.UpdateInputMode();

        inputGear.ApplyInputTorque(Time.fixedDeltaTime);
        inputGearWheel.ApplyLossTorque(Time.fixedDeltaTime);

        EqualizeSystemImpulses(out float inputGearImpulse, out float driveImpulse, out float steeringImpulse, out float vehicleDriveImpulse, out float vehicleSteeringImpulse);

        inputGearWheel.ApplyForce(inputGearImpulse, Time.fixedDeltaTime, ForceMode.Impulse);
        driveWheel.ApplyForce(driveImpulse, Time.fixedDeltaTime, ForceMode.Impulse);
        steeringWheel.ApplyForce(steeringImpulse, Time.fixedDeltaTime, ForceMode.Impulse);

        rigidBody.AddForce(rigidBody.transform.forward * vehicleDriveImpulse, ForceMode.Impulse);
        rigidBody.AddForce(steeringHandles.GetLocalRotation() * rigidBody.transform.forward * vehicleSteeringImpulse, ForceMode.Impulse);
        
        if (driveWheelHasContact)
        {
            Vector3 vehicleVelocity1 = PhysHelper.UnrotateForces(rigidBody, Time.fixedDeltaTime, ForceMode.VelocityChange);
            float driveSideGripLimit = PhysHelper.GetMaxLateralGripImpulse(vehicleVelocity1, DownImpulse * .5f, driveWheel.GetTireMaterial(), driveWheel.GetGroundMaterial());
            Vector3 sideFriction1 = rigidBody.transform.right * Mathf.Clamp(PhysHelper.ExtractForce(rigidBody, Time.fixedDeltaTime, ForceMode.Impulse, PhysDirection.Right), -driveSideGripLimit, driveSideGripLimit);

            rigidBody.AddForce(-sideFriction1, ForceMode.Impulse);
        }

        if (steeringWheelHasContact)
        {
            Vector3 vehicleVelocity2 = PhysHelper.UnrotateVector(PhysHelper.UnrotateForces(rigidBody, Time.fixedDeltaTime, ForceMode.VelocityChange), steeringHandles.GetLocalRotation());
            float steeringSideGripLimit = PhysHelper.GetMaxLateralGripImpulse(vehicleVelocity2, DownImpulse * .5f, steeringWheel.GetTireMaterial(), steeringWheel.GetGroundMaterial());
            Vector3 sideFriction2 = rigidBody.transform.right * Mathf.Clamp(PhysHelper.ExtractForce(rigidBody, Time.fixedDeltaTime, ForceMode.Impulse, PhysDirection.Right), -steeringSideGripLimit, steeringSideGripLimit);

            rigidBody.AddForce(-sideFriction2, ForceMode.Impulse);

            float forwardVelocity = Vector3.Dot(rigidBody.linearVelocity, rigidBody.transform.forward);
            float steeredAngle = steeringHandles.GetSteeredAngle(forwardVelocity, wheelsDist, Time.fixedDeltaTime);

            rigidBody.MoveRotation(Quaternion.LookRotation(Quaternion.AngleAxis(steeredAngle, Vector3.up) * rigidBody.transform.forward, Vector3.up));
        }


        if (driveWheelHasContact || steeringWheelHasContact)
        {
            rigidBody.AddForce(jumpSpring.ApplyJumpImpulse(), ForceMode.Impulse);
        }

        Debug.Log(inputGearWheel.AngSpeed);

        driveWheel.ApplyLossTorque(Time.fixedDeltaTime);
        steeringWheel.ApplyLossTorque(Time.fixedDeltaTime);

        driveWheel.Step(Time.fixedDeltaTime);
        steeringWheel.Step(Time.fixedDeltaTime);
        inputGearWheel.Step(Time.fixedDeltaTime);
    }

    private void EqualizeSystemImpulses(out float inputGearImpulse, out float driveImpulse, out float steeringImpulse, out float vehicleDriveImpulse, out float vehicleSteeringImpulse)
    {
        bool steeringWheelHasContact = steeringWheelCollider.ContactPoints.Count > 0;
        bool driveWheelHasContact = driveWheelCollider.ContactPoints.Count > 0;

        var impulsesEquation = new ImpulseEquation()
            .Add(inputGearWheel, transmission.GetCurrentRatio())
            .Add(driveWheel);

        if (driveWheelHasContact)
        {
            impulsesEquation.Add(rigidBody);

            if (steeringWheelHasContact)
            {                
                impulsesEquation.Add(steeringWheel);
            }
        }

        Debug.LogFormat("Total impulse is {0:F2} per {1:F2} = {2:F2}", impulsesEquation.CommonImpulse, Time.time,  impulsesEquation.CommonImpulse / Time.time);

        inputGearImpulse = EqualizeInputGear(impulsesEquation);

        if (driveWheelHasContact)
        {
            driveImpulse = EqualizeDriveWheel(impulsesEquation);
            
            if (steeringWheelHasContact)
            {
                steeringImpulse = EqualizeSteeringWheel(impulsesEquation);

                float vehicleDeltaImpulse = impulsesEquation.CommonImpulse - PhysHelper.ExtractForce(rigidBody, Time.fixedDeltaTime, ForceMode.Impulse, PhysDirection.Forward);

                vehicleDriveImpulse = vehicleDeltaImpulse - steeringImpulse;
                vehicleSteeringImpulse = vehicleDeltaImpulse - vehicleDriveImpulse;
            }
            else
            {
                steeringImpulse = 0f;

                vehicleDriveImpulse = impulsesEquation.CommonImpulse - PhysHelper.ExtractForce(rigidBody, Time.fixedDeltaTime, ForceMode.Impulse, PhysDirection.Forward);
                vehicleSteeringImpulse = 0f;
            }
        }
        else
        {
            driveImpulse = impulsesEquation.CommonImpulse - driveWheel.LinearImpulse;

            if (steeringWheelHasContact)
            {
                vehicleDriveImpulse = 0f;

                impulsesEquation = new ImpulseEquation()
                    .Add(rigidBody)
                    .Add(steeringWheel);

                steeringImpulse = EqualizeSteeringWheel(impulsesEquation);

                vehicleSteeringImpulse = impulsesEquation.CommonImpulse - PhysHelper.ExtractForce(rigidBody, Time.fixedDeltaTime, ForceMode.Impulse, PhysDirection.Forward);
            }
            else
            {
                steeringImpulse = 0f;

                vehicleDriveImpulse = 0f;
                vehicleSteeringImpulse = 0f;
            }
        }
    }

    private float EqualizeInputGear(ImpulseEquation impulsesEquation)
    {
        float inputGearMinImpulse = .0f;
        float inputGearMaxImpulse = inputGearWheel.LinearImpulse;

        float ratio = transmission.GetCurrentRatio();

        float syncAngVelocityFactor = inputGearWheel.Radius / driveWheel.Radius;
        
        float fixedCommonVelocity = impulsesEquation.Clone().Remove(inputGearWheel, 0f, ratio).Add(0f, inputGearWheel.LinearInertia * syncAngVelocityFactor, ratio).CommonVelocity * syncAngVelocityFactor;
        float inputGearFullImpulse = Mathf.Clamp(transmission.ReverseTransmitSpeed(fixedCommonVelocity) * inputGearWheel.LinearInertia, inputGearMinImpulse, inputGearMaxImpulse);

        impulsesEquation.Remove(inputGearWheel, inputGearFullImpulse, ratio);

        return inputGearFullImpulse - inputGearWheel.LinearImpulse;
    }

    private float EqualizeDriveWheel(ImpulseEquation impulsesEquation)
    {
        var untouchedImpulse = impulsesEquation
            .Clone()
            .Remove(driveWheel);

        var drivePotentialImpulse = impulsesEquation
            .Clone()
            .Remove(untouchedImpulse);

        float driveGripLimit = PhysHelper.GetMaxLongitudalGripImpulse(drivePotentialImpulse.Clone().AddLoss(driveWheel, Time.deltaTime, true), untouchedImpulse, DownImpulse * .5f, driveWheel.GetTireMaterial(), driveWheel.GetGroundMaterial());

        float driveFullImpulse = Mathf.Clamp(impulsesEquation.CommonVelocity * driveWheel.LinearInertia, drivePotentialImpulse.CommonImpulse - driveGripLimit, drivePotentialImpulse.CommonImpulse + driveGripLimit);

        impulsesEquation.Remove(driveWheel, driveFullImpulse);

        return driveFullImpulse - driveWheel.LinearImpulse;
    }

    private float EqualizeSteeringWheel(ImpulseEquation impulsesEquation)
    {
        var rightImpulse = impulsesEquation
            .Clone()
            .Remove(rigidBody);

        var vehiclePotentialImpulse = impulsesEquation
            .Clone()
            .Remove(rightImpulse);

        float steeringGripLimit = PhysHelper.GetMaxLongitudalGripImpulse(vehiclePotentialImpulse, rightImpulse, DownImpulse * .5f, steeringWheel.GetTireMaterial(), steeringWheel.GetGroundMaterial());
        float vehicleFullImpulse = Mathf.Clamp(impulsesEquation.CommonVelocity * rigidBody.mass, vehiclePotentialImpulse.CommonImpulse - steeringGripLimit, vehiclePotentialImpulse.CommonImpulse + steeringGripLimit);
        float steeringFullImpulse = impulsesEquation.Clone().Remove(rigidBody, vehicleFullImpulse).CommonImpulse;
        impulsesEquation.Remove(steeringWheel, steeringFullImpulse);

        return steeringFullImpulse - steeringWheel.LinearImpulse;
    }
}