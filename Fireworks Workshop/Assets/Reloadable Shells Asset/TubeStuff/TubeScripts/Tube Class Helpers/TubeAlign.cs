using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FireworksMania.Core.Behaviors.Fireworks.Parts;

namespace CustomTubes
{
    public class TubeAlign : MonoBehaviour
    {
        private Quaternion temp;
        private GameObject shellobject;
        private float Id;
        private GameObject go;
        public float TempRadiusMultiplier;
        public float TempHeightMultiplier;
        private Fuse localfuse;


        public GameObject tube;
        private LoadableTubeBehaviour behaviour;
        // Start is called before the first frame update
        void Start()
        {
            go = transform.parent.gameObject;
            behaviour = tube.GetComponent<LoadableTubeBehaviour>();
            StartCoroutine(waitonload());
            //Debug.Log("behaviour = " + behaviour + behaviour.TubeID);
        }

        IEnumerator waitonload()
        {
            CapsuleCollider aligner = this.gameObject.GetComponent<CapsuleCollider>();
            aligner.radius = aligner.radius * 2;
            aligner.height = aligner.height * 1.5f;
            yield return new WaitForSecondsRealtime(2);
            aligner.radius = aligner.radius / 4;
            aligner.height = aligner.height / 3f;
            yield return null;
        }

        // Update is called once per frame

        public void OntubeEnter(GameObject other)
        {
            temp = gameObject.transform.localRotation;
            other.transform.localRotation = temp;

            shellobject = other;
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.material.dynamicFriction == 0.111f)
            {
                //Debug.Log("Shell name = " + other.gameObject.name);
                if (other.gameObject.name == behaviour.TubeID)
                {
                    tube.SendMessage("addCurrentShell", other.gameObject);
                }
                if (other.gameObject.name == "ignoreTA Potection")
                {
                    tube.SendMessage("addCurrentShell", other.gameObject);
                    //Debug.Log("Special Shell Detected by tube");
                }
            }
        }
    }

}
