using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;

public class ARSceneController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start() {
        QuitOnConnectionErrors();
    }

    // Update is called once per frame
    void Update() {
        // The session status must be Tracking in order to access the Frame.
        if (Session.Status != SessionStatus.Tracking) {
            int lostTrackingSleepTimeout = 15;
            Screen.sleepTimeout = lostTrackingSleepTimeout;
            return;
        }
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    /// <summary>
	/// Quit the application if there was a connection error for the ARCore session.
	/// </summary>
	private void QuitOnConnectionErrors() {
        if (Session.Status == SessionStatus.ErrorPermissionNotGranted) {
            StartCoroutine(ToastAndExit("Camera permission is needed to run this application.", 5));
        }
        else if (Session.Status.IsError()) {
            StartCoroutine(ToastAndExit("ARCore encountered a problem connecting.  Please start the app again.", 5));
        }
    }


    /// <summary>Coroutine to display an error then exit.</summary>
    public static IEnumerator ToastAndExit(string message, int seconds) {
        _ShowAndroidToastMessage(message);
        yield return new WaitForSeconds(seconds);
        Application.Quit();
    }


    /// <summary>
    /// Show an Android toast message.
    /// </summary>
    /// <param name="message">Message string to show in the toast.</param>
    /// <param name="length">Toast message time length.</param>
    private static void _ShowAndroidToastMessage(string message) {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null) {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity, message, 0);
                toastObject.Call("show");
            }));
        }
    }
}
