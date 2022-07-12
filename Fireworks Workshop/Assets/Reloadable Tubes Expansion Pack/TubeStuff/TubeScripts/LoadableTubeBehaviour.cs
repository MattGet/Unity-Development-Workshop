using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FireworksMania.Core.Behaviors;
using FireworksMania.Core.Behaviors.Fireworks;
using FireworksMania.Core.Behaviors.Fireworks.Parts;
using FireworksMania.Core.Definitions.EntityDefinitions;
using FireworksMania.Core.Definitions;
using FireworksMania.Core.Messaging;
using CustomTubes;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

    public class LoadableTubeBehaviour : MonoBehaviour
    {
        private float distance;
        private float numberofshells = 0;
        [HideInInspector]
        public GameObject Shell;
        private ParticleSystem Shelleffect;
        private float Id;
        private GameObject go;
        private bool waitonstart = false;
        private bool limithits = false;
        public TubeClass TClass;
        private TubeIgniteComponent tubeignite;

        [HideInInspector]
        public string TubeID = "";

        private GameSoundDefinition shelltubeload;
        private Fuse _fuse;

        public int TubeMatchNumber;
        [SerializeField]
        private CapsuleCollider BlueprintDetector;

        [SerializeField]
        private GameObject Top;
        [SerializeField]
        private GameObject Bottom;

        private Fuse Shellfuse;

        public enum TubeClass
        {
            OneInch = 1,
            ThreeInch = 3,
            SixInch = 6,
            TenInch = 10,
            SixteenInch = 16,
            TwentyFourInch = 24,
        }

        private float radius;
        private float height;
        private float ExplosionForce;
        public GameSoundDefinition LoadSound;
        public GameSoundDefinition DropSound;

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(Top.transform.position, Bottom.transform.position);
        }
#endif

        public void SetGSLoad(GameSoundDefinition shellin)
        {
            shelltubeload = shellin;
        }

        // Start is called before the first frame update
        void Start()
        {
            go = transform.parent.gameObject;
            StartCoroutine(waitonload());
            ValidateClass();
        }

        public void ValidateClass()
        {
            switch (TClass)
            {
                case TubeClass.OneInch:
                    radius = 0.033f;
                    height = 0.3f;
                    ExplosionForce = 50;
                    break;
                case TubeClass.ThreeInch:
                    radius = 0.055f;
                    height = 0.5f;
                    ExplosionForce = 100;
                    break;
                case TubeClass.SixInch:
                    radius = 0.1f;
                    height = 0.85f;
                    ExplosionForce = 200;
                    break;
                case TubeClass.TenInch:
                    radius = 0.16f;
                    height = 0.84f;
                    ExplosionForce = 350;
                    break;
                case TubeClass.SixteenInch:
                    radius = 0.256f;
                    height = 1.2f;
                    ExplosionForce = 500;
                    break;
                case TubeClass.TwentyFourInch:
                    radius = 0.384f;
                    height = 1.8f;
                    ExplosionForce = 1000;
                    break;
            }
        }



#if UNITY_EDITOR
        private void OnValidate()
        {
            ValidateClass();
            if (BlueprintDetector == null)
            {
                CapsuleCollider capsule;
                if (this.gameObject.TryGetComponent<CapsuleCollider>(out capsule))
                {
                    BlueprintDetector = capsule;
                }
            }
            else
            {
                BlueprintDetector.isTrigger = true;
            }
            
        }

#endif
        IEnumerator waitonload()
        {
            this.BlueprintDetector.radius *= 3;
            this.BlueprintDetector.height *= 2;
            yield return new WaitForSeconds(2);
            this.BlueprintDetector.radius /= 3;
            this.BlueprintDetector.height /= 2;
            waitonstart = true;
            if (TubeID == "")
            {
                Id = UnityEngine.Random.Range(0, 10000000);
                TubeID = gameObject.name + Id;
            }
            //Debug.Log("load TubeID = " + TubeID);
            yield return null;
        }

        // Update is called once per frame
        void Update()
        {
            if (Shell == null && numberofshells >= 1)
            {
                removeShell();
            }
            if (Shellfuse != null)
            {
                if (Shellfuse.IsUsed)
                {
                    ExplodeTube();
                    Shellfuse = null;
                }
            }
        }

        public void ExplodeTube()
        {
            //Debug.Log("ExplodeTube Called");
            if (Bottom != null && Top != null)
            {
                Vector3 worldTop = this.transform.parent.TransformPoint(new Vector3(this.transform.localPosition.x, Top.transform.localPosition.y + (height / 4), this.transform.localPosition.z));
                Vector3 worldTop2 = this.transform.parent.TransformPoint(new Vector3(this.transform.localPosition.x, Top.transform.localPosition.y + height, this.transform.localPosition.z));
                Vector3 worldBottom = this.transform.parent.TransformPoint(new Vector3(this.transform.localPosition.x, Bottom.transform.localPosition.y - (height / 4), this.transform.localPosition.z));
                //Debug.Log("Checks Successful");
                //Debug.Log($"WorldTop = {worldTop}");
                //Debug.Log($"WorldTop2 = {worldTop2}");
                //Debug.Log($"WorldBottom = {worldBottom}");
                Collider[] objectsInTube = Physics.OverlapCapsule(worldBottom, worldTop, radius);
                foreach (Collider C in objectsInTube)
                {

                    if (C.gameObject == this.gameObject || C.gameObject == this.gameObject.transform.parent.gameObject || C.gameObject == this.gameObject.transform.parent.gameObject.transform.parent.gameObject)
                    {
                        continue;
                    }
                    if (C.gameObject.transform.IsChildOf(this.gameObject.transform.parent.transform))
                    {
                        continue;
                    }
                    //Debug.Log($"Found Object {C.gameObject.name} inside tube");
                    Rigidbody ObjectBody;
                    Fuse ObjectFuse;
                    if (C.gameObject.TryGetComponent(out ObjectBody))
                    {
                        if (C.gameObject.transform.parent.gameObject.name == "[FireworksManager]" || C.gameObject.tag == "MainCamera")
                        {
                            float force = UnityEngine.Random.Range((ExplosionForce * 0.75f), (ExplosionForce * 1.25f));
                            //Debug.Log($"\tObject {C.gameObject.name} had Rigidbody");
                            ObjectBody.MovePosition(worldTop2);
                            ObjectBody.MoveRotation(UnityEngine.Random.rotation);
                            //Debug.Log($"Obj Pos = {ObjectBody.transform.position}");
                            ObjectBody.AddExplosionForce(force, worldBottom, height * 20, 0, ForceMode.Impulse);
                            ObjectBody.AddForce(new Vector3(UnityEngine.Random.Range(-(force/5), (force / 5)), 0, UnityEngine.Random.Range(-(force / 5), (force / 5))), ForceMode.Impulse);
                            ObjectFuse = ObjectBody.gameObject.GetComponentInChildren<Fuse>();
                            if (ObjectFuse != null)
                            {
                                IgnoreExplosionPhysicsForcesBehavior temp;
                                if (!ObjectBody.gameObject.TryGetComponent(out temp))
                                {
                                    ObjectBody.gameObject.AddComponent<IgnoreExplosionPhysicsForcesBehavior>();
                                }
                                StartCoroutine(RandomeIgnite(ObjectFuse, ObjectBody));
                            }
                        }
                    }
                }
            }
        }

    public IEnumerator RandomeIgnite(Fuse fuse, Rigidbody body)
    {
        float time = Time.realtimeSinceStartup;
        yield return new WaitForSeconds(0.15f);
        float random = Mathf.RoundToInt(UnityEngine.Random.value);
        if (random == 1)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 3f));
            if (fuse != null)
            {
                //Debug.Log($"Ignite Instant Time = {Time.realtimeSinceStartup - time} seconds");
                fuse.Ignite(100000);
                yield break;
            }
        }
        yield return new WaitUntil(() => body.velocity.y <= 1);
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.0f, 4f));
        if (fuse != null)
        {
            //Debug.Log($"Ignite Instant Time = {Time.realtimeSinceStartup - time} seconds");
            fuse.Ignite(100000);
        }
    }

    public void OnTriggerStay(Collider others)
        {
            if (others.material.dynamicFriction != 0.111f) return;
            if (others.gameObject.name == "ignoreTA Potection")
            {
                return;
            }
            /*if (numberofshells >= 1 && others.material.dynamicFriction == 0.111f)
            {
                //Debug.Log("Destroying Shell, Reason: Tube contains " + numberofshells + " shells!");
                Rigidbody shell = others.gameObject.GetComponent<Rigidbody>();
                shell.velocity = new Vector3(UnityEngine.Random.Range(-1.5f, 1.5f), UnityEngine.Random.Range(1, 2), UnityEngine.Random.Range(-1.5f, 1.5f));
                return;
            }*/
            if (others.material.dynamicFriction == 0.111f && waitonstart == true && limithits == false && numberofshells < 1)
            {
                limithits = true;
                if (GetMultiplier(others.gameObject) <= 1.25f && GetMultiplier(others.gameObject) >= 0.5f)
                {
                    //Debug.Log("Number of Shells: " + numberofshells);
                    BaseFireworkBehavior basic = others.gameObject.GetComponent<BaseFireworkBehavior>();
                    BaseEntityDefinition item = basic.EntityDefinition;
                    Destroy(others.gameObject);
                    //Debug.Log("original destroyed");

                    addShell(item.PrefabGameObject);

                }
                else
                {
                    Rigidbody shell = others.gameObject.GetComponent<Rigidbody>();
                    shell.velocity = new Vector3(UnityEngine.Random.Range(-1.5f, 1.5f), UnityEngine.Random.Range(1, 2), UnityEngine.Random.Range(-1.5f, 1.5f));
                }
                StartCoroutine(wait());

            }
            else if (others.material.dynamicFriction == 0.111f && waitonstart == false && limithits == false && numberofshells < 1)
            {
                try
                {
                    //Debug.Log("Blueprint: " + others.gameObject.name + " = " + TubeID);
                    if (others.gameObject.name == TubeID)
                    {
                        limithits = true;
                        this.addCurrentShell(others.gameObject);
                        StartCoroutine(wait());
                    }
                }
                catch
                {
                    if (TubeID == null)
                    {
                        Debug.LogError("Tube ID was NULL during Blueprint Load!" + this.gameObject.transform.parent.transform.parent.name);
                        return;
                    }
                }


            }
        }

        public void SetDistance(GameObject other)
        {
            float height2 = getheight(other.gameObject);
            float thisheight = Top.transform.localPosition.y - Bottom.transform.localPosition.y;

            //Debug.Log("tubeheight = " + thisheight + "\nshellheight = " + height);
            if (numberofshells >= 1f)
            {
                distance = (thisheight - ((height2 / 2) * numberofshells));
            }
            else
            {
                distance = (thisheight - ((height2 / 2) * 1.25f));
            }
        }


        IEnumerator animateshell(float speed, bool withsoud, bool special)
        {
            if (special)
            {
                yield return new WaitForSecondsRealtime(2.2f);
            }
            yield return new WaitForFixedUpdate();
            SetDistance(Shell);

            
            
            float multiplier = GetMultiplier(Shell);
            //Debug.Log("test1");
            Shell.transform.localRotation = Top.transform.localRotation;
            Shell.transform.localPosition = new Vector3(this.transform.localPosition.x, Top.transform.localPosition.y, this.transform.localPosition.z);
            //Shell.transform.localPosition = Top.transform.localPosition;
            if (LoadSound != null && withsoud == true)
            {
                Shell.SendMessage("GetTubeSound", this.gameObject);
                //Debug.Log("test4");
                if (DropSound != null && multiplier <= 0.5f)
                {
                    Messenger.Broadcast(new MessengerEventPlaySound(DropSound.name, Shell.transform, true, true));
                }
                else if (shelltubeload != null)
                {
                    //Debug.Log("test5");
                    Messenger.Broadcast(new MessengerEventPlaySound(shelltubeload.name, Shell.transform, true, true));
                }
                else
                {
                    //Debug.Log("test6");
                    Messenger.Broadcast(new MessengerEventPlaySound(LoadSound.name, Shell.transform, true, true));
                }
            }
            Vector3 current = Shell.transform.localPosition;
            Vector3 target = new Vector3(this.transform.localPosition.x, Bottom.transform.localPosition.y, this.transform.localPosition.z);
            float increment = distance / (speed * multiplier);
            //Debug.Log("test7");
            //Debug.Log("current " + current + " target " + target + " increment " + increment + " speed " + speed);
            for (int i = 1; i <= (speed * multiplier); i++)
            {
                current = Vector3.MoveTowards(current, target, increment);
                //Debug.Log("test8");
                Shell.transform.localPosition = current;
                yield return new WaitForFixedUpdate();
            }
            //Debug.Log("test9");
            //Debug.Log("Tube positioned = " + TubeID);
            Shell.BroadcastMessage("entermortar", gameObject, SendMessageOptions.RequireReceiver);
            Shell.BroadcastMessage("SetTubeID", TubeID, SendMessageOptions.RequireReceiver);
            //Debug.Log("Test 10");
            yield return new WaitForFixedUpdate();
            yield return new WaitForSecondsRealtime(0.1f);
        try
        {
            Shellfuse = Shell.GetComponentInChildren<Fuse>();
            //Debug.Log("Shellfuse = " + Shellfuse);
            if (Shellfuse != null)
            {
                _fuse = Shellfuse;
                tubeignite = gameObject.AddComponent<TubeIgniteComponent>();
                tubeignite._fuse = Shellfuse;
            }
            else Debug.Log("Failed to find Fuse on " + Shell.name + " tube ignition will not work!");
        }
        catch 
        {
            //Debug.LogException(e);
            Debug.Log($"Failed to add ignite component to {this.name} for {Shell.name}, This may be due to mismatched game/mod versions!");
        }



        if (multiplier <= 0.75f || multiplier >= 1.05f)
        {
            foreach (Transform T in Shell.transform)
            {
                ParticleSystem P;
                if (T.gameObject.TryGetComponent<ParticleSystem>(out P))
                {
                    Shelleffect = P;

                    var main = P.main;
                    // Debug.Log("Modifying speed on " + T.gameObject.name + " start speed = " + main.startSpeed + "/" + main.startSpeed.constant);
                    if (main.startSpeed.mode == ParticleSystemCurveMode.Constant || main.startSpeed.mode == ParticleSystemCurveMode.TwoConstants)
                    {
                        main.startSpeed = main.startSpeed.constant * multiplier;
                        //Debug.Log("New Speed = " + main.startSpeed + "/" + main.startSpeed.constant);
                    }
                    else
                    {
                        Debug.Log("Shell start speed was not a valid mode, must be either constant or two constants");
                    }
                    //Debug.Log("Sucsessfully modified Shell's Speed");
                    break;
                }
            }
        }
        try
        {
            ShellTubeMatchData data = Shell.AddComponent<ShellTubeMatchData>();
            data.shellData.ShellMatchNumber = TubeMatchNumber;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            Debug.Log($"Failed to add Shell Data on {Shell.name}");
        }
    }
        

        public void removeShell()
        {
            if (numberofshells >= 1)
            {
                numberofshells = 0;
            }
            Shell = null;
        }

        public void addShell(GameObject other)
        {
            //Debug.Log("adding new shell");
            GameObject shell = UnityEngine.Object.Instantiate<GameObject>(other, gameObject.transform.parent.transform);
            shell.name = shell.name + " Tube Spawned";
            Rigidbody body = shell.GetComponent<Rigidbody>();
            Destroy(body);

            Shell = shell;


            StartCoroutine(animateshell(120, true, false));
            numberofshells = 1;
        }
        public void addCurrentShell(GameObject shell)
        {
            bool special = false;
            if (shell.name == "ignoreTA Potection")
            {
                special = true;
            }
            //Debug.Log("adding current shell");
            shell.transform.parent = gameObject.transform.parent.transform;
            Rigidbody body = shell.GetComponent<Rigidbody>();
            Destroy(body);
            Shell = shell;

            StartCoroutine(animateshell(60, false, special));
            numberofshells = 1;
        }

        public void addCopiedShell(CustomTubes.ShellTubeMatchData shellmatch)
        {
            //Debug.Log("loading Copied shell");
            if (shellmatch.shellData.ShellMatchNumber != TubeMatchNumber)
            {
                //Debug.Log("CopiedShell Shell Could not be loaded Shell ID = " + shellmatch.shellData.ShellMatchNumber.ToString() + " Tube ID = " + TubeMatchNumber);
                return;
            }


            GameObject shell = UnityEngine.Object.Instantiate<GameObject>(shellmatch.shellData.Shell, gameObject.transform.parent.transform);
            //Debug.Log("loading Copied shell 2");
            Rigidbody body = shell.GetComponent<Rigidbody>();
            Destroy(body);
            Shell = shell;
            numberofshells = 1;
            //Debug.Log("loading Copied shell 3");

            if (TubeID == "")
            {
                Id = UnityEngine.Random.Range(0, 10000000);
                TubeID = gameObject.name + Id;
            }
            StartCoroutine(animateshell(30, false, false));
        }

        IEnumerator wait()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(0.1f);
            limithits = false;
        }
        public float getheight(GameObject other)
        {
            CapsuleCollider[] objsize;
            float height = 0f;
            objsize = other.GetComponentsInChildren<CapsuleCollider>();
            //var rotation = other.transform.rotation;
            //other.transform.rotation = new Quaternion(0, 0, 0, 1);
            foreach (CapsuleCollider obj in objsize)
            {
                if (obj.gameObject.activeInHierarchy && !obj.isTrigger)
                {
                    if (obj.height > height)
                    {
                        height = obj.height;
                        //Debug.Log(" object for height = " + obj.gameObject);
                    }
                }
            }
            //other.transform.rotation = rotation;

            SphereCollider[] ballsize;
            ballsize = other.GetComponentsInChildren<SphereCollider>();
            foreach (SphereCollider obj2 in ballsize)
            {
                if (obj2.gameObject.activeInHierarchy && !obj2.isTrigger)
                {
                    if (obj2.radius > height)
                    {
                        height = obj2.radius;
                        //Debug.Log(" object for height = " + obj2.gameObject);
                    }
                }
            }
            return height;
        }

        public float GetMultiplier(GameObject other)
        {
            //Debug.Log("Getting Multiplier");
            CapsuleCollider[] objsize;
            float width = 0f;
            float mult;
            objsize = other.GetComponentsInChildren<CapsuleCollider>();
            //Debug.Log("Got A Component");
            //var rotation = other.transform.rotation;
            //other.transform.rotation = new Quaternion(0, 0, 0, 1);
            if (objsize != null)
            {
                foreach (CapsuleCollider obj in objsize)
                {
                    if (obj.gameObject.activeInHierarchy && !obj.isTrigger)
                    {
                        if (obj.radius > width)
                        {
                            width = obj.radius;
                           // Debug.Log(" object for width = " + obj.gameObject);
                        }
                    }
                }
            }
            
            //other.transform.rotation = rotation;

            SphereCollider[] ballsize;
            ballsize = other.GetComponentsInChildren<SphereCollider>();
            if (ballsize != null)
            {
                foreach (SphereCollider obj2 in ballsize)
                {
                    if (obj2.gameObject.activeInHierarchy && !obj2.isTrigger)
                    {
                        if (obj2.radius > width)
                        {
                            width = obj2.radius;
                            //Debug.Log(" object for width = " + obj2.gameObject);
                        }
                    }
                }
            }
            
            //Debug.Log("Shell Width = " + width + " tube size = " + radius);
            mult = width / radius;
            if (Shell != null)
            {
                //Debug.Log(Shell.name + " Shell Multiplier = " + mult);
            }
            return mult;
        }

    }