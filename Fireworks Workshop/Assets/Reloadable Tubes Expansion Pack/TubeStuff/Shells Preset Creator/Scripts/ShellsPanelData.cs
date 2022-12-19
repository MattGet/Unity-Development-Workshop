using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ShellsPanelData
{
    [Header("Data")]
    public string Title;
    public float Caliber;
    public int ShellCount;
    public List<string> Data;

    public ShellsPanelData(string title, float caliber, int shellCount, List<string> data)
    {
        this.Title = title;
        this.Caliber = caliber;
        this.ShellCount = shellCount;
        this.Data = data;
    }

    public ShellsPanelData(ShellsPanelData data)
    {
        this.Title = data.Title;
        this.Caliber = data.Caliber;
        this.ShellCount = data.ShellCount;
        this.Data = data.Data;
    }
}
