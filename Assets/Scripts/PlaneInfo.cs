using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlaneInfo : MonoBehaviour
{
    public Plane Plane;
    public GameObject PlaneGameObject;
    public DelaunayTriangulation.Triangulation CurrentTriangulation;

    public Vector2 Conv3dTo2d(Vector3 pos)
    {
        //TODO make efficient using Plane instead of GameObject for calculation
        GameObject g = new GameObject();
        g.transform.position = Plane.ClosestPointOnPlane(pos);
        
        var go = new GameObject();
        Transform tr = go.transform;
        tr.rotation = PlaneGameObject.transform.rotation;
        tr.position = PlaneGameObject.transform.position;
        g.transform.parent = tr;
        tr.rotation = Quaternion.identity;

        g.transform.parent = null;
        Destroy(go);
                    
        Vector2 vec2d = new Vector2(g.transform.position.x, g.transform.position.y);
        Destroy(g);
        return vec2d;
    }
}
