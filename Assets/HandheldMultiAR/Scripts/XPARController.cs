using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class XPARController : MonoBehaviour {

    public static XPARController instance;

    #region INSPECTOR_STUFF

    [Header("ARCore")]

    /// <summary>
    /// The ARRoot for ARCore-specific GameObjects in the scene.
    /// </summary>
    public GameObject ARCoreRoot;

    public Camera ARCoreFirstPersonCamera;
    public GameObject ARCorePlanes;
    public GameObject ARCorePointCloud;

    [Header("ARKit")]

    /// <summary>
    /// The ARRoot for ARKit-specific GameObjects in the scene.
    /// </summary>
    public GameObject ARKitRoot;

    /// <summary>
    /// The first-person camera used to render the AR background texture for ARKit.
    /// </summary>
    public Camera ARKitFirstPersonCamera;
    public GameObject ARKitPlanes;
    public GameObject ARKitPointCloud;

    [Space(5)]
    [Header("Scaled AR Stuff")]
    public Transform ARRoot;

    public Camera ScaledARCamera;

    [Space(5)]
    [Tooltip("The less value is the more scale")]
    public float firstScale = 4.0f;

    #endregion

    #region PRIVATE_VARS

    private float m_Scale = 2;
    private Quaternion m_Rotation = Quaternion.identity;
    private Quaternion m_InvRotation = Quaternion.identity;
    /// <summary>
    /// Actually the Y degree of anchor rotation.
    /// </summary>
    private float levelRotY = 0;

    #endregion

    #region PUBLIC_VARS

    /// <summary>
    /// The Y degree of rotation in which user set.
    /// </summary>
    [HideInInspector]
    public float arRotY = 0;

    [HideInInspector]
    public Vector3 pointOfInterest;

    [HideInInspector]
    public float scale
    {
        set
        {
            m_Scale = value;

            if (ARRoot)
            {
                var poiInRootSpace = ARRoot.InverseTransformPoint(pointOfInterest);
                ARRoot.localPosition = m_InvRotation * (-poiInRootSpace * m_Scale) + pointOfInterest;
            }
        }

        get { return m_Scale; }
    }

    [HideInInspector]
    public Quaternion rotation
    {
        get { return m_Rotation; }
        set
        {
            if (ARRoot)
            {
                m_Rotation = value;
                m_InvRotation = Quaternion.Inverse(rotation);
                var poiInRootSpace = ARRoot.InverseTransformPoint(pointOfInterest);

                ARRoot.localPosition = m_InvRotation * (-poiInRootSpace * scale) + pointOfInterest;
                ARRoot.localRotation = m_InvRotation;
            }
        }
    }

    #endregion

    #region PUBLIC_METHODS

    public void ToggleARObjects(bool open)
    {
#if UNITY_IOS
        ARKitPlanes.SetActive(open);
        //ARKitPointCloud.SetActive(open);
#elif UNITY_ANDROID
        ARCorePlanes.SetActive(open);
        //ARCorePointCloud.SetActive(open);
#endif
    }

    public void ToggleARObjects()
    {

#if UNITY_IOS
        ToggleARObjects(!ARKitPlanes.activeInHierarchy);
#elif UNITY_ANDROID
        ToggleARObjects(!ARCorePlanes.activeInHierarchy);
#endif

    }

    public void RotateAroundAnchor(float degree)
    {
        levelRotY = degree;
        RotateAroundAxisY(levelRotY + arRotY);
    }

    public void RotateAroundLevel(float degree)
    {
        arRotY = degree;
        RotateAroundAxisY(arRotY + levelRotY);
    }

    public void RotateAroundAxisY(float degree)
    {
        rotation = Quaternion.AngleAxis(degree, Vector3.up);
    }

    public void AlignWithPointOfInterest(Vector3 position)
    {
        if (ARRoot)
        {
            var poiInRootSpace = ARRoot.InverseTransformPoint(position - pointOfInterest);
            ARRoot.localPosition = m_InvRotation * (-poiInRootSpace * scale);
        }
    }

#endregion

    #region UNITY_EVENTS

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        SetMobileARRoot();
    }

    private void Start()
    {
        scale = firstScale;
    }

    private void OnEnable()
    {
        Application.onBeforeRender += OnBeforeRender;
    }

    private void OnDisable()
    {
        Application.onBeforeRender -= OnBeforeRender;
    }

    private void OnBeforeRender()
    {
        if (ARRoot)
        {
            ARRoot.localScale = Vector3.one * scale;
        }
    }

#endregion

    #region PROTECTED_METHODS

    protected void SetMobileARRoot()
    {
        if (Application.platform != RuntimePlatform.IPhonePlayer)
        {
            ARCoreRoot.SetActive(true);
            ARKitRoot.SetActive(false);
        }
        else
        {
            ARCoreRoot.SetActive(false);
            ARKitRoot.SetActive(true);
        }
    }

#endregion
}
