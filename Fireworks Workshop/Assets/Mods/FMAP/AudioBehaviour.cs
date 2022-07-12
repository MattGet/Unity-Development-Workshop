using System.Collections;
using UnityEngine;
using Cysharp.Threading.Tasks;
using FireworksMania.Core.Behaviors.Fireworks.Parts;
using FireworksMania.Core.Persistence;
using System.Threading;
using FireworksMania.Core.Behaviors.Fireworks;
using FireworksMania.Core;

public class AudioBehaviour : BaseFireworkBehavior, IHaveFuse, IIgnitable, IHaveFuseConnectionPoint
{

    [Header("Audio Settings")]
    public FmAudioPlayer AudioPlayer;
    public AudioInputMenu menu;
    private Rigidbody _rigidbody;
    private string localAudioURL;

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

        entitydata.Add<string>("URL", AudioPlayer.AudioURL);
        entitydata.Add<bool>("audioloop", AudioPlayer.Player.loop);
        entitydata.Add<float>("audioVol", menu.volnumb);
        entitydata.Add<int>("format", (int)AudioPlayer.Atype);
        entitydata.Add<bool>("use3D", menu.use3D);
        entitydata.Add<float>("Spatial", AudioPlayer.Player.spatialBlend);
        entitydata.Add<float>("Minvalue", AudioPlayer.Player.minDistance);
        entitydata.Add<float>("Maxvalue", AudioPlayer.Player.maxDistance);
        entitydata.Add<float>("Time", AudioPlayer.Player.time);
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

        string temp = customComponentData.Get<string>("URL");
        localAudioURL = temp;

        int temp2 = customComponentData.Get<int>("vidQual");
        AudioPlayer.Player.loop = customComponentData.Get<bool>("audioloop");
        menu.volnumb = customComponentData.Get<float>("audioVol");
        menu.use3D = customComponentData.Get<bool>("use3D");
        AudioPlayer.Player.spatialBlend = customComponentData.Get<float>("Spatial");
        AudioPlayer.Player.minDistance = customComponentData.Get<float>("Minvalue");
        AudioPlayer.Player.maxDistance = customComponentData.Get<float>("Maxvalue");
        AudioPlayer.Player.time = customComponentData.Get<float>("Time");
        AudioPlayer.Format(temp2);
        AudioPlayer.OK(temp);
    }


    // Update is called once per frame
    void Update()
    {

    }



    protected override void Awake()
    {
        base.Awake();
        this._rigidbody = this.GetComponent<Rigidbody>();
        if ((UnityEngine.Object)this._rigidbody == (UnityEngine.Object)null)
            Debug.LogError((object)"Missing Rigidbody", (UnityEngine.Object)this);
        if ((UnityEngine.Object)this.AudioPlayer == (UnityEngine.Object)null)
            Debug.LogError((object)"Missing Audio Player in AudioBehaviour!");
        //Debug.Log("SID = " + SaveableComponentTypeId);
    }

    protected override void Start()
    {
        base.Start();
        AudioPlayer.gotURL.AddListener(reciveURL);
    }

    private void reciveURL(string url)
    {
        localAudioURL = url;
    }

    protected override void OnValidate()
    {
        if (Application.isPlaying)
            return;
        base.OnValidate();
        if (this.gameObject.TryGetComponent<FmAudioPlayer>(out AudioPlayer) && this.AudioPlayer == null)
        {
            Debug.Log("FMAP: Found Native Audio Player");
        }
        if (!((UnityEngine.Object)this.AudioPlayer == (UnityEngine.Object)null))
            return;
        Debug.LogError((object)("FMAP: Missing Audio Player on gameobject '" + this.gameObject.name + "' on component 'AudioBehaviour'!"), (UnityEngine.Object)this.gameObject);


    }

    protected override async UniTask LaunchInternalAsync(CancellationToken token)
    {
        this.AudioPlayer.PlayAudio();
        await UniTask.WaitWhile(() => AudioPlayer.isPlaying == true, PlayerLoopTiming.Update, token);
        token.ThrowIfCancellationRequested();
        if (!CoreSettings.AutoDespawnFireworks)
            return;
        await this.DestroyFireworkAsync(token);
    }

}
