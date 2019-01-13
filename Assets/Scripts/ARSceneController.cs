using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;

public class ARSceneController : MonoBehaviour
{

    public Camera firstPersonCamera;
    public GameObject prefab;
    private const float k_ModelRotation = 180.0f;

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

        // Add to the end of Update()
        ProcessTouches();
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


    void ProcessTouches () {

        // If the player has not touched the screen, we are done with this update.
        Touch touch;
        if (Input.touchCount != 1 || (touch = Input.GetTouch (0)).phase != TouchPhase.Began) {
            return;
        } 

        // Raycast against the location the player touched to search for planes.
        TrackableHit hit;
        TrackableHitFlags raycastFilter = TrackableHitFlags.FeaturePointWithSurfaceNormal | TrackableHitFlags.PlaneWithinPolygon;

        if (Frame.Raycast (touch.position.x, touch.position.y, raycastFilter, out hit)) {
            // Use hit pose and camera pose to check if the out hit is a detected plane.
            // If it is, no need to create the anchor.
            if ((hit.Trackable is DetectedPlane)) {
                DetectedPlane selectedPlane = hit.Trackable as DetectedPlane;
                Debug.Log ("Selected plane centered at " + selectedPlane.CenterPose.position);
            
                // Instantiate Andy model at the hit pose.
                var andyObject = Instantiate(prefab, hit.Pose.position, hit.Pose.rotation);

                // Compensate for the hitPose rotation facing away from the raycast (i.e. camera).
                andyObject.transform.Rotate(0, k_ModelRotation, 0, Space.Self);

                // Create an anchor to allow ARCore to track the hitpoint as understanding of the physical
                // world evolves.
                var anchor = hit.Trackable.CreateAnchor(hit.Pose);

                // Make Andy model a child of the anchor.
                andyObject.transform.parent = anchor.transform;
            }
        }
    }
}
