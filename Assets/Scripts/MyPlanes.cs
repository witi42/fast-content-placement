using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.SpatialAwareness.Processing;
using UnityEngine;
using UnityEngine.UI;

public class MyPlanes : MonoBehaviour
{
    [Tooltip("decrease this value to find larger/more planes")]
    public float planeCalcScaleFactor = 0.1f;

    public Material PlaneMaterial;

    public Toggle planesToggle;

    private GameObject scaleParent;
    private List<GameObject> planesGameObjects;

    void Start()
    {
        planesToggle.onValueChanged.AddListener(ShowPlanes);
        
        scaleParent = new GameObject("ScaleParent");

        List<PlaneFinding.MeshData> meshData = new List<PlaneFinding.MeshData>();

        List<MeshFilter> filters = new List<MeshFilter>();
        GameObject[] mapGameObjects = GameObject.FindGameObjectsWithTag("Map");

        foreach (var o in mapGameObjects)
        {
            o.transform.parent = scaleParent.transform;
        }

        scaleParent.transform.localScale *= planeCalcScaleFactor;

        foreach (var o in mapGameObjects)
        {
            filters.Add(o.GetComponent<MeshFilter>());
        }

        foreach (var filter in filters)
        {
            if (filter != null && filter.sharedMesh != null)
            {
                // fix surface mesh normals so we can get correct plane orientation.
                filter.mesh.RecalculateNormals();
                meshData.Add(new PlaneFinding.MeshData(filter));
            }

            BoundedPlane[] planes = PlaneFinding.FindPlanes(meshData, 0f, 0f);
            planesGameObjects = new List<GameObject>();
            foreach (BoundedPlane plane in planes)
            {
                GameObject planeObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                // planeObj.GetComponent<Renderer>().sharedMaterial.color = Color.magenta;
                planeObj.GetComponent<Renderer>().material = PlaneMaterial;
                planeObj.transform.position = plane.Bounds.Center;
                planeObj.transform.rotation = plane.Bounds.Rotation;
                Vector3 extents = plane.Bounds.Extents * 2;
                planeObj.transform.localScale = new Vector3(extents.x, extents.y, 0.1f);
                planeObj.transform.parent = scaleParent.transform;

                // set to "Planes layer" (7) for RayCasts
                planeObj.layer = 7;
                
                // add plane (equation) to GameObject
                var planeInfo = planeObj.AddComponent<PlaneInfo>();
                planeInfo.Plane = plane.Plane;
                planeInfo.PlaneGameObject = planeObj;
            }
        }

        // scale back to original scale
        scaleParent.transform.localScale = Vector3.one;
        foreach (GameObject planeG in planesGameObjects)
        {
            Plane plane = planeG.GetComponent<PlaneInfo>().Plane;
            plane.SetNormalAndPosition(plane.normal, planeG.transform.position);
            planeG.GetComponent<PlaneInfo>().Plane = plane;
        }

        // foreach (GameObject plane in planesGameObjects)
        // {
        //     CalculateGrid(plane.transform);
        // }

        
    }

    private void CalculateGrid(Transform plane)
    {
        for (var i = 0; i < 50; i++)
        {
            for (var j = 0; j < 50; j++)
            {
                Vector3 curPos = plane.position;
                curPos += plane.up * (plane.lossyScale.y * (-0.5f + (float) j / (50 - 1)));
                curPos += plane.right * (plane.lossyScale.x * (-0.5f + (float) i / (50 - 1)));

                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                cube.transform.position = curPos;
                // cube.transform.localScale *= 0.1f;

                
                Color color = Color.black;
                RaycastHit hit;
                //Ray ray = new Ray(curPos, plane.forward);
                //Debug.DrawRay(curPos, plane.forward, Color.magenta, 30f);
                if (Physics.Raycast(curPos, plane.forward, out hit ,10f, 1) || Physics.Raycast(curPos, -plane.forward, out hit ,10f, 1))
                {
                    Renderer rend = hit.transform.GetComponent<Renderer>();
                    MeshCollider meshCollider = hit.collider as MeshCollider;

                    if (rend == null || rend.sharedMaterial == null || rend.sharedMaterial.mainTexture == null || meshCollider == null)
                        continue;

                    Texture2D tex = rend.material.mainTexture as Texture2D;
                    Vector2 pixelUV = hit.textureCoord;
                    pixelUV.x *= tex.width;
                    pixelUV.y *= tex.height;

                    color = tex.GetPixel((int) pixelUV.x, (int) pixelUV.y);
                    Debug.Log("hit");
                }

                cube.GetComponent<Renderer>().material.color = color;

            }
        }
    }
    
    

    private void ShowPlanes(bool active)
    {
        //scaleParent.SetActive(active);
        foreach (GameObject g in planesGameObjects)
        {
            g.GetComponent<Renderer>().enabled = active;
        }
    }
}