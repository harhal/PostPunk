using System;
using UnityEngine;
using UnityEngine.Splines;

public class DynamicGround : MonoBehaviour
{
    Vector3 velocity;

    [SerializeField]
    SplineContainer trajectory;

    [SerializeField]
    float loopTime = 10f;

    [SerializeField]
    bool isGround = false;

    public bool IsGround()
    {
        return isGround;
    }

    public Vector3 GetVelocityAt(Vector3 point, float deltaTime)
    {
        return ((Vector3)trajectory.Spline.EvaluatePosition(GetLocalTime(Time.time + deltaTime)) - (Vector3)trajectory.Spline.EvaluatePosition(GetLocalTime(Time.time))) / deltaTime;
    }

    void FixedUpdate()
    {
        transform.position = trajectory.transform.position + (Vector3)trajectory.Spline.EvaluatePosition(GetLocalTime(Time.time));
    }

    float GetLocalTime(float time)
    {
        return time % loopTime / loopTime;
    }
}
