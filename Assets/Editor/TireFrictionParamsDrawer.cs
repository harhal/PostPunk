#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TireFrictionParams))]
public class TireFrictionParamsDrawer : PropertyDrawer
{
    private const int FieldCount = 5;
    private const int GraphHeight = 60;
    private const float Padding = 2f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded)
            return EditorGUIUtility.singleLineHeight;

        return EditorGUIUtility.singleLineHeight          // foldout
             + (EditorGUIUtility.singleLineHeight + Padding) * FieldCount
             + GraphHeight + Padding * 2;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            float lineH = EditorGUIUtility.singleLineHeight + Padding;
            float y     = position.y + EditorGUIUtility.singleLineHeight + Padding;

            DrawField(position, ref y, property, "extremumSlip",   "Extremum Slip");
            DrawField(position, ref y, property, "extremumValue",  "Extremum Value");
            DrawField(position, ref y, property, "asymptoteSlip",  "Asymptote Slip");
            DrawField(position, ref y, property, "asymptoteValue", "Asymptote Value");
            DrawField(position, ref y, property, "stiffness",      "Stiffness");

            Rect graphRect = new Rect(position.x + 15f, y + Padding, position.width - 15f, GraphHeight);
            DrawCurvePreview(graphRect, property);

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    private void DrawField(Rect position, ref float y, SerializedProperty property, string name, string label)
    {
        Rect rect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(rect, property.FindPropertyRelative(name), new GUIContent(label));
        y += EditorGUIUtility.singleLineHeight + Padding;
    }

    private void DrawCurvePreview(Rect rect, SerializedProperty property)
    {
        float extremumSlip   = property.FindPropertyRelative("extremumSlip").floatValue;
        float extremumValue  = property.FindPropertyRelative("extremumValue").floatValue;
        float asymptoteSlip  = property.FindPropertyRelative("asymptoteSlip").floatValue;
        float asymptoteValue = property.FindPropertyRelative("asymptoteValue").floatValue;
        float stiffness      = property.FindPropertyRelative("stiffness").floatValue;

        EditorGUI.DrawRect(rect, new Color(0.15f, 0.15f, 0.15f));

        DrawGridLine(rect, 0.5f, new Color(0.3f, 0.3f, 0.3f));

        float maxValue = Mathf.Max(extremumValue * stiffness, 0.01f);
        float maxSlip  = Mathf.Max(asymptoteSlip * 1.1f, 0.01f);

        Vector3[] points = new Vector3[64];
        for (int i = 0; i < points.Length; i++)
        {
            float t    = i / (float)(points.Length - 1);
            float slip = t * maxSlip;
            float val  = EvaluateCurve(slip, extremumSlip, extremumValue,
                                               asymptoteSlip, asymptoteValue, stiffness);

            float px = rect.x + t * rect.width;
            float py = rect.yMax - (val / maxValue) * rect.height;
            points[i] = new Vector3(px, Mathf.Clamp(py, rect.y, rect.yMax));
        }

        Handles.color = new Color(0.3f, 0.8f, 0.4f);
        Handles.DrawPolyLine(points);

        GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = Color.gray } };
        EditorGUI.LabelField(new Rect(rect.x + 2,      rect.yMax - 14, 60, 14), "slip →",    labelStyle);
        EditorGUI.LabelField(new Rect(rect.xMax - 50,  rect.y,         50, 14), $"×{stiffness:F1}", labelStyle);
    }

    private void DrawGridLine(Rect rect, float t, Color color)
    {
        float y = rect.y + t * rect.height;
        Handles.color = color;
        Handles.DrawLine(new Vector3(rect.x, y), new Vector3(rect.xMax, y));
    }

    private float EvaluateCurve(float slip,
        float extremumSlip, float extremumValue,
        float asymptoteSlip, float asymptoteValue, float stiffness)
    {
        float mu;
        if (slip < extremumSlip)
            mu = (slip / Mathf.Max(extremumSlip, 0.001f)) * extremumValue;
        else if (slip < asymptoteSlip)
        {
            float t = (slip - extremumSlip) / Mathf.Max(asymptoteSlip - extremumSlip, 0.001f);
            mu = Mathf.Lerp(extremumValue, asymptoteValue, t);
        }
        else
            mu = asymptoteValue;

        return mu * stiffness;
    }
}
#endif