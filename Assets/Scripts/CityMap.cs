using System;
using System.Collections.Generic;
using UnityEngine;
using NodeId = System.Int32;
using StreetCacheId = System.Int32;
using Unity.Collections;

[Serializable]
public struct CityNodeData
{
    public NodeId Id;
    public Vector3 Position;

    public void UpdateId(int correctedNodeIdx)
    {
        Id = correctedNodeIdx;
    }
}

[Serializable]
public class StreetData
{
    public string SystemName;
    public string DisplayName;

    public List<NodeId> Nodes;
}

[Serializable]
public enum AddressType
{
    House,
    Crossroad
}

[Serializable]
public class AddressData
{
    public string SystemName;
    public string DisplayName;
    public NodeId AssignedNodeId;
    public AddressType Type;
    public string streetName;

    internal void UpdateNodeId(int selectedNodeIdx)
    {
        AssignedNodeId = selectedNodeIdx;
    }
}

[CreateAssetMenu(fileName = "CityMap", menuName = "Data/CityMap")]
public class CityMap : ScriptableObject, ISerializationCallbackReceiver
{
    [SerializeField]
    public List<CityNodeData> Nodes;
    
    [SerializeField]
    public List<StreetData> Streets;

    [SerializeField]
    public List<AddressData> Addresses;
    
    [field: SerializeField]
    public StreetCacheId?[] Edges {get; private set;}

    public StreetData GetStreetByEdge(NodeId nodeA, NodeId nodeB)
    {
        if (nodeA == nodeB)
        {
            return null;
        }

        StreetCacheId? edgeStreet = Edges[GetEdgeIdx(nodeA, nodeB)];

        return edgeStreet.HasValue ? Streets[edgeStreet.Value] : null;
    }

    public int GetEdgeIdx(NodeId nodeA, NodeId nodeB)
    {
        if (nodeA == nodeB)
        {
            return -1;
        }

        if (nodeA > nodeB)
        {
            return GetEdgeIdx(nodeB, nodeA);
        }

        return nodeB * (nodeB - 1) / 2 + nodeA;
    }

    public void OnAfterDeserialize()
    {
    }

    public void OnBeforeSerialize()
    {
        Edges = new StreetCacheId?[Math.Max(0, GetEdgeIdx(0, Nodes.Count))];
        Array.Fill(Edges, null);
        for (StreetCacheId streetCacheId = 0; streetCacheId < Streets.Count; streetCacheId++)
        {
            StreetData street = Streets[streetCacheId];
            for (int streetNodeIdx = 1; streetNodeIdx < street.Nodes.Count; streetNodeIdx++)
            {
                NodeId nodeA = street.Nodes[streetNodeIdx - 1];
                NodeId nodeB = street.Nodes[streetNodeIdx];
                Edges[GetEdgeIdx(nodeA, nodeB)] = streetCacheId;
            }
        }
    }

    public CityNodeData GetCityNode(NodeId nodeId)
    {
        if (nodeId < 0 || nodeId >= Nodes.Count || Nodes[nodeId].Id != nodeId)
        {
            throw new IndexOutOfRangeException();
        }

        return Nodes[nodeId];
    }

    public bool RemoveNode(NodeId selectedNodeIdx)
    {
        if (Addresses.FindIndex(addr => addr.AssignedNodeId == selectedNodeIdx) >= 0)
        {
            return false;
        }

        for (int idx = 0; idx < Streets.Count; idx++)
        {
            Streets[idx].Nodes.Remove(selectedNodeIdx);
        }

        Nodes.RemoveAtSwapBack(selectedNodeIdx);

        if (Nodes.Count > selectedNodeIdx)
        {
            NodeId movedNodeId = Nodes.Count;
            Nodes[selectedNodeIdx].UpdateId(selectedNodeIdx);

            for (int idx = 0; idx < Addresses.Count; idx++)
            {
                if (Addresses[idx].AssignedNodeId == movedNodeId)
                {
                    Addresses[idx].UpdateNodeId(selectedNodeIdx);
                }
            }

            for (int idx = 0; idx < Streets.Count; idx++)
            {
                for (int jdx = 0; jdx < Streets[idx].Nodes.Count; jdx++)
                {
                    if (Streets[idx].Nodes[jdx] == movedNodeId)
                    {
                        Streets[idx].Nodes[jdx] = selectedNodeIdx;
                    }
                }
            }
        }

        return true;
    }
}
