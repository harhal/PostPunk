using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class City : MonoBehaviour
{
    [SerializeField]
    private CityMap map;

    [SerializeField]
    private CityPrefabsPalette palette;

    private Dictionary<string, Street> Streets = new Dictionary<string, Street>();

    private Dictionary<string, Address> Addresses = new Dictionary<string, Address>();

    private Dictionary<int, Address> NodeAddresses = new Dictionary<int, Address>();

    [NonSerialized]
    private Street _selectedStreet = null;

    [NonSerialized]
    public bool bHasValidCachedData = false;

    public CityMap GetMap()
    {
        return map;
    }

    public CityPrefabsPalette GetPalette()
    {
        return palette;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void LoadCity()
    {
        Streets = new Dictionary<string, Street>();
        foreach (StreetData streetData in map.Streets)
        {
            SpawnStreet(streetData);
        }

        Addresses = new Dictionary<string, Address>();
        NodeAddresses = new Dictionary<int, Address>();
        foreach (AddressData addressData in map.Addresses)
        {
            SpawnAddress(addressData);
        }

        bHasValidCachedData = true;
    }

    public void UpdateRelatedStreets(int selectedNodeIdx)
    {
        foreach (StreetData street in map.Streets)
        {
            int idx = street.Nodes.FindIndex(e => e == selectedNodeIdx);
            if (idx >= 0)
            {
                Streets[street.SystemName].RegenerateMesh();
            }
        }
    }

    public void UpdateRelatedAddress(int selectedNodeIdx)
    {
        Address assocatedAddress = GetAddressByNodeId(selectedNodeIdx);
        if (assocatedAddress != null)
        {
            assocatedAddress.Sync();
        }
    }

    public Address GetAddressByNodeId(int nodeId)
    {
        if (NodeAddresses.TryGetValue(nodeId, out var address))
        {
            return address;
        }

        return null;
    }

    public Street GetStreetObject(string streetSystemName)
    {
        if (Streets.TryGetValue(streetSystemName, out var street))
        {
            return street;
        }

        return null;
    }

    public Street[] GetStreetsObjects()
    {
        return Streets.Values.ToArray();
    }

    public void SetSelectedStreet(Street selectedStreet)
    {
        _selectedStreet = selectedStreet;
        foreach (Street street in GetStreetsObjects())
        {
            street.SetHighlitedState(street == _selectedStreet);
        }
    }

    public Street SpawnStreet(StreetData streetData)
    {
        Street newStreet = Instantiate(palette.StreetPrefab, transform);

        newStreet.name = streetData.SystemName;
        newStreet.SetData(streetData);
        newStreet.InitSpline();
        newStreet.RegenerateMesh();

        newStreet.FreezeGameobject();

        Streets.Add(streetData.SystemName, newStreet);

        return newStreet;
    }

    public Address SpawnAddress(AddressData addressData)
    {
        Vector3 addressPosition = map.GetCityNode(addressData.AssignedNodeId).Position;
        Street street = GetStreetObject(addressData.streetName);
        Address newAddress = Instantiate(palette.AddressPrefab, addressPosition, Quaternion.identity, street != null ? street.transform : transform);
        newAddress.name = addressData.SystemName;
        newAddress.SetData(addressData);
        newAddress.SetStreet(street);

        newAddress.FreezeGameobject();

        Addresses.Add(addressData.SystemName, newAddress);
        NodeAddresses.Add(addressData.AssignedNodeId, newAddress);

        return newAddress;
    }
}
