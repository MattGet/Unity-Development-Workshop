using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;
using FireworksMania.Core.Behaviors.Fireworks.Parts;
using FireworksMania.Core.Persistence;
using System.Threading;
using FireworksMania.Core.Behaviors.Fireworks;
using FireworksMania.Core;

namespace RemoteFiringSystem
{
    public class RemoteDetonator : BaseFireworkBehavior, IHaveFuse, IIgnitable, IHaveFuseConnectionPoint
    {

        public NumericDisplay RDchannelDis;
        public NumericDisplay RDminDis;
        public NumericDisplay RDsecDis;
        [HideInInspector]
        public int RDchannel = 0;
        [HideInInspector]
        public int RDmin = 0;
        [HideInInspector]
        public int RDseconds = 0;
        private Rigidbody _RDrigidbody;

        [HideInInspector]
        public bool fired = false;


        public void FireButton()
        {
            if (RDchannel >= 0 && RDchannel <= 99)
            {
                if (RDminDis != null && RDsecDis != null)
                {
                    RDmin = Mathf.RoundToInt(RDminDis.Number);
                    RDseconds = Mathf.RoundToInt(RDsecDis.Number);
                }

                if (RDseconds == 0 && RDmin == 0)
                {
                    //Debug.Log("countdown over, firing RDchannel = " + RDchannel);
                    this.transform.parent.gameObject.BroadcastMessage("FIRE", RDchannel);
                    this.IgniteInstant();
                }
                else
                {
                    StartCoroutine(Countdown());
                }
            }
        }

        public IEnumerator Countdown()
        {
            int i = RDmin;
            RDmin = Mathf.RoundToInt(RDminDis.Number);
            RDseconds = Mathf.RoundToInt(RDsecDis.Number);
            while (i >= 0)
            {
                //Debug.Log("RDminutes: " + i);
                for (int t = RDseconds; t >= 0; t--)
                {
                    yield return new WaitForSeconds(1);
                    //Debug.Log("RDseconds: " + t);
                    if (i == 0 && t == 0)
                    {
                        //Debug.Log("countdown over, firing");
                        RDminDis.UpdateDisplay(i);
                        RDsecDis.UpdateDisplay(t);
                        this.transform.parent.gameObject.BroadcastMessage("FIRE", RDchannel);
                        this.IgniteInstant();
                        yield break;
                    }
                    if (t == 0)
                    {
                        //Debug.Log("RDseconds = 0");
                        if (i > 0)
                        {
                            //Debug.Log("RDseconds = 0 swaping to sec");
                            i = i - 1;
                            RDminDis.UpdateDisplay(i);
                            t = 59;
                        }
                        RDsecDis.UpdateDisplay(t);
                    }
                    else
                    {
                        //Debug.Log("RDseconds counting down");
                        if (t > 0)
                        {
                            //Debug.Log("subtracting RDseconds");
                            RDsecDis.UpdateDisplay(t);
                        }
                    }
                    //Debug.Log("end of sec loop: " + t);
                }
                //Debug.Log("end of RDmin loop: " + i);
            }
        }

        public void FIRED(int chnl)
        {
            if (chnl == RDchannel)
            {
                fired = true;
            }
        }

        public override CustomEntityComponentData CaptureState()
        {

            CustomEntityComponentData entitydata = new CustomEntityComponentData();
            Rigidbody component = this.GetComponent<Rigidbody>();
            entitydata.Add<bool>("IsKinematic", (UnityEngine.Object)component != (UnityEngine.Object)null && component.isKinematic);
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

            RDchannel = Mathf.RoundToInt(RDchannelDis.Number);

            entitydata.Add<int>("Channel", RDchannel);

            if (RDminDis != null && RDsecDis != null)
            {
                RDmin = Mathf.RoundToInt(RDminDis.Number);
                RDseconds = Mathf.RoundToInt(RDsecDis.Number);
                entitydata.Add<int>("Mins", RDmin);
                entitydata.Add<int>("Seconds", RDseconds);
            }


            return entitydata;
        }

        public override void RestoreState(CustomEntityComponentData customComponentData)
        {
            SerializableVector3 serializableVector3 = customComponentData.Get<SerializableVector3>("Position");
            SerializableRotation serializableRotation = customComponentData.Get<SerializableRotation>("Rotation");
            bool flag = customComponentData.Get<bool>("IsKinematic");
            this.transform.position = new Vector3(serializableVector3.X, serializableVector3.Y, serializableVector3.Z);
            this.transform.rotation = new Quaternion(serializableRotation.X, serializableRotation.Y, serializableRotation.Z, serializableRotation.W);
            Rigidbody component = this.GetComponent<Rigidbody>();
            if (!((UnityEngine.Object)component != (UnityEngine.Object)null))
                return;
            component.isKinematic = flag;

            RDchannel = customComponentData.Get<int>("Channel");
            RDchannelDis.UpdateDisplay(RDchannel);

            if (RDminDis != null && RDsecDis != null)
            {
                RDmin = customComponentData.Get<int>("Mins");
                RDminDis.UpdateDisplay(RDmin);

                RDseconds = customComponentData.Get<int>("Seconds");
                RDsecDis.UpdateDisplay(RDseconds);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            this._RDrigidbody = this.GetComponent<Rigidbody>();
            if ((UnityEngine.Object)this._RDrigidbody == (UnityEngine.Object)null)
                Debug.LogError((object)"Missing Rigidbody", (UnityEngine.Object)this);
            //Debug.Log("SID = " + SaveableComponentTypeId);
        }

        protected override void Start()
        {
            base.Start();
            RDchannelDis.updateid.AddListener(updateID);
        }

        public void updateID(float id)
        {
            RDchannel = Mathf.RoundToInt(id);
        }

        protected override void OnValidate()
        {
            if (Application.isPlaying)
                return;
            base.OnValidate();
        }

        protected override async UniTask LaunchInternalAsync(CancellationToken token)
        {
            FireButton();
            await UniTask.WaitWhile(() => this.fired == false, PlayerLoopTiming.Update, token);
            token.ThrowIfCancellationRequested();
            if (!CoreSettings.AutoDespawnFireworks)
                return;
            await this.DestroyFireworkAsync(token);
        }


    }
}