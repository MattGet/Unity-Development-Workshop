using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMod;

public class BurstEmission : MonoBehaviour
{
    private ParticleSystem ps;
    private float m_Timer = 0.0f;
    public float m_Interval = 2.0f;
    // Start is called before the first frame update
    void Start()
    {
        ps = gameObject.GetComponent(typeof(ParticleSystem)) as ParticleSystem;
        Debug.Log("Particle system = " + ps);
    }

    // Update is called once per frame
    void Update()
    {
        m_Timer += Time.deltaTime;
        while (m_Timer >= m_Interval)
        {
            ps.TriggerSubEmitter(0);
            m_Timer -= m_Interval;
        }
    }
}
