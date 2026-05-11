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
    private Vector3 cameraOffset = Vector3.zero;

    [SerializeField]
    private Vector3 focusAOffset = Vector3.zero;

    [SerializeField]
    private Vector3 focusBOffset = Vector3.zero;

    [SerializeField]
    private Vector3 focusCOffset = Vector3.zero;

    [SerializeField]
    private float snapAngle = 1f;

    [SerializeField]
    private float baseAngSpeed = 90f;

    void Awake()
    {
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
        Vector3 focusCLocalPos = target.rotation * focusCOffset;
        Vector3 camDirection = (focusCLocalPos - focusAOffset).normalized;
        Vector3 additionalTargetDir = target.rotation * focusBOffset - focusCLocalPos;

        float h = Vector3.Cross(-camDirection, additionalTargetDir).magnitude;
        Vector3 camLocalPos = focusCLocalPos + Vector3.Project(additionalTargetDir, -camDirection) - camDirection * (h / Mathf.Tan(cam.fieldOfView * .5f * Mathf.Deg2Rad));

        Quaternion camWorldRotation = Quaternion.LookRotation(focusCLocalPos - camLocalPos, Vector3.up);

        Vector3 camWorldPos = target.position + camLocalPos + camWorldRotation * cameraOffset;


        return Matrix4x4.Translate(camWorldPos) * Matrix4x4.Rotate(camWorldRotation);
    }

    void OnDrawGizmosSelected()
    {
        transform.position = getPerfectTransform().GetPosition();
        transform.rotation = getPerfectTransform().rotation;

        UnityEditor.Handles.color = Color.blue;
        UnityEditor.Handles.DrawLine(cam.transform.position, target.position + focusAOffset);
        UnityEditor.Handles.Label(target.position + focusAOffset, "A");

        UnityEditor.Handles.color = Color.navajoWhite;        
        UnityEditor.Handles.DrawWireCube(target.position + focusBOffset, Vector3.one * .05f);
        UnityEditor.Handles.Label(target.position + focusBOffset, "B");

        UnityEditor.Handles.color = Color.green;        
        UnityEditor.Handles.DrawWireCube(target.position + target.rotation * focusCOffset, Vector3.one * .05f);
        UnityEditor.Handles.Label(target.position + target.rotation * focusCOffset, "C");
    }
}
