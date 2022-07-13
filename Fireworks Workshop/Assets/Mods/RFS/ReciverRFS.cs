using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;
using FireworksMania.Core.Behaviors.Fireworks.Parts;
using FireworksMania.Core.Persistence;
using System.Threading;
using FireworksMania.Core.Behaviors.Fireworks;
using UnityEngine.UI;
using FireworksMania.Core.Messaging;
using FireworksMania.Core;


public class ReciverRFS : BaseFireworkBehavior, IHaveFuse, IIgnitable, IHaveFuseConnectionPoint
{

    public NumericDisplay channelDis;
    [HideInInspector]
    public int channel = 0;
    [HideInInspector]
    public int min = 0;
    [HideInInspector]
    public int seconds = 0;
    private Rigidbody _rigidbody;

    [HideInInspector]
    public bool fired = false;

    private bool IsActive = false;

    [Header("UI Settings")]
    public bool UseUI = false;
    [Space(10)]
    public GameObject UiController;
    public TMP_InputField InputField;
    public Button Close;

    public void FIRE(int chnl)
    {
        if (chnl == channel)
        {
            //Debug.Log("Recived Fire Signal at Channel: " + chnl);
            this.IgniteInstant();
            StartCoroutine(postfire());
        }
    }

    public IEnumerator postfire()
    {
        yield return new WaitForSecondsRealtime(3);
        this.gameObject.transform.parent.gameObject.BroadcastMessage("FIRED", channel);
        yield return new WaitForSecondsRealtime(1);
        fired = true;
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
    }



    protected override void Awake()
    {
        base.Awake();
        this._rigidbody = this.GetComponent<Rigidbody>();
        if ((UnityEngine.Object)this._rigidbody == (UnityEngine.Object)null)
            Debug.LogError((object)"Missing Rigidbody", (UnityEngine.Object)this);

    }

    protected override void Start()
    {
        base.Start();
        channelDis.updateid.AddListener(updateID);
        if (UseUI) UiController.SetActive(false);
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
        await UniTask.WaitWhile(() => this.fired == false, PlayerLoopTiming.Update, token);
        token.ThrowIfCancellationRequested();
        if (!CoreSettings.AutoDespawnFireworks)
            return;
        await this.DestroyFireworkAsync(token);
    }


    public void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                FIRE(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                FIRE(2);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                FIRE(3);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                FIRE(4);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                FIRE(5);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                FIRE(6);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                FIRE(7);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                FIRE(8);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                FIRE(9);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                FIRE(0);
            }
        }
    }

    #region UI

    public void UpdateUI()
    {
        if (UseUI)
        {
            if (!IsActive)
            {
                Messenger.Broadcast(new MessengerEventChangeUIMode(true, false));
                UiController.SetActive(true);
                Close.onClick.AddListener(UpdateUI);
                InputField.text = channel.ToString("0000");
                IsActive = true;
            }
            else
            {
                Messenger.Broadcast(new MessengerEventChangeUIMode(false, true));
                Close.onClick.RemoveAllListeners();
                int x = 10000;
                if (int.TryParse(InputField.text, out x))
                {
                    if (x <= 10000 && x >= 0)
                    {
                        channelDis.UpdateDisplay(x);
                    }
                    if (x < 0)
                    {
                        channelDis.UpdateDisplay(0);
                    }
                    if (x > 9999)
                    {
                        channelDis.UpdateDisplay(9999);
                    }
                }
                else
                {
                    channelDis.UpdateDisplay(0);
                }
                IsActive = false;
                UiController.SetActive(false);
            }
        }
        
    }


    #endregion

}
