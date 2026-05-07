using System;
using UnityEngine;

enum BonusType
{
    None = 0,
    Bad = 1,
    Ok = 2,
    Good = 3,
    Excellent = 4,
    Perfect = 5
}

[Serializable]
class PedalPushModeSetup
{
    [SerializeField]
    public float MinEffectiveAng = 90.0f;
    [SerializeField]
    public float MaxEffectiveAng = 270.0f;
}

[Serializable]
class PedalImpulseModeSetup
{
    [SerializeField]
    public float PerfectImpulseAng = 180.0f;
    
    [SerializeField]
    private float[] frameAngOffsets = new float[(int)BonusType.Perfect - 1] 
    {
        90.0f,  // Ok
        60.0f,  // Good
        45.0f,  // Excellent
        30.0f    // Perfect
    };
    
    [SerializeField]
    private float[] bonusImpulses = new float[(int)BonusType.Perfect] 
    {
        1.10f,   // Bad
        1.25f,  // Ok
        1.40f,   // Good
        1.55f,   // Excellent
        1.70f    // Perfect
    };

    public float GetBonusFrameOffset(BonusType bonus)
    {
        return frameAngOffsets[(int)bonus - 2];
    }

    public float GetBonusMult(BonusType bonus)
    {
        return bonusImpulses[(int)bonus - 1];
    }
}

public enum PedalSide
{
    Left,
    Right
}

[Serializable]
class Pedal
{
    [SerializeField]
    private float lastInput;

    enum ImpulseState
    {
        NoInput,
        Applied,
        Reset
    }

    [SerializeField]
    private ImpulseState impulseState = ImpulseState.NoInput;

    [SerializeField]
    private float angOffset;

    public Pedal(float angOffset)
    {
        this.angOffset = angOffset;
    }

    public void UpdateInput(float input)
    {
        lastInput = input;
    }

    public float GetPositionAng(float baseAngle)
    {
        return Mathf.Repeat(baseAngle + angOffset, 360.0f);
    }
  
    public float GetPushOutput(float baseAngle, PedalPushModeSetup setup)
    {
        float posAng = GetPositionAng(baseAngle);
        if (posAng < setup.MinEffectiveAng || posAng >= setup.MaxEffectiveAng)
        {
            return .0f;
        }

        return lastInput;
    }
  
    public BonusType ApplyImpulseBonus(float baseAngle, PedalImpulseModeSetup setup)
    {
        BonusType currentBonus = GetAngleBonus(baseAngle, setup);

        if (currentBonus == BonusType.Bad)
        {
            bool bMissedFrame = impulseState == ImpulseState.NoInput;
            impulseState = ImpulseState.Reset;

            return bMissedFrame ? BonusType.Bad : BonusType.None;
        }
        else
        {
            if (impulseState == ImpulseState.Reset)
            {
                impulseState = ImpulseState.NoInput;
            }

            if (impulseState == ImpulseState.NoInput && lastInput >= 1.0f)
            {
                impulseState = ImpulseState.Applied;
                return currentBonus;
            }
        }

        return BonusType.None;
    }
  
    public BonusType GetAngleBonus(float baseAngle, PedalImpulseModeSetup setup)
    {
        float posDiff =  Mathf.Abs(Mathf.DeltaAngle(GetPositionAng(baseAngle), setup.PerfectImpulseAng));
        if (posDiff <= setup.GetBonusFrameOffset(BonusType.Perfect))
        {
            return BonusType.Perfect;
        }
        if (posDiff <= setup.GetBonusFrameOffset(BonusType.Excellent))
        {
            return BonusType.Excellent;
        }
        if (posDiff <= setup.GetBonusFrameOffset(BonusType.Good))
        {
            return BonusType.Good;
        }
        if (posDiff <= setup.GetBonusFrameOffset(BonusType.Ok))
        {
            return BonusType.Ok;
        }

        return BonusType.Bad;
    }

    public float GetLastInput()
    {
        return lastInput;
    }
}