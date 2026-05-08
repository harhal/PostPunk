using UnityEngine;

[RequireComponent(typeof(BycicleWheel))]
public class BycicleSteeringWheel : BycicleWheelProcessor
{    
    [SerializeField]
    private float maxSteeringAngle = 30.0f;

    [field: SerializeField]
    public float steeringAngle {get; private set;}

    [SerializeField]
    private float wheelsDist = 1.0f;

    [SerializeField]
    private Transform steeringMesh;

    public void PushSteeringInput(float input)
    {
        steeringAngle = maxSteeringAngle * input;
    }

    public float GetSteeredAngle(float linearForwardVelocity, float predictedDeltaTime)
    {        
        return linearForwardVelocity * predictedDeltaTime * Mathf.Sin(steeringAngle * Mathf.Deg2Rad) / wheelsDist * Mathf.Rad2Deg;
    }

    public override Quaternion GetLocalRotation()
    {
        return Quaternion.Euler(.0f, .0f, steeringAngle);
    }
}
