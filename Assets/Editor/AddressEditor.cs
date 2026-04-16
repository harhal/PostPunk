#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Address))]
public class AddressEditor : Editor
{
    bool _ShowSetStreet = false;

    private void OnEnable()
    {
        Address addressTarget = target as Address;
        if (addressTarget == null)
        {
            return;
        }

        City city = addressTarget.GetCity();     
        if (city == null)
        {
            return;
        }

        Selection.selectionChanged += Repaint;
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= Repaint;
        EditorApplication.hierarchyChanged -= OnHierarchyChanged;
    }

    public override void OnInspectorGUI()
    {
        Address addressTarget = target as Address;
        if (addressTarget == null)
        {
            return;
        }

        City city = addressTarget.GetCity();     
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
            GUILayout.Label(addressTarget.data.SystemName, GUI.skin.textField);
            GUI.enabled = prevState;

            string newDisplayName = GUILayout.TextField(addressTarget.data.DisplayName);
            if (newDisplayName != addressTarget.data.DisplayName)
            {
                addressTarget.data.DisplayName = newDisplayName;
                EditorUtility.SetDirty(city.GetMap());
            }
        }
        
        GUILayout.Label("Street:");
        if (addressTarget.data.streetName != "")
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(addressTarget.data.streetName);
                if (GUILayout.Button("Select"))
                {
                    Selection.activeObject = city.GetStreetObject(addressTarget.data.streetName);
                }
            }
        }
        else
        {
            GUILayout.Label("None");
        }

        EditorGUILayout.Space();
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.Space();
            if (GUILayout.Button(_ShowSetStreet ? "Collapse streets list -" : "Expand streets list +"))
            {
                _ShowSetStreet = !_ShowSetStreet;
            }
            EditorGUILayout.Space();
        }

        EditorGUILayout.Space();
        if (_ShowSetStreet)
        {
            foreach (Street street in city.GetStreetsObjects())
            {
                if (GUILayout.Toggle(street.data.SystemName == addressTarget.data.streetName, $"{street.data.SystemName}"))
                {
                    addressTarget.data.streetName = street.data.SystemName;
                    addressTarget.transform.SetParent(street.transform);
                    addressTarget.SetStreet(street);
                    EditorUtility.SetDirty(city.GetMap());
                }
            }
        }
    }

    private void OnHierarchyChanged()
    {
        Address addressTarget = target as Address;
        if (addressTarget == null)
        {
            return;
        }

        City city = addressTarget.GetCity();     
        if (city == null)
        {
            return;
        }

        if (addressTarget.name != addressTarget.data.SystemName)
        {
            addressTarget.data.SystemName = addressTarget.name;
            EditorUtility.SetDirty(city.GetMap());
            Repaint();
        }

        if (addressTarget.transform.parent != addressTarget.street)
        {
            Street street = addressTarget.transform.parent.GetComponent<Street>();
            addressTarget.data.streetName = street != null ? street.data.SystemName : "";
            addressTarget.SetStreet(street);
            EditorUtility.SetDirty(city.GetMap());
            Repaint();
        }
    }
}
#endif
