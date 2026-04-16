using UnityEngine;
using UnityEngine.Splines;

public class Street : MonoBehaviour
{
    [field: SerializeField]
    public StreetData data{get; private set;}

    [SerializeField]
    private SplineContainer spline;

    public void InitSpline()
    {
        if (spline == null)
        {
            spline = GetComponentInChildren<SplineContainer>();
        }
    }

    void Awake()
    {
        InitSpline();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void SetData(StreetData data)
    {
        this.data = data;
    }

    public void RegenerateMesh()
    {
        spline.Spline.Clear();
        var nodes = GetCity().GetMap().Nodes;
        foreach (var nodeId in data.Nodes)
        {
            spline.Spline.Add(spline.transform.worldToLocalMatrix.MultiplyPoint(nodes[nodeId].Position), TangentMode.Linear);
        }
    }

    public virtual City GetCity()
    {
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

    public void SetHighlitedState(bool bHighlitedState)
    {
        /*if (bHighlitedState)
        {
            spline.GetComponent<MeshRenderer>().material.EnableKeyword("_HIGHLIGHT");
        }
        else
        {
            spline.GetComponent<MeshRenderer>().material.DisableKeyword("_HIGHLIGHT");
        }*/
    }
}
