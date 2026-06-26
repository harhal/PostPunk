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
    private Quaternion cameraRotOffset = Quaternion.identity;

    [SerializeField]
    private Vector2 cameraRotationInput = Vector2.zero;

    [SerializeField]
    private Vector2 cameraRotationInputVelocity = Vector2.one;

    [SerializeField]
    private float snapAngle = 1f;

    [SerializeField]
    private float baseFov = 60f;

    [SerializeField]
    private float fastFov = 30f;

    [SerializeField]
    private float baseAngSpeed = 90f;

    [SerializeField]
    private float baseVelocity = 60f;

    void Awake()
    {
    }

    void Update()
    {
        if (transmission != null)
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, Mathf.Lerp(baseFov, fastFov, transmission.TransmissionInverseLerp()), Time.deltaTime);
        }
        else
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 60f, Time.deltaTime);
        }

        Vector3 angles = cameraRotOffset.eulerAngles;
        cameraRotOffset = Quaternion.Euler(Mathf.Repeat(angles.x + cameraRotationInput.y * cameraRotationInputVelocity.y * Time.deltaTime, 360f), Mathf.Repeat(angles.x + 90f + cameraRotationInput.x * cameraRotationInputVelocity.x * Time.deltaTime, 180f) - 90f, 0f);

        Vector3 perfectPosition = getPerfectTransform().GetPosition() + PhysHelper.UnrotateForces(target, Vector3.zero, Time.deltaTime, ForceMode.VelocityChange) * Time.deltaTime;;
        float dist = Vector3.Distance(perfectPosition, cam.transform.position);
        Vector3 newPosition = Vector3.Lerp(cam.transform.position, perfectPosition, Mathf.Clamp01(baseVelocity * Time.deltaTime / dist ));
        
        Quaternion lookAtFocusRotation = getPerfectTransform().rotation;
        float deltaAngle = Quaternion.Angle(cam.transform.rotation, lookAtFocusRotation);
        float angSpeed = MathF.Sqrt(deltaAngle / baseAngSpeed) * baseAngSpeed;
        Quaternion newRotation = deltaAngle < snapAngle 
            ? lookAtFocusRotation 
            : Quaternion.Lerp(cam.transform.rotation, lookAtFocusRotation, Mathf.Clamp01(angSpeed * Time.deltaTime / deltaAngle));
        
        cam.transform.SetPositionAndRotation(newPosition, newRotation);
    }

    Matrix4x4 getPerfectTransform()
    {
        Vector3 focusCLocalPos = cameraRotOffset * target.rotation * focusCOffset;
        Vector3 camDirection = (focusCLocalPos - focusAOffset).normalized;
        Vector3 additionalTargetDir = cameraRotOffset * target.rotation * focusBOffset - focusCLocalPos;

        float h = Vector3.Cross(-camDirection, additionalTargetDir).magnitude;
        Vector3 camLocalPos = focusCLocalPos + Vector3.Project(additionalTargetDir, -camDirection) - camDirection * (h / Mathf.Tan(cam.fieldOfView * .5f * Mathf.Deg2Rad));

        Quaternion camWorldRotation = Quaternion.LookRotation(focusCLocalPos - camLocalPos, Vector3.up);

        Vector3 camWorldPos = target.position + camLocalPos + camWorldRotation * cameraOffset;


        return Matrix4x4.Translate(camWorldPos) * Matrix4x4.Rotate(camWorldRotation);
    }

    void OnDrawGizmosSelected()
    {
        if (target == null || !target.gameObject.activeSelf)
        {
            return;
        }
        
        cam.transform.position = getPerfectTransform().GetPosition();
        cam.transform.rotation = getPerfectTransform().rotation;

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

    public void ApplyLookAroundInput(Vector2 input)
    {
        cameraRotationInput = -input;
    }
}
