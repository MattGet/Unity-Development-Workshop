using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelData : MonoBehaviour
{
    [Header("Data")]
    public string Title = "Default Block Title";
    public int Caliber = 30;
    public int CandleCount = 5;
    public List<string> Data = new List<string>();

    public PanelData(string title, int caliber, int candleCount, List<string> data)
    {
        this.Title = title;
        this.Caliber = caliber;
        this.CandleCount = candleCount;
        this.Data = data;
    }

    public PanelData(PanelData data)
    {
        this.Title = data.Title;
        this.Caliber = data.Caliber;
        this.CandleCount = data.CandleCount;
        this.Data = data.Data;
    }

    public PanelData()
    {
        Title = "Default Block Title";
        Caliber = 30;
        CandleCount = 5;
        Data = new List<string>();
    }
}
