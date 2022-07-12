#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class MenuPrefabToPNG : MonoBehaviour
{
    [MenuItem("Assets/Save Prefab Preview Texture")]
    private static void DoSomethingWithTexture()
    {
        int width = Screen.width;
        int height = Screen.height;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex = AssetPreview.GetAssetPreview(Selection.activeObject);

        // Read screen contents into the texture
       

        string prefabpath = AssetDatabase.GetAssetPath(Selection.activeObject);
        int end = prefabpath.LastIndexOf('/');
        
        string path = prefabpath.Substring(0, end);
        
        string name = path + Selection.activeObject.name + ".png";
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(name, bytes);
        DestroyImmediate(tex);
        Debug.Log("Preview PNG Created at: " + name);
    }

    // Note that we pass the same path, and also pass "true" to the second argument.
    [MenuItem("Assets/Save Prefab Preview Texture", true)]
    private static bool NewMenuOptionValidation()
    {
        if (Selection.activeObject == null) return false;
        // This returns true when the selected object is a Texture2D (the menu item will be disabled otherwise).
        return Selection.activeObject.GetType() == typeof(GameObject);
    }
}
#endif