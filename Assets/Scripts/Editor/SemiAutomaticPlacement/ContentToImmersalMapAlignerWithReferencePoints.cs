using System.Collections.Generic;
using System.IO;
using UniGLTF;
using UnityEditor;
using UnityEngine;

public class ContentToImmersalMapAlignerWithReferencePoints : EditorWindow
{
    private Transform _targetTransform;
    private Vector3 _targetTransformInitialPosition = Vector3.negativeInfinity;
    private Quaternion _targetTransformInitialRotation;
    private Vector3 _targetTransformInitialScale;
    private string _immersalMapLayer = "ImmersalMap";
    private string _imageLayer = "AugmentationAssets";
    private int _activeLayer;
    private Transform _immersalPointsTransform;
    private Transform _imagePointsTransform;
    private Transform _currentPointsParentTransform;
    static bool active;
    private bool _mapPointSelectionMode;
    private bool _createNewPoint;
    private GameObject _currentPointObject;

    private List<Material> _pointColors = new List<Material>();

    private int _currentColorPosition;
    KabschSolver solver = new KabschSolver();

    // Open this from Window menu
    [MenuItem("Window/ContentToImmersalAlignment")]
    static void Init()
    {
        var window =
            (ContentToImmersalMapAlignerWithReferencePoints) GetWindow(
                typeof(ContentToImmersalMapAlignerWithReferencePoints));
        window.Show();
    }

    void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        _immersalPointsTransform = InitParentObject("ImmersalPointsParent");
        _imagePointsTransform = InitParentObject("ImagePointsParent");
        _mapPointSelectionMode = true;
        _createNewPoint = true;
        _currentColorPosition = 0;
        _activeLayer = LayerMask.NameToLayer(_immersalMapLayer);
        _currentPointsParentTransform = _immersalPointsTransform;
        LoadMaterials();
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        DestroyPointsParentTransform(_immersalPointsTransform);
        DestroyPointsParentTransform(_imagePointsTransform);
        active = false;
    }

    // Receives scene events
    // Use event mouse click for raycasting
    void OnSceneGUI(SceneView view)
    {
        if (!active)
        {
            return;
        }

        var currentEventType = Event.current.type;
        if (currentEventType == EventType.MouseDown)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hit;

            // Spawn cube on hit location
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.gameObject.layer != _activeLayer)
                    return;
                // Debug.Log("Hit: " + hit.collider.gameObject.name);

                if (_createNewPoint)
                {
                    _currentPointObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    var currentPointNum = _currentColorPosition / 2;
                    _currentPointObject.name = "Point" + currentPointNum;
                    var renderer = _currentPointObject.GetComponent<Renderer>();
                    renderer.material = _pointColors[currentPointNum];
                    _createNewPoint = false;
                }

                _currentPointObject.transform.position = hit.point;
                _currentPointObject.transform.parent = _currentPointsParentTransform;
            }
        }
        else if (currentEventType == EventType.KeyDown)
        {
            if (Event.current.keyCode == KeyCode.Y)
                ConfirmPointSelection();
        }

        Event.current.Use();
    }

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        _targetTransform = (Transform) EditorGUILayout.ObjectField(_targetTransform, typeof(Transform), true);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Enable Raycasting"))
        {
            active = !active;
        }

        if (GUILayout.Button("Confirm Point Selection(Press Y)"))
        {
            ConfirmPointSelection();
        }

        if (GUILayout.Button("Align Content to Map"))
        {
            AlignContentToMap();
        }
        
        if (GUILayout.Button("Reset Transform"))
        {
           ResetTransform();
        }

        GUILayout.Label("Active:" + active);
    }

    private void ClearPoints()
    {
        DestroyPointsParentTransform(_immersalPointsTransform);
        DestroyPointsParentTransform(_imagePointsTransform);
    }

    private void DestroyPointsParentTransform(Transform parentTransform)
    {
        if (!parentTransform)
            return;
        DestroyImmediate(parentTransform.gameObject);
        /* foreach (Transform childPoint in parentTransform)
         {
             Destroy(childPoint.gameObject);
         }*/
    }

    private Transform InitParentObject(string parentName)
    {
        var parenPoints = new GameObject(parentName);
        return parenPoints.transform;
    }

    private void ConfirmPointSelection()
    {
        _currentColorPosition += 1;
        if (_currentColorPosition > _pointColors.Count * 2 - 1)
            _currentColorPosition = 0;
        _createNewPoint = true;
        _mapPointSelectionMode = !_mapPointSelectionMode;
        if (_mapPointSelectionMode)
        {
            _activeLayer = LayerMask.NameToLayer(_immersalMapLayer);
            _currentPointsParentTransform = _immersalPointsTransform;
        }
        else
        {
            _activeLayer = LayerMask.NameToLayer(_imageLayer);
            _currentPointsParentTransform = _imagePointsTransform;
        }
    }

    private void LoadMaterials()
    {
        var materials = LoadAllMaterialsAtPath("Assets/Materials/SingleColorMaterials");
        _pointColors = materials;
    }

    List<Material> LoadAllMaterialsAtPath(string path)
    {
        var materials = new List<Material>();
        if (!Directory.Exists(path))
            return null;

        var assets = Directory.GetFiles(path);
        foreach (string assetPath in assets)
        {
            if (assetPath.Contains(".mat") && !assetPath.Contains(".meta"))
            {
                materials.Add(AssetDatabase.LoadAssetAtPath<Material>(assetPath));
                //Debug.Log("Loaded " + assetPath);
            }
        }

        return materials;
    }

    private void AlignContentToMap()
    {
        if (_targetTransform != null && _targetTransformInitialPosition.Equals(Vector3.negativeInfinity))
        {
            _targetTransformInitialPosition = _targetTransform.position;
            _targetTransformInitialRotation = _targetTransform.rotation;
            _targetTransformInitialScale = _targetTransform.localScale;
            Debug.Log("initial transform set");
        }
        
        var numOfPoints = _imagePointsTransform.childCount;
        if (numOfPoints != _immersalPointsTransform.childCount)
        {
            Debug.Log("Image and map points do no match. Aborting alignment");
            return;
        }

        if (numOfPoints < 3)
        {
            Debug.Log("Select at least 3 pairs of points for alignment");
            return;
        }

        if (_targetTransform == null)
        {
            Debug.Log("Set target transform to align with the map!");
            return;
        }

        var imagePoints = new Vector3[numOfPoints];
        var mapPoints = new Vector4[numOfPoints];
        for (int i = 0; i < numOfPoints; i++)
        {
            var imagePoint = _imagePointsTransform.GetChild(i);
            var mapPoint = _immersalPointsTransform.GetChild(i);
            imagePoints[i] = imagePoint.position;
            var mapPointPosition = mapPoint.position;
            mapPoints[i] = new Vector4(mapPointPosition.x, mapPointPosition.y, mapPointPosition.z,
                mapPoint.localScale.x);
        }

        Matrix4x4 kabschTransform = solver.SolveKabsch(imagePoints, mapPoints, solveScale: true);
        _targetTransform.position = kabschTransform.MultiplyPoint3x4(_targetTransform.position);
        _targetTransform.Rotate(kabschTransform.ExtractRotation().eulerAngles);
        _targetTransform.localScale = Vector3.Scale(_targetTransform.localScale, kabschTransform.ExtractScale());
    }

    private void ResetTransform()
    {
        if (_targetTransformInitialPosition.Equals(Vector3.negativeInfinity) || _targetTransform == null)
            return;
        _targetTransform.position = _targetTransformInitialPosition;
        _targetTransform.rotation = _targetTransformInitialRotation;
        _targetTransform.localScale = _targetTransformInitialScale;
    }
}