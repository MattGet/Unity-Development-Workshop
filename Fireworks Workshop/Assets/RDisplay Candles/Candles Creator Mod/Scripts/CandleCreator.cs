using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMod;
using FireworksMania.Core.Definitions.EntityDefinitions;
using FireworksMania.Core.Behaviors.Fireworks;
using FireworksMania.Core.Persistence;
using FireworksMania.Core.Messaging;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using UnityEngine.UI;
using System;

public class CandleCreator : ModScriptBehaviour
{
    public Dictionary<string, GameObject> CandleLibrary = new Dictionary<string, GameObject>();
    private bool Initialized = false;
    private GameObject RackItem;
    public Dictionary<string, PanelData> PresetLibrary = new Dictionary<string, PanelData>();
    private Dictionary<string, PanelData> UsablePresets = new Dictionary<string, PanelData>();

    [Header("Candle Creator Settings")]
    public GameObject CandleManagerMenu;
    private bool ManagerActive = false;
    public AudioClip ClickSound;
    public AudioClip ErrorSound;
    public AudioSource source1;

    [Header("Preset Inventory Settings")]
    public GameObject PresetInventory;
    public GameObject RackPanelPrefab;
    public TMP_InputField SearchBar;
    public Button CloseToggle;
    public Sprite SliderOff;
    public Sprite SliderOn;
    public TMP_Dropdown Sorting;
    public Button DebugToggle;
    private string SearchParameter = "";
    private bool UpdatingInventory = false;
    private bool CloseOnLoad = false;
    private bool ShowAll = false;
    private bool ShowOnlyRack = false;
    private bool UseDebug = false;
    private int SortingOption = 1;

    [Header("Preset Save Menu Settings")]
    public GameObject PresetSaveMenu;
    public TMP_Text SavePrompt;
    public TMP_InputField presetname;
    public TMP_InputField caliberid;
    public TMP_InputField countid;
    public TMP_Text UniquePrompt;
    private bool PresetMenuActive = false;

    [Header("Remove Menu Settings")]
    public GameObject RemoveMenu;
    public TMP_Text RemovePrompt;
    private bool RemoveMenuActive = false;
    private string PresetToRemove;



    public void Start()
    {
        Debug.Log("\n\nCandle Creator Loaded... Waiting For Initialization\n\n");
        this.gameObject.name = "Candle Library Manager";
        StartCoroutine(wait());
        CandleManagerMenu.SetActive(false);
    }

    private IEnumerator wait()
    {
        yield return new WaitUntil(() => (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Q)));
        Debug.Log("\n\nStarting Initialization...\n\n");
        InitializeDictionary();
    }

    private void InitializeDictionary()
    {
        if (Initialized) return;
        CandleLibrary.Clear();
        List<RomanCandleBehavior> candles = FindObjectsOfType<RomanCandleBehavior>(true).ToList();
        Debug.Log($"Candles Collection Size = {candles.Count}");
        foreach (RomanCandleBehavior def in candles)
        {
            if (!CandleLibrary.ContainsKey(def.EntityDefinition.Id))
            {
                CandleLibrary.Add(def.EntityDefinition.Id, def.EntityDefinition.PrefabGameObject);
            }
        }

        Initialized = true;

        Debug.Log("\n\nInitialized Candle Dictionary\n\n");
        foreach (KeyValuePair<string, GameObject> kvp in CandleLibrary)
        {
            Debug.Log($"\t{kvp.Key}, {kvp.Value.name}");
        }
        Debug.Log("\nFinished Candle Dictionary Readout\n\n");
    }

    public void ToggleCandleCreator(GameObject Rack = null)
    {
        if (ManagerActive)
        {
            if (Rack != null) return;
            ManagerActive = false;
            CandleManagerMenu.SetActive(false);
            ModPersistentData.SaveBool("CloseOnLoad", CloseOnLoad);
            ModPersistentData.SaveBool("UseDebug", UseDebug);
            ModPersistentData.SaveInt("SortingValue", SortingOption);
            Messenger.Broadcast<MessengerEventChangeUIMode>(new MessengerEventChangeUIMode(false, true));
            RackItem = null;
            PlayClick();
        }
        else
        {
            ManagerActive = true;
            RackItem = Rack;
            try
            {
                PersistentLoadLibrary();
                if (RackItem != null)
                {
                    CandleManagerMenu.SetActive(true);
                    Messenger.Broadcast<MessengerEventChangeUIMode>(new MessengerEventChangeUIMode(true, false));
                    PlayClick();
                    if (ModPersistentData.Exists("CloseOnLoad"))
                    {
                        CloseOnLoad = ModPersistentData.LoadBool("CloseOnLoad");
                        if (CloseOnLoad)
                        {
                            CloseToggle.image.sprite = SliderOn;
                        }
                        else
                        {
                            CloseToggle.image.sprite = SliderOff;
                        }
                    }
                    if (ModPersistentData.Exists("UseDebug"))
                    {
                        UseDebug = ModPersistentData.LoadBool("UseDebug");
                        if (UseDebug)
                        {
                            DebugToggle.image.sprite = SliderOn;
                        }
                        else
                        {
                            DebugToggle.image.sprite = SliderOff;
                        }
                    }
                    if (ModPersistentData.Exists("SortingValue"))
                    {
                        SortingOption = ModPersistentData.LoadInt("SortingValue");
                        Sorting.value = SortingOption;
                    }
                    GetUsablePresets();
                    StartCoroutine(UpdateInventory());
                }
                else
                {
                    Debug.LogError("CC FATAL ERROR: GAMEOBJECT NOT PASSED ON INITIALIZE");
                    PlayError();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("CC FATAL ERROR: FAILED TO LOAD PERSISTENT LIBRARY");
                Debug.LogException(ex);
                PlayError();
            }
        }
    }

    private void GetUsablePresets()
    {
        UsablePresets.Clear();
        List<string> presetData = GetPresetData();
        int caliber = 0;
        try
        {
            caliber = int.Parse(RackItem.name.Substring(0, 2));
        }
        catch
        {
            caliber = 0;
            if (UseDebug) Debug.Log("Rack Did not Idntify as 30mm, 40mm, or 48mm! Reverting to 0mm!");
        }

        if (caliber != 30 && caliber != 40 && caliber != 48) caliber = 99;

        
        foreach (KeyValuePair<string, PanelData> preset in PresetLibrary)
        {
            if (ShowOnlyRack)
            {
                if (preset.Value.CandleCount == presetData.Count && (preset.Value.Caliber == caliber || caliber == 99) && preset.Value.Title == RackItem.name)
                {
                    UsablePresets.Add(preset.Key, preset.Value);
                }
            }
            else
            {
                if (preset.Value.CandleCount == presetData.Count && (preset.Value.Caliber == caliber || caliber == 99))
                {
                    UsablePresets.Add(preset.Key, preset.Value);
                }
            }
        }
    }

    public void TogglePresetCreationMenu(bool save)
    {
        if (PresetMenuActive)
        {
            if (save)
            {
                if (SavePreset())
                {
                    UniquePrompt.gameObject.SetActive(false);
                    GetUsablePresets();
                    StartCoroutine(UpdateInventory());
                }
                else
                {
                    return;
                }
            }
            PresetMenuActive = false;
            PresetSaveMenu.SetActive(false);
        }
        else
        {
            PresetMenuActive = true;
            PresetSaveMenu.SetActive(true);
            List<string> preset = GetPresetData();
            int caliber;
            try
            {
                caliber = int.Parse(RackItem.name.Substring(0, 2));
            }
            catch
            {
                caliber = 0;
            }
            caliberid.text = caliber.ToString();
            countid.text = preset.Count.ToString();
            string RackDescription = $"Rack Name: {RackItem.name}\n";
            int i = 0;
            foreach (string item in preset)
            {
                RackDescription = RackDescription + $"Slot({i}) - {item}\n";
                i++;
            }
            SavePrompt.text = RackDescription;
            UniquePrompt.gameObject.SetActive(false);
            PlayClick();
        }
    }

    public void ToggleCloseOnLoad()
    {
        PlayClick();
        if (CloseOnLoad)
        {
            CloseOnLoad = false;
            CloseToggle.image.sprite = SliderOff;
            ModPersistentData.SaveBool("CloseOnLoad", CloseOnLoad);
        }
        else
        {
            CloseOnLoad = true;
            CloseToggle.image.sprite = SliderOn;
            ModPersistentData.SaveBool("CloseOnLoad", CloseOnLoad);
        }
    }

    public void ToggleDebug()
    {
        PlayClick();
        if (UseDebug)
        {
            UseDebug = false;
            DebugToggle.image.sprite = SliderOff;
            ModPersistentData.SaveBool("UseDebug", UseDebug);
        }
        else
        {
            UseDebug = true;
            DebugToggle.image.sprite = SliderOn;
            ModPersistentData.SaveBool("UseDebug", UseDebug);
        }
    }

    public void ToggleSorting(int option)
    {
        PlayClick();
        if (option == 0)
        {
            ShowAll = false;
            ShowOnlyRack = false;
        }
        else if (option == 1)
        {
            ShowAll = false;
            ShowOnlyRack = true;
        }
        else if (option == 2)
        {
            ShowAll = true;
            ShowOnlyRack = false;
        }
        SortingOption = option;
        ModPersistentData.SaveInt("SortingValue", SortingOption);
        GetUsablePresets();
        StartCoroutine(UpdateInventory());
    }

    public bool SavePreset()
    {
        List<string> presetdata = GetPresetData();
        int caliber = int.Parse(caliberid.text);
        int count = int.Parse(countid.text);
        string name = presetname.text;

        PanelData preset = new PanelData(RackItem.name, caliber, count, presetdata);
        if (UseDebug)
        {
            Debug.Log($"Data = {RackItem.name}, {preset.Caliber}, {preset.CandleCount}");
            Debug.Log(this.PresetLibrary);
        }
        
        if (!this.PresetLibrary.ContainsKey(name))
        {
            PresetLibrary.Add(name, preset);
            PlayClick();
            return true;
        }
        else
        {
            Debug.Log("CC ERROR: PRESET ALREADY EXISTS!!!");
            PlayError();
            UniquePrompt.gameObject.SetActive(true);
            return false;
        }
    }

    private List<string> GetPresetData()
    {
        List<string> presetData = new List<string>();
        Transform Candlemanager = RackItem.transform.Find("Candle Managers Parent");
        foreach (Transform T in Candlemanager)
        {
            RomanCandleBehavior candle = T.gameObject.GetComponentInChildren<RomanCandleBehavior>();
            if (candle == null)
            {
                presetData.Add("Empty");
            }
            else
            {
                presetData.Add(candle.EntityDefinition.Id);
            }
        }
        return presetData;
    }

    public bool LoadPreset(string preset)
    {
        PanelData data;
        bool loadSuccess = true;
        if (this.PresetLibrary.TryGetValue(preset, out data))
        {
            if (UseDebug) Debug.Log($"Load data = {data}");
            List<string> presData = data.Data;
            Transform Candlemanagers = RackItem.transform.Find("Candle Managers Parent");
            Rigidbody RackBody;
            if (RackItem.TryGetComponent(out RackBody))
            {
                StartCoroutine(Freeze(RackBody));
            }

            if (UseDebug) Debug.Log($"Candle Manager = {Candlemanagers}");
            int j = 0;
            foreach (Transform T in Candlemanagers.transform)
            {
                foreach (Transform K in T)
                {
                    RomanCandleBehavior Candlescript;
                    if (K.gameObject.TryGetComponent(out Candlescript))
                    {
                        Destroy(K.gameObject);
                    }
                }
                if (j > presData.Count - 1)
                {
                    Debug.Log($"CC ERROR: SIZE OF RACK DIDNT MATCH SIZE OF PRESET, PRESET NAME = {preset}, RACK = {RackItem}");
                    loadSuccess = false;
                    PlayError();
                    break;
                }
                if (presData[j] == "Empty")
                {
                    j++;
                    continue;
                }
                else
                {
                    GameObject ItemToSpawn;
                    if (CandleLibrary.TryGetValue(presData[j], out ItemToSpawn))
                    {
                        GameObject SpawnedItem = Instantiate(ItemToSpawn, T);
                        SpawnedItem.transform.localPosition = Vector3.zero;
                    }
                    j++;
                }
            }
            if (CloseOnLoad && loadSuccess)
            {
                ToggleCandleCreator();
            }
            else if (loadSuccess)
            {
                PlayClick();
            }
        }
        else
        {
            Debug.Log("CC FATAL ERROR: COULD NOT LOAD PRESET, PRESET NOT FOUND");
            loadSuccess = false;
            PlayError();
        }
        return loadSuccess;
    }

    private IEnumerator Freeze(Rigidbody body)
    {
        body.constraints = RigidbodyConstraints.FreezeAll;
        bool temp = body.isKinematic;
        body.isKinematic = true;
        yield return new WaitForSeconds(0.5f);
        body.constraints = RigidbodyConstraints.None;
        body.isKinematic = temp;
    }

    public void ToggleOnRemoveMenu(string preset)
    {
        if (!RemoveMenuActive)
        {
            PlayClick();
            RemoveMenu.SetActive(true);
            RemovePrompt.text = $"Permanetly Delete The Preset: {preset}?";
            RemoveMenuActive = true;
            PresetToRemove = preset;
        }
    }

    public void ToggleOffRemoveMenu(bool remove)
    {
        if (RemoveMenuActive)
        {
            if (remove)
            {
                RemovePreset(PresetToRemove);
            }
            PlayClick();
            RemoveMenuActive = false;
            RemoveMenu.SetActive(false);
            PresetToRemove = "";
        }
    }

    public void RemovePreset(string preset)
    {
        PresetLibrary.Remove(preset);
        GetUsablePresets();
        StartCoroutine(UpdateInventory());
    }

    public void UpdateSearch(string search)
    {
        SearchParameter = search;
        if (!UpdatingInventory) StartCoroutine(UpdateInventory());
    }

    public void ClearSearch()
    {
        SearchParameter = "";
        SearchBar.SetTextWithoutNotify("");
        StartCoroutine(UpdateInventory());
    }

    private IEnumerator UpdateInventory()
    {
        UpdatingInventory = true;
        foreach (Transform T in PresetInventory.transform)
        {
            Destroy(T.gameObject);
        }

        Dictionary<string, PanelData> CurrentPresets = new Dictionary<string, PanelData>();
        if (SearchParameter == "")
        {
            if (ShowAll)
            {
                CurrentPresets = PresetLibrary;
            }
            else
            {
                CurrentPresets = UsablePresets;
            }
        }
        else
        {
            if (ShowAll)
            {
                foreach (KeyValuePair<string, PanelData> preset in PresetLibrary)
                {
                    if (preset.Key.Contains(SearchParameter, StringComparison.OrdinalIgnoreCase))
                    {
                        CurrentPresets.Add(preset.Key, preset.Value);
                    }
                }
            }
            else
            {
                foreach (KeyValuePair<string, PanelData> preset in UsablePresets)
                {
                    if (preset.Key.Contains(SearchParameter, StringComparison.OrdinalIgnoreCase))
                    {
                        CurrentPresets.Add(preset.Key, preset.Value);
                    }
                }
            }

        }


        foreach (KeyValuePair<string, PanelData> preset in CurrentPresets)
        {
            GameObject Panel = Instantiate(RackPanelPrefab, PresetInventory.transform);
            Panel.name = preset.Key;

            CustomRackPanel data = Panel.GetComponent<CustomRackPanel>();
            data.data = preset.Value;
            yield return new WaitForSeconds(Time.deltaTime);
            data.GetUI();
            yield return new WaitForSeconds(Time.deltaTime);
            data.InitializeData();
        }
        PersistentSaveLibrary();
        UpdatingInventory = false;
    }

    private void PersistentSaveLibrary()
    {
        string json = JsonConvert.SerializeObject(PresetLibrary, Formatting.Indented);
        if (UseDebug) Debug.Log("Saving JSON: \n\n" + json);
        ModPersistentData.SaveString("CCLibrary", json);
    }

    private void PersistentLoadLibrary()
    {
        if (!ModPersistentData.Exists("CCLibrary"))
        {
            string jsontemp = JsonConvert.SerializeObject(PresetLibrary, Formatting.Indented);
            ModPersistentData.SaveString("CCLibrary", jsontemp);
        }
        string json = ModPersistentData.LoadString("CCLibrary", "Error Loading Data");
        if (UseDebug) Debug.Log("Loaded JSON: \n\n" + json + "\n\n");
        Dictionary<string, PanelData> tempDic = JsonConvert.DeserializeObject<Dictionary<string, PanelData>>(json);
        if (tempDic != null)
        {
            PresetLibrary = tempDic;
        }
        else
        {
            PresetLibrary = new Dictionary<string, PanelData>();
        }
    }

    private void Update()
    {
        if (ManagerActive)
        {
            if (Cursor.lockState != CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.None;
            }
        }
        if (ManagerActive)
        {
            if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
            {
                ToggleCandleCreator();
            }
        }
    }

    private void PlayClick()
    {
        source1.PlayOneShot(ClickSound);
    }

    private void PlayError()
    {
        source1.PlayOneShot(ErrorSound);
    }
}

public static class StringExtensions
{
    public static bool Contains(this string source, string toCheck, StringComparison comp)
    {
        if (source != null)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }
        else
        {
            return false;
        }

    }
}
