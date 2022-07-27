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
    }

    public void OpenCC()
    {
        if (CandleManager != null)
        {
            CandleManager.SendMessage("ToggleCandleCreator", this.gameObject);
        }
        else
        {
            Debug.LogError("CANDLE RACK FAILED TO SEND MESSAGE AS CANDLE MANAGER WAS NULL");
        }
    }

}
