using UnityEngine;

public class BycicleSteerHandlesAvatar : MonoBehaviour
{
    [SerializeField]
    private BycicleSteeringWheel steeringWheel;

    [SerializeField]
    private Transform avatar;

    void Awake()
    {
        enabled = steeringWheel != null && avatar != null;
    }

    void Update()
    {
        avatar.SetLocalPositionAndRotation(avatar.localPosition, steeringWheel.GetLocalRotation());
    }
}
