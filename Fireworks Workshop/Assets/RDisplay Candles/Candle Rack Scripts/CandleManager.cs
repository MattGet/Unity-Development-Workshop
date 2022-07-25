using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FireworksMania.Core.Behaviors.Fireworks;
using FireworksMania.Core.Definitions.EntityDefinitions;
using FireworksMania.Core.Definitions;
using FireworksMania.Core.Messaging;

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
        RomanCandleBehavior Candlescript;
        if (other.gameObject.TryGetComponent(out Candlescript) && other.gameObject != Candle)
        {
            if (!HasCandle)
            {
                HasCandle = true;
                BaseEntityDefinition definition = Candlescript.EntityDefinition;
                GameObject candle = definition.PrefabGameObject;
                Destroy(other.gameObject);
                SpawnCandle(candle);
            }
        }
    }

    public void SpawnCandle(GameObject prefabcandle)
    {
        float candleheight = GetCapsuleHeight(prefabcandle);
        GameObject candle = Instantiate(prefabcandle, this.gameObject.transform);
        candle.transform.localPosition = new Vector3(candle.transform.localPosition.x, candle.transform.localPosition.y + candleheight / 2, candle.transform.localPosition.z);
        Candle = candle;

        Rigidbody rigidbody;
        if (candle.TryGetComponent(out rigidbody))
        {
            rigidbody.isKinematic = true;
        }
        else
        {
            Debug.LogError($"Failed to locate Ridgidbody on {candle.name}");
        }

        StartCoroutine(AddZippers(true));

        
    }

    private IEnumerator AddZippers(bool withsoud)
    {
        if (Candle != null)
        {
            if (ZipperSound != null)
            {
                GameObject Zip1 = Instantiate(ZipTiePrefab, this.gameObject.transform);
                Zip1.transform.localPosition = ZipPos1;

                if (withsoud) Messenger.Broadcast(new MessengerEventPlaySound(ZipperSound.name, Candle.transform, true, true));
            }
            yield return new WaitForSeconds(0.5f);
            if (ZipperSound != null)
            {
                GameObject Zip2 = Instantiate(ZipTiePrefab, this.gameObject.transform);
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
