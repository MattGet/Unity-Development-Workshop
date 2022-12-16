using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellsMessagingService : MonoBehaviour
{
    private GameObject ShellsManager;
    // Start is called before the first frame update
    void Start()
    {
        ShellsManager = GameObject.Find("Shells Library Manager");
    }

    public void OpenCC()
    {
        if (ShellsManager != null)
        {
            ShellsManager.SendMessage("ToggleShellsCreator", this.gameObject);
        }
        else
        {
            Debug.LogError("SHELL RACK FAILED TO SEND MESSAGE AS SHELLS MANAGER WAS NULL");
        }
    }

}
