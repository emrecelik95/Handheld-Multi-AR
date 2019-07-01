using GoogleARCore;
using GoogleARCore.CrossPlatform;
using UnityEngine;
using System;


#if UNITY_EDITOR
// Set up touch input propagation while using Instant Preview in the editor.
using Input = GoogleARCore.InstantPreviewInput;
#endif

/// <summary>
/// _HostPlacedAnchor and _ResolveAnchorFrom must be used for multi user.
/// Syncing anchorID(Google Cloud Anchor API) is needed.
/// ***** (anchorID, arScale, arRotY) must be sent and receive using Public Events (HostEvent, ResolveEvent). *****
/// </summary>
public class XPAnchorNetworkController : XPAnchorController
{
    #region PUBLIC_EVENTS

    // triggered when anchor is hosted
    public Action<string, float, float> HostEvent;
    // triggered when anchor is resolved
    public Action ResolveEvent;

    #endregion

    #region PUBLIC_STUFF

    /// <summary>
    /// Handles user intent to enter a mode where they can place an anchor to host or to exit this mode if
    /// already in it.
    /// </summary>
    public void EnterHostingMode()
    {
        if (m_CurrentMode == ApplicationMode.Hosting)
        {
            _ResetStatus();
            return;
        }

        Debug.Log("Hosting Anchor...");
        m_CurrentMode = ApplicationMode.Hosting;

        CreateAnchorAtLastHitPose();
        _HostPlacedAnchor(m_LastPlacedAnchor);
    }

    /// <summary>
    /// Handles a user intent to enter a mode where they can input an anchor to be resolved or exit this mode if
    /// already in it.
    /// </summary>
    public void EnterResolvingMode(string anchorID, float arScale, float arRotY)
    {
        _ResetStatus();

        m_CurrentMode = ApplicationMode.Resolving;
        Debug.Log("Resolving Anchor...");

        _ResolveAnchorFrom(anchorID, arScale, arRotY);
    }

    /// <summary>
    /// Hosts the user placed cloud anchor and sends it to the server.
    /// </summary>
    protected void _HostPlacedAnchor(Component placedAnchor)
    {
#if !UNITY_IOS || ARCORE_IOS_SUPPORT

#if !UNITY_IOS
        var anchor = (Anchor)placedAnchor;
#else
        var anchor = (UnityEngine.XR.iOS.UnityARUserAnchorComponent)placedAnchor;
#endif
        Debug.Log("Sending create cloud anchor request...");
        XPSession.CreateCloudAnchor(anchor).ThenAction(result =>
        {
            if (result.Response != CloudServiceResponse.Success)
            {
                Debug.Log(string.Format("Failed to host cloud anchor: {0}", result.Response));
                return;
            }

            Debug.Log("Cloud anchor was created and saved.");

            string anchorId = result.Anchor.CloudId; // id to be sent the others
            float arScale = XPARController.instance.scale;
            float arRotY = XPARController.instance.arRotY;

            // RPC ::: send ar anchor data to the others
            HostEvent?.Invoke(anchorId, arScale, arRotY);

            EnterPlayingMode(); // we're done with handling ar content
        });
#endif
    }

    /// <summary>
    /// Resolves an anchor id and instantiates the content prefab on it.
    /// </summary>
    /// <param name="cloudAnchorId">Cloud anchor id to be resolved.</param>
    protected void _ResolveAnchorFrom(string anchorID, float arScale, float arRotY)
    {
        XPSession.ResolveCloudAnchor(anchorID).ThenAction((System.Action<CloudAnchorResult>)(result =>
        {
            if (result.Response != CloudServiceResponse.Success)
            {
                Debug.Log(string.Format("Resolving Error: {0}.", result.Response));
                return;
            }

            m_LastResolvedAnchor = result.Anchor;

            AlignARRootWithHitPose(new Pose(m_LastResolvedAnchor.transform.position, m_LastResolvedAnchor.transform.rotation));
            XPARController.instance.RotateAroundLevel(arRotY);
            XPARController.instance.scale = arScale;

            Debug.Log(string.Format("Resolved Successfully! ", result.Response));

            ResolveEvent?.Invoke();
            EnterPlayingMode(); // we're done with handling ar content
        }));
    }

    #endregion
}

