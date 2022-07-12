using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMod;

public class MapLoad : ModScriptBehaviour,
    IMod
{
    override public void OnModLoaded()
    {
        Vector3 pos = new Vector3(-26f, -1.5f, 6.0f);
        Quaternion rot = new Quaternion(0, 0, 0, 1);
        tplayer(pos);
        
        Rigidbody map = this.GetComponent<Rigidbody>();
        map.isKinematic = true;
        map.constraints = RigidbodyConstraints.FreezeAll;
        Instantiate(this, pos, rot);
    }
    public void tplayer(Vector3 pos)
    {

        foreach (Rigidbody obj in FindObjectsOfType<Rigidbody>())
        {
            if (obj.tag == "MainCamera")
            {
                obj.MovePosition(pos);
            }
        }
    }
}

