using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleCameraFPS : MonoBehaviour
{

    [SerializeField]
    private float targetFPS = 30;

    private float timer = 0;

    private Camera cam;

    private Camera handledCam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        GameObject go = new GameObject("HandledCamera");
        go.transform.SetParent(gameObject.transform);
        handledCam = go.AddComponent<Camera>();
        handledCam.CopyFrom(cam);
        cam.enabled = false;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer < 1 / targetFPS)
        {
            return;
        }

        timer = 0.0f;
        handledCam.CopyFrom(cam);
    }
}
