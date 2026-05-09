using UnityEngine;

public class SlaveTransform : MonoBehaviour
{
    [SerializeField]
    Transform masterTransform;

    private Matrix4x4 transformOffset;

    void Awake()
    {
        enabled = masterTransform != null;
        transformOffset = masterTransform.worldToLocalMatrix * transform.localToWorldMatrix;
    }

    void FixedUpdate()
    {
        Matrix4x4 followMatrix = masterTransform.localToWorldMatrix * transformOffset;
        
        transform.SetPositionAndRotation(followMatrix.GetPosition(),  followMatrix.rotation);
    }
}
