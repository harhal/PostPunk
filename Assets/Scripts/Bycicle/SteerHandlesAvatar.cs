using UnityEngine;

public class BycicleSteerHandlesAvatar : MonoBehaviour
{
    [SerializeField]
    private SteeringHandles steeringHandles;

    [SerializeField]
    private Transform avatar;

    void Awake()
    {
        enabled = steeringHandles != null && avatar != null;
    }

    void Update()
    {
        avatar.SetLocalPositionAndRotation(avatar.localPosition, steeringHandles.GetLocalRotation());
    }
}
