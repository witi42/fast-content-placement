using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class UvTo3dWorldPositionMapper : MonoBehaviour
{
    [SerializeField] private Toggle markerToggle;

    private Transform _transform;
    private Mesh _mesh;

    private GameObject _markersParent;
    
    
    private void Awake()
    {
        _markersParent = new GameObject("MarkersParent");
        
        GameObject g = GameObject.FindGameObjectWithTag("Map");
        _transform = g.transform;
        _mesh = g.GetComponent<MeshFilter>().mesh;
        
        markerToggle.onValueChanged.AddListener(ShowMarkers);
    }

    private void Start()
    {
        MapToWorld();
    }

    private void ShowMarkers(bool active)
    {
        _markersParent.SetActive(active);
    }

    private void MapToWorld()
    {
        // for (var i = 0; i < _mesh.subMeshCount; i++)
        for (var i = 0; i < 10; i++)
        {
            SubMeshDescriptor subMeshDescriptor = _mesh.GetSubMesh(i);
            
            //Load json file file (Assets/Resources/UVs/[0..].json)
            var jsonTextFile = Resources.Load<TextAsset>($"UVs/{i}");
            var uvs = UVs.CreateFromJson(jsonTextFile.ToString());

            int id = 0;
            foreach (Window window in uvs.windows)
            {
                int number = 0;
                foreach(UVCoordinate uvCoordinate in window.corners)
                {
                    CreateMarker(uvCoordinate, subMeshDescriptor, Color.magenta, id, number++, "window");
                }
                CreateMarker(window.center, subMeshDescriptor, Color.blue, id, number, "window");
                id++;
            }
        }
    }

    private void CreateMarker(UVCoordinate uvCoordinate, SubMeshDescriptor subMeshDescriptor, Color color, int id, int number, string type)

    {
        Vector2 uv = new Vector2(uvCoordinate.u, uvCoordinate.v);
        Vector3 worldPos = UvTo3D(uv, subMeshDescriptor.indexStart, subMeshDescriptor.indexCount);

        if (worldPos.Equals(Vector3.zero))
        {
            Debug.Log("UV mapping failed");
            return;
        }
        
        var g = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        g.GetComponent<Renderer>().material.color = color;
        g.transform.position = worldPos;
        g.transform.parent = _markersParent.transform;
        // set to "markers layer"
        g.layer = 8;
        
        Marker marker = g.AddComponent<Marker>();
        marker.id = id;
        marker.number = number;
        marker.type = type;
    }
    
    // code from aldonaletto on https://answers.unity.com/questions/372047/find-world-position-of-a-texture2d.html
    private Vector3 UvTo3D(Vector2 uv, int indexStart, int indexCount) {
        int [] tris = _mesh.triangles; //contain indices into the vertices array
        Vector2 [] uvs = _mesh.uv;
        Vector3 [] verts  = _mesh.vertices;
        int toIndex = indexStart + indexCount;
        for (int i = indexStart; i < toIndex; i += 3){
            Vector2 u1 = uvs[tris[i]]; // get the triangle UVs
            Vector2 u2 = uvs[tris[i+1]];
            Vector2 u3 = uvs[tris[i+2]];
            // Debug.Log($"UVs {u1} {u2} {u3}");
            // calculate triangle area - if zero, skip it
            float a = Area(u1, u2, u3); 
            if (a == 0) 
                continue;
            // calculate barycentric coordinates of u1, u2 and u3
            // if anyone is negative, point is outside the triangle: skip it
            float a1 = Area(u2, u3, uv)/a; if (a1 < 0) continue;
            float a2 = Area(u3, u1, uv)/a; if (a2 < 0) continue;
            float a3 = Area(u1, u2, uv)/a; if (a3 < 0) continue;
            // point inside the triangle - find mesh position by interpolation...
            Vector3 p3D = a1*verts[tris[i]]+a2*verts[tris[i+1]]+a3*verts[tris[i+2]];
            // Debug.Log($"LOCALPOS {p3D}");
            // and return it in world coordinates:
            return _transform.TransformPoint(p3D);
        }
        // point outside any uv triangle
        return Vector3.zero;
    }

    private Vector3 UvTo3D(Vector2 uv) {
        int [] tris = _mesh.triangles;
        Vector2 [] uvs = _mesh.uv;
        Vector3 [] verts  = _mesh.vertices;
        for (int i = 0; i < tris.Length; i += 3){
            Vector2 u1 = uvs[tris[i]]; // get the triangle UVs
            Vector2 u2 = uvs[tris[i+1]];
            Vector2 u3 = uvs[tris[i+2]];
            // calculate triangle area - if zero, skip it
            float a = Area(u1, u2, u3); 
            if (a == 0) 
                continue;
            // calculate barycentric coordinates of u1, u2 and u3
            // if anyone is negative, point is outside the triangle: skip it
            float a1 = Area(u2, u3, uv)/a; if (a1 < 0) continue;
            float a2 = Area(u3, u1, uv)/a; if (a2 < 0) continue;
            float a3 = Area(u1, u2, uv)/a; if (a3 < 0) continue;
            // point inside the triangle - find mesh position by interpolation...
            Vector3 p3D = a1*verts[tris[i]]+a2*verts[tris[i+1]]+a3*verts[tris[i+2]];
            // and return it in world coordinates:
            return _transform.TransformPoint(p3D);
        }
        // point outside any uv triangle: return Vector3.zero
        return Vector3.zero;
    }

    // calculate signed triangle area using a kind of "2D cross product":
    private static float Area(Vector2 p1, Vector2 p2, Vector2 p3) {
        Vector2 v1 = p1 - p3;
        Vector2 v2 = p2 - p3;
        return (v1.x * v2.y - v1.y * v2.x)/2;
    }
}


[System.Serializable]
public class UVs
{
    public List<Window> windows;
    public static UVs CreateFromJson(string jsonString)
    {
        return JsonUtility.FromJson<UVs>(jsonString);
    }
}

[System.Serializable]
public class Window
{
    public List<UVCoordinate> corners;
    public UVCoordinate center;
}

[System.Serializable]
public class UVCoordinate
{
    public float u;
    public float v;
}
