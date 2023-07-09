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
using Unity.Netcode.Components;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// The behaviour that controls the logic of each individual tube that shells are put into.
/// </summary>
public class LoadableTubeBehaviour : MonoBehaviour
{
    private float distance;
    private float numberofshells = 0;
    [HideInInspector]
    public GameObject Shell;
    private ParticleSystem Shelleffect;
    private float Id;
    private bool waitonstart = false;
    private bool limithits = false;
    public TubeClass TClass;
    private TubeIgniteComponent tubeignite;

    /// <summary>
    /// A unique ID for the Tube used to match the tube with a shell after a blueprint load
    /// </summary>
    [HideInInspector]
    public string TubeID = "";

    private GameSoundDefinition shelltubeload;
    private Fuse _fuse;

    /// <summary>
    /// Represents the tube's position in an array of tubes, this is required for multi-tube mortar objects and is set automatically
    /// </summary>
    public int TubeMatchNumber;

    /// <summary>
    /// The trigger inside the "tube" part of the tube, this is what is used to detect a shell loaded inside the tube during a blueprint load.
    /// </summary>
    [SerializeField]
    private CapsuleCollider BlueprintDetector;

    /// <summary>
    /// The trigger above the top of the tube that detects when a shell is entering the tube.
    /// </summary>
    [SerializeField]
    private SphereCollider ShellDetector;

    /// <summary>
    /// An arbitrary gameobject used to represet the "Top" of the tube in 3d space, its transform location is required for some logic
    /// </summary>
    [SerializeField]
    private GameObject Top;

    /// <summary>
    /// An arbitrary gameobject used to represet the "Bottom" of the tube in 3d space, its transform location is required for some logic
    /// </summary>
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

    /// <summary>
    /// Allows the Shell to set a custom tube load sound via Messages
    /// </summary>
    /// <param name="shellin"></param>
    public void SetGSLoad(GameSoundDefinition shellin)
    {
        shelltubeload = shellin;
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(waitonload());
        ValidateClass();
    }

    /// <summary>
    /// Sets the radius, height and explosion force of the tube based on the caliber size, in a public build these values should be changed
    /// to simply be public fields the user/modder can edit to allow for greater extensibility.
    /// </summary>
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
    /// <summary>
    /// Automatically fetches the Collider components in the editor
    /// </summary>
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

        if (ShellDetector == null)
        {
            SphereCollider capsule;
            if (this.gameObject.TryGetComponent<SphereCollider>(out capsule))
            {
                ShellDetector = capsule;
            }
        }
        else
        {
            ShellDetector.isTrigger = true;
        }

    }

#endif

    /// <summary>
    /// When the tube is initialized ths size of the triggers is expanded to enusre that all shells are picked up when loaded as part of a blueprint
    /// </summary>
    /// <returns></returns>
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

        // If the shellfuse exists and is used, explode the tube 
        if (Shellfuse != null)
        {
            if (Shellfuse.IsUsed)
            {
                ExplodeTube();
                Shellfuse = null;
            }
        }
    }

    /// <summary>
    /// This function will apply a large upwards force to all gameobjects inside the tube upon a shell being lit.
    /// </summary>
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
                // ensure that none of the components of the tube/mortar itself are shot upwards
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
                    // Check to make sure the object being shot up is either a firework or a player
                    if (C.gameObject.transform.parent.gameObject.name == "[FireworksManager]" || C.gameObject.tag == "MainCamera")
                    {
                        float force = UnityEngine.Random.Range((ExplosionForce * 0.75f), (ExplosionForce * 1.25f));
                        //Debug.Log($"\tObject {C.gameObject.name} had Rigidbody");
                        ObjectBody.MovePosition(worldTop2);
                        ObjectBody.MoveRotation(UnityEngine.Random.rotation);
                        //Debug.Log($"Obj Pos = {ObjectBody.transform.position}");
                        ObjectBody.AddExplosionForce(force, worldBottom, height * 20, 0, ForceMode.Impulse);
                        ObjectBody.AddForce(new Vector3(UnityEngine.Random.Range(-(force / 5), (force / 5)), 0, UnityEngine.Random.Range(-(force / 5), (force / 5))), ForceMode.Impulse);
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

    /// <summary>
    /// Will ignite the supplied firework after a random amount of time or after it has reached its peak height (aka has a near 0 velocity)
    /// </summary>
    /// <param name="fuse">The fuse to ignite</param>
    /// <param name="body">The Ridgidbody to track the velocity of</param>
    /// <returns></returns>
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

    /// <summary>
    /// Base unity function that is used to detect when a shell has "entered" the tube
    /// </summary>
    /// <param name="others">The collider of the shell entering the tube</param>
    public void OnTriggerStay(Collider others)
    {
        // Currently all shell colliders have a dynamic friction value of 0.111, it would be better to simply identify if the object is a shell
        // by attempting to find the ShellBehaviour class on the collider gameobject. The benefit of this method is that it doesnt create a dependency
        // between this class and the shell class.
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

        // If the object is a shell & the tube did not just spawn & the rate limit is not active & the number of shells is < 1
        if (others.material.dynamicFriction == 0.111f && waitonstart == true && limithits == false && numberofshells < 1)
        {
            limithits = true; // set rate limit flag

            // Check to see if the shell will "fit" inside this caliber of tube
            if (GetMultiplier(others.gameObject) <= 1.25f && GetMultiplier(others.gameObject) >= 0.5f)
            {
                //Debug.Log("Number of Shells: " + numberofshells);
                BaseFireworkBehavior basic = others.gameObject.GetComponent<BaseFireworkBehavior>();
                BaseEntityDefinition item = basic.EntityDefinition; // fetch the shell base entity def

                // destroy the shell that is entering the tube
                Destroy(others.gameObject);
                //Debug.Log("original destroyed");

                // send its prefab game object info to the addshell method
                addShell(item.PrefabGameObject);
            }
            else // Reject the shell from the tube by "bouncing" it out of the tube
            {
                Rigidbody shell = others.gameObject.GetComponent<Rigidbody>();
                shell.velocity = new Vector3(UnityEngine.Random.Range(-1.5f, 1.5f), UnityEngine.Random.Range(1, 2), UnityEngine.Random.Range(-1.5f, 1.5f));
            }

            // wait a bit before removing the rate limit flag
            StartCoroutine(wait());

        }

        // if the object is a shell & the tube just spawned and is expecting a blueprint to be loaded & the rate limit is not active
        // & the number of shells is less than 1
        else if (others.material.dynamicFriction == 0.111f && waitonstart == false && limithits == false && numberofshells < 1)
        {
            try // match the shell Unique ID with the Tube Unique ID
            {
                //Debug.Log("Blueprint: " + others.gameObject.name + " = " + TubeID);
                if (others.gameObject.name == TubeID)
                {
                    limithits = true;
                    this.addCurrentShell(others.gameObject);
                    StartCoroutine(wait());
                }
            }
            catch // If the TubeID was not loaded with the blueprint log an error
            {
                if (TubeID == null)
                {
                    Debug.LogError("Tube ID was NULL during Blueprint Load!" + this.gameObject.transform.parent.transform.parent.name);
                    return;
                }
            }


        }
    }

    // Sets the distance the shell needs to travel "down the tube" to properly be placed into the tube during the animation.
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

    /// <summary>
    /// Animates the shell going into the tube and applies other effects to the newly spawned shell
    /// </summary>
    /// <param name="speed">The speed of the animation</param>
    /// <param name="withsoud">If true will play the tubeLoad game sound when animating</param>
    /// <param name="special">If true will add a delay before the animation starts, this is used when loading a blueprint</param>
    /// <returns></returns>
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
        Shell.BroadcastMessage("entermortar", gameObject, SendMessageOptions.RequireReceiver); // Tells the shell it has entered a mortar
        Shell.BroadcastMessage("SetTubeID", TubeID, SendMessageOptions.RequireReceiver); // Sets the shells Unique ID to match the Unique ID of this tube
        //Debug.Log("Test 10");
        yield return new WaitForFixedUpdate();
        yield return new WaitForSecondsRealtime(0.1f);

        try
        {
            // Gets the fuse of the shell and sets it to the Shellfuse var
            Shellfuse = Shell.GetComponentInChildren<Fuse>();
            //Debug.Log("Shellfuse = " + Shellfuse);
            if (Shellfuse != null)
            {
                // Add the tubeIgnite component to this tube, this component allows the tube to be "ignited" as if it were a firework, and is 
                // removed when the shell is fired off.
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

        // Increases the start speed of the shell's particle system, this is how shells will launch up into the air only when placed in tubes.
        // This value is consistent as the shell behaviour class lowers the value when the shell is spawned.
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
            // adds the custom blueprint data to the shell, this is where the Unique ID is stored, preferably this would just be done in the shell
            // Behaviour class. This current implementation was done prevent the need for this class to be dependent on ShellBehaviour.
            ShellTubeMatchData data = Shell.AddComponent<ShellTubeMatchData>();
            data.shellData.ShellMatchNumber = TubeMatchNumber;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            Debug.Log($"Failed to add Shell Data on {Shell.name}");
        }
    }

    // Clears out the Shell var
    public void removeShell()
    {
        if (numberofshells >= 1)
        {
            numberofshells = 0;
        }
        Shell = null;
    }

    /// <summary>
    /// Adds a "new" shell to the tube placed or spawned by the player
    /// </summary>
    /// <param name="other">The GameObject of the shell to add</param>
    public void addShell(GameObject other)
    {
        if (Shell != null) return; // Don't add a shell if one already exists
        //Debug.Log("adding new shell");
        GameObject shell = UnityEngine.Object.Instantiate<GameObject>(other, gameObject.transform.parent.transform);
        shell.name = shell.name + " Tube Spawned";

        // enusre shells in the tube do not have a ridgidbody
        NetworkRigidbody nBody = shell.GetComponent<NetworkRigidbody>();
        Rigidbody body = shell.GetComponent<Rigidbody>();
        Destroy(nBody);
        Destroy(body);

        Shell = shell;

        // send shell to be animated and update shell count
        StartCoroutine(animateshell(120, true, false));
        numberofshells = 1;
    }

    /// <summary>
    /// Adds a "current" shell (i.e. one from a bleuprint load) to the tube
    /// </summary>
    /// <param name="shell">The shell GameObject to add</param>
    public void addCurrentShell(GameObject shell)
    {
        if (Shell != null) return;
        bool special = false;
        if (shell.name == "ignoreTA Potection")
        {
            special = true;
        }
        //Debug.Log("adding current shell");
        shell.transform.parent = gameObject.transform.parent.transform;
        NetworkRigidbody nBody = shell.GetComponent<NetworkRigidbody>();
        Rigidbody body = shell.GetComponent<Rigidbody>();
        Destroy(nBody);
        Destroy(body);
        Shell = shell;

        StartCoroutine(animateshell(60, false, special));
        numberofshells = 1;
    }

    [Obsolete("addCopiedShell is no longer used, use addShell or addCurrentShell instead!")]
    public void addCopiedShell(CustomTubes.ShellTubeMatchData shellmatch)
    {
        if (Shell != null) return;
        //Debug.Log("loading Copied shell");
        if (shellmatch.shellData.ShellMatchNumber != TubeMatchNumber)
        {
            //Debug.Log("CopiedShell Shell Could not be loaded Shell ID = " + shellmatch.shellData.ShellMatchNumber.ToString() + " Tube ID = " + TubeMatchNumber);
            return;
        }


        GameObject shell = UnityEngine.Object.Instantiate<GameObject>(shellmatch.shellData.Shell, gameObject.transform.parent.transform);
        //Debug.Log("loading Copied shell 2");
        NetworkRigidbody nBody = shell.GetComponent<NetworkRigidbody>();
        Rigidbody body = shell.GetComponent<Rigidbody>();
        Destroy(nBody);
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

    /// <summary>
    /// Simple wait method to allow for the rate limit flag to be effective.
    /// </summary>
    /// <returns></returns>
    IEnumerator wait()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.1f);
        limithits = false;
    }

    /// <summary>
    /// Gets the height of the provided gameObjec based on either its capsuleCollider or shpereCollider
    /// </summary>
    /// <param name="other">Object to get the height of</param>
    /// <returns>The height of the largest capsuleCollider or sphereCollider in the gameObject</returns>
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

    /// <summary>
    /// Gets the "Multiplier" value which represents the % difference between the radius of the shell provided and the radius of the tube
    /// </summary>
    /// <param name="other">The shell to measure the radius of</param>
    /// <returns>The % difference between the radius of the shell provided and the radius of the tube</returns>
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