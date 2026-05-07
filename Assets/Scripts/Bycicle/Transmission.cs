using Unity.Mathematics;
using UnityEngine;

public class BycicleTransmission : MonoBehaviour
{
    [SerializeField]
    float transmissionsRatio = 1.25f;

    [SerializeField]
    float firstTransmissionRatio = 0.8f;

    [SerializeField]
    int lastTransmission = 5;

    [SerializeField]
    int currentTransmission = 0;

    public float TransmitInertia(float inertia)
    {
        float ratio = getTransmissionRatio(currentTransmission);
        return inertia / ratio / ratio;
    }

    public float ReverseTransmitInertia(float inertia)
    {
        float ratio = getTransmissionRatio(currentTransmission);
        return inertia * ratio * ratio;
    }

    public float TransmitSpeed(float inputAngSpeed)
    {
        return inputAngSpeed * getTransmissionRatio(currentTransmission);
    }

    public float ReverseTransmitSpeed(float outputAngSpeed)
    {
        return outputAngSpeed / getTransmissionRatio(currentTransmission);
    }

    public float TransmitTorque(float inputTorque)
    {
        return inputTorque / getTransmissionRatio(currentTransmission);
    }

    public float ReverseTransmitTorque(float outputTorque)
    {
        return outputTorque * getTransmissionRatio(currentTransmission);
    }

    public float TransmitForce(float inputTorque)
    {
        return inputTorque / getTransmissionRatio(currentTransmission);
    }

    public float ReverseTransmitForce(float outputTorque)
    {
        return outputTorque * getTransmissionRatio(currentTransmission);
    }

    public void AddTransmission(int addOffset = 1)
    {
        currentTransmission = math.clamp(currentTransmission + addOffset, 0, lastTransmission);
    }

    public void UpdateTransmission(float currentInputWheelAngSpeed, float basicInputWheelAngSpeed)
    {
        float currentEfficiency = currentInputWheelAngSpeed / basicInputWheelAngSpeed;

        if (currentEfficiency > transmissionsRatio)
        {
            AddTransmission();
        } 
        else if (currentEfficiency < 1.0f / transmissionsRatio)
        {
            AddTransmission(-1);
        }
    }

    public bool IsFirstTransmission()
    {
        return currentTransmission <= 0;
    }

    public bool IsLastTransmission()
    {
        return currentTransmission >= lastTransmission;
    }

    public int GetCurrentTransmission()
    {
        return currentTransmission;
    }

    private float getTransmissionRatio(int transmission)
    {
        return firstTransmissionRatio * Mathf.Pow(transmissionsRatio, transmission);
    }

    public float GetCurrentRatio()
    {
        return getTransmissionRatio(currentTransmission);
    }
}
