using System.Transactions;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [SerializeField]
    private Transform target;

    private Matrix4x4 transformOffset;

    void Awake()
    {
        transformOffset = target.worldToLocalMatrix * transform.localToWorldMatrix;
    }

    void Update()
    {
        Matrix4x4 followMatrix = target.localToWorldMatrix * transformOffset;
        
        transform.SetPositionAndRotation(followMatrix.GetPosition(),  Quaternion.Euler(transform.rotation.eulerAngles.x, followMatrix.rotation.eulerAngles.y, transform.rotation.eulerAngles.z));
    }
}
