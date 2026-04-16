#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(Street))]
public class StreetEditor : Editor
{
    private ReorderableList _nodesList;
    private bool _editNodes = false;

    private void OnEnable()
    {
        Street streetTarget = target as Street;
        if (streetTarget == null)
        {
            return;
        }

        City city = streetTarget.GetCity();     
        if (city == null)
        {
            return;
        }

        city.SetSelectedStreet(streetTarget);

        _nodesList = new ReorderableList(streetTarget.data.Nodes, typeof(int), true, false, false, false);
        _nodesList.onReorderCallback += e => 
        {
            streetTarget.RegenerateMesh();
        };
        _nodesList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            EditorGUI.LabelField(rect, $"{streetTarget.data.Nodes[index]}");
        };
        _nodesList.multiSelect = false;

        Selection.selectionChanged += Repaint;
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
        SceneView.duringSceneGui += OnSceneGUIUpdated;
        CityNodesEditorWindow.SetActiveNodes(new HashSet<int>(streetTarget.data.Nodes));
    }

    private void OnDisable()
    {
        Street streetTarget = target as Street;
        if (streetTarget != null)
        {
            City city = streetTarget.GetCity();     
            if (city != null)
            {
                city.SetSelectedStreet(null);
            }
        }
       
        _nodesList = null;
        Selection.selectionChanged -= Repaint;
        EditorApplication.hierarchyChanged -= OnHierarchyChanged;
        SceneView.duringSceneGui -= OnSceneGUIUpdated;
        CityNodesEditorWindow.SetActiveNodes(new HashSet<int>());
    }

    private void OnHierarchyChanged()
    {
        Street streetTarget = target as Street;
        if (streetTarget == null)
        {
            return;
        }

        City city = streetTarget.GetCity();     
        if (city == null)
        {
            return;
        }

        if (streetTarget.name != streetTarget.data.SystemName)
        {
            streetTarget.data.SystemName = streetTarget.name;
            EditorUtility.SetDirty(city.GetMap());
            Repaint();
        }
    }

    private void OnSceneGUIUpdated(SceneView view)
    {
        Street streetTarget = target as Street;        
        if (streetTarget == null)
        {
            return;
        }

        City city = streetTarget.GetCity();     
        if (city == null)
        {
            return;
        }

        int selectedNode = CityNodesEditorWindow.GetNodeSelection();

        if (_editNodes)
        {
            if (selectedNode >= 0)
            {
                HashSet<int> streetNodesSet = new HashSet<int>(streetTarget.data.Nodes);
                if (streetNodesSet.Contains(selectedNode))
                {
                    streetTarget.data.Nodes.Remove(selectedNode);
                    EditorUtility.SetDirty(city.GetMap());
                    streetTarget.RegenerateMesh();
                }
                else
                {
                    streetTarget.data.Nodes.Add(selectedNode);
                    EditorUtility.SetDirty(city.GetMap());
                    streetTarget.RegenerateMesh();
                }

                CityNodesEditorWindow.SetNodeSelection(-1);
                CityNodesEditorWindow.SetActiveNodes(new HashSet<int>(streetTarget.data.Nodes));
            }
        }
        else
        {
            if (selectedNode < 0)
            {
                _nodesList.ClearSelection();
            }
            else
            {
                _nodesList.Select(streetTarget.data.Nodes.FindIndex(id => id == selectedNode));
            }
        }
    }

    public override void OnInspectorGUI()
    {
        Street streetTarget = target as Street;        
        if (streetTarget == null)
        {
            return;
        }

        City city = streetTarget.GetCity();     
        if (city == null)
        {
            return;
        }
        
        if (!city.bHasValidCachedData)
        {
            GUILayout.Label("Rebuild city data");
            if (GUILayout.Button("SelectCity"))
            {
                Selection.activeObject = city;
            }
            return;
        }
        
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label("SystemName");
            GUILayout.Label("DisplayName");
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            bool prevState = GUI.enabled;
            GUI.enabled = false;
            GUILayout.Label(streetTarget.data.SystemName, GUI.skin.textField);
            GUI.enabled = prevState;

            string newDisplayName = GUILayout.TextField(streetTarget.data.DisplayName);
            if (newDisplayName != streetTarget.data.DisplayName)
            {
                streetTarget.data.DisplayName = newDisplayName;
                EditorUtility.SetDirty(city.GetMap());
            }
        }

        EditorGUILayout.Space();
        GUILayout.Label("Nodes:");

        _nodesList.DoLayoutList();

        if (!CityNodesEditorWindow.IsActive())
        {
            return;
        }

        if (!_editNodes)
        {
            if (GUILayout.Button("Add or remove nodes"))
            {
                _editNodes = true;
                CityNodesEditorWindow.SetNodeSelection(-1);
            }
        }
        else
        {
            if (GUILayout.Button("Cancel"))
            {
                _editNodes = false;
            }
        }
    }
}
#endif
