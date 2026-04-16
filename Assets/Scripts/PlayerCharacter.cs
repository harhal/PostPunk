using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    public float MaxSpeed = 10.0f;
    public float BaseAcceleration = 1.0f;
    public float CurrentSpeed = .0f;
    public int CurrentNode = 0;
    public LinkedList<int> Route = new LinkedList<int>();
    public float DistanceFromCurrentNode = .0f;
    public GameObject MapAvatar;
    public City City;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Route.AddLast(1);
        Route.AddLast(2);
        Route.AddLast(3);
        Route.AddLast(4);
        Route.AddLast(5);
    }

    // Update is called once per frame
    void Update()
    {
        if (City == null)
        {
            return;
        }

        if (Route.Count > 0)
        {
            CurrentSpeed = Mathf.Min(MaxSpeed, CurrentSpeed + BaseAcceleration * Time.deltaTime);
        }

        float deltaDistance = CurrentSpeed * Time.deltaTime;

        while (Route.Count > 0 && deltaDistance > .0f)
        {
            Vector3 currentNodePosition = City.GetMap().Nodes[CurrentNode].Position;
            Vector3 nextNodePosition = City.GetMap().Nodes[Route.First.Value].Position;
            float distance = Vector2.Distance(nextNodePosition, currentNodePosition);
            if (DistanceFromCurrentNode + deltaDistance >= distance)
            {
                DistanceFromCurrentNode = .0f;
                deltaDistance -= distance - DistanceFromCurrentNode;
                CurrentNode = Route.First.Value;
                Route.RemoveFirst();
            }
            else
            {
                DistanceFromCurrentNode += deltaDistance;
                deltaDistance = .0f;
            }
        }
        
        if (Route.Count <= 0)
        {
            CurrentSpeed = .0f;
            DistanceFromCurrentNode = .0f;
        }

        if (MapAvatar != null)
        {
            Vector3 avatarPosition = City.GetMap().Nodes[CurrentNode].Position;
            if (Route.Count > 0)
            {
                Vector3 nextNodePosition = City.GetMap().Nodes[Route.First.Value].Position;
                Vector3 direction = (nextNodePosition - avatarPosition).normalized;
                avatarPosition += direction * DistanceFromCurrentNode;
                MapAvatar.transform.rotation = Quaternion.LookRotation(Vector3.back, direction);
            }

            MapAvatar.transform.position = avatarPosition;
        }
    }
}
