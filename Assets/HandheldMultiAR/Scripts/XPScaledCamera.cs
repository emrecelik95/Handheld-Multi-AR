using UnityEngine;
using UnityEngine.XR.iOS;
using GoogleARCore;

public class XPScaledCamera : MonoBehaviour {

#if UNITY_IOS
    private UnityARSessionNativeInterface m_session;
    private bool sessionStarted;
#endif

    [HideInInspector]
    public Camera m_camera;

    void Start () {
        m_camera = GetComponent<Camera>();
#if UNITY_IOS
        m_session = UnityARSessionNativeInterface.GetARSessionNativeInterface();
#endif        
        //m_camera.enabled = false;
        //m_camera.enabled = true;

#if UNITY_IOS
        UnityARSessionNativeInterface.ARFrameUpdatedEvent += FirstFrameUpdate;
#endif
    }

#if UNITY_IOS
    void FirstFrameUpdate(UnityARCamera cam)
    {
        sessionStarted = true;
        UnityARSessionNativeInterface.ARFrameUpdatedEvent -= FirstFrameUpdate;
    }
#endif

    void Update () {

#if UNITY_IOS
        if (sessionStarted)
        {
            Matrix4x4 matrix = m_session.GetCameraPose();
            m_camera.transform.localPosition = UnityARMatrixOps.GetPosition(matrix);
            m_camera.transform.localRotation = UnityARMatrixOps.GetRotation(matrix);

            m_camera.projectionMatrix = m_session.GetCameraProjection();
        }
#endif

#if UNITY_ANDROID
        transform.localPosition = Frame.Pose.position; 
        transform.localRotation = Frame.Pose.rotation; 
        m_camera.projectionMatrix = Frame.CameraImage.GetCameraProjectionMatrix(m_camera.nearClipPlane, m_camera.farClipPlane);
#endif

    }
}
