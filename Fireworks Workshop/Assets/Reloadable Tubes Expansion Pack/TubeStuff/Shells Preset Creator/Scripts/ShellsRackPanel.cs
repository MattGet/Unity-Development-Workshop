using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShellsRackPanel: MonoBehaviour
{
    public ShellsPanelData data;
    [Header("UI Objects")]
    public TMP_Text TitleBlock;
    public TMP_Text CaliberBlock;
    public TMP_Text CountBlock;
    public GameObject Manager;
    private ShellsCreator creator;
    public string Name = "Shells Library Manager 2";

    private void OnValidate()
    {
        GetUI();
        InitializeData();
    }

    public void GetUI()
    {
        if (TitleBlock == null)
        {
            foreach (Transform T in this.gameObject.transform)
            {
                if (T.gameObject.name == "Title")
                {
                    TitleBlock = T.gameObject.GetComponent<TMP_Text>();
                }
            }
        }
        if (CaliberBlock == null)
        {
            foreach (Transform T in this.gameObject.transform)
            {
                if (T.gameObject.name == "Caliber")
                {
                    CaliberBlock = T.gameObject.GetComponent<TMP_Text>();
                }
            }
        }
        if (CountBlock == null)
        {
            foreach (Transform T in this.gameObject.transform)
            {
                if (T.gameObject.name == "Count")
                {
                    CountBlock = T.gameObject.GetComponent<TMP_Text>();
                }
            }
        }
        FindManger();
    }

    private void FindManger()
    {
        if (Manager == null)
        {
            Manager = GameObject.Find(Name);
        }
    }

    public void InitializeData()
    {
        if (TitleBlock != null)
        {
            TitleBlock.text = this.gameObject.name;
        }
        if (CaliberBlock != null)
        {
            CaliberBlock.text = $"Caliber = {data.Caliber} Inch";
        }
        if (CountBlock != null)
        {
            CountBlock.text = $"Rack Size = {data.ShellCount}";
        }
    }

    public void LOAD()
    {
        try
        {
            FindManger();
            Debug.Log($"Sending Load Request For {this.gameObject.name}");
            if (Manager.TryGetComponent<ShellsCreator>(out creator))
            {
                creator.LoadPreset(this.gameObject.name);
            }
            else {
                Manager.SendMessage("LoadPreset", this.gameObject.name);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex);
        }


    }

    public void DELETE()
    {
        try
        {
            FindManger();
            if (Manager.TryGetComponent<ShellsCreator>(out creator))
            {
                creator.ToggleOnRemoveMenu(this.gameObject.name);
            }
            else
            {
                Manager.SendMessage("ToggleOnRemoveMenu", this.gameObject.name);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex);
        }
    }
}
