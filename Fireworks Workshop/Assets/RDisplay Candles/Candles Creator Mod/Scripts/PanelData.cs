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
}
