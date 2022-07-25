using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using FireworksMania.Core.Behaviors.Fireworks.Parts;

namespace CustomCandles {
    public class CandleRuntimeHelper : MonoBehaviour
    {
        public GameObject Cap;
        public UnityEvent Destroyed;
        public Fuse candleFuse;

        private void Start()
        {
            candleFuse = this.gameObject.GetComponentInChildren<Fuse>();

            foreach (Transform T in this.gameObject.transform)
            {
                if (T.gameObject.name.Contains("Cap") || T.gameObject.name.Contains("cap"))
                {
                    Cap = T.gameObject;
                    break;
                }
            }
        }

        private void Update()
        {
            if (candleFuse != null && Cap != null)
            {
                if (candleFuse.IsUsed)
                {
                    Destroy(Cap);
                }
            }
        }

        private void OnDestroy()
        {
            Destroyed.Invoke();
        }
    }

}
