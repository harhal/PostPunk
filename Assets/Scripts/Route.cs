using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class Route : MonoBehaviour
{
    [field: SerializeField]
    private List<int> nodes;

    [field: SerializeField]
    private Vector3 playerPosition;

    [SerializeField]
    private SplineContainer spline;


    [SerializeField]
    private City city;

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
        spline.Spline.SetKnot(0, new BezierKnot(spline.transform.worldToLocalMatrix.MultiplyPoint(playerPosition)));
    }

    // Update is called once per frame
    void RemoveNode(int idx)
    {
        nodes.RemoveAt(idx);
        spline.Spline.RemoveAt(idx + 1);
    }

    public void RegenerateMesh()
    {
        spline.Spline.Clear();

        spline.Spline.Add(spline.transform.worldToLocalMatrix.MultiplyPoint(playerPosition), TangentMode.Linear);

        var nodes = city.GetMap().Nodes;
        foreach (var nodeId in this.nodes)
        {
            spline.Spline.Add(spline.transform.worldToLocalMatrix.MultiplyPoint(nodes[nodeId].Position), TangentMode.Linear);
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
