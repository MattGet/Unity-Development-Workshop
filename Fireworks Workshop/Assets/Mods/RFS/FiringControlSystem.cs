using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;
using FireworksMania.Core.Behaviors.Fireworks.Parts;
using FireworksMania.Core.Persistence;
using System.Threading;
using FireworksMania.Core.Behaviors.Fireworks;
using UnityEngine.UI;
using FireworksMania.Core.Messaging;
using FireworksMania.Core.Definitions;
using FireworksMania.Core;

namespace RemoteFiringSystem {

    public class FiringControlSystem : BaseFireworkBehavior, IHaveFuse, IIgnitable, IHaveFuseConnectionPoint
    {
        [Space(10)]
        [Header("Firing System Settings")]
        public GameObject UiController;
        public GameObject TilePrefab;
        public GameObject TileParent;
        public TMP_Text ComputerDisplay;
        public GameObject ConfirmScreen;
        public GameSoundDefinition clickSound;

        [Space(10)]
        [Header("Firing System Channels")]
        public List<GameObject> Channels = new List<GameObject>();

        [Space(10)]
        [Header("Firing System Buttons")]
        public Button AddChannel;
        public Button RemoveChannel;
        public Button FSClose;
        public Button DestroyToggle;
        public Button RemoteToggle;
        public Button Populate;
        public Button ConfYES;
        public Button ConfNO;

        [Header("Toggle Sprites")]
        public Sprite FSToggleOn;
        public Sprite FSToggleOff;

        [Space(10)]
        [Header("Showmaker Stuff")]
        public GameObject ToolBar;
        public GameObject TopLevelUi;
        public TMP_InputField StartChannel;
        public TMP_Text AudioChnnls;
        public TMP_Text ShowTime;
        public Button ShowMakerToggle;

        private bool ToolActive = false;
        private bool ShowmakerActive = false;
        private int startChannel = 0;
        private float startTime = 0;

        private AudioSource[] FAudioPlayers;
        private Rigidbody _rigidbody;

        private bool IsActive = false;
        private bool fired = false;
        private bool IsFiring = false;
        [SerializeField]
        private bool destroy = true;
        private List<int> ids = new List<int>();
        private bool confirm = false;
        private bool remote = true;


        public void FireButton(bool withSound)
        {
            if (IsFiring) return;

            IsFiring = true;
            this.IgniteInstant();
            if (withSound) PlayButtonClick();
            ComputerDisplay.text = "Started Launch Sequence";

            int input = 0;
            if (ShowmakerActive)
            {
                input = startChannel;
                SetAudioTimes();
                StartCoroutine(Timer());
                if (ToolActive) ToggleTool();
            }

            for (int i = input; i <= Channels.Count - 1; i++)
            {
                List<float> data = new List<float>();
                foreach (Transform T in Channels[i].transform)
                {
                    TMP_InputField field;
                    if (T.gameObject.TryGetComponent<TMP_InputField>(out field))
                    {
                        if (field != null)
                        {
                            float x = 10000f;
                            if (float.TryParse(field.text, out x)) { data.Add(x); }
                        }
                    }
                }

                if (data.Count == 2)
                {
                    //Debug.Log("Caught Channel: " + data[0] + " Caught Time: " + data[1]);
                    if (data[0] <= 9999f)
                    {
                        StartCoroutine(ChannelCount(Mathf.RoundToInt(data[0]), data[1]));
                    }
                }
            }
        }

        public IEnumerator Timer()
        {
            float CurrTime = startTime;
            float TotalTime = FAudioPlayers[0].clip.length + 30;
            do
            {
                ShowTime.text = CurrTime.ToString("0000");
                yield return new WaitForSeconds(1);
                CurrTime = CurrTime + 1;
            } while (CurrTime < TotalTime);
        }

        public IEnumerator ChannelCount(int channel, float time)
        {
            ids.Add(channel);
            if (ShowmakerActive)
            {
                time = time - startTime;
            }
            Debug.Log("Firing Channel: " + channel + " With time delay: " + time);
            yield return new WaitForSeconds(time);
            this.transform.parent.gameObject.BroadcastMessage("FIRE", channel);
            string text = "Firing Channel     " + channel.ToString();
            ComputerDisplay.text = text;
        }

        public void FIRED(int chnl)
        {
            StartCoroutine(RemoveId(chnl));
            //Debug.Log("FIRED = " + chnl);

        }

        private IEnumerator RemoveId(int chnl)
        {
            List<int> temp = ids.ToList();
            foreach (int id in temp)
            {
                //Debug.Log("ID = " + id);
                if (id == chnl)
                {
                    ids.Remove(id);
                    //Debug.Log("id = " + id + " chnl = " + chnl + " temp count = " + temp.Count + " ids count = " + ids.Count);
                    if (ids.Count == 0)
                    {
                        StartCoroutine(DestroyCount());
                    }
                }
            }
            yield break;
        }


        private IEnumerator DestroyCount()
        {
            yield return new WaitForSeconds(3);
            ComputerDisplay.text = "Finished Launch Sequence";
            yield return new WaitForSeconds(8);
            if (destroy)
            {
                ComputerDisplay.text = "Destroying FCS";
            }
            yield return new WaitForSeconds(3);
            fired = true;

        }

        // Update is called once per frame
        void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.K))
            {
                if (IsActive)
                {
                    UIControl();
                }
                else
                {
                    UIControl();
                }
            }
#endif
            if (remote && !IsFiring)
            {
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    if (Input.GetKeyDown(KeyCode.I))
                    {
                        FireButton(true);
                    }
                }
            }
            if (ShowmakerActive)
            {
                if (Input.GetKeyDown(KeyCode.T))
                {
                    ToggleTool();
                }
            }
        }


        public void UIControl()
        {
            if (!IsActive)
            {
                Messenger.Broadcast(new MessengerEventChangeUIMode(true, false));
                UiController.SetActive(true);
                AddChannel.onClick.AddListener(AddChnl);
                RemoveChannel.onClick.AddListener(RemoveChnl);
                FSClose.onClick.AddListener(UIControl);
                DestroyToggle.onClick.AddListener(DesToggle);
                RemoteToggle.onClick.AddListener(RemoteDetToggle);
                Populate.onClick.AddListener(AutoPopulate);
                ShowMakerToggle.onClick.AddListener(ToggleShowmaker);
                IsActive = true;
            }
            else
            {
                PlayButtonClick();
                Messenger.Broadcast(new MessengerEventChangeUIMode(false, true));
                AddChannel.onClick.RemoveAllListeners();
                RemoveChannel.onClick.RemoveAllListeners();
                FSClose.onClick.RemoveAllListeners();
                DestroyToggle.onClick.RemoveAllListeners();
                RemoteToggle.onClick.RemoveAllListeners();
                Populate.onClick.RemoveAllListeners();
                ShowMakerToggle.onClick.RemoveAllListeners();
                UiController.SetActive(false);
                IsActive = false;
            }
        }

        public void DesToggle()
        {
            PlayButtonClick();
            if (destroy)
            {
                DestroyToggle.image.sprite = FSToggleOff;
                destroy = false;
            }
            else
            {
                DestroyToggle.image.sprite = FSToggleOn;
                destroy = true;
            }
        }

        public void RemoteDetToggle()
        {
            PlayButtonClick();
            if (remote)
            {
                RemoteToggle.image.sprite = FSToggleOff;
                remote = false;
            }
            else
            {
                RemoteToggle.image.sprite = FSToggleOn;
                remote = true;
            }
        }

        public void AutoPopulate()
        {
            PlayButtonClick();
            if (!confirm)
            {
                ConfirmScreen.SetActive(true);
                ConfYES.onClick.AddListener(CYES);
                ConfNO.onClick.AddListener(CNO);
                confirm = true;
            }
            else
            {
                ConfirmScreen.SetActive(false);
                ConfYES.onClick.RemoveAllListeners();
                ConfNO.onClick.RemoveAllListeners();
                confirm = false;
            }
        }

        public void CYES()
        {
            PlayButtonClick();
            populator();
            ConfirmScreen.SetActive(false);
            ConfYES.onClick.RemoveAllListeners();
            ConfNO.onClick.RemoveAllListeners();
            confirm = false;
        }
        public void CNO()
        {
            PlayButtonClick();
            ConfirmScreen.SetActive(false);
            ConfYES.onClick.RemoveAllListeners();
            ConfNO.onClick.RemoveAllListeners();
            confirm = false;
        }

        public void populator()
        {
            do
            {
                RemoveChnl();
            } while (Channels.Count > 0);

            List<int> ids2 = new List<int>();

            foreach (Transform T in this.gameObject.transform.parent.transform)
            {
                ReciverRFS reciver;
                if (T.gameObject.TryGetComponent<ReciverRFS>(out reciver))
                {
                    if (reciver != null)
                    {
                        ids2.Add(reciver.Rchannel);
                    }
                }
            }

            foreach (int id in ids2)
            {
                AddChnl(id);
            }

            if (Channels.Count == 0)
            {
                AddChnl(0);
            }
        }

        public void AddChnl()
        {
            PlayButtonClick();
            GameObject Tile = Instantiate(TilePrefab, TileParent.transform);
            Channels.Add(Tile);

            foreach (Transform T in Tile.transform)
            {
                TMP_InputField field;
                if (T.gameObject.TryGetComponent<TMP_InputField>(out field))
                {
                    field.text = (Channels.Count - 1).ToString();
                    break;
                }
            }
        }
        public void AddChnl(int chnl)
        {
            GameObject Tile = Instantiate(TilePrefab, TileParent.transform);
            Channels.Add(Tile);

            foreach (Transform T in Tile.transform)
            {
                TMP_InputField field;
                if (T.gameObject.TryGetComponent<TMP_InputField>(out field))
                {
                    field.text = (chnl).ToString();
                    break;
                }
            }
        }

        public void AddChnl(string chnl, string time)
        {
            GameObject Tile = Instantiate(TilePrefab, TileParent.transform);
            Channels.Add(Tile);

            foreach (Transform T in Tile.transform)
            {
                TMP_InputField field;
                if (T.gameObject.TryGetComponent<TMP_InputField>(out field))
                {
                    if (T.gameObject.name.Contains("ChannelInputField (TMP)"))
                    {
                        Debug.Log("\tUpdating Channel Feild");
                        field.text = chnl;
                        field.textComponent.text = chnl;
                        Debug.Log($"\t\tField Text = {field.text}");
                    }
                    if (T.gameObject.name.Contains("DelayInputField (TMP)"))
                    {
                        Debug.Log("\tUpdating Delay Feild");
                        field.text = time;
                        field.textComponent.text = time;
                        Debug.Log($"\t\tField Text = {field.text}");
                    }

                }
            }
            Canvas.ForceUpdateCanvases();
        }

        public void RemoveChnl()
        {
            if (Channels.Count >= 1)
            {
                PlayButtonClick();
                GameObject TileToRemove = Channels[Channels.Count - 1];
                Channels.Remove(TileToRemove);
                Destroy(TileToRemove);
            }
        }

        public void ToggleTool()
        {
            if (ToolActive)
            {
                ToolBar.SetActive(false);
                ToolActive = false;
                Messenger.Broadcast(new MessengerEventChangeUIMode(false, true));
            }
            else
            {
                ToolBar.SetActive(true);
                ToolActive = true;
                SetAudioPlayers();
                Messenger.Broadcast(new MessengerEventChangeUIMode(true, false));
            }
        }

        public void ToggleShowmaker()
        {
            // Debug.Log("Toggling Showmaker");
            PlayButtonClick();
            if (ShowmakerActive)
            {
                //Debug.Log("Turning Off");
                TopLevelUi.SetActive(false);
                ShowmakerActive = false;
                ShowMakerToggle.image.sprite = FSToggleOff;
            }
            else
            {
                //Debug.Log("Turning On");
                TopLevelUi.SetActive(true);
                ShowmakerActive = true;
                ShowMakerToggle.image.sprite = FSToggleOn;
                ToolBar.SetActive(true);
                ToolActive = true;
                SetAudioPlayers();
            }
        }

        public void SetStartChannel(string start)
        {
            startChannel = Mathf.Clamp(int.Parse(start), 0, 10000);
            SetAudioPlayers();
            SetAudioTimes();
        }

        public void SetStartChannel(int start)
        {
            startChannel = start;
            StartChannel.text = start.ToString("0");
            SetAudioPlayers();
            SetAudioTimes();
        }

        private void SetAudioPlayers()
        {
            Transform FManager = this.gameObject.transform.parent.transform;
            List<AudioSource> players = new List<AudioSource>();
            foreach (Transform T in FManager)
            {
                if (T.gameObject.name.Contains("FM Audio Player"))
                {
                    Debug.Log(T.name);
                    AudioSource Player;
                    if (T.gameObject.TryGetComponent(out Player))
                    {
                        Debug.Log("Found Audio Player");
                        players.Add(Player);
                    }
                }

            }
            FAudioPlayers = players.ToArray();
            AudioChnnls.text = $"Audio Players In Show: {FAudioPlayers.Length}";
        }

        private void SetAudioTimes()
        {
            if (ShowmakerActive)
            {

                List<float> data = new List<float>();
                foreach (Transform T in Channels[startChannel].transform)
                {
                    TMP_InputField field;
                    if (T.gameObject.TryGetComponent<TMP_InputField>(out field))
                    {
                        if (field != null)
                        {
                            float x = 10000f;
                            if (float.TryParse(field.text, out x)) { data.Add(x); }
                        }
                    }
                }

                if (data.Count == 2)
                {
                    //Debug.Log("Caught Channel: " + data[0] + " Caught Time: " + data[1]);
                    if (data[0] == startChannel)
                    {
                        startTime = data[1];
                    }
                }

                foreach (AudioSource Player in FAudioPlayers)
                {
                    if (Player.isPlaying) continue;
                    Player.time = startTime;
                    ShowTime.text = startTime.ToString("0000");
                }
            }
        }



        private void PlayButtonClick()
        {
            if (clickSound != null)
            {
                Messenger.Broadcast(new MessengerEventPlaySound(clickSound.name, this.transform, true, true));
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

            List<string> temp = new List<string>();


            foreach (GameObject G in Channels)
            {
                foreach (Transform T in G.transform)
                {
                    TMP_InputField field;
                    if (T.gameObject.TryGetComponent<TMP_InputField>(out field))
                    {
                        if (field != null)
                        {
                            temp.Add(field.text);
                        }
                    }
                }
            }

            entitydata.Add<List<string>>("Channels", temp);

            entitydata.Add<int>("Number", (Channels.Count - 1));

            entitydata.Add<bool>("Destroy", destroy);

            entitydata.Add<bool>("ShowMaker", ShowmakerActive);

            entitydata.Add<int>("StartChnnl", startChannel);

            entitydata.Add<bool>("HideTool", ToolActive);


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

            List<string> temp = customComponentData.Get<List<string>>("Channels");

            int num = customComponentData.Get<int>("Number");
            StartCoroutine(updateRFS(num, temp));


            bool tempbool = customComponentData.Get<bool>("Destroy");

            if (tempbool)
            {
                destroy = true;
                DestroyToggle.image.sprite = FSToggleOn;
            }
            else
            {
                destroy = false;
                DestroyToggle.image.sprite = FSToggleOff;
            }

            bool showactive = customComponentData.Get<bool>("ShowMaker");
            int srtchnl = customComponentData.Get<int>("StartChnnl");
            if (showactive)
            {
                ToggleShowmaker();
                SetStartChannel(srtchnl);
                SetAudioTimes();
            }

            bool toolactive = customComponentData.Get<bool>("HideTool");
            if (!toolactive) ToggleTool();

        }

        private IEnumerator updateRFS(int numb, List<string> data)
        {
            for (int i = 0; i <= numb; i++)
            {
                yield return new WaitForSeconds(Time.deltaTime);
                Debug.Log($"RFS - Adding Channel From BluePrint: {data[0]}, time: {data[1]} ");
                AddChnl(data[0], data[1]);

                data.RemoveAt(0);
                data.RemoveAt(0);
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
            UiController.SetActive(false);
            ConfirmScreen.SetActive(false);
        }

        protected override void OnValidate()
        {
            if (Application.isPlaying)
                return;
            base.OnValidate();
        }

        protected override async UniTask LaunchInternalAsync(CancellationToken token)
        {
            FireButton(false);
            await UniTask.WaitWhile(() => this.fired == false, PlayerLoopTiming.Update, token);
            token.ThrowIfCancellationRequested();
            if (destroy)
            {
                await this.DestroyFireworkAsync(token);
            }
        }
    }

}

