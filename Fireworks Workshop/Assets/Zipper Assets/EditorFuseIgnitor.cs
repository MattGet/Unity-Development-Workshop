using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using FireworksMania.Core.Behaviors.Fireworks;

public class EditorFuseIgnitor : MonoBehaviour
{
    [Header("This will ignite the fuse of any child fireworks within the unity editor during runtime")]
    public string Key = "Press P to Use";

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Ignite();
        }
    }

    private void Ignite()
    {
        Debug.Log("Searching for Fuses");
        foreach (Transform T in this.gameObject.transform)
        {
            BaseFireworkBehavior fuse;
            if (T.gameObject.TryGetComponent<BaseFireworkBehavior>(out fuse))
            {
                fuse.IgniteInstant();
                Debug.Log("Ignited Fuse On: " + T.gameObject.name);
            }
        }
    }




#if UNITY_EDITOR
    [CustomEditor(typeof(EditorFuseIgnitor))]
    public class CustomFuseIgnitorInspector : Editor
    {
        SerializedProperty thisscript;
        private void OnEnable()
        {
            thisscript = serializedObject.FindProperty("thisscript");
        }
        public override void OnInspectorGUI()
        {
            EditorFuseIgnitor script = (EditorFuseIgnitor)target;

            base.OnInspectorGUI();
            serializedObject.Update();
            var oldColor = GUI.backgroundColor;
            if (Application.isPlaying)
            {

                GUILayout.Space(20);
                        if (GUILayout.Button("Ignite Fireworks"))
                        {
                            script.Ignite();
                            Debug.Log("Started Ignition");
                        }

                GUILayout.Space(20);


                serializedObject.ApplyModifiedProperties();
            }
        }

    }


#endif
}
