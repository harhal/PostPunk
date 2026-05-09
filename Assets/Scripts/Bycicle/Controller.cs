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

    private Vector2 pedalsInput;

    private bool controlModifier = false;

    private void OnLeftPedal(InputValue input)
    {
        pedalsInput.x = input.Get<float>();
        validateAndSendPedalsInput();
    }

    private void OnRightPedal(InputValue input)
    {
        pedalsInput.y = input.Get<float>();
        validateAndSendPedalsInput();
    }

    private void validateAndSendPedalsInput()
    {
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
    }

    private void OnTurnRight(InputValue input)
    {
            steeringHandles.SetSteeringInput(input.Get<float>());
    }

    private void OnControlModifier(InputValue input)
    {
        controlModifier = input.Get<float>() > .5f;
    }

    private void OnJump(InputValue input)
    {
        jumpSpring.RequestJump();
    }

    void Update()
    {
    }
}
