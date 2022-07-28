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

    [Header("Preset Inventory Settings")]
    public GameObject PresetInventory;
    public GameObject RackPanelPrefab;
    public TMP_InputField SearchBar;
    public Button CloseToggle;
    public Sprite SliderOff;
    public Sprite SliderOn;
    private string SearchParameter = "";
    private bool UpdatingInventory = false;
    private bool CloseOnLoad = false;

    [Header("Preset Save Menu Settings")]
    public GameObject PresetSaveMenu;
    public TMP_Text SavePrompt;
    public TMP_InputField presetname;
    public TMP_InputField caliberid;
    public TMP_InputField countid;
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
            Messenger.Broadcast<MessengerEventChangeUIMode>(new MessengerEventChangeUIMode(false, true));
            RackItem = null;
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
                    GetUsablePresets();
                    StartCoroutine(UpdateInventory());
                }
                else
                {
                    Debug.LogError("CC FATAL ERROR: GAMEOBJECT NOT PASSED ON INITIALIZE");
                }
            }
            catch
            {
                Debug.LogError("CC FATAL ERROR: FAILED TO LOAD PERSISTENT LIBRARY");
            }
        }
    }

    private void GetUsablePresets()
    {
        UsablePresets.Clear();
        List<string> presetData = GetPresetData();
        int caliber = int.Parse(RackItem.name.Substring(0, 2));
        foreach (KeyValuePair<string, PanelData> preset in PresetLibrary)
        {
            if (preset.Value.CandleCount == presetData.Count && preset.Value.Caliber == caliber)
            {
                UsablePresets.Add(preset.Key, preset.Value);
            }
        }
    }

    public void TogglePresetCreationMenu(bool save)
    {
        if (PresetMenuActive)
        {
            if (save)
            {
                SavePreset();
                GetUsablePresets();
                StartCoroutine(UpdateInventory());
            }
            PresetMenuActive = false;
            PresetSaveMenu.SetActive(false);
        }
        else
        {
            PresetMenuActive = true;
            PresetSaveMenu.SetActive(true);
            List<string> preset = GetPresetData();
            int caliber = int.Parse(RackItem.name.Substring(0, 2));
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
        }
    }

    public void ToggleCloseOnLoad()
    {
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

    public void SavePreset()
    {
        List<string> presetdata = GetPresetData();
        int caliber = int.Parse(caliberid.text);
        int count = int.Parse(countid.text);
        string name = presetname.text;

        PanelData preset = new PanelData(name, caliber, count, presetdata);
        Debug.Log($"Data = {preset.Title}, {preset.Caliber}, {preset.CandleCount}");
        Debug.Log(this.PresetLibrary);
        if (!this.PresetLibrary.ContainsKey(name))
        {
            PresetLibrary.Add(name, preset);
        }
        else
        {
            Debug.Log("CC ERROR: PRESET ALREADY EXISTS!!!");
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
            Debug.Log($"Load data = {data}");
            List<string> presData = data.Data;
            GameObject Candlemanagers = new GameObject();
            foreach (Transform T in RackItem.transform)
            {
                if (T.gameObject.name == "Candle Managers Parent")
                {
                    Candlemanagers = T.gameObject;
                }
            }
            Debug.Log($"Candle Manager = {Candlemanagers}");
            int j = 0;
            foreach (Transform T in Candlemanagers.transform)
            {
                foreach (Transform K in T)
                {
                    Destroy(K.gameObject);
                }
                if (j > presData.Count - 1)
                {
                    Debug.Log($"CC ERROR: SIZE OF RACK DIDNT MATCH SIZE OF PRESET, PRESET NAME = {preset}, RACK = {RackItem}");
                    loadSuccess = false;
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
        }
        else
        {
            Debug.Log("CC FATAL ERROR: COULD NOT LOAD PRESET, PRESET NOT FOUND");
            loadSuccess = false;
        }
        return loadSuccess;
    }

    public void ToggleOnRemoveMenu(string preset)
    {
        if (!RemoveMenuActive)
        {
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
            CurrentPresets = UsablePresets;
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
        Debug.Log("Saving JSON: \n\n" + json);
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
        Debug.Log("Loaded JSON: \n\n" + json + "\n\n");
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
