using System;
using System.IO;
using UniGLTF;
using UnityEditor;
using UnityEngine;

public static class UniGltfCustomImporter
{
    [MenuItem(UniGLTFVersion.UNIGLTF_VERSION + "/CustomImport", priority = 1)]
    public static void ImportMenu()
    {
        var path = "C:/Users/cribin/Documents/MTC/Huawei Project/Immersal Maps/Test";
        var destinationDir =
            "C:/Users/cribin/Documents/MTC/UnityProjectNoSpaceTest/ContextAwareMRFinalVersion/UnityImmersal/Assets/3rd-Party/ImmersalSDK/Map Meshes/";
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        if (Application.isPlaying)
        {
            return;
        }
       
        //
        // save as asset
        //
        if (path.StartsWithUnityAssetPath())
        {
            Debug.LogWarningFormat("disallow import from folder under the Assets");
            return;
        }
        
        var info = new DirectoryInfo(path);
        var fileInfo = info.GetFiles();
        foreach (var file in fileInfo)
        {
           Import(file, destinationDir);
           
        }


    }

    public static void Import(FileInfo file, string destinationDir)
    {
        string srcPath = file.FullName;
        var map_id_and_name = file.Name.Split(new[] { "-tex" }, StringSplitOptions.None)[0];
        string destinationPath = destinationDir +  map_id_and_name + "/" + map_id_and_name + ".prefab";
        
        Debug.Log(srcPath);
        Debug.Log(destinationPath);
        Debug.Log(UnityPath.FromFullpath(destinationPath));
        // import as asset
         gltfAssetPostprocessor.ImportAsset(srcPath, Path.GetExtension(srcPath).ToLower(), UnityPath.FromFullpath(destinationPath));
    }
    
  
}
