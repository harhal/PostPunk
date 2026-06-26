using UnityEngine;
using UnityEngine.InputSystem;

public class CourierController : MonoBehaviour
{
    [SerializeField]
    Courier pawn;

    [SerializeField]
    private CharacterModeSwitcher characterSwitcher;

    [SerializeField]
    private FollowCamera followCamera;

    void OnMovement(InputValue input)
    {
        if (!enabled)
        {
            return;
        }

        pawn.SetMovementInput(input.Get<Vector2>());
    }

    void OnJump(InputValue input)
    {
        if (!enabled)
        {
            return;
        }

        if (input.Get<float>() > .5f)
        {
            pawn.Jump();
        }
    }

    void OnSummonBycicle(InputValue input)
    {
        if (!enabled)
        {
            return;
        }

        if (input.Get<float>() > .5f)
        {
            characterSwitcher.SetCharacterMode(CharacterMode.Bycicle);
        }
    }

    void OnLookAround(InputValue input)
    {        
        if (!enabled)
        {
            return;
        }

        followCamera.ApplyLookAroundInput(input.Get<Vector2>());
    }
}
