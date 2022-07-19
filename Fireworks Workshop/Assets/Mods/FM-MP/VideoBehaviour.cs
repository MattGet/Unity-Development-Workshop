using System.Collections;
using UnityEngine;
using Cysharp.Threading.Tasks;
using FireworksMania.Core.Behaviors.Fireworks.Parts;
using FireworksMania.Core.Persistence;
using System.Threading;
using FireworksMania.Core.Behaviors.Fireworks;
using FireworksMania.Core;

public class VideoBehaviour : BaseFireworkBehavior, IHaveFuse, IIgnitable, IHaveFuseConnectionPoint
{

    [Header("Video Settings")]
    public StartVideo VideoPlayer;
    public InputMenu menu;
    private Rigidbody _rigidbody;
    private string localVideoURL;

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

        entitydata.Add<string>("URL", VideoPlayer.VideoURL);
        entitydata.Add<bool>("videoloop", VideoPlayer.player.isLooping);
        entitydata.Add<bool>("audioloop", VideoPlayer.audioSource.isLooping);
        entitydata.Add<float>("videoVol", menu.volnumb);
        entitydata.Add<int>("vidQual", (int)VideoPlayer.videoQuality);
        entitydata.Add<float>("VidTiem", (float)VideoPlayer.player.time);
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

        string temp = customComponentData.Get<string>("URL");
        localVideoURL = temp;

        int temp2 = customComponentData.Get<int>("vidQual");
        menu.Quality.value = temp2;
        VideoPlayer.player.isLooping = customComponentData.Get<bool>("videoloop");
        VideoPlayer.audioSource.isLooping = customComponentData.Get<bool>("audioloop");
        menu.volnumb = customComponentData.Get<float>("videoVol");
        VideoPlayer.Quality(temp2);
        VideoPlayer.OK(temp);

        float time = customComponentData.Get<float>("VidTime");
        if (time != 0f && temp2 != 3)
        {
            VideoPlayer.SetVideoStartTime(time);
        }
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
        if ((UnityEngine.Object)this.VideoPlayer == (UnityEngine.Object)null)
            Debug.LogError((object)"Missing Video Player in VideoBehaviour!");
        //Debug.Log("SID = " + SaveableComponentTypeId);
    }

    protected override void Start()
    {
        base.Start();
        VideoPlayer.gotURL.AddListener(reciveURL);
        // Debug.Log(gameObject + " initial speed = " + LaunchForce);
        //Debug.Log(gameObject + " is at position " + gameObject.transform.position.ToString("F2") + " on spawn\n");
    }

    private void reciveURL(string url)
    {
        localVideoURL = url;
    }

    protected override void OnValidate()
    {
        if (Application.isPlaying)
            return;
        base.OnValidate();
        if (this.gameObject.TryGetComponent<StartVideo>(out VideoPlayer) && this.VideoPlayer == null)
        {
            Debug.Log("FMVP: Found Native Video Player");
        }
        if (!((UnityEngine.Object)this.VideoPlayer == (UnityEngine.Object)null))
            return;
        Debug.LogError((object)("FMVP: Missing Video Player on gameobject '" + this.gameObject.name + "' on component 'VideoBehaviour'!"), (UnityEngine.Object)this.gameObject);


    }

    protected override async UniTask LaunchInternalAsync(CancellationToken token)
    {
        this.VideoPlayer.PlayVideo();
        await UniTask.WaitWhile(() => VideoPlayer.isPlaying == true, PlayerLoopTiming.Update, token);
        token.ThrowIfCancellationRequested();
        if (!CoreSettings.AutoDespawnFireworks)
            return;
        await this.DestroyFireworkAsync(token);
    }

}
