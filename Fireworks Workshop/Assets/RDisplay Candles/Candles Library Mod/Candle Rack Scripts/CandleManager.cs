using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FireworksMania.Core.Behaviors.Fireworks;
using FireworksMania.Core.Definitions.EntityDefinitions;
using FireworksMania.Core.Definitions;
using FireworksMania.Core.Messaging;
using CustomCandles;

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

    [Header("Zip Tie Settings")]
    public GameObject ZipTiePrefab;
    public Vector3 ZipPos1 = new Vector3(0f, 0.0549999997f, 0.00510000018f);
    public Vector3 ZipPos2 = new Vector3(0f, 0.402999997f, 0.00510000018f);
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
                        HasCandle = true;
                        StartCoroutine(LoadCandle(other.gameObject));
                    }
                }
            }
        }
    }

    public IEnumerator SpawnCandle(GameObject prefabcandle)
    {
        //Debug.Log($"Spawning Candle {prefabcandle}");
        float candleheight = GetCapsuleHeight(prefabcandle);
        GameObject candle = Instantiate(prefabcandle, this.gameObject.transform);
        candle.transform.localPosition = Vector3.zero;
        candle.transform.localPosition = new Vector3(candle.transform.localPosition.x, candle.transform.localPosition.y + candleheight / 2, candle.transform.localPosition.z);
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

        StartCoroutine(AddZippers(true));
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
        candle.transform.localPosition = new Vector3(candle.transform.localPosition.x, candle.transform.localPosition.y + candleheight / 2, candle.transform.localPosition.z);
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

        StartCoroutine(AddZippers(true));
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
