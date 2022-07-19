using System.Collections;
using UnityEngine;
using FireworksMania.Core.Behaviors.Fireworks.Parts;
using UnityEngine.Video;
using UnityEngine.Networking;
using UnityEngine.Events;

[RequireComponent(typeof(AudioSource))]
public class FmAudioPlayer : MonoBehaviour
{
    public AudioSource Player;
    public string AudioURL = "Enter Audio File Path Here";

    public bool playOnStart = false;
    private string audioUrl;

    private bool ratelimit = false;
    public AudioInputMenu menu;
    private bool isdisplayed = false;
    [HideInInspector]
    public bool onlytriggeronce = false;
    private string VideoID;
    [HideInInspector]
    public UnityEvent<string> gotURL;
    [HideInInspector]
    public bool isPlaying = false;
    //private bool Isloaded = false;
    private AudioClip FMAudio;
    public AudioType Atype = AudioType.MPEG;

    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(Startvideo());
        if (playOnStart == true)
        {
            StartCoroutine(Startvideo(true));
        }

    }

    private void OnValidate()
    {
        if (menu == null)
        {
            Debug.LogError("FMAP: AudioPlayer Requires an Audio Input Menu to Work!");
        }
        else Debug.ClearDeveloperConsole();

        if (AudioURL == "")
        {
            AudioURL = "FMAP: Enter Audio URL Here";
        }
        if (Player == null)
        {
            if (!this.gameObject.TryGetComponent(out Player))
            {
                Debug.Log("FMAP: Couldnt Find Audio Source Please Assign One!");
            }
        }
    }

    public void OnButtonPress()
    {

        if (ratelimit == false && isdisplayed == false)
        {
            ratelimit = true;
            menu.Show(AudioURL);
            isdisplayed = true;
            StartCoroutine(wait());
        }
        if (ratelimit == false && isdisplayed == true)
        {
            ratelimit = true;
            menu.CancelClick();
            isdisplayed = false;
            StartCoroutine(wait());
        }

    }

    IEnumerator wait()
    {
        yield return new WaitForSecondsRealtime(0.25f);
        ratelimit = false;
    }

    public void OK(string input)
    {
        AudioURL = input;
        isdisplayed = false;
        StartCoroutine(Startvideo(false));
    }
    public void Cancel()
    {
        isdisplayed = false;
    }

    public void forcestartvideo(string url)
    {
        AudioURL = url;
        Debug.Log("FMAP: force playing " + url);
        StartCoroutine(Startvideo(true));
    }


    public void PlayAudio()
    {
        //Debug.Log("playing started");
        if (onlytriggeronce == false)
        {
            Debug.Log("FMAP: Playing audio");
            Player.Play();
            isPlaying = true;
        }
    }

    public void Stop()
    {
        Debug.Log("FMAP: Stopping Audio");
        Player.Stop();
        isPlaying = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Format(int input)
    {
        switch (input)
        {
            case 0:
                Atype = AudioType.MPEG;
                break;
            case 1:
                Atype = AudioType.WAV;
                break;
            case 2:
                Atype = AudioType.OGGVORBIS;
                break;
        }
    }


    IEnumerator Startvideo(bool andplay)
    {
        if (AudioURL == "Enter Video URL Here")
        {
            Debug.LogWarning("FMVP: Attempted to start video without a URL input!!!");
            yield break;
        }
        Debug.Log("Getting File Path");
        GetFilePath();
        Debug.Log($"File Path = {AudioURL}");
        yield return StartCoroutine(LoadClip(AudioURL));
        if (FMAudio != null && Player != null)
        {
            Debug.Log("Set Audio Clip");
            Player.clip = FMAudio;
        }
        if (andplay)
        {
            Debug.Log("Playing Audio");
            Player.Play();
            isPlaying = true;
            if (isdisplayed)
            {
                menu.StartTime();
            }
        }
    }

    private void GetFilePath()
    {
        if (AudioURL.Contains("\"C:"))
        {
            int start = AudioURL.IndexOf("\"");
            int end = AudioURL.LastIndexOf("\"");
            string result = AudioURL.Substring(start + 1, end - 1);
            AudioURL = "file://" + result;
            Debug.Log("FMAP: Copied File Path Detected attempting to play Audio from file");
        }
        else if (AudioURL.StartsWith("file://"))
        {
            Debug.Log("FMAP: File Path Detected attempting to play audio from file");
        }
        else if (AudioURL.Contains("C:"))
        {
            AudioURL = "file://" + AudioURL;
            Debug.Log("FMAP: File Detected attempting to play audio from file");
        }
    }


    private IEnumerator  LoadClip(string path)
    {
        AudioClip clip = null;
        using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(path, Atype))
        {
            uwr.SendWebRequest();
            yield return new WaitUntil(() => uwr.isDone);
            // wrap tasks in try/catch, otherwise it'll fail silently
            try
            {
                if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError || uwr.result == UnityWebRequest.Result.DataProcessingError) Debug.Log($"{uwr.error}");
                else
                {
                    clip = DownloadHandlerAudioClip.GetContent(uwr);
                    FMAudio = clip;
                }
            }
            catch (System.Exception err)
            {
                Debug.Log($"{err.Message}, {err.StackTrace}");
            }
        }
        yield break;
    }
}
