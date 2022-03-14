using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using DelaunayTriangulation;
using PlacementCriteria;
using UnityEngine;

public class WindowsCriterion : PlacementCriterion
{
    [SerializeField] private WindowMode windowMode = WindowMode.Avoid;
    [SerializeField] private ContentPointSampleMode contentPointSampleMode = ContentPointSampleMode.SinglePosition;
    
    private enum WindowMode
    {
        None,
        Avoid,
        Inside
    }
    private enum ContentPointSampleMode
    {
        SinglePosition,
        Bounds
    }
    public override float Score(RaycastHit hit, MyTriangulation myTriangulation)
    {
        if (windowMode == WindowMode.None)
            return 1f;
        
        // collect points
        var points = new List<Vector3>();

        switch (contentPointSampleMode)
        {
            case ContentPointSampleMode.SinglePosition:
                points.Add(hit.point);
                break;
            case ContentPointSampleMode.Bounds:
                throw new NotImplementedException();
                break;
        }


        // calc score based on points
        var ids = new HashSet<int>();
        PlaneInfo planeInfo = hit.transform.GetComponent<PlaneInfo>();
        foreach (Vector3 p in points)
        {
            foreach (Triangle t in myTriangulation.Triangulation.triangles)
            {
                if (t.PointInTriangle(planeInfo.Conv3dTo2d(p)))
                {
                    Debug.Log("WINDOW INSIDE TRIANGLE");
                    ids.Add(myTriangulation.Markers[t.vertex0.index].id);
                    ids.Add(myTriangulation.Markers[t.vertex1.index].id);
                    ids.Add(myTriangulation.Markers[t.vertex2.index].id);
                }
            }
        }

        if (ids.Count > 0)
        {
            Debug.Log($"WINDOW size{ids.Count} ");
            foreach (int id in ids)
            {
                Debug.Log($"{id} ");
            }
        }

        switch (windowMode)
        {
            case WindowMode.Inside:
                if (ids.Count == 1)
                    return 1f;
                break;
            case WindowMode.Avoid:
                if (ids.Count != 1)
                    return 1f;
                break;
        }


        return 0f;
    }

}
