using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using FireworksMania.Core.Definitions;
using FireworksMania.Core.Messaging;

public class InputMenu : MonoBehaviour
{

    public Button cancel;
    public Button OK;
    public Button Paste;
    public TMP_InputField inputField;
    public TMP_Dropdown Quality;
    public Button play;
    public Button pause;
    public Button stop;
    public Button Back;
    public Button Forward;
    public Sprite forward0;
    public Sprite forward1;
    public Sprite forward2;
    private int forwardnumb;
    public Toggle loop;
    public Sprite loopoff;
    public Sprite loopon;
    public Slider volume;
    private StartVideo videocontroller;
    private bool whiledisplay = false;
    public AudioClip clicks;
    public AudioClip errors;
    public AudioSource audioplay;
    public float volnumb = 1;

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

    public void Show(string inputtext, StartVideo start)
    {
        gameObject.SetActive(true);
        inputField.text = inputtext;
        whiledisplay = true;
        videocontroller = start;

        OK.onClick.AddListener(OKClick);
        cancel.onClick.AddListener(CancelClick);
        Paste.onClick.AddListener(PasteClick);
        play.onClick.AddListener(CPlay);
        pause.onClick.AddListener(CPause);
        stop.onClick.AddListener(CStop);
        loop.onValueChanged.AddListener(CLoop);
        Back.onClick.AddListener(CBack);
        Forward.onClick.AddListener(CForward);

        Messenger.Broadcast(new MessengerEventChangeUIMode(true, false));

        if (videocontroller.player.isLooping)
        {
            loop.image.sprite = loopon;
        }
        else
        {
            loop.image.sprite = loopoff;
        }
        volume.value = volnumb;
        volume.onValueChanged.AddListener(Svolume);
    }


    public void Svolume(float value)
    {
        //Debug.Log("New Slider Value " + value);
        videocontroller.player.SetDirectAudioVolume(0, value);
        videocontroller.audioSource.SetDirectAudioVolume(0, value);
        volnumb = value;
        //Debug.Log("confirmedslidervol = " + videocontroller.player.GetDirectAudioVolume(0) + "/" + volnumb);
    }

    public void CLoop(bool value)
    {
        if (videocontroller != null)
        {
            if (value)
            {
                Playclick();
                loop.image.sprite = loopon;
                //Debug.Log("Set to loop");
                videocontroller.player.isLooping = true;
                videocontroller.audioSource.isLooping = true;
            }
            else
            {
                Playclick();
                loop.image.sprite = loopoff;
                //Debug.Log("Stopped looping");
                videocontroller.player.isLooping = false;
                videocontroller.audioSource.isLooping = false;
            }
        }
        else
        {
            Playerror();
        }
    }

    public void CForward()
    {
        if (videocontroller != null)
        {
            if (videocontroller.player.isPlaying || videocontroller.player.isPaused)
            {
                //Debug.Log("Force Starting");
                videocontroller.Seek(1);
                if (forwardnumb == 0)
                {
                    Forward.image.sprite = forward1;
                }
                if (forwardnumb == 1)
                {
                    Forward.image.sprite = forward2;
                }
                if (forwardnumb == 2)
                {
                    Forward.image.sprite = forward0;
                    forwardnumb = 0;
                }
                else
                {
                    forwardnumb += 1;
                }
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
  
    public void CBack()
    {
        if (videocontroller != null)
        {
            if (videocontroller.isSeeking)
            {
                //Debug.Log("Force Starting");
                videocontroller.Seek(0);
                    Forward.image.sprite = forward0;
                    forwardnumb = 0;
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
    public void CPlay()
    {
        if (videocontroller != null)
        {
            if (!videocontroller.player.isPlaying && !videocontroller.player.isPaused)
            {
                //Debug.Log("Force Starting");
                videocontroller.Quality(Quality.value);
                videocontroller.forcestartvideo(inputField.text);
                Playclick();
            }
            else if (videocontroller.player.isPaused)
            {
                videocontroller.player.Play();
                if (videocontroller.requireaudio)
                {
                    //Debug.Log("Starting from pause");
                    videocontroller.audioSource.Play();
                    Playclick();
                }
                videocontroller.isPlaying = true;
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
        if (videocontroller != null)
        {
            if (videocontroller.player.isPlaying || videocontroller.player.isPaused)
            {

                Playclick();
                videocontroller.Stop();
                videocontroller.onlytriggeronce = false;
                videocontroller.isPlaying = false;
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
        if (videocontroller != null)
        {
            if (videocontroller.player.isPlaying && !videocontroller.player.isPaused)
            {
                videocontroller.player.Pause();
                videocontroller.player.playbackSpeed = 1;
                videocontroller.isSeeking = false;
                Playclick();
                if (videocontroller.requireaudio || videocontroller.audioSource.isPlaying)
                {
                    videocontroller.audioSource.Pause();
                    videocontroller.audioSource.playbackSpeed = 1;
                }
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
        if (videocontroller.isPlaying || videocontroller.player.isPlaying || videocontroller.player.isPaused)
        {
            return;
        }
        videocontroller.OK(inputField.text);
        videocontroller.Quality(Quality.value);
    }
    public void CancelClick()
    {
        Playclick();
        videocontroller.Cancel();
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
        Back.onClick.RemoveAllListeners();
        Forward.onClick.RemoveAllListeners();

        gameObject.SetActive(false);
    }
}
