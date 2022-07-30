#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using FireworksMania.Core.Definitions.EntityDefinitions;
using FireworksMania.Core.Behaviors.Fireworks;
using FireworksMania.Core.Persistence;
using System.Reflection;
using System.IO;

public class PrefabToEntityDefenition : UnityEditor.Editor
{
    [MenuItem("Assets/Prefab To Entity/Default Fireworks/Front View")]
    public static void Prefab2Entity()
    {
        GameObject Prefab = Selection.activeGameObject;

        if (PrefabUtility.GetPrefabAssetType(Prefab) == PrefabAssetType.NotAPrefab || PrefabUtility.GetPrefabAssetType(Prefab) == PrefabAssetType.Model) return;

        FireworkType firework = ReturnType(Prefab);
        FireworkEntityDefinition newDef = ScriptableObject.CreateInstance<FireworkEntityDefinition>();
        newDef.name = Prefab.name;
        string folderPath = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
        if (folderPath.Contains("."))
            folderPath = folderPath.Remove(folderPath.LastIndexOf('/'));
        Debug.Log(folderPath);
        AssetDatabase.CreateAsset(newDef, $"{ folderPath}/{Prefab.name}.asset");
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();

        Sprite icon = CaptureImage(Prefab, folderPath, true);

        Selection.activeObject = newDef;
        SerializedObject serializedObject = new SerializedObject(newDef);

        serializedObject.FindProperty("_id").stringValue = Prefab.name;
        serializedObject.FindProperty("_itemName").stringValue = Prefab.name;
        serializedObject.FindProperty("_prefabGameObject").objectReferenceValue = Prefab;
        serializedObject.FindProperty("_icon").objectReferenceValue = icon;

        switch (firework)
        {
            case FireworkType.Cake:
                serializedObject.FindProperty("_entityDefinitionType").objectReferenceValue = AssetDatabase.LoadAssetAtPath("Packages/net.laumania.fireworksmania-modtools/FireworksMania/Resources/EntityDefinitionTypes/Fireworks_Cake.asset", typeof(EntityDefinitionType));
                serializedObject.ApplyModifiedProperties();
                break;
            case FireworkType.Tube:
                serializedObject.FindProperty("_entityDefinitionType").objectReferenceValue = AssetDatabase.LoadAssetAtPath("Packages/net.laumania.fireworksmania-modtools/FireworksMania/Resources/EntityDefinitionTypes/Fireworks_Tube.asset", typeof(EntityDefinitionType));
                serializedObject.ApplyModifiedProperties();
                break;
            case FireworkType.Firecracker:
                serializedObject.FindProperty("_entityDefinitionType").objectReferenceValue = AssetDatabase.LoadAssetAtPath("Packages/net.laumania.fireworksmania-modtools/FireworksMania/Resources/EntityDefinitionTypes/Fireworks_Firecracker.asset", typeof(EntityDefinitionType));
                serializedObject.ApplyModifiedProperties();
                break;
            case FireworkType.Fountain:
                serializedObject.FindProperty("_entityDefinitionType").objectReferenceValue = AssetDatabase.LoadAssetAtPath("Packages/net.laumania.fireworksmania-modtools/FireworksMania/Resources/EntityDefinitionTypes/Fireworks_Fountains.asset", typeof(EntityDefinitionType));
                serializedObject.ApplyModifiedProperties();
                break;
            case FireworkType.Rocket:
                serializedObject.FindProperty("_entityDefinitionType").objectReferenceValue = AssetDatabase.LoadAssetAtPath("Packages/net.laumania.fireworksmania-modtools/FireworksMania/Resources/EntityDefinitionTypes/Fireworks_Rocket.asset", typeof(EntityDefinitionType));
                serializedObject.ApplyModifiedProperties();
                break;
            case FireworkType.Smoke:
                serializedObject.FindProperty("_entityDefinitionType").objectReferenceValue = AssetDatabase.LoadAssetAtPath("Packages/net.laumania.fireworksmania-modtools/FireworksMania/Resources/EntityDefinitionTypes/Fireworks_Smoke.asset", typeof(EntityDefinitionType));
                serializedObject.ApplyModifiedProperties();
                break;
            case FireworkType.Novelty:
                serializedObject.FindProperty("_entityDefinitionType").objectReferenceValue = AssetDatabase.LoadAssetAtPath("Packages/net.laumania.fireworksmania-modtools/FireworksMania/Resources/EntityDefinitionTypes/Fireworks_Novelty.asset", typeof(EntityDefinitionType));
                serializedObject.ApplyModifiedProperties();
                break;
            default:
                Debug.Log("No Suitable Entity Defenition Type Could Be Found!");
                break;
        }

        BaseFireworkBehavior Behaviors = Prefab.GetComponent<BaseFireworkBehavior>();

        var prop = Behaviors.GetType().BaseType.GetField("_entityDefinition", System.Reflection.BindingFlags.NonPublic
     | System.Reflection.BindingFlags.Instance);
        prop.SetValue(Behaviors, newDef);


        SaveableEntity Entity = Prefab.GetComponent<SaveableEntity>();
        SerializedObject SE = new SerializedObject(Entity);
        SE.FindProperty("_entityDefinition").objectReferenceValue = newDef;

        SE.ApplyModifiedProperties();
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(Prefab);
        EditorUtility.SetDirty(newDef);
    }

    [MenuItem("Assets/Prefab To Entity/Shell/Front View")]
    public static void Prefab2EntityShell()
    {
        GameObject Prefab = Selection.activeGameObject;

        if (PrefabUtility.GetPrefabAssetType(Prefab) == PrefabAssetType.NotAPrefab || PrefabUtility.GetPrefabAssetType(Prefab) == PrefabAssetType.Model) return;

        FireworkEntityDefinition newDef = ScriptableObject.CreateInstance<FireworkEntityDefinition>();
        newDef.name = Prefab.name;
        string folderPath = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
        if (folderPath.Contains("."))
            folderPath = folderPath.Remove(folderPath.LastIndexOf('/'));
        Debug.Log(folderPath);
        AssetDatabase.CreateAsset(newDef, $"{ folderPath}/{Prefab.name}.asset");
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();

        Sprite icon = CaptureImage(Prefab, folderPath, true);

        Selection.activeObject = newDef;
        SerializedObject serializedObject = new SerializedObject(newDef);

        serializedObject.FindProperty("_id").stringValue = Prefab.name;
        serializedObject.FindProperty("_itemName").stringValue = Prefab.name;
        serializedObject.FindProperty("_prefabGameObject").objectReferenceValue = Prefab;
        serializedObject.FindProperty("_icon").objectReferenceValue = icon;

        serializedObject.FindProperty("_entityDefinitionType").objectReferenceValue = AssetDatabase.LoadAssetAtPath("Packages/net.laumania.fireworksmania-modtools/FireworksMania/Resources/EntityDefinitionTypes/Fireworks_Shell.asset", typeof(EntityDefinitionType));
        serializedObject.ApplyModifiedProperties();


        BaseFireworkBehavior Behaviors = Prefab.GetComponent<BaseFireworkBehavior>();

        var prop = Behaviors.GetType().BaseType.GetField("_entityDefinition", System.Reflection.BindingFlags.NonPublic
     | System.Reflection.BindingFlags.Instance);
        prop.SetValue(Behaviors, newDef);


        SaveableEntity Entity = Prefab.GetComponent<SaveableEntity>();
        SerializedObject SE = new SerializedObject(Entity);
        SE.FindProperty("_entityDefinition").objectReferenceValue = newDef;

        SE.ApplyModifiedProperties();
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(Prefab);
        EditorUtility.SetDirty(newDef);
    }

    [MenuItem("Assets/Prefab To Entity/Mortar/Front View")]
    public static void Prefab2EntityMortar()
    {
        GameObject Prefab = Selection.activeGameObject;

        if (PrefabUtility.GetPrefabAssetType(Prefab) == PrefabAssetType.NotAPrefab || PrefabUtility.GetPrefabAssetType(Prefab) == PrefabAssetType.Model) return;

        PropEntityDefinition newDef = ScriptableObject.CreateInstance<PropEntityDefinition>();
        newDef.name = Prefab.name;
        string folderPath = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
        if (folderPath.Contains("."))
            folderPath = folderPath.Remove(folderPath.LastIndexOf('/'));
        Debug.Log(folderPath);
        AssetDatabase.CreateAsset(newDef, $"{ folderPath}/{Prefab.name}.asset");
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();

        Sprite icon = CaptureImage(Prefab, folderPath, true);

        Selection.activeObject = newDef;
        SerializedObject serializedObject = new SerializedObject(newDef);

        serializedObject.FindProperty("_id").stringValue = Prefab.name;
        serializedObject.FindProperty("_itemName").stringValue = Prefab.name;
        serializedObject.FindProperty("_prefabGameObject").objectReferenceValue = Prefab;
        serializedObject.FindProperty("_icon").objectReferenceValue = icon;

        serializedObject.FindProperty("_entityDefinitionType").objectReferenceValue = AssetDatabase.LoadAssetAtPath("Packages/net.laumania.fireworksmania-modtools/FireworksMania/Resources/EntityDefinitionTypes/Fireworks_Mortar.asset", typeof(EntityDefinitionType));
        serializedObject.ApplyModifiedProperties();

        SaveableEntity Entity = Prefab.GetComponent<SaveableEntity>();
        SerializedObject SE = new SerializedObject(Entity);
        SE.FindProperty("_entityDefinition").objectReferenceValue = newDef;

        SE.ApplyModifiedProperties();
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(Prefab);
        EditorUtility.SetDirty(newDef);
    }

    [MenuItem("Assets/Prefab To Entity/Prop/Front View")]
    public static void Prefab2EntityProp()
    {
        GameObject Prefab = Selection.activeGameObject;

        if (PrefabUtility.GetPrefabAssetType(Prefab) == PrefabAssetType.NotAPrefab || PrefabUtility.GetPrefabAssetType(Prefab) == PrefabAssetType.Model) return;

        PropEntityDefinition newDef = ScriptableObject.CreateInstance<PropEntityDefinition>();
        newDef.name = Prefab.name;
        string folderPath = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
        if (folderPath.Contains("."))
            folderPath = folderPath.Remove(folderPath.LastIndexOf('/'));
        Debug.Log(folderPath);
        AssetDatabase.CreateAsset(newDef, $"{ folderPath}/{Prefab.name}.asset");
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();

        Sprite icon = CaptureImage(Prefab, folderPath, true);

        Selection.activeObject = newDef;
        SerializedObject serializedObject = new SerializedObject(newDef);

        serializedObject.FindProperty("_id").stringValue = Prefab.name;
        serializedObject.FindProperty("_itemName").stringValue = Prefab.name;
        serializedObject.FindProperty("_prefabGameObject").objectReferenceValue = Prefab;
        serializedObject.FindProperty("_icon").objectReferenceValue = icon;

        serializedObject.FindProperty("_entityDefinitionType").objectReferenceValue = AssetDatabase.LoadAssetAtPath("Packages/net.laumania.fireworksmania-modtools/FireworksMania/Resources/EntityDefinitionTypes/Prop.asset", typeof(EntityDefinitionType));
        serializedObject.ApplyModifiedProperties();

        SaveableEntity Entity = Prefab.GetComponent<SaveableEntity>();
        SerializedObject SE = new SerializedObject(Entity);
        SE.FindProperty("_entityDefinition").objectReferenceValue = newDef;

        SE.ApplyModifiedProperties();
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(Prefab);
        EditorUtility.SetDirty(newDef);
    }

#region Back


    [MenuItem("Assets/Prefab To Entity/Default Fireworks/Back View")]
    public static void Prefab2Entity2()
    {
        GameObject Prefab = Selection.activeGameObject;

        if (PrefabUtility.GetPrefabAssetType(Prefab) == PrefabAssetType.NotAPrefab || PrefabUtility.GetPrefabAssetType(Prefab) == PrefabAssetType.Model) return;

        FireworkType firework = ReturnType(Prefab);
        FireworkEntityDefinition newDef = ScriptableObject.CreateInstance<FireworkEntityDefinition>();
        newDef.name = Prefab.name;
        string folderPath = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
        if (folderPath.Contains("."))
            folderPath = folderPath.Remove(folderPath.LastIndexOf('/'));
        Debug.Log(folderPath);
        AssetDatabase.CreateAsset(newDef, $"{ folderPath}/{Prefab.name}.asset");
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();

        Sprite icon = CaptureImage(Prefab, folderPath, false);

        Selection.activeObject = newDef;
        SerializedObject serializedObject = new SerializedObject(newDef);

        serializedObject.FindProperty("_id").stringValue = Prefab.name;
        serializedObject.FindProperty("_itemName").stringValue = Prefab.name;
        serializedObject.FindProperty("_prefabGameObject").objectReferenceValue = Prefab;
        serializedObject.FindProperty("_icon").objectReferenceValue = icon;

        switch (firework)
        {
            case FireworkType.Cake:
                serializedObject.FindProperty("_entityDefinitionType").objectReferenceValue = AssetDatabase.LoadAssetAtPath("Packages/net.laumania.fireworksmania-modtools/FireworksMania/Resources/EntityDefinitionTypes/Fireworks_Cake.asset", typeof(EntityDefinitionType));
                serializedObject.ApplyModifiedProperties();
                break;
            case FireworkType.Tube:
                serializedObject.FindProperty("_entityDefinitionType").objectReferenceValue = AssetDatabase.LoadAssetAtPath("Packages/net.laumania.fireworksmania-modtools/FireworksMania/Resources/EntityDefinitionTypes/Fireworks_Tube.asset", typeof(EntityDefinitionType));
                serializedObject.ApplyModifiedProperties();
                break;
            case FireworkType.Firecracker:
                serializedObject.FindProperty("_entityDefinitionType").objectReferenceValue = AssetDatabase.LoadAssetAtPath("Packages/net.laumania.fireworksmania-modtools/FireworksMania/Resources/EntityDefinitionTypes/Fireworks_Firecracker.asset", typeof(EntityDefinitionType));
                serializedObject.ApplyModifiedProperties();
                break;
            case FireworkType.Fountain:
                serializedObject.FindProperty("_entityDefinitionType").objectReferenceValue = AssetDatabase.LoadAssetAtPath("Packages/net.laumania.fireworksmania-modtools/FireworksMania/Resources/EntityDefinitionTypes/Fireworks_Fountains.asset", typeof(EntityDefinitionType));
                serializedObject.ApplyModifiedProperties();
                break;
            case FireworkType.Rocket:
                serializedObject.FindProperty("_entityDefinitionType").objectReferenceValue = AssetDatabase.LoadAssetAtPath("Packages/net.laumania.fireworksmania-modtools/FireworksMania/Resources/EntityDefinitionTypes/Fireworks_Rocket.asset", typeof(EntityDefinitionType));
                serializedObject.ApplyModifiedProperties();
                break;
            case FireworkType.Smoke:
                serializedObject.FindProperty("_entityDefinitionType").objectReferenceValue = AssetDatabase.LoadAssetAtPath("Packages/net.laumania.fireworksmania-modtools/FireworksMania/Resources/EntityDefinitionTypes/Fireworks_Smoke.asset", typeof(EntityDefinitionType));
                serializedObject.ApplyModifiedProperties();
                break;
            case FireworkType.Novelty:
                serializedObject.FindProperty("_entityDefinitionType").objectReferenceValue = AssetDatabase.LoadAssetAtPath("Packages/net.laumania.fireworksmania-modtools/FireworksMania/Resources/EntityDefinitionTypes/Fireworks_Novelty.asset", typeof(EntityDefinitionType));
                serializedObject.ApplyModifiedProperties();
                break;
            default:
                Debug.Log("No Suitable Entity Defenition Type Could Be Found!");
                break;
        }

        BaseFireworkBehavior Behaviors = Prefab.GetComponent<BaseFireworkBehavior>();

        var prop = Behaviors.GetType().BaseType.GetField("_entityDefinition", System.Reflection.BindingFlags.NonPublic
     | System.Reflection.BindingFlags.Instance);
        prop.SetValue(Behaviors, newDef);


        SaveableEntity Entity = Prefab.GetComponent<SaveableEntity>();
        SerializedObject SE = new SerializedObject(Entity);
        SE.FindProperty("_entityDefinition").objectReferenceValue = newDef;

        SE.ApplyModifiedProperties();
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(Prefab);
        EditorUtility.SetDirty(newDef);
    }

    [MenuItem("Assets/Prefab To Entity/Shell/Back View")]
    public static void Prefab2EntityShell2()
    {
        GameObject Prefab = Selection.activeGameObject;

        if (PrefabUtility.GetPrefabAssetType(Prefab) == PrefabAssetType.NotAPrefab || PrefabUtility.GetPrefabAssetType(Prefab) == PrefabAssetType.Model) return;

        FireworkEntityDefinition newDef = ScriptableObject.CreateInstance<FireworkEntityDefinition>();
        newDef.name = Prefab.name;
        string folderPath = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
        if (folderPath.Contains("."))
            folderPath = folderPath.Remove(folderPath.LastIndexOf('/'));
        Debug.Log(folderPath);
        AssetDatabase.CreateAsset(newDef, $"{ folderPath}/{Prefab.name}.asset");
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();

        Sprite icon = CaptureImage(Prefab, folderPath, false);

        Selection.activeObject = newDef;
        SerializedObject serializedObject = new SerializedObject(newDef);

        serializedObject.FindProperty("_id").stringValue = Prefab.name;
        serializedObject.FindProperty("_itemName").stringValue = Prefab.name;
        serializedObject.FindProperty("_prefabGameObject").objectReferenceValue = Prefab;
        serializedObject.FindProperty("_icon").objectReferenceValue = icon;

        serializedObject.FindProperty("_entityDefinitionType").objectReferenceValue = AssetDatabase.LoadAssetAtPath("Packages/net.laumania.fireworksmania-modtools/FireworksMania/Resources/EntityDefinitionTypes/Fireworks_Shell.asset", typeof(EntityDefinitionType));
        serializedObject.ApplyModifiedProperties();


        BaseFireworkBehavior Behaviors = Prefab.GetComponent<BaseFireworkBehavior>();

        var prop = Behaviors.GetType().BaseType.GetField("_entityDefinition", System.Reflection.BindingFlags.NonPublic
     | System.Reflection.BindingFlags.Instance);
        prop.SetValue(Behaviors, newDef);


        SaveableEntity Entity = Prefab.GetComponent<SaveableEntity>();
        SerializedObject SE = new SerializedObject(Entity);
        SE.FindProperty("_entityDefinition").objectReferenceValue = newDef;

        SE.ApplyModifiedProperties();
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(Prefab);
        EditorUtility.SetDirty(newDef);
    }

    [MenuItem("Assets/Prefab To Entity/Mortar/Back View")]
    public static void Prefab2EntityMortar2()
    {
        GameObject Prefab = Selection.activeGameObject;

        if (PrefabUtility.GetPrefabAssetType(Prefab) == PrefabAssetType.NotAPrefab || PrefabUtility.GetPrefabAssetType(Prefab) == PrefabAssetType.Model) return;

        PropEntityDefinition newDef = ScriptableObject.CreateInstance<PropEntityDefinition>();
        newDef.name = Prefab.name;
        string folderPath = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
        if (folderPath.Contains("."))
            folderPath = folderPath.Remove(folderPath.LastIndexOf('/'));
        Debug.Log(folderPath);
        AssetDatabase.CreateAsset(newDef, $"{ folderPath}/{Prefab.name}.asset");
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();

        Sprite icon = CaptureImage(Prefab, folderPath, false);

        Selection.activeObject = newDef;
        SerializedObject serializedObject = new SerializedObject(newDef);

        serializedObject.FindProperty("_id").stringValue = Prefab.name;
        serializedObject.FindProperty("_itemName").stringValue = Prefab.name;
        serializedObject.FindProperty("_prefabGameObject").objectReferenceValue = Prefab;
        serializedObject.FindProperty("_icon").objectReferenceValue = icon;

        serializedObject.FindProperty("_entityDefinitionType").objectReferenceValue = AssetDatabase.LoadAssetAtPath("Packages/net.laumania.fireworksmania-modtools/FireworksMania/Resources/EntityDefinitionTypes/Fireworks_Mortar.asset", typeof(EntityDefinitionType));
        serializedObject.ApplyModifiedProperties();

        SaveableEntity Entity = Prefab.GetComponent<SaveableEntity>();
        SerializedObject SE = new SerializedObject(Entity);
        SE.FindProperty("_entityDefinition").objectReferenceValue = newDef;

        SE.ApplyModifiedProperties();
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(Prefab);
        EditorUtility.SetDirty(newDef);
    }

    [MenuItem("Assets/Prefab To Entity/Prop/Back View")]
    public static void Prefab2EntityProp2()
    {
        GameObject Prefab = Selection.activeGameObject;

        if (PrefabUtility.GetPrefabAssetType(Prefab) == PrefabAssetType.NotAPrefab || PrefabUtility.GetPrefabAssetType(Prefab) == PrefabAssetType.Model) return;

        PropEntityDefinition newDef = ScriptableObject.CreateInstance<PropEntityDefinition>();
        newDef.name = Prefab.name;
        string folderPath = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
        if (folderPath.Contains("."))
            folderPath = folderPath.Remove(folderPath.LastIndexOf('/'));
        Debug.Log(folderPath);
        AssetDatabase.CreateAsset(newDef, $"{ folderPath}/{Prefab.name}.asset");
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();

        Sprite icon = CaptureImage(Prefab, folderPath, false); ;

        Selection.activeObject = newDef;
        SerializedObject serializedObject = new SerializedObject(newDef);

        serializedObject.FindProperty("_id").stringValue = Prefab.name;
        serializedObject.FindProperty("_itemName").stringValue = Prefab.name;
        serializedObject.FindProperty("_prefabGameObject").objectReferenceValue = Prefab;
        serializedObject.FindProperty("_icon").objectReferenceValue = icon;

        serializedObject.FindProperty("_entityDefinitionType").objectReferenceValue = AssetDatabase.LoadAssetAtPath("Packages/net.laumania.fireworksmania-modtools/FireworksMania/Resources/EntityDefinitionTypes/Prop.asset", typeof(EntityDefinitionType));
        serializedObject.ApplyModifiedProperties();

        SaveableEntity Entity = Prefab.GetComponent<SaveableEntity>();
        SerializedObject SE = new SerializedObject(Entity);
        SE.FindProperty("_entityDefinition").objectReferenceValue = newDef;

        SE.ApplyModifiedProperties();
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(Prefab);
        EditorUtility.SetDirty(newDef);
    }
    #endregion

    private static FireworkType ReturnType(GameObject item)
    {
        CakeBehavior Cake;
        if (item.TryGetComponent(out Cake))
        {
            return FireworkType.Cake;
        }
        PreloadedTubeBehavior Tube;
        if (item.TryGetComponent(out Tube))
        {
            return FireworkType.Tube;
        }
        RocketBehavior rocket;
        if (item.TryGetComponent(out rocket))
        {
            return FireworkType.Rocket;
        }
        FirecrackerBehavior firecracker;
        if (item.TryGetComponent(out firecracker))
        {
            return FireworkType.Firecracker;
        }
        RomanCandleBehavior romanCandle;
        if (item.TryGetComponent(out romanCandle))
        {
            return FireworkType.Novelty;
        }
        SmokeBombBehavior Smoke;
        if (item.TryGetComponent(out Smoke))
        {
            return FireworkType.Smoke;
        }
        FountainBehavior fountain;
        if (item.TryGetComponent(out fountain))
        {
            return FireworkType.Fountain;
        }
        RocketStrobeBehavior Strobe;
        if (item.TryGetComponent(out Strobe))
        {
            return FireworkType.Rocket;
        }
        ZipperBehavior zipperNovelty;
        if (item.TryGetComponent(out zipperNovelty))
        {
            return FireworkType.Novelty;
        }
        WhistlerBehavior whistlerNovelty;
        if (item.TryGetComponent(out whistlerNovelty))
        {
            return FireworkType.Novelty;
        }
        return FireworkType.Null;
    }


    public enum FireworkType
    {
        Cake,
        Rocket,
        Tube,
        Novelty,
        Fountain,
        Firecracker,
        Smoke,
        Null,
    }

    public static Sprite CaptureImage(GameObject pref, string path, bool front)
    {
        int width = 512;
        int height = 512;
        RuntimePreviewGenerator.BackgroundColor = Color.clear;
        RuntimePreviewGenerator.MarkTextureNonReadable = false;
        if (front)
        {
            RuntimePreviewGenerator.PreviewDirection = new Vector3(-0.75f, -1, 1.5f);
        }
        else
        {
            RuntimePreviewGenerator.PreviewDirection = new Vector3(-0.75f, -1, -1.5f);
        }

        RuntimePreviewGenerator.RenderSupersampling = 2;
        RuntimePreviewGenerator.OrthographicMode = true;


        GameObject temp = (GameObject)PrefabUtility.InstantiatePrefab(pref);


        Sprite result = SetTex(RuntimePreviewGenerator.GenerateModelPreview(temp.transform, width, height, false, true), temp, path);

        DestroyImmediate(temp);
        return result;
    }



    public static Sprite SetTex(Texture2D tex, GameObject prefObject, string path)
    {
        if (tex == null)
        {
            Debug.LogWarning("Failed to Produce Texture");
            return null;
        }
        if (!tex.isReadable)
        {
            Debug.Log("Texture Could not be Read");
            return null;
        }

        Sprite Png = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100);
        Png.name = prefObject.name + " AutoGeneratedImage";

        string name = path + $"/{Png.name}.png";
        Sprite result = SaveSpriteAsAsset(Png, name);
        return result;
    }


    static Sprite SaveSpriteAsAsset(Sprite sprite, string proj_path)
    {
        string dataPath = Application.dataPath;
        int point = dataPath.LastIndexOf("/");
        dataPath = dataPath.Substring(0, point);

        var abs_path = Path.Combine(dataPath, proj_path);

        //Directory.CreateDirectory(Path.GetDirectoryName(abs_path));
        File.WriteAllBytes(abs_path, ImageConversion.EncodeToPNG(sprite.texture));

        AssetDatabase.Refresh();

        var ti = AssetImporter.GetAtPath(proj_path) as TextureImporter;
        ti.spritePixelsPerUnit = sprite.pixelsPerUnit;
        ti.mipmapEnabled = false;
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.textureCompression = TextureImporterCompression.CompressedHQ;
        ti.maxTextureSize = 512;

        EditorUtility.SetDirty(ti);
        ti.SaveAndReimport();

        Debug.Log($"Saved PNG {sprite.name} at path: {proj_path}");
        Sprite returnSprite = AssetDatabase.LoadAssetAtPath<Sprite>(proj_path);
        EditorGUIUtility.PingObject(returnSprite);
        return returnSprite;
    }
}
#endif