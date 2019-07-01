using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IOSLightWrapper : MonoBehaviour
{
    [SerializeField]
    private Light dirLight;

    private void Awake()
    {
        if (!Application.platform.Equals(RuntimePlatform.IPhonePlayer))
            return;

        Light l = GetComponent<Light>();

        l.shadows = dirLight.shadows;
        //l.shadowAngle = dirLight.shadowAngle;
        l.shadowStrength = dirLight.shadowStrength;
        l.shadowResolution = dirLight.shadowResolution;
        l.shadowBias = dirLight.shadowBias;
        l.shadowNormalBias = dirLight.shadowNormalBias;
        l.shadowNearPlane = dirLight.shadowNearPlane;

        Destroy(dirLight.gameObject);
    }
}
