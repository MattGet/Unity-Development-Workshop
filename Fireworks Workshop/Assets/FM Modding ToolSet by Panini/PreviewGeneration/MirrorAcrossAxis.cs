#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Experimental.SceneManagement;

public class MirrorAcrossAxis : UnityEditor.Editor
{
    [MenuItem("GameObject/Flip Across Y")]
    private static void FlipAcrossY()
    {
        GameObject Flip = Selection.activeGameObject;
        Undo.RecordObject(Flip, "Flipped Object Across Y Axis");
        var curr = Flip.transform.position;
        Flip.transform.position = new Vector3(curr.x * -1, curr.y, curr.z);
        var rot = Flip.transform.rotation;
        Flip.transform.rotation = new Quaternion(rot.x, rot.y, rot.z * -1, rot.w);
        var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        if (prefabStage != null)
        {
            EditorSceneManager.MarkSceneDirty(prefabStage.scene);
        }
    }
}
#endif