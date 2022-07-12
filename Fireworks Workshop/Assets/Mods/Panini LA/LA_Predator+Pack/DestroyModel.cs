using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FireworksMania.Core.Behaviors.Fireworks.Parts;

public class DestroyModel : MonoBehaviour
{
    [Header("Model To Destroy")]
    [Tooltip("This is the Model Gameobject that will be destroyed")]
    public GameObject Model;
    [Header("Destroy Firework Object")]
    [Tooltip("If checked all Models/meshes in your firework will be destroyed instead of the selsected one")]
    public bool DestroyAll = false;
    [Header("Time Delay From End Of Fuse")]
    [Tooltip("Number of Seconds after the fuse has ended to delete the model")]
    public float TimeDelay = 0f;
    private bool startedonce = false;
    private bool Deleted = false;
    private Rigidbody _rigidbody;
    private Collider[] _colliders;
    private Fuse _fuse;
    // Start is called before the first frame update
    void Awake()
    {
        this._rigidbody = this.GetComponent<Rigidbody>();
        if ((UnityEngine.Object)this._rigidbody == (UnityEngine.Object)null)
            Debug.LogError((object)"Missing Rigidbody on Firework", (UnityEngine.Object)this);
        this._colliders = this._rigidbody.GetComponents<Collider>();
        this._fuse = this.gameObject.GetComponentInChildren<Fuse>();
        ToggleMeshRendering(true, gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (_fuse.IsUsed && Deleted == false && startedonce == false)
        {
            StartCoroutine(waitfordelay());
            startedonce = true;
        }
    }

    IEnumerator waitfordelay()
    {
        yield return new WaitForSeconds(TimeDelay);
        if (DestroyAll == true)
        {
            delete();
        }
        else if (Model != null)
        {
            ToggleMeshRendering(false, Model);
            Collider[] colliders = Model.GetComponentsInChildren<Collider>();
            if (colliders != null)
            {
                foreach (Collider collider in colliders)
                    collider.enabled = false;
            }

            Collider[] colliders2 = Model.GetComponents<Collider>();
            if (colliders != null)
            {
                foreach (Collider collider in colliders2)
                    collider.enabled = false;
            }
            Deleted = true;
        }
    }

    private void delete()
    {
        ToggleMeshRendering(false, gameObject);
        DisableRigidBodyAndColliders();
        Deleted = true;
    }

    private void ToggleMeshRendering(bool state, GameObject @object)
    {
        MeshRenderer[] meshRenderers = @object.GetComponentsInChildren<MeshRenderer>();
       if(meshRenderers != null)
        {
            foreach (MeshRenderer mesh in meshRenderers)
                mesh.enabled = state;
        }
        MeshRenderer[] meshRenderers2 = @object.GetComponents<MeshRenderer>();
        if (meshRenderers != null)
        {
            foreach (MeshRenderer mesh in meshRenderers2)
                mesh.enabled = state;
        }

    }
    private void DisableRigidBodyAndColliders()
    {
        this._rigidbody.isKinematic = true;
        this._rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        this._rigidbody.detectCollisions = false;
        this._rigidbody.useGravity = false;
        Collider[] colliders = gameObject.GetComponentsInChildren<Collider>();
        if (colliders != null)
        {
            foreach (Collider collider in colliders)
                collider.enabled = false;
        }

        Collider[] colliders2 = gameObject.GetComponents<Collider>();
        if (colliders != null)
        {
            foreach (Collider collider in colliders2)
                collider.enabled = false;
        }
       
        foreach (Collider collider in this._colliders)
            collider.enabled = false;
    }
}
