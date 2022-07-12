using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FireworksMania.Core.Messaging;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Experimental.SceneManagement;
#endif
using Fractures;

[AddComponentMenu("Mania Destruction")]
public class ManiaDestructor : MonoBehaviour
{
    public GameObject ObjectToFracture;
    public Rigidbody ThisRigidbody;
    public int NumberOfFragments = 10;
    public int MinForceToBreak = 300;
    public Material InteriorMaterial;
    public bool AddErasableBehvaior = false;
    public bool DestroyAsync = true;

    [HideInInspector]
    public Fracture destroythis;

    public bool GenerateAtRuntime = true;

    // Start is called before the first frame update
    void Start()
    {
        Messenger.AddListener<MessengerEventApplyExplosionForce>(new Callback<MessengerEventApplyExplosionForce>(this.OnApplyExplosionForce));
        //Debug.Log("added listner");

        if (destroythis == null && GenerateAtRuntime)
        {
            if (ThisRigidbody != null && ObjectToFracture != null)
            {
                Debug.Log($"Adding Fracture Component To {this.gameObject.name}");
                AddFractureRuntime();
            }
        }
    }

    private void OnDestroy()
    {
        Messenger.RemoveListener<MessengerEventApplyExplosionForce>(new Callback<MessengerEventApplyExplosionForce>(this.OnApplyExplosionForce));
    }

    private void OnApplyExplosionForce(MessengerEventApplyExplosionForce args) => this.GotExplosionForce(args.RigidBody, args.ActualExplosionForce, args.Position, args.Range, args.UpwardsModifier, args.ForceMode);

    public void GotExplosionForce(
      Rigidbody rigidBody,
      float actualExplosionForce,
      Vector3 position,
      float range,
      float upwardsmodifier,
      ForceMode forceMode)
    {
        //Debug.Log("2Got Explosion Force\n Rigidbody : " + rigidBody + "\nExplosion Force : " + actualExplosionForce + "\nPosition : " + position + "\nRange : " + range + "\nUpwardsModifier : " + upwardsmodifier + "\nForce Mode : " + forceMode);

        if (destroythis != null)
        {
            destroythis.GotExplosionForce(rigidBody, actualExplosionForce, position, range, upwardsmodifier, forceMode, AddErasableBehvaior);
            Debug.Log($"Fired Explosion Event On {this.gameObject.name}");
        }
        else
        {
            Debug.Log("Fracture Was Null When Attempting To Explode");
        }

    }

    public void AddFractureRuntime()
    {
        MeshFilter temp1;
        MeshRenderer temp2;
        Collider temp3;

        if (!ObjectToFracture.TryGetComponent(out temp1))
        {
            Debug.LogWarning($"The Object to Fracture Must have a Mesh Filter, Mesh Renderer, And Collider Component!");
        }
        else if (!ObjectToFracture.TryGetComponent(out temp2))
        {
            Debug.LogWarning($"The Object to Fracture Must have a Mesh Filter, Mesh Renderer, And Collider Component!");
        }
        else if (!ObjectToFracture.TryGetComponent(out temp3))
        {
            Debug.LogWarning($"The Object to Fracture Must have a Mesh Filter, Mesh Renderer, And Collider Component!");
        }
        else
        {
            destroythis = ObjectToFracture.AddComponent<Fracture>();
            destroythis.FractureBody = ThisRigidbody;
            TriggerOptions temp = new TriggerOptions();
            temp.triggerType = TriggerType.ExplosionEvent;
            temp.minimumCollisionForce = MinForceToBreak;
            destroythis.triggerOptions = temp;
            FractureOptions frac = new FractureOptions();
            frac.asynchronous = DestroyAsync;
            frac.fragmentCount = NumberOfFragments;

            if (InteriorMaterial != null)
            {
                frac.insideMaterial = InteriorMaterial;
            }
            destroythis.fractureOptions = frac;
            RefractureOptions refrac = new RefractureOptions();
            refrac.enableRefracturing = false;
            refrac.maxRefractureCount = 0;
            destroythis.refractureOptions = refrac;



            Debug.Log("Created Fracture");
        }

        
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        Rigidbody temp;

        if (this.gameObject.TryGetComponent(out temp))
        {
            ThisRigidbody = temp;
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                EditorSceneManager.MarkSceneDirty(prefabStage.scene);
            }
        }

        Fracture temp2;
        if (destroythis == null)
        {
            if (this.ObjectToFracture != null)
            {
                if (this.ObjectToFracture.TryGetComponent(out temp2))
                {
                    destroythis = temp2;
                    var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                    if (prefabStage != null)
                    {
                        EditorSceneManager.MarkSceneDirty(prefabStage.scene);
                    }
                }
            }
            
        }
        else
        {
            if (destroythis.triggerOptions != null)
            {
                TriggerOptions trig = new TriggerOptions();
                trig.triggerType = TriggerType.ExplosionEvent;
                trig.minimumCollisionForce = MinForceToBreak;
                destroythis.triggerOptions = trig;
            }
            if (destroythis.fractureOptions != null)
            {
                FractureOptions frac = new FractureOptions();
                frac.asynchronous = DestroyAsync;
                frac.fragmentCount = NumberOfFragments;
                if (InteriorMaterial != null)
                {
                    frac.insideMaterial = InteriorMaterial;
                }
                destroythis.fractureOptions = frac;
            }
            if (destroythis.refractureOptions != null)
            {
                RefractureOptions refrac = new RefractureOptions();
                refrac.enableRefracturing = false;
                refrac.maxRefractureCount = 0;
                destroythis.refractureOptions = refrac;
            }

            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                EditorSceneManager.MarkSceneDirty(prefabStage.scene);
            }
        }


    }



    public void AddFracture()
    {

        destroythis = ObjectToFracture.AddComponent<Fracture>();
        destroythis.FractureBody = ThisRigidbody;
        TriggerOptions temp = new TriggerOptions();
        temp.triggerType = TriggerType.ExplosionEvent;
        temp.minimumCollisionForce = MinForceToBreak;
        destroythis.triggerOptions = temp;
        FractureOptions frac = new FractureOptions();
        frac.asynchronous = DestroyAsync;
        frac.fragmentCount = NumberOfFragments;

        if (InteriorMaterial != null)
        {
            frac.insideMaterial = InteriorMaterial;
        }
        destroythis.fractureOptions = frac;


        Debug.Log("Created Fracture");
        var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        if (prefabStage != null)
        {
            EditorSceneManager.MarkSceneDirty(prefabStage.scene);
        }
    }
#endif
}


#region CustomInspector
#if UNITY_EDITOR
[CustomEditor(typeof(ManiaDestructor))]
public class CustomDestructorInspector : Editor
{
    SerializedProperty thisscript;
    private void OnEnable()
    {
        thisscript = serializedObject.FindProperty("thisscript");
    }
    public override void OnInspectorGUI()
    {
        ManiaDestructor script = (ManiaDestructor)target;

        base.OnInspectorGUI();
        serializedObject.Update();
        var oldColor = GUI.backgroundColor;
        if (!Application.isPlaying)
        {

            if (script.destroythis == null && !script.GenerateAtRuntime)
            {
                if (GUILayout.Button("Create Fracture"))
                {
                    if (script.ObjectToFracture != null)
                    {
                        MeshFilter temp1;
                        MeshRenderer temp2;
                        Collider temp3;

                        if (!script.ObjectToFracture.TryGetComponent(out temp1))
                        {
                            Debug.LogWarning($"The Object to Fracture Must have a Mesh Filter, Mesh Renderer, And Collider Component!");
                        }
                        else if (!script.ObjectToFracture.TryGetComponent(out temp2))
                        {
                            Debug.LogWarning($"The Object to Fracture Must have a Mesh Filter, Mesh Renderer, And Collider Component!");
                        }
                        else if (!script.ObjectToFracture.TryGetComponent(out temp3))
                        {
                            Debug.LogWarning($"The Object to Fracture Must have a Mesh Filter, Mesh Renderer, And Collider Component!");
                        }
                        else
                        {
                            if (script.ObjectToFracture != null && script.ThisRigidbody != null && script.destroythis == null)
                            {
                                script.AddFracture();
                            }
                            else
                            {
                                Debug.LogWarning("Failed to create fracture, Requires ObjectToFracture & Rigidbody to create!");
                            }
                        }
                    }
                    
                }
            }


            serializedObject.ApplyModifiedProperties();
        }
    }

}
#endif
#endregion