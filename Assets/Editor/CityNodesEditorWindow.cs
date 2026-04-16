#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class CityNodesEditorWindow : EditorWindow
{
    private City _city;
    private int _selectedNodeIdx = -1;
    private bool _addingNode = false;

    private bool _bEditNodes = true;

    HashSet<int> _activeNodes = new HashSet<int>();

    private static CityNodesEditorWindow _Wnd;

    public static bool IsActive()
    {
        return _Wnd != null;
    }

    public static void SetNodeSelection(int nodeId)
    {
        if (_Wnd != null)
        {
            _Wnd._selectedNodeIdx = nodeId;
            _Wnd.Repaint();
            SceneView.RepaintAll();
        }
    }

    public static int GetNodeSelection()
    {
        if (_Wnd == null)
        {
            return -1;
        }
        return _Wnd._selectedNodeIdx;
    }

    public static void SetActiveNodes(HashSet<int> activeNodes)
    {
        if (_Wnd != null)
        {
            _Wnd._activeNodes = activeNodes;
        }
    }

    public static void Open(City city)
    {
        var wnd = GetWindow<CityNodesEditorWindow>("City nodes editor");
        wnd._city = city;
        wnd.Show();
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        Selection.selectionChanged += Repaint;

        _Wnd = this;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        Selection.selectionChanged -= Repaint;

        _Wnd = null;
    }
    
    private void OnGUI()
    {
        if (_city == null)
        {
            EditorGUILayout.HelpBox("Set an city object to edit.", MessageType.Info);
            return;
        }
        
        if (!_city.bHasValidCachedData)
        {
            GUILayout.Label("Rebuild city data");
            if (GUILayout.Button("SelectCity"))
            {
                Selection.activeObject = _city;
            }
            return;
        }

        EditorGUILayout.ObjectField("City", _city, typeof(City), false);

        if (_addingNode)
        {
            GUILayout.Label("Adding new node");
            if (GUILayout.Button("Cancel"))
            {
                _addingNode = false;
            }
            return;
        }
        
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.Space();
            if (GUILayout.Button("SelectCity"))
            {
                Selection.activeObject = _city;
            }
        }
        
        EditorGUILayout.Separator();

        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label("Nodes:");

            if (GUILayout.Toggle(_bEditNodes, "Show edit node handle") != _bEditNodes)
            {
                _bEditNodes = !_bEditNodes;
                SceneView.RepaintAll();
            }
        }

        Color defaultBackground = GUI.backgroundColor;
        Color hlBackgroundColor = Color.blue;

        const int rowLength = 10;
        for (int startNodeId = 0; startNodeId < _city.GetMap().Nodes.Count; startNodeId += rowLength)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                for (int collumnIdx = 0; collumnIdx < rowLength; collumnIdx++)
                {
                    bool bIsValid = startNodeId + collumnIdx < _city.GetMap().Nodes.Count;
                    using (new EditorGUI.DisabledScope(!bIsValid))
                    {
                        bool isSelected = _selectedNodeIdx == startNodeId + collumnIdx;
                        GUI.backgroundColor = isSelected ? hlBackgroundColor : defaultBackground;
                        if (GUILayout.Button(bIsValid ? $"{startNodeId + collumnIdx}" : "") && bIsValid)
                        {
                            if (isSelected)
                            {
                                SceneView.lastActiveSceneView.LookAt(_city.GetMap().Nodes[startNodeId + collumnIdx].Position);
                            }
                            else
                            {
                                _selectedNodeIdx = startNodeId + collumnIdx;
                            }
                        }
                    }
                }
            }
        }
        GUI.backgroundColor = defaultBackground;

        EditorGUILayout.Space();
        
        bool bSelectionValid = _selectedNodeIdx >= 0 && _selectedNodeIdx < _city.GetMap().Nodes.Count;
        
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Add Node"))
            {
                AddNode();
            }
            
            using (new EditorGUI.DisabledScope(!bSelectionValid))
            {
                if (GUILayout.Button("Remove Node"))
                {
                    RemoveNode();
                }
            }
        }

        if (bSelectionValid)
        {
            EditorGUILayout.Space();
            EditorGUILayout.Separator();

            GUILayout.Label($"Selected node {_selectedNodeIdx}");
            GUILayout.Label("Associated address:");
            var associatedAddress = _city.GetAddressByNodeId(_selectedNodeIdx);
            if (associatedAddress != null)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label($"\t{associatedAddress.data.DisplayName}({associatedAddress.data.SystemName})");
                    if (GUILayout.Button("Select"))
                    {
                        Selection.activeObject = associatedAddress;
                    }
                }
            }
            else
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("\tNone");
                    if (GUILayout.Button("Create"))
                    {
                        string guid = Guid.NewGuid().ToShortString();
                        AddressData newAddress = new AddressData()
                        {
                            SystemName = $"Addr_{guid}",
                            DisplayName = $"Addr_{guid}", 
                            AssignedNodeId = _selectedNodeIdx, 
                            Type = AddressType.House
                        };

                        _city.GetMap().Addresses.Add(newAddress);
                        Selection.activeObject = _city.SpawnAddress(newAddress);
                    }
                }
            }

            bool isEmpty = true;
            GUILayout.Label("Passing streets:");
            foreach (var street in _city.GetStreetsObjects())
            {
                if (street.data.Nodes.Contains(_selectedNodeIdx))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Label($"\t{street.data.DisplayName}({street.data.SystemName})");
                        if (GUILayout.Button("Select"))
                        {
                            Selection.activeObject = street;
                        }
                    }

                    isEmpty = false;
                }
            }

            if (isEmpty)
            {
                GUILayout.Label("\tNone");
            }

            if (GUILayout.Button("StartNewStreet"))
            {
                string guid = Guid.NewGuid().ToShortString();
                StreetData newStreet = new StreetData()
                {
                    SystemName = $"Street_{guid}",
                    DisplayName = $"Street_{guid}",
                    Nodes = new List<int>(){_selectedNodeIdx}
                };

                _city.GetMap().Streets.Add(newStreet);
                Selection.activeObject = _city.SpawnStreet(newStreet);
            }
        }
    }

    private void AddNode()
    {
        _addingNode = true;

        _selectedNodeIdx = -1;
    }

    private void RemoveNode()
    {
        int assignedAddressIdx = _city.GetMap().Addresses.FindIndex(addr => addr.AssignedNodeId == _selectedNodeIdx);
        if (assignedAddressIdx >= 0)
        {
            string addressName = _city.GetMap().Addresses[assignedAddressIdx].SystemName;
            EditorGUILayout.HelpBox($"Selected node has associated adress `{addressName}`", MessageType.Error);
            return;
        }

        _city.UpdateRelatedStreets(_selectedNodeIdx);
        _city.GetMap().RemoveNode(_selectedNodeIdx);
        EditorUtility.SetDirty(_city.GetMap());

        _selectedNodeIdx = -1;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (_city == null)
        {
            return;
        }

        DrawHandles(sceneView);
    }

    private void DrawHandles(SceneView sceneView)
    {
        var nodes = _city.GetMap().Nodes;

        if (_addingNode)
        {
            Vector3? newNodePos = CityNodesEditorHelper.DrawAddNode(sceneView, nodes.Count);
            if (newNodePos.HasValue)
            {
                _selectedNodeIdx = nodes.Count;
                Undo.RecordObject(_city.GetMap(), "Add city node");
                nodes.Add(new CityNodeData(){Id = nodes.Count, Position = newNodePos.Value});
                EditorUtility.SetDirty(_city.GetMap());
            }
        }

        if (_activeNodes == null || _activeNodes.Count <= 0)
        {
            CityNodesEditorHelper.DrawNodes(sceneView, nodes, ref _selectedNodeIdx);
        }
        else
        {
            CityNodesEditorHelper.DrawNodes(sceneView, nodes, ref _selectedNodeIdx, filter: node => !_activeNodes.Contains(node.Id), sizeMultiplier: 0.8f, overrideDefaultHandleColor: Color.gray);
            CityNodesEditorHelper.DrawNodes(sceneView, nodes, ref _selectedNodeIdx, filter: node => _activeNodes.Contains(node.Id), sizeMultiplier: 1.0f);
        }

        if (_selectedNodeIdx >= 0 && _selectedNodeIdx < nodes.Count && _bEditNodes)
        {
            var node = nodes[_selectedNodeIdx];
            Vector3 nodePosition = node.Position;
            if (CityNodesEditorHelper.DrawMoveHandle(ref nodePosition))
            {
                Undo.RecordObject(_city.GetMap(), "Move city node");
                node.Position = nodePosition;
                nodes[_selectedNodeIdx] = node;
                _city.UpdateRelatedStreets(_selectedNodeIdx);
                _city.UpdateRelatedAddress(_selectedNodeIdx);
                EditorUtility.SetDirty(_city.GetMap());
            }
        }

        sceneView.Repaint();
        Repaint();
    }
}
#endif
