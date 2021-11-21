using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Networking;

public interface WebRequestProvider
{
    void HTTPGet(string url, Action<WebRequestProviderResult> callback);
}
public class WebRequestProviderResult
{
    public string text;
    public string error;
}


public class UnityWebRequestProvider : WebRequestProvider
{
	public void HTTPGet(string url, Action<WebRequestProviderResult> callback)
	{
        MonoWebRequestHelper.Instance.StartCoroutine(PerformRequest(url, callback));
    }

    IEnumerator PerformRequest(string url, Action<WebRequestProviderResult> callback)
    {
        UnityWebRequest www = new UnityWebRequest(url);
        www.downloadHandler = new DownloadHandlerBuffer();
        yield return www.SendWebRequest();

        var result = new WebRequestProviderResult()
        {
            error = www.error,
            text = www.downloadHandler.text
        };

        if (callback != null)
        {
            callback(result);
        }
    }
}

public class MonoWebRequestHelper : MonoBehaviour
{
    private static MonoWebRequestHelper _Instance;
    public static MonoWebRequestHelper Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = new GameObject("MonoWebRequestHelper").AddComponent<MonoWebRequestHelper>();
                DontDestroyOnLoad(_Instance);
            }
            return _Instance;
        }
    }
}