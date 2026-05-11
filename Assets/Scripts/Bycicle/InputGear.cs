using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(BycicleWheel))]
public class BycicleInputGear : MonoBehaviour
{
    protected BycicleWheel wheel;

    [SerializeField]
    public float BasicAngSpeed = 270.0f;

    [SerializeField]
    public float MaxAngSpeed = 720f;

    [SerializeField]
    public float BaseTorque = 5.0f;
    
// Pedals
    [SerializeField]
    private Pedal leftPedal = new Pedal(90.0f);
   
    [SerializeField]
    private Pedal rightPedal = new Pedal(270.0f);

//Input Setup
    [SerializeField]
    private PedalPushModeSetup pushModeSetup;

    [SerializeField]
    private PedalImpulseModeSetup impulseModeSetup;

//Input Mode
    [SerializeField]
    private float impulseModeActivation = .8f;

    enum InputMode
    {
        Push,
        Auto
    }

    [SerializeField]
    private InputMode inputMode = InputMode.Push;

//Bonus
    [SerializeField]
    private BonusType timingBonus;

    void Awake()
    {
        wheel = GetComponent<BycicleWheel>();
        enabled = wheel == null;
    }

    public void SetPedalsInput(Vector2 input)
    {
        leftPedal.UpdateInput(input.x);
        rightPedal.UpdateInput(input.y);
    }

    public void UpdateInputMode()
    {
        if (wheel.AngSpeed < impulseModeActivation * BasicAngSpeed)
        {
            inputMode = InputMode.Push;
        }
        else
        {
            if (inputMode == InputMode.Push)
            {
                timingBonus = BonusType.Ok;
            }

            inputMode = InputMode.Auto;
        }
    }

    public float GetBonusMult()
    {
        if (inputMode == InputMode.Auto)
        {
            return impulseModeSetup.GetBonusMult(timingBonus);
        }

        return 1.0f;
    }

    public void ApplyInputTorque(float deltaTime)
    {
        float inputTorque = .0f;
        switch (inputMode)
        {
            case InputMode.Auto:
                BonusType leftPedalBonus = leftPedal.ApplyImpulseBonus(wheel.Angle, impulseModeSetup);
                BonusType rightPedalBonus = rightPedal.ApplyImpulseBonus(wheel.Angle, impulseModeSetup);
                addTimingBonus(leftPedalBonus);
                addTimingBonus(rightPedalBonus);

                inputTorque = BaseTorque * GetBonusMult();
                break;

            case InputMode.Push:
                float pedalsOutput = leftPedal.GetPushOutput(wheel.Angle, pushModeSetup) + rightPedal.GetPushOutput(wheel.Angle, pushModeSetup);
                inputTorque = BaseTorque * pedalsOutput;
                break;
        }
        
        wheel.ApplyTorque(inputTorque, deltaTime, AngForceMode.Torque);
    }

    public int GetPedalEfficiency(PedalSide side)
    {
        Pedal pedal = getPedal(side);
        if (inputMode == InputMode.Push)
        {
            float angle = pedal.GetPositionAng(wheel.Angle);
            if (angle < pushModeSetup.MinEffectiveAng || angle >= pushModeSetup.MaxEffectiveAng)
            {
                return 0; //Not Effective
            }

            return 4; //Not Effective
        }

        switch (pedal.GetAngleBonus(wheel.Angle, impulseModeSetup))
        {
            case BonusType.Perfect: return 4;
            case BonusType.Excellent: return 3;
            case BonusType.Good: return 2;
            case BonusType.Ok: return 1;
            case BonusType.Bad: return 0;
        }

        return 0;
    }

    private Pedal getPedal(PedalSide side)
    {
        if (side == PedalSide.Left) return leftPedal;
        return rightPedal;
    }


    private void addTimingBonus(BonusType bonus)
    {
        int intTimingBonus = (int)timingBonus;
        switch (bonus)
        {
            case BonusType.Bad:
                intTimingBonus -= 2; 
            break;
            case BonusType.Ok:
                intTimingBonus -= 1; 
            break;
            case BonusType.Excellent:
                intTimingBonus += 1; 
            break;
            case BonusType.Perfect:
                intTimingBonus += 2; 
            break;
        }

        timingBonus = (BonusType)math.clamp(intTimingBonus, (int)BonusType.Bad, (int)BonusType.Perfect);
    }
}
