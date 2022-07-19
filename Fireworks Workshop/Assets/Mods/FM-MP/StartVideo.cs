using System.Collections;
using UnityEngine;
using FireworksMania.Core.Behaviors.Fireworks.Parts;
using UnityEngine.Video;
using UnityEngine.Networking;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine.Events;


public class StartVideo : MonoBehaviour
{
    public GameObject Screen;
    public string VideoURL = "Enter Video URL Here";
    public VideoPlayer player;
    public VideoPlayer audioSource;

    public bool playOnStart = false;
    public bool generateUsingClient;

    private string audioUrl;
    private string videoUrl;
    public VideoQuality videoQuality = VideoQuality.AUTOMATIC;
    public VideoFormatType videoFormat = VideoFormatType.MP4;

    private bool ratelimit = false;
    public InputMenu menu;
    private bool isdisplayed = false;
    [HideInInspector]
    public bool onlytriggeronce = false;
    private string VideoID;
    private static string audiostring = "\"itag\": 18";
    [HideInInspector]
    public bool requireaudio = false;
    private bool isVideoUrl = false;
    [HideInInspector]
    public UnityEvent<string> gotURL;
    [HideInInspector]
    public bool isPlaying;
    //private bool Isloaded = false;
    public Material errormat;
    [HideInInspector]
    public bool isSeeking = false;
    public UnityEvent VideoPrepared;

    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(Startvideo());
        if (playOnStart == true)
        {
            StartCoroutine(Startvideo(true));
        }
#if UNITY_EDITOR
        menu.Show(VideoURL, this);
#endif
    }

    private void OnValidate()
    {
        if (menu == null)
        {
            Debug.LogError("FMVP: VideoPlayer Requires an Input Menu to Work!");
        }
        else Debug.ClearDeveloperConsole();

        if (VideoURL == "")
        {
            VideoURL = "FMVP: Enter Video URL Here";
        }
    }

    public void OnButtonPress()
    {
        
            if (ratelimit == false && isdisplayed == false)
            {
                ratelimit = true;
                menu.Show(VideoURL, this);
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
        VideoURL = input;
        isdisplayed = false;
        StartCoroutine(Startvideo(false));
    }
    public void Quality(int input)
    {
        switch (input)
        {
            case 0:
                videoQuality = VideoQuality.AUTOMATIC;
                break;
            case 1:
                videoQuality = VideoQuality.LowQuality;
                break;
            case 2:
                videoQuality = VideoQuality.HighQuality;
                break;
            case 3:
                videoQuality = VideoQuality.UltraHighQuality;
                break;
            default:
                videoQuality = VideoQuality.AUTOMATIC;
                break;
        }
    }
    public void Cancel()
    {
        isdisplayed = false;
    }

    public void forcestartvideo(string url)
    {
        VideoURL = url;
        Debug.Log("FMVP: force playing " + url);
        StartCoroutine(Startvideo(true));
    }

    IEnumerator Startvideo(bool andplay)
    {
        player.url = "";
        audioSource.url = "";
        requireaudio = false;
        if (VideoURL == "Enter Video URL Here")
        {
            Debug.LogWarning("FMVP: Attempted to start video without a URL input!!!");
            yield break;
        }
        if (generateUsingClient == true)
        {
            Debug.Log("FMVP: Generating link with client");
            yield return StartCoroutine(VideoGenerateUrlUsingClient());
        }
        Debug.Log("FMVP: Preparing Video");
        //Debug.Log("Playing URL: " + videoUrl);
        player.url = videoUrl;
        player.Prepare();
        player.errorReceived += VideoPlayer_errorReceived;
        if (requireaudio == true)
        {
            Debug.Log("FMVP: Preparing Audio");
            audioSource.url = audioUrl;
            audioSource.Prepare();
            audioSource.errorReceived += VideoPlayer_errorReceived;
            yield return new WaitUntil(() => audioSource.isPrepared);
        }
        yield return new WaitUntil(() => player.isPrepared);
        if (player.isPrepared)
        {
            Debug.Log("invoking Video Prepared");
            VideoPrepared.Invoke();
        }
        if (andplay == true)
        {
            PlayVideo();
        }
    }

    public void SetVideoStartTime(float time)
    {
        if (videoQuality != VideoQuality.UltraHighQuality)
        {
            StartCoroutine(VST(time));
        }
    }

    private IEnumerator VST(float time)
    {
        yield return new WaitForSeconds(Time.deltaTime);
        yield return new WaitUntil(() => player.isPrepared);
        PlayVideo();
        player.time = time;
        player.Pause();
    }

    public void PlayVideo()
    {
        //Debug.Log("playing started");
        if (onlytriggeronce == false)
        {
                Debug.Log("FMVP: Playing video");
                player.Play();
                
                if (requireaudio == true)
                {
                    audioSource.Play();
                    
                }
                onlytriggeronce = true;
                isPlaying = true;
                player.loopPointReached += EndReached;
        }
    }
    private void VideoPlayer_errorReceived(VideoPlayer source, string message)
    {
        OnVideoError("FMVP: VIDEO PLAYBACK FAILURE");
        Debug.Log(message);
        Stop();
        MeshRenderer temp = player.gameObject.GetComponent<MeshRenderer>();
        temp.materials[1].mainTexture = errormat.mainTexture;
    }

    public void Stop()
    {
        player.errorReceived -= VideoPlayer_errorReceived;
        player.Stop();
        isSeeking = false;
        player.playbackSpeed = 1;
        audioSource.playbackSpeed = 1;
        if (requireaudio)
        {
            audioSource.errorReceived -= VideoPlayer_errorReceived;
            audioSource.Stop();
        }
    }

   
    public void EndReached(UnityEngine.Video.VideoPlayer vp)
    {
        Debug.Log("FMVP: End of video reached, video looping = " + player.isLooping);
        if (!player.isLooping)
        {
            isPlaying = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isdisplayed == true)
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                player.Play();
            }
            if (Input.GetKeyDown(KeyCode.K))
            {
                player.Stop();
            }
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            OnButtonPress();
        }
    }


    private string makeString(int id)
    {
        string search = "\"itag\": " + id.ToString();
        return search;
    }


    #region clientURl
    IEnumerator VideoGenerateUrlUsingClient()
    {
        //Debug.Log("VideoGenerateUrlUsingClient");
        CheckVideoUrlAndExtractThevideoId(VideoURL);
        if (!isVideoUrl)
        {
            if (VideoURL.Contains("\"C:"))
            {
                int start = VideoURL.IndexOf("\"");
                int end = VideoURL.LastIndexOf("\"");
                string result = VideoURL.Substring(start + 1, end - 1);
                VideoURL = "file://" + result;
                Debug.Log("FMVP: Copied File Path Detected attempting to play video from file, video files must be .mp4");
            }
            else if (VideoURL.StartsWith("file://"))
            {
                Debug.Log("FMVP: File Path Detected attempting to play video from file, video files must be .mp4");
            }
            else if (VideoURL.Contains("C:"))
            {
                VideoURL = "file://" + VideoURL;
                Debug.Log("FMVP: File Detected attempting to play video from file, video files must be .mp4");
            }
            else
            {
                Debug.Log("FMVP: Not a recognized video URL, reverting to standard input, chance of failure is high");
            }
            videoUrl = VideoURL;
            yield break;
        }
        else
        {
            if (player.source == VideoSource.VideoClip) player.source = VideoSource.Url;
        }
        WWWForm form = new WWWForm();
        string f = "{\"context\": {\"client\": {\"clientName\": \"ANDROID\",\"clientVersion\": \"16.20\",\"hl\": \"en\"}},\"videoId\": \"" + VideoID + "\",}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(f);
        UnityWebRequest request = UnityWebRequest.Post("https://www.youtube.com/youtubei/v1/player?key=AIzaSyAO_FJ2SlqU8Q4STEHLGCilw_Y9_11qcW8", form);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.SetRequestHeader("Content-Type", "application/json");
        //request.SetRequestHeader("Origin","https://www.youtube.com");
        request.SetRequestHeader("X-YouTube-Client-Name", "3");
        request.SetRequestHeader("X-YouTube-Client-Version", "16.20");
        request.certificateHandler = new Certificateinsert();
        yield return request.SendWebRequest();
        if (request.error != null)
        {
            Debug.Log("FMVP: Error: " + request.error);
        }
        else
        { 
            string result = request.downloadHandler.text.Remove(0, 4500);

#if UNITY_EDITOR
            Debug.Log("FMVP: RESULT = " + result);
#endif
            findURLs(result);
            gotURL.Invoke(VideoURL);
        }
    }



    private bool tryGetURL(int id, string result, bool withaudio, bool HDAudio)
    {
        string temp = makeString(id);
        Debug.Log("FMVP: Searching For " + temp);
        if (result.Contains(temp))
        {
            int first = result.IndexOf(temp) + temp.Length;
            int start = result.IndexOf("url", first) + 7;
            int end = result.IndexOf("\"", start + 3);
            string finresult = result.Substring(start, end - start);
            //Debug.Log(" Result Video = " + finresult);
            videoUrl = finresult;
            if (withaudio == false)
            {
                requireaudio = false;
                return true;
            }
        }
        else
        {
            Debug.LogError("FMVP: Video Search not found!");
            return false;
        }
        if (withaudio == true)
        {
            if (HDAudio == true)
            {
                if (result.Contains(makeString(22)))
                {
                    Debug.Log("FMVP: Falling Back on HD Audio type 22");
                    int first = result.IndexOf(makeString(22)) + makeString(22).Length;
                    int start = result.IndexOf("url", first) + 7;
                    int end = result.IndexOf("\"", start + 3);
                    string finresult = result.Substring(start, end - start);
                    //Debug.Log(" Result Audio = " + finresult);
                    audioUrl = finresult;
                    requireaudio = true;
                    return true;
                }
            }
            if (result.Contains(makeString(18)))
            {
                Debug.Log("FMVP: Falling Back on Standard Audio type 18");
                int first = result.IndexOf(audiostring) + audiostring.Length;
                int start = result.IndexOf("url", first) + 7;
                int end = result.IndexOf("\"", start + 3);
                string finresult = result.Substring(start, end - start);
                //Debug.Log(" Result Audio = " + finresult);
                audioUrl = finresult;
                requireaudio = true;
                return true;
            }
            else if (result.Contains(makeString(22)))
            {
                Debug.Log("FMVP: Falling Back on Standard Audio type 22");
                int first = result.IndexOf(makeString(22)) + makeString(22).Length;
                int start = result.IndexOf("url", first) + 7;
                int end = result.IndexOf("\"", start + 3);
                string finresult = result.Substring(start, end - start);
                //Debug.Log(" Result Audio = " + finresult);
                audioUrl = finresult;
                requireaudio = true;
                return true;
            }
            else
            {
                Debug.Log("FMVP: Failed to get backup audio for video setting");
            }
        }

        Debug.LogError("FMVP: Video Search not found!");
        return false;

    }

    private int[] UHDids = new int[18] { 266, 305, 313, 315, 337, 401, 264, 304, 271, 308, 336, 400, 137, 299, 248, 303, 355, 399 };
    private int[] HDids = new int[4] { 37, 22, 59, 18 };
    private int[] LDids = new int[2] {59, 18 };

    private void findURLs(string result)
    {
        switch (videoQuality)
        {
            case VideoQuality.UltraHighQuality:
                for (int i = 0; i <= UHDids.Length - 1; i++)
                {
                    if (tryGetURL(UHDids[i], result, true, true))
                    {
                        break;
                    }
                }
                break;
            case VideoQuality.HighQuality:
                for (int i = 0; i <= HDids.Length - 1; i++)
                {
                    if (tryGetURL(HDids[i], result, false, false))
                    {
                        break;
                    }
                }
                break;
            case VideoQuality.LowQuality:
                for (int i = 0; i <= LDids.Length - 1; i++)
                {
                    if (tryGetURL(LDids[i], result, false, false))
                    {
                        break;
                    }
                }
                break;
            case VideoQuality.AUTOMATIC:
                for (int i = 0; i <= HDids.Length - 1; i++)
                {
                    if (tryGetURL(HDids[i], result, false, false))
                    {
                        break;
                    }
                }
                break;
        }
    }
    protected string CheckVideoUrlAndExtractThevideoId(string url)
    {
        if (url.Contains("?t="))
        {
            int last = url.LastIndexOf("?t=");
            string copy = url;
            string newString = copy.Remove(0, last);
            newString = newString.Replace("?t=", "");
            url = url.Remove(last);
        }

        /*if (!url.Contains("youtu"))
    {
        url = "Video.com/watch?v=" + url;
    }*/

        isVideoUrl = TryNormalizeVideoUrlLocal(url, out url);
        if (!isVideoUrl)
        {
            OnVideoError("FMVP: Not a Video Url");
        }

        return url;
    }

    public void OnVideoError(string errorType)
    {
        Debug.Log("<color=red>" + errorType + "</color>");
    }

    private bool TryNormalizeVideoUrlLocal(string url, out string normalizedUrl)
    {
        url = url.Trim();
        url = url.Replace("youtu.be/", "youtube.com/watch?v=");
        url = url.Replace("www.youtube", "youtube");
        url = url.Replace("youtube.com/embed/", "youtube.com/watch?v=");

        if (url.Contains("/v/"))
        {
            url = "https://youtube.com" + new System.Uri(url).AbsolutePath.Replace("/v/", "/watch?v=");
        }

        url = url.Replace("/watch#", "/watch?");
        IDictionary<string, string> query = ParseQueryString(url);

        string v;


        if (!query.TryGetValue("v", out v))
        {
            normalizedUrl = null;
            return false;
        }

        VideoID = v;
        normalizedUrl = "https://youtube.com/watch?v=" + v;

        return true;
    }

    public IDictionary<string, string> ParseQueryString(string s)
    {
        // remove anything other than query string from url
        if (s.StartsWith("http") && s.Contains("?"))
        {
            s = s.Substring(s.IndexOf('?') + 1);
        }
        //Debug.Log("ADDAAP "+ s);

        var dictionary = new Dictionary<string, string>();

        foreach (string vp in Regex.Split(s, "&"))
        {
            string[] strings = Regex.Split(vp, "=");
            //dictionary.Add(strings[0], strings.Length == 2 ? UrlDecode(strings[1]) : string.Empty); //old
            string key = strings[0];
            string value = string.Empty;

            if (strings.Length == 2)
                value = strings[1];
            else if (strings.Length > 2)
                value = string.Join("=", strings.Skip(1).Take(strings.Length).ToArray());

            dictionary.Add(key, value);
        }

        return dictionary;
    }

    #endregion

}
public enum VideoQuality
{
    AUTOMATIC,
    LowQuality,
    HighQuality,
    UltraHighQuality,
}

public enum VideoFormatType
{
    MP4,
    WEBM
}

public class Certificateinsert : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        return true;
    }
}