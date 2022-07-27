using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PanelData
{
    [Header("Data")]
    public string Title;
    public int Caliber;
    public int CandleCount;
    public List<string> Data;

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
}
