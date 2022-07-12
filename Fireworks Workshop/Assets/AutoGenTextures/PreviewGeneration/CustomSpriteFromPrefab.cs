#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.Events;
using System;
using System.IO;
using UnityEditor.SceneManagement;

public class CustomSpriteFromPrefab : UnityEditor.Editor
{
    static int width = 1024;
    static int height = 1024;


    [MenuItem("GameObject/Prefab/PrefabToPNG Orthographic/Front")]
    private static void DoSomethingWithTexture2()
    {
        RuntimePreviewGenerator.BackgroundColor = Color.clear;
        RuntimePreviewGenerator.MarkTextureNonReadable = false;
        RuntimePreviewGenerator.PreviewDirection = new Vector3(-0.75f, -1, 1.5f);
        RuntimePreviewGenerator.RenderSupersampling = 2;
        RuntimePreviewGenerator.OrthographicMode = true;

        SetTex(RuntimePreviewGenerator.GenerateModelPreview(Selection.activeGameObject.transform, width, height, false, true), Selection.activeGameObject);
        //Debug.Log("Starting PNG Creation, This process may take around a minute.");
    }
    [MenuItem("GameObject/Prefab/PrefabToPNG Orthographic/Back")]
    private static void DoSomethingWithTexture()
    {

        RuntimePreviewGenerator.BackgroundColor = Color.clear;
        RuntimePreviewGenerator.MarkTextureNonReadable = false;
        RuntimePreviewGenerator.PreviewDirection = new Vector3(-0.75f, -1, -1.5f);
        RuntimePreviewGenerator.RenderSupersampling = 2;
        RuntimePreviewGenerator.OrthographicMode = true;

        SetTex(RuntimePreviewGenerator.GenerateModelPreview(Selection.activeGameObject.transform, width, height, false, true), Selection.activeGameObject);
        //Debug.Log("Starting PNG Creation, This process may take around a minute.");
    }
    [MenuItem("GameObject/Prefab/PrefabToPNG Orthographic/Current Veiw In Scene")]
    private static void DoSomethingWithTexture3()
    {
        Vector3 camerapos = SceneView.lastActiveSceneView.camera.transform.position;
        Vector3 relative = Selection.activeGameObject.transform.InverseTransformPoint(camerapos);

        RuntimePreviewGenerator.BackgroundColor = Color.clear;
        RuntimePreviewGenerator.MarkTextureNonReadable = false;
        RuntimePreviewGenerator.RenderSupersampling = 2;
        RuntimePreviewGenerator.OrthographicMode = true;

        SetTex(RuntimePreviewGenerator.GenerateModelPreview(Selection.activeGameObject.transform, width, height, false, true, true, relative), Selection.activeGameObject);
        //Debug.Log("Starting PNG Creation, This process may take around a minute.");

    }
    [MenuItem("GameObject/Prefab/PrefabToPNG/Front")]
    private static void DoSomethingWithTexture6()
    {
        RuntimePreviewGenerator.BackgroundColor = Color.clear;
        RuntimePreviewGenerator.MarkTextureNonReadable = false;
        RuntimePreviewGenerator.PreviewDirection = new Vector3(-0.75f, -1, 1.5f);
        RuntimePreviewGenerator.RenderSupersampling = 2;
        RuntimePreviewGenerator.OrthographicMode = false;

        SetTex(RuntimePreviewGenerator.GenerateModelPreview(Selection.activeGameObject.transform, width, height, false, true), Selection.activeGameObject);
        //Debug.Log("Starting PNG Creation, This process may take around a minute.");
    }
    [MenuItem("GameObject/Prefab/PrefabToPNG/Back")]
    private static void DoSomethingWithTexture5()
    {

        RuntimePreviewGenerator.BackgroundColor = Color.clear;
        RuntimePreviewGenerator.MarkTextureNonReadable = false;
        RuntimePreviewGenerator.PreviewDirection = new Vector3(-0.75f, -1, -1.5f);
        RuntimePreviewGenerator.RenderSupersampling = 2;
        RuntimePreviewGenerator.OrthographicMode = false;

        SetTex(RuntimePreviewGenerator.GenerateModelPreview(Selection.activeGameObject.transform, width, height, false, true), Selection.activeGameObject);
        //Debug.Log("Starting PNG Creation, This process may take around a minute.");
    }
    [MenuItem("GameObject/Prefab/PrefabToPNG/Current Veiw In Scene")]
    private static void DoSomethingWithTexture4()
    {
        Vector3 camerapos = SceneView.lastActiveSceneView.camera.transform.position;
        Vector3 relative = Selection.activeGameObject.transform.InverseTransformPoint(camerapos);

        RuntimePreviewGenerator.BackgroundColor = Color.clear;
        RuntimePreviewGenerator.MarkTextureNonReadable = false;
        RuntimePreviewGenerator.RenderSupersampling = 2;
        RuntimePreviewGenerator.OrthographicMode = false;

        SetTex(RuntimePreviewGenerator.GenerateModelPreview(Selection.activeGameObject.transform, width, height, false, true, true, relative), Selection.activeGameObject);
        //Debug.Log("Starting PNG Creation, This process may take around a minute.");

    }

    // Note that we pass the same path, and also pass "true" to the second argument.
    [MenuItem("GameObject/PrefabToTexture2D", true, 100)]
    private static bool NewMenuOptionValidation()
    {
        bool check = true;
        if (Selection.activeObject == null) return false;
        if (Selection.activeObject.GetType() == typeof(GameObject)) check = true; else return false;
        if (Selection.activeGameObject.activeInHierarchy) check = true; else return false;
        string sceneName = Selection.activeGameObject.scene.name;
        if (sceneName != null) check = true; else return false;


        // This returns true when the selected object is a Texture2D (the menu item will be disabled otherwise).
        return check;
    }

  

    public static void SetTex(Texture2D tex, GameObject prefObject)
    {
        if (tex == null)
        {
            Debug.LogWarning("Failed to Produce Texture");
            return;
        }
        if (!tex.isReadable)
        {
            Debug.Log("Texture Could not be Read");
            return;
        }

        Sprite Png = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100);
        Png.name = prefObject.name + " AutoGeneratedImage";
        //byte[] bytes = tex.EncodeToPNG();
        try
        {
            string prefabpath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(prefObject);
            //Debug.Log(prefabpath);
            string parentpath = Directory.GetParent(prefabpath).Name;
            //Debug.Log(parentpath);
            int end = prefabpath.LastIndexOf(parentpath) +1;
            int start = prefabpath.IndexOf('/') + 1;
            string path = prefabpath.Substring(start, end);
            //Debug.Log(path);

            string name = path + $"{Png.name}.png";
            SaveSpriteAsAsset(Png, name);
        }
        catch
        {
            int temp = UnityEngine.Random.Range(0, 1000000);
            string app = "AutoGenTextures/Generated Textures/" + $"{Png.name}.png";
            try
            {
                if (!AssetDatabase.IsValidFolder("Assets/AutoGenTextures"))
                {
                    AssetDatabase.CreateFolder("Assets", "AutoGenTextures");
                    Debug.Log("Created Folder AutoGenTextures");
                }
                if (!AssetDatabase.IsValidFolder("Assets/AutoGenTextures/Generated Textures"))
                {
                    AssetDatabase.CreateFolder("Assets/AutoGenTextures", "Generated Textures");
                    Debug.Log("Created Folder AutoGenTextures/Generated Textures");
                }
                SaveSpriteAsAsset(Png, app);
            }
            catch
            {
                if (!AssetDatabase.IsValidFolder("Assets/AutoGenTextures"))
                {
                    AssetDatabase.CreateFolder("Assets", "AutoGenTextures");
                    Debug.Log("Created Folder AutoGenTextures");
                }
                if (!AssetDatabase.IsValidFolder("Assets/AutoGenTextures/Generated Textures"))
                {
                    AssetDatabase.CreateFolder("Assets/AutoGenTextures", "Generated Textures");
                    Debug.Log("Created Folder AutoGenTextures/Generated Textures");
                }
                SaveSpriteAsAsset(Png, app);

            }

        }
    }

    static Sprite SaveSpriteAsAsset(Sprite sprite, string proj_path)
    {
        var abs_path = Path.Combine(Application.dataPath, proj_path);
        proj_path = Path.Combine("Assets", proj_path);

        //Directory.CreateDirectory(Path.GetDirectoryName(abs_path));
        File.WriteAllBytes(abs_path, ImageConversion.EncodeToPNG(sprite.texture));

        AssetDatabase.Refresh();

        var ti = AssetImporter.GetAtPath(proj_path) as TextureImporter;
        ti.spritePixelsPerUnit = sprite.pixelsPerUnit;
        ti.mipmapEnabled = false;
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.textureCompression = TextureImporterCompression.CompressedHQ;
        ti.maxTextureSize = 1024;

        EditorUtility.SetDirty(ti);
        ti.SaveAndReimport();

        Debug.Log($"Saved PNG {sprite.name} at path: {proj_path}");
        Sprite returnSprite = AssetDatabase.LoadAssetAtPath<Sprite>(proj_path);
        EditorGUIUtility.PingObject(returnSprite);
        return returnSprite;
    }
}
#endif