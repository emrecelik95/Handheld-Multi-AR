using GoogleARCore;
using GoogleARCore.CrossPlatform;
using GoogleARCore.Examples.CloudAnchors;
using UnityEngine;


#if UNITY_EDITOR
// Set up touch input propagation while using Instant Preview in the editor.
using Input = GoogleARCore.InstantPreviewInput;
#endif

public class XPAnchorController : MonoBehaviour
{

    public enum ApplicationMode
    {
        Ready,
        Nothing,
        Placing,
        Hosting,
        Resolving,
        Playing
    }

    public bool ignoreRay { get; set; }

    #region INSPECTOR STUFF

    public GameObject m_Level;

    #endregion

    #region PRIVATE_VARS

    /// <summary>
    /// A helper object to ARKit functionality.
    /// </summary>
    private ARKitHelper m_ARKit = new ARKitHelper();

    /// <summary>
    /// True if the app is in the process of quitting due to an ARCore connection error, otherwise false.
    /// </summary>
    private bool m_IsQuitting = false;

    private Pose lastRaycastPose;

    private Trackable lastARCoreTrackable = null;

    #endregion

    #region PROTECTED_VARS

    /// <summary>
    /// The last placed anchor.
    /// </summary>
    protected Component m_LastPlacedAnchor = null;

    /// <summary>
    /// The last resolved anchor.
    /// </summary>
    protected XPAnchor m_LastResolvedAnchor = null;

    protected ApplicationMode m_CurrentMode = ApplicationMode.Ready;

    #endregion

    #region UNITY_EVENTS

    protected void Awake()
    {
        _ResetStatus();
    }

    protected void Update()
    {
        _UpdateApplicationLifecycle();

        if (ignoreRay)
            return;

        if (m_LastPlacedAnchor != null || m_LastResolvedAnchor != null)
            return;

        if (m_CurrentMode == ApplicationMode.Placing)
        {

            // If the player has not touched the screen then the update is complete.
            Touch touch;
            if (Input.touchCount < 1)
                return;

            touch = Input.GetTouch(0);
            if (touch.phase != TouchPhase.Began && touch.phase != TouchPhase.Moved)
            {
                return;
            }

            // Raycast against the location the player touched to search for planes.
            if (Application.platform != RuntimePlatform.IPhonePlayer)
            {
                TrackableHit hit;
                if (Frame.Raycast(touch.position.x, touch.position.y,
                        TrackableHitFlags.PlaneWithinPolygon | TrackableHitFlags.PlaneWithinBounds, out hit))
                {
                    AlignARRootWithHitPose(hit.Pose);
                    lastRaycastPose = hit.Pose;
                    lastARCoreTrackable = hit.Trackable;
                }
            }
            else
            {
                Pose hitPose;
                if (m_ARKit.RaycastPlane(XPARController.instance.ARKitFirstPersonCamera, touch.position.x, touch.position.y, out hitPose))
                {
                    AlignARRootWithHitPose(hitPose);
                    lastRaycastPose = hitPose;
                }
            }
        }

    }

    #endregion

    #region PUBLIC_STUFF
   
    public void EnterPlacingMode()
    {
        if(m_CurrentMode == ApplicationMode.Placing)
        {
            _ResetStatus();
            return;
        }

        Debug.Log("Placing Anchor...");
        m_CurrentMode = ApplicationMode.Placing;
    }

    public void EnterPlayingMode()
    {
        Debug.Log("AR Anchor Ready (AR Playing Mode).");
        m_CurrentMode = ApplicationMode.Playing;
    }

    #endregion

    #region PRIVATE_METHODS

    /// <summary>
    /// Check and update the application lifecycle.
    /// </summary>
    private void _UpdateApplicationLifecycle()
    {

        var sleepTimeout = SleepTimeout.NeverSleep;

#if !UNITY_IOS
        // Only allow the screen to sleep when not tracking.
        if (Session.Status != SessionStatus.Tracking)
        {
            const int lostTrackingSleepTimeout = 15;
            sleepTimeout = lostTrackingSleepTimeout;
        }
#endif

        Screen.sleepTimeout = sleepTimeout;

        if (m_IsQuitting)
        {
            return;
        }

        // Quit if ARCore was unable to connect and give Unity some time for the toast to appear.
        if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
        {
            _ShowAndroidToastMessage("Camera permission is needed to run this application.");
            m_IsQuitting = true;
            Invoke("_DoQuit", 0.5f);
        }
        else if (Session.Status.IsError())
        {
            _ShowAndroidToastMessage("ARCore encountered a problem connecting.  Please start the app again.");
            m_IsQuitting = true;
            Invoke("_DoQuit", 0.5f);
        }
    }

    /// <summary>
    /// Show an Android toast message.
    /// </summary>
    /// <param name="message">Message string to show in the toast.</param>
    private void _ShowAndroidToastMessage(string message)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity,
                    message, 0);
                toastObject.Call("show");
            }));
        }
    }

    #endregion

    #region PROTECTED_METHODS

    protected void AlignARRootWithHitPose(Pose hitPose)
    {
        Transform root = XPARController.instance.ARRoot;
        Vector3 hitPos = root.transform.rotation * (hitPose.position * root.localScale.x) + root.transform.position;
        XPARController.instance.pointOfInterest = m_Level.transform.position;
        XPARController.instance.AlignWithPointOfInterest(hitPos);

        XPARController.instance.RotateAroundAnchor(hitPose.rotation.eulerAngles.y); /////////////////////////////////////////////////////////////////
    }

    protected void CreateAnchorAtLastHitPose()
    {
        if (Application.platform != RuntimePlatform.IPhonePlayer)
            m_LastPlacedAnchor = lastARCoreTrackable.CreateAnchor(lastRaycastPose);
        else
            m_LastPlacedAnchor = m_ARKit.CreateAnchor(lastRaycastPose);
    }

    /// <summary>
    /// Resets the internal status and UI.
    /// </summary>
    protected void _ResetStatus()
    {
        m_CurrentMode = ApplicationMode.Ready;
        if (m_LastPlacedAnchor != null)
        {
            Destroy(m_LastPlacedAnchor.gameObject);
        }

        m_LastPlacedAnchor = null;
        if (m_LastResolvedAnchor != null)
        {
            Destroy(m_LastResolvedAnchor.gameObject);
        }

        m_LastResolvedAnchor = null;
    }

    #endregion

}

