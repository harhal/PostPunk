using System;
using UnityEngine;

public class Address : MonoBehaviour
{
    [field: SerializeField]
    public AddressData data{get; private set;}

    [field: SerializeField]
    public Street street{get; private set;}

    public void SetData(AddressData data)
    {
        this.data = data;
    }

    public void SetStreet(Street street)
    {
        this.street = street;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public City GetCity()
    {
        if (street != null)
        {
            return street.GetCity();
        }

        if (transform.parent)
        {
            return transform.parent.GetComponent<City>();
        }
        else
        {
            return null;
        }
    }

    public void FreezeGameobject()
    {
        foreach (Component component in GetComponents<Component>())
        {
            if (component != this)
            {
                component.hideFlags = HideFlags.NotEditable;
            }
        }

        foreach (Transform child in transform)
        {
            child.hideFlags = HideFlags.HideInHierarchy;
        }
    }

    public void Sync()
    {
        transform.position = GetCity().GetMap().Nodes[data.AssignedNodeId].Position;
    }
}
