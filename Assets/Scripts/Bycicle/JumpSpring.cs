using UnityEngine;

public class BycicleJumpSpring : MonoBehaviour
{
    [SerializeField]
    float? lastRequestTime = null;

    [SerializeField]
    float requestLifeTime = .5f;

    [SerializeField]
    float jumpImpulse = 350f;
    
    public void RequestJump()
    {
        lastRequestTime = Time.time;
    }

    public Vector3 ApplyJumpImpulse()
    {
        if (lastRequestTime.HasValue && Time.time - lastRequestTime.Value > requestLifeTime)
        {
            lastRequestTime = null;
        }

        if (!lastRequestTime.HasValue)
        {
            return Vector3.zero;
        }

        lastRequestTime = null;
        return Vector3.up * jumpImpulse;
    }
}
