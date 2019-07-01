using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyInAwake : MonoBehaviour {

    public List<RuntimePlatform> destroyPlatforms;

    public bool dontDestroyForTest = false;

    private void Awake()
    {
        if (dontDestroyForTest)
            return;

        if(destroyPlatforms.Contains(Application.platform))
            Destroy(gameObject);
    }
}
