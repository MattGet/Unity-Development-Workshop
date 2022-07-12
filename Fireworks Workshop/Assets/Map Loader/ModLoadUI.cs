using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMod;

public class ModLoadUI : ModScriptBehaviour
{
    private bool UILoaded = false;
    public GameObject UIGameObject;
    public override void OnModLoaded()
    {
        base.OnModLoaded();

    }

    public override void OnModUpdate()
    {
        base.OnModUpdate();
        if (UILoaded == false)
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                Instantiate(UIGameObject);
                UIGameObject.SetActive(true);
                UILoaded = true;
            }
        }
    }
}
