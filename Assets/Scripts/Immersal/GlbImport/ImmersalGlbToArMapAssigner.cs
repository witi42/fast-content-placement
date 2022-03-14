using System;
using System.Collections.Generic;
using System.IO;
using Immersal.AR;
using UniGLTF;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class ImmersalGlbToArMapAssigner : EditorWindow
{
    private Transform _arSpacesMainParent;
    private string _prefabRootPath = "Assets/CustomAssets/3dModels/ImmersalGlbMeshes/NiederdorfPilotStudyMaps";
    private Dictionary<string, string> _mapNameToPrefabMap;
    private const string GlbPrefabSuffix = ".prefab";

    // private void OnEnable()
    // {
    //     SetupMapNameToPrefabMap();
    //     AssignPrefabToMap();
    // }
    
    [MenuItem("Window/ImmersalSetup/ImmersalGlbToArMapAssigner")]
    public static void ShowWindow()
    {
        GetWindow<ImmersalGlbToArMapAssigner>("ImmersalGlbToArMapAssigner");
    }
    
    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        _arSpacesMainParent = (Transform) EditorGUILayout.ObjectField(_arSpacesMainParent, typeof(Transform), true);
        EditorGUILayout.EndHorizontal();
        
        if (GUILayout.Button("Assign Glb to ARMap"))
        {
            SetupMapNameToPrefabMap();
            AssignPrefabToMap();
        }
    }

    private void AssignPrefabToMap()
    {
        var allARMaps = _arSpacesMainParent.GetComponentsInChildren<ARMap>(true);
        foreach (var arMap in allARMaps)
        {
            var mapName = arMap.name.Split(new[] {"AR Map "}, StringSplitOptions.None)[1].Trim();
            if (!_mapNameToPrefabMap.TryGetValue(mapName, out string mapPrefabRoot))
                continue;

            var mapPrefabPath = mapPrefabRoot + "/" + mapName + GlbPrefabSuffix;
            var unityPrefabPath = UnityPath.FromFullpath(mapPrefabPath).Value;
            var prefabObject = (GameObject)AssetDatabase.LoadAssetAtPath(unityPrefabPath, typeof(GameObject));
            if (prefabObject == null)
            {
                Debug.Log("Glb Assignment Unsuccessful. Prefab at path: " + unityPrefabPath + " is null");
                return;
            }

            var test = Instantiate(prefabObject, arMap.transform);
			test.tag = "EditorOnly";
            test.transform.localRotation = Quaternion.Euler(0, 180, 0);
            Debug.Log("Glb Assignment successful.");
        }
    }

    private void SetupMapNameToPrefabMap()
    {
        _mapNameToPrefabMap = new Dictionary<string, string>();
        var info = new DirectoryInfo(_prefabRootPath);
        var directoryInfos = info.GetDirectories();
        foreach (var directoryInfo in directoryInfos)
        {
            _mapNameToPrefabMap.Add(directoryInfo.Name, directoryInfo.FullName);
        }
    }
}

#endif