// WebView-Unity mediator plugin script.
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System;

public class WebMediator : MonoBehaviour{
    private static bool isClearCache;

    private string lastRequestedUrl;
    private bool loadRequest;
    private bool visibility;
    private int leftMargin;
    private int topMargin;
    private int rightMargin;
    private int bottomMargin;

    private Action hideCallback;

    void Start()
    {
        InstallPlatform();
    }

    // Set margins around the web view.
    public void SetMargin(int left, int top, int right, int bottom) {
        leftMargin = left;
        topMargin = top;
        rightMargin = right;
        bottomMargin = bottom;
        ApplyMarginsPlatform();
    }

    // Visibility functions.
    public void Show(string url) {
        LoadUrl(url);
        if (transform is RectTransform)
        {
            var rect = transform as RectTransform;
            SetMargin((int)rect.offsetMin.x, (int)-rect.offsetMax.y, (int)-rect.offsetMax.x, (int)rect.offsetMin.y);
        }

        visibility = true;
    }
    public void Hide(Action hideCallback = null) {
        this.hideCallback = hideCallback;
        visibility = false;
        LoadUrl("about:blank");
    }
    public bool IsVisible() {
        return visibility;
    }

    static void SetClearCache()
    {
        isClearCache = true;
    }

    static void SetCache()
    {
        isClearCache = false;
    }

    // Load the page at the URL.
    public void LoadUrl(string url) {
        lastRequestedUrl = url;
        loadRequest = true;
    }



    void Update() {
        UpdatePlatform();
        loadRequest = false;

        if(visibility == false && hideCallback != null)
        {
            hideCallback();
        }
    }

    #if UNITY_EDITOR

    // Unity Editor implementation.

    private void InstallPlatform() { }
    private void UpdatePlatform() { }
    private void ApplyMarginsPlatform() { }
    public WebMediatorMessage PollMessage(){ return null; }
    public void MakeTransparentWebViewBackground() { }
    public void CallJavascript(string method, string args) { }

    #elif UNITY_IPHONE

    // iOS platform implementation.

    [DllImport("__Internal")]
    static extern void _WebViewPluginInstall();
    [DllImport("__Internal")]
    static extern void _WebViewPluginLoadUrl(string url, bool isClearCache);
    [DllImport("__Internal")]
    static extern void _WebViewPluginSetVisibility(bool visibility);
    [DllImport("__Internal")]
    static extern void _WebViewPluginSetMargins(int left, int top, int right, int bottom);
    [DllImport("__Internal")]
    static extern string _WebViewPluginPollMessage();
    [DllImport("__Internal")]
    static extern void _WebViewPluginMakeTransparentBackground();
    [DllImport("__Internal")]
    static extern void _WebViewPluginCallJavascript(string script);

    private static bool viewVisibility;

    private void InstallPlatform() {
        _WebViewPluginInstall();
    }

    private void ApplyMarginsPlatform() {
        _WebViewPluginSetMargins(leftMargin, topMargin, rightMargin, bottomMargin);
    }

    private void UpdatePlatform() {
        if (viewVisibility != visibility) {
            viewVisibility = visibility;
            _WebViewPluginSetVisibility(viewVisibility);
        }
        if (loadRequest) {
            loadRequest = false;
            _WebViewPluginLoadUrl(lastRequestedUrl, isClearCache);
        }
    }

    public WebMediatorMessage PollMessage(){
        var message =  _WebViewPluginPollMessage();
        return !string.IsNullOrEmpty(message) ? new WebMediatorMessage(message) : null;
    }

    public void MakeTransparentWebViewBackground()
    {
        _WebViewPluginMakeTransparentBackground();
    }

    public void CallJavascript(string method, string args)
    {
        _WebViewPluginCallJavascript(method + "('" + args + "')");
    }

    #elif UNITY_ANDROID

    // Android platform implementation.

    private static AndroidJavaClass unityPlayerClass;

    private void InstallPlatform() {
        unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    }

    private void ApplyMarginsPlatform() { }

    private void UpdatePlatform() {
        var activity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
        activity.Call("updateWebView", !string.IsNullOrEmpty(lastRequestedUrl) ? lastRequestedUrl : "", loadRequest, visibility, leftMargin, topMargin, rightMargin, bottomMargin);
    }

    public WebMediatorMessage PollMessage() {
        var activity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
        var message = activity.Call<string>("pollWebViewMessage");
        return !string.IsNullOrEmpty(message) ? new WebMediatorMessage(message) : null;
    }

    public void MakeTransparentWebViewBackground()
    {
        var activity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
        activity.Call("makeTransparentWebViewBackground");
    }

    public void CallJavascript(string method, string args)
    {
        LoadUrl("javascript:" + method + "('" + args + "')");
    }
    #endif
}
public class WebMediatorMessage {
    public string path;
    public Hashtable args;

    public WebMediatorMessage(string rawMessage) {
        // Retrieve a path.
        var split = rawMessage.Split('?');
        path = split[0];
        // Parse arguments.
        args = new Hashtable();
        if (split.Length > 1) {
            foreach (var pair in split[1].Split('&')) {
                var elems = pair.Split('=');
                args[elems[0]] = WWW.UnEscapeURL(elems[1]);
            }
        }
    }

}

