using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FireworksMania.Core.Messaging;
using System;
using UnityEngine.SceneManagement;

public class UIMapLoader : MonoBehaviour
{
    [Space(10)]
    [Header("---------- Custom Map Varibles ----------")]
    [Space(20)]
    public GameObject MAP;
    public Vector3 PlayerSpawnLocation;
    public string MapName = "Default Map Name";
    public Sprite LoadScreenBackground;

    [Space(20)]
    [Header("---------- Preset Variables --------------")]
    [Space(20)]
    public Button Confirm;
    public Button Close;
    public GameObject LoadingScreen;
    public GameObject MapLoadUI;
    public GameObject MapFail;
    public Image LoadImage;
    public TMP_Text MainText;
    public TMP_Text LoadingText;
    public GameObject PlayerHolder;

    private GameObject Player;
    private GameObject Catcher;
    private Vector3 Catcherloc;
    private GameObject FPC;
    private GameObject EnvironmentParent;
    private GameObject tempground;
    private bool IsMenu;
    // Start is called before the first frame update
    void Start()
    {

        EnvironmentParent = GameObject.Find("--- ENVIRONMENT ---");
        Debug.Log(EnvironmentParent.transform.position);

        foreach (Transform T in EnvironmentParent.transform)
        {
            if (T.gameObject.name == "Environment")
            {
                foreach (Transform K in T)
                {
                    //Debug.Log("Catcher Finder = " + K.gameObject.name);
                    if (K.gameObject.name == "ObjectCatcher")
                    {
                        Catcher = K.gameObject;
                        //Debug.Log("Found Catcher, Pos = " + Catcher.transform.position);
                        Catcherloc = Catcher.transform.position;
                    }
                }
            }
        }
        foreach (Rigidbody obj in FindObjectsOfType<Rigidbody>())
        {
            if (obj.tag == "MainCamera")
            {
                Player = obj.gameObject;
                //Debug.Log("Found Player = " + Player.gameObject.name);
            }
        }
        FPC = GameObject.Find("FirstPersonCharacter");
        Debug.Log("Found FPC = " + FPC.name);
    }

    public void Usable()
    {
        Rigidbody rig = this.GetComponent<Rigidbody>();
        rig.constraints = RigidbodyConstraints.FreezeAll;

        Confirm.onClick.AddListener(StartMapLoad);
        Close.onClick.AddListener(CloseUI);
        LoadingScreen.SetActive(false);
        MapLoadUI.SetActive(true);
        IsMenu = true;
        Messenger.Broadcast(new MessengerEventChangeUIMode(true, false));
    }

    public void OnValidate()
    {
        if (MainText != null)
        {
            MainText.text = "You Have The Mod \" " + MapName + " \" Installed, Do You Wish To Load This Map?";
        }
        if (LoadImage != null && LoadScreenBackground != null)
        {
            LoadImage.sprite = LoadScreenBackground;
        }
    }


    public void CloseUI()
    {
        IsMenu = false;
        Messenger.Broadcast(new MessengerEventChangeUIMode(false, true));
        Confirm.onClick.RemoveAllListeners();
        Close.onClick.RemoveAllListeners();
        UnityEngine.Object.Destroy(this.gameObject);
    }

    public void StartMapLoad()
    {
        IsMenu = false;
        MapLoadUI.SetActive(false);
        StartCoroutine(DestroyCurrentEnviornment());
    }

    private IEnumerator MapFailed(bool unfreeze)
    {
        MapFail.SetActive(true);
        yield return new WaitForSeconds(5);
        if (unfreeze)
        {
            Messenger.Broadcast(new MessengerEventChangeUIMode(false, true));
            MapFail.SetActive(false);
        }
        yield return new WaitForSeconds(3);
        CloseUI();
    }

    public IEnumerator MapLoader()
    {
        GameObject MapParent = new GameObject();
        try
        {
            LoadingText.text = $"Starting Loading Map: {MapName}";
            Debug.Log("Started Map Loading");
            MapParent.name = MapName;
            GameObject.Instantiate(MapParent, EnvironmentParent.transform);
            MapParent.transform.position = MAP.transform.position;
        }
        catch (Exception e)
        {
            Debug.Log("Map Loading Failed at stage 1:  " + e);
            StartCoroutine(MapFailed(false));
        }


        yield return new WaitForSeconds(2);


        int numberofObjects = 0;
        try
        {
            foreach (Transform test in MAP.transform)
            {
                numberofObjects++;
            }
            LoadingText.text = "Starting To Load " + numberofObjects + " Objects";
            Debug.Log("Starting To Load " + numberofObjects + " Objects");
        }
        catch (Exception e)
        {
            Debug.Log("Map Loading Failed at stage 2:  " + e);
            StartCoroutine(MapFailed(false));
        }


        yield return new WaitForSeconds(2);
        int CurrentObject = 1;

        foreach (Transform T in MAP.transform)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            try
            {
                GameObject temp = GameObject.Instantiate(T.gameObject, MapParent.transform);
                temp.transform.localPosition = T.localPosition;
                temp.name = T.gameObject.name;

                //Debug.Log("CUSTOM MAP Loading " + temp.gameObject.name + " At Position: " + temp.transform.position +" (" + CurrentObject + " / " + numberofObjects + ") Objects");
                LoadingText.text = "Loading Objects " + " (" + CurrentObject.ToString("00000") + " / " + numberofObjects.ToString("00000") + ")";
                CurrentObject++;
            }
            catch (Exception e)
            {
                Debug.Log("Map Loading Failed at Object " + CurrentObject + " : " + e);
                StartCoroutine(MapFailed(false));
            }
        }
        yield return new WaitForSeconds(Time.deltaTime);
        LoadingText.text = "Finished Loading " + MapName + " Spawning in...";
        Debug.Log("Finished Loading " + MapName + " Spawning in");
        Destroy(tempground);
        yield return new WaitForSeconds(3);


        tplayer(PlayerSpawnLocation);


        yield return new WaitForSeconds(Time.deltaTime);


        LoadingScreen.SetActive(false);
        yield return new WaitForSeconds(2);

        /*
        foreach(Transform T in FPC.transform)
        {
            Debug.Log("-" + T.gameObject.name + " Is " + T.gameObject.activeSelf);
            foreach (Transform K in T)
            {
                Debug.Log("\t-" + K.gameObject.name + " Is " + K.gameObject.activeSelf);
                foreach (Transform C in K)
                {
                    Debug.Log("\t\t-" + C.gameObject.name + " Is " + C.gameObject.activeSelf);
                    foreach (Transform J in C)
                    {
                        Debug.Log("\t\t\t-" + J.gameObject.name + " Is " + J.gameObject.activeSelf);

                    }
                }
            }
        }

        */

        CloseUI();
    }

    public IEnumerator DestroyCurrentEnviornment()
    {
        if (EnvironmentParent != null)
        {
            LoadingScreen.SetActive(true);
            Messenger.Broadcast(new MessengerEventChangeUIMode(false, true));
            Scene scene = SceneManager.GetActiveScene();
            LoadingText.text = $"Destroying Current Map: {scene.name}";
            tempground = GameObject.Instantiate(PlayerHolder.gameObject);
            tempground.transform.position = new Vector3(Player.transform.position.x, Player.transform.position.y - 2, Player.transform.position.z);
            foreach (Transform T in EnvironmentParent.transform)
            {
                Debug.Log("GameObject = " + T.gameObject.name);
                if (T.gameObject.name != "Environment" && T.gameObject.name != "[GlobalPostProcessingVolume]")
                {
                    Destroy(T.gameObject);
                    Debug.Log($"Destroyed {T.gameObject.name}");
                    continue;
                }
                if (T.gameObject.name == "CloudParent_Fast" || T.gameObject.name == "CloudParent_Slow")
                {
                    Destroy(T.gameObject);
                    Debug.Log("\tDestroyed");
                    continue;
                }
                if (T.gameObject.name == "Ocean")
                {
                    Destroy(T.gameObject);
                    Debug.Log("\tDestroyed");
                    continue;
                }
                if (T.gameObject.name == "Ground")
                {
                    Destroy(T.gameObject);
                    Debug.Log("\tDestroyed");
                    continue;
                }
                if (T.gameObject.name == "Mountains")
                {
                    Destroy(T.gameObject);
                    Debug.Log("\tDestroyed");
                    continue;
                }
                if (T.gameObject.name == "Town")
                {
                    Destroy(T.gameObject);
                    Debug.Log("\tDestroyed");
                    continue;
                }
                if (T.gameObject.name == "Unlockables")
                {
                    Destroy(T.gameObject);
                    Debug.Log("\tDestroyed");
                    continue;
                }
                if (T.gameObject.name == "4th of July Decorations")
                {
                    Destroy(T.gameObject);
                    Debug.Log("\tDestroyed");
                    continue;
                }
                if (T.gameObject.name == "4th Of July Decorations")
                {
                    Destroy(T.gameObject);
                    Debug.Log("\tDestroyed");
                    continue;
                }
                if (T.gameObject.name == "4th of July decorations 2021")
                {
                    Destroy(T.gameObject);
                    Debug.Log("\tDestroyed");
                    continue;
                }
                if (T.gameObject.name == "Farm")
                {
                    Destroy(T.gameObject);
                    Debug.Log("\tDestroyed");
                    continue;
                }
                if (T.gameObject.name == "City")
                {
                    Destroy(T.gameObject);
                    Debug.Log("\tDestroyed");
                    continue;
                }
                Component[] components = T.gameObject.GetComponents(typeof(Component));
                foreach (Component C in components)
                {
                    Type Comp = C.GetType();
                    if (Comp == typeof(MeshRenderer))
                    {
                        Destroy(C);
                    }
                    else if (Comp == typeof(MeshFilter))
                    {
                        Destroy(C);
                    }
                    else if (Comp == typeof(Collider))
                    {
                        Destroy(C);
                    }
                    else if (Comp == typeof(MeshCollider))
                    {
                        Destroy(C);
                    }
                    else if (Comp == typeof(BoxCollider))
                    {
                        Destroy(C);
                    }
                    else if (Comp == typeof(SphereCollider))
                    {
                        Destroy(C);
                    }
                    else if (Comp == typeof(CapsuleCollider))
                    {
                        Destroy(C);
                    }
                    else if (Comp == typeof(LODGroup))
                    {
                        Destroy(C);
                    }
                    else if (Comp != typeof(Transform))
                    {
                        Debug.Log("Skipped over " + C.name + " type = " + Comp);
                    }
                }

                foreach (Transform K in T)
                {
                    //Debug.Log("GameObject = " + K.gameObject.name);
                    Component[] components2 = K.gameObject.GetComponents(typeof(Component));
                    foreach (Component C in components2)
                    {
                        yield return new WaitForSeconds(Time.deltaTime);
                        Type Comp2 = C.GetType();
                        if (Comp2 == typeof(MeshRenderer))
                        {
                            Destroy(C);
                        }
                        else if (Comp2 == typeof(MeshFilter))
                        {
                            Destroy(C);
                        }
                        else if (Comp2 == typeof(Collider))
                        {
                            Destroy(C);
                        }
                        else if (Comp2 == typeof(MeshCollider))
                        {
                            Destroy(C);
                        }
                        else if (Comp2 == typeof(BoxCollider))
                        {
                            Destroy(C);
                        }
                        else if (Comp2 == typeof(SphereCollider))
                        {
                            Destroy(C);
                        }
                        else if (Comp2 == typeof(CapsuleCollider))
                        {
                            Destroy(C);
                        }
                        else if (Comp2 == typeof(LODGroup))
                        {
                            Destroy(C);
                        }
                        else if (Comp2 != typeof(Transform))
                        {
                            Debug.Log("Skipped over " + C.name + " type = " + Comp2);
                        }
                    }
                }

            }
            yield return new WaitForSeconds(Time.deltaTime + 2);
            StartCoroutine(MapLoader());
        }
        else
        {
            StartCoroutine(MapFailed(true));
            Debug.LogError("Failed To Load Map, Environment Variables Could Not Be Found!");
            yield break;
        }
    }


    public void tplayer(Vector3 position)
    {
        GameObject Player2;
        Player2 = GameObject.FindWithTag("Player");
        Player2.transform.position = position;
        Physics.SyncTransforms();
        Debug.Log("Teleported Player to " + position + " Player = " + Player2.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        if (IsMenu)
        {
            //Messenger.Broadcast(new MessengerEventChangeUIMode(true, false));
            if (Input.GetKeyDown(KeyCode.Y))
            {
                StartMapLoad();
                IsMenu = false;
            }
            if (Input.GetKeyDown(KeyCode.N))
            {
                IsMenu = false;
                CloseUI();
            }
        }
    }
}
