using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using UMod;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerSettingsUpdater : ModScriptBehaviour
{

    private IModPersistentData PlayerPersistentData;
    [SerializeField]
    private GameObject Manager;
    private GameObject LoadedManager;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void OnModLoaded()
    {
        base.OnModLoaded();
        int Jump = 0;
        int Speed = 0;
        PlayerPersistentData.LoadInt("jumpHeight", Jump);
        PlayerPersistentData.LoadInt("speedValue", Speed);
        if (Jump != 0)
        {
            UpdateSuperJump(Jump);
        }
        if (Speed != 0)
        {
            UpdateSuperSpeed(Speed);
        }
    }

    public void UpdateSuperJump(int jumpHeight)
    {
        FirstPersonController firstPerson = FindObjectOfType<FirstPersonController>();
        if (firstPerson == null) return;
        GameReflector gr = new GameReflector(firstPerson);
        FieldInfo field = gr.GetField("m_JumpSpeed", BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(firstPerson, jumpHeight);
            PlayerPersistentData.SaveInt("jumpHeight", jumpHeight);
            return;
        }
    }

    public void UpdateSuperSpeed(int speed)
    {
        FirstPersonController firstPerson = FindObjectOfType<FirstPersonController>();
        if (firstPerson == null) return;
        GameReflector gr = new GameReflector(firstPerson);
        FieldInfo field = gr.GetField("m_RunSpeed", BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(firstPerson, speed);
            PlayerPersistentData.SaveInt("speedValue", speed);
            return;
        }
    }

}
