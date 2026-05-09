using UnityEngine;

public class SteeringHandles : MonoBehaviour
{    
    [SerializeField]
    private float maxSteeringAngle = 30.0f;

    [field: SerializeField]
    public float steeringAngle {get; private set;}

    [SerializeField]
    private Transform steeringMesh;

    public void SetSteeringInput(float input)
    {
        steeringAngle = maxSteeringAngle * input;
    }

    public float GetSteeredAngle(float linearForwardVelocity, float wheelsDistance, float predictedDeltaTime)
    {        
        return linearForwardVelocity * predictedDeltaTime * Mathf.Sin(steeringAngle * Mathf.Deg2Rad) / wheelsDistance * Mathf.Rad2Deg;
    }

    public Quaternion GetLocalRotation()
    {
        return Quaternion.Euler(.0f, .0f, steeringAngle);
    }
}
