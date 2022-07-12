
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using FireworksMania.Core.Definitions;
using FireworksMania.Core.Messaging;
public class AudioInputMenu : MonoBehaviour
{
    public FmAudioPlayer audiocontroller;
    public Button cancel;
    public Button OK;
    public Button Paste;
    public Button Help;
    public TMP_InputField inputField;
    public TMP_Dropdown Quality;
    public Button play;
    public Button pause;
    public Button stop;
    public Toggle loop;
    public Toggle threeD;
    public GameObject D3Menu;
    public Sprite D3On;
    public Sprite D3Off;
    public TMP_Text Strength;
    public Slider SS;
    public TMP_Text MinValue;
    public Slider SMin;
    public TMP_Text MaxValue;
    public Slider SMax;
    public Sprite loopoff;
    public Sprite loopon;
    public Slider volume;
    public Slider Scrubbing;
    public TMP_Text TimeValue;
    private bool whiledisplay = false;
    public AudioClip clicks;
    public AudioClip errors;
    public AudioSource audioplay;
    public float volnumb = 1;
    [HideInInspector]
    public bool use3D = false;
    [SerializeField]
    private string URL;

    private void Awake()
    {
        Hide();
    }



    public void Update()
    {
        if (whiledisplay && Cursor.visible == false || Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void Playclick()
    {
        if (!audioplay.gameObject.activeInHierarchy) return;
        audioplay.clip = clicks;
        audioplay.Play();
    }
    public void Playerror()
    {
        if (!audioplay.gameObject.activeInHierarchy) return;
        audioplay.clip = errors;
        audioplay.Play();
    }

    public void Show(string inputtext)
    {
        gameObject.SetActive(true);
        inputField.text = inputtext;
        whiledisplay = true;

        OK.onClick.AddListener(OKClick);
        cancel.onClick.AddListener(CancelClick);
        Paste.onClick.AddListener(PasteClick);
        play.onClick.AddListener(CPlay);
        pause.onClick.AddListener(CPause);
        stop.onClick.AddListener(CStop);
        loop.onValueChanged.AddListener(CLoop);
        Scrubbing.onValueChanged.AddListener(CScrubber);
        threeD.onValueChanged.AddListener(C3D);
        Help.onClick.AddListener(OpenURL);

        Messenger.Broadcast(new MessengerEventChangeUIMode(true, false));

        if (audiocontroller.Player.loop)
        {
            loop.image.sprite = loopon;
        }
        else
        {
            loop.image.sprite = loopoff;
        }
        volume.value = volnumb;
        volume.onValueChanged.AddListener(Svolume);
        StartTime();
        SS.value = audiocontroller.Player.spatialBlend;
        SMax.value = audiocontroller.Player.maxDistance;
        SMin.value = audiocontroller.Player.minDistance;
        D3Strenght(audiocontroller.Player.spatialBlend);
        D3MaxValue(audiocontroller.Player.maxDistance);
        D3MinValue(audiocontroller.Player.minDistance);
        if (use3D)
        {
            D3Menu.SetActive(true);
            threeD.image.sprite = D3On;
        }
        else
        {
            D3Menu.SetActive(false);
            threeD.image.sprite = D3Off;
        }
    }

    public void StartTime()
    {
        if (audiocontroller.Player.clip != null)
        {
            Scrubbing.maxValue = audiocontroller.Player.clip.length;
            StartCoroutine(updatetime());
        }
    }

    private void OpenURL()
    {
        Application.OpenURL(URL);
    }


    private IEnumerator updatetime()
    {
        do
        {
            Scrubbing.value = audiocontroller.Player.time;
            TimeValue.text = $"{audiocontroller.Player.time.ToString("000")}/{audiocontroller.Player.clip.length.ToString("000")} Seconds";
            yield return new WaitForSeconds(1);
        } while (whiledisplay);
        yield break;
    }

    public void Svolume(float value)
    {
        //Debug.Log("New Slider Value " + value);
        audiocontroller.Player.volume = value;
        volnumb = value;
        //Debug.Log("confirmedslidervol = " + audiocontroller.player.GetDirectAudioVolume(0) + "/" + volnumb);
    }

    public void CLoop(bool value)
    {
        if (audiocontroller != null)
        {
            if (value)
            {
                Playclick();
                loop.image.sprite = loopon;
                Debug.Log("Set to loop");
                audiocontroller.Player.loop = true;

            }
            else
            {
                Playclick();
                loop.image.sprite = loopoff;
                Debug.Log("Stopped looping");
                audiocontroller.Player.loop = false;
            }
        }
        else
        {
            Playerror();
        }
    }

    public void C3D(bool value)
    {
        if (audiocontroller != null)
        {
            if (value)
            {
                Playclick();
                threeD.image.sprite = D3On;
                Debug.Log("Set to loop");
                D3Menu.SetActive(true);
                use3D = true;

            }
            else
            {
                Playclick();
                threeD.image.sprite = D3Off;
                Debug.Log("Stopped 3D audio");
                audiocontroller.Player.spatialBlend = 0;
                D3Menu.SetActive(false);
                use3D = false;
            }
        }
        else
        {
            Playerror();
        }
    }

    public void D3Strenght(float str)
    {
        audiocontroller.Player.spatialBlend = str;
        Strength.text = (str * 100).ToString("00") + "%";
    }
    public void D3MinValue(float str)
    {
        audiocontroller.Player.minDistance = str;
        MinValue.text = str.ToString("0000");
    }
    public void D3MaxValue(float str)
    {
        audiocontroller.Player.maxDistance = str;
        MaxValue.text = str.ToString("0000");
    }


    public void CScrubber(float time)
    {
        audiocontroller.Player.time = time;
    }

    


    public void CPlay()
    {
        if (audiocontroller != null)
        {
            if (!audiocontroller.Player.isPlaying && !audiocontroller.isPlaying)
            {
                Debug.Log("Force Starting");
                audiocontroller.Format(Quality.value);
                audiocontroller.forcestartvideo(inputField.text);
                Playclick();
            }
            else if (!audiocontroller.Player.isPlaying && audiocontroller.isPlaying)
            {
                audiocontroller.Player.UnPause();
                audiocontroller.isPlaying = true;
                Playclick();
            }
            else
            {
                Playerror();
            }
        }
        else
        {
            Playerror();
        }
    }
    public void CStop()
    {
        if (audiocontroller != null)
        {
            if (audiocontroller.Player.isPlaying)
            {
                Playclick();
                audiocontroller.Stop();
                audiocontroller.onlytriggeronce = false;
                audiocontroller.isPlaying = false;
            }
            else
            {
                Playerror();
            }

        }
        else
        {
            Playerror();
        }
    }
    public void CPause()
    {
        if (audiocontroller != null)
        {
            if (audiocontroller.isPlaying && audiocontroller.Player.isPlaying)
            {
                audiocontroller.Player.Pause();
                Playclick();
            }
            else if (!audiocontroller.Player.isPlaying && audiocontroller.isPlaying)
            {
                audiocontroller.Player.UnPause();
                Playclick();
            }
            else
            {
                Playerror();
            }

        }
        else
        {
            Playerror();
        }
    }


    public void PasteClick()
    {
        inputField.text = GUIUtility.systemCopyBuffer;
        Playclick();
    }

    public void OKClick()
    {
        Playclick();
        Hide();
        if (audiocontroller.isPlaying || audiocontroller.Player.isPlaying)
        {
            return;
        }
        audiocontroller.Format(Quality.value);
        audiocontroller.OK(inputField.text);

    }
    public void CancelClick()
    {
        Playclick();
        audiocontroller.Cancel();
        Hide();
    }

    public void Hide()
    {
        whiledisplay = false;
        Messenger.Broadcast(new MessengerEventChangeUIMode(false, true));

        OK.onClick.RemoveAllListeners();
        cancel.onClick.RemoveAllListeners();
        Paste.onClick.RemoveAllListeners();
        play.onClick.RemoveAllListeners();
        pause.onClick.RemoveAllListeners();
        stop.onClick.RemoveAllListeners();
        loop.onValueChanged.RemoveAllListeners();
        Scrubbing.onValueChanged.RemoveAllListeners();
        threeD.onValueChanged.RemoveAllListeners();
        Help.onClick.RemoveAllListeners();

        gameObject.SetActive(false);
    }
}