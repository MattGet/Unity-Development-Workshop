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
using CustomTubes;

public class ShellsCreator : ModScriptBehaviour
{
    public string Name = "Shells Library Manager 2";
    public Dictionary<string, GameObject> ShellLibrary = new Dictionary<string, GameObject>();
    private bool Initialized = false;
    private GameObject RackItem;
    [SerializeField]
    public Dictionary<string, ShellsPanelData> PresetLibrary = new Dictionary<string, ShellsPanelData>();
    private Dictionary<string, ShellsPanelData> UsablePresets = new Dictionary<string, ShellsPanelData>();

    [Header("Shell Creator Settings")]
    public GameObject ShellManagerMenu;
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
    public TMP_Dropdown PSorting;
    public Button DebugToggle;
    private string SearchParameter = "";
    private bool UpdatingInventory = false;
    private bool CloseOnLoad = false;
    private bool ShowAll = false;
    private bool ShowOnlyRack = false;

    public static explicit operator ShellsCreator(GameObject v)
    {
        throw new NotImplementedException();
    }

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
    private ShellsCreator Creator;



    public void Start()
    {
        Debug.Log("\n\nShell Creator Loaded... Waiting For Initialization\n\n");
        this.gameObject.name = Name;
        this.gameObject.transform.parent = GameObject.Find("--- MANAGERS ---").transform;
        Debug.Log("Name: " + this.gameObject.name);
        Debug.Log("Parent: " + this.gameObject.transform.parent.name);
        ShellManagerMenu.SetActive(false);
        InitializeDictionary();
        source1.gameObject.SetActive(true);
    }
    
    private void InitializeDictionary()
    {
        if (Initialized) return;
        if (ShellLibrary == null) Debug.LogError("SS: Fatal error Shell Dictionary was null!!");
        ShellLibrary.Clear();
        List<BaseFireworkBehavior> candles = FindObjectsOfType<BaseFireworkBehavior>(true).ToList();
        Debug.Log($"Shells Collection Size = {candles.Count}");
        if (candles == null) Debug.LogError("SS: Fatal error Dictionary was null!!");
        foreach (BaseFireworkBehavior def in candles)
        {
            try
            {
                if (def.EntityDefinition == null)
                {
                    Debug.LogError("SS: Entity was null");
                    continue;
                }
                if (!ShellLibrary.ContainsKey(def.EntityDefinition.Id))
                {
                    if (def.EntityDefinition.PrefabGameObject == null)
                    {
                        Debug.LogError("SS: Entity GameObject was null at " + def.EntityDefinition.name);
                        continue;
                    }
                    ShellLibrary.Add(def.EntityDefinition.Id, def.EntityDefinition.PrefabGameObject);
                }
            }
            catch (Exception ex) {
                Debug.LogError(ex);
                continue;
            }
        }

        Initialized = true;

        Debug.Log("\n\nInitialized Shell Dictionary\n\n");
        foreach (KeyValuePair<string, GameObject> kvp in ShellLibrary)
        {
            Debug.Log($"\t{kvp.Key}, {kvp.Value.name}");
        }
        Debug.Log("\nFinished Shell Dictionary Readout\n\n");
    }

    public void ToggleShellsCreator(GameObject Rack = null)
    {
        Debug.Log("SS: Message Recived");
        if (ManagerActive)
        {
            if (Rack != null) return;
            ManagerActive = false;
            ShellManagerMenu.SetActive(false);
            ModPersistentData.SaveBool("CloseOnLoad", CloseOnLoad);
            ModPersistentData.SaveBool("UseDebug", UseDebug);
            ModPersistentData.SaveInt("SortingValue", SortingOption);
            PSorting.onValueChanged.RemoveAllListeners();
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
                    ShellManagerMenu.SetActive(true);
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
                        PSorting.onValueChanged.AddListener(ToggleSorting);
                        PSorting.value = SortingOption;
                    }
                    GetUsablePresets();
                    StartCoroutine(UpdateInventory());
                }
                else
                {
                    Debug.LogError("SS FATAL ERROR: GAMEOBJECT NOT PASSED ON INITIALIZE");
                    PlayError();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("SS FATAL ERROR: FAILED TO LOAD PERSISTENT LIBRARY");
                Debug.LogException(ex);
                PlayError();
            }
        }
    }

    private void GetUsablePresets()
    {
        UsablePresets.Clear();
        List<string> presetData = GetPresetData(false);
        float caliber = 0f;
        try
        {
            caliber = float.Parse(RackItem.name.Substring(0, 3));
        }
        catch
        {
            caliber = 0;
            if (UseDebug) Debug.Log("Rack Did not Idntify Correctly! Reverting to 0Inch!");
        }

        if (caliber != 1.75 && caliber != 3.0 && caliber != 6.0 && caliber != 10.0) caliber = 99.9f;

        
        foreach (KeyValuePair<string, ShellsPanelData> preset in PresetLibrary)
        {
            if (ShowOnlyRack)
            {
                if (preset.Value.ShellCount == presetData.Count && (preset.Value.Caliber == caliber || caliber == 99.9f) && preset.Value.Title == RackItem.name)
                {
                    UsablePresets.Add(preset.Key, preset.Value);
                }
            }
            else
            {
                if (preset.Value.ShellCount == presetData.Count && (preset.Value.Caliber == caliber || caliber == 99.9f))
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
            List<string> preset = GetPresetData(true);
            float caliber;
            try
            {
                caliber = float.Parse(RackItem.name.Substring(0, 3));
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
        List<string> presetdata = GetPresetData(false);
        float caliber = float.Parse(caliberid.text);
        int count = int.Parse(countid.text);
        string name = presetname.text;

        ShellsPanelData preset = new ShellsPanelData(RackItem.name, caliber, count, presetdata);
        if (UseDebug)
        {
            Debug.Log($"Data = {RackItem.name}, {preset.Caliber}, {preset.ShellCount}");
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
            Debug.Log("SS ERROR: PRESET ALREADY EXISTS!!!");
            PlayError();
            UniquePrompt.gameObject.SetActive(true);
            return false;
        }
    }

    private List<string> GetPresetData(bool forDisplay)
    {
        List<string> presetData = new List<string>();
        LoadableTubeBehaviour[] tubeBehaviours = RackItem.GetComponentsInChildren<LoadableTubeBehaviour>();
        foreach (LoadableTubeBehaviour T in tubeBehaviours)
        {
            if (T.Shell == null)
            {
                presetData.Add("Empty");
            }
            else
            {
                BaseFireworkBehavior fireworkBehavior;
                if (T.Shell.TryGetComponent(out fireworkBehavior))
                {
                    if (forDisplay) { presetData.Add(fireworkBehavior.EntityDefinition.name);
                    } else presetData.Add(fireworkBehavior.EntityDefinition.Id);
                }
                else {
                    Debug.LogWarning("SS: Failed to Find Firework in Tube Shell");
                }
            }
        }
        return presetData;
    }

    public bool LoadPreset(string preset)
    {
        if (preset == null){ Debug.LogError("SS: PRESET STRING WAS NULL!"); return false; }
        ShellsPanelData data;
        bool loadSuccess = true;
        if (this.PresetLibrary.TryGetValue(preset, out data))
        {
            if (UseDebug) Debug.Log($"Load data = {data}");
            List<string> presData = data.Data;
            LoadableTubeBehaviour[] loadableTubeBehaviours = RackItem.GetComponentsInChildren<LoadableTubeBehaviour>();
            Rigidbody RackBody;
            if (RackItem.TryGetComponent(out RackBody))
            {
                StartCoroutine(Freeze(RackBody));
            }

            int j = 0;
            foreach (LoadableTubeBehaviour T in loadableTubeBehaviours)
            {
                if (T.Shell != null)
                { 
                    Destroy(T.Shell);
                    T.Shell = null;
                }
                TubeIgniteComponent ignite;
                if (T.gameObject.TryGetComponent(out ignite))
                {
                    Destroy(ignite);
                }
                if (j > presData.Count - 1)
                {
                    Debug.Log($"SS ERROR: SIZE OF RACK DIDNT MATCH SIZE OF PRESET, PRESET NAME = {preset}, RACK = {RackItem}");
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
                    if (ShellLibrary.TryGetValue(presData[j], out ItemToSpawn))
                    {
                        T.addShell(ItemToSpawn);
                    }
                    j++;
                }
            }
            if (CloseOnLoad && loadSuccess)
            {
                ToggleShellsCreator();
            }
            else if (loadSuccess)
            {
                PlayClick();
            }
        }
        else
        {
            Debug.Log("SS FATAL ERROR: COULD NOT LOAD PRESET, PRESET NOT FOUND");
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

        Dictionary<string, ShellsPanelData> CurrentPresets = new Dictionary<string, ShellsPanelData>();
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
                foreach (KeyValuePair<string, ShellsPanelData> preset in PresetLibrary)
                {
                    if (preset.Key.Contain(SearchParameter, StringComparison.OrdinalIgnoreCase))
                    {
                        CurrentPresets.Add(preset.Key, preset.Value);
                    }
                }
            }
            else
            {
                foreach (KeyValuePair<string, ShellsPanelData> preset in UsablePresets)
                {
                    if (preset.Key.Contain(SearchParameter, StringComparison.OrdinalIgnoreCase))
                    {
                        CurrentPresets.Add(preset.Key, preset.Value);
                    }
                }
            }

        }


        foreach (KeyValuePair<string, ShellsPanelData> preset in CurrentPresets)
        {
            GameObject Panel = Instantiate(RackPanelPrefab, PresetInventory.transform);
            Panel.name = preset.Key;

            ShellsRackPanel data = Panel.GetComponent<ShellsRackPanel>();
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
        ModPersistentData.SaveString("SSLibrary", json);
    }

    private void PersistentLoadLibrary()
    {
        if (!ModPersistentData.Exists("SSLibrary"))
        {
            string jsontemp = JsonConvert.SerializeObject(PresetLibrary, Formatting.Indented);
            ModPersistentData.SaveString("SSLibrary", jsontemp);
        }
        try
        {
            string json = ModPersistentData.LoadString("SSLibrary", "Error Loading Data");
            if (UseDebug) Debug.Log("Loaded JSON: \n\n" + json + "\n\n");
            Dictionary<string, ShellsPanelData> tempDic = JsonConvert.DeserializeObject<Dictionary<string, ShellsPanelData>>(json);
            if (tempDic != null)
            {
                PresetLibrary = tempDic;
            }
            else
            {
                PresetLibrary = new Dictionary<string, ShellsPanelData>();
            }
        }
        catch (Exception e) {
            Debug.LogException(e);
            Debug.LogError("SS FATAL ERROR: COULD NOT LOAD SSLibrary");
            PlayError();
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
                ToggleShellsCreator();
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

public static class StringExtension
{
    public static bool Contain(this string source, string toCheck, StringComparison comp)
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
