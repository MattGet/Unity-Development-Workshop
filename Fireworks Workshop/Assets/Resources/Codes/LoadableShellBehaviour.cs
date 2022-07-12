
using UnityEngine;
using FireworksMania.Core.Behaviors.Fireworks.Parts;


public class LoadableShellBehaviour : MonoBehaviour
{
    public GameObject Fuse;
    public GameObject ShellObject;
    private Fuse localfuse;
    private bool movedUP = false;
    private Quaternion temp;
    private Vector3 localpos;
    private Vector3 worldpos;
    private bool islaunched;
    // Start is called before the first frame update
    void Start()
    {
        localfuse = Fuse.gameObject.GetComponent<Fuse>();
    }

    // Update is called once per frame
    void Update()
    {
        if (localfuse.IsUsed && islaunched == false)
        {
            OnFuseCompleted(localfuse);
            islaunched = true;
        }
    }
    void OnFuseCompleted(Fuse local)
    {
        Destroy(ShellObject);
        SetAllCollidersStatus(false);
    }
    public void SetAllCollidersStatus(bool active)
    {
        foreach (Collider c in gameObject.GetComponents<Collider>())
        {
            c.enabled = active;
        }
    }
    public void OnTriggerStay(Collider other)
    {
        if (other.material.dynamicFriction == 0.169f)
        {
            temp = other.gameObject.transform.parent.transform.rotation;
            gameObject.transform.rotation = temp;
            if (movedUP == false)
            {
                worldpos = other.gameObject.transform.parent.transform.position;
                worldpos = worldpos + other.gameObject.transform.localPosition;
                gameObject.transform.position = worldpos;

                gameObject.transform.parent = other.gameObject.transform.parent.transform;
                localpos = other.gameObject.transform.localPosition;
                gameObject.transform.localPosition = localpos;
                movedUP = true;
            }
        }
    }
    public void OnTriggerEnter(Collider other)
    {
        if(movedUP == true) stuck(other);
    }
    public void stuck(Collider other)
    {
        if (other.material.dynamicFriction == 0.111f)
        {
            if (IsInside(gameObject.transform.position))
            {
                float objsize;
                localpos = gameObject.transform.position;
                objsize = other.gameObject.GetComponentInChildren<Renderer>().bounds.size.y;
                localpos.y = localpos.y + (objsize * 2) + Random.Range(0f, 0.25f);
                gameObject.transform.up = localpos;
            }
        }
    }
        public static bool IsInside(Vector3 point)
    {
        bool trmp = false;
        Collider[] hitColliders = new Collider[10];
        int numColliders = Physics.OverlapSphereNonAlloc(point, 0.00001f, hitColliders, 0, QueryTriggerInteraction.Ignore);
        if (numColliders > 0)
        {
            trmp = true;
        }
        return trmp;
    }
};
