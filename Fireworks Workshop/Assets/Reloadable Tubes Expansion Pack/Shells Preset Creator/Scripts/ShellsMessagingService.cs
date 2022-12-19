using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellsMessagingService : MonoBehaviour
{
    public void OpenCC()
    {
        try
        {
            ShellsCreator ShellsManager = (ShellsCreator)GameObject.FindObjectOfType(typeof(ShellsCreator));
            if (ShellsManager == null)
            {
                Debug.Log("SS: Auto Method Failed Attempting Manual Search!");
                Transform root = this.gameObject.transform.root;
                foreach (Transform T in root)
                {
                    if (T.gameObject.name == "Shells Library Manager")
                    {
                        Debug.Log("Found Shells Manager... Sending Message");
                        T.gameObject.BroadcastMessage("ToggleShellsCreator", this.gameObject, SendMessageOptions.RequireReceiver);
                        ShellsCreator creator = T.gameObject.GetComponent<ShellsCreator>();
                        creator.ToggleShellsCreator(this.gameObject);
                    }
                }
            }
            else {
                ShellsManager.ToggleShellsCreator(this.gameObject);
            }
        }
        catch (System.Exception ex) {
            Debug.LogException(ex);
        }
    }

}
