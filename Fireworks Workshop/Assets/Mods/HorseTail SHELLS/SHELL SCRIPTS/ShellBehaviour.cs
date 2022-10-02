using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using FireworksMania.Core.Behaviors.Fireworks.Parts;
using Cysharp.Threading.Tasks;
using System.Threading;
using FireworksMania.Core.Persistence;
using FireworksMania.Core.Definitions;

namespace FireworksMania.Core.Behaviors.Fireworks
{

    [AddComponentMenu("Fireworks Mania/Behaviors/Fireworks/ShellBehavior")]
    public class ShellBehaviour : BaseFireworkBehavior, IHaveFuse, IIgnitable, IHaveFuseConnectionPoint
    {

        [Header("Shell Settings")]
        [SerializeField]
        private ParticleSystem _effect;

        [Header("Shell Speed")]
        [Tooltip("The speed the shell will launch at when not inside a tube")]
        public float GroundShellSpeed = 10;
        private float LaunchForce;

        private bool inmortar = false;
        private bool Setspeed = false;
        private bool onlytriggeronce = false;
        private GameObject Tube;
        private GameObject Cyllinder;
        private Rigidbody _rigidbody;
        [HideInInspector]
        public string TubeID;

        [Header("Tube Launch Effect")]
        [Tooltip("Select a Particle effect PREFAB to spawn when ignited in a tube")]
        public ParticleSystem TubeEffect;

        [Header("Tube Load Sound - Leave empty for default sound")]
        [Tooltip("Select a GameSoundDefenition to be played when the Shell enters a tube\n\nLeave this empty to use the default tube sound instead!")]
        public GameSoundDefinition TubeLoadSound;

        [HideInInspector]
        public bool UseLaunchEffect = true;

        public override CustomEntityComponentData CaptureState()
        {

            CustomEntityComponentData entitydata = new CustomEntityComponentData();
            Rigidbody component = this.GetComponent<Rigidbody>();
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
            entitydata.Add<bool>("IsKinematic", (UnityEngine.Object)component != (UnityEngine.Object)null && component.isKinematic);
            entitydata.Add<bool>("mortar", inmortar);
            entitydata.Add<string>("ShellID", TubeID);
            //Debug.Log("stored Shell ID = " + TubeID);
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

            bool mortar = customComponentData.Get<bool>("mortar");
            string tubeid = customComponentData.Get<string>("ShellID");
            TubeID = tubeid;
            this.gameObject.name = TubeID;
            //Debug.Log("Loaded shell with name " + TubeID);
            inmortar = mortar;
        }

        public void SetTubeID(string id)
        {
            TubeID = id;
            this.gameObject.name = id;
            //Debug.Log("SetName = " + id);
        }

        public void GetTubeSound(GameObject sendto)
        {
            if (TubeLoadSound != null)
            {
                sendto.SendMessage("SetGSLoad", TubeLoadSound);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (inmortar == false && Setspeed == false)
            {
                var main = _effect.main;
                main.startSpeed = GroundShellSpeed;
                Setspeed = true;
                //Debug.Log(gameObject + " Start Speed set to: " + 1);
            }
            if (_fuse.IsUsed && inmortar == false && onlytriggeronce == false)
            {
                OnFuseCompleted(gameObject);
                onlytriggeronce = true;
            }
            if (_fuse.IsUsed && inmortar == true && onlytriggeronce == false && TubeEffect != null)
            {
                PlayEffect();
                StartCoroutine(waitforlaunch());
                OnFuseCompleted(gameObject);
                onlytriggeronce = true;
            }
            else if (_fuse.IsUsed && inmortar == true && onlytriggeronce == false && TubeEffect == null)
            {
                OnFuseCompleted(gameObject);
                onlytriggeronce = true;
            }
        }

        IEnumerator waitforlaunch()
        {
            float temp = _effect.main.startDelay.constant;
            yield return new WaitForSeconds(0.25f + temp);
            ParticleSystemSound shellshound = _effect.gameObject.GetComponent<ParticleSystemSound>();
            shellshound.enabled = true;
        }
        public void entermortar(GameObject tube)
        {
            //Debug.Log("Entermortar " + gameObject.name);

            var main = _effect.main;
            main.startSpeed = LaunchForce;
            //Debug.Log(gameObject + " Reset Speed set to: " + LaunchForce);
            inmortar = true;
            Tube = tube.transform.parent.gameObject;
            Cyllinder = tube;
            BroadcastMessage("UnWrapFuse");
        }
        void OnFuseCompleted(GameObject ShellObject)
        {
            //Debug.Log("Removing" + ShellObject + "from the game, Reason: Fuse ended");
            MeshRenderer[] models;
            models = ShellObject.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer model in models)
            {
                model.enabled = false;
            }
            SetAllobjectsStatus(false, ShellObject);
            if (inmortar == true && Cyllinder != null)
            {
                Cyllinder.SendMessage("removeShell");
            }
        }
        public void SetAllobjectsStatus(bool active, GameObject ShellObject)
        {
            foreach (Collider c in ShellObject.GetComponents<Collider>())
            {
                c.enabled = active;
            }
        }
        public void PlayEffect()
        {
            if (!UseLaunchEffect) return;
            ParticleSystemSound shellshound = _effect.gameObject.GetComponent<ParticleSystemSound>();
            shellshound.enabled = false;
            ParticleSystem effect = Instantiate(TubeEffect, transform, false);
            effect.transform.parent = Tube.transform;
        }

        public void UseTubeEffect(bool isOn)
        {
            UseLaunchEffect = isOn;
        }

        protected override void Awake()
        {
            base.Awake();
            this._rigidbody = this.GetComponent<Rigidbody>();
            if ((UnityEngine.Object)this._rigidbody == (UnityEngine.Object)null)
                Debug.LogError((object)"Missing Rigidbody", (UnityEngine.Object)this);
            if ((UnityEngine.Object)this._effect == (UnityEngine.Object)null)
                Debug.LogError((object)"Missing particle effects in CakeBehavior!");
            this.StopAllEffects();

            //Debug.Log("SID = " + SaveableComponentTypeId);
        }

        protected override void Start()
        {
            base.Start();
            LaunchForce = _effect.main.startSpeed.Evaluate(0, UnityEngine.Random.Range(0, 1));
           // Debug.Log(gameObject + " initial speed = " + LaunchForce);
            //Debug.Log(gameObject + " is at position " + gameObject.transform.position.ToString("F2") + " on spawn\n");
        }

        protected override void OnValidate()
        {
            if (Application.isPlaying)
                return;
            base.OnValidate();
            if (!((UnityEngine.Object)this._effect == (UnityEngine.Object)null))
                return;
            Debug.LogError((object)("Missing particle effect on gameobject '" + this.gameObject.name + "' on component 'ShellBehavior'!"), (UnityEngine.Object)this.gameObject);
        }

        protected override async UniTask LaunchInternalAsync(CancellationToken token)
        {
            this._effect.gameObject.SetActive(true);
            this._effect.Play(true);
            await UniTask.WaitWhile(() => this._effect.IsAlive() || this._effect.isPlaying, PlayerLoopTiming.Update, token);
            token.ThrowIfCancellationRequested();
            await this.DestroyFireworkAsync(token);
        }

        private void StopAllEffects()
        {
            this._effect.Stop();
            this._effect.gameObject.SetActive(false);
        }
    }
}
