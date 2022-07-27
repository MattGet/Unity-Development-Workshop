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
    public CandleCreator Manager;

    public CustomRackPanel(PanelData data)
    {
        this.data.Title = data.Title;
        this.data.Caliber = data.Caliber;
        this.data.CandleCount = data.CandleCount;
        this.data.Data = data.Data;
        GetUI();
        InitializeData();
    }

    private void OnValidate()
    {
        GetUI();
        InitializeData();
    }

    private void GetUI()
    {
        if (TitleBlock == null)
        {
            foreach (Transform T in this.transform)
            {
                if (T.gameObject.name == "Title")
                {
                    TitleBlock = T.gameObject.GetComponent<TMP_Text>();
                }
            }
        }
        if (CaliberBlock == null)
        {
            foreach (Transform T in this.transform)
            {
                if (T.gameObject.name == "Caliber")
                {
                    CaliberBlock = T.gameObject.GetComponent<TMP_Text>();
                }
            }
        }
        if (CountBlock == null)
        {
            foreach (Transform T in this.transform)
            {
                if (T.gameObject.name == "Count")
                {
                    CountBlock = T.gameObject.GetComponent<TMP_Text>();
                }
            }
        }
        if (Manager == null)
        {
            Manager = this.transform.root.gameObject.GetComponent<CandleCreator>();
        }
    }

    private void InitializeData()
    {
        if (TitleBlock != null)
        {
            TitleBlock.text = data.Title;
        }
        if (CaliberBlock != null)
        {
            CaliberBlock.text = $"Caliber = {data.Caliber}mm";
        }
        if (CountBlock != null)
        {
            CountBlock.text = $"Candle Count = {data.CandleCount}";
        }
    }

    public void LOAD()
    {
        Manager.LoadPreset(data.Title);
    }

    public void DELETE()
    {
        Manager.ToggleOnRemoveMenu(data.Title);
    }
}
