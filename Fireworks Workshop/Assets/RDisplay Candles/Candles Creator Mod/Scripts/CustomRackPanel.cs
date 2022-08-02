using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CustomRackPanel: MonoBehaviour
{
    public PanelData data;
    [Header("UI Objects")]
    public TMP_Text TitleBlock;
    public TMP_Text CaliberBlock;
    public TMP_Text CountBlock;
    public GameObject Manager;

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
            Manager = GameObject.Find("Candle Library Manager");
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
            CaliberBlock.text = $"Caliber = {data.Caliber}mm";
        }
        if (CountBlock != null)
        {
            CountBlock.text = $"Rack Size = {data.CandleCount}";
        }
    }

    public void LOAD()
    {
        try
        {
            FindManger();
            Debug.Log($"Sending Load Request For {this.gameObject.name}");
            Manager.SendMessage("LoadPreset", this.gameObject.name);
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
            Manager.SendMessage("ToggleOnRemoveMenu", this.gameObject.name);
        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex);
        }
    }
}
