using UnityEngine;

public class BycicleSteeringWheel : BycicleWheelBase
{    
    [SerializeField]
    private float maxSteeringAngle = 30.0f;

    [SerializeField]
    private float steeringAngle;

    [SerializeField]
    private float wheelsDist = 1.0f;

    [SerializeField]
    private Transform steeringMesh;

    protected override void Update()
    {
        base.Update();
        steeringMesh.SetLocalPositionAndRotation(steeringMesh.localPosition, GetLocalRotation());
    }

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
