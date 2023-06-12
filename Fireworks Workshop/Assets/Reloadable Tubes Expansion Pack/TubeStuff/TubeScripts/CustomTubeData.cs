using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FireworksMania.Core.Persistence;
using FireworksMania.Core.Behaviors.Fireworks;
using FireworksMania.Core.Definitions.EntityDefinitions;
using FireworksMania.Core.Definitions;
using FireworksMania.Core.Common;
using CustomTubes;


    [AddComponentMenu("Fireworks Mania/Persistence/CustomTubeData")]
    public class CustomTubeData : MonoBehaviour, ISaveableComponent
    {
        [HideInInspector]
        private List<string> ids = new List<string>();
        public List<GameObject> tubes = new List<GameObject>();
        [SerializeField]
        private Rigidbody _rigidbody;

        public string SaveableComponentTypeId => "CustomTubeData";

        public void OnValidate()
        {
            SetTubes();
            this._rigidbody = this.GetComponent<Rigidbody>();
            if ((UnityEngine.Object)this._rigidbody == (UnityEngine.Object)null)
                Debug.LogError((object)"Missing Rigidbody On: " + this.gameObject.name, (UnityEngine.Object)this);
        }

        public void SetTubes()
        {

            tubes.Clear();
            LoadableTubeBehaviour[] tubeBehaviours = this.gameObject.GetComponentsInChildren<LoadableTubeBehaviour>();
            int i = 1;
            foreach (LoadableTubeBehaviour tubeBehaviour in tubeBehaviours)
            {
                tubes.Add(tubeBehaviour.gameObject);
                tubeBehaviour.gameObject.name =  "Cylinder Id: " + i;
                tubeBehaviour.TubeMatchNumber = i;
                i++;

            }

        }

        public void setIDs()
        {
            ids.Clear();
            foreach (GameObject G in tubes)
            {
                LoadableTubeBehaviour B = G.GetComponent<LoadableTubeBehaviour>();
                if (B == null)
                {
                    Debug.LogWarning("Id was null in tubedata");
                    return;
                }
                string temp = B.TubeID;
                ids.Add(temp);
            }
        }

        public CustomEntityComponentData CaptureState()
        {
            setIDs();
            CustomEntityComponentData entitydata = new CustomEntityComponentData();
            entitydata.Add<SerializableVector3>("Position", new SerializableVector3()
            {
                X = this.transform.position.x,
                Y = this.transform.position.y,
                Z = this.transform.position.z
            });
            entitydata.Add<SerializableRotation>("Rotation", new SerializableRotation()
            {
                X = this.transform.rotation.x,
                Y = this.transform.rotation.y,
                Z = this.transform.rotation.z,
                W = this.transform.rotation.w
            });
            //Debug.Log("TubeCapture4");
            entitydata.Add<List<string>>("ids", ids);
            Rigidbody component = this.GetComponent<Rigidbody>();
            entitydata.Add<bool>("IsKinematic", (UnityEngine.Object)component != (UnityEngine.Object)null && component.isKinematic);
            return entitydata;
        }

        public void RestoreState(CustomEntityComponentData customComponentData)
        {

            SerializableVector3 serializableVector3 = customComponentData.Get<SerializableVector3>("Position");
            SerializableRotation serializableRotation = customComponentData.Get<SerializableRotation>("Rotation");
            this.transform.position = new Vector3(serializableVector3.X, serializableVector3.Y, serializableVector3.Z);
            this.transform.rotation = new Quaternion(serializableRotation.X, serializableRotation.Y, serializableRotation.Z, serializableRotation.W);
            //Debug.Log("TubeRestore");
            bool flag = customComponentData.Get<bool>("IsKinematic");

            Rigidbody component = this.GetComponent<Rigidbody>();
            if (!((UnityEngine.Object)component != (UnityEngine.Object)null))
                return;
            component.isKinematic = flag;
            _rigidbody = component;
            StartCoroutine(RigidbodyHandler());

            ids = customComponentData.Get<List<string>>("ids");
            //Debug.Log("tubes = " + tubes.Count + " ids = " + ids.Count);
            for (int i = 0; i <= tubes.Count - 1; i++)
            {
                LoadableTubeBehaviour B = tubes[i].GetComponent<LoadableTubeBehaviour>();
                if (B == null)
                {
                    Debug.LogWarning("Id was null in tubedata");
                    return;
                }
                B.TubeID = ids[i];
            }
            
        }

        private IEnumerator RigidbodyHandler()
        {
            bool original = _rigidbody.isKinematic;
            _rigidbody.isKinematic = true;
            _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
            yield return new WaitForSeconds(3);
            _rigidbody.isKinematic = original;
            _rigidbody.constraints = RigidbodyConstraints.None;
        }
    }

