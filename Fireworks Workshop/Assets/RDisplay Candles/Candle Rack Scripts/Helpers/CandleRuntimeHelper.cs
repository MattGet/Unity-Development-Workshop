using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using FireworksMania.Core.Behaviors.Fireworks.Parts;

namespace CustomCandles {
    public class CandleRuntimeHelper : MonoBehaviour
    {
        public GameObject Cap;
        public UnityEvent Destroyed = new UnityEvent();
        public Fuse candleFuse;

        private void Start()
        {
            candleFuse = this.gameObject.GetComponentInChildren<Fuse>();
            Debug.Log($"Found Fuse: {candleFuse}");
            MeshRenderer[] models = this.gameObject.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer M in models)
            {
                if (M.gameObject.name.Contains("Cap") || M.gameObject.name.Contains("cap"))
                {
                    Cap = M.gameObject;
                    Debug.Log($"Cap Found = {Cap}");
                    break;
                }
            }

            Debug.Log("Cangle Runtime Helper Intialized");
        }

        private void Update()
        {
            if (candleFuse != null && Cap != null)
            {
                if (candleFuse.IsUsed)
                {
                    Destroy(Cap);
                    Debug.Log("Candle Runtime Helper Destroyed Cap");
                }
            }
        }

        private void OnDestroy()
        {
            Destroyed.Invoke();
        }

    }

}
