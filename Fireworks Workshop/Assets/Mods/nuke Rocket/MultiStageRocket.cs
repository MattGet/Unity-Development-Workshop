using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;
using FireworksMania.Core.Behaviors.Fireworks.Parts;
using System;
using System.Threading;
using FireworksMania.Core.Attributes;
using FireworksMania.Core.Messaging;

namespace FireworksMania.Core.Behaviors.Fireworks
{
    [AddComponentMenu("Fireworks Mania/Behaviors/Fireworks/MultiStageRocket")]
    public class MultiStageRocket : BaseFireworkBehavior
    {
        [Header("Rocket Settings")]
        [SerializeField]
        protected GameObject _model;
        [SerializeField]
        protected GameObject ThrusterObject;
        [SerializeField]
        protected float ReleaseDelay;
        [Tooltip("If enabled, a small random delay is added between the thruster finishing and the explosion happening. You should only disable this is you have a very specific reason.")]
        [SerializeField]
        protected bool _randomTimeDelayAfterThruster = true;
        [SerializeField]
        protected ParticleSystem _ExplosionEffect;
        [SerializeField]
        protected Rigidbody _rigidbody;
        private Collider[] _colliders;
        [Header("Sound")]
        [GameSound]
        [SerializeField]
        private string _thrustSound;
        private bool Thrusting = false;
        [SerializeField]
        public List<ThrusterAttributes> ThrusterStages;


        protected override void Awake()
        {
            base.Awake();
            if ((UnityEngine.Object)this._model == (UnityEngine.Object)null)
                Debug.LogError((object)"Missing model reference in rocket", (UnityEngine.Object)this);
            if ((UnityEngine.Object)this.ThrusterObject == (UnityEngine.Object)null)
                Debug.LogError((object)"Missing Thruster on rocket - this is not gonna fly!", (UnityEngine.Object)this);
            if ((UnityEngine.Object)this._fuse == (UnityEngine.Object)null)
                Debug.LogError((object)"Missing Fuse on rocket", (UnityEngine.Object)this);
            if ((UnityEngine.Object)this._ExplosionEffect == (UnityEngine.Object)null)
                Debug.LogError((object)"Missing IExplosion on rocket", (UnityEngine.Object)this);
            this._rigidbody = this.GetComponent<Rigidbody>();
            if ((UnityEngine.Object)this._rigidbody == (UnityEngine.Object)null)
                Debug.LogError((object)"Missing Rigidbody on rocket", (UnityEngine.Object)this);
            this._colliders = this._rigidbody.GetComponents<Collider>();

            for (int i = 0; i <= ThrusterStages.Count - 1; i++)
            {
                var T = ThrusterStages[i];
                if (T._effect == null)
                    Debug.LogError((object)"Missing at least one particle system on Thruster", this);
                T._isThrusting = false;
                T.SetEmissionOnParticleSystems(false);
            }
            this._ExplosionEffect.Stop();
        }

        protected override void Start()
        {
            base.Start();
            foreach (ThrusterAttributes T in ThrusterStages)
            {
                T.SetEmissionOnParticleSystems(false);
            }
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (!((UnityEngine.Object)this.GetComponent<Rigidbody>() == (UnityEngine.Object)null))
                return;
            this._rigidbody = this.gameObject.AddComponent<Rigidbody>();
        }

        protected override async UniTask LaunchInternalAsync(CancellationToken token)
        {
            MultiStageRocket rocketBehavior = this;
            StartCoroutine(ThrustController());
            UniTask uniTask = UniTask.Delay(200);
            await uniTask;
            token.ThrowIfCancellationRequested();
            StartCoroutine(ReleaseRocket(rocketBehavior));
            Messenger.Broadcast<MessengerEventPlaySound>(new MessengerEventPlaySound(this._thrustSound, this.ThrusterObject.transform, followTransform: true));
            token.ThrowIfCancellationRequested();
            uniTask = UniTask.WaitWhile(() => Thrusting == true);
            await uniTask;
            Messenger.Broadcast<MessengerEventStopSound>(new MessengerEventStopSound(this._thrustSound, this.ThrusterObject.transform));
            if (rocketBehavior._randomTimeDelayAfterThruster)
            {
                uniTask = UniTask.Delay(Mathf.RoundToInt(UnityEngine.Random.Range(0.0f, 0.1f) * 1000f), cancellationToken: token);
                await uniTask;
                token.ThrowIfCancellationRequested();
            }
            if (CoreSettings.AutoDespawnFireworks)
            {
                rocketBehavior.DisableRigidBodyAndColliders();
                rocketBehavior._model.SetActive(false);
            }
            rocketBehavior._ExplosionEffect.Play();
            // ISSUE: reference to a compiler-generated method
            uniTask = UniTask.WaitWhile(() => this._ExplosionEffect.IsAlive() || this._ExplosionEffect.isPlaying, PlayerLoopTiming.Update, token);
            await uniTask;
            token.ThrowIfCancellationRequested();
            if (!CoreSettings.AutoDespawnFireworks)
                return;
            uniTask = rocketBehavior.DestroyFireworkAsync(token);
            await uniTask;
            token.ThrowIfCancellationRequested();
        }

        private IEnumerator ThrustController()
        {
            Thrusting = true;
            foreach (ThrusterAttributes T in ThrusterStages)
            {
                T.TurnOn();
                yield return new WaitForSeconds(T._thrustTime);
                T.TurnOff();
            }
            Thrusting = false;
        }

        private void FixedUpdate()
        {
            for (int i = 0; i <= ThrusterStages.Count -1; i++) {
                var T = ThrusterStages[i]; 
                if (T._isThrusting)
                {
                    //Debug.Log($"Thrusting with force: {T._thrustForcePerSecond}");
                    T._curveDeltaTime += Time.fixedDeltaTime;
                    if (T._thrustAtPosition)
                    {
                        this._rigidbody.AddForceAtPosition(ThrusterObject.transform.up * T._thrustForcePerSecond * T._thrustEffectCurve.Evaluate(T._curveDeltaTime) * Time.fixedDeltaTime, ThrusterObject.transform.position, T._thrustForceMode);
                    }
                    else
                    {
                        this._rigidbody.AddForce(ThrusterObject.transform.up * T._thrustForcePerSecond * T._thrustEffectCurve.Evaluate(T._curveDeltaTime) * Time.fixedDeltaTime, T._thrustForceMode);
                    }
                }
            }
        }

        private IEnumerator ReleaseRocket(MultiStageRocket rocketbehaviour)
        {
            yield return new WaitForSeconds(ReleaseDelay);
            rocketbehaviour._rigidbody.isKinematic = false;
            rocketbehaviour._rigidbody.useGravity = true;
        }

        protected void DisableRigidBodyAndColliders()
        {
            this._rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            this._rigidbody.isKinematic = true;
            this._rigidbody.useGravity = false;
            foreach (Collider collider in this._colliders)
                collider.enabled = false;
        }

        /*private void FixedUpdate()
        {
            ThrusterAttributes thruster;
            thruster = ThrusterStages[0];
            if (!thruster._isThrusting)
            {
                return;
            }
            thruster._remainingThrustTime -= Time.deltaTime;
            if ((double)thruster._remainingThrustTime <= 0.0)
            {
                thruster.TurnOff();
                ThrusterStages.Remove(thruster);
                return;
            }
            else
            {
                thruster._curveDeltaTime += Time.fixedDeltaTime;
                if (thruster._thrustAtPosition)
                    this._rigidbody.AddForceAtPosition(ThrusterObject.transform.up * thruster._thrustForcePerSecond * thruster._thrustEffectCurve.Evaluate(thruster._curveDeltaTime) * Time.fixedDeltaTime, ThrusterObject.transform.position, thruster._thrustForceMode);
                else
                    this._rigidbody.AddForce(ThrusterObject.transform.up * thruster._thrustForcePerSecond * thruster._thrustEffectCurve.Evaluate(thruster._curveDeltaTime) * Time.fixedDeltaTime, thruster._thrustForceMode);
            }
        }
        */
    }

    [Serializable]
    public class ThrusterAttributes
    {
        [Header("General")]
        [SerializeField]
        public float _thrustForcePerSecond;
        [SerializeField]
        public float _thrustTime;
        [SerializeField]
        public AnimationCurve _thrustEffectCurve;
        [SerializeField]
        public ForceMode _thrustForceMode;
        [SerializeField]
        public ParticleSystem _effect;
        [SerializeField]
        [Tooltip("If false, force will be applied in the up direction of the truster on the entire rigidbody. If true the force will be applied at the specific position")]
        public bool _thrustAtPosition;
        [HideInInspector]
        public bool _isThrusting;
        [HideInInspector]
        public float _curveDeltaTime;


        public void SetEmissionOnParticleSystems(bool enableEmission)
        {
            if (enableEmission)
            {
                this._effect.gameObject.SetActive(true);
                this._effect.Play();
            }
            else
            {
                this._effect.Stop();
            }
        }

        public void TurnOn()
        {
            this._isThrusting = true;
            this.SetEmissionOnParticleSystems(true);
        }

        public void TurnOff()
        {
            if (this._isThrusting)
            {
                this._isThrusting = false;
                this.SetEmissionOnParticleSystems(false);
            }
        }
    }
}

