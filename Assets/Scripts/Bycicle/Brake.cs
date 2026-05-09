using UnityEngine;

[RequireComponent(typeof(BycicleWheel))]
public class BycicleBrake : MonoBehaviour
{
    protected BycicleWheel wheel;

    [SerializeField]
    protected float brakeInput = 0f;

    [SerializeField]
    protected float brakeTorque = 500.0f;

    void Awake()
    {
        wheel = GetComponent<BycicleWheel>();
        enabled = wheel == null;
    }

    public void SetBrakeInput(float input)
    {
        brakeInput = input;
        wheel.virtualWheel.SetBrakeFriction(brakeTorque * input);
    }

    public bool IsBrakeActive(float treshhold = .5f)
    {
        return brakeInput > treshhold;
    }
}
