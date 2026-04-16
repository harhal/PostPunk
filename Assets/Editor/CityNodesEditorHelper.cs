#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

static class CityNodesEditorHelper
{
    public static Vector3? DrawAddNode(SceneView sceneView, int iD)
    {
        Handles.BeginGUI();
        GUILayout.BeginArea(new Rect(10, 10, 280, 90), "Click to add a new node", GUI.skin.window);
        GUILayout.EndArea();
        Handles.EndGUI();
        Event evnt = Event.current;
        
        Ray ray = HandleUtility.GUIPointToWorldRay(evnt.mousePosition);
        const float fixedZ = 10;
        Plane plane = new Plane(-ray.direction, Vector3.forward * fixedZ);
        if (plane.Raycast(ray, out float distance))
        {
            Vector3 currentPosition = ray.GetPoint(distance);
            float size = HandleUtility.GetHandleSize(currentPosition) * 0.15f;
            Handles.DrawSolidDisc(currentPosition, -ray.direction, size);
            if (sceneView.position.Contains(evnt.mousePosition + sceneView.position.position)
            && evnt.type == EventType.MouseDown)
            {
                if (evnt.button == 0)
                {
                    evnt.Use();
                    return currentPosition;
                }
            }
        }
        sceneView.Repaint();
        return null;
    }
    public static void DrawNodes(SceneView sceneView, List<CityNodeData> nodes, ref int selectedID, Func<CityNodeData, bool> filter = null, Color? overrideDefaultHandleColor = null, Color? overrideDefaultTextColor = null, float sizeMultiplier = 1.0f)
    {
        GUIStyle labelStyle = new GUIStyle();
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.normal.textColor = overrideDefaultTextColor.HasValue ? overrideDefaultTextColor.Value : Color.black;

        GUIStyle highlightedLabelStyle = new GUIStyle();
        highlightedLabelStyle.alignment = TextAnchor.MiddleCenter;
        highlightedLabelStyle.normal.textColor = Color.white;

        Color defaultHandlesColor = Handles.color;

        for (int idx = 0; idx < nodes.Count; idx++)
        {
            var node = nodes[idx];

            if (filter != null && !filter(node))
            {
                continue;
            }

            float size = HandleUtility.GetHandleSize(node.Position) * 0.3f * (selectedID == idx ? 2.0f : sizeMultiplier);

            Handles.color = selectedID == idx ? Color.blue : overrideDefaultHandleColor.HasValue ? overrideDefaultHandleColor.Value : defaultHandlesColor;
            
            if (Handles.Button(node.Position, Quaternion.identity, size, size / 2, Handles.SphereHandleCap))
            {
                selectedID = idx;
            }
            Handles.Label(node.Position, $"{idx}", selectedID == idx ? highlightedLabelStyle : labelStyle);

            Handles.color = defaultHandlesColor;
        }

        sceneView.Repaint();
    }

    public static bool DrawMoveHandle(ref Vector3 nodePosition)
    {
        EditorGUI.BeginChangeCheck();
        Vector3 newPosition = Handles.PositionHandle(nodePosition, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            nodePosition = newPosition;
            return true;
        }

        return false;
    }
}
#endif