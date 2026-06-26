using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BycicleWheelCollider : MonoBehaviour
{
    [SerializeField]
    float radius = 1f;

    [SerializeField]
    float width = 0.01f;

    [SerializeField]
    float startAngle = 0f;

    [SerializeField]
    float endAngle = 270f;

    [SerializeField]
    int segmentsCount = 6;

    [SerializeField]
    float raycastGap = .001f;

    [SerializeField]
    LayerMask raycastMask = LayerMask.GetMask();

    [SerializeField]
    PhysicsMaterial material;

    [SerializeField]
    CapsuleCollider[] segmentsColliders;

    List<RaycastHit> contactPoints = new List<RaycastHit>();

    public List<RaycastHit> ContactPoints => contactPoints;

    public void CreateColliders()
    {
        segmentsColliders = new CapsuleCollider[segmentsCount * 2];
        float segmentAngle = (endAngle - startAngle) / segmentsCount;
        float halfSegmentAngle = segmentAngle * .5f;
        float distance = radius - width;
        float chordaLength = (Mathf.Sin(halfSegmentAngle * Mathf.Deg2Rad) * distance + width) * 2f;
        float chordaDist = Mathf.Cos(halfSegmentAngle * Mathf.Deg2Rad) * distance;
        Vector3 locOffset = Vector3.up * chordaDist;

        for (int idx = 0; idx < segmentsCount; idx++)
        {
            float localAngle = segmentAngle * idx + startAngle;
            Quaternion chordaRotation = Quaternion.AngleAxis(halfSegmentAngle + localAngle, Vector3.right);

            CapsuleCollider chordaCollider = new GameObject(gameObject.name + "_" + idx, typeof(CapsuleCollider)).GetComponent<CapsuleCollider>();
            chordaCollider.transform.parent = transform;
            chordaCollider.direction = 2;
            chordaCollider.radius = width;
            chordaCollider.height = chordaLength;
            chordaCollider.transform.SetLocalPositionAndRotation(chordaRotation * locOffset, chordaRotation);
            chordaCollider.material = material;
            chordaCollider.providesContacts = true;
            segmentsColliders[idx * 2] = chordaCollider;

            CapsuleCollider radiusCollider = new GameObject(gameObject.name + "_" + idx, typeof(CapsuleCollider)).GetComponent<CapsuleCollider>();
            radiusCollider.transform.parent = transform;
            radiusCollider.radius = width;
            radiusCollider.height = chordaDist;
            radiusCollider.transform.SetLocalPositionAndRotation(chordaRotation * locOffset * .5f, chordaRotation);
            radiusCollider.material = material;
            radiusCollider.providesContacts = true;
            segmentsColliders[idx * 2 + 1] = radiusCollider;
        }
    }

    private void OnDrawGizmosSelected()
    {
        UnityEditor.Handles.color = Color.blue;
        UnityEditor.Handles.DrawWireDisc(transform.position, transform.right, radius);
        
        float segmentAngle = (endAngle - startAngle) / segmentsCount;
        float halfSegmentAngle = segmentAngle * .5f;
        float distance = radius + raycastGap;
        float chordaLength = segmentAngle * Mathf.Deg2Rad * distance;
        Vector3 locOffset = Vector3.up * distance;

        Matrix4x4 mx = transform.localToWorldMatrix;

        UnityEditor.Handles.color = Color.red;
        for (int idx = 0; idx < segmentsCount; idx++)
        {
            float localAngle = segmentAngle * idx + startAngle;
            Quaternion radiusRotation = Quaternion.AngleAxis(localAngle, Vector3.right);
            Quaternion chordaRotation = Quaternion.AngleAxis(halfSegmentAngle + localAngle, Vector3.right);
            Vector3 source = mx.MultiplyPoint(radiusRotation * locOffset);
            Vector3 direction = mx.MultiplyVector(chordaRotation * Vector3.forward * chordaLength);
            UnityEditor.Handles.DrawLine(source, source + direction);
        }
        
        UnityEditor.Handles.color = Color.orange;
        foreach (RaycastHit contactPoint in contactPoints)
        {
            UnityEditor.Handles.DrawSolidDisc(contactPoint.point, contactPoint.point + contactPoint.normal, 0.01f);            
        }
    }

    public void UpdateContactPoints()
    {
        contactPoints = new List<RaycastHit>();

        float segmentAngle = (endAngle - startAngle) / segmentsCount;
        float halfSegmentAngle = segmentAngle * .5f;
        float distance = radius + raycastGap;
        float chordaLength = segmentAngle * Mathf.Deg2Rad * distance;
        Vector3 locOffset = Vector3.up * distance;

        Matrix4x4 mx = transform.localToWorldMatrix;

        for (int idx = 0; idx < segmentsCount; idx++)
        {
            float localAngle = segmentAngle * idx + startAngle;
            Quaternion radiusRotation = Quaternion.AngleAxis(localAngle, Vector3.right);
            Quaternion chordaRotation = Quaternion.AngleAxis(halfSegmentAngle + localAngle, Vector3.right);
            Vector3 source = mx.MultiplyPoint(radiusRotation * locOffset);
            Vector3 direction = mx.MultiplyVector(chordaRotation * Vector3.forward);
            contactPoints.AddRange(Physics.RaycastAll(source, direction, chordaLength, raycastMask));
        }
    }

    public Vector3 GetGroundVelocity(float deltaTime)
    {
        Vector3 sum = Vector3.zero;

        if (contactPoints.Count <= 0)
        {
            return sum;
        }

        foreach (RaycastHit contactPoint in ContactPoints)
        {
            DynamicGround ground = contactPoint.collider.GetComponent<DynamicGround>();
            if (ground != null)
            {
                sum += ground.GetVelocityAt(contactPoint.point, deltaTime);
            }
        }

        return sum / contactPoints.Count;
    }
}
