using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
#if UNITY_EDITOR
using NaughtyAttributes;
#endif
using FireworksMania.Core.Behaviors.Fireworks;
using FireworksMania.Core;
using FireworksMania.Core.Behaviors.Fireworks.Parts;

public class RealisticCakeBehaviour : BaseFireworkBehavior, IHaveFuse, IIgnitable, IHaveFuseConnectionPoint
{
    public GameObject TubeParent;
    public GameObject CakeTop;
    public float TimeBetweenLaunch;
    public List<GameObject> CakeTubes;
    public GameObject Pellet;
    public List<float> LaunchTimes;
    private bool firing;
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        if (CakeTop != null)
        {
            CakeTop.SetActive(true);
        }
        
        if (TubeParent == null)
        {
            foreach (Transform T in this.transform)
            {
                if (T.gameObject.name == "CakeTubes")
                {
                    TubeParent = T.gameObject;
                    break;
                }
            }
        }
        if (TubeParent != null)
        {
            CakeTubes.Clear();
            foreach (Transform T in TubeParent.transform)
            {
                CakeTubes.Add(T.gameObject);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    protected override void OnValidate()
    {
        base.OnValidate();
        if (TubeParent == null)
        {
            foreach (Transform T in this.transform)
            {
                if (T.gameObject.name == "CakeTubes")
                {
                    TubeParent = T.gameObject;
                    break;
                }
            }
        }
        if (TubeParent != null)
        {
            CakeTubes.Clear();
            foreach (Transform T in TubeParent.transform)
            {
                CakeTubes.Add(T.gameObject);
            }
        }
        if (CakeTubes.Count > 0)
        {
            LaunchTimes.Clear();
            float i = 0;
            foreach (GameObject G in CakeTubes)
            {
                LaunchTimes.Add(i);
                i = i + TimeBetweenLaunch;
            }
        }

    }

#if UNITY_EDITOR
    [Button("Test Cake (Rquires Play Mode)", EButtonEnableMode.Playmode)]
#endif
    public void IGNITE()
    {
        if (TubeParent == null)
        {
            foreach (Transform T in this.transform)
            {
                if (T.gameObject.name == "CakeTubes")
                {
                    TubeParent = T.gameObject;
                    break;
                }
            }
        }
        if (TubeParent != null)
        {
            CakeTubes.Clear();
            foreach (Transform T in TubeParent.transform)
            {
                CakeTubes.Add(T.gameObject);
            }
        }
        StartCoroutine(Launch());
    }

    private IEnumerator Launch()
    {
        CakeTop.SetActive(false);
        for (int i = 0; i <= CakeTubes.Count - 1; i++)
        {
            yield return new WaitForSeconds(LaunchTimes[i]);
            foreach (Transform T in CakeTubes[i].transform)
            {
                Destroy(T.gameObject);
            }
            GameObject PelletTemp = GameObject.Instantiate(Pellet, TubeParent.transform);
            PelletTemp.transform.localPosition = CakeTubes[i].transform.localPosition;
            PelletTemp.transform.localPosition = new Vector3(PelletTemp.transform.localPosition.x, CakeTop.transform.localPosition.y + (PelletTemp.transform.localScale.y *2), PelletTemp.transform.localPosition.z);
            PelletBehaviour temp = PelletTemp.GetComponent<PelletBehaviour>();
            temp.LAUNCH();
            yield return new WaitForFixedUpdate();
        }
    }

    protected override async UniTask LaunchInternalAsync(CancellationToken token)
    {
        IGNITE();
        firing = true;
        await UniTask.WaitWhile(() => this.firing == true, PlayerLoopTiming.Update, token);
        token.ThrowIfCancellationRequested();
        if (CoreSettings.AutoDespawnFireworks)
        {
            await this.DestroyFireworkAsync(token);
        }
    }
}
