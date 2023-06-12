using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Networking;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine.Events;


public class SimplifiedVideoPlayer : MonoBehaviour
{
    public GameObject Screen;
    public string VideoURL = "Enter Video URL Here";
    public VideoPlayer player;

    public bool playOnStart = false;
    public bool generateUsingClient;

    private string videoUrl;

    private bool ratelimit = false;
    //public InputMenu menu;
    private bool isdisplayed = false;
    [HideInInspector]
    public bool onlytriggeronce = false;
    private string VideoID;
    private bool isVideoUrl = false;
    [HideInInspector]
    public UnityEvent<string> gotURL;
    [HideInInspector]
    public bool isPlaying;
    //private bool Isloaded = false;
    public Material errormat;
    [HideInInspector]
    public bool isSeeking = false;

    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(Startvideo());
        if (playOnStart == true)
        {
            StartCoroutine(Startvideo(true));
        }
#if UNITY_EDITOR
        //menu.Show(VideoURL, this);
#endif
    }

    private void OnValidate()
    {
        //this checks to ensue that a UI menu class is present
        //if (menu == null)
        //{
        //    Debug.LogError("VideoPlayer: VideoPlayer Requires an Input Menu to Work!");
        //}
        //else Debug.ClearDeveloperConsole();

        if (VideoURL == "")
        {
            VideoURL = "VideoPlayer: Enter Video URL Here";
        }
    }

    //this function manages when to show the UI menu (I called this method using the Usable Behaviour Component)
    public void OnButtonPress()
    {

        if (ratelimit == false && isdisplayed == false)
        {
            ratelimit = true;
            //menu.Show(VideoURL, this);
            isdisplayed = true;
            StartCoroutine(wait());
        }
        if (ratelimit == false && isdisplayed == true)
        {
            ratelimit = true;
            //menu.CancelClick();
            isdisplayed = false;
            StartCoroutine(wait());
        }

    }

    //prevents spamming the UI
    IEnumerator wait()
    {
        yield return new WaitForSecondsRealtime(0.25f);
        ratelimit = false;
    }

    //prepares the video in the video player but doesnt play it, useful if you want to start the video when a fuse is lit or at a later time
    public void OK(string input)
    {
        VideoURL = input;
        isdisplayed = false;
        StartCoroutine(Startvideo(false));
    }
   
    //function called when the UI is no longer displayed
    public void Cancel()
    {
        isdisplayed = false;
    }

    //starts the video instantly from the given url
    public void forcestartvideo(string url)
    {
        VideoURL = url;
        Debug.Log("VideoPlayer: force playing " + url);
        StartCoroutine(Startvideo(true));
    }

    //starts the process of extracting and loading the video, includes the option to play the video after it has been loaded or not
    IEnumerator Startvideo(bool andplay)
    {
        player.url = "";
        if (VideoURL == "Enter Video URL Here")
        {
            Debug.LogWarning("VideoPlayer: Attempted to start video without a URL input!!!");
            yield break;
        }
        if (generateUsingClient == true)
        {
            Debug.Log("VideoPlayer: Generating link with client");
            yield return StartCoroutine(VideoGenerateUrlUsingClient());
        }
        Debug.Log("VideoPlayer: Preparing Video");
        //Debug.Log("Playing URL: " + videoUrl);
        player.url = videoUrl;
        player.Prepare();
        player.errorReceived += VideoPlayer_errorReceived;
        
        if (andplay == true)
        {
            yield return new WaitUntil(() => player.isPrepared);
            PlayVideo();
        }
    }

    //starts playing the actual video player after the url has been loaded in
    public void PlayVideo()
    {
        //Debug.Log("playing started");
        if (onlytriggeronce == false)
        {
            Debug.Log("VideoPlayer: Playing video");
            player.Play();


            onlytriggeronce = true;
            isPlaying = true;
            player.loopPointReached += EndReached;
        }
    }

    //manages errors with the video player
    private void VideoPlayer_errorReceived(VideoPlayer source, string message)
    {
        OnVideoError("VideoPlayer: VIDEO PLAYBACK FAILURE");
        Debug.Log(message);
        Stop();
        MeshRenderer temp = player.gameObject.GetComponent<MeshRenderer>();
        temp.materials[1].mainTexture = errormat.mainTexture;
    }

    //stops the video
    public void Stop()
    {
        player.errorReceived -= VideoPlayer_errorReceived;
        player.Stop();
        isSeeking = false;
        player.playbackSpeed = 1;
       
    }

    //function called when your video has finished playing
    public void EndReached(UnityEngine.Video.VideoPlayer vp)
    {
        Debug.Log("VideoPlayer: End of video reached, video looping = " + player.isLooping);
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
    }


    //helper function to locate specific video qualities, Itags are the numbers used to identify a video quality and format
    private string makeString(int id)
    {
        string search = "\"itag\": " + id.ToString();
        return search;
    }


    //the main function that takes the VideoURL and returns a usable .mp4 link
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
                Debug.Log("VideoPlayer: Copied File Path Detected attempting to play video from file, video files must be .mp4");
            }
            else if (VideoURL.StartsWith("file://"))
            {
                Debug.Log("VideoPlayer: File Path Detected attempting to play video from file, video files must be .mp4");
            }
            else if (VideoURL.Contains("C:"))
            {
                VideoURL = "file://" + VideoURL;
                Debug.Log("VideoPlayer: File Detected attempting to play video from file, video files must be .mp4");
            }
            else
            {
                Debug.Log("VideoPlayer: Not a recognized video URL, reverting to standard input, chance of failure is high");
            }
            videoUrl = VideoURL;
            yield break;
        }
        else
        {
            if (player.source == VideoSource.VideoClip) player.source = VideoSource.Url;
        }
        WWWForm form = new WWWForm();
        string f = "{\"context\": {\"client\": {\"clientName\": \"ANDROID\",\"clientVersion\": \"17.31.35\",\"androidSdkVersion\": \"30\",\"hl\": \"en\"}},\"videoId\": \"" + VideoID + "\",}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(f);
        UnityWebRequest request = UnityWebRequest.Post("https://www.youtube.com/youtubei/v1/player?key=AIzaSyA8eiZmM1FaDVjRy-df2KTyQ_vz_yYM39w", form);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("X-YouTube-Client-Name", "1");
        request.SetRequestHeader("X-YouTube-Client-Version", "2.20220801.00.00");
        string userAgentTemporary = "com.google.android.youtube/17.31.35 (Linux; U; Android 11) gzip";
        //request.SetRequestHeader("Accept", "*/*");
        //request.SetRequestHeader("Accept-Encoding", "gzip, deflate");
        request.SetRequestHeader("User-Agent", userAgentTemporary);
        request.certificateHandler = new Certificateinsert();
        yield return request.SendWebRequest();
        if (request.error != null)
        {
            Debug.Log("VideoPlayer: Error: " + request.error);
        }
        else
        {
            string result = request.downloadHandler.text.Remove(0, 4500);

#if UNITY_EDITOR
            Debug.Log("VideoPlayer: RESULT = " + result);
#endif
            findURLs(result);
            gotURL.Invoke(VideoURL);
        }
    }


    //this function finds the .mp4 link for a specific video format
    private bool tryGetURL(int id, string result, bool withaudio, bool HDAudio)
    {
        string temp = makeString(id);
        Debug.Log("VideoPlayer: Searching For " + temp);
        if (result.Contains(temp))
        {
            int first = result.IndexOf(temp) + temp.Length;
            int start = result.IndexOf("url", first) + 7;
            int end = result.IndexOf("\"", start + 3);
            string finresult = result.Substring(start, end - start);
            //Debug.Log(" Result Video = " + finresult);
            videoUrl = finresult;
            return true;
        }
        else
        {
            Debug.LogError("VideoPlayer: Video Search not found!");
            return false;
        }
    }

    //this function attempts to find the best possible video quality that also contains audio codec starting at 720p then 480, then 320
    private void findURLs(string result)
    {

        if (tryGetURL(22, result, false, false))
        {
            return;
        }
        if (tryGetURL(18, result, false, false))
        {
            return;
        }
        if (tryGetURL(43, result, false, false))
        {
            return;
        }
    }

    //this function gets the video ID from the video link
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
            OnVideoError("VideoPlayer: Not a Video Url");
        }

        return url;
    }


    //custom error method
    public void OnVideoError(string errorType)
    {
        Debug.Log("<color=red>" + errorType + "</color>");
    }

    //this function helpts to extract the video ID from the video link
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


    //helps to format the url in order to extract the ID
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




