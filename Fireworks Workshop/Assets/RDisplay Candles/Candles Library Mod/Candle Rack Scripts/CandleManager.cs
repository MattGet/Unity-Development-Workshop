using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FireworksMania.Core.Behaviors.Fireworks;
using FireworksMania.Core.Definitions.EntityDefinitions;
using FireworksMania.Core.Definitions;
using FireworksMania.Core.Messaging;
using CustomCandles;

#if UNITY_EDITOR
using NaughtyAttributes;
#endif

[RequireComponent(typeof(CapsuleCollider))]
public class CandleManager : MonoBehaviour
{
    [HideInInspector]
    public float ManagerID = 0;
    [HideInInspector]
    public GameObject Candle;
    [HideInInspector]
    public bool HasCandle = false;

    [Header("Manager Settings")]
    [SerializeField]
    private CapsuleCollider Ctrigger;
    public bool IncludeZippers = false;
    public bool IsCandleCube = false;
    public GameSoundDefinition LoadSound;
    public Rigidbody RackBody;

#if UNITY_EDITOR
    [ShowIf("IsCandleCube")]
    [Foldout("Candle Cube")]
#endif
    public GameObject CubeTensionPart;
#if UNITY_EDITOR
    [ShowIf("IsCandleCube")]
    [Foldout("Candle Cube")]
#endif
    public float TensionPartSize = 0.1f;
#if UNITY_EDITOR
    [ShowIf("IsCandleCube")]
    [Foldout("Candle Cube")]
#endif
    public float CubeRadius = 0f;
#if UNITY_EDITOR
    [ShowIf("IsCandleCube")]
    [Foldout("Candle Cube")]
#endif
    public float CubeCrossSection = 0f;

#if UNITY_EDITOR
    [ShowIf("IncludeZippers")]
    [Foldout("Zip Ties")]
#endif
    [Header("Zip Tie Settings")]
    public GameObject ZipTiePrefab;
#if UNITY_EDITOR
    [ShowIf("IncludeZippers")]
    [Foldout("Zip Ties")]
#endif
    public Vector3 ZipPos1 = new Vector3(0f, 0.0549999997f, 0.00510000018f);
#if UNITY_EDITOR
    [ShowIf("IncludeZippers")]
    [Foldout("Zip Ties")]
#endif
    public Vector3 ZipPos2 = new Vector3(0f, 0.402999997f, 0.00510000018f);
#if UNITY_EDITOR
    [ShowIf("IncludeZippers")]
    [Foldout("Zip Ties")]
#endif
    public GameSoundDefinition ZipperSound;

    private GameObject Zip1;
    private GameObject Zip2;
    [HideInInspector]
    public bool DCIsBluePrint = true;


    // Start is called before the first frame update
    void Start()
    {
        GetCollider();
    }

    private void OnValidate()
    {
        GetCollider();

        if (this.gameObject.transform.parent.gameObject.name != "Candle Managers Parent")
        {
            this.gameObject.transform.parent.gameObject.name = "Candle Managers Parent";
        }
        if (RackBody == null)
        {
            RackBody = this.gameObject.transform.parent.parent.GetComponent<Rigidbody>();
        }
    }

    private void GetCollider()
    {
        if (Ctrigger == null)
        {
            Ctrigger = this.gameObject.GetComponent<CapsuleCollider>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!HasCandle)
        {
            RomanCandleBehavior Candlescript;
            if (other.gameObject.TryGetComponent(out Candlescript) && other.gameObject != Candle)
            {
                if (!other.gameObject.name.Contains("DC Enabled"))
                {
                    if (!HasCandle)
                    {
                        if (IsCandleCube)
                        {
                            HasCandle = true;
                            StartCoroutine(LoadInCube(other.gameObject));
                        }
                        else
                        {
                            HasCandle = true;
                            StartCoroutine(LoadCandle(other.gameObject));
                        }

                    }
                }
            }
        }
    }

    public IEnumerator LoadInCube(GameObject candle)
    {
        float candleheight = GetCapsuleHeight(candle);
        float Candleradius = GetCapsuleradius(candle);
        float modifier = CubeRadius - Candleradius;
        candle.transform.parent = this.gameObject.transform;
        candle.transform.localRotation = Quaternion.identity;
        candle.transform.localPosition = Vector3.zero;
        candle.transform.localPosition = new Vector3(candle.transform.localPosition.x + modifier -0.003f, candle.transform.localPosition.y + candleheight / 2, candle.transform.localPosition.z - modifier +0.003f);
        Candle = candle;
        candle.name = $"{candle.name} - DC Enabled";

        float angle = Mathf.Asin((CubeCrossSection - Candleradius) / CubeTensionPart.transform.localScale.z) * Mathf.Rad2Deg;
        CubeTensionPart.transform.eulerAngles = new Vector3(-(90 -angle), -45, 0);
        Debug.Log($"Angle = {angle}");
        Rigidbody rigidbody;
        if (candle.TryGetComponent(out rigidbody))
        {
            Destroy(rigidbody);
        }
        else
        {
            Debug.LogError($"Failed to locate Ridgidbody on {candle.name}");
        }

        if (LoadSound != null) Messenger.Broadcast(new MessengerEventPlaySound(LoadSound.name, Candle.transform));
        CandleRuntimeHelper helper = candle.AddComponent<CandleRuntimeHelper>();
        yield return new WaitForSeconds(Time.deltaTime);
        helper.Destroyed.AddListener(OnCandleDestroy);
    }


    public IEnumerator LoadCandle(GameObject candle)
    {
        //Debug.Log($"Loading Candle {candle}");
        float candleheight = GetCapsuleHeight(candle);

        candle.transform.parent = this.gameObject.transform;
        candle.transform.localRotation = Quaternion.identity;
        candle.transform.localPosition = Vector3.zero;
        candle.transform.localPosition = new Vector3(candle.transform.localPosition.x, candle.transform.localPosition.y + candleheight / 2, candle.transform.localPosition.z );
        Candle = candle;
        candle.name = $"{candle.name} - DC Enabled";

        Rigidbody rigidbody;
        if (candle.TryGetComponent(out rigidbody))
        {
            Destroy(rigidbody);
        }
        else
        {
            Debug.LogError($"Failed to locate Ridgidbody on {candle.name}");
        }

        if (IncludeZippers) StartCoroutine(AddZippers(true));
        if (LoadSound != null) Messenger.Broadcast(new MessengerEventPlaySound(LoadSound.name, Candle.transform));
        CandleRuntimeHelper helper = candle.AddComponent<CandleRuntimeHelper>();
        yield return new WaitForSeconds(Time.deltaTime);
        helper.Destroyed.AddListener(OnCandleDestroy);
    }

    private void OnCandleDestroy()
    {
        Destroy(Zip1);
        Destroy(Zip2);
        HasCandle = false;
        //Debug.Log("Destroyed Candle");
        if (RackBody != null) StartCoroutine(Freeze(RackBody));
    }

    private IEnumerator Freeze(Rigidbody body)
    {
        body.constraints = RigidbodyConstraints.FreezeAll;
        bool temp = body.isKinematic;
        body.isKinematic = true;
        yield return new WaitForSeconds(0.5f);
        body.constraints = RigidbodyConstraints.None;
        body.isKinematic = temp;
    }

    private IEnumerator AddZippers(bool withsoud)
    {
        if (Candle != null)
        {
            yield return new WaitForSeconds(0.5f);
            if (ZipperSound != null)
            {
                Zip1 = Instantiate(ZipTiePrefab, this.gameObject.transform);
                Zip1.transform.localPosition = ZipPos1;

                if (withsoud) Messenger.Broadcast(new MessengerEventPlaySound(ZipperSound.name, Candle.transform, true, true));
            }
            yield return new WaitForSeconds(0.5f);
            if (ZipperSound != null)
            {
                Zip2 = Instantiate(ZipTiePrefab, this.gameObject.transform);
                Zip2.transform.localPosition = ZipPos2;

                if (withsoud) Messenger.Broadcast(new MessengerEventPlaySound(ZipperSound.name, Candle.transform, true, true));
            }
        }
    }

    private float GetCapsuleradius(GameObject candle)
    {
        CapsuleCollider capsule;
        float radius = 0;
        if (candle.TryGetComponent(out capsule))
        {
           radius = capsule.radius;
        }
        else
        {
            radius = 0.05f;
            Debug.LogError($"Failed To Get Height for Candle: {candle.name}");
        }
        return radius;
    }

    private float GetCapsuleHeight(GameObject candle)
    {
        CapsuleCollider capsule;
        float height = 0;
        if (candle.TryGetComponent(out capsule))
        {
            height = capsule.height;
        }
        else
        {
            height = 1.5f;
            Debug.LogError($"Failed To Get Height for Candle: {candle.name}");
        }
        return height;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
