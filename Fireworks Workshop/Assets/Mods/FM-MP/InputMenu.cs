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
    public Button Back10;
    public Button Forward10;
    public Toggle loop;
    public Sprite loopoff;
    public Sprite loopon;
    public Slider volume;
    public Slider VidTime;
    public GameObject ScrubMissing;
    public TMP_Text TimeValue;
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
        Back10.onClick.AddListener(CBack);
        Forward10.onClick.AddListener(CForward);
        Quality.onValueChanged.AddListener(CQuality);

        videocontroller.VideoPrepared.AddListener(StartTime);

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
        if (videocontroller.player.isPrepared || videocontroller.isPlaying)
        {
            StartTime();
        }

        CQuality(Quality.value);
    }

    public void StartTime()
    {
        if (videocontroller.player != null)
        {
            Debug.Log($"Starting Time Counter max time = {videocontroller.player.length}");
            VidTime.maxValue = (float)videocontroller.player.length;
            StartCoroutine(updatetime());
        }
    }

    public void CQuality(int id)
    {
        videocontroller.Quality(Quality.value);
        if (Quality.value == 3)
        {
            Debug.Log("FMVP: Disabling Video Scrubbing, Not Available on UHD Qualiy");
            VidTime.gameObject.SetActive(false);
            Back10.gameObject.SetActive(false);
            Forward10.gameObject.SetActive(false);
            ScrubMissing.SetActive(true);
        }
        else if (!VidTime.IsActive())
        {
            Debug.Log("Re Enabling Video Scrubbing");
            VidTime.gameObject.SetActive(true);
            Back10.gameObject.SetActive(true);
            Forward10.gameObject.SetActive(true);
            ScrubMissing.SetActive(false);
        }
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

    public void CBack()
    {
        if (videocontroller == null)
        {
            return;
        }
        if (videocontroller.videoQuality == VideoQuality.UltraHighQuality)
        {
            return;
        }
        Playclick();
        videocontroller.player.time = videocontroller.player.time - 10;
    }

    public void CForward()
    {
        if (videocontroller == null)
        {
            return;
        }
        if (videocontroller.videoQuality == VideoQuality.UltraHighQuality)
        {
            return;
        }
        Playclick();
        videocontroller.player.time = videocontroller.player.time + 10;
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

    private IEnumerator updatetime()
    {
        do
        {
            VidTime.SetValueWithoutNotify((float)videocontroller.player.time);
            TimeValue.text = SecToMinString(videocontroller.player.time);
            yield return new WaitForSeconds(1);
        } while (whiledisplay);
        yield break;
    }

    private string SecToMinString(double seconds)
    {
        string Time = "";
        TimeSpan t = TimeSpan.FromSeconds(seconds);
        if (t.Hours != 0)
        {
            Time = $"{t.Hours}:{t.Minutes.ToString("00")}:{t.Seconds.ToString("00")}";
            return Time;
        }
        if (t.Hours == 0)
        {
            Time = $"{t.Minutes.ToString("00")}:{t.Seconds.ToString("00")}";
            return Time;
        }
        if (t.Minutes == 0)
        {
            Time = $"{seconds.ToString("00")}";
            return Time;
        }
        Time = $"{t.Minutes.ToString("00")}:{t.Seconds.ToString("00")}";
        return Time;
    }

    public void CScrubber(float time)
    {
        if (videocontroller == null)
        {
            return;
        }
        if (videocontroller.videoQuality == VideoQuality.UltraHighQuality)
        {
            return;
        }
        if (videocontroller.player.isPrepared || videocontroller.player.isPlaying || videocontroller.player.isPaused)
        {
            videocontroller.player.time = time;
            Debug.Log($"Setting time to: {time}");
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
        Back10.onClick.RemoveAllListeners();
        Forward10.onClick.RemoveAllListeners();
        loop.onValueChanged.RemoveAllListeners();
        if (videocontroller != null)
        {
            videocontroller.VideoPrepared.RemoveListener(StartTime);
        }
        gameObject.SetActive(false);
    }
}
