using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMod;

public class ModScriptLoader : ModScriptBehaviour
{
    public GameObject ScriptToLoad;
    private GameObject InitializedCopy;

    public override void OnModLoaded()
    {
        base.OnModLoaded();
        InitializedCopy = GameObject.Instantiate(ScriptToLoad);
    }

    public override void OnModUnload()
    {
        base.OnModUnload();
        Object.Destroy(InitializedCopy);
    }
}
