using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using NaughtyAttributes;


[ExecuteAlways]
#endif
public class PelletBehaviour : MonoBehaviour
{
    public GameObject Pellet;
    public ParticleSystem ExplosionEffect;
    public ParticleSystem Trail;
    public GameObject LaunchEffectPrefab;

    public float LaunchForce = 50f;
#if UNITY_EDITOR
    [HideIf("ExplodeAtApex")]
#endif
    public float FlightTime = 3;
    [Range(0f, 10f)]
    public float HorizontalVelocity = 3;

    public bool ExplodeAtApex = false;

    private Rigidbody PelletBody;
    private MeshRenderer Mrenderer;

    private void OnValidate()
    {
        if (Pellet == null)
        {
            Pellet = this.gameObject;
        }
        if (Mrenderer == null)
        {
            Mrenderer = this.gameObject.GetComponentInChildren<MeshRenderer>();
        }
    }
    private void Start()
    {
        if (ExplosionEffect != null)
        {
            ExplosionEffect.Stop(true);
            ExplosionEffect.gameObject.SetActive(false);
        }
        if (Trail != null)
        {
            Trail.Stop(true);
            Trail.gameObject.SetActive(false);
        }
    }

#if UNITY_EDITOR
    [Button("Test Pellet (Requires Play Mode)", EButtonEnableMode.Playmode)]
#endif
    public void LAUNCH()
    {
        if (Application.isPlaying)
        {
            StartCoroutine(Launch());
        }
        
    }

    private IEnumerator Launch()
    {
        GameObject LaunchTemp = GameObject.Instantiate(LaunchEffectPrefab, this.gameObject.transform.parent.transform);
        LaunchTemp.transform.localPosition = this.gameObject.transform.localPosition;
        PelletBody = Pellet.AddComponent<Rigidbody>();
        PelletBody.AddExplosionForce(LaunchForce, Pellet.transform.position, 5, 1);
        PelletBody.velocity = PelletBody.velocity + new Vector3(Random.Range(-HorizontalVelocity, HorizontalVelocity), PelletBody.velocity.y, Random.Range(-HorizontalVelocity, HorizontalVelocity));
        
        Trail.gameObject.SetActive(true);
        Trail.Play();
        if (ExplodeAtApex)
        {
            yield return new WaitUntil(() => (PelletBody.velocity.y <= 1));
        }
        else
        {
            yield return new WaitForSeconds(FlightTime);
        }
        Trail.Stop(true);
        if (Mrenderer == null)
        {
            Mrenderer = this.gameObject.GetComponentInChildren<MeshRenderer>();
        }
        Mrenderer.enabled = false;
        ExplosionEffect.gameObject.SetActive(true);
        ExplosionEffect.Play();
    }
}
