using System;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [SerializeField]
    private Rigidbody target;

    [SerializeField]
    private BycicleTransmission transmission;

    [SerializeField]
    private Camera cam;

    [SerializeField]
    private float focusDistance = 100f;

    [SerializeField]
    private float snapAngle = 1f;

    [SerializeField]
    private float baseAngSpeed = 90f;

    private Matrix4x4 transformOffset;

    void Awake()
    {
        transformOffset = target.transform.worldToLocalMatrix * transform.localToWorldMatrix;
    }

    void Update()
    {
        Vector3 perfectPosition = (target.transform.localToWorldMatrix * transformOffset).GetPosition();
        Vector3 newPosition = perfectPosition;

        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, Mathf.Lerp(60f, 90f, transmission.TransmissionInverseLerp()), Time.deltaTime);

        Vector3 focusPosition = target.position + target.transform.forward * focusDistance;
        Quaternion lookAtFocusRotation = Quaternion.LookRotation(focusPosition - newPosition, Vector3.up);
        float deltaAngle = Quaternion.Angle(transform.rotation, lookAtFocusRotation);
        float angSpeed = MathF.Sqrt(deltaAngle / baseAngSpeed) * baseAngSpeed;
        Quaternion newRotation = deltaAngle < snapAngle 
            ? lookAtFocusRotation 
            : Quaternion.Lerp(transform.rotation, lookAtFocusRotation, deltaAngle / (angSpeed * Time.deltaTime));
        
        transform.SetPositionAndRotation(newPosition, newRotation);
    }
}
