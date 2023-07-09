
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FuseBehaviour : MonoBehaviour
{
    [Header("Fuse Wrapped Model:")]
    [Tooltip("The model of the fuse when the shell is not in a tube")]
    [SerializeField]
    private GameObject wrapped;

    [Header("Fuse Unwrapped Model:")]
    [Tooltip("The model of the fuse when the shell is inside a tube")]
    [SerializeField]
    private GameObject Unwrapped;

    [Header("Fuse Part:")]
    [Tooltip("The End part of your fuse with the \"Fuse\" Script as a component")]
    [SerializeField]
    private GameObject fuse;

    [Header("Fuse Part Position Wrapped:")]
    [Tooltip("The Local Position of the Standard Fuse Part when the shell is not in a tube")]
    [SerializeField]
    private Vector3 Wpos;

    [Header("Fuse Part Position Unwrapped:")]
    [Tooltip("The Local Position of the Standard Fuse Part when the shell is inside a tube")]
    [SerializeField]
    private Vector3 Upos;

    [Header("\nState of the Fuse:")]
    private bool IsWrapped = true;
#if UNITY_EDITOR
    private Vector3 StoreFCPos;
#endif

    private void Start()
    {
        IsWrapped = true;
        wrapped.SetActive(true);
        Unwrapped.SetActive(false);
        fuse.transform.localPosition = Wpos;
        //Debug.Log("Fuse Parent " + fuse.transform.parent.name);
        //Debug.Log("Fuse Local: " + fuse.transform.localPosition.ToString("f3")
        //    + "\nFuse World: " + fuse.transform.position.ToString("f3"));

    }

    /// <summary>
    /// Sets the fuse model to the unwrapped state
    /// </summary>
    public void UnWrapFuse()
    {
            //Debug.Log("Entered Tube");
            if (IsWrapped == true)
            {
                // Debug.Log("Setting Fuse to Unwrapped");
                wrapped.SetActive(false);
                Unwrapped.SetActive(true);
                fuse.transform.localPosition = Upos;
                //Debug.Log("Fuse Parent " + fuse.transform.parent.name);
                //Debug.Log("Fuse Local: " + fuse.transform.localPosition.ToString("f3")
                //    + "\nFuse World: " + fuse.transform.position.ToString("f3"));
                IsWrapped = false;
            }
    }

    /// <summary>
    /// Sets the fuse model to the wrapped state
    /// </summary>
    public void WrapFuse()
    {
        if (IsWrapped == false)
        {
            //Debug.Log("Set Fuse to Wrapped");
            wrapped.SetActive(true);
            Unwrapped.SetActive(false);
            fuse.transform.localPosition = Wpos;
            //Debug.Log("Fuse Parent " + fuse.transform.parent.name);
            //Debug.Log("Fuse Local: " + fuse.transform.localPosition.ToString("f3")
            //    + "\nFuse World: " + fuse.transform.position.ToString("f3"));
            IsWrapped = true;
        }
    }


#if UNITY_EDITOR
    /// <summary>
    /// Custom Inspector GUI that adds buttons to change the fuse state in the editor
    /// </summary>
    private void ChangeState()
    {

        if (!EditorApplication.isPlaying)
        {
            if (IsWrapped == true)
            {
                wrapped.SetActive(false);
                Unwrapped.SetActive(true);
                fuse.transform.localPosition = Upos;
                IsWrapped = false;
                Debug.Log("Set Fuse to UnWrapped");
                return;
            }
            if (IsWrapped == false)
            {

                wrapped.SetActive(true);
                Unwrapped.SetActive(false);
                fuse.transform.localPosition = Wpos;
                IsWrapped = true;
                Debug.Log("Set Fuse to Wrapped");
            }
        }
    }

    private void SetWpos()
    {
        Wpos = fuse.transform.localPosition;
    }

    private void SetUpos()
    {
        Upos = fuse.transform.localPosition;
    }

    private Vector3 GetFuseLocal()
    {
        return fuse.transform.localPosition;
    }

    private bool Iszeroed()
    {
        bool ret = false;
        if (Wpos == Vector3.zero || Upos == Vector3.zero) ret = true;
        return ret;
    }

    private void unpacker()
    {
        if (PrefabUtility.IsPartOfPrefabInstance(gameObject))
        {
            Debug.Log("Unpacking Prefab Instance " + gameObject.name);
            PrefabUtility.UnpackPrefabInstance(PrefabUtility.GetOutermostPrefabInstanceRoot(gameObject), PrefabUnpackMode.Completely, InteractionMode.UserAction);
        }
    }
#endif



    #region CustomInspector
#if UNITY_EDITOR
    [CustomEditor(typeof(FuseBehaviour))]
    public class CustomZipperInspector : Editor
    {
        SerializedProperty script;
        private void OnEnable()
        { 
            script = serializedObject.FindProperty("script");
        }
        public override void OnInspectorGUI()
        {
            FuseBehaviour targetobject = (FuseBehaviour)target;
            if (PrefabUtility.IsPartOfPrefabInstance(this.target))
            {
                targetobject.unpacker();
            }
            base.OnInspectorGUI();
            serializedObject.Update();
            var oldColor = GUI.backgroundColor;

            if (!Application.isPlaying)
            {
                serializedObject.Update();
                GUILayout.Space(20);
                if (GUILayout.Button("Change State of the Fuse"))
                {
                    targetobject.ChangeState();
                }
                GUILayout.Space(20);
                Vector3 temp = targetobject.GetFuseLocal();
                bool same =  temp != targetobject.StoreFCPos && temp != targetobject.Wpos && temp != targetobject.Upos;
                //Debug.Log(same + " " + targetobject.GetFuseLocal() + " " + targetobject.StoreFCPos);
                if (same || targetobject.Iszeroed())
                {
                    if (targetobject.Wpos == Vector3.zero || same)
                    {
                        if (GUILayout.Button("Set Current Fuse Part Position to Wrapped Position"))
                        {
                            targetobject.SetWpos();
                        }
                    }
                    if (targetobject.Upos == Vector3.zero || same)
                    {
                        GUILayout.Space(10);
                        if (GUILayout.Button("Set Current Fuse Part Position to Unwrapped Position"))
                        {
                            targetobject.SetUpos();
                        }

                    }
                    GUILayout.Space(20);
                }
                else targetobject.StoreFCPos = targetobject.GetFuseLocal();

            }
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
    #endregion CustomInspector
}




