using UnityEngine;
using UnityEngine.InputSystem;

public class BycicleController : MonoBehaviour
{
    [SerializeField]
    private BycicleInputGear gear;

    [SerializeField]
    private SteeringHandles steeringHandles;

    [SerializeField]
    private BycicleBrake frontBrake;

    [SerializeField]
    private BycicleBrake driveBrake;

    [SerializeField]
    private BycicleJumpSpring jumpSpring;

    [SerializeField]
    private CharacterModeSwitcher characterSwitcher;

    private Vector2 pedalsInput;

    private bool controlModifier = false;

    private void OnLeftPedal(InputValue input)
    {
        if (!enabled)
        {
            return;
        }

        pedalsInput.x = input.Get<float>();
        validateAndSendPedalsInput();
    }

    private void OnRightPedal(InputValue input)
    {
        if (!enabled)
        {
            return;
        }
        
        pedalsInput.y = input.Get<float>();
        validateAndSendPedalsInput();
    }

    private void validateAndSendPedalsInput()
    {
        if (!enabled)
        {
            return;
        }
        
        if (controlModifier)
        {
            frontBrake.SetBrakeInput(pedalsInput.x);
            driveBrake.SetBrakeInput(pedalsInput.y);
            return;
        }

        frontBrake.SetBrakeInput(0f);
        driveBrake.SetBrakeInput(0f);

        if (pedalsInput.x > pedalsInput.y)
        {
            gear.SetPedalsInput(new Vector2(pedalsInput.x, 0f));
        }
        else
        {
            gear.SetPedalsInput(new Vector2(0f, pedalsInput.y));
        }
    }

    private void OnWeightForward(InputValue input)
    {
        if (!enabled)
        {
            return;
        }
        
    }

    private void OnTurnRight(InputValue input)
    {
        if (!enabled)
        {
            return;
        }
        
        steeringHandles.SetSteeringInput(input.Get<float>());
    }

    private void OnControlModifier(InputValue input)
    {
        if (!enabled)
        {
            return;
        }
        
        controlModifier = input.Get<float>() > .5f;
    }

    private void OnJump(InputValue input)
    {
        if (!enabled)
        {
            return;
        }
        
        jumpSpring.RequestJump();
    }

    void OnDismissBycicle(InputValue input)
    {
        if (!enabled)
        {
            return;
        }

        if (input.Get<float>() > .5f)
        {
            characterSwitcher.SetCharacterMode(CharacterMode.Courier);
        }
    }

    void Update()
    {
    }
}
