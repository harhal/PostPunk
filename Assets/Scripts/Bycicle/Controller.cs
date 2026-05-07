using UnityEngine;
using UnityEngine.InputSystem;

public class BycicleController : MonoBehaviour
{   
    [SerializeField]
    private BycicleEngine bycicle;

    private float minPedalsInputDiff = .3f;

    private Vector2 pedalsInput;


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
        if (Mathf.Abs(pedalsInput.x - pedalsInput.y) > minPedalsInputDiff)
        {
            bycicle.SetPedalsInput(pedalsInput);
        }
        else
        {
            bycicle.SetPedalsInput(Vector2.zero);
        }
    }

    private void OnWeightForward(InputValue input)
    {
    }

    private void OnTurnRight(InputValue input)
    {
        bycicle.SetSteeringInput(input.Get<float>());
    }

    private void OnBrake(InputValue input)
    {
        bycicle.SetBrakeInput(input.Get<float>() > .5f);
    }

    private void OnJump(InputValue input)
    {
        bycicle.RequestJump();
    }

    void Update()
    {
    }
}
