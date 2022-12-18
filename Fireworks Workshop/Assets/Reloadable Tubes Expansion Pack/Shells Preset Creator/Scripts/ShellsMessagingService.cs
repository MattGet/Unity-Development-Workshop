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
        if (ShellsManager == null)
        {
            Debug.Log("SS: First Method Failed Trying Again!");
            ShellsManager = (GameObject)GameObject.FindObjectOfType(typeof(ShellsCreator));
        }
        else
        {
            Debug.Log("SS: Found Shells Manager: " + ShellsManager);
        }
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
            ShellsManager = this.gameObject.transform.root.gameObject;
            ShellsManager.BroadcastMessage("ToggleShellsCreator", this.gameObject);
            Debug.LogWarning("SS: Broadcasted Message Instead from: " + ShellsManager);
        }
    }

}
