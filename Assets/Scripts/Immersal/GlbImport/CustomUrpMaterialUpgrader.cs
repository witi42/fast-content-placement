using System.IO;
using UniGLTF;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class CustomUrpMaterialUpgrader : MonoBehaviour
{
    private static readonly int Smoothness = Shader.PropertyToID("_Smoothness");
    private static readonly int Glossiness = Shader.PropertyToID("_Glossiness");
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");

    [SerializeField] string _materialsRootDir =
        "Assets/3rd-Party/ImmersalSDK/Map Meshes/NiederdorfPilotStudyMaps/";

    // Start is called before the first frame update
    void Start()
    {
        var info = new DirectoryInfo(_materialsRootDir);
        var directoryInfos = info.GetDirectories();
        foreach (var directoryInfo in directoryInfos)
        {
            var subDirectoryInfos = directoryInfo.GetDirectories();
            foreach (var subDirectoryInfo in subDirectoryInfos)
            {
                if (subDirectoryInfo.Extension == ".Materials")
                    print(subDirectoryInfo.FullName);

                var materials = subDirectoryInfo.GetFiles();
                foreach (var materialInfo in materials)
                {
                    var materialPath = UnityPath.FromFullpath(materialInfo.FullName).Value;
                    var material = (Material) AssetDatabase.LoadAssetAtPath(materialPath, typeof(Material));
                    if (material != null)
                    {
                        ConvertStandardShaderToURPShader(material);
                    }
                }
            }
        }
    }

    void ConvertStandardShaderToURPShader(Material materialToConvert)
    {
        if (materialToConvert.shader != Shader.Find("Standard"))
        {
            print("Material already converted. Skipping conversion");
            return;
        }

        Texture textureData = materialToConvert.GetTexture(MainTex);
        float smoothness = materialToConvert.GetFloat(Glossiness);


        materialToConvert.shader = Shader.Find("Universal Render Pipeline/Lit");
        materialToConvert.SetFloat(Smoothness, smoothness);
        materialToConvert.mainTexture = textureData;

        print("Converted material to URP : " + materialToConvert.name);
    }
}

#endif