using UnityEngine;

public class BycicleWheelAvatar : MonoBehaviour
{
    [SerializeField]
    private BycicleWheel wheel;

    [SerializeField]
    private Transform avatar;

    void Awake()
    {
        enabled = wheel != null && avatar != null;
    }

    void Update()
    {
        avatar.SetLocalPositionAndRotation(avatar.transform.localPosition, Quaternion.Euler(wheel.Angle, .0f, .0f));
    }
}
