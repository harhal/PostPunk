using UnityEngine;

public class Courier : MonoBehaviour
{
    [SerializeField]
    Rigidbody body;

    [SerializeField]
    Animator animator;

    [SerializeField]
    BycicleEngine bycicle;

    Vector3 lastInput;

    [SerializeField]
    float walkVelocity = 1f;

    [SerializeField]
    string forwardVelocityAnimatorField = "forwardVelocity";

    [SerializeField]
    string sidewayVelocityAnimatorField = "sidewayVelocity";

    [SerializeField]
    string groundedAnimatorField = "Grounded";

    [SerializeField]
    string jumpAnimatorTrigger = "Jump";

    [SerializeField]
    LayerMask raycastMask = LayerMask.GetMask();

    [SerializeField]
    float capsuleHalfHeight = 1f;

    [SerializeField]
    float raycastGap = 0.01f;

    [SerializeField]
    float jumpImpulse = 10f;

    bool bGrounded = true;

    DynamicGround dynamicGround;

    public bool IsGrounded()
    {
        return bGrounded;
    }

    public void SetMovementInput(Vector2 input)
    {
        lastInput = new Vector3(input.x, 0f, input.y);
    }

    public void Jump()
    {
        if (IsGrounded())
        {
            body.AddForce(Vector3.up * jumpImpulse, ForceMode.Impulse);
            animator.SetBool(groundedAnimatorField, false);
            animator.SetTrigger(jumpAnimatorTrigger);
        }
    }

    public void SummonBycicle()
    {
        gameObject.SetActive(false);
        bycicle.transform.SetPositionAndRotation(transform.position, transform.rotation);
        bycicle.gameObject.SetActive(true);
    }

    void FixedUpdate()
    {
        Vector3 source = body.position + Vector3.down * capsuleHalfHeight;
        bGrounded = Physics.Raycast(source, Vector3.down, out RaycastHit hitInfo, raycastGap, raycastMask);

        animator.SetBool(groundedAnimatorField, bGrounded);

        dynamicGround = bGrounded ? hitInfo.collider.GetComponent<DynamicGround>() : null;
        Vector3 groundVelocity = dynamicGround != null ? dynamicGround.GetVelocityAt(hitInfo.point, Time.fixedDeltaTime) : Vector3.zero;
        
        if (IsGrounded())
        {
            body.linearVelocity = body.rotation * lastInput * walkVelocity + groundVelocity;
        }
        Vector3 localVelocity = PhysHelper.UnrotateForces(body, groundVelocity, Time.fixedDeltaTime, ForceMode.VelocityChange);
        animator.SetFloat(forwardVelocityAnimatorField, localVelocity.z / walkVelocity);
        animator.SetFloat(sidewayVelocityAnimatorField, localVelocity.x / walkVelocity);
    }
}
