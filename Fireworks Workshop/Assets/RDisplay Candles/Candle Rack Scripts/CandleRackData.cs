using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FireworksMania.Core.Persistence;
using FireworksMania.Core.Behaviors.Fireworks;
using System.Linq;

public class CandleRackData : MonoBehaviour, ISaveableComponent
{
    [HideInInspector]
    public string SaveableComponentTypeId => "CandleRackData";
    public List<CandleManager> Managers = new List<CandleManager>();

    public CustomEntityComponentData CaptureState()
    {
        List<bool> hasCandle = new List<bool>();
        foreach (CandleManager manager in Managers)
        {
            if (manager.HasCandle)
            {
                hasCandle.Add(true);
            }
            else
            {
                hasCandle.Add(false);
            }
        }

        CustomEntityComponentData entityComponentData = new CustomEntityComponentData();
        entityComponentData.Add<List<bool>>("candles", hasCandle);

        return entityComponentData;
    }

    public void RestoreState(CustomEntityComponentData customComponentData)
    {
        List<bool> candles = customComponentData.Get<List<bool>>("candles");
        for (int i = 0; i <= Managers.Count - 1; i++)
        {
            Managers[i].IsBluePrint = candles[i];
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    private void OnValidate()
    {
        Managers.Clear();
        Managers = this.gameObject.GetComponentsInChildren<CandleManager>().ToList();
    }

    void Awake()
    {
        Managers.Clear();
        Managers = this.gameObject.GetComponentsInChildren<CandleManager>().ToList();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
