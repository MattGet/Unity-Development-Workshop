using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CustomRackPanel : PanelData
{
    [Header("UI Objects")]
    public TMP_Text TitleBlock;
    public TMP_Text CaliberBlock;
    public TMP_Text CountBlock;
    public CandleCreator Manager;

    public CustomRackPanel(PanelData data)
    {
        this.Title = data.Title;
        this.Caliber = data.Caliber;
        this.CandleCount = data.CandleCount;
        Data = data.Data;
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
            TitleBlock.text = Title;
        }
        if (CaliberBlock != null)
        {
            CaliberBlock.text = $"Caliber = {Caliber}mm";
        }
        if (CountBlock != null)
        {
            CountBlock.text = $"Candle Count = {CandleCount}";
        }
    }

    public void LOAD()
    {
        Manager.LoadPreset(Title);
    }

    public void DELETE()
    {
        Manager.ToggleOnRemoveMenu(Title);
    }
}
