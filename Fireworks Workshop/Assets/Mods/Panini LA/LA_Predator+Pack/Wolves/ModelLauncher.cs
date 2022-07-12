using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FireworksMania.Core.Behaviors.Fireworks.Parts;
using FireworksMania.Core.Behaviors.Fireworks;
using FireworksMania.Core.Messaging;
using FireworksMania.Core.Definitions;

public class ModelLauncher : MonoBehaviour
{
    public GameObject Model;
    public float Number;
    public float spacing;
    public float TimeDelayLaunch;
    public float StartDelay;
    public float StartSpeedyMAX = 50;
    public float StartSpeedyMIN = 10;
    public float StartSpeedvarMAX = 10;
    public float StartSpeedvarMIN = -10;
    public float ignitionForce = 50;
    public bool igniteOnLaunch = true;
    private bool onlydoonce;
    private Rigidbody _rigidbody;
    public Fuse CakeFuse;
    private GameObject scene;
    public GameSoundDefinition Sound;
    public ParticleSystem LaunchEffect;

    // Start is called before the first frame update
    void Start()
    {
        scene = GameObject.Find("[FireworksManager]");
    }

    // Update is called once per frame
    void Update()
    {
        if (CakeFuse.IsUsed && onlydoonce == false)
        {
            StartCoroutine(launcher());
            onlydoonce = true;
        }
    }

    IEnumerator launcher()
    {
        yield return new WaitForSeconds(StartDelay);
        float startpos = (((Number / 2) - 0.5f) * spacing) * (-1f);
        for (int i = 1; i <= Number; i++)
        {
            float up = Random.Range(StartSpeedyMIN, StartSpeedyMAX);
            float variance1 = Random.Range(StartSpeedvarMIN, StartSpeedvarMAX);
            float variance2 = Random.Range(StartSpeedvarMIN, StartSpeedvarMAX);
            GameObject item = Instantiate(Model, this.gameObject.transform);
            if (LaunchEffect != null)
            {
                GameObject effect = Instantiate(LaunchEffect.gameObject, this.gameObject.transform);
                effect.transform.localPosition = new Vector3(0, 0, startpos);
                effect.transform.parent = scene.transform;
                effect.transform.localScale = new Vector3(1, 1, 1);

            }
            item.transform.localPosition = new Vector3(0, 0, startpos);

            _rigidbody = item.GetComponent<Rigidbody>();

            if (igniteOnLaunch == true)
            {
                PreloadedTubeBehavior behavior = item.GetComponent<PreloadedTubeBehavior>();
                Messenger.Broadcast(new MessengerEventApplyIgnitableForce(behavior, ignitionForce));

            }
            if (Sound != null)
            {
                Messenger.Broadcast(new MessengerEventPlaySound(Sound.name, gameObject.transform, true, false));
            }
            item.transform.parent = scene.transform;
            item.transform.localScale = new Vector3(1, 1, 1);
            _rigidbody.AddRelativeForce(new Vector3(variance1, up, variance2), ForceMode.Impulse);


            startpos = startpos + spacing;
            yield return new WaitForSeconds(TimeDelayLaunch);
        }
    }
}
