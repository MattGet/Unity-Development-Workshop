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

public class RemoteDetonator : BaseFireworkBehavior, IHaveFuse, IIgnitable, IHaveFuseConnectionPoint
{

    public NumericDisplay channelDis;
    public NumericDisplay minDis;
    public NumericDisplay secDis;
    [HideInInspector]
    public int channel = 0;
    [HideInInspector]
    public int min = 0;
    [HideInInspector]
    public int seconds = 0;
    private Rigidbody _rigidbody;

    [HideInInspector]
    public bool fired = false;


    public void FireButton()
    {
        if (channel >= 0 && channel <= 99)
        {
            if (minDis != null && secDis != null)
            {
                min = minDis.Number;
                seconds = secDis.Number;
            }

            if (seconds == 0 && min == 0)
            {
                //Debug.Log("countdown over, firing channel = " + channel);
                this.transform.parent.gameObject.BroadcastMessage("FIRE", channel);
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
        int i = min;
        min = minDis.Number;
        seconds = secDis.Number;
        while (i >= 0)
        {
            //Debug.Log("minutes: " + i);
            for (int t = seconds; t >= 0; t--)
            {
                yield return new WaitForSeconds(1);
                //Debug.Log("seconds: " + t);
                if (i == 0 && t == 0)
                {
                    //Debug.Log("countdown over, firing");
                    minDis.UpdateDisplay(i);
                    secDis.UpdateDisplay(t);
                    this.transform.parent.gameObject.BroadcastMessage("FIRE", channel);
                    this.IgniteInstant();
                    yield break;
                }
                if (t == 0)
                {
                    //Debug.Log("seconds = 0");
                    if (i > 0)
                    {
                        //Debug.Log("seconds = 0 swaping to sec");
                        i = i - 1;
                        minDis.UpdateDisplay(i);
                        t = 59;
                    }
                    secDis.UpdateDisplay(t);
                }
                else
                {
                    //Debug.Log("seconds counting down");
                    if (t > 0)
                    {
                        //Debug.Log("subtracting seconds");
                        secDis.UpdateDisplay(t);
                    }
                }
                //Debug.Log("end of sec loop: " + t);
            }
            //Debug.Log("end of min loop: " + i);
        }
    }

    public void FIRED(int chnl)
    {
        if (chnl == channel)
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

        channel = channelDis.Number;

        entitydata.Add<int>("Channel", channel);

        if (minDis != null && secDis != null)
        {
            min = minDis.Number;
            seconds = secDis.Number;
            entitydata.Add<int>("Mins", min);
            entitydata.Add<int>("Seconds", seconds);
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

        channel = customComponentData.Get<int>("Channel");
        channelDis.UpdateDisplay(channel);

        if (minDis != null && secDis != null)
        {
            min = customComponentData.Get<int>("Mins");
            minDis.UpdateDisplay(min);

            seconds = customComponentData.Get<int>("Seconds");
            secDis.UpdateDisplay(seconds);
        }
    }

    protected override void Awake()
    {
        base.Awake();
        this._rigidbody = this.GetComponent<Rigidbody>();
        if ((UnityEngine.Object)this._rigidbody == (UnityEngine.Object)null)
            Debug.LogError((object)"Missing Rigidbody", (UnityEngine.Object)this);
        //Debug.Log("SID = " + SaveableComponentTypeId);
    }

    protected override void Start()
    {
        base.Start();
        channelDis.updateid.AddListener(updateID);
    }

    public void updateID(int id)
    {
        channel = id;
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
        await UniTask.WaitWhile(() =>  this.fired == false, PlayerLoopTiming.Update, token);
        token.ThrowIfCancellationRequested();
        if (!CoreSettings.AutoDespawnFireworks)
            return;
        await this.DestroyFireworkAsync(token);
    }


}
