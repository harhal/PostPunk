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
    private Vector3 cameraLocalPosition = Vector3.zero;

    [SerializeField]
    private Vector3 focusOffset = Vector3.zero;

    [SerializeField]
    private Vector3 additionalTargetOffset = Vector3.zero;

    [SerializeField]
    private float locusDistance = 100f;

    [SerializeField]
    private float snapAngle = 1f;

    [SerializeField]
    private float baseAngSpeed = 90f;

    //private Matrix4x4 transformOffset;

    void Awake()
    {
        //transformOffset = target.transform.worldToLocalMatrix * transform.localToWorldMatrix;
    }

    void Update()
    {
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, Mathf.Lerp(60f, 90f, transmission.TransmissionInverseLerp()), Time.deltaTime);

        Vector3 perfectPosition = getPerfectTransform().GetPosition();
        Vector3 newPosition = perfectPosition;

        Quaternion lookAtFocusRotation = getPerfectTransform().rotation;
        float deltaAngle = Quaternion.Angle(transform.rotation, lookAtFocusRotation);
        float angSpeed = MathF.Sqrt(deltaAngle / baseAngSpeed) * baseAngSpeed;
        Quaternion newRotation = deltaAngle < snapAngle 
            ? lookAtFocusRotation 
            : Quaternion.Lerp(transform.rotation, lookAtFocusRotation, deltaAngle / (angSpeed * Time.deltaTime));
        
        transform.SetPositionAndRotation(newPosition, newRotation);
    }

    Matrix4x4 getPerfectTransform()
    {
        Vector3 locusLocalPos = target.transform.forward * locusDistance;
        Vector3 camDirection = (locusLocalPos - focusOffset).normalized;
        Vector3 additionalTargetDir = additionalTargetOffset - locusLocalPos;

        float h = Vector3.Cross(-camDirection, additionalTargetDir).magnitude;
        Vector3 camLocalPos = locusLocalPos + Vector3.Project(additionalTargetDir, -camDirection) - camDirection * (h / Mathf.Tan(cam.fieldOfView * .5f * Mathf.Deg2Rad));

        Quaternion camWorldRotation = Quaternion.LookRotation(locusLocalPos - camLocalPos, Vector3.up);

        Vector3 camWorldPos = target.position + camLocalPos + camWorldRotation * cameraLocalPosition;


        return Matrix4x4.Translate(camWorldPos) * Matrix4x4.Rotate(camWorldRotation);
    }

    void OnDrawGizmosSelected()
    {
        transform.position = getPerfectTransform().GetPosition();
        transform.rotation = getPerfectTransform().rotation;

        UnityEditor.Handles.color = Color.blue;        
        UnityEditor.Handles.DrawLine(cam.transform.position, target.position + focusOffset);

        UnityEditor.Handles.color = Color.navajoWhite;        
        UnityEditor.Handles.DrawWireCube(target.position + additionalTargetOffset, Vector3.one * .05f);

        UnityEditor.Handles.color = Color.green;        
        UnityEditor.Handles.DrawWireCube(target.position + target.transform.forward * locusDistance, Vector3.one * .05f);
    }
}
