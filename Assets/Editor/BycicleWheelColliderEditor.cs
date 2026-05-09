using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BycicleWheelCollider))]
public class BycicleWheelColliderEditor : Editor
{    
    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUIUpdated;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUIUpdated;
    }

    public override void OnInspectorGUI()
    {
        BycicleWheelCollider colliderTarget = target as BycicleWheelCollider;
        if (colliderTarget == null)
        {
            return;
        }

        DrawDefaultInspector();

        if (GUILayout.Button("Regenerate colliders"))
        {
            while (colliderTarget.transform.childCount > 0)
            {
                DestroyImmediate(colliderTarget.transform.GetChild(0).gameObject);
            }
            colliderTarget.CreateColliders();
        }
    }

    void OnSceneGUIUpdated(SceneView view)
    {
        BycicleWheelCollider colliderTarget = target as BycicleWheelCollider;
        if (colliderTarget == null)
        {
            return;
        }
    }
}
