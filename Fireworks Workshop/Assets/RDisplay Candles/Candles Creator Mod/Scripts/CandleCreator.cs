using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMod;
using FireworksMania.Core.Definitions.EntityDefinitions;
using FireworksMania.Core.Behaviors.Fireworks;
using FireworksMania.Core.Persistence;
using System.Linq;
using Newtonsoft;

public class CandleCreator : ModScriptBehaviour
{
    private Camera SceneCamera;
    public Dictionary<string, GameObject> CandleLibrary = new Dictionary<string, GameObject>();

    public override void OnModLoaded()
    {
        SceneCamera = Camera.main;
        Debug.Log("\n\nCandle Creator Loaded... Waiting For Initialization\n\n");
    }

    IEnumerator wait()
    {
        yield return new WaitUntil(() => SceneCamera.enabled);
        InitializeDictionary();
    }

    private void InitializeDictionary()
    {
        CandleLibrary.Clear();

        List<BaseInventoryEntityDefinition> definitions = FindObjectsOfType<BaseInventoryEntityDefinition>().ToList();
        foreach (BaseInventoryEntityDefinition def in definitions)
        {
            if (def.EntityDefinitionType.Id == "Fireworks_Novelty")
            {
                if (!CandleLibrary.ContainsKey(def.Id))
                {
                    CandleLibrary.Add(def.Id, def.PrefabGameObject);
                }
            }
        }

        Debug.Log("\n\nInitialized Candle Dictionary\n\n");
        foreach (KeyValuePair<string, GameObject> kvp in CandleLibrary)
        {
            Debug.Log($"\t{kvp.Key}, {kvp.Value.name}");
        }
        Debug.Log("\n\nFinished Candle Dictionary Readout\n\n");
    }
}
