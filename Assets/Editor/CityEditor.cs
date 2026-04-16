#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(City))]
public class CityEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        City cityTarget = target as City;
        if (cityTarget != null 
        && cityTarget.GetMap() != null
        && cityTarget.GetPalette() != null)
        {
            bool bLoadCityPreview = cityTarget.transform.childCount > 0;
            if (bLoadCityPreview && !cityTarget.bHasValidCachedData)
            {
                if (GUILayout.Button("Rebulid city"))
                {
                    while (cityTarget.transform.childCount > 0)
                    {
                        DestroyImmediate(cityTarget.transform.GetChild(0).gameObject);
                    }
                    cityTarget.LoadCity();
                }
            }
            else 
            {
                
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("Load city preview");
                    if (GUILayout.Toggle(bLoadCityPreview, "") != bLoadCityPreview)
                    {
                        if (!bLoadCityPreview)
                        {
                            cityTarget.LoadCity();
                        }
                        else while (cityTarget.transform.childCount > 0)
                        {
                            DestroyImmediate(cityTarget.transform.GetChild(0).gameObject);
                        }
                    }
                }
            }
            
            if (cityTarget.transform.childCount > 0)
            {
                if (GUILayout.Button("Nodes editor"))
                {
                    CityNodesEditorWindow.Open(cityTarget);
                }
            }
        }
    }
}
#endif
