using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DelaunayTriangulation;
using PlacementCriteria;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Must be added to the camera
/// Let's user place content using the mouse
/// </summary>
public class ContentPlacement : MonoBehaviour
{

    [SerializeField] private Slider scaleSlider;
    
    // Content
    private GameObject _contentToPlace;
    
    // Instantiated Content
    private GameObject _contentToModify;
    private Vector3 originalScale;

    // only RayCast for planes
    private const int PlaneLayerMask = 1 << 7;
    private const int MarkerLayerMask = 1 << 8;
    private Camera _camera; 

    private void Awake()
    {
        _camera = gameObject.GetComponent<Camera>();
    }

    public void PlaceGameObject(GameObject gameObjectToPlace)
    {
        _contentToPlace = gameObjectToPlace;
        _contentToModify = null;
        originalScale = gameObjectToPlace.transform.localScale;
        scaleSlider.value = 1f;
    }
    
    public void AutoPlace()
    {
        // TODO make work
        
        PlacementCriterion[] criteria = (_contentToModify != null ? _contentToModify : _contentToPlace).GetComponents<PlacementCriterion>();

        // Dic<planeID, List<Tuple<2dPositionOnPlane, marker> >
        var planeToPoints = new Dictionary<int, List<Tuple<Vector2, GameObject>>>();
        // List<planeID, planeHit>
        var hitsToEvaluate = new List<Tuple<int, RaycastHit>>();

        // find visible planes
        int width = Screen.width;
        int height = Screen.height;
        int stepSize = Math.Max(1, width / 50);
        for (var x = 0; x < width; x += stepSize)
        {
            for (var y = 0; y < height; y += stepSize)
            {
                RaycastHit hit;
                RaycastHit hitPlane;
                Ray ray = _camera.ScreenPointToRay(new Vector3(x, y, 0));

                bool hitMarkerSuccess = Physics.Raycast(ray, out hit, Mathf.Infinity, MarkerLayerMask);
                if (Physics.Raycast(ray, out hitPlane, Mathf.Infinity, PlaneLayerMask))
                {
                    PlaneInfo planeInfo = hitPlane.transform.GetComponent<PlaneInfo>();
                    Plane plane = planeInfo.Plane;
                    


                    
                    int planeId = hitPlane.transform.GetInstanceID();
                    hitsToEvaluate.Add(Tuple.Create(planeId, hitPlane));
                    if (!planeToPoints.ContainsKey(planeId))
                    {
                        planeToPoints[planeId] = new List<Tuple<Vector2, GameObject>>();
                    }
                    
                    if (!hitMarkerSuccess)
                    {
                        goto Skip;
                    }
                    // here until SKIP only for markers
                    
                    
                    // skip markers too far from the plane
                    if (plane.GetDistanceToPoint(hit.transform.position) > 2f)
                    {
                        continue;
                    }
                    
                    //skip center points of windows
                    var marker = hit.transform.GetComponent<Marker>();
                    if (marker !=null && marker.number == 4)
                    {
                        continue;
                    }
                    
                    // project point to plane
                    Vector2 vec2d = planeInfo.Conv3dTo2d(hit.transform.position);
                    
                    


                    foreach (var t in planeToPoints[planeId])
                    {
                        if (t.Item1.Equals(vec2d))
                        {
                            goto Skip;
                        }
                    }
                    
                    
                    planeToPoints[planeId].Add(Tuple.Create<Vector2, GameObject>(vec2d, hit.transform.gameObject));

                    Skip:
                    Debug.DrawLine(_camera.transform.position, hitPlane.point, Color.cyan, 20f);

                }
            }
        }
        
        // calculate triangulation foreach plane
        // Dic<planeId, triangulation>
        var triangulations = new Dictionary<int, MyTriangulation>();
        foreach (var item in planeToPoints)
        {
            int planeId = item.Key;
            var twoDAndMarker = item.Value;
            
            List<DelaunayTriangulation.Vertex> triangulationData = new List<DelaunayTriangulation.Vertex>();

            int idx = 0;
            foreach (var tuple in twoDAndMarker)
            {
                Vector2 vec2 = tuple.Item1;
                triangulationData.Add(new DelaunayTriangulation.Vertex(new Vector2(vec2.x, vec2.y), idx));
                idx++;
            }

            DelaunayTriangulation.Triangulation triangulation = new DelaunayTriangulation.Triangulation(triangulationData);

            List<Marker> myMarkers = new List<Marker>();
            foreach (var t in twoDAndMarker)
            {
                myMarkers.Add(t.Item2.GetComponent<Marker>());
            }
            triangulations[planeId] = new MyTriangulation(triangulation,myMarkers);
            
            foreach (DelaunayTriangulation.Triangle triangle in triangulation.triangles)
            {
                Vector3 pos0 = twoDAndMarker[triangle.vertex0.index].Item2.transform.position;
                Vector3 pos1 = twoDAndMarker[triangle.vertex1.index].Item2.transform.position;
                Vector3 pos2 = twoDAndMarker[triangle.vertex2.index].Item2.transform.position;
                
                Debug.DrawLine(pos0, pos1, Color.magenta, 30f);
                Debug.DrawLine(pos1, pos2, Color.magenta, 30f);
                Debug.DrawLine(pos2, pos0, Color.magenta, 30f);
            }
        }
        
        //calculate scores
        var scoresAndHits = new List<Tuple<float, RaycastHit>>();
        foreach (var item in hitsToEvaluate)
        {
            RaycastHit hitPlane = item.Item2;
            
            var score = 1f;
            foreach (PlacementCriterion c in criteria)
            {
                score *= c.Score(hitPlane, triangulations[hitPlane.transform.GetInstanceID()]);
            }
            scoresAndHits.Add(Tuple.Create(score, hitPlane));
        }

        if (scoresAndHits.Count <= 0)
        {
            //TODO REMOVE DEBUG
            Debug.LogError("NO SCORES");
            return;
        }
            
        
        //find location with highest score
        Tuple<float, RaycastHit> best = scoresAndHits[0];
        foreach (Tuple<float, RaycastHit> t in scoresAndHits)
        {
            if (t.Item1 > best.Item1)
            {
                best = t;
            }
        }
        
        PlaceOrMove(best.Item2);
        Debug.Log($"AUTOPLACE: score={best.Item1}");
    }
    
    private void Update()
    {

        // skip if there is no content
        if (_contentToPlace == null && _contentToModify == null)
        {
            scaleSlider.gameObject.SetActive(false);
            return;
        }
        scaleSlider.gameObject.SetActive(true);
            
        
        // check if mouse is on a plane
        RaycastHit hit;
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        if (!EventSystem.current.IsPointerOverGameObject (-1) && Input.GetMouseButton(0) && Physics.Raycast(ray,out hit, Mathf.Infinity, PlaneLayerMask))
        {
            PlaceOrMove(hit);
        }

        if (_contentToModify != null)
            _contentToModify.transform.localScale = originalScale * scaleSlider.value;

    }

    private void PlaceOrMove(RaycastHit hit)
    {
        // hit.transform.GetComponent<Renderer>().material.color = Color.green;
        if (_contentToModify == null)
        {
            _contentToModify = Instantiate(_contentToPlace);
            _contentToModify.SetActive(true);
        }

        _contentToModify.transform.position = hit.point;
        _contentToModify.transform.rotation = hit.transform.rotation;
        _contentToModify.transform.Rotate(new Vector3(0f, 0f, -_contentToModify.transform.eulerAngles.z));
        
        _contentToModify.transform.localScale = originalScale * scaleSlider.value;
    }
}

public class MyTriangulation
{
    public DelaunayTriangulation.Triangulation Triangulation;
    public List<Marker> Markers;

    public MyTriangulation(DelaunayTriangulation.Triangulation tri, List<Marker> markers)
    {
        Triangulation = tri;
        Markers = markers;
    }
}
