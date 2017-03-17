using UnityEngine;
using UnityEngine.UI;

public class TestInterface : MonoBehaviour{

    public WebMediator WebMediator;

    public Text A;
    public Text B;
    public InputField Input;

    // Show the web view (with margins) and load the index page.
    public void ActivateWebView(string url) {
        WebMediator.Show(url);
    }

    // Hide the web view.
    public void DeactivateWebView() {
        WebMediator.Hide();
    }

    // Process messages coming from the web view.
    private void ProcessMessages() {
        while (true) {
            // Poll a message or break.
            var message = WebMediator.PollMessage();
            if (message == null) break;
            Debug.Log(message.path);
            switch (message.path)
            {
                case "A":
                    A.text = message.args ["msg"].ToString();
                    break;
                case "B":
                    B.text = message.args ["msg"].ToString();
                    break;
                default:
                    break;
            }
        }
    }

    public void SendMsg()
    {
        WebMediator.CallJavascript("callForUnity", Input.text);
    }

    void Update() {
        if (WebMediator.IsVisible()) {
            ProcessMessages();
        }
    }
}