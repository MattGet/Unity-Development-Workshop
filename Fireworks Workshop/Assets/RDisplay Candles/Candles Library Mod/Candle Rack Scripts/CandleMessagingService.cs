using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandleMessagingService : MonoBehaviour
{
    private GameObject CandleManager;
    // Start is called before the first frame update
    void Start()
    {
        CandleManager = GameObject.Find("Candle Library Manager");
        if (CandleManager == null) {
            Debug.Log("SS: First Method Failed Trying Again!");
            CandleManager = (GameObject)GameObject.FindObjectOfType(typeof(ShellsCreator));
        }
    }

    public void OpenCC()
    {
        if (CandleManager != null)
        {
            CandleManager.SendMessage("ToggleShellsCreator", this.gameObject);
        }
        else
        {
            Debug.LogError("CANDLE RACK FAILED TO SEND MESSAGE AS CANDLE MANAGER WAS NULL");
        }
    }

}
